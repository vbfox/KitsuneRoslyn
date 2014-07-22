// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.NoStringConcat
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.NoStringConcat", LanguageNames.CSharp)]
    public class NoStringConcatAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string UseFormatId = "BlackFox.NoStringConcat_UseFormat";
        public const string UseStringId = "BlackFox.NoStringConcat_UseString";

        public static DiagnosticDescriptor UseFormatDescriptor { get; }
            = new DiagnosticDescriptor(
                UseFormatId,
                "Don't use string.Concat prefer string.Format",
                "Don't use string.Concat prefer string.Format",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor UseStringDescriptor { get; }
            = new DiagnosticDescriptor(
                UseStringId,
                "Don't use string.Concat prefer a simple string",
                "Don't use string.Concat prefer a simple string",
                "Readability",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(UseFormatDescriptor, UseStringDescriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.InvocationExpression);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            Parameter.MustNotBeNull(addDiagnostic, "addDiagnostic");
            var invocation = Parameter.MustBeOfType<InvocationExpressionSyntax>(node, "node");

            var infos = StringConcatInfo.Create(invocation, semanticModel);

            switch (infos.Classification)
            {
                case StringConcatClassification.ReplaceWithSingleString:
                    addDiagnostic(Diagnostic.Create(UseStringDescriptor, node.GetLocation()));
                    break;

                case StringConcatClassification.ReplaceWithStringFormat:
                    addDiagnostic(Diagnostic.Create(UseFormatDescriptor, node.GetLocation()));
                    break;
            }
        }
    }
}
