using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class CompilationExtensions
    {
        /// <summary>
        /// Enumerate all types present in a <see cref="Compilation"/>
        /// </summary>
        public static IEnumerable<INamedTypeSymbol> GetTypes(this Compilation compilation)
        {
            Queue<INamespaceSymbol> namespacesToExplore = new Queue<INamespaceSymbol>();
            namespacesToExplore.Enqueue(compilation.GlobalNamespace);
            while (namespacesToExplore.Count != 0)
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

        /// <summary>
        /// Enumerate all types deriving from <paramref name="type"/> in <paramref name="compilation"/>
        /// </summary>
        public static IEnumerable<INamedTypeSymbol> GetTypesDerivingFrom(this Compilation compilation, ITypeSymbol type)
        {
            return compilation.GetTypes().Where(t => t.DerivateFrom(type));
        }
    }
}
