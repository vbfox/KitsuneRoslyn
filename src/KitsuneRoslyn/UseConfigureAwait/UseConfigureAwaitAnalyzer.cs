// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BlackFox.KitsuneRoslyn.UseConfigureAwait
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseConfigureAwaitAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "KR.UseConfigureAwait";
        public const string ConfigureAwaitMethodName = "ConfigureAwait";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "await called without ConfigureAwait",
                "await called without ConfigureAwait",
                "Readability",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAwait, SyntaxKind.AwaitExpression);
        }

        private void AnalyzeAwait(SyntaxNodeAnalysisContext context)
        {
            var awaitExpression = (AwaitExpressionSyntax) context.Node;
            if (IsConfigureAwaitNeeded(awaitExpression, context.SemanticModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, awaitExpression.Expression.GetLocation()));
            }
        }

        private bool IsConfigureAwaitNeeded(AwaitExpressionSyntax awaitExpression, SemanticModel semanticModel)
        {
            var expressionTarget = semanticModel.GetTypeInfo(awaitExpression.Expression).Type;
            if (expressionTarget == null || expressionTarget is IErrorTypeSymbol)
            {
                // Error, don't bother the user
                return false;
            }

            var taskType = WellKnownTypes.Task(semanticModel.Compilation);

            if (!expressionTarget.Equals(taskType))
            {
                // Other awaitable types don't have ConfigureAwait
                return false;
            }

            var invocationExpression = awaitExpression.Expression as InvocationExpressionSyntax;
            if (invocationExpression == null)
            {
                // Direct reference to a field, a property, ...
                return true;
            }

            var symbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol;
            if (symbol == null)
            {
                // Error, don't bother the user
                return false;
            }

            // It's a method call but is it ConfigureAwait ?
            return symbol.Name != ConfigureAwaitMethodName
                || symbol.IsStatic
                || !symbol.ContainingType.Equals(taskType);
        }
    }
}
