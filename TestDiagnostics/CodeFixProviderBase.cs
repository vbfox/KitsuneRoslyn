﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace BlackFox.Roslyn.TestDiagnostics
{
    public abstract class CodeFixProviderBase : ICodeFixProvider
    {
        protected virtual bool GetInnermostNodeForTie { get { return false; } }

        protected abstract string GetCodeFixDescription(string ruleId);

        public abstract IEnumerable<string> GetFixableDiagnosticIds();

        internal abstract Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel model,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken);

        async Task<IEnumerable<CodeAction>> ICodeFixProvider.GetFixesAsync(Document document, TextSpan span,
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
                    var codeFixDescription = GetCodeFixDescription(diagnostic.Id);
                    actions = actions.Add(CodeAction.Create(codeFixDescription, newDocument));
                }
            }

            return actions;
        }
    }
}
