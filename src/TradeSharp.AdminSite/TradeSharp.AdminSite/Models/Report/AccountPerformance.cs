using System.Collections.Generic;
using Entity;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Models.Report
{
    /// <summary>
    /// сколько завел - вывел, в родной валюте и в валюте брокера,
    /// сколько заработал - слил...
    /// </summary>
    public class AccountPerformance
    {
        public ACCOUNT Account { get; set; }

        public decimal TotalIncome { get; set; }

        /// <summary>
        /// TotalIncome в валюте брокера
        /// </summary>
        public decimal TotalIncomeBroker { get; set; }

        public decimal TotalWithdraw { get; set; }

        public decimal TotalWithdrawBroker { get; set; }

        public decimal TotalProfit { get; set; }

        public decimal TotalProfitBroker { get; set; }

        /// <summary>
        /// TotalProfit / TotalIncome
        /// </summary>
        public decimal TotalProfitPercent { get; set; }

        /// <summary>
        /// посчитать поля TotalIncomeBroker, TotalWithdrawBroker, TotalProfitBroker
        /// </summary>
        public void CalculateResultsInBrokerCurrency(Dictionary<string, CandleData> candles, string brokerCurrency, out string errorStr)
        {
            errorStr = string.Empty;

            if (Account.Currency == brokerCurrency)
            {
                TotalIncomeBroker = TotalIncome;
                TotalWithdrawBroker = TotalWithdraw;
                TotalProfitBroker = TotalProfit;
                return;
            }
            // перевести профит из валюты Account.Currency в валюту brokerCurrency
            bool inverse;
            var smb = DalSpot.Instance.FindSymbol(Account.Currency, brokerCurrency, out inverse);
            if (string.IsNullOrEmpty(smb))
            {
                errorStr = "CalculateResultsInBrokerCurrency - " + Resource.ErrorMessageNotFoundInstrument + " " + Account.Currency + "/" +
                           brokerCurrency;
                return;
            }
            CandleData candle;
            if (!candles.TryGetValue(smb, out candle))
            {
                errorStr = "CalculateResultsInBrokerCurrency - " + Resource.ErrorMessageNotFoundQuote + " " + smb;
                return;
            }
            var price = (candle.close + DalSpot.Instance.GetAskPriceWithDefaultSpread(smb, candle.close)) * 0.5f;
            if (inverse) price = 1 / price;

            TotalIncomeBroker = (decimal)price * TotalIncome;
            TotalWithdrawBroker = (decimal)price * TotalWithdraw;
            TotalProfitBroker = (decimal)price * TotalProfit;
        }
    }
}