using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.RobotFarm.BL;

namespace TradeSharp.Test.Entity
{
    [TestFixture]
    public class RobotFarm
    {
        #region HttpForm

        private readonly List<string> queriesSrc = new List<string>
            {
                "Chrome",
                "IE"
            };

        private readonly List<string> queriesHttp = new List<string>
        {
            "------WebKitFormBoundaryOSJ8OW8mWjRDtnzw\r\n" +
            "Content-Disposition: form-data; name=\"uploadedFile\"; filename=\"renco_plus_channels.pxml\"\r\n" +
            "Content-Type: application/octet-stream\r\n" +
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<RobotsPortfolio>\r\n" +
            "<robot>\r\n" +
            "<Robot.StopLossPoints value=\"500\" />\r\n" +
            "<Robot.TakeProfitPoints value=\"500\" />\r\n" +
            "<Robot.BrickSize value=\"50\" />\r\n" +
            "<Robot.AutoBrickSize value=\"False\" />\r\n" +
            "<Robot.AutoScale value=\"1\" />\r\n" +
            "<Robot.AutosizePeriod value=\"30\" />\r\n" +
            "<Robot.VolatilityType value=\"Размах\" />\r\n" +
            "<Robot.TypeName value=\"Ренко\" />\r\n" +
            "<Robot.Magic value=\"2\" />\r\n" +
            "<Robot.MaxLeverage value=\"10\" />\r\n" +
            "<Robot.TradeVolume value=\"100000\" />\r\n" +
            "<Robot.TimeSeries value=\"USDCHF:H1\" />\r\n" +
            "<Robot.NewsChannels value=\"\" />\r\n" +
            "</robot>\r\n" +
            "</RobotsPortfolio>\r\n" +
            "------WebKitFormBoundaryOSJ8OW8mWjRDtnzw\r\n" +
            "Content-Disposition: form-data; name=\"accountId\"\r\n" +
            "\r\n" +
            "3\r\n" +
            "------WebKitFormBoundaryOSJ8OW8mWjRDtnzw--\r\n",

            "-----------------------------7dd3a023dc0864\r\n" +
            "Content-Disposition: form-data; name=\"uploadedFile\"; filename=\"renco_plus_channels.pxml\"\r\n" +
            "Content-Type: text/plain\r\n" +
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<RobotsPortfolio>\r\n" +
            "<robot>\r\n" +
            "<Robot.StopLossPoints value=\"500\" />\r\n" +
            "<Robot.TakeProfitPoints value=\"500\" />\r\n" +
            "<Robot.BrickSize value=\"50\" />\r\n" +
            "<Robot.AutoBrickSize value=\"False\" />\r\n" +
            "<Robot.AutoScale value=\"1\" />\r\n" +
            "<Robot.AutosizePeriod value=\"30\" />\r\n" +
            "<Robot.VolatilityType value=\"Размах\" />\r\n" +
            "<Robot.TypeName value=\"Ренко\" />\r\n" +
            "<Robot.Magic value=\"2\" />\r\n" +
            "<Robot.MaxLeverage value=\"10\" />\r\n" +
            "<Robot.TradeVolume value=\"100000\" />\r\n" +
            "<Robot.TimeSeries value=\"USDCHF:H1\" />\r\n" +
            "<Robot.NewsChannels value=\"\" />\r\n" +
            "</robot>\r\n" +
            "</RobotsPortfolio>\r\n" +
            "-----------------------------7dd3a023dc0864\r\n" +
            "Content-Disposition: form-data; name=\"accountId\"\r\n" +
            "\r\n" +
            "3\r\n" +
            "-----------------------------7dd3a023dc0864--\r\n"
        };
        #endregion

        private Dictionary<string, List<CandleDataBidAsk>> quotes;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var randomizer = new Random(0);
            // подготовить список котировок, не быез дырочек, с меняющимися во времени ценами
            // старт каждого списка - в свой момент
            quotes = new Dictionary<string, List<CandleDataBidAsk>>
                {
                    {"EURCAD", new List<CandleDataBidAsk>
                        {
                            new CandleDataBidAsk
                                {
                                    open = 1.345f,
                                    openAsk = 1.3455f,
                                    high = 1.345f,
                                    highAsk = 1.3455f,
                                    low = 1.345f,
                                    lowAsk = 1.3455f,
                                    close = 1.345f,
                                    closeAsk = 1.3455f,
                                    timeOpen = new DateTime(2013, 5, 1),
                                    timeClose = new DateTime(2013, 5, 1, 0, 0, 1)
                                }
                        }},
                    {"EURAUD", new List<CandleDataBidAsk>
                        {
                            new CandleDataBidAsk
                                {
                                    open = 1.015f,
                                    openAsk = 1.016f,
                                    high = 1.015f,
                                    highAsk = 1.016f,
                                    low = 1.015f,
                                    lowAsk = 1.016f,
                                    close = 1.015f,
                                    closeAsk = 1.016f,
                                    timeOpen = new DateTime(2013, 6, 3),
                                    timeClose = new DateTime(2013, 6, 3, 0, 0, 1)
                                }
                        }},
                    {"AUDUSD", new List<CandleDataBidAsk>
                        {
                            new CandleDataBidAsk
                                {
                                    open = 1.20f,
                                    openAsk = 1.2004f,
                                    high = 1.20f,
                                    highAsk = 1.2004f,
                                    low = 1.20f,
                                    lowAsk = 1.2004f,
                                    close = 1.20f,
                                    closeAsk = 1.2004f,
                                    timeOpen = new DateTime(2013, 6, 11),
                                    timeClose = new DateTime(2013, 6, 11, 0, 0, 1)
                                }
                        }}
                };
            var dateEnd = new DateTime(2013, 8, 5);
            const float spread = 0.00032f;
            foreach (var list in quotes.Values)
            {
                var date = list[0].timeClose.AddMinutes(1);
                var counter = 0;
                var bidStart = list[0].open;

                while (date < dateEnd)
                {
                    // липовая цена
                    var bid = bidStart + bidStart * 0.2f * (float)Math.Sin(counter++ / 20.0);
                    var close = bid + ((counter%2) - 0.5f) / 1000;
                    var high = Math.Max(bid, close) + spread / 2;
                    var low = Math.Min(bid, close) - spread / 2;

                    list.Add(new CandleDataBidAsk(new CandleData(bid, high, low, close, date, date.AddMinutes(1)), spread));

                    // делаю историю малость дырявой
                    var dateStep = randomizer.Next(1000) == 0 ? randomizer.Next(500) : 1;
                    date = date.AddMinutes(dateStep);
                }
            }
        }

        /// <summary>
        /// протестить "курсор" котировок - все ли читаются, в нужной ли последовательности
        /// (иначе, если подсунуть тухляк, роботы могут взбунтоваться)
        /// <image url="http://radical-foto.ru/fp/9172586dce384144bcbef8778e6df96f" scale="0,6" />
        /// </summary>
        [Test]
        public void TestHistoryTickerStreamCountSteps()
        {
            var stream = new HistoryTickerStream(quotes);
            var longestChain = quotes.Max(q => q.Value.Count);
            var maxSteps = longestChain * 2;
            var firstDate = quotes.Min(q => q.Value[0].timeClose);
            var lastDate = quotes.Max(q => q.Value[q.Value.Count - 1].timeClose);

            var countSteps = 0;
            var prevDate = default(DateTime);
            var firstIter = true;
            
            while (true)
            {
                string[] names;
                CandleDataBidAsk[] curQuotes;
                if (!stream.Step(out names, out curQuotes)) break;

                Assert.IsNotEmpty(names, "массив не пуст (names)?");
                Assert.IsNotEmpty(curQuotes, "массив не пуст (curQuotes)?");

                Assert.IsTrue(curQuotes.Select(q => q.timeClose).Distinct().Count() == 1, "все котировки одной даты?");
                prevDate = curQuotes[0].timeClose;

                Assert.Less(++countSteps, maxSteps, "не зациклились нафиг?");

                if (firstIter)
                {
                    firstIter = false;
                    Assert.AreEqual(firstDate, curQuotes[0].timeClose, "начали с самой первой котировки?");
                }
            }

            Assert.GreaterOrEqual(countSteps, longestChain, "не слишком мало шагов пройдено?");
            Assert.AreEqual(prevDate, lastDate, "добрались до последней котировки?");
        }

        /// <summary>
        /// на входе два запроса - из Chrome и Осла
        /// </summary>
        [Test]
        public void TestHttpRequestParser()
        {
            var index = 0;
            foreach (var query in queriesHttp)
            {
                var querySrc = queriesSrc[index++];

                var parser = new HttpRequestParser(query);
                Assert.AreEqual(parser.records.Count, 2, querySrc + ": all records are parsed");
                
                var doc = parser.records[0].GetXml();
                Assert.IsNotNull(doc, querySrc + ": doc is not null");
                Assert.IsNotNull(doc.DocumentElement, querySrc + ": doc is not empty");
                Assert.AreEqual("3", parser.records[1].FileData, querySrc + ": hidden is parsed OK");
            }
        }
    }
}
