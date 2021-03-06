﻿// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.NoStringConcat
{
    [TestClass]
    public class NoStringConcatAnalyzerTests
    {
        [TestMethod]
        public async void No_diagnostic_on_other_call()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"String.Join(""hello"", ""world"");");
            Check.That(diagnostics.Length).IsEqualTo(0);
        }

        [TestMethod]
        public async void Diagnostic_simple_one_string_param()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(""Hello"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_one_string_para_with_namespace()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"System.String.Concat(""Hello"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_one_string_param_fully_qualified()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"global::System.String.Concat(""Hello"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_two_string_param()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(""Hello"", ""World"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_five_string_param()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(""Hello"", ""World"", ""hi"", ""params"", ""version"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_with_null()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(""Hello"", null, ""hi"", ""params"", ""version"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_with_char()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(""Hello"", 'a', ""hi"", ""params"", ""version"");");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_simple_array()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(new[] { ""hello"", ""world"" });");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseStringId);
        }

        [TestMethod]
        public async void Diagnostic_format_string_const()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"const string TOTO=""toto""; string.Concat(""Hello"", TOTO);");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseFormatId);
        }

        [TestMethod]
        public async void Diagnostic_format_int_const()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"const int FORTY_TWO=42; string.Concat(""Hello"", FORTY_TWO);");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseFormatId);
        }

        [TestMethod]
        public async void Diagnostic_format_literal_non_string()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(""Hello"", 0);");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseFormatId);
        }

        [TestMethod]
        public async void Diagnostic_format_var()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"var d = 666.42; string.Concat(""Hello"", d, 'w', 'o', 'r', 'l', 'd');");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseFormatId);
        }

        [TestMethod]
        public async void Diagnostic_format_string_array()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"var y = ""z""; string.Concat(new[] { ""hello"", y });");
            Check.That(diagnostics.Length).IsEqualTo(1);
            var diag = diagnostics[0];
            Check.That(diag.Id).IsEqualTo(NoStringConcatAnalyzer.UseFormatId);
        }

        [TestMethod]
        public async void No_diagnostic_external_array()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"var arr = new[] { ""hello"", y };string.Concat(arr);");
            Check.That(diagnostics.Length).IsEqualTo(0);
        }

        [TestMethod]
        public async void No_diagnostic_string_enumerable()
        {
            var analyzer = new NoStringConcatAnalyzer();
            var diagnostics = await GetDiagnosticsInSimpleCodeAsync(analyzer, @"string.Concat(Enumerable.Empty<string>());");
            Check.That(diagnostics.Length).IsEqualTo(0);
        }
    }
}
