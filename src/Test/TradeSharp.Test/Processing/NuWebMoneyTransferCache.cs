using System;
using System.Configuration;
using System.IO;
using NUnit.Framework;
using TradeSharp.Processing.WebMoney.Server;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.Processing
{
    [TestFixture]
    public class NuWebMoneyTransferCache
    {
        private DateTime dateTimeNow = new DateTime(2014, 2, 5);

        [TestFixtureSetUp]
        public void InitTest()
        {
        }

        [SetUp]
        public void Init()
        {
            if (File.Exists(WebMoneySettings.Instance.FullFileName)) File.Delete(WebMoneySettings.Instance.FullFileName);
            WebMoneySettings.Instance.LastTransferId = null;
            WebMoneySettings.Instance.LastUpdateDate = null;

            MoqPaymentAccessor.successInitial = true;
        }


        [TestFixtureTearDown]
        public void TearDown()
        {
        }

        /// <summary>
        /// Проверяем как читаются и запоминаются свойства в разных ситуациях
        /// </summary>
        [Test]
        public void XmlPropertiesTest()
        {
            // Файл настроек отсутствует.
            var wmTransferCache = new WebMoneyTransferCache(1000, MoqPaymentAccessor.MakeMoq().Object);

            var testDateValue0 = wmTransferCache.LastRequestDate;
            var testIdValue0 = wmTransferCache.LastRequestTransferId;

            Assert.IsNull(testDateValue0);
            Assert.IsNull(testIdValue0);


            wmTransferCache.LastRequestDate = dateTimeNow;
            wmTransferCache.LastRequestTransferId = 5;

            var testDateValue1 = wmTransferCache.LastRequestDate;
            var testIdValue1 = wmTransferCache.LastRequestTransferId;

            Assert.AreEqual(dateTimeNow, testDateValue1);
            Assert.AreEqual(5, testIdValue1);

            //Файл настроек создан. Теперь попробуем из него прочитать данные новым экземпляром WebMoneyTransferCache

            var voidTransferCache = new WebMoneyTransferCache(1000, MoqPaymentAccessor.MakeMoq().Object);

            var testDateValue2 = voidTransferCache.LastRequestDate;
            var testIdValue2 = voidTransferCache.LastRequestTransferId;

            Assert.NotNull(testDateValue2 != null);
// ReSharper disable PossibleInvalidOperationException
            Assert.AreEqual(dateTimeNow.Date, testDateValue2.Value.Date);
// ReSharper restore PossibleInvalidOperationException
            Assert.AreEqual(dateTimeNow.Hour, testDateValue2.Value.Hour);
            Assert.AreEqual(dateTimeNow.Minute, testDateValue2.Value.Minute);
            Assert.AreEqual(dateTimeNow.Second, testDateValue2.Value.Second);
            Assert.AreEqual(5, testIdValue2);

            voidTransferCache.LastRequestDate = null;
            voidTransferCache.LastRequestTransferId = null;

            Assert.AreEqual(dateTimeNow.Date, testDateValue2.Value.Date);
            Assert.AreEqual(dateTimeNow.Hour, testDateValue2.Value.Hour);
            Assert.AreEqual(dateTimeNow.Minute, testDateValue2.Value.Minute);
            Assert.AreEqual(dateTimeNow.Second, testDateValue2.Value.Second);
            Assert.AreEqual(5, testIdValue2);
            
        }

        [Test]
        public void ConstructorFailTest()
        {
            MoqPaymentAccessor.successInitial = false;
            Assert.Throws<SettingsPropertyNotFoundException>(() => new WebMoneyTransferCache(100, MoqPaymentAccessor.MakeMoq().Object)); 
        }

        [Test]
        public void GetActualTransactionTest()
        {
            var wmTransferCache = new WebMoneyTransferCache(100, MoqPaymentAccessor.MakeMoq().Object);

            var r1 = wmTransferCache.GetActualTransaction(); //Первое обращение на 3 месяца назад 
            Assert.NotNull(r1);
            Assert.AreEqual(1, r1.Count);


            var r2 = wmTransferCache.GetActualTransaction(); // Второе обращение на 30 секунд назад
            Assert.NotNull(r2);
            Assert.AreEqual(0, r2.Count);


            MoqPaymentAccessor.AddNewTransfer(); //Поступил новый платёж за последние 30 секунд

            var r3 = wmTransferCache.GetActualTransaction(); // Второе обращение на 30 секунд назад
            Assert.NotNull(r3);
            Assert.AreEqual(1, r3.Count);
            

            wmTransferCache.LastRequestDate = dateTimeNow.AddMonths(-5); // Неадекватные дата и Id предыдущей транзакции зечисления - должен запросить за 3 месяца
            wmTransferCache.LastRequestTransferId = 5;
            var r4 = wmTransferCache.GetActualTransaction();
            Assert.NotNull(r4);
            Assert.AreEqual(2, r4.Count);

            // Неадекватные дата (она выправиться в логике "GetActualTransaction"), но адекватный Id предыдущего трансвера зачисления - 
            // должен выбрать все транзакции ПОСЛЕ указанной (948396383). На данный момент будет выбрана только одна 
            // т.к. всего транзакций на зачисление - две (первая указана - вторую вернёт).
            wmTransferCache.LastRequestDate = dateTimeNow.AddMonths(-5);
            wmTransferCache.LastRequestTransferId = 959076445;
            var r5 = wmTransferCache.GetActualTransaction();
            Assert.NotNull(r5);
            Assert.AreEqual(1, r5.Count);
            Assert.AreEqual(959076446, r5[0].Id);


            wmTransferCache.LastRequestDate = new DateTime(3000,1,1); // генерируем исключение
            var r6 = wmTransferCache.GetActualTransaction();
            Assert.IsNull(r6);


            wmTransferCache.LastRequestDate = new DateTime(2014, 1, 1, 0, 0, 0); // запрашиваем за дату в которой нет транзакций.
            var r7 = wmTransferCache.GetActualTransaction();
            Assert.NotNull(r7);
            Assert.AreEqual(0, r7.Count);
             
        }
    }
}
