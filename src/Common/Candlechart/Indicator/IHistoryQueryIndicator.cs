using System;
using System.Collections.Generic;

namespace Candlechart.Indicator
{
    public delegate void UpdateTickersCacheForRobotsDel(Dictionary<string, DateTime> tickers);

    public delegate void UpdateTickersCacheForRobotsExDel(Dictionary<string, DateTime> tickers,
                                                          int maxMinutesDeltaStartEnd);

    public interface IHistoryQueryIndicator
    {
        //event UpdateTickersCacheForRobotsExDel OnUpdateTickersCacheForRobots;
        Dictionary<string, DateTime> GetRequiredTickersHistory(DateTime? timeStart);
    }
}
