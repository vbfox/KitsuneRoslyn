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
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorInitializerToStatement { get; }
            = new DiagnosticDescriptor(
                IdInitializerToStatement,
                "Can be converted to statement body",
                "Can be converted to statement body",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorExpressionToInitializer { get; }
            = new DiagnosticDescriptor(
                    IdExpressionToInitializer,
                    "Can be converted to initializer",
                    "Can be converted to initializer",
                    "Readability",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorStatementToInitializer { get; }
            = new DiagnosticDescriptor(
                    IdStatementToInitializer,
                    "Can be converted to initializer",
                    "Can be converted to initializer",
                    "Readability",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorStatementToExpression { get; }
            = new DiagnosticDescriptor(
                    IdStatementToExpression,
                    "Can be converted to expression",
                    "Can be converted to expression",
                    "Readability",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorInitializerToExpression { get; }
            = new DiagnosticDescriptor(
                    IdInitializerToExpression,
                    "Can be converted to expression",
                    "Can be converted to expression",
                    "Readability",
                    DiagnosticSeverity.Warning,
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
            addDiagnostic(Diagnostic.Create(DescriptorStatementToExpression, location));

            var type = semanticModel.GetDeclaredSymbol(property).ContainingType;
            if (semanticModel.CanBeMadeStatic(returnStatement.Expression, type))
            {
                addDiagnostic(Diagnostic.Create(DescriptorStatementToInitializer, location));
            }
        }

        private static void AnalyzeWithExpressionBody(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, PropertyDeclarationSyntax property)
        {
            var location = property.GetLocation();
            addDiagnostic(Diagnostic.Create(DescriptorExpressionToStatement, location));

            var type = semanticModel.GetDeclaredSymbol(property).ContainingType;
            if (semanticModel.CanBeMadeStatic(property.ExpressionBody.Expression, type))
            {
                addDiagnostic(Diagnostic.Create(DescriptorExpressionToInitializer, location));
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
            addDiagnostic(Diagnostic.Create(DescriptorInitializerToExpression, location));
            addDiagnostic(Diagnostic.Create(DescriptorInitializerToStatement, location));
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
