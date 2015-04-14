using System;
using System.Collections.Generic;
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
    public class TestRobotMA
    {
        /// <summary>
        /// Объект, который позволяет тестировать роботов
        /// </summary>
        private RobotContextBacktest context;

        /// <summary>
        /// Экземпляр тестируемого работа
        /// </summary>
        private RobotMA bot;

        /// <summary>
        /// Хранит состояние робота до следующей итерации
        /// </summary>
        private RobotMA.RobotMAInnerState specimanState;

        [TestFixtureSetUp]
        public void InitContext()
        {
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
            context = TestRobotUtil.GetRobotContextBacktest(new DateTime(2013, 12, 25), new DateTime(2013, 12, 25).AddDays(30));

            bot = new RobotMA
                {
                    RangeFastMA = 5,
                    RangeSlowMA = 15,
                    DeriveSiteFromHistory = true
                };
            bot.Graphics.Add(new Cortege2<string, BarSettings>("EURUSD", new BarSettings
                {
                    StartMinute = 0,
                    Intervals = new List<int> {30}
                }));
            bot.Initialize(context, CurrentProtectedContext.Instance);

            ExecutablePath.InitializeFake(string.Empty);
            context.SubscribeRobot(bot);
            context.InitiateTest();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ExecutablePath.Unitialize();
        }

        [Test]
        public void TradeTest()
        {
            //Список хранит время и состояние робота, на этот момент времени. Это состояние задаётся руками и берётся, например, из графика.
            var checkPoints = new List<Cortege2<DateTime, RobotMA.RobotMAInnerState>>
                {
                    
                    new Cortege2<DateTime, RobotMA.RobotMAInnerState>(
                        new DateTime(2013, 12, 26, 18, 30, 0), 
                        new RobotMA.RobotMAInnerState
                            {
                                maValueFast = 1.3688,
                                maValueSlow = 1.36891,
                                maDifSign = 0
                            }),         
                    new Cortege2<DateTime, RobotMA.RobotMAInnerState>(
                        new DateTime(2013, 12, 26, 21, 30, 0), 
                        new RobotMA.RobotMAInnerState
                            {
                                maValueFast = 1.3689,
                                maValueSlow = 1.36896,
                                maDifSign = 0
                            }),
                    new Cortege2<DateTime, RobotMA.RobotMAInnerState>(
                        new DateTime(2013, 12, 27, 19, 30, 0), 
                        new RobotMA.RobotMAInnerState
                            {
                                maValueFast = 1.38078,
                                maValueSlow = 1.38133,
                                maDifSign = 1
                            }),                            
                   new Cortege2<DateTime, RobotMA.RobotMAInnerState>(
                        new DateTime(2013, 12, 27, 23, 0, 0), 
                        new RobotMA.RobotMAInnerState
                            {
                                maValueFast = 1.37662,
                                maValueSlow = 1.37981,
                                maDifSign = 1
                            })
                            
                };
            var curCheckPointIndex = 0;
            
            try
            {
                while (true)
                {
                    DateTime modelTime, realTime;
                    if (context.MakeStep(out modelTime, out realTime)) break;
                                  
                    if (bot.debugAction != null)
                    {
                        bot.debugAction -= DebugAction;
                        curCheckPointIndex++;
                        if (curCheckPointIndex == checkPoints.Count)
                            curCheckPointIndex = -1;
                    }

                    if (curCheckPointIndex >= 0 && modelTime == checkPoints[curCheckPointIndex].a)
                    {
                        specimanState = checkPoints[curCheckPointIndex].b;
                        bot.debugAction += DebugAction;
                    }
                }
            }
            finally
            {
                context.FinalizeTest();
            }
        }

        /// <summary>
        /// По сути, вызывается в конце метода "OnQuotesReceived" у робота, а он в свою очередь вызывается внутри "context.MakeStep" (при тестировании)
        /// Параметры заполняются в "OnQuotesReceived" робота и заполняются актуальными значениями состояния робота.
        /// </summary>
        private void DebugAction(RobotMA.RobotMAInnerState robotMAInnerState, DateTime candleCloseTime)
        {
            if (specimanState == null)
                return;
            var areSame = specimanState.AreSame(robotMAInnerState);
            Assert.IsTrue(areSame, "Bot reaction is wrong");
        }
    }
}
