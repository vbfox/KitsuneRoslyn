using BlackFox.Roslyn.TestDiagnostics.NoStringEmpty;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestDiagnosticsUnitTests.Helpers.CodeFixTestHelpers;

namespace TestDiagnosticsUnitTests
{
    [TestClass]
    public class NoStringEmptyCodeFixTests
    {
        [TestMethod]
        public void Fix_on_fully_qualified_call()
        {
            CheckSingleFixAsync(
                "class Foo{void Bar(){var x = global::System.String.Empty;}}",
                "class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer()).Wait();
        }

        [TestMethod]
        public void Fix_on_namespace_qualified_call()
        {
            CheckSingleFixAsync(
                "class Foo{void Bar(){var x = System.String.Empty;}}",
                "class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer()).Wait();
        }

        [TestMethod]
        public void Fix_on_standard_call()
        {
            CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = String.Empty;}}",
                "using System;class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer()).Wait();
        }

        [TestMethod]
        public void Fix_on_method_call()
        {
            CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = FooBar(String.Empty);} void FooBar(string s) {}}",
                "using System;class Foo{void Bar(){var x = FooBar(\"\");} void FooBar(string s) {}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer()).Wait();
        }
    }
}
