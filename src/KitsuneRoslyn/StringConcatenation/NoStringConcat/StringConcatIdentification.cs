// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions.TypeSymbolExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.NoStringConcat
{
    static class StringConcatIdentification
    {
        public static bool IsDirectArrayOverloadCall(SemanticModel semanticModel, InvocationExpressionSyntax invocation,
            IMethodSymbol symbol)
        {
            if (invocation.ArgumentList.Arguments.Count != 1)
            {
                return false;
            }

            var argumentType = semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression);

            return (argumentType.ConvertedType.IsArrayOfSystemObject() && objectArrayOverload.IsOverload(symbol))
                || (argumentType.ConvertedType.IsArrayOfSystemString() && stringArrayOverload.IsOverload(symbol));
        }

        static readonly OverloadDefinition stringArrayOverload = new OverloadDefinition(IsArrayOfSystemString);
        static readonly OverloadDefinition objectArrayOverload = new OverloadDefinition(IsArrayOfSystemObject);

        static readonly ImmutableArray<OverloadDefinition> concernedOverloads = ImmutableArray.Create(
            new OverloadDefinition(IsSystemObject),
            stringArrayOverload,
            objectArrayOverload,
            new OverloadDefinition(IsSystemObject, IsSystemObject),
            new OverloadDefinition(IsSystemString, IsSystemString),
            new OverloadDefinition(IsSystemObject, IsSystemObject, IsSystemObject),
            new OverloadDefinition(IsSystemString, IsSystemString, IsSystemString),
            new OverloadDefinition(IsSystemObject, IsSystemObject, IsSystemObject, IsSystemObject),
            new OverloadDefinition(IsSystemString, IsSystemString, IsSystemString, IsSystemString)
            );

        public static bool IsConcernedOverload(IMethodSymbol symbol)
        {
            return concernedOverloads.Any(overload => overload.IsOverload(symbol));
        }

        public static bool IsNonGenericStringConcat(IMethodSymbol symbol)
        {
            return symbol != null
                && symbol.ContainingType.SpecialType == SpecialType.System_String
                && symbol.Name == "Concat"
                && symbol.IsStatic
                && !symbol.IsGenericMethod // Ignore the overload taking IEnumerable<T>
                && symbol.MethodKind == MethodKind.Ordinary;
        }

        public static bool CouldBeStringConcatFast(InvocationExpressionSyntax invocation)
        {
            var memberAccessSyntax = invocation.Expression as MemberAccessExpressionSyntax;

            return memberAccessSyntax?.Name.Identifier.Text == "Concat";
        }
    }
}
