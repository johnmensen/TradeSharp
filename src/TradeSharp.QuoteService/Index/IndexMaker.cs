using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.QuoteService.Index
{
    /// <summary>
    /// формирует индексы из котировок
    /// учитывает только одну цену (bid + ask) / 2
    /// </summary>
    class IndexMaker
    {
        private readonly Dictionary<string, Cortege2<float,DateTime>> lastQuotes =
            new Dictionary<string, Cortege2<float, DateTime>>();
        private const string LastQuotesFileName = "last_quotes.txt";
        private const string IndexSettingsFileName = "indicies.txt";
        private const int QuoteStaleSeconds = 10*60;
        private IIndexFormula[] indexFormulas;
        private DateTime? lastTimeQuotesSaved;
        private const int MinSecondsBetweenSaveQuotes = 60;
        
        public bool HasIndicies { get { return indexFormulas.Length > 0; } }
        
        /// <summary>
        /// при старте прочитать из файла с предопределенным именем
        /// последние сохраненные котировки
        /// </summary>
        public IndexMaker()
        {
            // подгрузить последние котировки
            LoadLastQuotes();
            // загрузить настройки индексов
            LoadIndicies();
        }

        public List<IBaseNews> MakeIndicies(List<IBaseNews> newsQuotes)
        {
            var indicies = new List<IBaseNews>();
            if (newsQuotes.Count == 0) return indicies;
            var quotes = newsQuotes.OfType<TickerQuoteData>().ToList();
            // обновить последние котировки
            UpdateLastQuotes(quotes);
            // посчитать индексы
            if (indexFormulas.Length == 0) return indicies;
            indicies.AddRange(indexFormulas.Select(ind => ind.MakeIndex(lastQuotes, QuoteStaleSeconds)).Where(
                indexVal => indexVal != null));
            return indicies;
        }

        private void UpdateLastQuotes(List<TickerQuoteData> quotes)
        {
            foreach (var quote in quotes)
            {
                var price = quote.bid > 0 && quote.ask > 0
                                ? (quote.bid + quote.ask)*0.5f
                                : quote.ask > 0 ? quote.ask : quote.bid;
                if (price == 0) continue;
                var quoteData = new Cortege2<float, DateTime>(price, DateTime.Now);

                if (!lastQuotes.ContainsKey(quote.Ticker))
                    lastQuotes.Add(quote.Ticker, quoteData);
                else
                    lastQuotes[quote.Ticker] = quoteData;
            }
            SaveLastQuotes();
        }

        private void LoadLastQuotes()
        {
            var fileName = string.Format("{0}\\{1}", ExecutablePath.ExecPath, LastQuotesFileName);
            if (!File.Exists(fileName)) return;
            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var lineParts = line.Split(new[] {(char) 9}, StringSplitOptions.RemoveEmptyEntries);
                    if (lineParts.Length != 3) continue;
                    var ticker = lineParts[0];
                    var quote = lineParts[1].ToFloatUniformSafe();
                    var time = lineParts[2].ToDateTimeUniformSafe();
                    if (string.IsNullOrEmpty(ticker) || quote.HasValue == false ||
                        time.HasValue == false) continue;
                    lastQuotes.Add(ticker, new Cortege2<float, DateTime> { a = quote.Value, b = time.Value });                        
                }
            }
        }
    
        private void SaveLastQuotes()
        {
            if (lastQuotes.Count == 0) return;

            // проверка от заспамливания
            if (!lastTimeQuotesSaved.HasValue)
                lastTimeQuotesSaved = DateTime.Now;
            else
            {
                var deltaSeconds = (DateTime.Now - lastTimeQuotesSaved.Value).TotalSeconds;
                if (deltaSeconds < MinSecondsBetweenSaveQuotes) return;
                lastTimeQuotesSaved = DateTime.Now;
            }    

            var fileName = string.Format("{0}\\{1}", ExecutablePath.ExecPath, LastQuotesFileName);
            using (var sw = new StreamWriter(fileName))
            {
                foreach (var q in lastQuotes)
                {
                    sw.WriteLine(string.Format("{1}{0}{2}{0}{3}",
                    (char)9, q.Key, q.Value.a.ToStringUniform(), q.Value.b.ToStringUniform()));
                }
            }
        }
    
        private void LoadIndicies()
        {// загрузить описания индексов из файла
            var fileName = string.Format("{0}\\{1}", ExecutablePath.ExecPath,
                                         IndexSettingsFileName);
            if (!File.Exists(fileName)) return;
            var doc = new XmlDocument();
            doc.Load(fileName);
            if (doc.DocumentElement == null) return;

            var indicies = new List<IIndexFormula>();
            foreach (XmlElement node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "MultiplicativeIndex")
                    indicies.Add(MultiplicativeIndex.LoadIndexFromXml(node));
            }
            indexFormulas = indicies.ToArray();
        }
    }

    interface IIndexFormula
    {
        IBaseNews MakeIndex(Dictionary<string, Cortege2<float, DateTime>> lastQuotes,
            int secondsToStale);
    }

    class MultiplicativeIndex : IIndexFormula
    {
        private readonly string[] tickers;
        private readonly decimal[] powers;
        private readonly double multiplier;
        private readonly string tickerName;

        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(10000);
        private const int LogMagicNoQuote = 1;

        public static MultiplicativeIndex LoadIndexFromXml(XmlNode node)
        {
            var tickerName = node.Attributes["ticker"].Value;
            var multiplier = node.Attributes["multiplier"].Value.ToDoubleUniform();
            var indexStr = node.Attributes["formula"].Value;
            return new MultiplicativeIndex(indexStr, multiplier, tickerName);
        }

        /// <summary>
        /// на входе строка вида EURUSD -0.576 USDCHF 0.132
        /// тикер(пробел)степень(пробел)
        /// </summary>
        public MultiplicativeIndex(string indexStr, double multiplier, string tickerName)
        {
            this.multiplier = multiplier;
            this.tickerName = tickerName;

            var parts = indexStr.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var count = parts.Length/2;
            var powerList = new List<decimal>();
            var tickerList = new List<string>();

            for (var i = 0; i < count; i++)
            {
                var ticker = parts[i*2];
                var power = parts[i*2 + 1].ToDecimalUniform();
                powerList.Add(power);
                tickerList.Add(ticker);
            }
            tickers = tickerList.ToArray();
            powers = powerList.ToArray();
        }

        public IBaseNews MakeIndex(Dictionary<string, Cortege2<float, DateTime>> lastQuotes,
            int secondsToStale)
        {
            // "слепить" индекс из котировок, посчитать количество
            // устаревших котировок
            var timeNow = DateTime.Now;
            var hasUpdatedValue = false;

            var index = multiplier;
            for (var i = 0; i < tickers.Length; i++)
            {
                var tickName = tickers[i];
                if (!lastQuotes.ContainsKey(tickName))
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                        LogMagicNoQuote, 60 * 1000, "MultiplicativeIndex: нет котировки \"{0}\"", tickName);
                    return null;
                }
                var tickValue = lastQuotes[tickName];
                var price = tickValue.a;
                if (price == 0)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Info,
                        LogMagicNoQuote, 60 * 1000, "MultiplicativeIndex: нет котировки \"{0}\"", tickName);
                    return null;
                }
                // значение устарело?
                var deltaSeconds = (timeNow - tickValue.b).TotalSeconds;
                var isStale = deltaSeconds > secondsToStale;
                if (!isStale) hasUpdatedValue = true;
                // домножить индекс
                index = index*Math.Pow((double) price, (double) powers[i]);
            }
            var newsQuote = new TickerQuoteData
                                {
                                    Ticker = tickerName,
                                    bid = (float)index,
                                    ask = (float) index
                                };
            return newsQuote;
        }
    }
}
