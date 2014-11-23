// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BlackFox.Roslyn.Diagnostics.NoStringEmpty
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoStringEmptyAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "BlackFox.NoStringEmpty";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Don't use String.Empty",
                "String.Empty should be replaced by \"\"",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = Parameter.MustBeOfType<MemberAccessExpressionSyntax>(context.Node, "node");

            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
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

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleMemberAccessExpression);
        }
    }
}
