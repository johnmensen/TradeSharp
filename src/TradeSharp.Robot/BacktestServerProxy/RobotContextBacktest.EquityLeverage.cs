using System;
using System.Collections.Generic;
using TradeSharp.Util;

namespace TradeSharp.Robot.BacktestServerProxy
{
    /*
     кривая доходности и экспозиция
     */
    public partial class RobotContextBacktest
    {
        /// <summary>
        /// дата, средства, экспозиция
        /// - по дням
        /// </summary>
        public List<Cortege3<DateTime, float, float>> dailyEquityExposure = new List<Cortege3<DateTime, float, float>>();
    
        private void UpdateDailyEquityExposure(DateTime date)
        {
            // проредить до 1 дня
            if (dailyEquityExposure.Count > 0)
                if (dailyEquityExposure[dailyEquityExposure.Count - 1].a == date) return;
            // средства
            var quotes = quotesStorage.ReceiveAllData();
            AccountInfo.Equity = profitCalculator.CalculateAccountEquity(AccountInfo.ID,
                AccountInfo.Balance, AccountInfo.Currency, quotes, this);
            // плечо
            decimal reservedMargin, exposure, equity;
            profitCalculator.CalculateAccountExposure(AccountInfo.ID, 
                out equity, out reservedMargin, out exposure, quotes, this, GetAccountGroup);
            dailyEquityExposure.Add(new Cortege3<DateTime, float, float>(date, (float)equity, (float)exposure));
        }
    }
}
