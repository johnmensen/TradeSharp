using System;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// подписка на платную услугу
    /// </summary>
    [Serializable]
    public class Subscription
    {
        public bool TradeAuto
        {
            get { return AutoTradeSettings != null && AutoTradeSettings.TradeAuto; }
        }

        /// <summary>
        /// также "вычисляемое" поле
        /// </summary>
        public PaidService PaidService { get; set; }

        public int Service { get; set; }

        public int User { get; set; }

        public DateTime TimeStarted { get; set; }

        public DateTime TimeEnd { get; set; }

        public bool RenewAuto { get; set; }

        public string Title
        {
            get { return PaidService == null ? "Сигналы №" + Service : PaidService.Comment; }
        }

        /// <summary>
        /// настройки автоматической торговли
        /// если подписка на торговые сигналы
        /// берется из View
        /// </summary>
        public AutoTradeSettings AutoTradeSettings { get; set; }
    
        public bool AreSame(Subscription s)
        {
            var areSame = Service == s.Service && User == s.User && TimeEnd == s.TimeEnd && TimeStarted == s.TimeStarted &&
                          RenewAuto == s.RenewAuto;
            if (!areSame) return false;
            if (AutoTradeSettings == null && s.AutoTradeSettings == null) return true;
            if (AutoTradeSettings == null || s.AutoTradeSettings == null) return false;
            return AutoTradeSettings.AreSame(s.AutoTradeSettings);
        }
    }
}
