using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class Performer
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
        public void TestPerformerCriteriaFunction()
        {
            PerformerCriteriaFunctionCollection.Instance.ResetCriterias();
            var defaultCrits = PerformerCriteriaFunctionCollection.Instance.criterias;
            Assert.IsNotEmpty(defaultCrits, "Есть критерии по-умолчанию");
            Assert.IsNotEmpty(PerformerCriteriaFunctionCollection.Instance.propertyByVariable,
                              "Заполнен словарь переменная - свойство");

            var critsToStore = defaultCrits.ToList();
            PerformerCriteriaFunction critA, critB, critC, critD;
            critsToStore.Add(critA = new PerformerCriteriaFunction
            {
                Function = "1-(3+(5-(7+11)))",
                Description = "!"
            });
            critsToStore.Add(critB = new PerformerCriteriaFunction
                {
                    Function = "AYP-(DD^2+ML*2)",
                    Description = "Некая сложная формула, в описании\r\nмного строк."
                });
            // эта не должна прочитаться
            critsToStore.Add(critC = new PerformerCriteriaFunction
            {
                Function = "MVP-sin(DD/5)",
                Description = "Некая весьма сложная формула, в описании\r\nмного строк."
            });
            // эта тоже не должна прочитаться
            critsToStore.Add(critD = new PerformerCriteriaFunction
            {
                Function = "AYP-)DD^2+ML*2)",
                Description = "!2"
            });
            PerformerCriteriaFunctionCollection.Instance.criterias = critsToStore;
            PerformerCriteriaFunctionCollection.Instance.SelectedFunction = critA;
            PerformerCriteriaFunctionCollection.Instance.WriteToFile();
            PerformerCriteriaFunctionCollection.Instance.ReadFromFile();

            Assert.AreEqual(defaultCrits.Count + 2, PerformerCriteriaFunctionCollection.Instance.criterias.Count, 
                "Прочитано верное количество формул");
            var countDefault =
                PerformerCriteriaFunctionCollection.Instance.criterias.Count(c => defaultCrits.Any(d => d.AreEqual(c)));
            Assert.AreEqual(defaultCrits.Count, countDefault, "Критерии по-умолчанию прочитаны");

            Assert.AreEqual(1, PerformerCriteriaFunctionCollection.Instance.criterias.Count(c => c.AreEqual(critA)), "Функция А прочитана");
            Assert.AreEqual(1, PerformerCriteriaFunctionCollection.Instance.criterias.Count(c => c.AreEqual(critB)), "Функция B прочитана");
            Assert.IsFalse(PerformerCriteriaFunctionCollection.Instance.criterias.Any(c => c.AreEqual(critC)), "Функция C не прочитана");
            Assert.IsFalse(PerformerCriteriaFunctionCollection.Instance.criterias.Any(c => c.AreEqual(critD)), "Функция D не прочитана");

            Assert.IsTrue(PerformerCriteriaFunctionCollection.Instance.SelectedFunction.AreEqual(critA),
                "Функция запомнена");
        }
    }
}
