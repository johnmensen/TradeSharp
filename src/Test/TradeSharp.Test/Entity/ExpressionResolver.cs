using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeSharp.Util;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    class NuExpressionResolver
    {
        [Test]
        public void BracketDisparity()
        {
            var resolver = new ExpressionResolver("(a + (b) * c");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 1);
            vars.Add("b", 1);
            vars.Add("c", 1);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsFalse(ret, resolver.Formula);
        }

        [Test]
        public void Comparison()
        {
            var resolver = new ExpressionResolver("a > b = c < d");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 1);
            vars.Add("b", 2);
            vars.Add("c", 4);
            vars.Add("d", 3);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            var expected = vars["a"] > vars["b"] == vars["c"] < vars["d"];
            Assert.AreEqual(expected ? 1 : 0, result, resolver.Formula);
        }

        [Test]
        public void Logical()
        {
            var resolver = new ExpressionResolver("a & b | c");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 1);
            vars.Add("b", 0);
            vars.Add("c", 1);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(1, result, resolver.Formula);
        }

        [Test]
        public void AdditionAndVarDisparity()
        {
            var resolver = new ExpressionResolver("a + b + c");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 1);
            vars.Add("b", 2);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsFalse(ret, resolver.Formula + " lack of args");
            vars.Add("c", 3);
            ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(vars["a"] + vars["b"] + vars["c"], result, resolver.Formula);
            vars.Add("d", 4);
            ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula + " excess of args");
        }

        [Test]
        public void SubtractionNegationAndSpacing()
        {
            var resolver = new ExpressionResolver("-a -  -b - c");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 1);
            vars.Add("b", 1);
            vars.Add("c", 1);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(-vars["a"] - -vars["b"] - vars["c"], result, resolver.Formula);
        }

        [Test]
        public void MultiplicationAndDivision()
        {
            var resolver = new ExpressionResolver("a * b / c");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 3);
            vars.Add("b", 4);
            vars.Add("c", 6);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(vars["a"] * vars["b"] / vars["c"], result, resolver.Formula);
        }

        [Test]
        public void Logarithmic()
        {
            var resolver = new ExpressionResolver("2.71 ^ ln(a) + 10 ^ lg(b)");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 2);
            vars.Add("b", 3);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(Math.Pow(2.71, Math.Log(vars["a"])) + Math.Pow(10, Math.Log10(vars["b"])), result, resolver.Formula);
        }

        [Test]
        public void Trigonometrical()
        {
            var resolver = new ExpressionResolver("sin(a) / cos(a) - tan(a)");
            double result;
            var vars = new Dictionary<string, double>();
            vars.Add("a", 0.3);
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(Math.Sin(vars["a"]) / Math.Cos(vars["a"]) - Math.Tan(vars["a"]), result, resolver.Formula);
        }

        [Test]
        public void SignAndNullReplacing()
        {
            var resolver = new ExpressionResolver("sign(a ?? 5)");
            double result;
            var vars = new Dictionary<string, double> {{"a", 0}};
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(Math.Sign(vars["a"] == 0 ? 5 : vars["a"]), result, resolver.Formula);
            vars["a"] = -1;
            ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);
            Assert.AreEqual(Math.Sign(vars["a"] == 0 ? 5 : vars["a"]), result, resolver.Formula);
        }

        [Test]
        public void LogicalExpression()
        {
            var expressionResult = new Dictionary<string, Cortege2<Dictionary<string, double>, double>>
                {
                    {
                        "~a", new Cortege2<Dictionary<string, double>, double>(new Dictionary<string, double>
                            {
                                {"a", 1}
                            }, 0)
                    },
                    {
                        "a&~b", new Cortege2<Dictionary<string, double>, double>(new Dictionary<string, double>
                            {
                                {"a", 1},
                                {"b", 0}
                            }, 1)
                    },
                    {
                        "~c!=~b", new Cortege2<Dictionary<string, double>, double>(new Dictionary<string, double>
                            {
                                {"b", 1},
                                {"c", 0}
                            }, 1)
                    },
                    {
                        "(a=~b)&({c!=a}||~c)", new Cortege2<Dictionary<string, double>, double>(new Dictionary<string, double>
                            {
                                {"a", 1},
                                {"b", 0},
                                {"c", 1} // not 0 therefore true
                            }, 0)
                    },
                    {
                        "(a = ~b) & ({c != a} || ~c)", new Cortege2<Dictionary<string, double>, double>(new Dictionary<string, double>
                            {
                                {"a", 1},
                                {"b", 0},
                                {"c", 0} // not 0 therefore true
                            }, 1)
                    },
                    {
                        "(_pi&1)=(b&b)", new Cortege2<Dictionary<string, double>, double>(new Dictionary<string, double>
                            {
                                {"b", 1},
                            }, 1)
                    }
                };

            foreach (var exRst in expressionResult)
            {
                var expression = exRst.Key;

                ExpressionResolver resolver;
                try
                {
                    resolver = new ExpressionResolver(expression);
                }
                catch (Exception ex)
                {
                    Assert.Fail("\"" + expression + "\" не распознано: " + ex);
                    continue;
                }

                var vars = exRst.Value.a;
                var expected = exRst.Value.b;
                try
                {
                    double result;
                    var rst = resolver.Calculate(vars, out result);
                    Assert.IsTrue(rst, expression + " is calculated");
                    Assert.AreEqual(expected, result, expression + " should be equal " + expected);
                }
                catch (Exception ex)
                {
                    Assert.Fail(expression + ": exception while calculate: " + ex);
                }
            }
        }

        [Test]
        public void TestIndexFunction()
        {
            var resolver = new ExpressionResolver("(close-close#40)*2");
            double result;
            var vars = new Dictionary<string, double>
                {
                    { "close", 1.35 },
                    { "close#40", 1.34 }
                };
            var ret = resolver.Calculate(vars, out result);
            Assert.IsTrue(ret, resolver.Formula);

            const double resultRight = (1.35 - 1.34)*2;
            var delta = Math.Abs(resultRight - result);
            Assert.Less(delta, 0.000001, "IndexFunction is calculated right");
        }
    }
}
