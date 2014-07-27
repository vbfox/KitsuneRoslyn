// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.CodeAnalysis.Formatting;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [ExportCodeFixProvider("BlackFox.UseTernaryOperatorCodeFix", LanguageNames.CSharp)]
    public class UseTernaryOperatorCodeFix
    : CodeFixProviderBase
    {
        static readonly ImmutableDictionary<string, string> diagnostics
            = ImmutableDictionary<string, string>.Empty
                .Add(UseTernaryOperatorAnalyzer.IdSimple, "Convert to return statement")
                .Add(UseTernaryOperatorAnalyzer.IdComplex, "Convert to ':?' operator");

        public UseTernaryOperatorCodeFix()
            : base(diagnostics)
        {
        }

        protected async override Task<Document> GetUpdatedDocumentAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)nodeToFix;

            var potentialTernary = PotentialTernaryOperator.Create(ifStatement);

            // As we will apply multiple operations we need to enable tracking for the nodes we will replace/remove.
            var nodesToTrack = ImmutableList<SyntaxNode>.Empty.Add(ifStatement);
            if (potentialTernary.NodeToRemove.HasValue)
            {
                nodesToTrack = nodesToTrack.Add(potentialTernary.NodeToRemove.Value);
            }
            var wipRoot = root.TrackNodes(nodesToTrack);

            // Replace the if with the ternary operator
            var replacement = potentialTernary.GetReplacement().WithAdditionalAnnotations(Formatter.Annotation);
            wipRoot = wipRoot.ReplaceNode(wipRoot.GetCurrentNode(ifStatement), replacement);

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
