// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
{
    static class MethodCanBeMadeStaticAnalysisExtensions
    {
        public static MethodCanBeMadeStaticAnalysis AnalyzeIfMethodCanBeMadeStatic(this SemanticModel semanticModel,
            MethodDeclarationSyntax method, CancellationToken cancellationToken = default(CancellationToken))
        {
            return MethodCanBeMadeStaticAnalysis.Create(semanticModel, method, cancellationToken);
        }

        public static MethodCanBeMadeStaticAnalysis AnalyzeIfMethodCanBeMadeStatic(this SemanticModel semanticModel,
            MethodDeclarationSyntax method, Compilation compilation,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return MethodCanBeMadeStaticAnalysis.Create(semanticModel, method, compilation, cancellationToken);
        }
    }
}
