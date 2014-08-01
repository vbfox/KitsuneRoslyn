// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
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

        protected override async Task<Document> GetUpdatedDocumentAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var replacement = await GetReplacementAsync(document, semanticModel, root, nodeToFix,
                diagnosticId, cancellationToken).ConfigureAwait(false);

            if (replacement.From == replacement.To)
            {
                return document;
            }

            var annotatedTo = replacement.To.WithAdditionalAnnotations(GetAnnotations());

            var replacedDocument = await document.ReplaceNodeAsync(replacement.From, annotatedTo);
            var finalDocument = await ApplyAdditionalActions(replacedDocument, cancellationToken);

            return finalDocument;
        }

        private async Task<Document> ApplyAdditionalActions(Document document, CancellationToken cancellationToken)
        {
            if (Simplify != AdditionalAction.DoNotRun)
            {
                var simplificationTask = Simplifier.ReduceAsync(
                    document,
                    Simplifier.Annotation,
                    cancellationToken: cancellationToken);

                document = await simplificationTask.ConfigureAwait(false);
            }

            if (Format != AdditionalAction.DoNotRun)
            {
                var formattingTask = Formatter.FormatAsync(
                    document,
                    Formatter.Annotation,
                    cancellationToken: cancellationToken);

                document = await formattingTask.ConfigureAwait(false);
            }

            return document;
        }

        private ImmutableList<SyntaxAnnotation> GetAnnotations()
        {
            var annotations = ImmutableList<SyntaxAnnotation>.Empty;

            if (Simplify == AdditionalAction.AddAnnotationAndRun)
            {
                annotations = annotations.Add(Simplifier.Annotation);
            }
            if (Format == AdditionalAction.AddAnnotationAndRun)
            {
                annotations = annotations.Add(Formatter.Annotation);
            }

            return annotations;
        }

        protected abstract Task<Replacement> GetReplacementAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);
    }
}
