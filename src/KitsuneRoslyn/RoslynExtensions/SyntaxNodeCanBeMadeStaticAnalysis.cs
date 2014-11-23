// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class SyntaxNodeCanBeMadeStaticAnalysis
    {
        public static bool CanBeMadeStatic(this SemanticModel semanticModel, SyntaxNode node,
            INamedTypeSymbol type)
        {
            return !semanticModel.EnumerateSymbols(node)
                .Distinct()
                .Any(s => IsInstanceReference(type, s));
        }

        static ImmutableHashSet<SymbolKind> potentialInstanceReferenceSymbolKinds
            = ImmutableHashSet.Create(SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.Property);

        static bool IsInstanceReference(INamedTypeSymbol type, ISymbol symbol)
        {
            return !symbol.IsStatic
                && (potentialInstanceReferenceSymbolKinds.Contains(symbol.Kind) || IsThis(symbol))
                && symbol.ContainingType != null
                && symbol.ContainingType.Equals(type);
        }

        static bool IsThis(ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Parameter
                && symbol.Name == "this"
                && symbol.IsImplicitlyDeclared;
        }
    }
}
