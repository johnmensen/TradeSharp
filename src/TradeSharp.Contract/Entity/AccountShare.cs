using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class AccountShare
    {
        public int UserId { get; set; }

        /// <summary>
        /// "вычисляемое" поле
        /// </summary>
        public string UserLogin { get; set; }

        public decimal SharePercent { get; set; }

        public decimal ShareMoney { get; set; }

        public decimal HWM { get; set; }
    }
}
