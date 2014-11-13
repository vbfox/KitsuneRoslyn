// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.NoNewGuid
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceNewGuidWithGuidEmpty : ReplacementNodeCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceNewGuidWithGuidEmpty";

        protected override AdditionalAction Simplify { get; } = AdditionalAction.AddAnnotationAndRun;

        static readonly ExpressionSyntax guidEmptyExpression
            = SimpleMemberAccessExpression("System", "Guid", "Empty");

        public ReplaceNewGuidWithGuidEmpty()
            : base(NoNewGuidAnalyzer.Id, "Replace with Guid.Empty")
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            return guidEmptyExpression;
        }
    }
}