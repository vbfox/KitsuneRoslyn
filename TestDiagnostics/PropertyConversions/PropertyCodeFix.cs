// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public const string Id = "BlackFox.PropertyCodeFixes";

        private static readonly ImmutableDictionary<string, string> supported
            = ImmutableDictionary<string, string>.Empty
            .Add(PropertyAnalyzer.IdToStatement, "Convert to statement body")
            .Add(PropertyAnalyzer.IdToExpression, "Convert to expression")
            .Add(PropertyAnalyzer.IdToInitializer, "Convert to initializer");

        public PropertyCodeFix() : base(supported)
        {
        }

        protected override Task<Document> GetUpdatedDocumentAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var property = nodeToFix.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();
            PropertyDeclarationSyntax replacement = null;

            if (diagnosticId == PropertyAnalyzer.IdToStatement)
            {
                if (property.Initializer != null)
                {
                    replacement = property.WithInitializer(null).WithGet(property.Initializer.Value);
                }
                if (property.ExpressionBody != null)
                {
                    replacement = property.WithExpressionBody(null).WithGet(property.ExpressionBody.Expression);
                }
            }

            if (replacement != null)
            {
                var newRoot = root.ReplaceNode(property, replacement);
                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            }
            else
            {
                return Task.FromResult(document);
            }
        }
    }
}
