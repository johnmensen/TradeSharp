using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class TopPortfolio
    {
        public int Id { get; set; }

        /// <summary>
        /// произвольное название портфеля
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// формула - критерий отсева
        /// </summary>
        public string Criteria { get; set; }

        /// <summary>
        /// макс. количество участников портфеля
        /// </summary>
        public int ParticipantCount { get; set; }

        /// <summary>
        /// сортировка по формуле
        /// </summary>
        public bool DescendingOrder { get; set; }

        /// <summary>
        /// граничное значение (отсев) для формулы
        /// </summary>
        public float? MarginValue { get; set; }

        /// <summary>
        /// "эталонный" счет, управляемый портфелем, созданным брокером
        /// </summary>
        public int? ManagedAccount { get; set; }

        /// <summary>
        /// пользователь - составитель портфеля
        /// </summary>
        public int? OwnerUser { get; set; }

        public AutoTradeSettings TradeSettings { get; set; }

        public bool IsCompanyPortfolio
        {
            get { return ManagedAccount.HasValue; }
        }

        #region runtime
        /// <summary>
        /// доходность портфеля (runtime)
        /// </summary>
        public PerformerStat Statistics;

        /// <summary>
        /// управляющие, находящиеся в портфеле (runtime)
        /// </summary>
        public List<int> ManagerIds { get; set; }

        /// <summary>
        /// доходность управляющих портфеля (runtime)
        /// </summary>
        public List<PerformerStat> Managers { get; set; }

        /// <summary>
        /// открытые по "эталонному" счету ордера
        /// </summary>
        public List<MarketOrder> Orders { get; set; }
        #endregion

        public TopPortfolio()
        {
        }

        public TopPortfolio(TopPortfolio topPortfolio)
        {
            Id = topPortfolio.Id;
            Name = topPortfolio.Name;
            Criteria = topPortfolio.Criteria;
            ParticipantCount = topPortfolio.ParticipantCount;
            DescendingOrder = topPortfolio.DescendingOrder;
            ManagedAccount = topPortfolio.ManagedAccount;
            Statistics = topPortfolio.Statistics;
            ManagerIds = topPortfolio.ManagerIds.ToList();
            Managers = topPortfolio.Managers.ToList();
            Orders = topPortfolio.Orders.ToList();
            MarginValue = topPortfolio.MarginValue;
            OwnerUser = topPortfolio.OwnerUser;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool AreSame(TopPortfolio p)
        {
            return Name == p.Name && Criteria == p.Criteria && ParticipantCount == p.ParticipantCount
                   && DescendingOrder == p.DescendingOrder && MarginValue == p.MarginValue;
        }
    }
}
