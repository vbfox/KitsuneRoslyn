// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class TypeSymbolExtensions
    {
        public static bool IsSystemString(this ITypeSymbol symbol)
        {
            return symbol?.SpecialType == SpecialType.System_String;
        }

        public static bool IsSystemObject(this ITypeSymbol symbol)
        {
            return symbol?.SpecialType == SpecialType.System_Object;
        }

        public static bool IsArrayOfSystemString(this ITypeSymbol symbol)
        {
            var arraySymbol = symbol as IArrayTypeSymbol;

            return arraySymbol?.ElementType.SpecialType == SpecialType.System_String;
        }

        public static bool IsArrayOfSystemObject(this ITypeSymbol symbol)
        {
            var arraySymbol = symbol as IArrayTypeSymbol;

            return arraySymbol?.ElementType.SpecialType == SpecialType.System_Object;
        }

        public static bool DerivateFrom(this INamedTypeSymbol type, ITypeSymbol potentialBaseType)
        {
            return type.BaseTypes().Contains(potentialBaseType);
        }

        public static IEnumerable<ITypeSymbol> BaseTypes(this ITypeSymbol type)
        {
            Parameter.MustNotBeNull(type, "type");

            var currentType = type;
            while (currentType.BaseType != null)
            {
                yield return currentType.BaseType;
                currentType = currentType.BaseType;
            }
        }
    }
}
