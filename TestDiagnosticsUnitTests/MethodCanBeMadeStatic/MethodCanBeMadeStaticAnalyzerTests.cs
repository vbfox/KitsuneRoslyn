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
        public void Parameter_references_function()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer, "public int X(int answer) { return answer;}");
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
        public void This()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public object X() { return this;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Already_static()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public static void X() { }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Virtual()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnosticsInClassLevelCode(analyzer,
                "public virtual void X() { }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Abstract()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "abstract class Foo{public abstract void X();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Override()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "class Base{public virtual void X() {}} class Foo : Base {public override void X();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Interface_implementation()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "interface IFoo { void X(); } class Foo : IFoo {public void X(){}}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        //[Ignore]
        public void Interface_implementation_for_derived_class()
        {
            // Ignored for now
            // Can't work without some compilation related analyzer and the only one in the current code base
            // "ICompilationEndedAnalyzer" is never invoked by VS. 

            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "interface IFoo { void X(); } class Base {public void X(){}}; class Foo : Base, IFoo {}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public void Interface_explicit_implementation()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = GetDiagnostics(analyzer,
                "interface IFoo { void X(); } class Foo : IFoo {void IFoo.X(){}}");
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
