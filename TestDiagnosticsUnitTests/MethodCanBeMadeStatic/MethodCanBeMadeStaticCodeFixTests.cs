using BlackFox.Roslyn.Diagnostics.TestHelpers.CodeFixTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackFox.Roslyn.Diagnostics.MethodCanBeMadeStatic
{
    [TestClass]
    public class MethodCanBeMadeStaticCodeFixTests
    {
        [TestMethod]
        public void No_references_function()
        {
            CheckSingleFix(
                "class Foo{public int X() { return 42;}}",
                "X",
                "class Foo{public static int X() { return 42;}}",
                "Make method static",
                new MethodCanBeMadeStaticCodeFix(),
                MethodCanBeMadeStaticAnalyzer.Descriptor);
        }

        [TestMethod]
        public void Static_outside_reference()
        {
            CheckSingleFix(
                "class Foo{public int X() { return (int)Math.Sin(42);}}",
                "X",
                "class Foo{public static int X() { return (int)Math.Sin(42);}}",
                "Make method static",
                new MethodCanBeMadeStaticCodeFix(),
                MethodCanBeMadeStaticAnalyzer.Descriptor);
        }

        [TestMethod]
        public void Internal_constant_reference()
        {
            CheckSingleFix(
                "class Foo{const int ANSWER = 42;public int X() { return ANSWER;}}",
                "X",
                "class Foo{const int ANSWER = 42;public static int X() { return ANSWER;}}",
                "Make method static",
                new MethodCanBeMadeStaticCodeFix(),
                MethodCanBeMadeStaticAnalyzer.Descriptor);
        }

        [TestMethod]
        public void Internal_static_reference()
        {
            CheckSingleFix(
                "class Foo{static int answer = 42;public int X() { return answer;}}",
                "X",
                "class Foo{static int answer = 42;public static int X() { return answer;}}",
                "Make method static",
                new MethodCanBeMadeStaticCodeFix(),
                MethodCanBeMadeStaticAnalyzer.Descriptor);
        }

        [TestMethod]
        public void Internal_static_method()
        {
            CheckSingleFix(
                "class Foo{static int getAnswer() { return 42; } public int X() { return getAnswer();}}",
                "X",
                "class Foo{static int getAnswer() { return 42; } public static int X() { return getAnswer();}}",
                "Make method static",
                new MethodCanBeMadeStaticCodeFix(),
                MethodCanBeMadeStaticAnalyzer.Descriptor);
        }

        [TestMethod]
        public void No_references_method()
        {
            CheckSingleFix(
                "class Foo{public void X() { }}",
                "X",
                "class Foo{public static void X() { }}",
                "Make method static",
                new MethodCanBeMadeStaticCodeFix(),
                MethodCanBeMadeStaticAnalyzer.Descriptor);
        }
    }
}
