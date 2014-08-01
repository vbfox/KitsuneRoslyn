using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var symbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);
            if (symbol == null || symbol.IsStatic)
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

        static ImmutableHashSet<SymbolKind> potentialInstanceReferenceSymbolKinds
            = ImmutableHashSet.Create(SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.Property);

        static bool IsInstanceReference(INamedTypeSymbol type, ISymbol symbol)
        {
            return !symbol.IsStatic
                && potentialInstanceReferenceSymbolKinds.Contains(symbol.Kind)
                && symbol.ContainingType != null
                && symbol.ContainingType.Equals(type);
        }
    }
}
