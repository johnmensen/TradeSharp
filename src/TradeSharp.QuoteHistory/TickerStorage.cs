using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.QuoteHistory
{
    public class TickerStorage
    {
        private readonly IQuoteStorage quoteStorage;
        
        private readonly string quotesFolder;        
        
        private readonly ServerQuoteHistory serverQuoteHistory;

        private static TickerStorage instance;
        
        public static TickerStorage Instance
        {
            get { return instance ?? (instance = new TickerStorage()); }
        }

        private TickerStorage()
        {
            quotesFolder = ExecutablePath.ExecPath + "\\quotes";
            try
            {
                quoteStorage = Contract.Util.Proxy.QuoteStorage.Instance.proxy;
            }
            catch
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
                return;
            }

            serverQuoteHistory = new ServerQuoteHistory(quoteStorage);
        }
        
        public Dictionary<string, List<CandleDataBidAsk>> GetQuotes(Dictionary<string, DateTime?> usedTickers)
        {
            var dic = new Dictionary<string, List<CandleDataBidAsk>>();
            if (usedTickers.Count == 0) return dic;

            foreach (var ticker in usedTickers)
            {
                var candles = ActualizeAndLoadTickerData(ticker.Key, ticker.Value) ?? new List<CandleData>();
                // получить котировки из свечек
                var symbolName = ticker.Key;
                var quotes = candles.Select(c => new CandleDataBidAsk(c,
                    DalSpot.Instance.GetDefaultSpread(symbolName))).ToList();
                dic.Add(symbolName, quotes);
            }
            return dic;
        }

        private List<CandleData> ActualizeAndLoadTickerData(string tickerName, DateTime? dateStart)
        {
            // нуждаются в актуализации?
            if (!serverQuoteHistory.IsFileActual(tickerName))
            {
                ActualizeTickerHistory(tickerName, dateStart);
                serverQuoteHistory.UpdateTickerTime(tickerName);
            }
            // вернуть данные из хранилища
            return AtomCandleStorage.Instance.GetAllMinuteCandles(tickerName);
        }

        private void ActualizeTickerHistory(string ticker, DateTime? dateStart)
        {       
            // дата первой котировки
            var timeStart = dateStart ?? serverQuoteHistory.GetServerTickerHistoryStart(ticker);
            // список гэпов
            var gaps = FileGapActualizator.VerifyTickerHistory(ticker, timeStart, quotesFolder, null);
            // подкачать историю по гэпам
            if (gaps.Count > 0)
            {
                Logger.Info(string.Format("Выполняется актаулизация {0}, начиная с {1}; найдено гэпов: {2}", ticker, dateStart, gaps.Count));
                FileGapActualizator.FillGapsByTickerFast(ticker, dateStart, DateTime.Now, gaps, quotesFolder, null,
                                                         (s, list) => { });
            }
        }
    }    
}
