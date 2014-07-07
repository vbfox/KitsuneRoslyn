using System.Collections.Generic;
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
    [ExportCodeFixProvider(NoNewGuidAnalyzer.Id, LanguageNames.CSharp)]
    public class NoNewGuidCodeFix : CodeFixProviderBase
    {
        public override IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { NoNewGuidAnalyzer.Id };
        }

        ExpressionSyntax guidEmptyExpression = SimpleMemberAccessExpression("System", "Guid", "Empty");

        protected override string GetCodeFixDescription(string ruleId)
        {
            return "Replace with Guid.Empty";
        }

        protected override bool GetInnermostNodeForTie
        {
            get
            {
                return true;
            }
        }

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