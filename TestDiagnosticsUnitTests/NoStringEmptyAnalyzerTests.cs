using BlackFox.Roslyn.TestDiagnostics.NoStringEmpty;
using NFluent;
using Xunit;
using TestDiagnosticsUnitTests.DiagnosticTestHelpers;

namespace TestDiagnosticsUnitTests
{
    public class NoStringEmptyAnalyzerTests
    {
        [Fact]
        public void No_diagnostic_on_empty_string()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = \"\";");
            Check.That(diagnostics).IsEmpty();
        }

        [Fact]
        public void Diagnostic_on_standard_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = String.Empty;");
            Check.That(diagnostics).HasSize(1);
        }

        [Fact]
        public void Diagnostic_on_namespace_qualified_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = System.String.Empty;");
            Check.That(diagnostics).HasSize(1);
        }

        [Fact]
        public void Diagnostic_on_fully_qualified_call()
        {
            var analyzer = new NoStringEmptyAnalyzer();
            var diagnostics = GetDiagnosticsInSimpleCode(analyzer, "var x = global::System.String.Empty;");
            Check.That(diagnostics).HasSize(1);
        }
    }
}
