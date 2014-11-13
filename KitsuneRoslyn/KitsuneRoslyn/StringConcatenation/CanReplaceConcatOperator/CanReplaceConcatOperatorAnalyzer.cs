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

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.CanReplaceConcatOperator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CanReplaceConcatOperatorAnalyzer : DiagnosticAnalyzer
    {
        public const string UseFormatId = "BlackFox.CanReplaceConcatOperator_UseFormat";
        public const string UseStringId = "BlackFox.CanReplaceConcatOperator_UseString";

        // "a" + b -> string.Format("a{0}, b)
        public static DiagnosticDescriptor UseFormatDescriptor { get; }
            = new DiagnosticDescriptor(
                UseFormatId,
                "string.Format can be used instead of concatenation",
                "string.Format can be used instead of concatenation",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        // "a" + "b" -> "ab"
        public static DiagnosticDescriptor UseStringDescriptor { get; }
            = new DiagnosticDescriptor(
                UseStringId,
                "A single string can be used instead of concatenation",
                "A single string can be used instead of concatenation",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(UseFormatDescriptor, UseStringDescriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.AddExpression);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = Parameter.MustBeOfType<BinaryExpressionSyntax>(context.Node, "node");

            var infos = StringConcatOperatorInfo.Create(binaryExpression, context.SemanticModel);

            switch (infos.Classification)
            {
                case StringConcatOperatorClassification.ReplaceWithSingleString:
                    context.ReportDiagnostic(Diagnostic.Create(UseStringDescriptor, context.Node.GetLocation()));
                    break;

                case StringConcatOperatorClassification.ReplaceWithStringFormat:
                    context.ReportDiagnostic(Diagnostic.Create(UseFormatDescriptor, context.Node.GetLocation()));
                    break;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AddExpression);
        }
    }
}
