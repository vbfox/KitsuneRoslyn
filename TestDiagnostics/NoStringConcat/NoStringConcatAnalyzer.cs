using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using BlackFox.Roslyn.TestDiagnostics.NoStringConcat.StringConcatReplacement;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringConcat
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.NoStringConcat", LanguageNames.CSharp)]
    public class NoStringConcatAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string UseFormatId = "BlackFox.NoStringConcat_UseFormat";
        public const string UseStringId = "BlackFox.NoStringConcat_UseString";

        public static DiagnosticDescriptor UseFormatDescriptor = new DiagnosticDescriptor(
            UseFormatId,
            "Don't use string.Concat prefer string.Format",
            "Don't use string.Concat prefer string.Format",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor UseStringDescriptor = new DiagnosticDescriptor(
            UseStringId,
            "Don't use string.Concat prefer a simple string",
            "Don't use string.Concat prefer a simple string",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics
            = ImmutableArray.Create(UseFormatDescriptor, UseStringDescriptor);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return supportedDiagnostics; } }

        static readonly ImmutableArray<SyntaxKind> syntaxKindsOfInterest
            = ImmutableArray.Create(SyntaxKind.InvocationExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return syntaxKindsOfInterest; } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var invocation = (InvocationExpressionSyntax)node;

            if (!CouldBeStringConcatFast(invocation))
            {
                // In some cases without ever calling into the semantic model we know that we aren't interested
                return;
            }

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

                var initializer = explicitCreation != null
                    ? explicitCreation.Initializer
                    : implicitCreation.Initializer;

                if (initializer != null)
                {
                    canBeTransformedToSingleString = StringCoalescing.CanBeTransformedToSingleString(semanticModel,
                        initializer.Expressions);
                }
                else
                {
                    canBeTransformedToSingleString = true; // Empty string
                }
            }
            else
            {
                canBeTransformedToSingleString = StringCoalescing.CanBeTransformedToSingleString(semanticModel,
                    invocation.ArgumentList.Arguments.Select(a => a.Expression));
            }

            var descriptor = canBeTransformedToSingleString ? UseStringDescriptor : UseFormatDescriptor;
            addDiagnostic(Diagnostic.Create(descriptor, node.GetLocation()));
        }
    }
}