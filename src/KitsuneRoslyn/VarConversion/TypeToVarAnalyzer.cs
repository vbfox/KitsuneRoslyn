// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.VarConversion
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypeToVarAnalyzer : DiagnosticAnalyzer
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
                DiagnosticSeverity.Info,
                isEnabledByDefault: true,
                customTags: WellKnownDiagnosticTags.Unnecessary);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor, DescriptorWithCast);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var localDeclaration = Parameter.MustBeOfType<LocalDeclarationStatementSyntax>(context.Node, "node");

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

            var leftType = context.SemanticModel.GetTypeInfo(variableDeclaration.Type, context.CancellationToken);
            context.CancellationToken.ThrowIfCancellationRequested();

            var variableInitializer = variableDeclaration.Variables.Single().Initializer as EqualsValueClauseSyntax;
            if (variableInitializer == null)
            {
                return;
            }
            var rightType = context.SemanticModel.GetTypeInfo(variableInitializer.Value, context.CancellationToken);
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!rightType.Equals(leftType))
            {
                return;
            }

            var location = variableDeclaration.Type.GetLocation();
            var rightIsCast = IsWithPotentialParentheses<CastExpressionSyntax>(variableInitializer.Value);
            context.ReportDiagnostic(Diagnostic.Create(rightIsCast ? DescriptorWithCast : Descriptor, location));
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

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
        }
    }
}
