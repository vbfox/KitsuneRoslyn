// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.TestDiagnostics.NoNewGuid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using TestDiagnosticsUnitTests.Helpers.DiagnosticTestHelpers;

namespace TestDiagnosticsUnitTests
{
    [TestClass]
    public class NoNewGuidAnalyzerTests
    {
        [TestMethod]
        public void No_diagnostic_on_guid_empty()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = Guid.Empty;");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Diagnostic_new_call()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = new Guid();");
            Check.That(diagnostics).HasSize(1);
        }

        [TestMethod]
        public void Diagnostic_on_namespace_qualified_call()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = new System.Guid();");
            Check.That(diagnostics).HasSize(1);
        }

        [TestMethod]
        public void Diagnostic_on_fully_qualified_call()
        {
            var analyzer = new NoNewGuidAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = new global::System.Guid();");
            Check.That(diagnostics).HasSize(1);
        }
    }
}
