using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class SimpleCodeFixProviderBase : CodeFixProviderBase
    {
        ImmutableDictionary<string, string> diagnosticIdsAndDescriptions;

        protected SimpleCodeFixProviderBase(ImmutableDictionary<string, string> diagnosticIdsAndDescriptions)
            : base(diagnosticIdsAndDescriptions.Keys)
        {
            if (diagnosticIdsAndDescriptions == null)
            {
                throw new ArgumentNullException("diagnosticIdsAndDescriptions");
            }

            this.diagnosticIdsAndDescriptions = diagnosticIdsAndDescriptions;
        }

        protected SimpleCodeFixProviderBase(string diagnosticId, string fixDescription)
            : base(ImmutableList<string>.Empty.Add(diagnosticId))
        {
            if (diagnosticId == null)
            {
                throw new ArgumentNullException("diagnosticId");
            }
            if (fixDescription == null)
            {
                throw new ArgumentNullException("fixDescription");
            }

            diagnosticIdsAndDescriptions = ImmutableDictionary<string, string>.Empty
                .Add(diagnosticId, fixDescription);
        }

        protected abstract Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);

        protected override Task<CodeAction> GetCodeAction(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var codeFixDescription = diagnosticIdsAndDescriptions[diagnosticId];

            var codeAction = CodeAction.Create(codeFixDescription,
                token => GetUpdatedDocumentAsync(document, semanticModel, root, nodeToFix, diagnosticId, token));

            return Task.FromResult(codeAction);
        }
    }
}
