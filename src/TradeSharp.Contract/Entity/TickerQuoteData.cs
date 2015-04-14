using System;
using System.Globalization;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    public class TickerQuoteData : QuoteData, IBaseNews
    {
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
        public String Ticker { get; set; }
        public const string NewsPrefix = "Q:";
        public const string PartSeparator = ",";

        public override String ToString()
        {
            return String.Format(CultureInfo.InvariantCulture ,"{0}{1}{2}{3}{2}{4}", 
                NewsPrefix,
                Ticker, 
                PartSeparator, 
                bid.ToStringUniform(5),
                ask.ToStringUniform(5));
        }

        /// <summary>
        /// формат строки:  Q:<Ticker>;<Bid>;<Ask>
        /// </summary>
        public void Parse(String str)
        {
            if (str.StartsWith(NewsPrefix))
            {
                str = str.Substring(NewsPrefix.Length);
                // если Body будет пустым то в массиве будет пустое значение
                var newsItem = str.Split(new [] { PartSeparator }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    time = DateTime.Now;
                    Ticker = newsItem[0];
                    bid = newsItem[1].ToFloatUniform();
                    ask = newsItem[2].ToFloatUniform();

                }
                catch (Exception ex)
                {
                    throw new Exception("TickerQuoteData.Parse: неправильный формат данных", ex);
                }
            }
            else
                throw new Exception("TickerQuoteData.Parse: не найден префикс данных news");
        }
    }
}
