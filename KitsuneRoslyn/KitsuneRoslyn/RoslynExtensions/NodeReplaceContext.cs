// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.KitsuneRoslyn.RoslynExtensions
{
    struct NodeReplacement
    {
        public Document Document { get;}
        public SyntaxNode From { get; }
        public SyntaxNode To { get; }
        public CancellationToken CancellationToken { get; }
        public AdditionalAction Simplify { get; }
        public AdditionalAction Format { get; }

        public NodeReplacement(Document document, SyntaxNode from, SyntaxNode to,
            CancellationToken cancellationToken = default(CancellationToken),
            AdditionalAction simplify = AdditionalAction.DoNotRun,
            AdditionalAction format = AdditionalAction.DoNotRun)
        {
            Document = document;
            From = from;
            To = to;
            CancellationToken = cancellationToken;
            Simplify = simplify;
            Format = format;
        }

        public async Task<Document> ReplaceAsync()
        {
            if (From == To)
            {
                return Document;
            }

            var annotatedTo = To.WithAdditionalAnnotations(GetAnnotations());

            var replacedDocument = await Document.ReplaceNodeAsync(From, annotatedTo)
                .ConfigureAwait(false);

            return await ApplyAdditionalActions(replacedDocument).ConfigureAwait(false);
        }

        private async Task<Document> ApplyAdditionalActions(Document document)
        {
            if (Simplify != AdditionalAction.DoNotRun)
            {
                var simplificationTask = Simplifier.ReduceAsync(
                    document,
                    Simplifier.Annotation,
                    cancellationToken: CancellationToken);

                document = await simplificationTask.ConfigureAwait(false);
            }

            if (Format != AdditionalAction.DoNotRun)
            {
                var formattingTask = Formatter.FormatAsync(
                    document,
                    Formatter.Annotation,
                    cancellationToken: CancellationToken);

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
    }
}
