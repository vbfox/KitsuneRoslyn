using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class CanBeMadeStaticAnalysis
    {
        /// <summary>
        /// Return true if the method currently isn't static but can be made static.
        /// </summary>
        public static bool CanBeMadeStatic(this SemanticModel semanticModel, MethodDeclarationSyntax method,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ContainsNeverStaticModifiers(method.Modifiers))
            {
                return false;
            }

            if (method.ExplicitInterfaceSpecifier != null)
            {
                return false; // Same test is done later on the symbol
            }

            var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);
            if (symbol == null || symbol.IsStatic ||symbol.IsAbstract || symbol.IsOverride || symbol.IsVirtual)
            {
                return false;
            }
            
            if (symbol.ContainingType == null
                || !allowedTypeKinds.Contains(symbol.ContainingType.TypeKind))
            {
                return false;
            }

            if (symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
            {
                return false;
            }

            var type = symbol.ContainingType;
            var contentCanBeMadeStatic = CanBeMadeStatic(semanticModel, method, type);

            if (!contentCanBeMadeStatic)
            {
                return false;
            }

            var implementInterfaceSymbol = GetImplementedInterfacesSymbols(symbol).Any();
            return !implementInterfaceSymbol;
        }

        public static bool CanBeMadeStatic(this SemanticModel semanticModel, MethodDeclarationSyntax method,
            Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!CanBeMadeStatic(semanticModel, method, cancellationToken))
            {
                return false;
            }

            var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);

            var derivatedTypes = GetTypeDerivingFrom(compilation, symbol.ContainingType);

            bool isDerivedInterfaceImplementation = derivatedTypes
                .Any(d => GetImplementedInterfacesSymbols(symbol, d).Any());

            return !isDerivedInterfaceImplementation;
        }

        private static IEnumerable<INamedTypeSymbol> GetTypeDerivingFrom(Compilation compilation, ITypeSymbol type)
        {
            return EnumerateTypes(compilation).Where(t => DerivateFrom(t, type));
        }

        private static bool DerivateFrom(INamedTypeSymbol type, ITypeSymbol potentialBaseType)
        {
            return EnumerateBaseTypes(type).Contains(potentialBaseType);
        }

        private static IEnumerable<ITypeSymbol> EnumerateBaseTypes(ITypeSymbol type)
        {
            Parameter.MustNotBeNull(type, "type");

            var currentType = type;
            while (currentType.BaseType != null)
            {
                yield return currentType.BaseType;
                currentType = currentType.BaseType;
            }
        }

        private static IEnumerable<INamedTypeSymbol> EnumerateTypes(Compilation compilation)
        {
            Queue<INamespaceSymbol> namespacesToExplore = new Queue<INamespaceSymbol>();
            namespacesToExplore.Enqueue(compilation.GlobalNamespace);
            while(namespacesToExplore.Count != 0)
            {
                var ns = namespacesToExplore.Dequeue();
                foreach (var member in ns.GetMembers())
                {
                    if (member.Kind == SymbolKind.Namespace)
                    {
                        namespacesToExplore.Enqueue((INamespaceSymbol)member);
                    }
                    else if (member.Kind == SymbolKind.NamedType)
                    {
                        yield return (INamedTypeSymbol)member;
                    }
                }
            }
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

        private static bool CanBeMadeStatic(SemanticModel semanticModel, SyntaxNode node,
            INamedTypeSymbol type)
        {
            return !semanticModel.EnumerateSymbols(node)
                .Any(s => IsInstanceReference(type, s));
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

        static ImmutableHashSet<SymbolKind> potentialInstanceReferenceSymbolKinds
            = ImmutableHashSet.Create(SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.Property,
                /*this*/ SymbolKind.Parameter);

        static bool IsInstanceReference(INamedTypeSymbol type, ISymbol symbol)
        {
            return !symbol.IsStatic
                && potentialInstanceReferenceSymbolKinds.Contains(symbol.Kind)
                && symbol.ContainingType != null
                && symbol.ContainingType.Equals(type);
        }
    }
}
