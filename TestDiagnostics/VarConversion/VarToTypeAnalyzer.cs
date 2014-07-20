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

namespace BlackFox.Roslyn.Diagnostics.VarConversion
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.CanReplaceConcatOperator", LanguageNames.CSharp)]
    public class VarToTypeAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.VarToType";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "var can be replaced with specific type",
                "var can be replaced with '{0}'",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
        = ImmutableArray.Create(SyntaxKind.VariableDeclaration);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var declaration = (VariableDeclarationSyntax)node;
            if (!declaration.Type.IsVar)
            {
                return;
            }

            var type = semanticModel.GetTypeInfo(declaration.Type, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            addDiagnostic(Diagnostic.Create(Descriptor, declaration.Type.GetLocation(), type.Type));
        }
    }
}
