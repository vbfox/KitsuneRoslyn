// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    public static class SyntaxFactoryAdditions
    {
        public static ExpressionSyntax SimpleMemberAccessExpression(params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

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

            // Ask Simplify to remove global:: and namespace declarations when not necessary
            return result.WithAdditionalAnnotations(Simplifier.Annotation);
        }

        public static LiteralExpressionSyntax StringLiteralExpression(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
        }

        public static LiteralExpressionSyntax BooleanLiteralExpression(bool value)
        {
            return LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        public static ArgumentListSyntax ArgumentList(IEnumerable<ExpressionSyntax> expressions)
        {
            Parameter.MustNotBeNull(expressions, "expressions");

            var arguments = expressions.Select(e => Argument(e));

            var argumentsSyntaxList = new SeparatedSyntaxList<ArgumentSyntax>().AddRange(arguments);

            return SyntaxFactory.ArgumentList(argumentsSyntaxList);
        }
    }
}
