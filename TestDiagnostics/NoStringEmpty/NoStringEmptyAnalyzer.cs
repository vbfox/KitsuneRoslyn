// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.NoStringEmpty
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(Id, LanguageNames.CSharp)]
    public class NoStringEmptyAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.NoStringEmpty";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Don't use String.Empty",
                "String.Empty should be replaced by \"\"",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.SimpleMemberAccessExpression);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            Parameter.MustNotBeNull(addDiagnostic, "addDiagnostic");
            var memberAccess = Parameter.MustBeOfType<MemberAccessExpressionSyntax>(node, "node");

            var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol == null
                || symbol.ContainingType == null
                || symbol.ContainingType.SpecialType != SpecialType.System_String
                || !symbol.IsStatic
                || symbol.Kind != SymbolKind.Field
                || symbol.Name != "Empty"
                )
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(Descriptor, node.GetLocation()));
        }
    }
}
