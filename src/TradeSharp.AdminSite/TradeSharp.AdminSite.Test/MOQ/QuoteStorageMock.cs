using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.AdminSite.Test.MOQ
{
    class QuoteStorageMock : IQuoteStorage
    {
        public PackedCandleStream GetMinuteCandlesPacked(string symbol, DateTime start, DateTime end)
        {
            throw new NotImplementedException("GetMinuteCandlesPacked!!!");
        }

        public Dictionary<string, DateSpan> GetTickersHistoryStarts()
        {
            throw new NotImplementedException("GetTickersHistoryStarts!!!");
        }

        public Dictionary<string, QuoteData> GetQuoteData()
        {
            return ReadFromFile();
        }

        public PackedCandleStream GetMinuteCandlesPackedFast(string symbol, List<Cortege2<DateTime, DateTime>> intervals)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Читает котировки из файла. Строки вида:   EURUSD	1.371825	09.12.2013 16:09:35
        /// </summary>
        private static Dictionary<string, QuoteData> ReadFromFile()
        {
            var result = new Dictionary<string, QuoteData>();


            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DAL\\last_quotes.txt");


            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error("ReadFromFile", ex);
                return result;
            }

            if (lines.Length == 0) return result;
            
           

            foreach (var quoteItemes in lines.Select(line => line.Split('\t')))
            {
                float bid;
                if (Single.TryParse(quoteItemes[1].Trim(), out bid)) continue;

                var ticker = quoteItemes[0].Trim();
                if (!result.Keys.Contains(ticker)) continue;
                var quote = new QuoteData(bid, DalSpot.Instance.GetAskPriceWithDefaultSpread(ticker, bid), DateTime.Now);
                result.Add(ticker, quote);
            }

            return result;
        }
    }
}
