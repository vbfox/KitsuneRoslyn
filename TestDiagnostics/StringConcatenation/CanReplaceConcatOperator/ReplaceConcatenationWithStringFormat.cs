// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.TestDiagnostics.StringConcatenation.CanReplaceConcatOperator
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceConcatenationWithStringFormat : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceConcatenationWithStringFormat";

        protected override bool Simplify
        {
            get
            {
                return true;
            }
        }

        protected override bool Format
        {
            get
            {
                return true;
            }
        }

        public ReplaceConcatenationWithStringFormat()
            : base(CanReplaceConcatOperatorAnalyzer.UseFormatId, "Replace with String.Format")
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var binaryExpression = (BinaryExpressionSyntax)nodeToFix;

            var info = StringConcatOperatorInfo.Create(binaryExpression, semanticModel);
            Debug.Assert(info.Classification == StringConcatOperatorClassification.ReplaceWithStringFormat,
                "Expected replace with string format classification");

            return StringCoalescing.ToStringFormat(info.Expressions, semanticModel);
        }
    }
}