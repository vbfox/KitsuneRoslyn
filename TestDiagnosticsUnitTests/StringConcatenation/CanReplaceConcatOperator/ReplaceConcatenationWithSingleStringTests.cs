// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.TestHelpers.CodeFixTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackFox.Roslyn.Diagnostics.StringConcatenation.CanReplaceConcatOperator
{
    [TestClass]
    public class ReplaceConcatenationWithSingleStringTests
    {
        [TestMethod]
        public void Two_string()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = ""Hello"" + ""World"";}}",
                @"""Hello"" + ""World""",
                @"using System;class Foo{void Bar(){var x = ""HelloWorld"";}}",
                new ReplaceConcatenationWithSingleString(),
                CanReplaceConcatOperatorAnalyzer.UseStringDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_char()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = ""Hello"" + 'W';}}",
                @"""Hello"" + 'W'",
                @"using System;class Foo{void Bar(){var x = ""HelloW"";}}",
                new ReplaceConcatenationWithSingleString(),
                CanReplaceConcatOperatorAnalyzer.UseStringDescriptor).Wait();
        }

        [TestMethod]
        public void String_and_null()
        {
            CheckSingleFixAsync(
                @"using System;class Foo{void Bar(){var x = ""Hello"" + null + ""World"";}}",
                @"""Hello"" + null + ""World""",
                @"using System;class Foo{void Bar(){var x = ""HelloWorld"";}}",
                new ReplaceConcatenationWithSingleString(),
                CanReplaceConcatOperatorAnalyzer.UseStringDescriptor).Wait();
        }
    }
}