// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.CodeFixTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.CanReplaceConcatOperator
{
    [TestClass]
    public class ReplaceConcatenationWithStringFormatTests
    {
        [TestMethod]
        public void String_and_int_value()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = ""Hello "" +  5;}}",
                @"""Hello "" +  5",
                @"using System;class Foo{void Bar(){var x = string.Format(""Hello {0}"", 5); }}",
                new ReplaceConcatenationWithStringFormat(),
                CanReplaceConcatOperatorAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_string_variable()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var s = ""42"";var x = ""Hello "" +  s;}}",
                @"""Hello "" +  s",
                @"using System;class Foo{void Bar(){var s = ""42"";var x = string.Format(""Hello {0}"", s); }}",
                new ReplaceConcatenationWithStringFormat(),
                CanReplaceConcatOperatorAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_object_variable()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){object i = 42;var x = ""Hello "" +  i;}}",
                @"""Hello "" +  i",
                @"using System;class Foo{void Bar(){object i = 42;var x = string.Format(""Hello {0}"", i); }}",
                new ReplaceConcatenationWithStringFormat(),
                CanReplaceConcatOperatorAnalyzer.UseFormatDescriptor).Wait();
        }

        [TestMethod]
        public void Variable_and_chars()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){string d = ""42"";var x = ""Hello"" + d + 'w' + 'o' + 'r' + 'l' + 'd';}}",
                @"""Hello"" + d + 'w' + 'o' + 'r' + 'l' + 'd'",
                @"using System;class Foo{void Bar(){string d = ""42"";var x = string.Format(""Hello{0}world"", d); }}",
                new ReplaceConcatenationWithStringFormat(),
                CanReplaceConcatOperatorAnalyzer.UseFormatDescriptor).Wait();
        }
    }
}
