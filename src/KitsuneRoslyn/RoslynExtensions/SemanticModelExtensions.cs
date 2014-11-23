// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

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
