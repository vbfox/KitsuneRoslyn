// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class CodeFixProviderBase : CodeFixProvider
    {
        protected virtual bool GetInnermostNodeForTie { get { return true; } }

        ImmutableArray<string> diagnosticIds;

        protected CodeFixProviderBase(ImmutableArray<string> diagnosticIds)
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

            this.diagnosticIds = diagnosticIds.ToImmutableArray();
        }

        protected CodeFixProviderBase(string diagnosticId)
        {
            if (diagnosticId == null)
            {
                throw new ArgumentNullException("diagnosticId");
            }

            diagnosticIds = ImmutableArray<string>.Empty
                .Add(diagnosticId);
        }

        public override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return diagnosticIds;
        }

        protected abstract Task<CodeAction> GetCodeAction(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var nodeToFix = root.FindNode(diagnostic.Location.SourceSpan,
                    getInnermostNodeForTie: GetInnermostNodeForTie);

                var codeAction = await GetCodeAction(context.Document, model, root,
                    nodeToFix, diagnostic.Id, context.CancellationToken).ConfigureAwait(false);

                context.RegisterFix(codeAction, diagnostic);
            }
        }
    }
}
