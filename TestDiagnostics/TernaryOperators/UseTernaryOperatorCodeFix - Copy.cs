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
using System.Linq;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [ExportCodeFixProvider("BlackFox.UseTernaryOperatorCodeFix2", LanguageNames.CSharp)]
    public class UseTernaryOperatorCodeFix2
    : CodeFixProviderBase
    {
        static readonly ImmutableDictionary<string, string> diagnostics
            = ImmutableDictionary<string, string>.Empty
                .Add(UseTernaryOperatorAnalyzer2.IdSimple, "X-Convert to return statement")
                .Add(UseTernaryOperatorAnalyzer2.IdComplex, "X-Convert to ':?' operator");

        public UseTernaryOperatorCodeFix2()
            : base(diagnostics)
        {
        }

        protected async override Task<Document> GetUpdatedDocumentAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)nodeToFix;

            var potentialTernary = PotentialTernaryOperator2.Create(ifStatement);

            // As we will apply multiple operations we need to enable tracking for the nodes we will replace/remove.
            var nodesToTrack = ImmutableList<SyntaxNode>.Empty.Add(ifStatement);
            if (potentialTernary.NodeToRemove.HasValue)
            {
                nodesToTrack = nodesToTrack.Add(potentialTernary.NodeToRemove.Value);
            }
            var wipRoot = root.TrackNodes(nodesToTrack);

            // Replace the if with the ternary operator
            wipRoot = wipRoot.ReplaceNode(wipRoot.GetCurrentNode(ifStatement),
                potentialTernary.Replacements.Cast<SyntaxNode>().Select(n => n.WithAdditionalAnnotations(Formatter.Annotation)));

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
