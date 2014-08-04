// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    class MethodCanBeMadeStaticAnalysis
    {
        public bool CanBeMadeStatic { get; private set; }

        public MethodDeclarationSyntax Method { get; private set; }

        private MethodCanBeMadeStaticAnalysis(bool canBeMadeStatic, MethodDeclarationSyntax method)
        {
            CanBeMadeStatic = canBeMadeStatic;
            Method = method;
        }

        private static MethodCanBeMadeStaticAnalysis True(MethodDeclarationSyntax method)
        {
            return new MethodCanBeMadeStaticAnalysis(true, method);
        }

        private static MethodCanBeMadeStaticAnalysis False { get; }
            = new MethodCanBeMadeStaticAnalysis(false, null);

        public MethodDeclarationSyntax GetFixedMethod()
        {
            if (!CanBeMadeStatic)
            {
                throw new InvalidOperationException("The method cannot be made static");
            }
            return GetFixedMethod(Method);
        }

        public static MethodDeclarationSyntax GetFixedMethod(MethodDeclarationSyntax method)
        {
            var token = SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                .WithAdditionalAnnotations(Formatter.Annotation);

            return method.AddModifiers(token);
        }

        /// <summary>
        /// Return true if the method currently isn't static but can be made static.
        /// </summary>
        public static MethodCanBeMadeStaticAnalysis Create(SemanticModel semanticModel, MethodDeclarationSyntax method,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ContainsNeverStaticModifiers(method.Modifiers))
            {
                return False;
            }

            if (method.ExplicitInterfaceSpecifier != null)
            {
                return False; // Same test is done later on the symbol
            }

            var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);
            if (symbol == null || symbol.IsStatic || symbol.IsAbstract || symbol.IsOverride || symbol.IsVirtual)
            {
                return False;
            }

            if (symbol.ContainingType == null
                || !allowedTypeKinds.Contains(symbol.ContainingType.TypeKind))
            {
                return False;
            }

            if (symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
            {
                return False;
            }

            var type = symbol.ContainingType;
            var contentCanBeMadeStatic = semanticModel.CanBeMadeStatic(method, type);

            if (!contentCanBeMadeStatic)
            {
                return False;
            }

            var implementInterfaceSymbol = GetImplementedInterfacesSymbols(symbol).Any();
            return implementInterfaceSymbol ? False : True(method);
        }

        public static MethodCanBeMadeStaticAnalysis Create(SemanticModel semanticModel, MethodDeclarationSyntax method,
            Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
        {
            var resultWithoutCompilation = Create(semanticModel, method, cancellationToken);
            if (!resultWithoutCompilation.CanBeMadeStatic)
            {
                return resultWithoutCompilation;
            }

            var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);

            var derivatedTypes = compilation.GetTypesDerivingFrom(symbol.ContainingType);

            bool isDerivedInterfaceImplementation = derivatedTypes
                .Any(d => GetImplementedInterfacesSymbols(symbol, d).Any());

            return isDerivedInterfaceImplementation ? False : True(method);
        }

        static ImmutableHashSet<TypeKind> allowedTypeKinds
            = ImmutableHashSet.Create(TypeKind.Class, TypeKind.Struct);

        static ImmutableHashSet<SyntaxKind> neverStaticModifiers
            = ImmutableHashSet.Create(SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword,
                SyntaxKind.AbstractKeyword);

        static bool ContainsNeverStaticModifiers(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => neverStaticModifiers.Contains(m.CSharpKind()));
        }

        private static ImmutableList<ISymbol> GetImplementedInterfacesSymbols(ISymbol symbol)
        {
            return GetImplementedInterfacesSymbols(symbol, symbol.ContainingType);
        }

        private static ImmutableList<ISymbol> GetImplementedInterfacesSymbols(ISymbol symbol,
            ITypeSymbol inType)
        {
            var interfaceMembers = inType.AllInterfaces
                .SelectMany(i => i.GetMembers());

            var builder = ImmutableList<ISymbol>.Empty.ToBuilder();
            foreach (var interfaceMember in interfaceMembers)
            {
                var impl = inType.FindImplementationForInterfaceMember(interfaceMember);
                if (impl != null && symbol.Equals(impl))
                {
                    builder.Add(impl);
                }
            }

            return builder.ToImmutable();
        }
    }
}
