using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestDiagnosticsUnitTests.Helpers.CodeFixTestHelpers;
using BlackFox.Roslyn.TestDiagnostics.NoNewGuid;

namespace TestDiagnosticsUnitTests
{
    [TestClass]
    public class NoNewGuidCodeFixTests
    {
        [TestMethod]
        public void Fix_on_fully_qualified_call()
        {
            CheckSingleFixAsync(
                "class Foo{void Bar(){var x = new global::System.Guid();}}",
                "new global::System.Guid()",
                "class Foo{void Bar(){var x = System.Guid.Empty;}}",
                "Replace with Guid.Empty",
                new NoNewGuidCodeFix(),
                NoNewGuidAnalyzer.Descriptor).Wait();
        }

        [TestMethod]
        public void Fix_on_namespace_qualified_call()
        {
            CheckSingleFixAsync(
                "class Foo{void Bar(){var x = new System.Guid();}}",
                "new System.Guid()",
                "class Foo{void Bar(){var x = System.Guid.Empty;}}",
                "Replace with Guid.Empty",
                new NoNewGuidCodeFix(),
                NoNewGuidAnalyzer.Descriptor).Wait();
        }

        [TestMethod]
        public void Fix_on_standard_call()
        {
            CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = new Guid();}}",
                "new Guid()",
                "using System;class Foo{void Bar(){var x = Guid.Empty;}}",
                "Replace with Guid.Empty",
                new NoNewGuidCodeFix(),
                NoNewGuidAnalyzer.Descriptor).Wait();
        }

        [TestMethod]
        public void Fix_on_method_call()
        {
            CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = FooBar(new Guid());} void FooBar(Guid g) {}}",
                "new Guid()",
                "using System;class Foo{void Bar(){var x = FooBar(Guid.Empty);} void FooBar(Guid g) {}}",
                "Replace with Guid.Empty",
                new NoNewGuidCodeFix(),
                NoNewGuidAnalyzer.Descriptor).Wait();
        }
    }
}
