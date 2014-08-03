// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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
                return await ReplaceAndFormat(document, root, cancellationToken, property, replacement);
            }
            else
            {
                return document;
            }
        }

        private static async Task<Document> ReplaceAndFormat(Document document, SyntaxNode root,
            CancellationToken cancellationToken, PropertyDeclarationSyntax from,
            PropertyDeclarationSyntax to)
        {
            var newRoot = root.ReplaceNode(from, to.WithAdditionalAnnotations(Formatter.Annotation));
            var newDocument = document.WithSyntaxRoot(newRoot);

            var formattingTask = Formatter.FormatAsync(
                newDocument,
                Formatter.Annotation,
                cancellationToken: cancellationToken);

            return await formattingTask.ConfigureAwait(false);
        }

        private static PropertyDeclarationSyntax GetReplacement(string diagnosticId, PropertyDeclarationSyntax property)
        {
            PropertyDeclarationSyntax replacement = null;

            if (diagnosticId == PropertyAnalyzer.IdInitializerToStatement)
            {
                    replacement = property
                        .WithInitializer(null)
                        .WithSemicolon(Token(SyntaxKind.None))
                        .WithGet(property.Initializer.Value);
            }
            else if(diagnosticId == PropertyAnalyzer.IdExpressionToStatement)
            {
                replacement = property
                    .WithExpressionBody(null)
                    .WithSemicolon(Token(SyntaxKind.None))
                    .WithGet(property.ExpressionBody.Expression);
            }

            return replacement;
        }
    }
}
