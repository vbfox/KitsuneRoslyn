// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using NFluent;

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    [TestClass]
    public class PropertyAnalyzerTests
    {
        [TestMethod]
        public void Property_return_constant()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get { return 42; } }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsOnlyMadeOf(PropertyAnalyzer.IdToExpression, PropertyAnalyzer.IdToInitializer);
        }

        [TestMethod]
        public void Property_return_expression()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get { return Math.Sin(42); } }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsOnlyMadeOf(PropertyAnalyzer.IdToExpression);
        }

        [TestMethod]
        public void Property_get_set_auto_implemented()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get; set; }");
            Check.That(diagnostics).IsEmpty();
        }

        [TestMethod]
        public void Property_get_set()
        {
            var analyzer = new PropertyAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X { get { return 42; } set {} }");
            Check.That(diagnostics).IsEmpty();
        }
    }
}
