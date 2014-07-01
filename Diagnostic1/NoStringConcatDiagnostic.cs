using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions;
using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.TypeSymbolExtensions;
using System.Collections.Generic;

namespace BlackFox.Roslyn.TestDiagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.NoStringConcat", LanguageNames.CSharp)]
    public class NoStringConcatDiagnostic : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticIdFormat = "BlackFox.NoStringConcat_UseFormat";
        internal const string DiagnosticIdSimple = "BlackFox.NoStringConcat_UseString";

        static DiagnosticDescriptor RuleFormat = new DiagnosticDescriptor(
            DiagnosticIdFormat,
            "Don't use string.Concat prefer string.Format",
            "Don't use string.Concat prefer string.Format",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        static DiagnosticDescriptor RuleSimple = new DiagnosticDescriptor(
            DiagnosticIdSimple,
            "Don't use string.Concat prefer a simple string",
            "Don't use string.Concat prefer a simple string",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create(RuleFormat, RuleSimple);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return supportedDiagnostics; } }

        static readonly ImmutableArray<SyntaxKind> syntaxKindsOfInterest
            = ImmutableArray.Create(SyntaxKind.InvocationExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return syntaxKindsOfInterest; } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var invocation = (InvocationExpressionSyntax)node;
           
            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            if (!IsNonGenericStringConcat(methodSymbol) || !IsConcernedOverload(methodSymbol))
            {
                // Not String.Concat or Not one of the overload we know that we can transform successfuly
                return;
            }

            bool canBeTransformedToSingleString;
            if (IsDirectArrayOverloadCall(semanticModel, invocation, methodSymbol))
            {
                // It is one of the single array overload, called via a non-params call

                var arrayExpression = invocation.ArgumentList.Arguments[0].Expression;
                var explicitCreation = arrayExpression as ArrayCreationExpressionSyntax;
                var implicitCreation = arrayExpression as ImplicitArrayCreationExpressionSyntax;

                if (explicitCreation == null && implicitCreation == null)
                {
                    return;
                }

                var initializer = explicitCreation != null ? explicitCreation.Initializer : implicitCreation.Initializer;
                if (initializer != null)
                {
                    canBeTransformedToSingleString = CanBeTransformedToSingleString(semanticModel, initializer.Expressions);
                }
                else
                {
                    canBeTransformedToSingleString = true; // Empty string
                }
            }
            else
            {
                canBeTransformedToSingleString = CanBeTransformedToSingleString(semanticModel,
                    invocation.ArgumentList.Arguments.Select(a => a.Expression));
            }

            addDiagnostic(Diagnostic.Create(canBeTransformedToSingleString ? RuleSimple : RuleFormat, node.GetLocation()));
        }

        static bool IsDirectArrayOverloadCall(SemanticModel semanticModel, InvocationExpressionSyntax invocation,
            IMethodSymbol symbol)
        {
            if (invocation.ArgumentList.Arguments.Count != 1)
            {
                return false;
            }

            var argumentType = semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression);

            return (argumentType.ConvertedType.IsArrayOfSystemObject() && objectArrayOverload.IsOverload(symbol))
                || (argumentType.ConvertedType.IsArrayOfSystemString() && stringArrayOverload.IsOverload(symbol));
        }

        static bool CanBeTransformedToSingleString(SemanticModel semanticModel, IEnumerable<ExpressionSyntax> expressions)
        {
            return expressions.All(expression => IsLiteralStringOrSimilar(semanticModel, expression));
        }

        static bool IsLiteralStringOrSimilar(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            var literal = expression as LiteralExpressionSyntax;

            return (literal != null) &&
                (
                typeInfo.Type.IsSystemString()
                || typeInfo.Type.IsSystemChar()
                || literal.CSharpKind() == SyntaxKind.NullLiteralExpression
                );
        }

        static OverloadDefinition stringArrayOverload = new OverloadDefinition(IsArrayOfSystemString);
        static OverloadDefinition objectArrayOverload = new OverloadDefinition(IsArrayOfSystemObject);

        static ImmutableArray<OverloadDefinition> concernedOverloads = ImmutableArray.Create(
            new OverloadDefinition(IsSystemObject),
            stringArrayOverload,
            objectArrayOverload,
            new OverloadDefinition(IsSystemObject, IsSystemObject),
            new OverloadDefinition(IsSystemString, IsSystemString),
            new OverloadDefinition(IsSystemObject, IsSystemObject, IsSystemObject),
            new OverloadDefinition(IsSystemString, IsSystemString, IsSystemString),
            new OverloadDefinition(IsSystemObject, IsSystemObject, IsSystemObject, IsSystemObject),
            new OverloadDefinition(IsSystemString, IsSystemString, IsSystemString, IsSystemString)
            );

        static bool IsConcernedOverload(IMethodSymbol symbol)
        {
            return concernedOverloads.Any(overload => overload.IsOverload(symbol));
        }

        static bool IsNonGenericStringConcat(IMethodSymbol symbol)
        {
            return symbol != null
                && symbol.ContainingType.SpecialType == SpecialType.System_String
                && symbol.Name == "Concat"
                && symbol.IsStatic
                && !symbol.IsGenericMethod // Ignore the overload taking IEnumerable<T>
                && symbol.MethodKind == MethodKind.Ordinary;
        }
    }
}