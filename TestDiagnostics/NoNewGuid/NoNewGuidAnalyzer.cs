// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.NoNewGuid
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(Id, LanguageNames.CSharp)]
    public class NoNewGuidAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.NoNewGuid";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Don't use new Guid()",
                "Don't use new Guid() prefer Guid.Empty",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.ObjectCreationExpression);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            Parameter.MustNotBeNull(addDiagnostic, "addDiagnostic");
            var objectCreation = Parameter.MustBeOfType<ObjectCreationExpressionSyntax>(node, "node");

            var symbol = semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;
            
            // Non-Static constructor without parameter
            if (symbol == null
                || symbol.IsStatic
                || symbol.MethodKind != MethodKind.Constructor
                || symbol.Parameters.Length != 0)
            {
                return;
            }

            if (!symbol.ContainingType.FullyQualifiedNameIs("System", "Guid"))
            {
                return;
            }
            
            addDiagnostic(Diagnostic.Create(Descriptor, node.GetLocation()));
        }
    }
}
