// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation
{
    static class StringCoalescing
    {
        public static bool IsCoalescable(this IEnumerable<ExpressionSyntax> expressions)
        {
            return expressions.All(expression => IsLiteralStringOrSimilar(expression));
        }

        public static bool IsLiteralStringOrSimilar(ExpressionSyntax expression)
        {
            var kind = expression.CSharpKind();
            return kind == SyntaxKind.StringLiteralExpression
                || kind == SyntaxKind.CharacterLiteralExpression
                || kind == SyntaxKind.NullLiteralExpression;
        }

        public static string Coalesce(IEnumerable<ExpressionSyntax> expressions,
            SemanticModel semanticModel)
        {
            IEnumerable<string> strings = expressions.Select(expression =>
            {
                switch (expression.CSharpKind())
                {
                    case SyntaxKind.StringLiteralExpression:
                        return (string)semanticModel.GetConstantValue(expression).Value;

                    case SyntaxKind.CharacterLiteralExpression:
                        return ((char)semanticModel.GetConstantValue(expression).Value).ToString();

                    case SyntaxKind.NullLiteralExpression:
                        return "";

                    default:
                        var message = string.Format(
                            "Unexpected kind: {0} expected string, character or null literal",
                            expression.CSharpKind());
                        throw new ArgumentException(message, "expression");
                }
            });

            return string.Concat(strings);
        }

        static readonly ExpressionSyntax stringFormatAccess
            = SimpleMemberAccessExpression("System", "String", "Format");

        public static InvocationExpressionSyntax ToStringFormat(ImmutableList<ExpressionSyntax> expressions,
            SemanticModel semanticModel)
        {
            string formatString;
            ImmutableQueue<ExpressionSyntax> otherArguments;
            BuildArguments(semanticModel, expressions, out formatString, out otherArguments);

            var arguments = new[] { StringLiteralExpression(formatString) }
                .Concat(otherArguments);

            return InvocationExpression(stringFormatAccess, ArgumentList(arguments));
        }

        private static void BuildArguments(SemanticModel semanticModel, IEnumerable<ExpressionSyntax> expressions,
            out string formatString, out ImmutableQueue<ExpressionSyntax> otherArguments)
        {
            formatString = "";
            var remainingExpressions = expressions;
            int currentReplacementIndex = 0;
            otherArguments = ImmutableQueue<ExpressionSyntax>.Empty;
            while (remainingExpressions.Any())
            {
                int takenAsSingleString;
                var longestSingleString = GetLongestSingleString(remainingExpressions, semanticModel,
                    out takenAsSingleString);
                if (takenAsSingleString != 0)
                {
                    formatString += longestSingleString;
                    remainingExpressions = remainingExpressions.Skip(takenAsSingleString);
                }
                else
                {
                    formatString += "{" + currentReplacementIndex + "}";
                    currentReplacementIndex += 1;
                    var expression = remainingExpressions.First();
                    otherArguments = otherArguments.Enqueue(expression);
                    remainingExpressions = remainingExpressions.Skip(1);
                }
            }
        }

        static string GetLongestSingleString(IEnumerable<ExpressionSyntax> expressions, SemanticModel semanticModel,
            out int taken)
        {
            var coalescable = expressions.TakeWhile(IsLiteralStringOrSimilar).ToImmutableArray();

            taken = coalescable.Length;
            return Coalesce(coalescable, semanticModel);
        }
    }
}
