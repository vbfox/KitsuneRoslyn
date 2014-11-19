using BlackFox.Roslyn.Diagnostics.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    [TestClass]
    public class TernaryReplaceableTests
    {
        [TestMethod]
        public void Variable_declarations_different_values()
        {
            var a = CompileStatement(@"var a = ""Foo"";");
            var b = CompileStatement(@"var a = ""Bar"";");

            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            var d = diff.Single();
            Check.That(d.Item1).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(d.Item2).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(d.Item1.GetText().ToString()).IsEqualTo(@"""Foo""");
            Check.That(d.Item2.GetText().ToString()).IsEqualTo(@"""Bar""");
        }

        [TestMethod]
        public void Variable_declarations_different_variables()
        {
            var a = CompileStatement(@"var a = ""Foo"";");
            var b = CompileStatement(@"var b = ""Foo"";");

            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;

            Check.That(result).IsFalse();
        }

        [TestMethod]
        public void Set_property_different_value()
        {
            var a = CompileStatement(@"Console.BufferHeight = Console.CursorLeft;");
            var b = CompileStatement(@"Console.BufferHeight = Console.CursorSize;");

            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            var d = diff.Single();
            Check.That(d.Item1).IsInstanceOf<MemberAccessExpressionSyntax>();
            Check.That(d.Item2).IsInstanceOf<MemberAccessExpressionSyntax>();
            Check.That(d.Item1.GetText().ToString()).IsEqualTo(@"Console.CursorLeft");
            Check.That(d.Item2.GetText().ToString()).IsEqualTo(@"Console.CursorSize");
        }

        [TestMethod]
        public void Add_two_strings_one_diff()
        {
            var a = CompileStatement(@"var x = ""Foo"" + ""Bar"";");
            var b = CompileStatement(@"var x = ""Foo"" + ""Baz"";");


            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            var d = diff.Single();
            Check.That(d.Item1).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(d.Item2).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(d.Item1.GetText().ToString()).IsEqualTo(@"""Bar""");
            Check.That(d.Item2.GetText().ToString()).IsEqualTo(@"""Baz""");
        }

        [TestMethod]
        public void Add_two_variables_one_diff()
        {
            var a = CompileStatements(@"var i = 0;var j = 1;var x = i + j;")
                .ChildNodes().OfType<LocalDeclarationStatementSyntax>().Last();
            var b = CompileStatements(@"var i = 0;var k = 1;var x = i + k;")
                .ChildNodes().OfType<LocalDeclarationStatementSyntax>().Last();


            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            var d = diff.Single();
            Check.That(d.Item1).IsInstanceOf<IdentifierNameSyntax>();
            Check.That(d.Item2).IsInstanceOf<IdentifierNameSyntax>();
            Check.That(d.Item1.GetText().ToString()).IsEqualTo(@"j");
            Check.That(d.Item2.GetText().ToString()).IsEqualTo(@"k");
        }

        [TestMethod]
        public void Set_property_different_properties()
        {
            var a = CompileStatement(@"Console.BufferHeight = Console.CursorLeft;");
            var b = CompileStatement(@"Console.BufferWidth = Console.CursorLeft;");


            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsFalse();
        }

        [TestMethod]
        public void Method_call_different_arguments_string()
        {
            var a = CompileStatement(@"Console.WriteLine(""Foo"");");
            var b = CompileStatement(@"Console.WriteLine(""Bar"");");


            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            var d = diff.Single();
            Check.That(d.Item1).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(d.Item2).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(d.Item1.GetText().ToString()).IsEqualTo(@"""Foo""");
            Check.That(d.Item2.GetText().ToString()).IsEqualTo(@"""Bar""");
        }

        [TestMethod]
        public void Method_call_different_arguments_string_variables()
        {
            var a = CompileStatement<ExpressionStatementSyntax>(
                @"var a = ""Foo"";Console.WriteLine(a);");
            var b = CompileStatement<ExpressionStatementSyntax>(
                @"var b = ""Bar"";Console.WriteLine(b);");


            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            var d = diff.Single();
            Check.That(d.Item1).IsInstanceOf<IdentifierNameSyntax>();
            Check.That(d.Item2).IsInstanceOf<IdentifierNameSyntax>();
            Check.That(d.Item1.GetText().ToString()).IsEqualTo(@"a");
            Check.That(d.Item2.GetText().ToString()).IsEqualTo(@"b");
        }

        [TestMethod]
        public void Method_call_different_arguments_two_string_variables()
        {
            var a = CompileStatement<ExpressionStatementSyntax>(
                @"var a = ""Foo"";Console.WriteLine(a, ""XX"");");
            var b = CompileStatement<ExpressionStatementSyntax>(
                @"var b = ""Bar"";Console.WriteLine(""YY"", b);");

            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsTrue();
            Check.That(diff).HasSize(2);

            var diff1 = diff[0];
            Check.That(diff1.Item1).IsInstanceOf<IdentifierNameSyntax>();
            Check.That(diff1.Item2).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(diff1.Item1.GetText().ToString()).IsEqualTo(@"a");
            Check.That(diff1.Item2.GetText().ToString()).IsEqualTo(@"""YY""");

            var diff2 = diff[1];
            Check.That(diff2.Item1).IsInstanceOf<LiteralExpressionSyntax>();
            Check.That(diff2.Item2).IsInstanceOf<IdentifierNameSyntax>();
            Check.That(diff2.Item1.GetText().ToString()).IsEqualTo(@"""XX""");
            Check.That(diff2.Item2.GetText().ToString()).IsEqualTo(@"b");
        }

        [TestMethod]
        public void Method_call_different_methods()
        {
            var a = CompileStatement(@"Console.Write(""Foo"");");
            var b = CompileStatement(@"Console.WriteLine(""Foo"");");


            var replaceable = TernaryReplaceable.Find(a, b);
            var result = replaceable.IsReplaceable;
            var diff = replaceable.Differences;

            Check.That(result).IsFalse();
        }

        //---------------------------------------------------------------------------

        static BlockSyntax CompileStatements(string code)
        {
            var fullCode = string.Format("using System; namespace TestNamespace {{ public class TestClass {{ "
                +"public static void TestMethod() {{ {0} }} }} }}", code);

            var compilation = DiagnosticTestHelpers.CreateCompilation(fullCode);
            var tree = compilation.SyntaxTrees.Single();
            var root = tree.GetRoot();

            var method = root.ChildNodes().OfType<NamespaceDeclarationSyntax>().Single()
                .ChildNodes().OfType<ClassDeclarationSyntax>().Single()
                .ChildNodes().OfType<MethodDeclarationSyntax>().Single();

            return method.Body;
        }

        static SyntaxNode CompileStatement(string code)
        {
            return CompileStatement<SyntaxNode>(code);
        }

        static TSyntaxNode CompileStatement<TSyntaxNode>(string code)
            where TSyntaxNode : SyntaxNode
        {
            var block = CompileStatements(code);
            return block.ChildNodes().OfType<TSyntaxNode>().Single();
        }
    }
}
