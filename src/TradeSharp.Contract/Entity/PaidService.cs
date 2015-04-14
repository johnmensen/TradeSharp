using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    public enum PaidServiceType
    {
        Signals = 0, PAMM = 1
    }

    /// <summary>
    /// платный сервис - SERVICE
    /// </summary>
    [Serializable]
    public class PaidService
    {
        public int Id { get; set; }

        public int User { get; set; }

        public decimal FixedPrice { get; set; }

        /// <summary>
        /// вычисляемое поле
        /// </summary>
        public decimal FixedPriceMonth
        {
            get { return FixedPrice*21; }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 64)
                    value = value.Substring(0, 64);
                comment = value;
            }
        }

        public PaidServiceType ServiceType { get; set; }

        /// <summary>
        /// "вычисляемое" поле - валюта кошелька
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// если это ПАММ или подписка на сигналы - ссылка на торговый счет
        /// </summary>
        public int? AccountId { get; set; }

        #region Прогрессивная шкала комиссии

        public List<PaidServiceRate> serviceRates;

        #endregion

        public static decimal GetMonthFeeFromDailyFee(decimal dailyFee, bool reverse = false)
        {
            return !reverse ? dailyFee * 21 : dailyFee / 21;
        }

        public override string ToString()
        {
            var parts = new List<string> {EnumFriendlyName<PaidServiceType>.GetString(ServiceType)};
            if (!string.IsNullOrEmpty(comment)) parts.Add(Comment);
            return string.Join(", ", parts);
        }

        public decimal CalculateFee(decimal shareMoney, decimal sumAboveHwm)
        {
            if (serviceRates == null || serviceRates.Count == 0 || sumAboveHwm <= 0.01M) return 0;
            var rate = serviceRates.FirstOrDefault(r => shareMoney >= r.UserBalance) ?? serviceRates[0];
            var feePercent = rate.Amount;
            return sumAboveHwm*feePercent/100M;
        }
    }

    [Serializable]
    public class PaidServiceRate
    {
        public enum ServiceRateType { Absolute = 0, Percent = 1 }

        /// <summary>
        /// при балансе до 100 USD ...
        /// </summary>
        public decimal UserBalance { get; set; }

        /// <summary>
        /// стрижем X% от превышения HWM
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// абс. размер комиссии - либо - процент от прибыли
        /// </summary>
        public ServiceRateType RateType { get; set; }
    }
}
