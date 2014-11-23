// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseTernaryOperatorAnalyzer : DiagnosticAnalyzer
    {
        public const string IdSimple = "BlackFox.UseTernaryOperator.Simple";
        public const string IdComplex = "BlackFox.UseTernaryOperator.Complex";

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DescriptorSimple, DescriptorComplex);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.IfStatement);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            var potentialTernary = PotentialTernaryOperator.Create(ifStatement);
            if (potentialTernary == PotentialTernaryOperator.NoReplacement)
            {
                return;
            }

            var descriptor = potentialTernary.TernaryOperatorCount == 1 && potentialTernary.Replacements.Count == 1
                ? DescriptorSimple
                : DescriptorComplex;

            context.ReportDiagnostic(Diagnostic.Create(descriptor, ifStatement.IfKeyword.GetLocation(),
                potentialTernary.TernaryOperatorCount,
                potentialTernary.TernaryOperatorCount > 1 ? "s" : ""));
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IfStatement);
        }
    }
}
