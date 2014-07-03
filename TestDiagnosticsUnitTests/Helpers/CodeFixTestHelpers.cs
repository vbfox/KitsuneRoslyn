using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

        public static async Task<ImmutableList<Tuple<CodeAction, string>>> GetFixesAsync(string code,
            ICodeFixProvider codeFixProvider, params ISyntaxNodeAnalyzer<SyntaxKind>[] analyzers)
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var solution = new CustomWorkspace().CurrentSolution
              .AddProject(projectId, "TestProject", "TestAssembly", LanguageNames.CSharp)
              .AddMetadataReference(projectId,
                new MetadataFileReference(typeof(object).Assembly.Location))
              .AddDocument(documentId, "TestDocument.cs", code);
            var document = solution.GetDocument(documentId);
            var tree = await document.GetSyntaxTreeAsync();
            var compilation = await solution.Projects.Single().GetCompilationAsync();

            return await GetFixesAsync(codeFixProvider, analyzers, documentId, document, tree, compilation);
        }

        private static async Task<ImmutableList<Tuple<CodeAction, string>>> GetFixesAsync(
            ICodeFixProvider codeFixProvider, ISyntaxNodeAnalyzer<SyntaxKind>[] analyzers, DocumentId documentId,
            Document document, SyntaxTree tree, Compilation compilation)
        {
            var diagnostics = GetFixableDiagnostics(codeFixProvider, analyzers, tree, compilation);
            var codeActions = await GetCodeActionsFromFixesAsync(codeFixProvider, document, diagnostics);

            var codeChangeTasks = codeActions.Select(a => GetChangedText(a, documentId));
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
