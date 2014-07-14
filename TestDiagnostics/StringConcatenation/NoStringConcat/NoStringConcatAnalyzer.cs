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
            var invocation = node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                throw new ArgumentException("Node isn't an instance of InvocationExpressionSyntax", "node");
            }
            if (addDiagnostic == null)
            {
                throw new ArgumentNullException("addDiagnostic");
            }

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
