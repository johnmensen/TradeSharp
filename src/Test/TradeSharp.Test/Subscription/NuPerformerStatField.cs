using System.Collections.Generic;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Subscription
{
    [TestFixture]
    public class NuPerformerStatField
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            Localizer.ResourceResolver = new MockResourceResolver();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Localizer.ResourceResolver = null;
        }

        [Test]
        public void TestParseSimpleFormula()
        {
            List<Cortege3<PerformerStatField, ExpressionOperator, double>> filters;
            PerformerStatField sortField;

            var rst = PerformerStatField.ParseSimpleFormula("Sharp*((Sub > 10)&(AL<5)&(DD<30))", out filters, out sortField);
            Assert.IsTrue(rst, "ParseSimpleFormula - таки должна распарситься");

            rst = PerformerStatField.ParseSimpleFormula("AYP", out filters, out sortField);
            Assert.IsTrue(rst, "ParseSimpleFormula - таки снова должна распарситься");
            Assert.AreEqual("AYP", sortField.ExpressionParamName, "Критерий сортировки должен быть AYP");

            rst = PerformerStatField.ParseSimpleFormula("(P>15)", out filters, out sortField);
            Assert.IsFalse(rst, "ParseSimpleFormula - не должна распарситься, нет фильтра");
        }

        [Test]
        public void TestGetFormulaHighlightedHtml()
        {
            var markup = PerformerStatField.GetFormulaHighlightedHtml("Sharp*((Sub > 10)&(AL<5)&(DD<30))");
            Assert.IsTrue(markup.Contains("title=\""), "GetFormulaHighlightedHtml() - должны быть гиперссылки с заголовками");

            markup = PerformerStatField.GetFormulaHighlightedHtml("((P>15)&(Sub>0)&(Eq>500)&(DD<20))*AYP");
            Assert.IsFalse(markup.Contains("aypp"), "GetFormulaHighlightedHtml() - некорректны (вложенные) гиперссылки");
        }
    }
}
