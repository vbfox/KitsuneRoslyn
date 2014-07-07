using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NFluent;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestDiagnosticsUnitTests.Helpers.DiagnosticTestHelpers;

namespace TestDiagnosticsUnitTests.Helpers
{
    static class CodeFixTestHelpers
    {
        public static async Task CheckSingleFixAsync(string code, string expectedCode, string expectedDescription,
            ICodeFixProvider codeFixProvider, params ISyntaxNodeAnalyzer<SyntaxKind>[] analyzers)
        {
            var fixes = await GetFixesAsync(code, codeFixProvider, analyzers);

            Check.That(fixes).HasSize(1);
            var fix = fixes.Single();
            Check.That(fix.Item1.Description).IsEqualTo(expectedDescription);
            Check.That(fix.Item2).IsEqualTo(expectedCode);
        }

        public static async Task CheckSingleFixAsync(string code, string spanText, string expectedCode,
            string expectedDescription, ICodeFixProvider codeFixProvider, DiagnosticDescriptor diagnosticDescriptor)
        {
            var fixes = await GetFixesAsync(code, codeFixProvider, diagnosticDescriptor, spanText);

            Check.That(fixes).HasSize(1);
            var fix = fixes.Single();
            Check.That(fix.Item1.Description).IsEqualTo(expectedDescription);
            Check.That(fix.Item2).IsEqualTo(expectedCode);
        }

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

                result.DocumentId = DocumentId.CreateNewId(result.ProjectId);
                result.Solution = new CustomWorkspace().CurrentSolution
                  .AddProject(result.ProjectId, "TestProject", "TestAssembly", LanguageNames.CSharp)
                  .AddMetadataReference(result.ProjectId,
                    new MetadataFileReference(typeof(object).Assembly.Location))
                  .AddDocument(result.DocumentId, "TestDocument.cs", code);
                result.Document = result.Solution.GetDocument(result.DocumentId);
                result.DocumentSyntaxTree = await result.Document.GetSyntaxTreeAsync();
                result.SolutionCompilation = await result.Solution.Projects.Single().GetCompilationAsync();

                return result;
            }
        }

        public static async Task<ImmutableList<Tuple<CodeAction, string>>> GetFixesAsync(string code,
            ICodeFixProvider codeFixProvider, DiagnosticDescriptor diagnosticDescriptor, string spanText)
        {
            var solution = await SingleDocumentTestSolution.CreateAsync(code);

            TextSpan span = SpanFromText(code, spanText);
            var location = Location.Create(solution.DocumentSyntaxTree, span);
            var diagnostic = Diagnostic.Create(diagnosticDescriptor, location);

            return await GetFixesAsync(codeFixProvider, solution.Document, ImmutableList.Create(diagnostic));
        }

        private static TextSpan SpanFromText(string code, string spanText)
        {
            if (spanText.StartsWith("#"))
            {
                var interesting = spanText.Select((c, i) => Tuple.Create(c != '#', i)).Where(t => t.Item1);
                var start = interesting.First().Item2;
                var end = interesting.Last().Item2;
                var length = end - start + 1;

                return new TextSpan(start, length);
            }
            else
            {
                var start = code.IndexOf(spanText);
                if (start == -1)
                {
                    throw new ArgumentException(
                        string.Format("Unable to find span '{0}' in '{1}'", spanText, code),
                        "spanText");
                }
                if (start != code.Length - 1 && code.IndexOf(spanText, start + 1) != -1)
                {
                    throw new ArgumentException(
                        string.Format("Found span '{0}' more than one time in '{1}'", spanText, code),
                        "spanText");
                }

                return new TextSpan(start, spanText.Length);
            }
        }

        public static async Task<ImmutableList<Tuple<CodeAction, string>>> GetFixesAsync(string code,
            ICodeFixProvider codeFixProvider, params ISyntaxNodeAnalyzer<SyntaxKind>[] analyzers)
        {
            var solution = await SingleDocumentTestSolution.CreateAsync(code);

            var diagnostics = GetFixableDiagnostics(codeFixProvider, analyzers, solution.DocumentSyntaxTree,
                solution.SolutionCompilation);

            return await GetFixesAsync(codeFixProvider, solution.Document, diagnostics);
        }

        private static async Task<ImmutableList<Tuple<CodeAction, string>>> GetFixesAsync(
            ICodeFixProvider codeFixProvider, Document document,
            ImmutableList<Diagnostic> diagnostics)
        {
            var codeActions = await GetCodeActionsFromFixesAsync(codeFixProvider, document, diagnostics);

            var codeChangeTasks = codeActions.Select(a => GetChangedText(a, document.Id));
            var codeChanges = await Task.WhenAll(codeChangeTasks);

            var result = codeActions.Zip(codeChanges, (a, t) => Tuple.Create(a, t));

            return ImmutableList<Tuple<CodeAction, string>>.Empty
                .AddRange(result);
        }

        private static async Task<IEnumerable<CodeAction>> GetCodeActionsFromFixesAsync(
            ICodeFixProvider codeFixProvider, Document document, ImmutableList<Diagnostic> diagnostics)
        {
            var getTasks = ImmutableList<Task<IEnumerable<CodeAction>>>.Empty;
            foreach (var diagnostic in diagnostics)
            {
                var newFixes = codeFixProvider.GetFixesAsync(
                    document,
                    diagnostic.Location.SourceSpan,
                    new[] { diagnostic },
                    new CancellationToken(false));
                getTasks = getTasks.Add(newFixes);
            }

            var codeActions = (await Task.WhenAll(getTasks)).SelectMany(_ => _);
            return codeActions;
        }

        private static ImmutableList<Diagnostic> GetFixableDiagnostics(ICodeFixProvider codeFixProvider,
            ISyntaxNodeAnalyzer<SyntaxKind>[] analyzers, SyntaxTree tree, Compilation compilation)
        {
            var fixableDiagnosticIds = codeFixProvider.GetFixableDiagnosticIds();
            var diagnostics = ImmutableList<Diagnostic>.Empty;
            foreach (var analyzer in analyzers)
            {
                AnalyzeTree(analyzer, tree, compilation, d =>
                {
                    if (fixableDiagnosticIds.Contains(d.Id))
                    {
                        diagnostics = diagnostics.Add(d);
                    }
                });
            }

            return diagnostics;
        }

        private static async Task<string> GetChangedText(CodeAction codeAction, DocumentId documentId)
        {
            var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
            var operation = (ApplyChangesOperation)operations.Single();
            var newDoc = operation.ChangedSolution.GetDocument(documentId);
            var text = await newDoc.GetTextAsync();
            return text.ToString();
        }
    }
}
