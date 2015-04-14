using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Robot.Robot;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Robot
{
    [TestFixture]
    public class TestIchimokuRobot
    {
        /// <summary>
        /// Объект, который позволяет тестировать роботов
        /// </summary>
        private RobotContextBacktest context;

        /// <summary>
        /// Экземпляр тестируемого работа
        /// </summary>
        private IchimokuRobot bot;


        [TestFixtureSetUp]
        public void InitContext()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            context = TestRobotUtil.GetRobotContextBacktest(new DateTime(2013, 9, 30), new DateTime(2014, 1, 30), currency : "IND");

            bot = new IchimokuRobot();
            bot.Graphics.Clear();
            bot.Graphics.Add(new Cortege2<string, BarSettings>("INDUSD", new BarSettings
            {
                StartMinute = 0,
                Intervals = new List<int> { 30 }
            }));
            bot.Initialize(context, CurrentProtectedContext.Instance);

            ExecutablePath.InitializeFake(string.Empty);            
        }

        [TestFixtureTearDown]
        public void TestTeardown()
        {
            ExecutablePath.Unitialize();
        }

        [SetUp]
        public void Setup()
        {
            bot.Initialize(context, CurrentProtectedContext.Instance);
            context.SubscribeRobot(bot);
            context.InitiateTest();
        }

        [TearDown]
        public void Teardown()
        {

        }

        /// <summary>
        /// Сверяем условия (которые рассчитал робот), при которых были заключены первая и вторая сделки, с 
        /// показаниями индикатора Ишимоку для этих же дат
        /// </summary>
        [Test]
        public void DealsDone()
        {
            #region "расчётные" значения, с которыми будут сравниваться состония робота 
            var dealData = new[]
                {
                    new
                        {
                            dealDate = new DateTime(2013, 10, 31, 17, 0, 0),
                            mainKijun = 1.3683d,
                            bid = 1.3636,
                            dayCandlePackerFirstCandle =
                                new CandleData(1.35280f, 1.35870f, 1.35170f, 1.35320f, new DateTime(2013, 10, 1),
                                               new DateTime(2013, 10, 2))
                        },
                    new
                        {
                            dealDate = new DateTime(2013, 11, 7, 18, 0, 0),
                            mainKijun = 1.34395d,
                            bid = 1.3368,
                            dayCandlePackerFirstCandle =
                                new CandleData(1.35630f, 1.35910f, 1.35420f, 1.35780f, new DateTime(2013, 10, 7),
                                               new DateTime(2013, 10, 8))
                        },
                    new
                        {
                            dealDate = new DateTime(2013, 11, 27, 0, 0, 0),
                            mainKijun = 1.3544d,
                            bid = 1.3561,
                            dayCandlePackerFirstCandle =
                                new CandleData(1.3799f, 1.3812f, 1.3797f, 1.3807f, new DateTime(2013, 10, 26),
                                               new DateTime(2013, 10, 27))
                        }
                };

            #endregion

            try
            {
                DateTime modelTime, realTime;
                while (true)
                {
                    if (context.MakeStep(out modelTime, out realTime)) break;

                    var calculatedDealValue = dealData.FirstOrDefault(x => x.dealDate == modelTime);
                    if (calculatedDealValue == null) continue;

                    Assert.IsTrue(calculatedDealValue.mainKijun.RoughCompares(bot.kijunMain, 0.0001),
                                  string.Format(
                                      "киджун основного тайм фрейма не совпал с расчётным. Свеча от {0}. Расчётный {1} / рассчитал робот {2}",
                                      modelTime, calculatedDealValue.mainKijun, bot.kijunMain));

                    var dayCandlePackerFirstCandle = bot.subQueue[0].First;
                    Assert.AreEqual(calculatedDealValue.dayCandlePackerFirstCandle.timeOpen,
                                    dayCandlePackerFirstCandle.timeOpen,
                                    string.Format(
                                        "не совпадает время открытия первой свечи из очереди (D1) для расчёта киджуна. Расчётный {0} / рассчитал робот {1}",
                                        calculatedDealValue.dayCandlePackerFirstCandle.timeOpen,
                                        dayCandlePackerFirstCandle.timeOpen));

                    Assert.AreEqual(calculatedDealValue.dayCandlePackerFirstCandle.timeClose,
                                    dayCandlePackerFirstCandle.timeClose,
                                    string.Format(
                                        "не совпадает время закрытия первой свечи из очереди (D1) для расчёта киджуна. Расчётный {0} / рассчитал робот {1}",
                                        calculatedDealValue.dayCandlePackerFirstCandle.timeClose,
                                        dayCandlePackerFirstCandle.timeClose));

                    Assert.IsTrue(
                        calculatedDealValue.dayCandlePackerFirstCandle.open.RoughCompares(
                            dayCandlePackerFirstCandle.open, 0.0001f),
                        string.Format(
                            "не совпадает цена открытия первой свечи из очереди (D1) для расчёта киджуна. Расчётный {0} / рассчитал робот {1}",
                            calculatedDealValue.dayCandlePackerFirstCandle.open, dayCandlePackerFirstCandle.open));

                    Assert.IsTrue(
                        calculatedDealValue.dayCandlePackerFirstCandle.low.RoughCompares(
                            dayCandlePackerFirstCandle.low, 0.0001f),
                        string.Format(
                            "не совпадает наименьшая цена первой свечи из очереди (D1) для расчёта киджуна. Расчётный {0} / рассчитал робот {1}",
                            calculatedDealValue.dayCandlePackerFirstCandle.low, dayCandlePackerFirstCandle.low));

                    Assert.IsTrue(
                        calculatedDealValue.dayCandlePackerFirstCandle.high.RoughCompares(
                            dayCandlePackerFirstCandle.high, 0.0001f),
                        string.Format(
                            "не совпадает наибольшая первой свечи из очереди (D1) для расчёта киджуна. Расчётный {0} / рассчитал робот {1}",
                            calculatedDealValue.dayCandlePackerFirstCandle.high, dayCandlePackerFirstCandle.high));


                    Assert.IsTrue(calculatedDealValue.bid.RoughCompares(bot.candleMainTimeFrame.close, 0.0001),
                                  string.Format(
                                      "объём сделки не совпадает с расчётным. Расчётный {0} / рассчитал робот {1}",
                                      calculatedDealValue.bid, bot.candleMainTimeFrame.close));
                }
            }
            finally
            {
                context.FinalizeTest();
            }
        }


        /// <summary>
        /// Сверяем условия (которые рассчитал робот), при которых была заключена вторая сделка, на их соответствие друг другу
        /// </summary>
        /// <remarks>
        /// Это, по большей части, регриссионнай тест.
        /// </remarks>
        [Test]
        public void AllDealsDone()
        {
            try
            {
                DateTime modelTime, realTime;
                while (true)
                {
                    if (context.MakeStep(out modelTime, out realTime)) break;

                    if (!bot.OpenedDealOnCurrentCandle) continue;
                    Assert.IsTrue(new[] {-1, 1}.Contains(bot.planning),
                                  string.Format("Сделка была заключена в {0}, но условие \"planning\" не выполнено",
                                                realTime));
                    Assert.IsFalse(
                        (bot.happenIntersection.Length < bot.happenIntersection.MaxQueueLength ||
                         bot.happenIntersection.All(x => false)),
                        string.Format("Сделка была заключена в {0}, но нет необходимого пересечения с облаком", realTime));
                    Assert.IsFalse((bot.planning > 0 && bot.candleMainTimeFrame.close - bot.kijunMain <= bot.K)
                                   || (bot.planning < 0 && bot.kijunMain - bot.candleMainTimeFrame.close <= bot.K),
                                   string.Format(
                                       "Сделка была заключена в {0}, но цена закрытия не отлична от Киджуна на K ценовых пунктов",
                                       realTime));
                }
            }
            finally
            {
                context.FinalizeTest();
            }
        }
    }
}