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
    public class NoStringEmptyDiagnostic : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "BlackFox.NoStringEmpty";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Don't use String.Empty",
            "String.Empty should be replaced by \"\"",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(SyntaxKind.SimpleMemberAccessExpression);
            }
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax)node;

            var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol == null
                || symbol.ContainingType.SpecialType != SpecialType.System_String
                || !symbol.IsStatic
                || symbol.Kind != SymbolKind.Field
                || symbol.Name != "Empty"
                )
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }
    }
}
