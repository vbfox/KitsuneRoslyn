// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class CodeFixProviderBase : ICodeFixProvider
    {
        protected virtual bool GetInnermostNodeForTie { get { return true; } }

        ImmutableDictionary<string, string> diagnosticIdsAndDescriptions;

        protected CodeFixProviderBase(ImmutableDictionary<string, string> diagnosticIdsAndDescriptions)
        {
            if (diagnosticIdsAndDescriptions == null)
            {
                throw new ArgumentNullException("diagnosticIdsAndDescriptions");
            }

            this.diagnosticIdsAndDescriptions = diagnosticIdsAndDescriptions;
        }

        protected CodeFixProviderBase(string diagnosticId, string fixDescription)
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

        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return diagnosticIdsAndDescriptions.Keys;
        }

        protected abstract Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var actions = ImmutableList<CodeAction>.Empty;
            foreach (var diagnostic in diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nodeToFix = root.FindNode(diagnostic.Location.SourceSpan,
                    getInnermostNodeForTie: GetInnermostNodeForTie);

                var newDocument = await GetUpdatedDocumentAsync(document, model, root, nodeToFix, diagnostic.Id,
                    cancellationToken).ConfigureAwait(false);

                Debug.Assert(newDocument != null);
                if (newDocument != document)
                {
                    var codeFixDescription = diagnosticIdsAndDescriptions[diagnostic.Id];
                    actions = actions.Add(CodeAction.Create(codeFixDescription, newDocument));
                }
            }

            return actions;
        }
    }
}
