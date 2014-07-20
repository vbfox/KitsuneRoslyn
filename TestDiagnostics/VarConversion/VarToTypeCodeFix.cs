// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.VarConversion
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class VarToTypeCodeFix : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.VarToTypeCodeFix";

        protected override bool Simplify { get; } = true;

        public VarToTypeCodeFix()
            :base(VarToTypeAnalyzer.Id, "var can be replaced with specific type")
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var nameSyntax = (NameSyntax)nodeToFix;

            var type = semanticModel.GetTypeInfo(nameSyntax, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var typeSyntax = type.Type.GenerateTypeSyntax().WithSameTriviaAs(nameSyntax);
            return typeSyntax;
        }
    }
}