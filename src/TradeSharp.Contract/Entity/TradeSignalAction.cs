using System;
using System.Xml.Serialization;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// конкретный торговый сигнал, (не категория, а именно - действие)
    /// купить/продать - закрыть - передвинуть стоп/тейк
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(TradeSignalActionTrade))]
    [XmlInclude(typeof(TradeSignalActionClose))]
    [XmlInclude(typeof(TradeSignalActionMoveStopTake))]
    public abstract class TradeSignalAction
    {
        /// <summary>
        /// идентификатор категории сигнала
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// уникальный идентификатор ордера
        /// </summary>
        public int OrderId { get; set; }
        
        public abstract TradeSignalAction MakeCopy();        
    }

    /// <summary>
    /// сигнальщик торганул
    /// </summary>
    [Serializable]
    public class TradeSignalActionTrade : TradeSignalAction
    {
        /// <summary>
        /// GBPUSD, SP500 ...
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// плечо
        /// </summary>
        public decimal Leverage { get; set; }

        /// <summary>
        /// 1 / -1 (BUY - SELL)
        /// </summary>
        public int Side { get; set; }

        public decimal StopLoss { get; set; }

        public decimal TakeProfit { get; set; }

        /// <summary>
        /// цена входа
        /// </summary>
        public float Price { get; set; }

        public override TradeSignalAction MakeCopy()
        {
            return new TradeSignalActionTrade
                {
                    ServiceId = ServiceId,
                    OrderId = OrderId,
                    Ticker = Ticker,
                    Leverage = Leverage,
                    Side = Side,
                    StopLoss = StopLoss,
                    TakeProfit = TakeProfit,
                    Price = Price
                };
        }

        public override string ToString()
        {
            return (Side < 0 ? "SELL " : "BUY ") + Ticker + ", сервис #" + ServiceId;
        }
    }

    /// <summary>
    /// сигнальщик торганул
    /// </summary>
    [Serializable]
    public class TradeSignalActionClose : TradeSignalAction
    {
        /// <summary>
        /// цена выхода
        /// </summary>
        public float Price { get; set; }

        public override TradeSignalAction MakeCopy()
        {
            return new TradeSignalActionClose
            {
                ServiceId = ServiceId,
                OrderId = OrderId,
                Price = Price
            };
        }

        public override string ToString()
        {
            return "Закрытие ордера " + OrderId + ", сервис #" + ServiceId;
        }
    }

    /// <summary>
    /// сигнальщик изменил стоп или тейк
    /// </summary>
    [Serializable]
    public class TradeSignalActionMoveStopTake : TradeSignalAction
    {
        public decimal? NewStopLoss { get; set; }

        public decimal? NewTakeProfit { get; set; }

        public override TradeSignalAction MakeCopy()
        {
            return new TradeSignalActionMoveStopTake
            {
                ServiceId = ServiceId,
                OrderId = OrderId,
                NewStopLoss = NewStopLoss,
                NewTakeProfit = NewTakeProfit
            };
        }

        public override string ToString()
        {
            return "Редактирование ордера " + OrderId + ", сервис #" + ServiceId;
        }
    }
}
