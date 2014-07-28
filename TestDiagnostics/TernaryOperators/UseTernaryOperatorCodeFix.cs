// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [ExportCodeFixProvider("BlackFox.UseTernaryOperatorCodeFix2", LanguageNames.CSharp)]
    public class UseTernaryOperatorCodeFix : CodeFixProviderBase
    {
        static readonly ImmutableList<string> fixableDiagnosticIds
            = ImmutableList<string>.Empty
            .Add(UseTernaryOperatorAnalyzer.IdSimple)
            .Add(UseTernaryOperatorAnalyzer.IdComplex);

        public UseTernaryOperatorCodeFix() : base(fixableDiagnosticIds)
        {
        }

        protected override Task<CodeAction> GetCodeAction(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)nodeToFix;

            var potentialTernary = PotentialTernaryOperator.Create(ifStatement);

            string description = diagnosticId == UseTernaryOperatorAnalyzer.IdSimple
                ? "Convert to ':?' operator"
                : string.Format("Convert {0} usages of ':?' operator", potentialTernary.TernaryOperatorCount);

            var codeAction = CodeAction.Create(description,
                token => GetUpdatedDocumentAsync(potentialTernary, ifStatement, document, root, token));

            return Task.FromResult(codeAction);
        }

        async Task<Document> GetUpdatedDocumentAsync(PotentialTernaryOperator potentialTernary,
            IfStatementSyntax ifStatement, Document document, SyntaxNode root, CancellationToken cancellationToken)
        {
            // As we will apply multiple operations we need to enable tracking for the nodes we will replace/remove.
            var nodesToTrack = ImmutableList<SyntaxNode>.Empty.Add(ifStatement);
            if (potentialTernary.NodeToRemove.HasValue)
            {
                nodesToTrack = nodesToTrack.Add(potentialTernary.NodeToRemove.Value);
            }

            var wipRoot = root.TrackNodes(nodesToTrack);

            var replacements = potentialTernary.Replacements
                .Cast<SyntaxNode>()
                .Select(n => n.WithAdditionalAnnotations(Formatter.Annotation));

            // Replace the if with the ternary operator
            wipRoot = wipRoot.ReplaceNode(wipRoot.GetCurrentNode(ifStatement), replacements);

            // Remove the potential next node (For when there is no 'else')
            if (potentialTernary.NodeToRemove.HasValue)
            {
                var nodeToRemove = wipRoot.GetCurrentNode(potentialTernary.NodeToRemove.Value);
                wipRoot = wipRoot.RemoveNode(nodeToRemove,
                    SyntaxRemoveOptions.KeepNoTrivia);
            }

            // Format to replace ElasticAnnotation
            return await document.FormatAsync(wipRoot, cancellationToken);
        }


    }
}
