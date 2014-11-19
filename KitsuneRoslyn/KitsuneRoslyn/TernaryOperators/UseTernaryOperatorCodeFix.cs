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

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [ExportCodeFixProvider("BlackFox.UseTernaryOperatorCodeFix", LanguageNames.CSharp)]
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
                : string.Format("Convert to {0} usage{1} of ':?' operator",
                    potentialTernary.TernaryOperatorCount,
                    potentialTernary.TernaryOperatorCount > 1 ? "s" : "");

            var codeAction = CodeAction.Create(description,
                token => GetUpdatedDocumentAsync(potentialTernary, ifStatement, document, root, token));

            return Task.FromResult(codeAction);
        }

        async Task<Document> GetUpdatedDocumentAsync(PotentialTernaryOperator potentialTernary,
            IfStatementSyntax ifStatement, Document document, SyntaxNode root, CancellationToken cancellationToken)
        {
            // As we will apply multiple operations we need to enable tracking for the nodes we will replace/remove.
            var nodesToTrack = ImmutableList<SyntaxNode>.Empty
                .Add(ifStatement)
                .AddRange(potentialTernary.NodesToRemove);

            var wipRoot = root.TrackNodes(nodesToTrack);

            var replacements = potentialTernary.Replacements
                .Cast<SyntaxNode>()
                .Select(n => n.WithAdditionalAnnotations(Formatter.Annotation));

            // Replace the if with the ternary operator
            wipRoot = wipRoot.ReplaceNode(wipRoot.GetCurrentNode(ifStatement), replacements);

            // Remove the next nodes (For when there is no 'else')
            foreach(var nodeToRemove in potentialTernary.NodesToRemove)
            {
                var current = wipRoot.GetCurrentNode(nodeToRemove);
                wipRoot = wipRoot.RemoveNode(current, SyntaxRemoveOptions.KeepNoTrivia);
            }

            // Format to replace ElasticAnnotation
            return await document.FormatAsync(wipRoot, cancellationToken);
        }
    }
}
