using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics.MethodCanBeMadeStatic
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class MethodCanBeMadeStaticCodeFix : SimpleCodeFixProviderBase
    {
        public const string Id = "BlackFox.MethodCanBeMadeStaticCodeFix";

        public MethodCanBeMadeStaticCodeFix()
            : base(MethodCanBeMadeStaticAnalyzer.Id, "Make method static")
        {
        }

        protected override async Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var method = nodeToFix.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            MethodDeclarationSyntax fixedMethod = GetFixedMethod(method);

            var newRoot = root.ReplaceNode(method, fixedMethod);
            var newDocument = document.WithSyntaxRoot(newRoot);

            var formattingTask = Formatter.FormatAsync(
                newDocument,
                Formatter.Annotation,
                cancellationToken: cancellationToken);

            return await formattingTask.ConfigureAwait(false);
        }

        private static MethodDeclarationSyntax GetFixedMethod(MethodDeclarationSyntax method)
        {
            var token = SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                .WithAdditionalAnnotations(Formatter.Annotation);

            return method.AddModifiers(token);
        }
    }
}
