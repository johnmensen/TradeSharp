using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class AccountEfficiency
    {
        private static readonly Func<object, object> makeCopy;

        [CopyValue]
        public DateTime StartDate { get; set; }

        [XmlIgnore] 
        public Dictionary<string, QuoteData> currentQuotes = new Dictionary<string, QuoteData>();

        #region Массивы

        public List<EquityOnTime> listEquity;

        public List<EquityOnTime> listLeverage;

        public List<BalanceChange> listTransaction;

        public List<EquityOnTime> listProfit1000;

        public List<MarketOrder> closedDeals = new List<MarketOrder>();

        public List<MarketOrder> openedDeals = new List<MarketOrder>();

        #endregion

        public PerformerStat Statistics { get; set; }
        
        [CopyValue]
        public float InitialBalance { get; set; }

        [CopyValue]
        public int DealsStillOpened { get; set; }

        [CopyValue]
        public float ProfitGeomMonth { get; set; }

        [CopyValue]
        public float ProfitGeomYear { get; set; }                

        static AccountEfficiency()
        {
            makeCopy = CopyValueAttribute.MakeCopyValuesRoutine(typeof (AccountEfficiency));
        }

        public AccountEfficiency()
        {
        }

        public AccountEfficiency(PerformerStat stat)
        {
            Statistics = stat;
        }

        public AccountEfficiency MakeCopy(bool copyOpenedPositions, bool copyClosedPositions, bool copyTransactions)
        {
            var cpy = (AccountEfficiency) makeCopy(this);
            cpy.listEquity = listEquity == null ? null : listEquity.ToList();
            cpy.listLeverage = listLeverage == null ? null : listLeverage.ToList();
            if (copyTransactions)
                cpy.listTransaction = listTransaction == null ? null : listTransaction.ToList();
            cpy.listProfit1000 = listProfit1000 == null ? null : listProfit1000.ToList();

            if (copyOpenedPositions)
                cpy.openedDeals = openedDeals == null ? null : openedDeals.Select(d => d.MakeCopy()).ToList();
            if (copyClosedPositions)
                cpy.closedDeals = closedDeals == null ? null : closedDeals.Select(d => d.MakeCopy()).ToList();
            
            cpy.Statistics = Statistics == null ? null : new PerformerStat(Statistics);

            return cpy;
        }
    }
}
