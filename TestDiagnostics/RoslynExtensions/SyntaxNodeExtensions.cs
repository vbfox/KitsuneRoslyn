// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class SyntaxNodeExtensions
    {
        public static TSyntaxNode WithSameTriviaAs<TSyntaxNode>(this TSyntaxNode target, SyntaxNode source)
            where TSyntaxNode : SyntaxNode
        {
            Parameter.MustNotBeNull(target, "target");
            Parameter.MustNotBeNull(source, "source");

            return target
                .WithLeadingTrivia(source.GetLeadingTrivia())
                .WithTrailingTrivia(source.GetTrailingTrivia());
        }
    }
}
