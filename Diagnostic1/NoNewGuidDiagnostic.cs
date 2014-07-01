using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Diagnostics.Contracts;

namespace BlackFox.Roslyn.TestDiagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DIAGNOSTIC_ID, LanguageNames.CSharp)]
    public class NoNewGuidDiagnostic : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DIAGNOSTIC_ID = "BlackFox.NoNewGuid";

        static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DIAGNOSTIC_ID,
            "Don't use new Guid()",
            "Don't use new Guid() prefer Guid.Empty",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create(Rule);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return supportedDiagnostics; } }

        static readonly ImmutableArray<SyntaxKind> syntaxKindsOfInterest
            = ImmutableArray.Create(SyntaxKind.ObjectCreationExpression);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return syntaxKindsOfInterest; } }

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

            if (!symbol.ContainingType.IsEqualTo("System", "Guid"))
            {
                return;
            }
            
            addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }

    }
}
