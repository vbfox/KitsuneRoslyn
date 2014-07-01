using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringConcat
{
    static class StringCoalescing
    {
        public static bool CanBeTransformedToSingleString(SemanticModel semanticModel, IEnumerable<ExpressionSyntax> expressions)
        {
            return expressions.All(expression => IsLiteralStringOrSimilar(semanticModel, expression));
        }

        static bool IsLiteralStringOrSimilar(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            var literal = expression as LiteralExpressionSyntax;

            return (literal != null) &&
                (
                typeInfo.Type.IsSystemString()
                || typeInfo.Type.IsSystemChar()
                || literal.CSharpKind() == SyntaxKind.NullLiteralExpression
                );
        }
    }
}
