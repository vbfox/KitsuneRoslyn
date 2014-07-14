// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.CanReplaceConcatOperator
{
    [TestClass]
    public class CanReplaceConcatOperatorAnalyzerTests
    {
        [TestMethod]
        public void No_diagnostic_on_single_string()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = ""Hello"";");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Diagnostic_one_string()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = ""Hello"" + ""World"";");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseStringId);
        }

        [TestMethod]
        public void Diagnostic_two_string()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = ""Hello"" + "" "" + ""World"";");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseStringId);
        }

        [TestMethod]
        public void Diagnostic_one_char_one_string()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = 'H' + ""ello"";");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseStringId);
        }

        [TestMethod]
        public void Diagnostic_one_string_one_char()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = ""Hell"" + ' ';");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseStringId);
        }

        [TestMethod]
        public void Diagnostic_one_string_null()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = ""Hello"" + null;");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseStringId);
        }

        [TestMethod]
        public void Diagnostic_null_one_string()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = null + ""Hello"";");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseStringId);
        }

        [TestMethod]
        public void Diagnostic_format_with_string()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = ""Hello""; var b = a + ""World"";");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseFormatId);
        }

        [TestMethod]
        public void Diagnostic_format_with_int()
        {
            var analyzer = new CanReplaceConcatOperatorAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, @"var a = 42; var b = a + ""World"";");
            Check.That(diagnostics).HasSize(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(CanReplaceConcatOperatorAnalyzer.UseFormatId);
        }
    }
}