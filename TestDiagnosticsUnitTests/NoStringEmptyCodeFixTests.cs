using BlackFox.Roslyn.TestDiagnostics.NoStringEmpty;
using System.Threading.Tasks;
using TestDiagnosticsUnitTests.Helpers.CodeFixTestHelpers;
using Xunit;

namespace TestDiagnosticsUnitTests
{
    public class NoStringEmptyCodeFixTests
    {
        [Fact]
        public Task Fix_on_fully_qualified_call()
        {
            return CheckSingleFixAsync(
                "class Foo{void Bar(){var x = global::System.String.Empty;}}",
                "class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer());
        }

        [Fact]
        public Task Fix_on_namespace_qualified_call()
        {
            return CheckSingleFixAsync(
                "class Foo{void Bar(){var x = System.String.Empty;}}",
                "class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer());
        }

        [Fact]
        public Task Fix_on_standard_call()
        {
            return CheckSingleFixAsync(
                "using System;class Foo{void Bar(){var x = String.Empty;}}",
                "using System;class Foo{void Bar(){var x = \"\";}}",
                "Use \"\"",
                new NoStringEmptyCodeFix(),
                new NoStringEmptyAnalyzer());
        }
    }
}
