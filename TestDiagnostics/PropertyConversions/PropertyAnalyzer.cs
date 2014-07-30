// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.PropertyAnalyzer", LanguageNames.CSharp)]
    public class PropertyAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string IdToStatement = "BlackFox.Property.CanBeConvertedToStatement";
        public const string IdToInitializer = "BlackFox.Property.CanBeConvertedToInitializer";
        public const string IdToExpression = "BlackFox.Property.CanBeConvertedToExpression";

        public static DiagnosticDescriptor DescriptorToStatement { get; }
            = new DiagnosticDescriptor(
                IdToStatement,
                "Can be converted to statement body",
                "Can be converted to statement body",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorToInitializer { get; }
            = new DiagnosticDescriptor(
                    IdToInitializer,
                    "Can be converted to initializer",
                    "Can be converted to initializer",
                    "Readability",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorToExpression { get; }
        = new DiagnosticDescriptor(
                    IdToExpression,
                    "Can be converted to expression",
                    "Can be converted to expression",
                    "Readability",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DescriptorToStatement, DescriptorToInitializer, DescriptorToExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.PropertyDeclaration);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var property = (PropertyDeclarationSyntax)node;

            var location = property.GetLocation();

            if (property.Initializer != null)
            {
                //var location = property.Initializer.EqualsToken.GetLocation();
                addDiagnostic(Diagnostic.Create(DescriptorToExpression, location));
                addDiagnostic(Diagnostic.Create(DescriptorToStatement, location));
            }

            if (property.ExpressionBody != null)
            {
                //var location = property.ExpressionBody.ArrowToken.GetLocation();

                //FIXME: Shouldn't be possible in case of primary constructor variable capture
                // semanticModel.AnalyzeControlFlow should be able to give the info
                addDiagnostic(Diagnostic.Create(DescriptorToStatement, location));
                
                if (semanticModel.GetConstantValue(property.ExpressionBody).HasValue)
                {
                    // All conversions should be legal but the semantic isn't the same when the expression isn't a
                    // constant. Should it be suggested as a code fix at all ?
                    addDiagnostic(Diagnostic.Create(DescriptorToInitializer, location));
                }
            }

            if (property.AccessorList != null && property.AccessorList.Accessors.Count == 1)
            {
                var accessor = property.AccessorList.Accessors.Single();

                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                {
                    if (accessor.Body != null && accessor.Body.Statements.Count == 1
                        && accessor.Body.Statements.Single().IsKind(SyntaxKind.ReturnStatement))
                    {
                        addDiagnostic(Diagnostic.Create(DescriptorToExpression, location));

                        // ??? When is it possible ???
                        addDiagnostic(Diagnostic.Create(DescriptorToInitializer, location));
                    }
                }
            }
        }
    }


}
