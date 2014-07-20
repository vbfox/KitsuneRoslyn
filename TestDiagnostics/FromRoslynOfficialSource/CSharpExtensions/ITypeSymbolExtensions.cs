// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Microsoft.CodeAnalysis.CSharp.Extensions
{
    internal static partial class ITypeSymbolExtensions
    {
        public static TypeSyntax GenerateTypeSyntax(
            this ITypeSymbol typeSymbol)
        {
            return typeSymbol.Accept(TypeSyntaxGeneratorVisitor.Instance).WithAdditionalAnnotations(Simplifier.Annotation);
        }
    }
}