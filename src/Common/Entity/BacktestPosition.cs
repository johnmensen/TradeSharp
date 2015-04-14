using System;

namespace Entity
{
    /// <summary>
    /// Позиция, созданная в процессе бэктестинга (BacktestPosition)
    /// </summary>
    [Serializable]
    public class BacktestPosition : Position
    {
        #region События времени жизни позиции
        public decimal? PriceProtected { get; set; }
        public DateTime? TimeProtected { get; set; }

        public decimal? PriceStoplossed { get; set; }
        public DateTime? TimeStoplossed { get; set; }

        public decimal? PriceTakenprofit { get; set; }
        public DateTime? TimeTakenprofit { get; set; }
        #endregion
    }
}
