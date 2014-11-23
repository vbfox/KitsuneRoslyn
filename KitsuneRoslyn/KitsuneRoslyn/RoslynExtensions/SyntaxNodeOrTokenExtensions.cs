// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    public static class SyntaxNodeOrTokenExtensions
    {
        /// <summary>
        /// Gets a list of ancestor nodes (including this node if it isn't a token) 
        /// </summary>
        public static IEnumerable<SyntaxNode> AncestorAndSelfNode(this SyntaxNodeOrToken nodeOrToken,
            bool ascendOutOfTrivia = true)
        {
            if (nodeOrToken.IsNode)
            {
                yield return nodeOrToken.AsNode();
            }

            foreach (var node in nodeOrToken.Parent.AncestorsAndSelf(ascendOutOfTrivia))
            {
                yield return node;
            }
        }

        /// <summary>
        /// Gets the first node of type TNode that matches the predicate.
        /// </summary>
        public static TNode FirstAncestorOrSelf<TNode>(this SyntaxNodeOrToken nodeOrToken,
            Func<TNode, bool> predicate = null, bool ascendOutOfTrivia = true)
            where TNode : SyntaxNode
        {
            var tnode = nodeOrToken.AsNode() as TNode;
            if (tnode != null && (predicate == null || predicate(tnode)))
            {
                return tnode;
            }

            return nodeOrToken.Parent.FirstAncestorOrSelf(predicate, ascendOutOfTrivia);
        }
    }
}