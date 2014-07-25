// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [ExportCodeFixProvider("BlackFox.UseTernaryOperatorCodeFix", LanguageNames.CSharp)]
    public class UseTernaryOperatorCodeFix
    : ReplacementCodeFixProviderBase
    {
        protected override bool Format { get; } = true;

        static readonly ImmutableDictionary<string, string> diagnostics
            = ImmutableDictionary<string, string>.Empty
                .Add(UseTernaryOperatorAnalyzer.IdSimple, "Convert to return statement")
                .Add(UseTernaryOperatorAnalyzer.IdComplex, "Convert to ':?' operator");

        public UseTernaryOperatorCodeFix()
            : base(diagnostics)
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)nodeToFix;

            var potentialTernary = PotentialTernaryOperator.Create(ifStatement);

            return potentialTernary.GetFullReplacement();
        }
    }
}
