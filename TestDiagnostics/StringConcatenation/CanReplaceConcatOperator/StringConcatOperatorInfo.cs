// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.TestDiagnostics.StringConcatenation.CanReplaceConcatOperator
{
    class StringConcatOperatorInfo
    {
        public static StringConcatOperatorInfo NoReplacement
        {
            get
            {
                return new StringConcatOperatorInfo(
                    StringConcatOperatorClassification.NoReplacement,
                    ImmutableList<ExpressionSyntax>.Empty);
            }
        }

        public StringConcatOperatorClassification Classification { get; private set; }
        public ImmutableList<ExpressionSyntax> Expressions { get; private set; }

        private StringConcatOperatorInfo(StringConcatOperatorClassification classification,
            ImmutableList<ExpressionSyntax> expressions)
        {
            Classification = classification;
            Expressions = expressions;
        }

        public static StringConcatOperatorInfo Create(BinaryExpressionSyntax binaryExpression,
            SemanticModel semanticModel)
        {
            if (semanticModel == null)
            {
                throw new ArgumentNullException("semanticModel");
            }

            if (!IsStringConcatenation(semanticModel, binaryExpression))
            {
                return NoReplacement;
            }

            var hasConcatenationAncestor = binaryExpression
                .Ancestors()
                .OfType<BinaryExpressionSyntax>()
                .Any(a => IsStringConcatenation(semanticModel, a));

            if (hasConcatenationAncestor)
            {
                return NoReplacement;
            }

            var expressions = ImmutableList<ExpressionSyntax>.Empty;
            VisitStringConcatenationExpression(semanticModel, binaryExpression,
                e => expressions = expressions.Add(e));

            if (StringCoalescing.IsCoalescable(expressions))
            {
                return new StringConcatOperatorInfo(StringConcatOperatorClassification.ReplaceWithSingleString,
                    expressions);
            }
            else
            {
                return new StringConcatOperatorInfo(StringConcatOperatorClassification.ReplaceWithStringFormat,
                    expressions);
            }
        }

        /// <summary>
        /// Visit a string concatenation in left to right order, calling onExpression for each
        /// non-string-concatenation expression found.
        /// </summary>
        private static void VisitStringConcatenationExpression(SemanticModel semanticModel,
            BinaryExpressionSyntax binaryExpression, Action<ExpressionSyntax> onExpression)
        {
            if (IsStringConcatenation(semanticModel, binaryExpression.Left))
            {
                VisitStringConcatenationExpression(semanticModel,
                    (BinaryExpressionSyntax)binaryExpression.Left, onExpression);
            }
            else
            {
                onExpression(binaryExpression.Left);
            }

            if (IsStringConcatenation(semanticModel, binaryExpression.Right))
            {
                VisitStringConcatenationExpression(semanticModel,
                    (BinaryExpressionSyntax)binaryExpression.Right, onExpression);
            }
            else
            {
                onExpression(binaryExpression.Right);
            }
        }

        private static bool IsStringConcatenation(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (expression.CSharpKind() != SyntaxKind.AddExpression)
            {
                return false;
            }

            var typeInfo = semanticModel.GetTypeInfo(expression);
            return typeInfo.Type.IsSystemString();
        }
    }
}
