using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestDiagnosticsUnitTests.Helpers.CodeFixTestHelpers;
using BlackFox.Roslyn.TestDiagnostics.NoStringConcat;

namespace TestDiagnosticsUnitTests
{
    [TestClass]
    public class NoStringConcatCodeFixTests
    {
        [TestMethod]
        public void Single_string()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(""Hello"");}}",
                @"string.Concat(""Hello"")",
                @"using System;class Foo{void Bar(){var x = ""Hello"";}}",
                new NoStringConcatCodeFix(),
                NoStringConcatAnalyzer.UseStringDescriptor).Wait();
        }

        [TestMethod]
        public void Two_string()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(""Hello"", ""World"");}}",
                @"string.Concat(""Hello"", ""World"")",
                @"using System;class Foo{void Bar(){var x = ""HelloWorld"";}}",
                new NoStringConcatCodeFix(),
                NoStringConcatAnalyzer.UseStringDescriptor).Wait();
        }

        [TestMethod]
        public void Three_string()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(""Hello"", ""World"", ""How"");}}",
                @"string.Concat(""Hello"", ""World"", ""How"")",
                @"using System;class Foo{void Bar(){var x = ""HelloWorldHow"";}}",
                new NoStringConcatCodeFix(),
                NoStringConcatAnalyzer.UseStringDescriptor).Wait();
        }

        [TestMethod]
        public void Four_string()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(""Hello"", ""World"", ""How"", ""Are"");}}",
                @"string.Concat(""Hello"", ""World"", ""How"", ""Are"")",
                @"using System;class Foo{void Bar(){var x = ""HelloWorldHowAre"";}}",
                new NoStringConcatCodeFix(),
                NoStringConcatAnalyzer.UseStringDescriptor).Wait();
        }

        [TestMethod]
        public void Five_string()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(""Hello"", ""World"", ""How"", ""Are"", ""U"");}}",
                @"string.Concat(""Hello"", ""World"", ""How"", ""Are"", ""U"")",
                @"using System;class Foo{void Bar(){var x = ""HelloWorldHowAreU"";}}",
                new NoStringConcatCodeFix(),
                NoStringConcatAnalyzer.UseStringDescriptor).Wait();
        }
    }
}
