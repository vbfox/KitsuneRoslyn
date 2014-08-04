// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    class PropertyConversionAnalysis
    {
        public PropertyConversionClassification Classification { get; private set; }
        public PropertyDeclarationSyntax Property { get; private set; }
        public bool CanBeConvertedToInitializer { get; private set; }
        public bool CanBeConvertedToExpression { get; private set; }
        public bool CanBeConvertedToGetWithReturn { get; private set; }

        private static PropertyConversionAnalysis NotSupported { get; }
        = new PropertyConversionAnalysis(PropertyConversionClassification.NotSupported, null);

        public PropertyConversionAnalysis(PropertyConversionClassification classification,
            PropertyDeclarationSyntax property, bool canBeConvertedToInitializer = false,
            bool canBeConvertedToExpression = false, bool canBeConvertedToGetWithReturn = false)
        {
            Classification = classification;
            Property = property;
            CanBeConvertedToInitializer = canBeConvertedToInitializer;
            CanBeConvertedToExpression = canBeConvertedToExpression;
            CanBeConvertedToGetWithReturn = canBeConvertedToGetWithReturn;
        }

        delegate PropertyDeclarationSyntax PropertyReplacement(PropertyDeclarationSyntax property);

        private static readonly ImmutableDictionary<Tuple<PropertyConversionClassification, PropertyConversionClassification>, PropertyReplacement> replacements
            = ImmutableDictionary<Tuple<PropertyConversionClassification, PropertyConversionClassification>, PropertyReplacement>.Empty
            .Add(Tuple.Create(PropertyConversionClassification.Initializer, PropertyConversionClassification.GetWithReturn), InitializerToStatement)
            .Add(Tuple.Create(PropertyConversionClassification.Expression, PropertyConversionClassification.GetWithReturn), ExpressionToStatement)
            .Add(Tuple.Create(PropertyConversionClassification.Expression, PropertyConversionClassification.Initializer), ExpressionToInitializer)
            .Add(Tuple.Create(PropertyConversionClassification.Initializer, PropertyConversionClassification.Expression), InitializerToExpression)
            .Add(Tuple.Create(PropertyConversionClassification.GetWithReturn, PropertyConversionClassification.Expression), StatementToExpression)
            .Add(Tuple.Create(PropertyConversionClassification.GetWithReturn, PropertyConversionClassification.Initializer), StatementToInitializer);

        public async Task<Document> GetDocumentAfterReplacementAsync(Document document,
            PropertyConversionClassification to, CancellationToken cancellationToken = default(CancellationToken))
        {
            var replacement = GetReplacement(to);

            var annotatedReplacement = replacement.WithAdditionalAnnotations(Formatter.Annotation);
            var newDocument = await document.ReplaceNodeAsync(Property, annotatedReplacement, cancellationToken);

            // Avoid a roslyn bug
            // See RoslynBugs.Property_with_expression_formating in test project for a repro
            bool format = to != PropertyConversionClassification.Expression;

            return format ? await newDocument.FormatAsync(cancellationToken) : newDocument;
        }

        public PropertyDeclarationSyntax GetReplacement(PropertyConversionClassification to)
        {
            if (to == PropertyConversionClassification.GetWithReturn && !CanBeConvertedToGetWithReturn)
            {
                throw new InvalidOperationException("This property can't be converted to get with return syntax");
            }

            if (to == PropertyConversionClassification.Initializer && !CanBeConvertedToInitializer)
            {
                throw new InvalidOperationException("This property can't be converted to initializer syntax");
            }

            if (to == PropertyConversionClassification.Expression && !CanBeConvertedToExpression)
            {
                throw new InvalidOperationException("This property can't be converted to expression syntax");
            }

            var supported = replacements.TryGetValue(Tuple.Create(Classification, to), out var replacement);
            if (!supported)
            {
                throw new InvalidOperationException(string.Format("Replacement not supported {0} -> {1}",
                    Classification, to));
            }

            return replacement(Property);
        }

        private static PropertyDeclarationSyntax StatementToInitializer(PropertyDeclarationSyntax property)
        {
            PropertyDeclarationSyntax replacement;
            var expression = GetPropertyGetterExpression(property);

            replacement = property
                .WithAccessorList(null)
                .WithSemicolon(Token(SyntaxKind.SemicolonToken))
                .WithAccessor(SyntaxKind.GetAccessorDeclaration, null)
                .WithInitializer(EqualsValueClause(expression));
            return replacement;
        }

        private static PropertyDeclarationSyntax StatementToExpression(PropertyDeclarationSyntax property)
        {
            PropertyDeclarationSyntax replacement;
            var expression = GetPropertyGetterExpression(property);

            replacement = property
                .WithAccessorList(null)
                .WithSemicolon(Token(SyntaxKind.SemicolonToken))
                .WithExpressionBody(ArrowExpressionClause(expression));
            return replacement;
        }

        private static PropertyDeclarationSyntax InitializerToExpression(PropertyDeclarationSyntax property)
        {
            return property
                .WithInitializer(null)
                .WithAccessorList(null)
                .WithExpressionBody(ArrowExpressionClause(property.Initializer.Value));
        }

        private static PropertyDeclarationSyntax ExpressionToInitializer(PropertyDeclarationSyntax property)
        {
            return property
                .WithExpressionBody(null)
                .WithAccessor(SyntaxKind.GetAccessorDeclaration, null)
                .WithInitializer(EqualsValueClause(property.ExpressionBody.Expression));
        }

        private static PropertyDeclarationSyntax ExpressionToStatement(PropertyDeclarationSyntax property)
        {
            return property
                .WithExpressionBody(null)
                .WithSemicolon(Token(SyntaxKind.None))
                .WithGet(property.ExpressionBody.Expression);
        }

        private static PropertyDeclarationSyntax InitializerToStatement(PropertyDeclarationSyntax property)
        {
            return property
                .WithInitializer(null)
                .WithSemicolon(Token(SyntaxKind.None))
                .WithGet(property.Initializer.Value);
        }

        private static ExpressionSyntax GetPropertyGetterExpression(PropertyDeclarationSyntax property)
        {
            var getAccessor = property.AccessorList.Accessors
                .Single(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

            var returnStatement = (ReturnStatementSyntax)getAccessor.Body.Statements.Single();
            return returnStatement.Expression;
        }

        public static PropertyConversionAnalysis Create(SemanticModel semanticModel, PropertyDeclarationSyntax property,
            CancellationToken cancellationToken)
        {
            if (property.Initializer != null)
            {
                return AnalyzeWithInitializer(semanticModel, cancellationToken, property);
            }
            else if (property.ExpressionBody != null)
            {
                return AnalyzeWithExpressionBody(semanticModel, property);
            }
            else
            {
                return AnalyzeStandardProperty(semanticModel, property);
            }
        }

        private static PropertyConversionAnalysis AnalyzeStandardProperty(SemanticModel semanticModel,
            PropertyDeclarationSyntax property)
        {
            if (property.AccessorList == null || property.AccessorList.Accessors.Count != 1)
            {
                // Only single accessor properties are matched
                return NotSupported;
            }

            var getAccessor = property.AccessorList.Accessors
                .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration) && a.Body != null);

            if (getAccessor == null)
            {
                return NotSupported;
            }

            var returnStatement = getAccessor.Body.Statements.OfType<ReturnStatementSyntax>()
                .FirstOrDefault();
            if (getAccessor.Body.Statements.Count != 1 || returnStatement == null)
            {
                return NotSupported;
            }

            var type = semanticModel.GetDeclaredSymbol(property).ContainingType;
            bool toInitializer = semanticModel.CanBeMadeStatic(returnStatement.Expression, type);

            return new PropertyConversionAnalysis(PropertyConversionClassification.GetWithReturn, property,
                canBeConvertedToExpression: true, canBeConvertedToInitializer: toInitializer);
        }

        private static PropertyConversionAnalysis AnalyzeWithExpressionBody(SemanticModel semanticModel,
            PropertyDeclarationSyntax property)
        {
            var type = semanticModel.GetDeclaredSymbol(property).ContainingType;
            bool toInitializer = semanticModel.CanBeMadeStatic(property.ExpressionBody.Expression, type);

            return new PropertyConversionAnalysis(PropertyConversionClassification.Expression, property,
                canBeConvertedToGetWithReturn: true, canBeConvertedToInitializer: toInitializer);
        }

        private static PropertyConversionAnalysis AnalyzeWithInitializer(SemanticModel semanticModel,
            CancellationToken cancellationToken, PropertyDeclarationSyntax property)
        {
            var referencesToCtor = ReferenceConstructorArgument(property.Initializer.Value, semanticModel,
                cancellationToken);

            if (referencesToCtor)
            {
                // The only potential references to a constructor argument are to a primary constructor argument.
                // And they can only be referenced from initializers, never from other forms of properties.
                return NotSupported;
            }

            return new PropertyConversionAnalysis(PropertyConversionClassification.Initializer, property,
                canBeConvertedToGetWithReturn: true, canBeConvertedToExpression: true);
        }

        static bool ReferenceConstructorArgument(ExpressionSyntax expression, SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return expression.DescendantNodesAndSelf()
                .Select(n => semanticModel.GetSymbolInfo(n, cancellationToken))
                .Any(s => s.Symbol != null
                    && s.Symbol.Kind == SymbolKind.Parameter
                    && s.Symbol.ContainingSymbol.Name == ".ctor");
        }
    }
}
