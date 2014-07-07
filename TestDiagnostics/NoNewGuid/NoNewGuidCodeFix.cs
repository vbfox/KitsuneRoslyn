using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis.Simplification;

namespace BlackFox.Roslyn.TestDiagnostics.NoNewGuid
{
    [ExportCodeFixProvider(NoNewGuidAnalyzer.Id, LanguageNames.CSharp)]
    public class NoNewGuidCodeFix : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { NoNewGuidAnalyzer.Id };
        }

        ExpressionSyntax guidEmptyExpression = SimpleMemberAccessExpression("System", "Guid", "Empty");

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var guidCreationExpression = diagnostics.First()
                .GetAncestorSyntaxNode<ObjectCreationExpressionSyntax>(root);

            var action = CodeAction.Create(
                "Replace with Guid.Empty",
                token => ReplaceWithEmptyGuid(document, root, guidCreationExpression, token));

            return new[] { action };
        }

        private async Task<Document> ReplaceWithEmptyGuid(Document document, SyntaxNode root,
            ObjectCreationExpressionSyntax guidCreationExpression, CancellationToken cancellationToken)
        {
            var finalExpression = guidEmptyExpression
                .WithSameTriviaAs(guidCreationExpression)
                .WithAdditionalAnnotations(Simplifier.Annotation);
            var newRoot = root.ReplaceNode<SyntaxNode, SyntaxNode>(guidCreationExpression, finalExpression);

            var simplificationTask = Simplifier.ReduceAsync(
                document.WithSyntaxRoot(newRoot),
                Simplifier.Annotation,
                cancellationToken: cancellationToken);

            return await simplificationTask.ConfigureAwait(false);
        }
    }
}