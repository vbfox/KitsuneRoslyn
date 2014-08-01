// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.CanReplaceConcatOperator
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceConcatenationWithSingleString()
        : ReplacementNodeCodeFixProviderBase(
            CanReplaceConcatOperatorAnalyzer.UseStringId,
            "Replace with a single string")
    {
        public const string Id = "BlackFox.ReplaceConcatenationWithSingleString";

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var binaryExpression = (BinaryExpressionSyntax)nodeToFix;

            var info = StringConcatOperatorInfo.Create(binaryExpression, semanticModel);
            Debug.Assert(info.Classification == StringConcatOperatorClassification.ReplaceWithSingleString,
                "Expected replace with single string classification");

            var singleString = StringCoalescing.Coalesce(info.Expressions, semanticModel);
            return StringLiteralExpression(singleString);
        }
    }
}
