using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.TradeLib
{
    public class ProfitCalculator : IProfitCalculator
    {
        private static readonly Lazy<IProfitCalculator> instance = new Lazy<IProfitCalculator>(() => new ProfitCalculator());

        public static IProfitCalculator Instance
        {
            get { return instance.Value; }
        }

        private readonly FloodSafeLogger floodSafeLogger = new FloodSafeLogger(2000);
        
        private const int MsgNoQuoteMagic = 1;
        
        private const int MsgNoQuoteAtAllMagic = 2;
        
        private readonly DateTime timeStarted;
        
        private const int MinMillsToReportError = 1000 * 30;

        private ProfitCalculator()
        {
            timeStarted = DateTime.Now;
        }

        public decimal CalculateAccountEquity(int accountId, decimal balance, string depoCurx,
            Dictionary<string, QuoteData> curPrices, ITradeSharpAccount proxyAccount)
        {
            var equity = balance;
            List<MarketOrder> posList;
            var result = proxyAccount.GetMarketOrders(accountId, out posList);
            if (result != RequestStatus.OK)
            {
                Logger.ErrorFormat("Ошибка в CalculateAccountEquity({0}) - ошибка получения позиций", accountId);
                return 0;
            }

            equity += posList.Sum(p =>
                p.State != PositionState.Opened ? 0
                : CalculatePositionProfit(p, depoCurx, curPrices));

            return equity;
        }

        public decimal CalculatePositionProfit(MarketOrder pos, string depoCurx, Dictionary<string, QuoteData> curPrices)
        {
            // получить текущую котиру по валютной паре сделки            
            if (!curPrices.ContainsKey(pos.Symbol))
            {
                if ((DateTime.Now - timeStarted).TotalMilliseconds > MinMillsToReportError)
                    floodSafeLogger.LogMessageFormatCheckFlood(LogEntryType.Error, MsgNoQuoteAtAllMagic,
                        1000 * 60 * 60, "Ошибка в CalculatePositionProfit: пара {0} не найдена", pos.Symbol);
                return 0;
            }
            var curPrice = curPrices[pos.Symbol];

            // получить прибыль в контрвалюте
            var profitCounter = (decimal)(pos.Side > 0
                                    ? curPrice.bid - pos.PriceEnter
                                    : pos.PriceEnter - curPrice.ask);
            profitCounter *= pos.Volume;
            // пересчитать в валюту депо

            string errorStr;
            var profitDepo = DalSpot.Instance.ConvertToTargetCurrency(pos.Symbol, false, depoCurx,
                (double)profitCounter, curPrices, out errorStr, false);
            if (!profitDepo.HasValue)
            {
                if ((DateTime.Now - timeStarted).TotalMilliseconds > MinMillsToReportError)
                    floodSafeLogger.LogMessageFormatCheckFlood(LogEntryType.Debug,
                        MsgNoQuoteMagic, 1000 * 60 * 10,
                        "Ошибка в CalculatePositionProfit: ConvertCounter2Depo ({0}, {1}, {2:f2}) не найдена",
                        pos.Symbol, depoCurx, profitCounter);
                return 0;
            }

            return profitDepo.Value;
        }

        public Dictionary<string, decimal> CalculateAccountExposure(int accountId,
            out decimal equity,
            out decimal reservedMargin,
            out decimal exposure,
            Dictionary<string, QuoteData> curPrices,
            ITradeSharpAccount proxyAccount,
            Func<int, AccountGroup> getAccountGroup)
        {
            if (curPrices == null)
            {
                Logger.Error("Ошибка в CalculateAccountExposure - curPrices == null");
                throw new Exception("Ошибка в CalculateAccountExposure - curPrices == null");
            }

            equity = 0;
            exposure = 0;
            reservedMargin = 0;
            var exps = new Dictionary<string, decimal>();
            Account account;
            proxyAccount.GetAccountInfo(accountId, true, out account);
            if (account == null) return exps;

            equity = account.Equity;
            List<MarketOrder> marketOrders;
            try
            {
                var result = proxyAccount.GetMarketOrders(accountId, out marketOrders);
                if (result != RequestStatus.OK) return exps;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CalculateAccountExposure - GetMarketOrders", ex);
                throw;
            }
            if (marketOrders == null)
            {
                Logger.Error("Ошибка в CalculateAccountExposure - posInf == null");
                throw new Exception("Ошибка в CalculateAccountExposure - posInf == null");
            }

            foreach (var position in marketOrders)
            {
                var volume = position.Volume * position.Side;
                if (exps.ContainsKey(position.Symbol)) exps[position.Symbol] += volume;
                else
                    exps.Add(position.Symbol, volume);
            }

            // перевести экспозицию по каждой валюте в валюту депо            
            foreach (var pair in exps)
            {
                var deltaExp = pair.Value;
                try
                {
                    string errorStr;
                    var deltaExpDepo = DalSpot.Instance.ConvertToTargetCurrency(pair.Key, true, account.Currency,
                        (double)deltaExp, curPrices, out errorStr, true);
                    if (deltaExpDepo.HasValue)
                        exposure += Math.Abs(deltaExpDepo.Value);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в CalculateAccountExposure - ConvertBase2Depo", ex);
                    throw;
                }
            }

            // марж. требования
            try
            {
                var accountGroup = getAccountGroup(accountId);
                reservedMargin = exposure / (decimal)accountGroup.BrokerLeverage;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CalculateAccountExposure - getAccountGroup", ex);
                throw;
            }

            return exps;
        }
    }
}
