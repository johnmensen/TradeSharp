using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class TradeTicker
    {
        /// <summary>
        /// название - код, уникальное
        /// </summary>
        public string Title { get; set; }
        
        public string Description { get; set; }

        /// <summary>
        /// базовый актив, что торгуется
        /// </summary>
        public string ActiveBase { get; set; }

        /// <summary>
        /// валюта, за которую торгуется базовый актив
        /// </summary>
        public string ActiveCounter { get; set; }
        
        /// <summary>
        /// знаков после точки
        /// </summary>
        public int Precision { get; set; }
        
        /// <summary>
        /// код в БД котировок
        /// </summary>
        public int? CodeFXI { get; set; }
        
        public decimal? SwapBuy { get; set; }
        
        public decimal? SwapSell { get; set; }

        /// <summary>
        /// тип актива
        /// </summary>
        public Instrument Instrument { get; set; }

        /// <summary>
        /// дата экспирации
        /// актуальна, например, для фьючерсов / опционов
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// флаг хранится не в БД, а в настройках пользовательского терминала
        /// </summary>
        public bool IsFavourite { get; set; }
    }
}