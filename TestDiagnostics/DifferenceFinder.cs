// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.
// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics
{
    public static class DifferenceFinder
    {
        public static ImmutableList<Tuple<SyntaxNode, SyntaxNode>> FindSimpleDifferences(SyntaxNodeOrToken before,
            SyntaxNodeOrToken after)
        {
            var builder = ImmutableList<Tuple<SyntaxNode, SyntaxNode>>
                .Empty.ToBuilder();
            FindSimpleDifferencesRecursive(before, after, builder);
            return builder.ToImmutable();
        }

        public static bool TryFindTernaryReplacable(SyntaxNodeOrToken before,
            SyntaxNodeOrToken after, out ImmutableList<Tuple<SyntaxNode, SyntaxNode>> differences)
        {
            var rawDifferences = FindSimpleDifferences(before, after);
            var manageableDifferences = rawDifferences
                .Select(d => NearestReplacableByTernary(d.Item1, d.Item2, before, after))
                .ToImmutableList();

            if (manageableDifferences.Any(d => d == null))
            {
                differences = null;
                return false;
            }

            differences = manageableDifferences;
            return true;
        }

        private static Tuple<SyntaxNode, SyntaxNode> NearestReplacableByTernary(
            SyntaxNode item1, SyntaxNode item2,
            SyntaxNodeOrToken before, SyntaxNodeOrToken after)
        {
            if (item1 == before || item2 == after)
            {
                return null;
            }

            if (IsReplacableByTernary(item1) && IsReplacableByTernary(item2))
            {
                return Tuple.Create(item1, item2);
            }

            return NearestReplacableByTernary(item1.Parent, item2.Parent, before, after);
        }

        static HashSet<SyntaxKind> okParents = new HashSet<SyntaxKind>
        {
            SyntaxKind.Argument,
            SyntaxKind.EqualsValueClause,
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
        };

        static HashSet<SyntaxKind> rightBinaryOkParents = new HashSet<SyntaxKind>
        {
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression,
        };

        static HashSet<SyntaxKind> skipAndCheckParent = new HashSet<SyntaxKind>
        {
            SyntaxKind.SimpleMemberAccessExpression,
        };

        private static bool IsReplacableByTernary(SyntaxNode node)
        {
            var parentKind = node.Parent.CSharpKind();
            if (okParents.Contains(parentKind))
            {
                return true;
            }
            if (rightBinaryOkParents.Contains(parentKind))
            {
                return ((BinaryExpressionSyntax)node.Parent).Right == node;
            }
            return false;
        }

        private static bool AreTokensEquivalent(SyntaxToken before, SyntaxToken after)
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

        private static bool ValidForInnerDifferences(SyntaxNodeOrToken before, SyntaxNodeOrToken after,
            IList<Tuple<SyntaxNode, SyntaxNode>> differences)
        {
            var beforeNode = before.AsNode();
            var afterNode = after.AsNode();
            var ok = beforeNode != null && afterNode != null
                && okParents.Contains(before.CSharpKind())
                && okParents.Contains(after.CSharpKind());
            if (ok)
            {
                differences.Add(Tuple.Create(beforeNode, afterNode));
            }
            return ok;
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
                return AreTokensEquivalent((SyntaxToken)before, (SyntaxToken)after);
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
