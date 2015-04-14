using System.Collections.Generic;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models.Report
{
    public partial class ReportBroker
    {
        public class TurnoverByActive
        {
            public string Ticker { get; set; }

            public long Turnover { get; set; }

            public int DealsCount { get; set; }
            
            public float DealsPercent { get; set; }
        }

        public class GroupWithAccounts
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public bool IsReal { get; set; }

            public int Accounts { get; set; }
        }

        public class CurrencyDepositWithdrawal
        {
            public string Currency { get; set; }

            public double Deposit { get; set; }

            public string DepositString
            {
                get { return Deposit == 0 ? "-" : Deposit.ToStringUniformMoneyFormat(); }
            }

            public double Withdraw { get; set; }

            public string WithdrawString
            {
                get { return Withdraw == 0 ? "-" : Withdraw.ToStringUniformMoneyFormat(); }
            }
        }

        public List<string> Errors { get; private set; }

        #region Счета
        public int AccountsCount { get; set; }

        public int AccountsDemoCount { get; set; }

        public int AccountsAdded { get; set; }

        public int AccountsDemoAdded { get; set; }

        public List<GroupWithAccounts> Groups { get; set; }
        #endregion

        #region Данные по объемам
        public List<TurnoverByActive> TurnoverByPair { get; set; }
        #endregion

        #region Заведенные - выведенные средства
        public Dictionary<string, double> TotalIncome { get; set; }

        public Dictionary<string, double> TotalOutcome { get; set; }

        public List<CurrencyDepositWithdrawal> CurrencyDepWith { get; set; }
        #endregion

        #region Маркап по закрытым позам
        public double TotalMarkupReal { get; set; }

        public double TotalMarkupDemo { get; set; }
        #endregion

        #region Закрытые позиции
        public double CloseProfit { get; set; }

        public double CloseGrossProfit { get; set; }

        public double CloseGrossLoss { get; set; }

        public double CloseProfitDemo { get; set; }

        public double CloseGrossProfitDemo { get; set; }

        public double CloseGrossLossDemo { get; set; }
        #endregion

        #region Перформанс трейдеров
        /// <summary>
        /// ср. заведенная сумма в валюте брокера
        /// </summary>
        public decimal AvgTotalIncome { get; set; }
        /// <summary>
        /// средний профит в валюте брокера
        /// </summary>
        public decimal AvgProfitBroker { get; set; }
        /// <summary>
        /// среднее отношение профит / заведенные средства, %
        /// </summary>
        public decimal AvgProfitIncome { get; set; }

        public List<AccountPerformance> PerformersTopTotalIncome { get; set; }
        public List<AccountPerformance> PerformersTopProfitBroker { get; set; }
        public List<AccountPerformance> PerformersTopWorstProfitBroker { get; set; }
        public List<AccountPerformance> PerformersTopProfitIncome { get; set; }
        #endregion
    }
}