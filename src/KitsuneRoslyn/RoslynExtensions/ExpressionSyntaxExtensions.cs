// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class ExpressionSyntaxExtensions
    {
        public static ParenthesizedExpressionSyntax WithParentheses(this ExpressionSyntax target)
        {
            Parameter.MustNotBeNull(target, "target");
            return ParenthesizedExpression(target);
        }
    }
}
