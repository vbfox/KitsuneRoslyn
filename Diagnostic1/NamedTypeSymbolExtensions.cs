using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace BlackFox.Roslyn.TestDiagnostics
{
    static class NamedTypeSymbolExtensions
    {
        public static bool IsEqualTo(this INamedTypeSymbol type, params string[] names)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            Contract.EndContractBlock();

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
    }
}
