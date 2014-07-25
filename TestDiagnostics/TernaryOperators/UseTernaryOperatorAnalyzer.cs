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
    [ExportDiagnosticAnalyzer("BlackFox.UseTernaryOperatorAnalyzer", LanguageNames.CSharp)]
    public class UseTernaryOperatorAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string IdSimple = "BlackFox.UseTernaryOperator.Simple";
        public const string IdComplex = "BlackFox.UseTernaryOperator.Complex";

        public static DiagnosticDescriptor DescriptorSimple { get; }
            = new DiagnosticDescriptor(
                IdSimple,
                "Convert to return statement",
                "Convert to return statement",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorComplex { get; }
            = new DiagnosticDescriptor(
                IdComplex,
                "Convert to ':?' operator",
                "Convert to ':?' operator",
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

            var potentialTernary = PotentialTernaryOperator.Create(ifStatement);
            if (potentialTernary == PotentialTernaryOperator.NoReplacement)
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(DescriptorSimple, ifStatement.IfKeyword.GetLocation()));
        }
    }
}
