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
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.CanReplaceConcatOperator", LanguageNames.CSharp)]
    public class CanReplaceConcatOperatorAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string UseFormatId = "BlackFox.CanReplaceConcatOperator_UseFormat";
        public const string UseStringId = "BlackFox.CanReplaceConcatOperator_UseString";

        // "a" + b -> string.Format("a{0}, b)
        public static DiagnosticDescriptor UseFormatDescriptor = new DiagnosticDescriptor(
            UseFormatId,
            "string.Format can be used instead of concatenation",
            "string.Format can be used instead of concatenation",
            "Readability",
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true);

        // "a" + "b" -> "ab"
        public static DiagnosticDescriptor UseStringDescriptor = new DiagnosticDescriptor(
            UseStringId,
            "A single string can be used instead of concatenation",
            "A single string can be used instead of concatenation",
            "Readability",
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true);

        static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics
            = ImmutableArray.Create(UseFormatDescriptor, UseStringDescriptor);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return supportedDiagnostics; } }

        static readonly ImmutableArray<SyntaxKind> syntaxKindsOfInterest
            = ImmutableArray.Create(SyntaxKind.AddExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return syntaxKindsOfInterest; } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var binaryExpression = node as BinaryExpressionSyntax;
            if (binaryExpression == null)
            {
                throw new ArgumentException("Node isn't an instance of BinaryExpressionSyntax", "node");
            }
            if (addDiagnostic == null)
            {
                throw new ArgumentNullException("addDiagnostic");
            }

            var infos = StringConcatOperatorInfo.Create(binaryExpression, semanticModel);

            switch (infos.Classification)
            {
                case StringConcatOperatorClassification.ReplaceWithSingleString:
                    addDiagnostic(Diagnostic.Create(UseStringDescriptor, node.GetLocation()));
                    break;

                case StringConcatOperatorClassification.ReplaceWithStringFormat:
                    addDiagnostic(Diagnostic.Create(UseFormatDescriptor, node.GetLocation()));
                    break;
            }
        }
    }
}
