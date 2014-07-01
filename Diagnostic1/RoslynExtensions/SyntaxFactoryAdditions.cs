using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Simplification;

namespace BlackFox.Roslyn.TestDiagnostics.RoslynExtensions
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
                    // global::XXX
                    result = AliasQualifiedName((IdentifierNameSyntax)result, name);
                }
                else
                {
                    // XXX.YYY
                    result = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, result, name);
                }
            }

            // Simplify to remove global:: and namespace declarations when not necessary
            return result.WithAdditionalAnnotations(Simplifier.Annotation);
        }

        public static LiteralExpressionSyntax StringLiteralExpression(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
        }
    }
}
