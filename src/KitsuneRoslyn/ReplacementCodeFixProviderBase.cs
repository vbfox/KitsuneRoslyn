// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.KitsuneRoslyn.RoslynExtensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class ReplacementCodeFixProviderBase : SimpleCodeFixProviderBase
    {
        protected virtual AdditionalAction Simplify { get; } = AdditionalAction.DoNotRun;
        protected virtual AdditionalAction Format { get; } = AdditionalAction.DoNotRun;

        protected ReplacementCodeFixProviderBase(string diagnosticId, string fixDescription)
            : base(diagnosticId, fixDescription)
        {
        }

        protected ReplacementCodeFixProviderBase(ImmutableDictionary<string, string> diagnosticIdsAndDescriptions)
            : base(diagnosticIdsAndDescriptions)
        {
        }

        protected sealed override async Task<Document> GetUpdatedDocumentAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var replacement = await GetReplacementAsync(document, semanticModel, root, nodeToFix,
                diagnosticId, cancellationToken).ConfigureAwait(false);

            var replacementAction = new NodeReplacement(document, replacement.From,
                replacement.To, cancellationToken, Simplify, Format);

            return await replacementAction.ReplaceAsync().ConfigureAwait(false);
        }

        protected abstract Task<Replacement> GetReplacementAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);
    }
}
