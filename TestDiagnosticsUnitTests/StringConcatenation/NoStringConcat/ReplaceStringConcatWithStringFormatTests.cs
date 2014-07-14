// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.CodeFixTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.NoStringConcat
{
    [TestClass]
    public class ReplaceStringConcatWithStringFormatTests
    {
        [TestMethod]
        public void Single_value()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(5);}}",
                @"string.Concat(5)",
                @"using System;class Foo{void Bar(){var x = string.Format(""{0}"", 5); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void Single_string_variable()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var s = """"; var x = string.Concat(s);}}",
                @"string.Concat(s)",
                @"using System;class Foo{void Bar(){var s = """"; var x = string.Format(""{0}"", s); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void Single_object_variable()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){object i = 42; var x = string.Concat(i);}}",
                @"string.Concat(i)",
                @"using System;class Foo{void Bar(){object i = 42; var x = string.Format(""{0}"", i); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_object_value()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = string.Concat(""Hello "", 5);}}",
                @"string.Concat(""Hello "", 5)",
                @"using System;class Foo{void Bar(){var x = string.Format(""Hello {0}"", 5); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_string_variable()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var s = ""42"";var x = string.Concat(""Hello "", s);}}",
                @"string.Concat(""Hello "", s)",
                @"using System;class Foo{void Bar(){var s = ""42"";var x = string.Format(""Hello {0}"", s); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_object_variable()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){object i = 42;var x = string.Concat(""Hello "", i);}}",
                @"string.Concat(""Hello "", i)",
                @"using System;class Foo{void Bar(){object i = 42;var x = string.Format(""Hello {0}"", i); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }

        //

        [TestMethod]
        public void Variable_and_chars()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){string d = ""42"";var x = string.Concat(""Hello"", d, 'w', 'o', 'r', 'l', 'd');}}",
                @"string.Concat(""Hello"", d, 'w', 'o', 'r', 'l', 'd')",
                @"using System;class Foo{void Bar(){string d = ""42"";var x = string.Format(""Hello{0}world"", d); }}",
                new ReplaceStringConcatWithStringFormat(),
                NoStringConcatAnalyzer.UseFormatDescriptor).Wait();
        }
    }
}
