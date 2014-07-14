﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.SyntaxFactoryAdditions;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.TestDiagnostics.NoNewGuid
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceNewGuidWithGuidEmpty : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceNewGuidWithGuidEmpty";

        public ReplaceNewGuidWithGuidEmpty()
            : base(NoNewGuidAnalyzer.Id, "Replace with Guid.Empty")
        {
        }

        protected override bool Simplify
        {
            get
            {
                return true;
            }
        }

        ExpressionSyntax guidEmptyExpression = SimpleMemberAccessExpression("System", "Guid", "Empty");

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document, SemanticModel model,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            return guidEmptyExpression;
        }
    }
}