// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.TestDiagnostics.NoStringEmpty;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using TestDiagnosticsUnitTests.Helpers.DiagnosticTestHelpers;

namespace TestDiagnosticsUnitTests
{
    [TestClass]
    public class NoStringEmptyAnalyzerTests
    {
        [TestMethod]
        public void No_diagnostic_on_empty_string()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = \"\";");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Diagnostic_on_standard_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = String.Empty;");
            Check.That(diagnostics).HasSize(1);
        }

        [TestMethod]
        public void Diagnostic_on_namespace_qualified_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = System.String.Empty;");
            Check.That(diagnostics).HasSize(1);
        }

        [TestMethod]
        public void Diagnostic_on_fully_qualified_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = global::System.String.Empty;");
            Check.That(diagnostics).HasSize(1);
        }
    }
}
