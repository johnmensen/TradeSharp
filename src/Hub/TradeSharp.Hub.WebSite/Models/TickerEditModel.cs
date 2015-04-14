using System.Collections.Generic;
using System.ComponentModel;
using TradeSharp.Hub.BL.Model;

namespace TradeSharp.Hub.WebSite.Models
{
    public class TickerEditModel : Ticker
    {
        public List<Currency> currencies;

        public string TickerAllias { get; set; }

        [DisplayName("Код")]
        public string CurrencyCode
        {
            get { return BaseCurrency == null ? "" : BaseCurrency.Code; }
            set { BaseCurrency = new Currency {Code = value}; }
        }

        public TickerEditModel()
        {
        }

        public TickerEditModel(Ticker ticker)
        {
            Name = ticker.Name;
            BaseCurrency = ticker.BaseCurrency;
        }
    }
}