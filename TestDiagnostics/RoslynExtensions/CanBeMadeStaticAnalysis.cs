using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{

    class Foo : IDisposable
    {
        public void Dispose()
        {
        }
    }
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
            return CanBeMadeStatic(semanticModel, method, type);
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
