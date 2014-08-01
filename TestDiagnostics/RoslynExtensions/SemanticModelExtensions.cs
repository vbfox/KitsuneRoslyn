using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class SemanticModelExtensions
    {
        public static IEnumerable<ISymbol> EnumerateSymbols(this SemanticModel semanticModel, SyntaxNode syntaxNode)
        {
            return syntaxNode.DescendantNodesAndSelf()
                .Select(n => semanticModel.GetSymbolInfo(n).Symbol)
                .Where(s => s != null);
        }
    }
}
