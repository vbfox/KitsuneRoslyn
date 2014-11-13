using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics.TestHelpers
{
    class SingleDocumentTestSolution
    {
        public ProjectId ProjectId { get; private set; }
        public DocumentId DocumentId { get; private set; }
        public Solution Solution { get; private set; }
        public Document Document { get; private set; }
        public SyntaxTree DocumentSyntaxTree { get; private set; }
        public Compilation SolutionCompilation { get; private set; }

        public static async Task<SingleDocumentTestSolution> CreateAsync(string code)
        {
            var result = new SingleDocumentTestSolution()
            {
                ProjectId = ProjectId.CreateNewId()
            };

            var parseOptions = new CSharpParseOptions(LanguageVersion.Experimental);
            ProjectInfo projectInfo = ProjectInfo.Create(result.ProjectId, VersionStamp.Default, "TestProject",
                "TestAssembly", LanguageNames.CSharp, parseOptions: parseOptions);

            result.DocumentId = DocumentId.CreateNewId(result.ProjectId);
            result.Solution = new CustomWorkspace().CurrentSolution
              .AddProject(projectInfo)
              .AddMetadataReference(result.ProjectId,
                AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference(display: "mscorlib"))
              .AddDocument(result.DocumentId, "TestDocument.cs", code);
            result.Document = result.Solution.GetDocument(result.DocumentId);
            result.DocumentSyntaxTree = await result.Document.GetSyntaxTreeAsync();
            result.SolutionCompilation = await result.Solution.Projects.Single().GetCompilationAsync();

            ThrowIfErrors(result);

            return result;
        }

        private static void ThrowIfErrors(SingleDocumentTestSolution result)
        {
            var errors = result.DocumentSyntaxTree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToImmutableArray();

            if (!errors.IsEmpty)
            {
                throw new InvalidOperationException("Compiled invalid program: " +
                    string.Join(", ", errors.Select(e => e.GetMessage())));
            }
        }
    }
}
