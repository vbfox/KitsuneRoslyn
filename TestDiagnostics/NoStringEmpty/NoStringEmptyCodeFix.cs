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

namespace BlackFox.Roslyn.TestDiagnostics.NoStringEmpty
{
    [ExportCodeFixProvider(NoStringEmptyAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    public class NoStringEmptyCodeFix : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { NoStringEmptyAnalyzer.DiagnosticId };
        }

        LiteralExpressionSyntax emptyStringLiteralExpression = StringLiteralExpression("");

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var stringEmptyExpression = diagnostics.First()
                .GetAncestorSyntaxNode<MemberAccessExpressionSyntax>(root);

            var action = CodeAction.Create(
                "Use \"\"",
                ReplaceWithEmptyStringLiteral(document, root, stringEmptyExpression));
            return new[] { action };
        }

        private Solution ReplaceWithEmptyStringLiteral(Document document, SyntaxNode root,
            MemberAccessExpressionSyntax stringEmptyExpression)
        {
            var finalExpression = emptyStringLiteralExpression.WithSameTriviaAs(stringEmptyExpression);
            var newRoot = root.ReplaceNode<SyntaxNode, SyntaxNode>(stringEmptyExpression, finalExpression);
            return document.WithSyntaxRoot(newRoot).Project.Solution;
        }
    }
}