using System;
using Entity;

namespace TradeSharp.Client.Subscription.Model
{
    /// <summary>
    /// торговый сигнал, представленный категорией,
    /// тикером, таймфреймом и временем обновления
    /// </summary>
    public class TradeSignalUpdate
    {
        public int ServiceId { get; set; }

        public string Ticker { get; set; }

        public BarSettings Timeframe { get; set; }

        public DateTime TimeUpdated { get; set; }

        public string CategoryName { get; set; }

        public int ObjectCount { get; set; }

        public string TimeframeFriendlyName
        {
            get { return BarSettingsStorage.Instance.GetBarSettingsFriendlyName(Timeframe); }
        }

        public TradeSignalUpdate(int serviceId, string ticker)
        {
            ServiceId = serviceId;
            Ticker = ticker;
        }

        public TradeSignalUpdate(int serviceId, string ticker, BarSettings timeframe)
        {
            ServiceId = serviceId;
            Ticker = ticker;
            Timeframe = timeframe;
        }

        public TradeSignalUpdate(int serviceId, string ticker, BarSettings timeframe, DateTime timeUpdated)
        {
            ServiceId = serviceId;
            Ticker = ticker;
            Timeframe = timeframe;
            TimeUpdated = timeUpdated;
        }
    }
}
