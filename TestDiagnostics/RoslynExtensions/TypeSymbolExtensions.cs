// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class TypeSymbolExtensions
    {
        public static bool IsEqualTo(this ITypeSymbol type, params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            Contract.EndContractBlock();

            if (type == null)
            {
                return false;
            }

            var expectedTypeName = names.Last();
            var expectedNamespaces = names.Reverse().Skip(1).ToArray();

            if (type.Name != expectedTypeName)
            {
                return false;
            }

            var expectedNamespaceIndex = 0;
            var typeNamespace = type.ContainingNamespace;
            while (!typeNamespace.IsGlobalNamespace)
            {
                if (expectedNamespaceIndex > expectedNamespaces.Length - 1)
                {
                    return false;
                }
                var expectedNamespace = expectedNamespaces[expectedNamespaceIndex];
                if (expectedNamespace == null)
                {
                    return false;
                }

                if (typeNamespace.Name != expectedNamespace)
                {
                    return false;
                }

                expectedNamespaceIndex += 1;
                typeNamespace = typeNamespace.ContainingNamespace;
            }

            return expectedNamespaceIndex == expectedNamespaces.Length;
        }

        public static bool IsSystemString(this ITypeSymbol symbol)
        {
            return symbol != null && symbol.SpecialType == SpecialType.System_String;
        }

        public static bool IsSystemChar(this ITypeSymbol symbol)
        {
            return symbol != null && symbol.SpecialType == SpecialType.System_Char;
        }

        public static bool IsSystemObject(this ITypeSymbol symbol)
        {
            return symbol != null && symbol.SpecialType == SpecialType.System_Object;
        }

        public static bool IsArrayOfSystemString(this ITypeSymbol symbol)
        {
            var arraySymbol = symbol as IArrayTypeSymbol;

            return arraySymbol != null
                && arraySymbol.ElementType.SpecialType == SpecialType.System_String;
        }

        public static bool IsArrayOfSystemObject(this ITypeSymbol symbol)
        {
            var arraySymbol = symbol as IArrayTypeSymbol;

            return arraySymbol != null
                && arraySymbol.ElementType.SpecialType == SpecialType.System_Object;
        }
    }
}
