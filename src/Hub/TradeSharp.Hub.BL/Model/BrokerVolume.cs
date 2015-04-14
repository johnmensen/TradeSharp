using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradeSharp.Hub.BL.Entity;

namespace TradeSharp.Hub.BL.Model
{
    public class BrokerVolume
    {
        [Key, Column(Order = 0), DisplayName("Дата")]
        public DateTime Date { get; set; }

        [Key, Column(Order = 1), DisplayName("Сервер")]
        public string ServerCode { get; private set; }

        [Key, Column(Order = 2), DisplayName("Тикер")]
        public string TickerName { get; private set; }

        [Key, Column(Order = 3), DisplayName("Категория счёта")]
        public int AccountCategory { get; set; }

        [ForeignKey("ServerCode")]
        public virtual ServerInstance Server { get; set; }

        [ForeignKey("TickerName")]
        public virtual Ticker Ticker { get; set; }

        [Required]
        [DisplayName("Объём")]
        public long Volume { get; set; }             
    }
}