using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics
{
    [TestClass]
    public class RoslynBugs
    {
        [TestMethod]
        public void Property_with_expression_formating()
        {
            var code = "class Foo{public int Bar=>42;}";

            var projectId = ProjectId.CreateNewId();

            var parseOptions = new CSharpParseOptions(LanguageVersion.Experimental);
            ProjectInfo projectInfo = ProjectInfo.Create(projectId, VersionStamp.Default, "TestProject",
                "TestAssembly", LanguageNames.CSharp, parseOptions: parseOptions);

            var documentId = DocumentId.CreateNewId(projectId);
            var solution = new CustomWorkspace().CurrentSolution
              .AddProject(projectInfo)
              .AddMetadataReference(projectId,
                new MetadataFileReference(typeof(object).Assembly.Location))
              .AddDocument(documentId, "TestDocument.cs", code);
            var document = solution.GetDocument(documentId);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;

            var root = syntaxTree.GetRoot();

            var property = root.DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>().Single();
            var markedForFormatProperty = property.WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = root.ReplaceNode(property, markedForFormatProperty);
            var newDocument = document.WithSyntaxRoot(newRoot);

            var formattingTask = Formatter.FormatAsync(
                newDocument,
                Formatter.Annotation);

            formattingTask.Wait();
        }
    }
}
