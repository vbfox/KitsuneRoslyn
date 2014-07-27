// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.
// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace BlackFox.Roslyn.Diagnostics
{
    public static class SyntaxDifferences
    {
        public static ImmutableList<Tuple<SyntaxNode, SyntaxNode>> Find(SyntaxNodeOrToken before,
            SyntaxNodeOrToken after)
        {
            var builder = ImmutableList<Tuple<SyntaxNode, SyntaxNode>>
                .Empty.ToBuilder();
            FindSimpleDifferencesRecursive(before, after, builder);
            return builder.ToImmutable();
        }

        private static bool FindSimpleDifferencesRecursive(SyntaxNodeOrToken before, SyntaxNodeOrToken after,
            IList<Tuple<SyntaxNode, SyntaxNode>> differences)
        {
            if (before == after)
            {
                return true;
            }

            if (before == null || after == null || before.RawKind != after.RawKind)
            {
                if (before.IsNode && after.IsNode)
                {
                    // If any is a token the difference will be reported in the parents
                    differences.Add(Tuple.Create(before.AsNode(), after.AsNode()));
                }

                return false;
            }

            if (before.IsToken)
            {
                Debug.Assert(after.IsToken);
                return SyntaxEquivalence.AreTokensEquivalent((SyntaxToken)before, (SyntaxToken)after);
            }

            var beforeChildren = before.ChildNodesAndTokens();
            var afterChildren = after.ChildNodesAndTokens();
            if (beforeChildren.Count != afterChildren.Count)
            {
                differences.Add(Tuple.Create(before.AsNode(), after.AsNode()));
                return false;
            }

            bool result = true;
            for (int i = 0; i < beforeChildren.Count; i++)
            {
                var child1 = beforeChildren[i];
                var child2 = afterChildren[i];

                var childEquivalent = FindSimpleDifferencesRecursive(child1, child2, differences);

                if (!childEquivalent && (child1.IsToken || child2.IsToken))
                {
                    // Report token differences as differences in the parent nodes
                    differences.Add(Tuple.Create(before.AsNode(), after.AsNode()));
                    return false;
                }

                result &= childEquivalent;
            }

            return result;
        }
    }
}