// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.VarConversion
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(Id, LanguageNames.CSharp)]
    public class TypeToVarAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.TypeToVar";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "'var' can be used",
                "'var' can be used",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
        = ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)node;

            if (localDeclaration.IsConst)
            {
                return;
            }

            var variableDeclaration = localDeclaration.Declaration as VariableDeclarationSyntax;

            if (variableDeclaration == null || variableDeclaration.Type.IsVar
                || variableDeclaration.Variables.Count != 1)
            {
                return;
            }

            var leftType = semanticModel.GetTypeInfo(variableDeclaration.Type, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var variableInitializer = variableDeclaration.Variables.Single().Initializer as EqualsValueClauseSyntax;
            if (variableInitializer == null)
            {
                return;
            }
            var rightType = semanticModel.GetTypeInfo(variableInitializer.Value, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (!rightType.Equals(leftType))
            {
                return;
            }

            addDiagnostic(Diagnostic.Create(Descriptor, variableDeclaration.Type.GetLocation()));
        }
    }
}
