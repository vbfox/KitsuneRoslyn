using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis.Simplification;

namespace BlackFox.Roslyn.TestDiagnostics.NoNewGuid
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceNewGuidWithGuidEmpty : CodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceNewGuidWithGuidEmpty";

        public ReplaceNewGuidWithGuidEmpty()
            : base(NoNewGuidAnalyzer.Id, "Replace with Guid.Empty")
        {
        }

        ExpressionSyntax guidEmptyExpression = SimpleMemberAccessExpression("System", "Guid", "Empty");

        internal override async Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel model,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var guidCreationExpression = (ObjectCreationExpressionSyntax)nodeToFix;

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