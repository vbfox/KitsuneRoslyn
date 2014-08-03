// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackFox.Roslyn.Diagnostics.TestHelpers.CodeFixTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackFox.Roslyn.Diagnostics.PropertyConversions
{
    [TestClass]
    public class PropertyCodeFixTests
    {
        [TestMethod]
        public void Get_return_constant_to_expression()
        {
            CheckSingleFix(
                @"class Foo{public int X { get { return 42; } }}",
                @"public int X { get { return 42; } }",
                @"class Foo{public int X => 42;}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorStatementToExpression);
        }

        [TestMethod]
        public void Get_return_constant_to_initializer()
        {
            CheckSingleFix(
                @"class Foo{public int X { get { return 42; } }}",
                @"public int X { get { return 42; } }",
                @"class Foo{public int X {get;} = 42;}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorStatementToInitializer);
        }

        [TestMethod]
        public void Get_return_expression_static_to_expression()
        {
            CheckSingleFix(
                @"class Foo{public static int x = 42; public int X { get { return System.Math.Sin(x); } }}",
                @"public int X { get { return System.Math.Sin(x); }",
                @"class Foo{public static int x = 42; public int X => System.Math.Sin(x); }}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorStatementToExpression);
        }

        [TestMethod]
        public void Get_return_expression_static_to_initializer()
        {
            CheckSingleFix(
                @"class Foo{public static int x = 42; public int X { get { return System.Math.Sin(x); } }}",
                @"public int X { get { return System.Math.Sin(x); }",
                @"class Foo{public static int x = 42; public int X {get;} = System.Math.Sin(x); }}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorStatementToInitializer);
        }

        [TestMethod]
        public void Get_return_expression_instance_to_expression()
        {
            CheckSingleFix(
                @"class Foo{public int x = 42; public int X { get { return System.Math.Sin(x); } }}",
                @"public int X { get { return System.Math.Sin(x); }",
                @"class Foo{public int x = 42; public int X => System.Math.Sin(x);}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorStatementToExpression);
        }

        [TestMethod]
        public void Expression_constant_to_statement()
        {
            CheckSingleFix(
                @"class Foo{public int X => 42;}",
                @"public int X => 42;",
                @"class Foo{
    public int X
    {
        get
        {
            return 42;
        }
    }
}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorExpressionToStatement);
        }

        [TestMethod]
        public void Expression_constant_to_initializer()
        {
            CheckSingleFix(
                @"class Foo{public int X => 42;}",
                @"public int X => 42;",
                @"class Foo{public int X { get; } = 42;}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorExpressionToInitializer);
        }

        [TestMethod]
        public void Expression_expression_static_to_statement()
        {
            CheckSingleFix(
                @"class Foo{public static int x = 42; public int SinX => System.Math.Sin(x);}",
                @"public int SinX => System.Math.Sin(x);",
                @"class Foo{public static int x = 42; public int SinX
    {
        get
        {
            return System.Math.Sin(x);
        }
    }
}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorExpressionToStatement);
        }

        [TestMethod]
        public void Expression_expression_static_to_initializer()
        {
            CheckSingleFix(
                @"class Foo{public static int x = 42; public int SinX => System.Math.Sin(x);}",
                @"public int SinX => Math.Sin(x);",
                @"class Foo{public static int x = 42; public int SinX {get;} = System.Math.Sin(x);}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorExpressionToInitializer);
        }

        [TestMethod]
        public void Expression_expression_instance_to_statement()
        {
            CheckSingleFix(
                @"class Foo{public int x = 42; public int SinX => System.Math.Sin(x);}",
                @"public int SinX => System.Math.Sin(x);",
                @"class Foo{public int x = 42; public int SinX
    {
        get
        {
            return System.Math.Sin(x);
        }
    }
}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorExpressionToStatement);
        }

        [TestMethod]
        public void Initializer_constant_to_statement()
        {
            CheckSingleFix(
                @"class Foo{public int X {get;} = 42;}",
                @"public int X {get;} = 42;",
                @"class Foo{
    public int X
    {
        get
        {
            return 42;
        }
    }
}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorInitializerToStatement);
        }

        [TestMethod]
        public void Initializer_constant_to_expression()
        {
            CheckSingleFix(
                @"class Foo{public int X {get;} = 42;}",
                @"public int X {get;} = 42;",
                @"class Foo{public int X => 42;}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorInitializerToExpression);
        }

        [TestMethod]
        public void Initializer_expression_to_statement()
        {
            CheckSingleFix(
                @"class Foo{public int X {get;} = System.Math.Sin(42);}",
                @"public int X {get;} = System.Math.Sin(42);",
                @"class Foo{
    public int X
    {
        get
        {
            return System.Math.Sin(42);
        }
    }
}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorInitializerToStatement);
        }

        [TestMethod]
        public void Initializer_expression_to_expression()
        {
            CheckSingleFix(
                @"class Foo{public int X {get;} = System.Math.Sin(42);}",
                @"public int X {get;} = System.Math.Sin(42);",
                @"class Foo{public int X => System.Math.Sin(42);}",
                new PropertyCodeFix(),
                PropertyAnalyzer.DescriptorInitializerToExpression);
        }
    }
}
