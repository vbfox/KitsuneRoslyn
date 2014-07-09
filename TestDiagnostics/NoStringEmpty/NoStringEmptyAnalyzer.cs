using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringEmpty
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(Id, LanguageNames.CSharp)]
    public class NoStringEmptyAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.Analyzers.NoStringEmpty";

        public static DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            "Don't use String.Empty",
            "String.Empty should be replaced by \"\"",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Descriptor); } }

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
            var memberAccess = (MemberAccessExpressionSyntax)node;

            var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol == null
                || symbol.ContainingType == null
                || symbol.ContainingType.SpecialType != SpecialType.System_String
                || !symbol.IsStatic
                || symbol.Kind != SymbolKind.Field
                || symbol.Name != "Empty"
                )
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(Descriptor, node.GetLocation()));
        }
    }
}
