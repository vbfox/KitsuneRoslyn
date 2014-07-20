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
    [ExportDiagnosticAnalyzer("BlackFox.TypeToVar", LanguageNames.CSharp)]
    public class TypeToVarAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string Id = "BlackFox.TypeToVar_Normal";
        public const string IdWithCast = "BlackFox.TypeToVar_WithCast";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Use implicitly typed local variable declaration",
                "Use implicitly typed local variable declaration",
                "Readability",
                DiagnosticSeverity.Hidden,
                isEnabledByDefault: true);

        public static DiagnosticDescriptor DescriptorWithCast { get; }
            = new DiagnosticDescriptor(
                IdWithCast,
                "Use implicitly typed local variable declaration",
                "Use implicitly typed local variable declaration",
                "Readability",
                DiagnosticSeverity.Warning,
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

            var location = variableDeclaration.Type.GetLocation();
            var rightIsCast = IsWithPotentialParentheses<CastExpressionSyntax>(variableInitializer.Value);
            addDiagnostic(Diagnostic.Create(rightIsCast ? DescriptorWithCast : Descriptor, location));
        }

        static bool IsWithPotentialParentheses<T>(ExpressionSyntax expression)
        {
            var parenthesized = expression as ParenthesizedExpressionSyntax;
            if (parenthesized != null)
            {
                return IsWithPotentialParentheses<T>(parenthesized.Expression);
            }

            return expression is T;
        }
    }
}
