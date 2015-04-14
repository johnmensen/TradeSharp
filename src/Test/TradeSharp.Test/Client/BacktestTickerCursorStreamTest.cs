using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BacktestServerProxy;
using TradeSharp.Test.Moq;
using TradeSharp.Util;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class BacktestTickerCursorStreamTest
    {
        private string quoteFolder;

        [TestFixtureSetUp]
        public void Setup()
        {
            ExecutablePath.InitializeFake(string.Empty);
            quoteFolder = ExecutablePath.ExecPath + "\\quotes";
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);
        }

        [Test]
        public void TestSetupCursor()
        {
            var stream = new BacktestTickerCursorStream(quoteFolder, "INDUSD");
            Assert.IsNotNull(stream.currentCandle, "TestSetupCursor() - should read candle");
            Assert.IsNotNull(stream.nextCandle, "TestSetupCursor() - should read next candle");
        }

        [Test]
        public void TestMoveToTime()
        {
            var stream = new BacktestTickerCursorStream(quoteFolder, "INDUSD");
            var time = stream.currentCandle.timeClose.AddMinutes(30);
            CandleData candle;
            var moveResult = stream.MoveToTime(time, out candle);
            Assert.IsTrue(moveResult, "TestMoveToTime() - should step on time");
            Assert.IsNotNull(candle, "TestMoveToTime() - should return candle");
            Assert.AreEqual(candle.timeClose, time, "TestMoveToTime() - should step on the right time");

            time = time.AddMinutes(-20);
            moveResult = stream.MoveToTime(time, out candle);
            Assert.IsTrue(moveResult, "TestMoveToTime() - should step on time (rewind)");
            Assert.IsNotNull(candle, "TestMoveToTime() - should return candle after rewind");
            Assert.AreEqual(candle.timeClose, time, "TestMoveToTime() - should step on the right time after rewind");
        }
    }
}
