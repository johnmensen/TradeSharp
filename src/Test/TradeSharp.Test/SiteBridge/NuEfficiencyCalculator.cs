using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Test.Moq;

namespace TradeSharp.Test.SiteBridge
{
    [TestFixture]
    class NuEfficiencyCalculator
    {
        public delegate RequestStatus GetHistoryOrdersDelegate(int? accountId, DateTime? startDate, out List<MarketOrder> orders);
        public delegate RequestStatus GetBalanceChangesDelegate(int accountId, DateTime? startTime, out List<BalanceChange> balanceChanges);
        public delegate RequestStatus GetMarketOrdersDelegate(int accountId, out List<MarketOrder> orders);
        public delegate List<TradeTicker> GetTickersDelegate(out long lotByGroupHash);

        #region Имена замоченных методов
        private string getHistoryOrdersName = string.Empty;
        private string getMarketOrdersName = string.Empty;
        private string getBalanceChangesName = string.Empty;
        #endregion

        #region делегаты
        /// <summary>
        /// Мок метода MarketOrders. Возвращает коллекцию данных
        /// </summary>
        private GetMarketOrdersDelegate getMarketOrdersFake;

        /// <summary>
        /// Мок метода HistoryOrders. Возвращает коллекцию данных
        /// </summary>
        private GetHistoryOrdersDelegate getHistoryOrdersFake;

        /// <summary>
        /// Мок метода BalanceChanges. Возвращает коллекцию данных
        /// </summary>
        private GetBalanceChangesDelegate getBalanceChangesFake;
        #endregion

        private const int AccountId = 1;

        private ITradeSharpAccount fakeTradeAccount;
        private DailyQuoteStorage dailyQuoteStorage;
        private EfficiencyCalculator efficiencyCalculator;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            #region Запоминиаем имена методов, которые будем мОчить разными реализациями в ходе тестов
            List<MarketOrder> voidOrdersList;
            List<BalanceChange> voidBalanceChangesList;
            getHistoryOrdersName = ProxyBuilder.GetMethodName<ITradeSharpAccount>(a => a.GetHistoryOrders(null, null, out voidOrdersList));
            getMarketOrdersName = ProxyBuilder.GetMethodName<ITradeSharpAccount>(a => a.GetMarketOrders(0, out voidOrdersList));
            getBalanceChangesName = ProxyBuilder.GetMethodName<ITradeSharpAccount>(a => a.GetBalanceChanges(0, null, out voidBalanceChangesList));
            #endregion


            fakeTradeAccount = ProxyBuilder.Instance.GetImplementer<ITradeSharpAccount>();
            TradeSharpAccount.Initialize(fakeTradeAccount);

            // Инициализируем словарь котировок
            dailyQuoteStorage = new DailyQuoteStorage();
            dailyQuoteStorage.InitializeFake(QuoteMaker.MakeQuotesForQuoteDailyStorage(null));

            SetupMockMethodImplementation();
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


        
        [Test]
        public void Calculate()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            ((IMockableProxy)fakeTradeAccount).IncludeMockMethod(getHistoryOrdersName, getHistoryOrdersFake);
            ((IMockableProxy)fakeTradeAccount).IncludeMockMethod(getMarketOrdersName, getMarketOrdersFake);
            ((IMockableProxy)fakeTradeAccount).IncludeMockMethod(getBalanceChangesName, getBalanceChangesFake);
            // ReSharper restore SuspiciousTypeConversion.Global

            var accEff = new AccountEfficiency(new PerformerStat
                {
                    Account = AccountId,
                    DepoCurrency = "USD"
                });

            //accEff.Statistics.Account = accountId;


            efficiencyCalculator = new EfficiencyCalculator(dailyQuoteStorage, new EquityCurveCalculator());
            efficiencyCalculator.Calculate(accEff);
        }

        [Test]
        public void CalculateProfitCoeffs()
        {
            var accEff = new AccountEfficiency(new PerformerStat
                {
                    Account = AccountId,
                    DepoCurrency = "USD"
                })
                {
                    listEquity = TestDataGenerator.GetEquityOnTime(),
                    listTransaction = TestDataGenerator.GetBalanceChange().Select(LinqToEntity.DecorateBalanceChange).ToList()
                };


            efficiencyCalculator = new EfficiencyCalculator(dailyQuoteStorage, new EquityCurveCalculator());
            efficiencyCalculator.CalculateProfitCoeffs(accEff);
        }

        //[Test]
        public void CalculateEquityCurveVoid()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            ((IMockableProxy)fakeTradeAccount).IncludeMockMethod(getHistoryOrdersName, getHistoryOrdersFake);
            ((IMockableProxy)fakeTradeAccount).IncludeMockMethod(getMarketOrdersName, getMarketOrdersFake);
            ((IMockableProxy)fakeTradeAccount).IncludeMockMethod(getBalanceChangesName, getBalanceChangesFake);
            // ReSharper restore SuspiciousTypeConversion.Global
            var dailyQuoteStorage = new DailyQuoteStorage();
            var curveCalculator = new EquityCurveCalculator();
            try
            {
                var calc = new EfficiencyCalculator(dailyQuoteStorage, curveCalculator);
                calc.Calculate(new AccountEfficiency(new PerformerStat
                {
                    Account = 0,
                    DepoCurrency = "USD"
                }));
                Assert.Fail("NuEfficiencyCalculator.Calculate() - не выбросил ArgumentException");
            }
            catch (ArgumentException) // Это не трогать
            {
            }

            /*
                ((IMockableProxy) fakeTradeAccount).IncludeMockMethod(getHistoryOrdersName, getHistoryOrdersFake);
                ((IMockableProxy) fakeTradeAccount).IncludeMockMethod(getMarketOrdersName, getMarketOrdersFake);
                ((IMockableProxy) fakeTradeAccount).IncludeMockMethod(getBalanceChangesName, getBalanceChangesFake);
                var accEff1 = new AccountEfficiency(new PerformerStat
                    {
                        Account = accountId,
                        DepoCurrency = "USD"
                    });
                var calc = new EfficiencyCalculator(dailyQuoteStorage, equityCurveCalculator);
                calc.Calculate(accEff1);
            
                ((IMockableProxy) fakeTradeAccount).IncludeMockMethod(getBalanceChangesName, getBalanceChangesVoidFake);
                EfficiencyCalculator.Calculate(accEff1);

                ((IMockableProxy) fakeTradeAccount).IncludeMockMethod(getHistoryOrdersName, getHistoryOrdersEmptyFake);
                ((IMockableProxy) fakeTradeAccount).IncludeMockMethod(getMarketOrdersName, getMarketOrdersEmptyFake);
                EfficiencyCalculator.Calculate(accEff1); 
             */
        }


        /// <summary>
        /// Метод содержит код инициализации тестовых данных. Логика тестирования находится в меотоде Calculate
        /// </summary>
        private void SetupMockMethodImplementation()
        {
            #region CalculateMethod
            getMarketOrdersFake = ((int id, out List<MarketOrder> orders) =>
            {
                orders = TestDataGenerator.GetOpenPosition().Select(LinqToEntity.DecorateOrder).ToList();
                return RequestStatus.OK;
            });

            getHistoryOrdersFake = ((int? id, DateTime? date, out List<MarketOrder> orders) =>
            {
                orders = TestDataGenerator.GetClosePosition().Select(LinqToEntity.DecorateOrder).ToList();             
                return RequestStatus.OK;
            });

            getBalanceChangesFake = ((int id, DateTime? time, out List<BalanceChange> balanceChanges) =>
            {
                balanceChanges = TestDataGenerator.GetBalanceChange().Select(LinqToEntity.DecorateBalanceChange).ToList();         
                return RequestStatus.OK;
            });
            #endregion
        }
    }
}
