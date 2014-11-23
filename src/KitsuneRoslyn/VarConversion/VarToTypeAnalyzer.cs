// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BlackFox.Roslyn.Diagnostics.VarConversion
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VarToTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "BlackFox.VarToType";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Type can be explicitly specified",
                "{0} can be explicitly specified",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
        = ImmutableArray.Create(SyntaxKind.VariableDeclaration);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var declaration = Parameter.MustBeOfType<VariableDeclarationSyntax>(context.Node, "node");
            
            if (!declaration.Type.IsVar)
            {
                return;
            }

            var type = context.SemanticModel.GetTypeInfo(declaration.Type, context.CancellationToken);
            context.CancellationToken.ThrowIfCancellationRequested();

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.Type.GetLocation(), type.Type));
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.VariableDeclaration);
        }
    }
}
