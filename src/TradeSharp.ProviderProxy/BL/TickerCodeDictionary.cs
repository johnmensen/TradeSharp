using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxy.BL
{
    class TickerCodeDictionary
    {
        private static TickerCodeDictionary instance;

        public static TickerCodeDictionary Instance
        {
            get { return instance ?? (instance = new TickerCodeDictionary()); }
        }

        public readonly string tickerPreffix = AppConfig.GetStringParam("TickerPreffix", "");

        public readonly string tickerSuffix = AppConfig.GetStringParam("TickerSuffix", "");

        public readonly Dictionary<string, int> tickerCode = new Dictionary<string, int>();

        public readonly Dictionary<string, int> tickerContract = new Dictionary<string, int>();

        public string GetTickerNameFormatted(string ticker)
        {
            return tickerPreffix + ticker + tickerSuffix;
        }

        public string GetClearTickerName(string ticker)
        {
            if (!string.IsNullOrEmpty(tickerPreffix)) ticker = ticker.Substring(tickerPreffix.Length);
            if (!string.IsNullOrEmpty(tickerSuffix)) ticker = ticker.Substring(0, ticker.Length - tickerSuffix.Length);
            return ticker;
        }

        private TickerCodeDictionary()
        {
            // AUD/JPY,4008,AUD/JPY,10000,0.001,10,09/07/2010,,JPY,
            var path = ExecutablePath.ExecPath + "\\ticker_code.csv";
            if (!File.Exists(path)) return;

            try
            {
                using (var sr = new StreamReader(path, Encoding.ASCII))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        var parts = line.Split(new[] {','});
                        if (parts.Length < 4) continue;

                        var ticker = parts[0].Replace("/", "");
                        var code = parts[1].ToIntSafe();
                        var contract = parts[3].ToIntSafe();

                        if (string.IsNullOrEmpty(ticker) || code == null) continue;
                        if (!tickerCode.ContainsKey(ticker))
                            tickerCode.Add(ticker, code.Value);

                        if (contract != null && !tickerContract.ContainsKey(ticker))
                            tickerContract.Add(ticker, contract.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в TickerCodeDictionary", ex);
                throw;
            }
        }
    }
}
