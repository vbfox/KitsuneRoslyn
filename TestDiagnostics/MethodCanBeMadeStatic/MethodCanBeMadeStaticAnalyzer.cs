using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.MethodCanBeMadeStatic
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.MethodCanBeMadeStaticAnalyzer", LanguageNames.CSharp)]
    public class MethodCanBeMadeStaticAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.MethodCanBeMadeStatic";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Method can be made static",
                "Method can be made static",
                "Readability",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.MethodDeclaration);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var method = (MethodDeclarationSyntax)node;

            if (semanticModel.CanBeMadeStatic(method, cancellationToken))
            {
                addDiagnostic(Diagnostic.Create(Descriptor, method.Identifier.GetLocation()));
            }
        }
    }


}
