// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.KitsuneRoslyn.RoslynExtensions;
using BlackFox.Roslyn.Diagnostics;
using BlackFox.Roslyn.Diagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.KitsuneRoslyn.UseConfigureAwait
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    class UseConfigureAwaitCodeFix : CodeFixProvider
    {
        public const string Id = "KR.AddConfigureAwait";
        public const string IdFalse = Id + ".False";
        public const string IdTrue = Id + ".True";

        static ImmutableArray<string> fixableDiagnostics = ImmutableArray.Create(UseConfigureAwaitAnalyzer.Id);

        public override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return fixableDiagnostics;
        }

        public override Task ComputeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterFix(CodeAction.Create("Use 'ConfigureAwait(false)'", 
                    ct => AddConfigureAwait(context, diagnostic, ct, false), IdFalse), diagnostic);
                context.RegisterFix(CodeAction.Create("Use 'ConfigureAwait(true)'",
                    ct => AddConfigureAwait(context, diagnostic, ct, true), IdTrue), diagnostic);
            }

            return Task.FromResult(true);
        }

        private async Task<Document> AddConfigureAwait(CodeFixContext context, Diagnostic diagnostic,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
                as ExpressionSyntax;

            if (node == null)
            {
                return context.Document;
            }

            var accessToConfigureAwait = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node,
                IdentifierName(UseConfigureAwaitAnalyzer.ConfigureAwaitMethodName));

            var invocation = InvocationExpression(accessToConfigureAwait,
                ArgumentList(new[] { BooleanLiteralExpression(continueOnCapturedContext) }));

            var replacementContext = new NodeReplacement(context.Document, node,
                invocation, context.CancellationToken, format: AdditionalAction.AddAnnotationAndRun);

            return await replacementContext.ReplaceAsync().ConfigureAwait(false);
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
