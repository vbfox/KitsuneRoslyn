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

        public static DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            "Don't use new Guid()",
            "Don't use new Guid() prefer Guid.Empty",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create(Descriptor);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return supportedDiagnostics; } }

        static readonly ImmutableArray<SyntaxKind> syntaxKindsOfInterest
            = ImmutableArray.Create(SyntaxKind.ObjectCreationExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return syntaxKindsOfInterest; } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var objectCreation = node as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
            {
                throw new ArgumentException("Node isn't an instance of ObjectCreationExpressionSyntax", "node");
            }
            if (addDiagnostic == null)
            {
                throw new ArgumentNullException("addDiagnostic");
            }

            var symbol = semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;
            
            // Non-Static constructor without parameter
            if (symbol == null
                || symbol.IsStatic
                || symbol.MethodKind != MethodKind.Constructor
                || symbol.Parameters.Length != 0)
            {
                return;
            }

            if (!symbol.ContainingType.IsEqualTo("System", "Guid"))
            {
                return;
            }
            
            addDiagnostic(Diagnostic.Create(Descriptor, node.GetLocation()));
        }

    }
}
