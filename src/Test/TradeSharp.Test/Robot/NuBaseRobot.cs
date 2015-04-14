using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Robot
{
    public class VoidRobot : BaseRobot
    {
        public override BaseRobot MakeCopy()
        {
            throw new NotImplementedException();
        }

        public override List<string> OnQuotesReceived(string[] names, CandleDataBidAsk[] quotes, bool isHistoryStartOff)
        {
            throw new NotImplementedException();
        }

        public int CalculateVolumeReCall(string ticker, decimal? calculateLeverage = null)
        {
            return CalculateVolume(ticker, calculateLeverage);
        }
    }


    /// <summary>
    /// тестируется функционал базового робота (через использование других роботов) 
    /// </summary>
    [TestFixture]
    public class NuBaseRobot
    {
        /// <summary>
        /// Объект, который позволяет тестировать роботов
        /// </summary>
        private RobotContextBacktest context;


        /// <summary>
        /// Экземпляр тестируемого работа
        /// </summary>
        private VoidRobot bot;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            context = TestRobotUtil.GetRobotContextBacktest(new DateTime(2013, 12, 25), new DateTime(2013, 12, 25).AddDays(30));

            bot = new VoidRobot();
            bot.Graphics.Add(new Cortege2<string, BarSettings>("EURUSD", new BarSettings
            {
                StartMinute = 0,
                Intervals = new List<int> { 30 }
            }));
            bot.Initialize(context, CurrentProtectedContext.Instance);

            ExecutablePath.InitializeFake(string.Empty);
            context.SubscribeRobot(bot);
            context.InitiateTest();

            // Инициализация дополнительных объектом, MOCK-и
            QuoteMaker.FillQuoteStorageWithDefaultValues();
        }


        [TestFixtureTearDown]
        public void TestTearDown()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CalculateVolumeTest()
        {
            // Проверяем метод в ситуации, когда FixedVolume не задан и объём сделки рассчитывается исходя из плеча
            bot.FixedVolume = null;
            var volume1 = bot.CalculateVolumeReCall("EURUSD");
            Assert.AreEqual(volume1, 30000, "Объем сделки не совпалает с рассчитанным значением (27040 и округлённого до 30000)");


            var volume2 = bot.CalculateVolumeReCall("EURUSD", 1);
            Assert.AreEqual(volume2, 140000, "Объем сделки не совпалает с рассчитанным значением (135200 и округлённого до 140000)");

            var volume3 = bot.CalculateVolumeReCall("EURUSD", 0);
            Assert.AreEqual(volume3, 0, "Объем сделки не совпалает с рассчитанным значением (0)");

            // Проверяем метод в ситуации, когда FixedVolume задан
            bot.FixedVolume = 30000;
            var volume4 = bot.CalculateVolumeReCall("EURUSD");
            Assert.AreEqual(volume4, bot.FixedVolume, "Объем сделки не совпалает с заданным значением (FixedVolume)");
        }
    }
}
