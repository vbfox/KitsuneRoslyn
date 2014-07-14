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

namespace BlackFox.Roslyn.TestDiagnostics.StringConcatenation.NoStringConcat
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceStringConcatWithStringFormat : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceStringConcatWithStringFormat";

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

        public ReplaceStringConcatWithStringFormat()
            : base(NoStringConcatAnalyzer.UseFormatId, "Replace with String.Format")
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var invocation = (InvocationExpressionSyntax)nodeToFix;

            var info = StringConcatInfo.Create(invocation, semanticModel);
            Debug.Assert(info.Classification == StringConcatClassification.ReplaceWithStringFormat,
                "Expected replace with string format classification");

            return StringCoalescing.ToStringFormat(info.Expressions, semanticModel);
        }
    }
}