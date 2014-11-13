// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace BlackFox.Roslyn.Diagnostics
{
    public class Replacement
    {
        public SyntaxNode From { get; private set; }
        public SyntaxNode To { get; private set; }

        private Replacement(SyntaxNode from, SyntaxNode to)
        {
            Parameter.MustNotBeNull(from, "from");
            Parameter.MustNotBeNull(to, "to");

            From = from;
            To = to;
        }

        public static Replacement Create(SyntaxNode from, SyntaxNode to)
        {
            return new Replacement(from, to);
        }
    }
}
