// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System;

namespace BlackFox.Roslyn.TestDiagnostics.RoslynExtensions
{
    static class SyntaxNodeExtensions
    {
        public static TSyntaxNode WithSameTriviaAs<TSyntaxNode>(this TSyntaxNode target, SyntaxNode source)
            where TSyntaxNode : SyntaxNode
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return target
                .WithLeadingTrivia(source.GetLeadingTrivia())
                .WithTrailingTrivia(source.GetTrailingTrivia());
        }
    }
}
