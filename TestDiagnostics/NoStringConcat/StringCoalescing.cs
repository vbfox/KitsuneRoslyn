// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringConcat
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

        public static string Coalesce(this IEnumerable<ExpressionSyntax> expressions,
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
                        throw new ArgumentException("Unexpected literal kind: " + expression.CSharpKind(), "expression");
                }
            });

            return string.Concat(strings);
        }
    }
}
