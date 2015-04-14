using System;
using Entity;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// торговый сигнал, представленный категорией,
    /// тикером, таймфреймом и временем обновления
    /// </summary>
    //public class TradeSignalUpdate
    //{
    //    public int CategoryId { get; set; }

    //    public string Ticker { get; set; }

    //    public BarSettings Timeframe { get; set; }

    //    public DateTime TimeUpdated { get; set; }

    //    public string CategoryName { get; set; }

    //    public string TimeframeFriendlyName
    //    {
    //        get { return BarSettingsStorage.Instance.GetBarSettingsFriendlyName(Timeframe); }
    //    }

    //    public TradeSignalUpdate(int catId, string ticker, BarSettings timeframe)
    //    {
    //        CategoryId = catId;
    //        Ticker = ticker;
    //        Timeframe = timeframe;            
    //    }

    //    public TradeSignalUpdate(int catId, string ticker, BarSettings timeframe, DateTime timeUpdated)
    //    {
    //        CategoryId = catId;
    //        Ticker = ticker;
    //        Timeframe = timeframe;
    //        TimeUpdated = timeUpdated;
    //    }
    //}
}
