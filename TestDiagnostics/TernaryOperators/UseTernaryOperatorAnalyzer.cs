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
    public class UseTernaryOperatorAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string IdSimple = "BlackFox.UseTernaryOperator2.Simple";
        public const string IdComplex = "BlackFox.UseTernaryOperator2.Complex";

        public static DiagnosticDescriptor DescriptorSimple { get; }
            = new DiagnosticDescriptor(
                IdSimple,
                "'if' can be converted to ':?' operator",
                "'if' can be converted to ':?' operator",
                "Readability",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorComplex { get; }
            = new DiagnosticDescriptor(
                IdComplex,
                "'if' can be converted to ':?' operator",
                "'if' can be converted to {0} usage{1} of ':?' operator",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DescriptorSimple);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
        = ImmutableArray.Create(SyntaxKind.IfStatement);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)node;

            var potentialTernary = PotentialTernaryOperator.Create(ifStatement);
            if (potentialTernary == PotentialTernaryOperator.NoReplacement)
            {
                return;
            }

            var descriptor = potentialTernary.TernaryOperatorCount == 1 && potentialTernary.Replacements.Count == 1
                ? DescriptorSimple
                : DescriptorComplex;

            addDiagnostic(Diagnostic.Create(descriptor, ifStatement.IfKeyword.GetLocation(),
                potentialTernary.TernaryOperatorCount,
                potentialTernary.TernaryOperatorCount > 1 ? "s" : ""));
        }
    }
}
