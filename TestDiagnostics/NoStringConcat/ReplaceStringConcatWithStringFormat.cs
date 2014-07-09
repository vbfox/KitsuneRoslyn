using BlackFox.Roslyn.TestDiagnostics.RoslynExtensions.SyntaxFactoryAdditions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.TestDiagnostics.NoStringConcat
{
    [ExportCodeFixProvider(Id, LanguageNames.CSharp)]
    public class ReplaceStringConcatWithStringFormat : ReplacementCodeFixProviderBase
    {
        public const string Id = "BlackFox.ReplaceStringConcatWithStringFormat";

        protected override bool Simplify
        {
            get
            {
                return true;
            }
        }

        protected override bool Format
        {
            get
            {
                return true;
            }
        }

        public ReplaceStringConcatWithStringFormat()
            : base(NoStringConcatAnalyzer.UseFormatId, "Replace with String.Format")
        {
        }

        ExpressionSyntax stringFormatAccess = SimpleMemberAccessExpression("System", "String", "Format");

        protected override async Task<SyntaxNode> GetReplacementNodeAsync(Document document,
            SemanticModel semanticModel, SyntaxNode root, SyntaxNode nodeToFix, string diagnosticId,
            CancellationToken cancellationToken)
        {
            var invocation = (InvocationExpressionSyntax)nodeToFix;

            var info = StringConcatInfo.Create(invocation, semanticModel);
            Debug.Assert(info.Classification == StringConcatClassification.ReplaceWithStringFormat,
                "Expected replace with string format classification");

            var formatString = "";
            var remainingExpressions = info.Expressions.AsEnumerable();
            int currentReplacementIndex = 0;
            var stringFormatArguments = ImmutableQueue<ExpressionSyntax>.Empty;
            while (remainingExpressions.Any())
            {
                int takenAsSingleString;
                var longestSingleString = GetLongestSingleString(remainingExpressions, semanticModel,
                    out takenAsSingleString);
                if (takenAsSingleString != 0)
                {
                    formatString += longestSingleString;
                    remainingExpressions = remainingExpressions.Skip(takenAsSingleString);
                }
                else
                {
                    formatString += "{" + currentReplacementIndex + "}";
                    currentReplacementIndex += 1;
                    var expression = remainingExpressions.First();
                    stringFormatArguments = stringFormatArguments.Enqueue(expression);
                    remainingExpressions = remainingExpressions.Skip(1);
                }
            }

            var arguments = new[] { StringLiteralExpression(formatString) }
                .Concat(stringFormatArguments)
                .Select(e => Argument(e));

            var argumentsSyntaxList = new SeparatedSyntaxList<ArgumentSyntax>().AddRange(arguments);

            return InvocationExpression(stringFormatAccess, ArgumentList(argumentsSyntaxList));
        }

        string GetLongestSingleString(IEnumerable<ExpressionSyntax> expressions, SemanticModel semanticModel,
            out int taken)
        {
            var coalescable = expressions.TakeWhile(StringCoalescing.IsLiteralStringOrSimilar)
                .ToImmutableArray();

            taken = coalescable.Length;

            return coalescable.Coalesce(semanticModel);
        }
    }
}
