// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    [ExportCodeRefactoringProvider("BlackFox.PropertyRefactoring", LanguageNames.CSharp)]
    public class PropertyRefactoring : CodeRefactoringProviderBase<PropertyDeclarationSyntax>
    {
        protected async override Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document,
            PropertyDeclarationSyntax property, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var analysis = PropertyConversionAnalysis.Create(semanticModel, property, cancellationToken);
            if (analysis.Classification == PropertyConversionClassification.NotSupported)
            {
                return Enumerable.Empty<CodeAction>();
            }

            var result = ImmutableList<CodeAction>.Empty.ToBuilder();
            if (analysis.CanBeConvertedToExpression)
            {
                result.Add(CodeAction.Create(string.Format("Convert '{0}' to expression", property.Identifier.Text),
                    token => analysis.GetDocumentAfterReplacementAsync(document,
                    PropertyConversionClassification.Expression, token)));
            }
            if (analysis.CanBeConvertedToGetWithReturn)
            {
                result.Add(CodeAction.Create(string.Format("Convert '{0}' to statement body", property.Identifier.Text),
                    token => analysis.GetDocumentAfterReplacementAsync(document,
                    PropertyConversionClassification.GetWithReturn, token)));
            }
            if (analysis.CanBeConvertedToInitializer)
            {
                result.Add(CodeAction.Create(string.Format("Convert '{0}' to initializer", property.Identifier.Text),
                    token => analysis.GetDocumentAfterReplacementAsync(document,
                    PropertyConversionClassification.Initializer, token)));
            }

            return result.ToImmutable();
        }
    }
}