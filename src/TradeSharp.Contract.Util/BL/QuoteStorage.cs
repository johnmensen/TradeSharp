using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.BL
{
    /// <summary>
    /// Хранит последнюю котировку по каждому тикеру
    /// </summary>
    public class QuoteStorage : ThreadSafeStorage <string, QuoteData>
    {
        private readonly Dictionary<string, QuoteData> quotes =
            new Dictionary<string, QuoteData>();

        private static readonly Lazy<QuoteStorage> instance = new Lazy<QuoteStorage>(() => new QuoteStorage());
        public static QuoteStorage Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// сохранить срез котировок
        /// </summary>        
        public bool SaveQuotes(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            try
            {
                var allQuotes = ReceiveAllData();
                if (allQuotes.Count == 0) return false;
                using (var sw = new StreamWriter(fileName, false, Encoding.ASCII))
                {
                    foreach (var q in quotes)
                    {                        
                        var line = string.Format(CultureProvider.Common,
                                                 "{0};{1:f5};{2:f5};{3:dd.MM.yyyy HH:mm:ss}", 
                                                 q.Key, q.Value.bid, q.Value.ask, q.Value.time);
                        sw.WriteLine(line);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения среза котировок: ", ex);
                return false;
            }
        }

        /// <summary>
        /// закачать срез котировок (старые, но лучше, чем ничего)
        /// </summary>        
        public bool LoadQuotes(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            try
            {       
                var lstName = new List<string>();
                var lstQuote = new List<QuoteData>();
                using (var sr = new StreamReader(fileName, Encoding.ASCII))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        var parts = line.Split(';');
                        if (parts.Length != 4) continue;
                        var q = new QuoteData(parts[1].ToFloatUniform(), parts[2].ToFloatUniform(),
                                              DateTime.ParseExact(parts[3], "dd.MM.yyyy HH:mm:ss",
                                                                  CultureProvider.Common));
                        var symbol = parts[0];
                        lstName.Add(symbol);
                        lstQuote.Add(q);
                    }
                }

                if (lstName.Count > 0)
                    UpdateValues(lstName.ToArray(), lstQuote.ToArray());
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки среза котировок: ", ex);
                return false;
            }
        }
    }
}