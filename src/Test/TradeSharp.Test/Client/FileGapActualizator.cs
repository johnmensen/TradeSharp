using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using Moq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.QuoteHistory;
using TradeSharp.Test.Moq;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.Proxy.QuoteStorage;

namespace TradeSharp.Test.Client
{
    [TestFixture]
    public class NuFileGapActualizator
    {
        #region Тестовые данные

        private readonly string quoteFolder = ExecutablePath.ExecPath + "\\quotes";

        private const string Ticker = "EURCAD";
        private readonly DateTime timeHistStart = new DateTime(2013, 4, 1);
        private readonly DateTime timeNow = new DateTime(2013, 9, 27, 16, 7, 1);
        private readonly DateTime timeFileEnd = new DateTime(2013, 9, 23, 11, 0, 20);

        private DateSpan[] serverGaps;
        private DateSpan[] clientGaps;

        private Mock<IQuoteStorage> moq;

        #endregion

        private void MakeGaps()
        {
            serverGaps = new[]
            {
                new DateSpan(timeHistStart.AddMinutes(200), timeHistStart.AddMinutes(235)),
                new DateSpan(timeHistStart.AddMinutes(560), timeHistStart.AddMinutes(800)),
                new DateSpan(timeHistStart.AddMinutes(805), timeHistStart.AddMinutes(806))
            };

            clientGaps = new[]
            {
                new DateSpan(timeHistStart.AddMinutes(200), timeHistStart.AddMinutes(235)),
                new DateSpan(timeHistStart.AddMinutes(560), timeHistStart.AddMinutes(800)),
                new DateSpan(timeHistStart.AddMinutes(805), timeHistStart.AddMinutes(806)),
                new DateSpan(timeHistStart.AddMinutes(9040), timeHistStart.AddMinutes(9041)),
                new DateSpan(timeHistStart.AddMinutes(24040), timeHistStart.AddMinutes(28040))
            };
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            // Mock для словаря метаданных
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            MakeGaps();
            // подготовить список котировок для "клиента" и "сервера"
            var allCandles = new List<CandleData>();
            var clientCandles = new List<CandleData>();

            var index = 0;
            for (var time = timeHistStart; time <= timeNow; time = time.AddMinutes(1))
            {
                // проверить попадание в дырку на сервере
                if (serverGaps.Any(g => g.IsIn(time))) continue;

                if (DaysOff.Instance.IsDayOff(time)) continue;
                var o = (float) Math.Sin((index++)/15.0);
                var candle = new CandleData(o, o + 0.001f, o - 0.003f, o - 0.001f, time, time.AddMinutes(1));
                allCandles.Add(candle);
                if (time > timeFileEnd) continue;
                
                // проверить попадание в дырку на клиенте
                if (clientGaps.Any(g => g.IsIn(time))) continue;
                clientCandles.Add(candle);
            }

            // поместить его в "локальное хранилище"
            AtomCandleStorage.Instance.RewriteCandles(Ticker, clientCandles);

            // инициализировать хранилище котировок
            moq = MoqQuoteStorage.MakeMoq(new Dictionary<string, List<CandleData>>
                {
                    {Ticker, allCandles}
                });
            QuoteStorage.Initialize(moq.Object);
        }

        [Test]
        public void TestStickSmallGaps()
        {
            var gaps = clientGaps.Select(g => new GapInfo {start = g.start, end = g.end}).ToList();
            GapInfo.StickSmallGaps(ref gaps, 30);
            Assert.True(gaps.Count == 4, "StickSmallGaps: gaps not sticked, gaps.Count != 4");
            Assert.True(gaps[2].end == clientGaps[3].end, "StickSmallGaps: data error while sticking");
        }

        [Test]
        public void TestCreateGapList()
        {
            var gaps = clientGaps.Select(g => new GapInfo { start = g.start, end = g.end }).ToList();
            var groups = GapList.CreateGapList(gaps, 1);
            Assert.AreEqual(3, groups.Count, "CreateGapList: error grouping gaps, groups.Count != 3");
            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                Assert.AreNotEqual(0, groups[groupIndex].Gaps.Count, "CreateGapList: gap group empty");
            for (var groupIndex = 1; groupIndex < groups.Count; groupIndex++)
                Assert.Less(groups[groupIndex - 1].Gaps.Last().end, groups[groupIndex].Gaps.First().start, "CreateGapList: gap groups intersect each other");
            gaps.Clear();
            groups.ForEach(g => gaps.AddRange(g.Gaps));
            GapInfo.StickSmallGaps(ref gaps, 1);
            Assert.True(gaps.Count == clientGaps.Length, "CreateGapList: error sticking & returning to original");
        }

        [Test]
        public void TestGetGaps()
        {
            var candles = AtomCandleStorage.Instance.GetAllMinuteCandles(Ticker);
            var startTime = timeHistStart.AddMinutes(700);
            var endTime = timeHistStart.AddMinutes(26000);
            var gaps = QuoteCacheManager.GetGaps(candles, startTime, endTime);
            Assert.NotNull(gaps, "TestGetGaps: gaps == null");
            Assert.AreEqual(startTime, gaps[0].start, "TestGetGaps: head gap not correct");
            Assert.AreEqual(endTime, gaps[gaps.Count - 1].end, "TestGetGaps: tail gap not correct");

            // частные случаи
            // все свечки правее интервала запроса
            startTime = new DateTime(2013, 3, 29);
            endTime = startTime.AddMinutes(50);
            gaps = QuoteCacheManager.GetGaps(candles, startTime, endTime);
            Assert.AreEqual(1, gaps.Count, "TestGetGaps(2): gaps count != 1");
            Assert.AreEqual(startTime, gaps[0].start, "TestGetGaps(2): head gap not correct");
            Assert.AreEqual(endTime, gaps[gaps.Count - 1].end, "TestGetGaps(2): tail gap not correct");

            // все свечки левее интервала запроса
            startTime = new DateTime(2013, 10, 1);
            endTime = startTime.AddMinutes(50);
            gaps = QuoteCacheManager.GetGaps(candles, startTime, endTime);
            Assert.AreEqual(1, gaps.Count, "TestGetGaps(3): gaps count != 1");
            Assert.AreEqual(startTime, gaps[0].start, "TestGetGaps(3): head gap not correct");
            Assert.AreEqual(endTime, gaps[gaps.Count - 1].end, "TestGetGap(3)s: tail gap not correct");
        }

        [Test]
        public void TestLoadQuotesFromServer()
        {
            GapMap.Instance.Clear();
            // найти в истории гэпы
            var gaps = FileGapActualizator.VerifyTickerHistory(Ticker, timeHistStart, quoteFolder, null, timeNow);
            Assert.NotNull(gaps, "VerifyHistory: gaps is not null");
            Assert.Greater(gaps.Count, 0, "VerifyHistory: gaps is not empty");
            
            // подкачать котировки
            FileGapActualizator.FillGapsByTicker(Ticker, timeHistStart, gaps, quoteFolder, null,
                (s, list) => {});
            // сравнить количество свечек "на клиенте" и "на сервере"
            var clientCandles = AtomCandleStorage.Instance.GetAllMinuteCandles(Ticker);
            Assert.Greater(clientCandles.Count, 0, "VerifyHistory: client candles are loaded");

            var serverCandlesPacked = QuoteStorage.Instance.proxy.GetMinuteCandlesPacked(Ticker, timeHistStart, DateTime.Now);
            var serverCandles = serverCandlesPacked.GetCandles();
            var lastServerTime = serverCandles[serverCandles.Count - 1].timeOpen;
            var lastClientTime = clientCandles[clientCandles.Count - 1].timeOpen;
            Assert.AreEqual(lastServerTime, lastClientTime, "VerifyHistory: last candle is loaded");
            
            // убедиться в том, что дырок больше нет
            var gapsNew = FileGapActualizator.VerifyTickerHistory(Ticker, timeHistStart, quoteFolder, null, timeNow);
            var gapsNewThatIsNotOnServer = gapsNew.Count(g => !serverGaps.Any(sg => sg.AreEqual(g.start, g.end)));
            Assert.AreEqual(0, gapsNewThatIsNotOnServer, "VerifyHistory: no gaps left");

            // нет повторяющихся свечей
            var hasDouble = false;
            for (var i = 1; i < clientCandles.Count; i++)
            {
                if (clientCandles[i].timeOpen <= clientCandles[i - 1].timeOpen)
                {
                    hasDouble = true;
                    break;                    
                }
            }
            Assert.AreEqual(false, hasDouble, "VerifyHistory: no doubled candles");

            // повторно подкачать гэпы - убедиться, что новых обращений к серверу не было,
            // ибо вся необходимая история серверных гэпов сохранена
            FileGapActualizator.FillGapsByTicker(Ticker, timeHistStart, gapsNew, quoteFolder, null,
                (s, list) =>
                    {
                        Assert.Fail("VerifyHistory: no redundant calls to server");                        
                    });

            //moq.Verify(lw => lw.GetMinuteCandlesPacked(It.IsAny<string>(),
            //                                           It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            //           Times.Exactly(gaps.Count), "VerifyHistory: no redundant calls to server");
        }

        [Test]
        public void TestLoadQuotesFromServerFast()
        {
            var gaps = FileGapActualizator.VerifyTickerHistory(Ticker, timeHistStart, quoteFolder, null, timeNow);
            FileGapActualizator.FillGapsByTickerFast(Ticker, timeHistStart, timeNow, gaps, quoteFolder, null,
                                                     (s, list) =>
                                                         {
                                                             Console.WriteLine("--------------");
                                                             foreach (var gap in list)
                                                                 Console.WriteLine("Gap progress: {0}", gap);
                                                         });

            // закачка в целом успешна
            var clientCandles = AtomCandleStorage.Instance.GetAllMinuteCandles(Ticker);
            Assert.Greater(clientCandles.Count, 0, "TestLoadQuotesFromServerFast: client candles not loaded");

            var serverCandlesPacked = QuoteStorage.Instance.proxy.GetMinuteCandlesPacked(Ticker, timeHistStart, DateTime.Now);
            var serverCandles = serverCandlesPacked.GetCandles();
            var lastServerTime = serverCandles[serverCandles.Count - 1].timeOpen;
            var lastClientTime = clientCandles[clientCandles.Count - 1].timeOpen;
            Assert.AreEqual(lastServerTime, lastClientTime, "TestLoadQuotesFromServerFast: last candle not loaded");

            // сравнить количество свечек на клиенте и на сервере
            /*var maxIndex = Math.Min(clientCandles.Count, serverCandles.Count);
            for (var index = 0; index < maxIndex; index++)
            {
                Assert.AreEqual(clientCandles[index].timeOpen, serverCandles[index].timeOpen, "TestLoadQuotesFromServerFast: candles not equal");
            }
            Assert.GreaterOrEqual(clientCandles.Count, serverCandles.Count, "TestLoadQuotesFromServerFast: client candles lost");*/

            // убедиться в том, что дырок больше нет
            var gapsNew = FileGapActualizator.VerifyTickerHistory(Ticker, timeHistStart, quoteFolder, null, timeNow);
            var gapsNewThatIsNotOnServer = gapsNew.Count(g => !serverGaps.Any(sg => sg.AreEqual(g.start, g.end)));
            Assert.AreEqual(0, gapsNewThatIsNotOnServer, "TestLoadQuotesFromServerFast: gaps still found");

            // свечи упорядочены по возрастанию, дубликатов нет
            var hasDouble = false;
            for (var i = 1; i < clientCandles.Count; i++)
            {
                if (clientCandles[i].timeOpen <= clientCandles[i - 1].timeOpen)
                {
                    hasDouble = true;
                    break;
                }
            }
            Assert.AreEqual(false, hasDouble, "TestLoadQuotesFromServerFast: double candles found");

            // повторно подкачать гэпы - убедиться, что новых обращений к серверу не было
            FileGapActualizator.FillGapsByTickerFast(Ticker, timeHistStart, timeNow, gapsNew, quoteFolder, null,
                (s, list) =>
                {
                    Assert.Fail("VerifyHistory: redundant server call");
                });
        }
    }
}
