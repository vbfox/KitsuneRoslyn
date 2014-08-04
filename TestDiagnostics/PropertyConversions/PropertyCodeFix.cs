// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class PropertyCodeFix : SimpleCodeFixProviderBase
    {
        public const string Id = "BlackFox.PropertyCodeFix";

        public int Prop { get; set; }

        private static readonly ImmutableDictionary<string, string> supported
            = ImmutableDictionary<string, string>.Empty
            .Add(PropertyAnalyzer.IdExpressionToStatement, "Convert to statement body")
            .Add(PropertyAnalyzer.IdExpressionToInitializer, "Convert to initializer")
            .Add(PropertyAnalyzer.IdInitializerToExpression, "Convert to expression")
            .Add(PropertyAnalyzer.IdInitializerToStatement, "Convert to statement body")
            .Add(PropertyAnalyzer.IdStatementToExpression, "Convert to expression")
            .Add(PropertyAnalyzer.IdStatementToInitializer, "Convert to initializer");

        public PropertyCodeFix() : base(supported)
        {
        }

        protected override async Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var property = nodeToFix.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

            var replacement = GetReplacement(diagnosticId, property);

            if (replacement != null)
            {
                var annotatedReplacement = replacement.WithAdditionalAnnotations(Formatter.Annotation);
                var newDocument = await document.ReplaceNodeAsync(property, annotatedReplacement, cancellationToken);

                // Avoid a roslyn bug
                // See RoslynBugs.Property_with_expression_formating in test project for a repro
                bool format = diagnosticId != PropertyAnalyzer.IdInitializerToExpression
                    && diagnosticId != PropertyAnalyzer.IdStatementToExpression;

                return format ? await newDocument.FormatAsync(cancellationToken) : newDocument;
            }
            else
            {
                return document;
            }
        }

        delegate PropertyDeclarationSyntax PropertyReplacement(PropertyDeclarationSyntax property);

        private static readonly ImmutableDictionary<string, PropertyReplacement> replacements
            = ImmutableDictionary<string, PropertyReplacement>.Empty
            .Add(PropertyAnalyzer.IdInitializerToStatement, InitializerToStatement)
            .Add(PropertyAnalyzer.IdExpressionToStatement, ExpressionToStatement)
            .Add(PropertyAnalyzer.IdExpressionToInitializer, ExpressionToInitializer)
            .Add(PropertyAnalyzer.IdInitializerToExpression, InitializerToExpression)
            .Add(PropertyAnalyzer.IdStatementToExpression, StatementToExpression)
            .Add(PropertyAnalyzer.IdStatementToInitializer, StatementToInitializer);

        private static PropertyDeclarationSyntax GetReplacement(string diagnosticId, PropertyDeclarationSyntax property)
        {
            return replacements[diagnosticId](property);
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
    }
}
