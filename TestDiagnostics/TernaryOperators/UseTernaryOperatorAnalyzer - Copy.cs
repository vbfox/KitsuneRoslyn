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

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.UseTernaryOperatorAnalyzer2", LanguageNames.CSharp)]
    public class UseTernaryOperatorAnalyzer2 : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string IdSimple = "BlackFox.UseTernaryOperator2.Simple";
        public const string IdComplex = "BlackFox.UseTernaryOperator2.Complex";

        public static DiagnosticDescriptor DescriptorSimple { get; }
            = new DiagnosticDescriptor(
                IdSimple,
                "X-Convert to return statement",
                "X-Convert to return statement",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorComplex { get; }
            = new DiagnosticDescriptor(
                IdComplex,
                "X-Convert to ':?' operator",
                "X-Convert to ':?' operator",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true,
                customTags: WellKnownDiagnosticTags.Unnecessary);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DescriptorSimple);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
        = ImmutableArray.Create(SyntaxKind.IfStatement);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)node;

            var potentialTernary = PotentialTernaryOperator2.Create(ifStatement);
            if (potentialTernary == PotentialTernaryOperator2.NoReplacement)
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(DescriptorSimple, ifStatement.IfKeyword.GetLocation()));
        }
    }
}
