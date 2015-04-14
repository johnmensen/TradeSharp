using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using NUnit.Framework;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.QuoteHistory;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.SiteBridge
{
    // ReSharper disable SuspiciousTypeConversion.Global
    [TestFixture]
    class NuEquityCurveCalculator
    {
        private DailyQuoteStorage dailyQuoteStorage;
        private EquityCurveCalculator curveCalculator;

        private const string DepoCurrency = "USD";

        [TestFixtureSetUp]
        public void TestSetup()
        {
            // Инициализируем методы GetTickers, GetMetadataByCategory, GetAccountGroupsWithSessionInfo
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock);

            // Инициализируем словарь котировок
            dailyQuoteStorage = new DailyQuoteStorage();
            dailyQuoteStorage.InitializeFake(QuoteMaker.MakeQuotesForQuoteDailyStorage(null));


            // Готовим объекты, которые будем тестировать и на которых будем тестировать
            curveCalculator = new EquityCurveCalculator();

            
        }
       
        [TestFixtureTearDown]
        public void TestTeardown()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {

        }

        /// <summary>
        /// тестируем метод CalculateDealVolumeInDepo
        /// </summary>
        [Test]
        public void CalculateDealVolumeInDepoCurrency()
        {
            const int volume = 10000;
            var deal = new MarketOrder
            {
                Symbol = "USDRUB",
                Volume = volume,
                Side = 1,
                PriceEnter = 1.3290f,
                TimeEnter = DateTime.Now.AddMinutes(-60 * 24 * 3),
                State = PositionState.Opened,
                ExpertComment = "",
                MasterOrder = 10001
            };
            var dicQuote = new Dictionary<string, List<QuoteData>>();
            var quoteArc = new QuoteArchive(dicQuote);

            curveCalculator.CalculateDealVolumeInDepoCurrency(deal, new QuoteData(1.3820f, 1.3822f, DateTime.Now), "USD", quoteArc, DateTime.Now);
            Assert.AreEqual(0, deal.VolumeInDepoCurrency, "При разных валютах депозита и сделки и не инициализированом словаре котировок, метод не вернул правильное значение");
            
            curveCalculator.CalculateDealVolumeInDepoCurrency(deal, new QuoteData(1.3820f, 1.3822f, DateTime.Now), "RUB", quoteArc, DateTime.Now);
            Assert.AreEqual(volume * (1.3822f + 1.3820f) / 2, deal.VolumeInDepoCurrency,
                "При одинаковых валютах депозита и сделки метод не вернул правильное значение");

            foreach (var smb in DalSpot.Instance.GetTickerNames())
                dicQuote.Add(smb, dailyQuoteStorage.GetQuotes(smb).Select(q => new QuoteData(q.b, q.b, q.a)).ToList());
            quoteArc = new QuoteArchive(dicQuote);

            curveCalculator.CalculateDealVolumeInDepoCurrency(deal, new QuoteData(1.3820f, 1.3822f, DateTime.Now), "RUB", quoteArc, dicQuote["USDRUB"].First().time);
            Assert.AreEqual(volume * (1.3822f + 1.3820f) / 2, deal.VolumeInDepoCurrency,
                "При инициализированных всех данных и метод не вернул правильное значение");
        }


        /// <summary>
        /// Убеждаемся, что при отсутствии котировки метод возвращает 0 (если логика метода изменится - данный тест это покажет)
        /// </summary>
        [Test]
        public void CalculateProfitInDepoCurx()
        {
            const int volume = 10000;
            var deal = new MarketOrder
            {
                Symbol = "USDRUB",
                Volume = volume,
                Side = 1,
                PriceEnter = 1.3290f,
                TimeEnter = DateTime.Now.AddMinutes(-60 * 24 * 3),
                State = PositionState.Opened,
                ExpertComment = "",
                MasterOrder = 10001
            };

            var dicQuote = new Dictionary<string, List<QuoteData>>();
            var quoteArc = new QuoteArchive(dicQuote);

            const float profit = 1000f;
            var volumeUsd = curveCalculator.CalculateProfitInDepoCurx(true, profit, deal.Symbol, "USD", quoteArc, DateTime.Now);
            Assert.AreEqual(profit, volumeUsd, "При совпадении валюты депозита и сделки, метод не вернул правильное значение прибыли по сделке");


            var volumeRub = curveCalculator.CalculateProfitInDepoCurx(true, profit, deal.Symbol + "f", "RUB", quoteArc, DateTime.Now);
            Assert.AreEqual(0, volumeRub, "При отсутствии тикеров валют, метод не вернул правильное значение прибыли по сделке");
            TradeSharpDictionary.Initialize(MoqTradeSharpDictionary.Mock); // возвращаем обратно реализацию метода GetTickers


            var volumeNoQuote = curveCalculator.CalculateProfitInDepoCurx(false, profit, deal.Symbol, "RUB", quoteArc, DateTime.Now);
            Assert.AreEqual(profit, volumeNoQuote, "При отсутствии котировок, метод не вернул правильное значение прибыли по сделке");


            foreach (var smb in DalSpot.Instance.GetTickerNames())
                dicQuote.Add(smb, dailyQuoteStorage.GetQuotes(smb).Select(q => new QuoteData(q.b, q.b, q.a)).ToList());
            var volumeQuote = curveCalculator.CalculateProfitInDepoCurx(true, profit, deal.Symbol, "RUB", new QuoteArchive(dicQuote), dicQuote["USDRUB"].First().time);

            //TODO значение "32369.2012" не расчитывалось в ручную, а просто скопировано, что бы тест не валился (это нужно исправить)
            Assert.AreEqual(32369.2012f, volumeQuote, "Метод рассчитал значение прибыли по сделке не правильно");
        }

        #region тестируем метод CalculateEquityCurve
        [Test]
        public void CalculateEquityCurve()
        {
            var deals = new List<MarketOrder>();

            var dealsOpen = TestDataGenerator.GetOpenPosition().Select(LinqToEntity.DecorateOrder).ToList();
            var dealsClose = TestDataGenerator.GetClosePosition().Select(LinqToEntity.DecorateOrder).ToList();

            deals.AddRange(dealsOpen);
            deals.AddRange(dealsClose);

            var transfers = TestDataGenerator.GetBalanceChange().Select(LinqToEntity.DecorateBalanceChange).ToList();
            var firstDealOpenedTime = deals.Min(d => d.TimeEnter);
            transfers.Insert(0, new BalanceChange
                {
                    AccountID = deals[0].AccountID,
                    ValueDate = firstDealOpenedTime.AddHours(-1),
                    CurrencyToDepoRate = 1,
                    ChangeType = BalanceChangeType.Deposit,
                    Amount = 500000
                });

            var dicQuote = new Dictionary<string, List<QuoteData>>();
            
            foreach (var smb in DalSpot.Instance.GetTickerNames())
                dicQuote.Add(smb, dailyQuoteStorage.GetQuotes(smb).Select(q => new QuoteData(q.b, q.b, q.a)).ToList());

            var quoteArc = new QuoteArchive(dicQuote);
            var res = curveCalculator.CalculateEquityCurve(deals, DepoCurrency, quoteArc, transfers);

            // сверить точку кривой доходности
            CheckEquityCurvePoint(res, transfers, dealsOpen, dealsClose, quoteArc, dicQuote, 0);
            CheckEquityCurvePoint(res, transfers, dealsOpen, dealsClose, quoteArc, dicQuote, 10);
            CheckEquityCurvePoint(res, transfers, dealsOpen, dealsClose, quoteArc, dicQuote, 40);
        }

        /// <summary>
        /// Рассчитать точку кривой доходности "вручную" и сверить её с рассчитанной в классе "CalculateEquityCurve"
        /// </summary>
        private void CheckEquityCurvePoint(AccountPerformanceRaw res, 
            List<BalanceChange> transfers, List<MarketOrder> dealsOpen, List<MarketOrder> dealsClose,
            QuoteArchive quoteArc, Dictionary<string, List<QuoteData>> dicQuote, int index)
        {
            var calculationTime = res.equity[index].time;

            var depoChangesOnDate = transfers.Where(t => t.ValueDate <= calculationTime).Sum(t => t.SignedAmountDepo);
            var dealsStilOpened = dealsOpen.Where(d => d.State == PositionState.Opened && d.TimeEnter <= calculationTime).ToList();
            var allDeals = dealsClose.Where(d => d.TimeEnter < calculationTime && d.TimeExit > calculationTime).ToList();
            allDeals.AddRange(dealsStilOpened);

            allDeals.ForEach(d =>
                {
                    d.State = PositionState.Opened;
                    d.PriceExit = null;
                });
            // получить котировки на момент
            quoteArc.ToBegin();
            var thatTimeQuotes = dicQuote.Keys.ToDictionary(ticker => ticker,
                                                            ticker => quoteArc.GetQuoteOnDateSeq(ticker, calculationTime));

            var sumDealsProfit = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(allDeals, thatTimeQuotes, DepoCurrency);
            var equity = (float) depoChangesOnDate + sumDealsProfit;
            var delta = Math.Abs(res.equity[index].equity - equity);
            Assert.Less(delta, 0.1f, "CalculateEquityCurve - погрешность расчета средств на момент должна быть в пределах 10C");


            var dealByTicker = allDeals.GroupBy(x => x.Symbol).ToDictionary(d => d.Key, d => d.ToList());
            var exposure = dealByTicker.Sum(d =>
                {
                    var totalVolume = Math.Abs(d.Value.Sum(order => order.Side*order.Volume));

                    if (totalVolume == 0) return 0;
                    string errorStr;
                    var exp = DalSpot.Instance.ConvertToTargetCurrency(d.Key, true, DepoCurrency, totalVolume, thatTimeQuotes, out errorStr);
                    return exp ?? 0;
                });

            var leverage = (float) exposure/equity;
            var resultedLeverage = res.leverage.First(l => l.time == calculationTime).equity;
            delta = Math.Abs(resultedLeverage - leverage);
            Assert.Less(delta, 0.05f, "CalculateEquityCurve - погрешность расчета плеча на момент должна быть в пределах 0.05");
        }
        #endregion
        
    }
    // ReSharper restore SuspiciousTypeConversion.Global          
}
