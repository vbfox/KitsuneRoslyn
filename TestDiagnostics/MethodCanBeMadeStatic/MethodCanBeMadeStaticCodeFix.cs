using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace BlackFox.Roslyn.Diagnostics.MethodCanBeMadeStatic
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class MethodCanBeMadeStaticCodeFix : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.MethodCanBeMadeStaticCodeFix";

        protected override AdditionalAction Format { get; } = AdditionalAction.Run;

        public MethodCanBeMadeStaticCodeFix()
            : base(MethodCanBeMadeStaticAnalyzer.Id, "Make method static")
        {
        }

        protected override Task<Replacement> GetReplacementAsync(Document document, SemanticModel semanticModel,
            SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId, CancellationToken cancellationToken)
        {
            var method = nodeToFix.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            MethodDeclarationSyntax fixedMethod = GetFixedMethod(method);

            return Task.FromResult(Replacement.Create(method, fixedMethod));
        }

        private static MethodDeclarationSyntax GetFixedMethod(MethodDeclarationSyntax method)
        {
            var token = SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                .WithAdditionalAnnotations(Formatter.Annotation);

            return method.AddModifiers(token);
        }
    }
}
