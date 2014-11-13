// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BlackFox.Roslyn.Diagnostics.NoNewGuid
{
    [TestClass]
    public class NoNewGuidAnalyzerTests
    {
        [TestMethod]
        public async void No_diagnostic_on_guid_empty()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = Guid.Empty;");
            Check.That(diagnostics.Length).IsEqualTo(0);
        }

        [TestMethod]
        public async void Diagnostic_new_call()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = new Guid();");
            Check.That(diagnostics.Length).IsEqualTo(1);
        }

        [TestMethod]
        public async void Diagnostic_on_namespace_qualified_call()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = new System.Guid();");
            Check.That(diagnostics.Length).IsEqualTo(1);
        }

        [TestMethod]
        public async void Diagnostic_on_fully_qualified_call()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, "var x = new global::System.Guid();");
            Check.That(diagnostics.Length).IsEqualTo(1);
        }
    }
}
