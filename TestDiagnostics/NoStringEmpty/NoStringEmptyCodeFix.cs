using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringEmpty
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class NoStringEmptyCodeFix : CodeFixProviderBase
    {
        public const string Id = "BlackFox.CodeFixes.NoStringEmpty";

        public NoStringEmptyCodeFix()
            : base(NoStringEmptyAnalyzer.Id, "Use \"\"")
        {
        }

        LiteralExpressionSyntax emptyStringLiteralExpression = StringLiteralExpression("");

        internal override Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel model,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var stringEmptyExpression = (MemberAccessExpressionSyntax)nodeToFix;
            var finalExpression = emptyStringLiteralExpression.WithSameTriviaAs(stringEmptyExpression);
            
            return document.ReplaceNode<SyntaxNode, SyntaxNode>(stringEmptyExpression, finalExpression);
        }
    }
}