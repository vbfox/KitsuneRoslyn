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
using System.Linq;
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

        public Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var actions = ImmutableList<CodeAction>.Empty.ToBuilder();
            foreach (var diagnostic in diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var codeAction = GetCodeAction(document, diagnostic);

                actions.Add(codeAction);
            }

            return Task.FromResult(actions.ToImmutable().AsEnumerable());
        }

        private CodeAction GetCodeAction(Document document, Diagnostic diagnostic)
        {
            Func<CancellationToken, Task<Document>> applyFix = async (cancellationToken) => {
                try
                {
                    var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                    var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                    var nodeToFix = root.FindNode(diagnostic.Location.SourceSpan,
                        getInnermostNodeForTie: GetInnermostNodeForTie);

                    var newDocument = await GetUpdatedDocumentAsync(document, model, root, nodeToFix, diagnostic.Id,
                        cancellationToken).ConfigureAwait(false);

                    return newDocument;
                }
                catch (Exception)
                {
                    throw;
                }
            };

            var codeFixDescription = diagnosticIdsAndDescriptions[diagnostic.Id];
            return CodeAction.Create(codeFixDescription, applyFix);
        }
    }
}
