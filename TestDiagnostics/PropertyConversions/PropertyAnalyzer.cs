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

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.PropertyAnalyzer", LanguageNames.CSharp)]
    public class PropertyAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string IdExpressionToStatement = "BlackFox.Property.ExpressionCanBeConvertedToStatement";
        public const string IdExpressionToInitializer = "BlackFox.Property.ExpressionCanBeConvertedToInitializer";
        public const string IdStatementToInitializer = "BlackFox.Property.StatementCanBeConvertedToInitializer";
        public const string IdStatementToExpression = "BlackFox.Property.StatementCanBeConvertedToExpression";
        public const string IdInitializerToStatement = "BlackFox.Property.InitializerCanBeConvertedToStatement";
        public const string IdInitializerToExpression = "BlackFox.Property.InitializerCanBeConvertedToExpression";

        public static DiagnosticDescriptor DescriptorExpressionToStatement { get; }
            = new DiagnosticDescriptor(
                IdExpressionToStatement,
                "Can be converted to statement body",
                "Can be converted to statement body",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorInitializerToStatement { get; }
            = new DiagnosticDescriptor(
                IdInitializerToStatement,
                "Can be converted to statement body",
                "Can be converted to statement body",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorExpressionToInitializer { get; }
            = new DiagnosticDescriptor(
                    IdExpressionToInitializer,
                    "Can be converted to initializer",
                    "Can be converted to initializer",
                    "Readability",
                    DiagnosticSeverity.Hidden,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorStatementToInitializer { get; }
            = new DiagnosticDescriptor(
                    IdStatementToInitializer,
                    "Can be converted to initializer",
                    "Can be converted to initializer",
                    "Readability",
                    DiagnosticSeverity.Hidden,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorStatementToExpression { get; }
            = new DiagnosticDescriptor(
                    IdStatementToExpression,
                    "Can be converted to expression",
                    "Can be converted to expression",
                    "Readability",
                    DiagnosticSeverity.Hidden,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorInitializerToExpression { get; }
            = new DiagnosticDescriptor(
                    IdInitializerToExpression,
                    "Can be converted to expression",
                    "Can be converted to expression",
                    "Readability",
                    DiagnosticSeverity.Hidden,
                    isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                DescriptorExpressionToStatement,
                DescriptorInitializerToStatement,
                DescriptorExpressionToInitializer,
                DescriptorStatementToInitializer,
                DescriptorStatementToExpression,
                DescriptorInitializerToExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.PropertyDeclaration);

        private static readonly ImmutableDictionary<Tuple<PropertyConversionClassification, PropertyConversionClassification>, DiagnosticDescriptor> descriptors
            = ImmutableDictionary<Tuple<PropertyConversionClassification, PropertyConversionClassification>, DiagnosticDescriptor>.Empty
            .Add(Tuple.Create(PropertyConversionClassification.Initializer, PropertyConversionClassification.GetWithReturn), DescriptorInitializerToStatement)
            .Add(Tuple.Create(PropertyConversionClassification.Expression, PropertyConversionClassification.GetWithReturn), DescriptorExpressionToStatement)
            .Add(Tuple.Create(PropertyConversionClassification.Expression, PropertyConversionClassification.Initializer), DescriptorExpressionToInitializer)
            .Add(Tuple.Create(PropertyConversionClassification.Initializer, PropertyConversionClassification.Expression), DescriptorInitializerToExpression)
            .Add(Tuple.Create(PropertyConversionClassification.GetWithReturn, PropertyConversionClassification.Expression), DescriptorStatementToExpression)
            .Add(Tuple.Create(PropertyConversionClassification.GetWithReturn, PropertyConversionClassification.Initializer), DescriptorStatementToInitializer);


        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var property = (PropertyDeclarationSyntax)node;

            var analysis = PropertyConversionAnalysis.Create(semanticModel, property, cancellationToken);
            if (analysis.Classification == PropertyConversionClassification.NotSupported)
            {
                return;
            }

            AddIfPossible(analysis, PropertyConversionClassification.Expression, addDiagnostic);
            AddIfPossible(analysis, PropertyConversionClassification.Initializer, addDiagnostic);
            AddIfPossible(analysis, PropertyConversionClassification.GetWithReturn, addDiagnostic);
        }

        private void AddIfPossible(PropertyConversionAnalysis analysis,
            PropertyConversionClassification to, Action<Diagnostic> addDiagnostic)
        {
            if (to == PropertyConversionClassification.Expression && !analysis.CanBeConvertedToExpression)
            {
                return;
            }
            if (to == PropertyConversionClassification.Initializer && !analysis.CanBeConvertedToInitializer)
            {
                return;
            }
            if (to == PropertyConversionClassification.GetWithReturn && !analysis.CanBeConvertedToGetWithReturn)
            {
                return;
            }

            var descriptor = descriptors[Tuple.Create(analysis.Classification, to)];

            addDiagnostic(Diagnostic.Create(descriptor, analysis.Property.GetLocation()));
        }
    }
}
