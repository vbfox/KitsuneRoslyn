// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics.TestHelpers
{
    static class DiagnosticTestHelpers
    {
        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsInSimpleCodeAsync(
            this DiagnosticAnalyzer analyzer, string code)
        {
            var fullCode = "using System; namespace TestNamespace { public class TestClass { "
                + "public static void TestMethod() { \{code} } "
                + "} }";

            return GetDiagnosticsAsync(analyzer, fullCode);
        }

        public static Task<ImmutableArray<Diagnostic>> GetDiagnosticsInClassLevelCodeAsync(
            this DiagnosticAnalyzer analyzer, string code)
        {
            var fullCode = "using System; namespace TestNamespace { public class TestClass { \{code} } }";

            return GetDiagnosticsAsync(analyzer, fullCode);
        }

        public static CSharpCompilation CreateCompilation(string code)
        {
            var options = new CSharpParseOptions(LanguageVersion.Experimental);

            var tree = SyntaxFactory.ParseSyntaxTree(code, options);
            var compilation = CSharpCompilation.Create(null,
                syntaxTrees: ImmutableArray.Create(tree),
                references: new[]
                {
                    AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference(display: "mscorlib")
                });
            var errors = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToImmutableArray();

            if (!errors.IsEmpty)
            {
                throw new InvalidOperationException("Compiled invalid program: " +
                    string.Join(", ", errors.Select(e => e.GetMessage())));
            }

            return compilation;
        }

        public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
            this DiagnosticAnalyzer analyzer, string code)
        {
            var compilation = CreateCompilation(code);
            var tree = compilation.SyntaxTrees.Single();
            var root = tree.GetRoot();

            return await AnalyzeTreeAsync(analyzer, tree, compilation);
        }

        public static Task<ImmutableArray<Diagnostic>> AnalyzeTreeAsync(this DiagnosticAnalyzer analyzer,
            SyntaxTree tree, Compilation compilation)
        {
            return AnalyzeTreeAsync(ImmutableArray.Create(analyzer), tree, compilation);
        }

        public static async Task<ImmutableArray<Diagnostic>> AnalyzeTreeAsync(ImmutableArray<DiagnosticAnalyzer> analyzers,
            SyntaxTree tree, Compilation compilation)
        {
            var analyzerOptions = new AnalyzerOptions(Enumerable.Empty<AdditionalStream>(), new Dictionary<string, string>());
            Compilation newCompilation;
            AnalyzerDriver driver = AnalyzerDriver.Create(compilation, analyzers,
                analyzerOptions, out newCompilation, CancellationToken.None);

            await driver.WhenCompletedAsync();
            return await driver.GetDiagnosticsAsync();
        }
    }
}
