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
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace BlackFox.Roslyn.TestDiagnostics
{
    [ExportCodeFixProvider(NoStringEmptyDiagnostic.DiagnosticId, LanguageNames.CSharp)]
    internal class NoStringEmptyCodeFix : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { NoStringEmptyDiagnostic.DiagnosticId };
        }

        LiteralExpressionSyntax emptyStringExpression = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(""));

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnosticSpan = diagnostics.First().Location.SourceSpan;

            var memberAccess = root.FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>()
                .First();

            return new[] { CodeAction.Create("Use \"\"", ReplaceWithEmptyStringLiteral(document, root, memberAccess)) };
        }

        private Solution ReplaceWithEmptyStringLiteral(Document document, SyntaxNode root,
            MemberAccessExpressionSyntax memberAccess)
        {
            var newRoot = root.ReplaceNode<SyntaxNode, SyntaxNode>(memberAccess, emptyStringExpression);
            return document.WithSyntaxRoot(newRoot).Project.Solution;
        }
    }
}