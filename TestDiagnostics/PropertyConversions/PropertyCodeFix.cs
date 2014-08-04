// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
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
            var property = (PropertyDeclarationSyntax)nodeToFix;

            var analysis = PropertyConversionAnalysis.Create(semanticModel, property, cancellationToken);

            switch (diagnosticId)
            {
                case PropertyAnalyzer.IdExpressionToInitializer:
                case PropertyAnalyzer.IdStatementToInitializer:
                    return await analysis.GetDocumentAfterReplacementAsync(document,
                        PropertyConversionClassification.Initializer, cancellationToken);

                case PropertyAnalyzer.IdInitializerToExpression:
                case PropertyAnalyzer.IdStatementToExpression:
                    return await analysis.GetDocumentAfterReplacementAsync(document,
                        PropertyConversionClassification.Expression, cancellationToken);

                case PropertyAnalyzer.IdExpressionToStatement:
                case PropertyAnalyzer.IdInitializerToStatement:
                    return await analysis.GetDocumentAfterReplacementAsync(document,
                        PropertyConversionClassification.GetWithReturn, cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException("diagnosticId", "Unknown diagnostic: " + diagnosticId);
            }
        }
    }
}
