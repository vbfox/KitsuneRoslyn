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
        public async void No_references_function()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer, "public int X() { return 42;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void Parameter_references_function()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer, "public int X(int answer) { return answer;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void Static_outside_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "public int X() { return (int)Math.Sin(42);}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void Internal_constant_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "const int ANSWER = 42;public int X() { return ANSWER;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void Internal_static_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "static int answer = 42;public int X() { return answer;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void Internal_instance_reference()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "int answer = 42;public int X() { return answer;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void This()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "public object X() { return this;}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Base()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "class FooBase { public object X() { return this; }} "
                + "class Foo : FooBase { public object Y() { return base.X();}}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Already_static()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "public static void X() { }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Virtual()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "public virtual void X() { }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Abstract()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "abstract class Foo{public abstract void X();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Override()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "class Base{public virtual void X() {}} class Foo : Base {public override void X();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Potential_name_confusion()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "class A { public static int X() { return 42; } } class B { public object X() { return this;} public int Y() { return A.X(); }}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void Interface_implementation()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "interface IFoo { void X(); } class Foo : IFoo {public void X(){}}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Interface_implementation_for_derived_class()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "interface IFoo { void X(); } class Base {public void X(){}}; class Foo : Base, IFoo {}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Interface_explicit_implementation()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsAsync(analyzer,
                "interface IFoo { void X(); } class Foo : IFoo {void IFoo.X(){}}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Internal_instance_method()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "int answer = 42;int getAnswer() { return answer; } public int X() { return getAnswer();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).IsEmpty();
        }

        [TestMethod]
        public async void Internal_static_method()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer,
                "static int getAnswer() { return 42; } public int X() { return getAnswer();}");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }

        [TestMethod]
        public async void No_references_method()
        {
            var analyzer = new MethodCanBeMadeStaticAnalyzer();
            var diagnostics = await GetDiagnosticsInClassLevelCodeAsync(analyzer, "public void X() { }");
            var ids = diagnostics.Select(d => d.Id);
            Check.That(ids).ContainExactlyAnyOrder(MethodCanBeMadeStaticAnalyzer.Id);
        }


    }
}
