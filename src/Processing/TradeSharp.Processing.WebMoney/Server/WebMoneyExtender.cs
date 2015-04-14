using System;
using System.Globalization;

using WebMoney.BasicObjects;

namespace TradeSharp.Processing.WebMoney.Server
{
    /// <summary>
    /// Содержит методы расширения
    /// </summary>
    static class WebMoneyExtender
    {
        public static DateTime GetServerTime(this WmDateTime time)
        {
            var str = time.ToServerString();
            return DateTime.ParseExact(str, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
