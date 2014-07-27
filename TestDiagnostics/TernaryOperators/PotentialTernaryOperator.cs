﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.FromRoslynOfficialSource;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    class PotentialTernaryOperator
    {
        public static PotentialTernaryOperator NoReplacement { get; }
            = new PotentialTernaryOperator(
                PotentialTernaryOperatorClassification.NoReplacement,
                null, null, null, new Optional<SyntaxNode>(), null);

        public PotentialTernaryOperatorClassification Classification { get; private set; }
        public ExpressionSyntax Condition { get; private set; }
        public ExpressionSyntax WhenTrue { get; private set; }
        public ExpressionSyntax WhenFalse { get; private set; }
        public Optional<SyntaxNode> NodeToRemove { get; private set; }
        public Func<ConditionalExpressionSyntax, SyntaxNode> Replacement { get; private set; }

        PotentialTernaryOperator(PotentialTernaryOperatorClassification classification,
            ExpressionSyntax condition, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse,
            Optional<SyntaxNode> toRemove, Func<ConditionalExpressionSyntax, SyntaxNode> useTernaryExpression)
        {
            Classification = classification;
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
            NodeToRemove = toRemove;
            Replacement = useTernaryExpression;
        }

        public SyntaxNode GetReplacement()
        {
            var conditional = ConditionalExpression(
                Condition,
                ParenthesesAroundConditionalExpression(WhenTrue),
                ParenthesesAroundConditionalExpression(WhenFalse));

            return Replacement(conditional)
                .WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation);
        }

        static ExpressionSyntax ParenthesesAroundConditionalExpression(ExpressionSyntax expression)
        {
            return expression is ConditionalExpressionSyntax
                ? expression.WithParentheses()
                : expression;
        }

        public static PotentialTernaryOperator Create(IfStatementSyntax ifStatement)
        {
            var isReturn = ExtractIfSingleElementsAs<ReturnStatementSyntax>(ifStatement,
                out var whenTrueReturn, out var whenFalseReturn, out var toRemoveReturn);

            if (isReturn)
            {
                return CreateForReturn(ifStatement, whenTrueReturn, whenFalseReturn, toRemoveReturn);
            }

            var isBinaryExpression = ExtractIfSingleElementsAs<BinaryExpressionSyntax>(ifStatement,
                out var whenTrueBinaryExpression, out var whenFalseBinaryExpression, out var toRemoveBinaryExpression);

            if (isBinaryExpression)
            {
                return CreateForAssignment(ifStatement, whenTrueBinaryExpression, whenFalseBinaryExpression,
                    toRemoveBinaryExpression);
            }

            return NoReplacement;
        }

        private static PotentialTernaryOperator CreateForAssignment(IfStatementSyntax ifStatement,
            BinaryExpressionSyntax whenTrue, BinaryExpressionSyntax whenFalse, Optional<SyntaxNode> toRemove)
        {
            if (!whenTrue.IsKind(SyntaxKind.SimpleAssignmentExpression)
                || !whenFalse.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return NoReplacement;
            }

            var equivalent = SyntaxEquivalence.AreTokensEquivalent(whenTrue.Left, whenFalse.Left);
            if (!equivalent)
            {
                return NoReplacement;
            }
            Func<ConditionalExpressionSyntax, SyntaxNode> usage =
                ternary => ExpressionStatement(
                    BinaryExpression(SyntaxKind.SimpleAssignmentExpression, whenTrue.Left,
                        ternary.WithLeadingTrivia(ElasticMarker)));

            return new PotentialTernaryOperator(PotentialTernaryOperatorClassification.Assignment,
                ifStatement.Condition, whenTrue.Right, whenFalse.Right, toRemove, usage);
        }

        private static PotentialTernaryOperator CreateForReturn(IfStatementSyntax ifStatement,
            ReturnStatementSyntax whenTrue, ReturnStatementSyntax whenFalse, Optional<SyntaxNode> toRemove)
        {
            if (whenTrue.Expression == null || whenFalse.Expression == null)
            {
                return NoReplacement;
            }

            Func<ConditionalExpressionSyntax, SyntaxNode> usage =
                ternary => ReturnStatement(ternary.WithLeadingTrivia(ElasticMarker));

            return new PotentialTernaryOperator(PotentialTernaryOperatorClassification.Return,
                ifStatement.Condition, whenTrue.Expression, whenFalse.Expression, toRemove, usage);
        }

        static bool ExtractIfSingleElementsAs<TSyntaxNode>(IfStatementSyntax ifStatement,
            out TSyntaxNode whenTrue, out TSyntaxNode whenFalse, out Optional<SyntaxNode> toRemove)
            where TSyntaxNode : SyntaxNode
        {
            whenTrue = GetSingleElementOrDefault(ifStatement.Statement) as TSyntaxNode;

            if (whenTrue == null)
            {
                whenFalse = null;
                toRemove = new Optional<SyntaxNode>();
                return false;
            }

            if (ifStatement.Else != null)
            {
                whenFalse = GetSingleElementOrDefault(ifStatement.Else.Statement)
                    as TSyntaxNode;
                toRemove = new Optional<SyntaxNode>();
            }
            else
            {
                var sibilings = ifStatement.Parent.ChildNodes();
                var nextNode = sibilings.SkipWhile(n => n != ifStatement).Skip(1).FirstOrDefault();
                whenFalse = GetSingleElementOrDefault(nextNode) as TSyntaxNode;
                toRemove = whenFalse != null ? nextNode : new Optional<SyntaxNode>();
            }

            return whenFalse != null;
        }

        static SyntaxNode GetSingleElementOrDefault(SyntaxNode node)
        {
            var isSupported = node is BlockSyntax || node is ExpressionStatementSyntax;

            if (isSupported)
            {
                var childNodes = node.ChildNodes();
                if (childNodes.Count() != 1)
                {
                    return null;
                }


                return GetSingleElementOrDefault(childNodes.Single());
            }

            return node;
        }
    }
}
