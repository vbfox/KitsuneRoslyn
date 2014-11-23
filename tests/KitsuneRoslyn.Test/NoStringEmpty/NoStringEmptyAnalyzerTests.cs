// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BlackFox.Roslyn.Diagnostics.NoStringEmpty
{
    [TestClass]
    public class NoStringEmptyAnalyzerTests
    {
        [TestMethod]
        public async void No_diagnostic_on_empty_string()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = \"\";");
            Check.That(diagnostics.Length).IsEqualTo(0);
        }

        [TestMethod]
        public async void Diagnostic_on_standard_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = String.Empty;");
            Check.That(diagnostics.Length).IsEqualTo(1);
        }

        [TestMethod]
        public async void Diagnostic_on_namespace_qualified_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = System.String.Empty;");
            Check.That(diagnostics.Length).IsEqualTo(1);
        }

        [TestMethod]
        public async void Diagnostic_on_fully_qualified_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = global::System.String.Empty;");
            Check.That(diagnostics.Length).IsEqualTo(1);
        }
    }
}
