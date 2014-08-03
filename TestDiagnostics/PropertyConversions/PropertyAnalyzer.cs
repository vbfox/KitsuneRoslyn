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

            if (property.Initializer != null)
            {
                AnalyzeWithInitializer(semanticModel, addDiagnostic, cancellationToken, property);
            }
            else if (property.ExpressionBody != null)
            {
                AnalyzeWithExpressionBody(semanticModel, addDiagnostic, property);
            }
            else
            {
                AnalyzeStandardProperty(semanticModel, addDiagnostic, property);
            }
        }

        private static void AnalyzeStandardProperty(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, PropertyDeclarationSyntax property)
        {
            if (property.AccessorList == null || property.AccessorList.Accessors.Count != 1)
            {
                // Only single accessor properties are matched
                return;
            }

            var getAccessor = property.AccessorList.Accessors
                .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration) && a.Body != null);

            if (getAccessor == null)
            {
                return;
            }

            var returnStatement = getAccessor.Body.Statements.OfType<ReturnStatementSyntax>()
                .FirstOrDefault();
            if (getAccessor.Body.Statements.Count != 1 || returnStatement == null)
            {
                return;
            }

            var location = property.GetLocation();
            addDiagnostic(Diagnostic.Create(DescriptorToExpression, location));

            var type = semanticModel.GetDeclaredSymbol(property).ContainingType;
            if (semanticModel.CanBeMadeStatic(returnStatement.Expression, type))
            {
                addDiagnostic(Diagnostic.Create(DescriptorToInitializer, location));
            }
        }

        private static void AnalyzeWithExpressionBody(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, PropertyDeclarationSyntax property)
        {
            var location = property.GetLocation();
            addDiagnostic(Diagnostic.Create(DescriptorToStatement, location));

            var type = semanticModel.GetDeclaredSymbol(property).ContainingType;
            if (semanticModel.CanBeMadeStatic(property.ExpressionBody.Expression, type))
            {
                addDiagnostic(Diagnostic.Create(DescriptorToInitializer, location));
            }
        }

        private static void AnalyzeWithInitializer(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken, PropertyDeclarationSyntax property)
        {
            var referencesToCtor = ReferenceConstructorArgument(property.Initializer.Value, semanticModel,
                cancellationToken);

            if (referencesToCtor)
            {
                // The only potential references to a constructor argument are to a primary constructor argument.
                // And they can only be referenced from initializers, never from other forms of properties.
                return;
            }

            var location = property.GetLocation();
            addDiagnostic(Diagnostic.Create(DescriptorToExpression, location));
            addDiagnostic(Diagnostic.Create(DescriptorToStatement, location));
        }

        static bool ReferenceConstructorArgument(ExpressionSyntax expression, SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return expression.DescendantNodesAndSelf()
                .Select(n => semanticModel.GetSymbolInfo(n, cancellationToken))
                .Any(s => s.Symbol != null
                    && s.Symbol.Kind == SymbolKind.Parameter
                    && s.Symbol.ContainingSymbol.Name == ".ctor");
        }
    }


}
