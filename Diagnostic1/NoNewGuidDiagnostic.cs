using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlackFox.Roslyn.TestDiagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class NoNewGuidDiagnostic : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "BlackFox.NoNewGuid";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Don't use new Guid()",
            "Don't use new Guid() prefer Guid.Empty",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(SyntaxKind.ObjectCreationExpression);
            }
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)node;
            
            var symbol = semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;

            // Non-Static constructor without parameter
            if (symbol == null
                || symbol.IsStatic
                || symbol.MethodKind != MethodKind.Constructor
                || symbol.Parameters.Length != 0)
            {
                return;
            }

            // For global::System.Guid
            var type = symbol.ContainingType;
            if (type.Name != "Guid"
                || type.ContainingNamespace == null
                || type.ContainingNamespace.Name != "System"
                || type.ContainingNamespace.ContainingNamespace == null
                || !type.ContainingNamespace.ContainingNamespace.IsGlobalNamespace)
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }
    }
}
