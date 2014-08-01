// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class ReplacementNodeCodeFixProviderBase : ReplacementCodeFixProviderBase
    {
        protected ReplacementNodeCodeFixProviderBase(ImmutableDictionary<string, string> diagnosticIdsAndDescriptions)
            : base(diagnosticIdsAndDescriptions)
        {
        }

        protected ReplacementNodeCodeFixProviderBase(string diagnosticId, string fixDescription)
            : base(diagnosticId, fixDescription)
        {
        }

        protected async override Task<Replacement> GetReplacementAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var to = await GetReplacementNodeAsync(document, semanticModel, root, nodeToFix, diagnosticId,
                cancellationToken);
            return Replacement.Create(nodeToFix, to);
        }

        protected abstract Task<SyntaxNode> GetReplacementNodeAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);
    }
}
