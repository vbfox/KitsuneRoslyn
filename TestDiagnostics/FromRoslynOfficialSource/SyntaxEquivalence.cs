// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.
// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace BlackFox.Roslyn.Diagnostics.FromRoslynOfficialSource
{
    static class SyntaxEquivalence
    {
        public static bool AreTokensEquivalent(SyntaxNodeOrToken before, SyntaxNodeOrToken after)
        {
            return AreEquivalentRecursive(before, after);
        }

        public static bool AreTokensEquivalent(SyntaxToken before, SyntaxToken after)
        {
            if (before.RawKind != after.RawKind)
            {
                return false;
            }

            if (before.IsMissing != after.IsMissing)
            {
                return false;
            }

            // These are the tokens that don't have fixed text.
            switch ((SyntaxKind)before.RawKind)
            {
                case SyntaxKind.IdentifierToken:
                    return before.ValueText == after.ValueText;

                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.CharacterLiteralToken:
                case SyntaxKind.StringLiteralToken:
                    return before.Text == after.Text;
            }

            return true;
        }

        private static bool AreEquivalentRecursive(SyntaxNodeOrToken before, SyntaxNodeOrToken after)
        {
            if (before == after)
            {
                return true;
            }

            if (before == null || after == null)
            {
                return false;
            }

            if (before.RawKind != after.RawKind)
            {
                return false;
            }

            if (before.IsToken)
            {
                Debug.Assert(after.IsToken);
                return AreTokensEquivalent((SyntaxToken)before, (SyntaxToken)after);
            }

            var beforeChildren = before.ChildNodesAndTokens();
            var afterChildren = after.ChildNodesAndTokens();
            if (beforeChildren.Count != afterChildren.Count)
            {
                return false;
            }

            for (int i = 0; i < beforeChildren.Count; i++)
            {
                var child1 = beforeChildren[i];
                var child2 = afterChildren[i];

                if (!AreEquivalentRecursive(child1, child2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
