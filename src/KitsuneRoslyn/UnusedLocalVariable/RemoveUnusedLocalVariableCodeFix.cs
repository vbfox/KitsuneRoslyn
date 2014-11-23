using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.KitsuneRoslyn.UnusedLocalVariable
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class RemoveUnusedLocalVariableCodeFix : CodeFixProvider
    {
        public const string Id = "KR.RemoveUnusedLocalVariable";
        public const string IdNeverAssigned = Id + ".NeverAssigned";
        public const string IdOnlyAssigned = Id + ".OnlyAssigned";

        private const string DeclaredButNeverUsedDiagnosticId = "CS0168";
        private const string OnlyAssignedToDiagnosticId = "CS0219";

        public override Task ComputeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == DeclaredButNeverUsedDiagnosticId)
                {
                    context.RegisterFix(CodeAction.Create("Remove unused local variable", 
                    ct => RemoveAsync(context, diagnostic, ct), IdNeverAssigned), diagnostic);
                }
                else if (diagnostic.Id == OnlyAssignedToDiagnosticId)
                {
                    context.RegisterFix(CodeAction.Create("Remove not accessed local variable", 
                    ct => RemoveAsync(context, diagnostic, ct), IdNeverAssigned), diagnostic);
                }
            }

            return Task.FromResult(true);
        }

        private async Task<Document> RemoveAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            // TODO: If the assignement is in a for() we remove the whole of it. Too violent ? R# is more intelligent
            // TODO: When assigned a field or method value the compiler doesn't emit any warning. Should create an analyzer.

            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var variableDeclarator = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
                .AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var variableDeclaration = variableDeclarator?.Ancestors().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
            var block = variableDeclarator?.Ancestors().OfType<BlockSyntax>().FirstOrDefault();

            if (variableDeclarator==null||variableDeclaration==null||block==null)
            {
                return context.Document;
            }

            var symbol = semanticModel.GetDeclaredSymbol(variableDeclarator, cancellationToken);

            if (symbol==null)
            {
                return context.Document;
            }

            var assignments = block.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignement => IsAssignementTo(semanticModel, assignement, symbol))
                .ToImmutableList();

            var modifiedRoot = root.TrackNodes(
                assignments.Cast<SyntaxNode>()
                .Concat(new[] { variableDeclaration })
                );

            modifiedRoot = RemoveDeclaration(variableDeclarator, variableDeclaration, modifiedRoot);
            modifiedRoot = RemoveAssignments(semanticModel, assignments, modifiedRoot);

            return context.Document.WithSyntaxRoot(modifiedRoot);
        }

        private static SyntaxNode RemoveAssignments(SemanticModel semanticModel,
            ImmutableList<AssignmentExpressionSyntax> assignments, SyntaxNode root)
        {
            foreach (var assignment in assignments)
            {
                var sideEffects = PotentialSideEffectAnalysis.Analyze(assignment.Right, semanticModel);
                var keepRight = sideEffects.CallMethods;
                if (keepRight)
                {
                    root=root.ReplaceNode(root.GetCurrentNode(assignment),
                        assignment.Right);
                }
                else
                {
                    root=root.RemoveNode(root.GetCurrentNode(assignment).Parent,
                        SyntaxRemoveOptions.KeepNoTrivia);
                }
            }

            return root;
        }

        private static SyntaxNode RemoveDeclaration(VariableDeclaratorSyntax variableDeclarator,
            LocalDeclarationStatementSyntax variableDeclaration, SyntaxNode root)
        {
            if (variableDeclaration.Declaration.Variables.Count>1)
            {
                var newVariables = SeparatedList(variableDeclaration.Declaration.Variables.Where(v => v!=variableDeclarator));
                root = root.ReplaceNode(
                    root.GetCurrentNode(variableDeclaration).Declaration,
                    variableDeclaration.Declaration.WithVariables(newVariables));
            }
            else
            {
                root = root.RemoveNode(root.GetCurrentNode(variableDeclaration),
                    SyntaxRemoveOptions.KeepNoTrivia);
            }

            return root;
        }

        private static bool IsAssignementTo(SemanticModel semanticModel, AssignmentExpressionSyntax assignement, ISymbol symbol)
        {
            var identifierName = assignement.Left as IdentifierNameSyntax;
            if (identifierName==null)
            {
                return false;
            }
            var identifierSymbol = semanticModel.GetSymbolInfo(identifierName);

            return symbol.Equals(identifierSymbol.Symbol);
        }

        public override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return ImmutableArray.Create(
                DeclaredButNeverUsedDiagnosticId,
                OnlyAssignedToDiagnosticId);
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
