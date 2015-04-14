using System;

namespace Entity
{
    /// <summary>
    /// Класс описывает позицию
    /// Частные случаи позиции - созданная в процессе бэктестинга (BacktestPosition)
    /// и созданная на счете
    /// </summary>
    [Serializable]
    public class Position
    {
        #region Свойства
        /// <summary>
        /// ID сделки в MTS live
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// ID счета сделки
        /// </summary>
        public int AccountID { get; set; }
        private int pendingOrderID = -1;
        /// <summary>
        /// ID отложенного ордера сделки
        /// -1 - сделка открыта не по отл. ордеру
        /// </summary>
        public int PendingOrderID
        {
            get { return pendingOrderID; }
            set { pendingOrderID = value; }
        }
        /// <summary>
        /// BUY = 1, SELL = -1
        /// </summary>
        public DealType Side { get; set; }
        /// <summary>
        /// объем как есть
        /// </summary>
        public int Volume { get; set; }
        /// <summary>
        /// инструмент (валютная пара)
        /// </summary>
        public string Symbol { get; set; }
        private DealStatus status = DealStatus.Closed;
        /// <summary>
        /// состояние сделки
        /// </summary>
        public DealStatus Status
        {
            get { return status; }
            set { status = value; }
        }
        public bool IsOpened
        {
            get { return status == DealStatus.Opened; }
        }
        /// <summary>
        /// коментарий по сделке
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// коментарий, проставляемый экспертом
        /// </summary>
        public string ExpertComment { get; set; }
        /// <summary>
        /// цена входа в позицию
        /// </summary>
        public decimal PriceEnter { get; set; }
        /// <summary>
        /// цена закрытия или 0
        /// </summary>
        public decimal PriceExit { get; set; }
        /// <summary>
        /// время входа
        /// </summary>
        public DateTime TimeEnter { get; set; }
        /// <summary>
        /// время выхода
        /// </summary>
        public DateTime TimeExit { get; set; }
        /// <summary>
        /// стоплосс, абс. величина
        /// </summary>
        public decimal Stoploss { get; set; }
        /// <summary>
        /// тейкпрофит, абс. величина
        /// </summary>
        public decimal Takeprofit { get; set; }
        #endregion

        public decimal GetResultAbs()
        {
            if (!IsOpened) return 0;
            return (PriceExit - PriceEnter)*(int) Side;
        }

        public decimal GetResultPoints()
        {
            if (!IsOpened) return 0;
            var abs = (PriceExit - PriceEnter) * (int)Side;
            return DalSpot.Instance.GetPointsValue(Symbol, abs);
        }
    }
}
