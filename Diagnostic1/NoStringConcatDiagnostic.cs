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

            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            if (!IsNonGenericStringConcat(symbol))
            {
                return;
            }

            if (!IsConcernedOverload(symbol))
            {
                return;
            }

            var allConstants = invocation.ArgumentList.Arguments
                .All(argument => ArgumentIsConstantStringOrSimilar(semanticModel, argument));

            addDiagnostic(Diagnostic.Create(allConstants ? RuleSimple : RuleFormat, node.GetLocation()));
        }

        static bool ArgumentIsConstantStringOrSimilar(SemanticModel semanticModel, ArgumentSyntax argument)
        {
            var typeInfo = semanticModel.GetTypeInfo(argument.Expression);
            var literal = argument.Expression as LiteralExpressionSyntax;

            return (literal != null) &&
                (
                typeInfo.Type.IsSystemString()
                || typeInfo.Type.IsSystemChar()
                || literal.CSharpKind() == SyntaxKind.NullLiteralExpression
                );
        }

        static ImmutableArray<ImmutableArray<Func<ITypeSymbol, bool>>> concernedOverloads = ImmutableArray.Create(
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemObject),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsArrayOfSystemObject),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsArrayOfSystemString),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemObject, IsSystemObject),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemString, IsSystemString),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemObject, IsSystemObject, IsSystemObject),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemString, IsSystemString, IsSystemString),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemObject, IsSystemObject, IsSystemObject, IsSystemObject),
            ImmutableArray.Create<Func<ITypeSymbol, bool>>(IsSystemString, IsSystemString, IsSystemString, IsSystemString)
            );

        static bool IsConcernedOverload(IMethodSymbol symbol)
        {
            return concernedOverloads.Any(checks => Matches(checks, symbol.Parameters));
        }

        static bool Matches(ImmutableArray<Func<ITypeSymbol, bool>> checks, ImmutableArray<IParameterSymbol> parameters)
        {
            return checks.Length == parameters.Length
                && checks.Zip(parameters, (matcher, p) => matcher(p.Type)).All(b => b);
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
