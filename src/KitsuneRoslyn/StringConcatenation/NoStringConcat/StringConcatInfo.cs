﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.StringConcatenation.NoStringConcat.StringConcatIdentification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.NoStringConcat
{
    class StringConcatInfo
    {
        public StringConcatInfo(StringConcatClassification classification, ImmutableList<ExpressionSyntax> expressions)
        {
            Parameter.MustNotBeNull(expressions, "expressions");
            Classification = classification;
            Expressions = expressions;
        }

        public static StringConcatInfo NoReplacement { get; }
            = new StringConcatInfo(
                StringConcatClassification.NoReplacement,
                ImmutableList<ExpressionSyntax>.Empty);

        public StringConcatClassification Classification { get; }
        public ImmutableList<ExpressionSyntax> Expressions { get; }

        public static StringConcatInfo Create(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            Parameter.MustNotBeNull(semanticModel, "semanticModel");

            if (invocation == null)
            {
                return NoReplacement;
            }

            if (!CouldBeStringConcatFast(invocation))
            {
                // In some cases without ever calling into the semantic model we know that we aren't interested
                return NoReplacement;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            if (!IsNonGenericStringConcat(methodSymbol) || !IsConcernedOverload(methodSymbol))
            {
                // Not String.Concat or Not one of the overload we know that we can transform successfuly
                return NoReplacement;
            }

            return CreateFromConcernedOverload(invocation, semanticModel, methodSymbol);
        }

        private static StringConcatInfo CreateFromConcernedOverload(InvocationExpressionSyntax invocation,
            SemanticModel semanticModel, IMethodSymbol methodSymbol)
        {
            ImmutableList<ExpressionSyntax> expressions;

            if (IsDirectArrayOverloadCall(semanticModel, invocation, methodSymbol))
            {
                // It is one of the single array overload, called via a non-params call

                var arrayExpression = invocation.ArgumentList.Arguments[0].Expression;
                var explicitCreation = arrayExpression as ArrayCreationExpressionSyntax;
                var implicitCreation = arrayExpression as ImplicitArrayCreationExpressionSyntax;

                if (explicitCreation == null && implicitCreation == null)
                {
                    return NoReplacement;
                }

                var initializer = explicitCreation != null
                    ? explicitCreation.Initializer
                    : implicitCreation.Initializer;

                if (initializer != null)
                {
                    expressions = initializer.Expressions.ToImmutableList();
                }
                else
                {
                    // 'new string[0]' or 'new[0]'
                    expressions = ImmutableList<ExpressionSyntax>.Empty;
                }
            }
            else
            {
                expressions = invocation.ArgumentList.Arguments.Select(a => a.Expression).ToImmutableList();
            }

            var classification = expressions.IsCoalescable()
                ? StringConcatClassification.ReplaceWithSingleString
                : StringConcatClassification.ReplaceWithStringFormat;

            return new StringConcatInfo(classification, expressions);
        }
    }
}
