// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.TestDiagnostics.StringConcatenation.NoStringConcat
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceStringConcatWithSingleString : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceStringConcatWithSingleString";

        public ReplaceStringConcatWithSingleString()
            :base(NoStringConcatAnalyzer.UseStringId, "Replace with a single string")
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var invocation = (InvocationExpressionSyntax)nodeToFix;

            var info = StringConcatInfo.Create(invocation, semanticModel);
            Debug.Assert(info.Classification == StringConcatClassification.ReplaceWithSingleString,
                "Expected replace with single string classification");

            var singleString = info.Expressions.Coalesce(semanticModel);
            return StringLiteralExpression(singleString);
        }
    }
}
