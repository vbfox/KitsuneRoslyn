// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    [ExportCodeRefactoringProvider("BlackFox.PropertyRefactoring", LanguageNames.CSharp)]
    public class PropertyRefactoring : CodeRefactoringProviderBase<PropertyDeclarationSyntax>
    {
        protected async override Task GetRefactoringsAsync(CodeRefactoringContext context,
            PropertyDeclarationSyntax property)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var analysis = PropertyConversionAnalysis.Create(semanticModel, property, context.CancellationToken);
            if (analysis.Classification == PropertyConversionClassification.NotSupported)
            {
                return;
            }

            var result = ImmutableList<CodeAction>.Empty.ToBuilder();
            if (analysis.CanBeConvertedToExpression)
            {
                var codeAction = CodeAction.Create(string.Format("Convert '{0}' to expression", property.Identifier.Text),
                    token => analysis.GetDocumentAfterReplacementAsync(context.Document,
                    PropertyConversionClassification.Expression, token));
                context.RegisterRefactoring(codeAction);
            }
            if (analysis.CanBeConvertedToGetWithReturn)
            {
                var codeAction = CodeAction.Create(string.Format("Convert '{0}' to statement body", property.Identifier.Text),
                    token => analysis.GetDocumentAfterReplacementAsync(context.Document,
                    PropertyConversionClassification.GetWithReturn, token));
                context.RegisterRefactoring(codeAction);
            }
            if (analysis.CanBeConvertedToInitializer)
            {
                var codeAction = CodeAction.Create(string.Format("Convert '{0}' to initializer", property.Identifier.Text),
                    token => analysis.GetDocumentAfterReplacementAsync(context.Document,
                    PropertyConversionClassification.Initializer, token));
                context.RegisterRefactoring(codeAction);
            }
        }
    }
}