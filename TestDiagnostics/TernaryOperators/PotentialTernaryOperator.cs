// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

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
        public ExpressionSyntax Condition { get; private set; }
        public ExpressionSyntax WhenTrue { get; private set; }
        public ExpressionSyntax WhenFalse { get; private set; }
        public Func<ConditionalExpressionSyntax, SyntaxNode> Replacement { get; private set; }

        PotentialTernaryOperator(ExpressionSyntax condition, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse,
            Func<ConditionalExpressionSyntax, SyntaxNode> useTernaryExpression)
        {
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
            Replacement = useTernaryExpression;
        }

        public SyntaxNode GetFullReplacement()
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

        public static Optional<PotentialTernaryOperator> Create(IfStatementSyntax ifStatement)
        {
            var trueReturn = GetSingleElementFromBlockOrDefault(ifStatement.Statement) as ReturnStatementSyntax;

            if (trueReturn == null || trueReturn.Expression == null)
            {
                return null;
            }

            ReturnStatementSyntax falseReturn = null;

            if (ifStatement.Else != null)
            {
                falseReturn = GetSingleElementFromBlockOrDefault(ifStatement.Else.Statement)
                    as ReturnStatementSyntax;
            }
            else
            {
                var sibilings = ifStatement.Parent.ChildNodes();
                var nextNode = sibilings.SkipWhile(n => n != ifStatement).Skip(1).FirstOrDefault();
                falseReturn = GetSingleElementFromBlockOrDefault(nextNode) as ReturnStatementSyntax;
            }

            if (falseReturn == null || falseReturn.Expression == null)
            {
                return null;
            }

            Func<ConditionalExpressionSyntax, SyntaxNode> createCall = ternary => ReturnStatement(ternary.WithLeadingTrivia(ElasticMarker));

            return new PotentialTernaryOperator(ifStatement.Condition, trueReturn.Expression,
                falseReturn.Expression, createCall);
        }

        static SyntaxNode GetSingleElementFromBlockOrDefault(SyntaxNode node)
        {
            var block = node as BlockSyntax;

            if (block != null)
            {
                var childNodes = node.ChildNodes();
                if (childNodes.Count() != 1)
                {
                    return null;
                }


                return GetSingleElementFromBlockOrDefault(childNodes.Single());
            }

            return node;
        }
    }
}
