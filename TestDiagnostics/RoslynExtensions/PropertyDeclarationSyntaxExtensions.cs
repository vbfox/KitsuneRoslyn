// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Generic;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    public static class PropertyDeclarationSyntaxExtensions
    {
        public static PropertyDeclarationSyntax WithAccessor(this PropertyDeclarationSyntax property, SyntaxKind accessorKind,
            BlockSyntax block)
        {
            var otherAccessors = property.AccessorList != null
                ? property.AccessorList.Accessors.Where(a => !a.IsKind(accessorKind))
                : Enumerable.Empty<AccessorDeclarationSyntax>();

            var newAccessor = AccessorDeclaration(accessorKind, block);

            var accessors = otherAccessors.Concat(new[] { newAccessor });

            return property.WithAccessorList(AccessorList(List(accessors)));
        }

        public static PropertyDeclarationSyntax WithGet(this PropertyDeclarationSyntax property, BlockSyntax block)
        {
            return WithAccessor(property, SyntaxKind.GetAccessorDeclaration, block);
        }

        public static PropertyDeclarationSyntax WithGet(this PropertyDeclarationSyntax property,
            ExpressionSyntax getResult)
        {
            var block = Block(SingletonList<StatementSyntax>(ReturnStatement(getResult)));
            return WithGet(property, block);
        }
    }
}
