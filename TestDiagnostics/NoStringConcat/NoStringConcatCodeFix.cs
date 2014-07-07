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
using System.Diagnostics;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringConcat
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class NoStringConcatCodeFix : ICodeFixProvider
    {
        public const string Id = "BlackFox.NoStringConcatCodeFix";

        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[]
            {
                NoStringConcatAnalyzer.UseStringId,
                //NoStringConcatAnalyzer.UseFormatId
            };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            
            var stringConcatExpression = diagnostics.First()
                .GetAncestorSyntaxNode<InvocationExpressionSyntax>(root);

            var action = CodeAction.Create(
                "Replace with a single string",
                token => ReplaceWithSingleString(document, root, stringConcatExpression, token));

            return new[] { action };
        }

        private async Task<Document> ReplaceWithSingleString(Document document, SyntaxNode root,
            InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false);
            var info = StringConcatInfo.Create(invocation, semanticModel);
            Debug.Assert(info.Classification == StringConcatClassification.ReplaceWithSingleString);

            var singleString = info.Expressions.Coalesce(semanticModel);
            var singleStringExpression = StringLiteralExpression(singleString)
                .WithSameTriviaAs(invocation);

            var newRoot = root.ReplaceNode<SyntaxNode, SyntaxNode>(invocation, singleStringExpression);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
