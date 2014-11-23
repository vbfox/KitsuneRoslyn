using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics
{
    [TestClass]
    public class RoslynBugs
    {
        [TestMethod]
        public void Property_with_expression_formating()
        {
            var code = @"class Program
{
    public static string Test
    {
        get
        {
            return ""42"";
        }
    }

    public static int Other { get; } = 0;
}";
            var projectId = ProjectId.CreateNewId();

            var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp6);
            ProjectInfo projectInfo = ProjectInfo.Create(projectId, VersionStamp.Default, "TestProject",
                "TestAssembly", LanguageNames.CSharp, parseOptions: parseOptions);

            var documentId = DocumentId.CreateNewId(projectId);
            var solution = new CustomWorkspace().CurrentSolution
              .AddProject(projectInfo)
              .AddMetadataReference(projectId,
                AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference(display: "mscorlib"))
              .AddDocument(documentId, "TestDocument.cs", code);
            var document = solution.GetDocument(documentId);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;

            var root = syntaxTree.GetRoot();

            var property = root.DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>().First();
            
            var replacement = property
                .WithAccessorList(null)
                .WithSemicolon(Token(SyntaxKind.SemicolonToken))
                .WithExpressionBody(ArrowExpressionClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Hello"))));
                
            var markedForFormatProperty = replacement.WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = root.ReplaceNode(property, markedForFormatProperty);
            var newDocument = document.WithSyntaxRoot(newRoot);

            var text = newDocument.GetTextAsync().Result.ToString();

            var formattingTask = Formatter.FormatAsync(
                newDocument,
                Formatter.Annotation);

            formattingTask.Wait();
        }
    }
}