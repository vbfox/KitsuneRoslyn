using BlackFox.Roslyn.Diagnostics.TestHelpers;
using BlackFox.Roslyn.Diagnostics.TestHelpers.DiagnosticTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.MethodCanBeMadeStatic
{
    [TestClass]
    public class MethodCanBeMadeStaticAnalyzerTests
    {
        [TestMethod]
        public void No_references_function()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X() { return 42;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public void Static_outside_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public int X() { return (int)Math.Sin(42);}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public void Internal_constant_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "const int ANSWER = 42;public int X() { return ANSWER;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public void Internal_static_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "static int answer = 42;public int X() { return answer;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public void Internal_instance_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "int answer = 42;public int X() { return answer;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Internal_instance_method()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "int answer = 42;int getAnswer() { return answer; } public int X() { return getAnswer();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Internal_static_method()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "static int getAnswer() { return 42; } public int X() { return getAnswer();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public void No_references_method()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public void X() { }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }
    }
}
