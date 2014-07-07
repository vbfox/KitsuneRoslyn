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
        public static bool IsCoalescable(this IEnumerable<ExpressionSyntax> expressions,
            SemanticModel semanticModel)
        {
            return expressions.All(expression => IsLiteralStringOrSimilar(semanticModel, expression));
        }

        static bool IsLiteralStringOrSimilar(SemanticModel semanticModel, ExpressionSyntax expression)
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
                    case SyntaxKind.CharacterLiteralExpression:
                        return (string)semanticModel.GetConstantValue(expression).Value;

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
