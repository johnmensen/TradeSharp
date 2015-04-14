using System.Collections.Generic;
using TradeSharp.Hub.BL.Model;

namespace TradeSharp.Hub.WebSite.Models
{
    public class TickerListModel
    {
        /// <summary>
        /// Секущий брокер
        /// </summary>
        public string CurrentServerInstanceCode { get; set; }

        /// <summary>
        /// Список всех брокеров
        /// </summary>
        public List<string> ServerInstanceCodeList { get; set; }

        /// <summary>
        /// Списов всех валют для выпадающих списков
        /// </summary>
        public List<Currency> Currencies { get; set; }

        /// <summary>
        /// Список всех тикеров
        /// </summary>
        public List<TickerEditModel> Items { get; set; }
    }
}