using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Simplification;

namespace BlackFox.Roslyn.TestDiagnostics
{
    static class SyntaxFactoryAdditions
    {
        public static ExpressionSyntax SimpleMemberAccessExpression(params string[] names)
        {
            ExpressionSyntax result = IdentifierName(Token(SyntaxKind.GlobalKeyword));
            for (int i = 0; i < names.Length; i++)
            {
                var name = IdentifierName(names[i]);
                if (i == 0)
                {
                    result = AliasQualifiedName((IdentifierNameSyntax)result, name);
                }
                else
                {
                    result = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, result, name);
                }
            }

            result = result.WithAdditionalAnnotations(Simplifier.Annotation);
            return result;
        }

        public static LiteralExpressionSyntax StringLiteralExpression(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
        }
    }
}
