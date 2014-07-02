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

namespace BlackFox.Roslyn.TestDiagnostics.NoNewGuid
{
    [ExportCodeFixProvider(NoNewGuidAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    internal class NoNewGuidCodeFix : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { NoNewGuidAnalyzer.DiagnosticId };
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
                ReplaceWithEmptyGuid(document, root, guidCreationExpression));

            return new[] { action };
        }

        private Solution ReplaceWithEmptyGuid(Document document, SyntaxNode root,
            ObjectCreationExpressionSyntax guidCreationExpression)
        {
            var finalExpression = guidEmptyExpression.WithSameTriviaAs(guidCreationExpression);
            var newRoot = root.ReplaceNode<SyntaxNode, SyntaxNode>(guidCreationExpression, finalExpression);
            return document.WithSyntaxRoot(newRoot).Project.Solution;
        }
    }
}