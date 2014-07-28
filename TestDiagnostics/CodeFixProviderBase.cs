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
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class CodeFixProviderBase : ICodeFixProvider
    {
        protected virtual bool GetInnermostNodeForTie { get { return true; } }

        ImmutableList<string> diagnosticIds;

        protected CodeFixProviderBase(ImmutableList<string> diagnosticIds)
        {
            if (diagnosticIds == null)
            {
                throw new ArgumentNullException("diagnosticIds");
            }

            this.diagnosticIds = diagnosticIds;
        }

        protected CodeFixProviderBase(IEnumerable<string> diagnosticIds)
        {
            if (diagnosticIds == null)
            {
                throw new ArgumentNullException("diagnosticIds");
            }

            this.diagnosticIds = diagnosticIds.ToImmutableList();
        }

        protected CodeFixProviderBase(string diagnosticId)
        {
            if (diagnosticId == null)
            {
                throw new ArgumentNullException("diagnosticId");
            }

            diagnosticIds = ImmutableList<string>.Empty
                .Add(diagnosticId);
        }

        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return diagnosticIds;
        }

        protected abstract Task<CodeAction> GetCodeAction(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken);

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span,
            IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var actions = ImmutableList<CodeAction>.Empty.ToBuilder();
            foreach (var diagnostic in diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var codeAction = await GetCodeActionX(document, diagnostic, cancellationToken);

                actions.Add(codeAction);
            }

            return actions.ToImmutable();
        }

        private async Task<CodeAction> GetCodeActionX(Document document, Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            try
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                var nodeToFix = root.FindNode(diagnostic.Location.SourceSpan,
                    getInnermostNodeForTie: GetInnermostNodeForTie);

                return await GetCodeAction(document, model, root,
                    nodeToFix, diagnostic.Id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }


}
