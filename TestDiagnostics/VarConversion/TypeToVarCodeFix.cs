// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.VarConversion
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class TypeToVarCodeFix : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.TypeToVarCodeFix";

        static readonly ImmutableDictionary<string, string> diagnostics
            = ImmutableDictionary<string, string>.Empty
                .Add(TypeToVarAnalyzer.Id, "Use 'var'")
                .Add(TypeToVarAnalyzer.IdWithCast, "Use 'var'");

        public TypeToVarCodeFix()
            :base(diagnostics)
        {
        }

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            return IdentifierName("var")
                .WithSameTriviaAs(nodeToFix);
        }
    }
}