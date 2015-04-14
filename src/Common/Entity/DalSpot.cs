using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace Entity
{
    public class DalSpot
    {
        private static DalSpot instance;
        
        public static DalSpot Instance
        {
            get { return instance ?? (instance = new DalSpot()); }
        }

        private Dictionary<string, TradeTicker> tickers;

        private Dictionary<string, TradeTicker> favTickers = new Dictionary<string, TradeTicker>();

        /// <summary>
        /// строки форматирования, индексом массива подставляется Precision
        /// </summary>
        private static readonly string[] formatPriceStrings = new [] { "f0", "f1", "f2", "f3", "f4", "f5" };

        private LotByGroupDictionary dictionaryGroupLot;

        private readonly string lotDicPath = ExecutablePath.ExecPath + "\\grouplot.txt";

        public bool ConnectionWasEstablished { get; private set; }

        private DalSpot()
        {
            FillDictionaries(TradeSharpDictionary.Instance.proxy);            
        }

        private DalSpot(ITradeSharpDictionary dict)
        {
            FillDictionaries(dict);
        }

        private void FillDictionaries(ITradeSharpDictionary dict)
        {
            // прочитать локальную копию настроек группа - лот
            dictionaryGroupLot = LotByGroupDictionary.LoadFromFile(lotDicPath);

            if (dict != null)
            try
            {
                long lotByGroupHashCode;
                tickers = dict.GetTickers(out lotByGroupHashCode).ToDictionary(t => t.Title, t => t);                
                if (tickers.Count > 0)
                {
                    // контракты прочитаны, проверить словарь группа - лот
                    if (lotByGroupHashCode == 0)
                        dictionaryGroupLot.Clear();
                    else
                    {
                        if (dictionaryGroupLot.calculatedHashCode != lotByGroupHashCode)
                        {
                            var newDictionaryGroupLot = dict.GetLotByGroup();
                            if (newDictionaryGroupLot != null)
                            {
                                dictionaryGroupLot = newDictionaryGroupLot;
                                dictionaryGroupLot.SaveInFile(lotDicPath);
                            }
                        }
                    }

                    ConnectionWasEstablished = true;
                    return;
                }
                else
                {
                    Logger.ErrorFormat("DalSpot: тикеры не получены");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в DalSpot ctor (доступ к БД)", ex);
            }
            InitializeDefault();
        }

        public static void Instantiate(ITradeSharpDictionary dict)
        {
            instance = new DalSpot(dict);
        }

        private void InitializeDefault()
        {
            tickers = new Dictionary<string, TradeTicker>
                {
                    {
                        "EURUSD", new TradeTicker
                            {
                                ActiveBase = "EUR",
                                ActiveCounter = "USD",
                                CodeFXI = 1,
                                Precision = 4,
                                Instrument = Instrument.Spot,
                                IsFavourite = true,
                                Title = "EURUSD"
                            }
                    },
                    {
                        "USDJPY", new TradeTicker
                            {
                                ActiveBase = "USD",
                                ActiveCounter = "JPY",
                                CodeFXI = 3,
                                Precision = 2,
                                Instrument = Instrument.Spot,
                                IsFavourite = true,
                                Title = "USDJPY"
                            }
                    },
                    {
                        "GBPUSD", new TradeTicker
                            {
                                ActiveBase = "GBP",
                                ActiveCounter = "USD",
                                CodeFXI = 2,
                                Precision = 4,
                                Instrument = Instrument.Spot,
                                IsFavourite = true,
                                Title = "GBPUSD"
                            }
                    },
                    {
                        "USDCHF", new TradeTicker
                            {
                                ActiveBase = "USD",
                                ActiveCounter = "CHF",
                                CodeFXI = 4,
                                Precision = 4,
                                Instrument = Instrument.Spot,
                                IsFavourite = true,
                                Title = "USDCHF"
                            }
                    },
                };
        }

        public void SetFavoritesList(string[] favTickersList)
        {
            favTickers = tickers.Where(t => favTickersList.Contains(t.Key)).ToDictionary(
                t => t.Key, t => t.Value);
        }

        /// <summary>
        /// получить список названий валютных пар и прочих торгуемых активов
        /// </summary>        
        public string[] GetTickerNames(bool favoritesOnly = false)
        {
            return favoritesOnly ? favTickers.Keys.ToArray() : tickers.Keys.ToArray();
        }
    
        /// <summary>
        /// Получить абсолютное значение из пунктов
        /// </summary>        
        public decimal GetAbsValue(string symbol, decimal points)
        {
            TradeTicker ticker;
            if (tickers.TryGetValue(symbol, out ticker))
                return points/Get10Power(ticker.Precision);
            return 0;
        }

        /// <summary>
        /// Получить абсолютное значение из пунктов
        /// </summary>        
        public float GetAbsValue(string symbol, float points)
        {
            TradeTicker ticker;
            if (tickers.TryGetValue(symbol, out ticker))
                return points / Get10PowerF(ticker.Precision);
            return 0;
        }

        public float GetAskPriceWithDefaultSpread(string symbol, float bid)
        {
            return bid + GetDefaultSpread(symbol);
        }

        public float GetBidPriceWithDefaultSpread(string symbol, float bid)
        {
            return bid - GetDefaultSpread(symbol);
        }

        /// <summary>
        /// Получить пункты из абсолютного значения
        /// </summary>        
        public decimal GetPointsValue(string symbol, decimal abs)
        {
            TradeTicker ticker;
            if (tickers.TryGetValue(symbol, out ticker))
                return abs * Get10Power(ticker.Precision);
            return 0;
        }

        /// <summary>
        /// Получить пункты из абсолютного значения
        /// </summary>        
        public float GetPointsValue(string symbol, float abs)
        {
            TradeTicker ticker;
            if (tickers.TryGetValue(symbol, out ticker))
                return abs * Get10PowerF(ticker.Precision);
            return 0;
        }

        public int GetFXICodeBySymbol(string symbol)
        {
            TradeTicker ticker;
            if (tickers.TryGetValue(symbol, out ticker))
                return ticker.CodeFXI ?? 0;
            return 0;
        }

        public string GetSymbolByFXICode(int code)
        {
            var ticker = tickers.Values.FirstOrDefault(t => t.CodeFXI == code);
            return ticker == null ? string.Empty : ticker.Title;
        }

        public int GetPrecision(string symbol)
        {
            TradeTicker ticker;
            if (tickers.TryGetValue(symbol, out ticker))
                return ticker.Precision;
            return 4;
        }

        /// <summary>
        /// сформировать строку вида "1.5051", "98.75" ...
        /// </summary>
        public string FormatPrice(string symbol, decimal price)
        {
            var fmtStr = formatPriceStrings[GetPrecision(symbol)];
            return price.ToString(fmtStr);
        }

        /// <summary>
        /// сформировать строку вида "1.5051", "98.75" ... - с использованием специфичного для культуры
        /// разделителя целой / дробной части
        /// </summary>
        public string FormatPrice(string symbol, float price, bool commonCulture = false)
        {
            var fmtStr = formatPriceStrings[GetPrecision(symbol)];
            return commonCulture 
                ? price.ToString(fmtStr, CultureProvider.Common)
                : price.ToString(fmtStr);
        }

        /// <summary>
        /// вернуть 1 / цену пункта
        /// (EURUSD - 10000)
        /// </summary>
        public int GetPrecision10(string symbol)
        {
            var pc = GetPrecision(symbol);
            return pc == 4 ? 10000 : pc == 2 ? 100 : pc == 3 ? 1000 : pc == 5 ? 100000 : 1;
        }

        public int GetPrecision10(float price)
        {
            return price < 7 ? 10000 : price < 35 ? 1000 : 100;
        }

        /// <summary>
        /// для исторических прогонов, спред по-умолчанию по каждому тикеру
        /// </summary>        
        public float GetDefaultSpread(string symbol)
        {
            var pc = GetPrecision(symbol);
            return pc == 4 ? 0.0002f : pc == 2 ? 0.02f : pc == 3 ? 0.002f : pc == 5 ? 0.00002f : 2f;
        }

        private static decimal Get10Power(int _base)
        {
            return _base == 0 ? 1 : _base == 1 ? 10 : _base == 2 ? 100 : _base == 3 ? 1000 : _base == 4 ? 10000 : 100000;
        }

        private static float Get10PowerF(int _base)
        {
            return _base == 0 ? 1 : _base == 1 ? 10 : _base == 2 ? 100 : _base == 3 ? 1000 : _base == 4 ? 10000 : 100000;
        }
    
        /// <summary>
        /// <example>
        /// bool inverse;
        /// var smb = FindSymbol("USD", "EUR", out inverse); // smb = "EURUSD", inverse = true
        /// </example>
        /// </summary>        
        public string FindSymbol(string activeBase, string activeCounter, out bool inverse)
        {            
            inverse = false;            
            var ticker = tickers.Values.FirstOrDefault(t => t.ActiveBase == activeBase && t.ActiveCounter == activeCounter);
            if (ticker != null) return ticker.Title;                
            ticker = tickers.Values.FirstOrDefault(t => t.ActiveBase == activeCounter && t.ActiveCounter == activeBase);
            if (ticker != null)
            {
                inverse = true;
                return ticker.Title;
            }            
            return string.Empty;
        }

        /// <summary>
        /// ("USDJPY", false) -> JPY; ("EURGBP", true) -> EUR
        /// </summary>        
        public string GetActiveFromPair(string ticker, bool useBase)
        {
            TradeTicker tick;
            if (!tickers.TryGetValue(ticker, out tick))
            {
                var midIndex = ticker.Length / 2;
                return useBase ? ticker.Substring(0, midIndex) : ticker.Substring(midIndex);
            }

            return useBase ? tick.ActiveBase : tick.ActiveCounter;
        }

        /// <param name="ticker">"EURUSD"</param>
        /// <param name="useBase">true - искать курс EUR/DEPO, иначе искать USD/DEPO</param>
        /// <param name="curxDepo">валюта депо (например, USD)</param>
        /// <param name="inverse">вместо EUR/DEPO (например) вернуть DEPO/EUR и true</param>
        /// <param name="pairsEqual">если контрвалюта и есть валюта депо</param>
        public string FindSymbol(string ticker, bool useBase, string curxDepo, out bool inverse, out bool pairsEqual)
        {
            inverse = false;
            pairsEqual = false;

            // найти тикер
            TradeTicker tick;           
            if (tickers.TryGetValue(ticker, out tick))
            {
                var srcCurx = useBase ? tick.ActiveBase : tick.ActiveCounter;
                if (srcCurx == curxDepo)
                {
                    pairsEqual = true;
                    return string.Empty;
                }
                return FindSymbol(srcCurx, curxDepo, out inverse);
            }

            // попытаться распарсить имя
            if ((ticker.Length & 1) != 0) // нечетное кол-во символов
                return "";
            var strCurx = useBase
                              ? ticker.Substring(0, ticker.Length/2)
                              : ticker.Substring(ticker.Length/2);
            if (strCurx == curxDepo)
            {
                pairsEqual = true;
                return "";
            }
            return FindSymbol(strCurx, curxDepo, out inverse);
        }

        public bool ConvertTickerNaming(string nameOrig, out string nameResult,
            TickerNamingStyle styleOrig, TickerNamingStyle styleDest)
        {
            nameResult = nameOrig;
            if (styleOrig == styleDest) return true;
            if (styleOrig == TickerNamingStyle.ПолныйСРазделителем && 
                styleDest == TickerNamingStyle.Системный)
            {
                nameResult = nameOrig.Replace("/", "").ToUpper();
                return true;
            }
            if (styleOrig == TickerNamingStyle.Системный &&
                styleDest == TickerNamingStyle.ПолныйСРазделителем)
            {
                TradeTicker ticker;
                if (!tickers.TryGetValue(nameOrig, out ticker))
                    return false;
                nameResult = string.Format("{0}/{1}", ticker.ActiveBase, ticker.ActiveCounter);
                return true;
            }
            return false;
        }

        /// <summary>
        /// прочитать из файла словарь тикер - дата первой котировки в БД
        /// словарь заполняется методом UpdateTickerFirstQuoteTimeInDatabase
        /// </summary>
        public static Dictionary<string, DateTime> GetTickerFirstQuoteTimeInDatabase(string filepath)
        {
            if (!File.Exists(filepath)) return new Dictionary<string, DateTime>();
            var dic = new Dictionary<string, DateTime>();
            using (var fs = new StreamReader(filepath, Encoding.Unicode))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var parts = line.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;
                    var ticker = parts[0];
                    var time = parts[1].ToDateTimeUniformSafe();
                    if (!time.HasValue) continue;
                    dic.Add(ticker, time.Value);
                }
            }
            return dic;
        }

        public static void UpdateTickerFirstQuoteTimeInDatabase(Dictionary<string, DateTime> tickerData, 
            string filepath)
        {
            var existData = GetTickerFirstQuoteTimeInDatabase(filepath);
            foreach (var pair in tickerData)
                if (existData.ContainsKey(pair.Key))
                    existData[pair.Key] = pair.Value;
                else
                    existData.Add(pair.Key, pair.Value);

            using (var fs = new StreamWriter(filepath, false, Encoding.Unicode))
            {
                foreach (var pair in existData)
                {                    
                    var line = string.Format("{0};{1}", pair.Key, pair.Value.ToStringUniform());
                    fs.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// пример: ("USDCHF", true, "EUR", 20000, [...], error)
        /// ищется котировка "USD" + "EUR" (EURUSD)
        /// находится значение: EURUSD = 1.3200
        /// на выходе: 20000 * 1.3200 = 26400
        /// </summary>        
        public decimal? ConvertToTargetCurrency(
            string destTicker, bool useBase, string srcCurrency, double srcVolume, 
            Dictionary<string, QuoteData> quotes,
            out string errorString, bool useAvgPrice = false)
        {
            const bool divideSrcVolume = false; //!useBase;
            errorString = string.Empty;
            bool inverse, equal;
            var ticker = FindSymbol(destTicker, useBase, srcCurrency, out inverse, out equal);
            if (equal) return (decimal)srcVolume;
            if (string.IsNullOrEmpty(ticker))
            {
                errorString = "не найден тикер";
                return null;
            }
            QuoteData quote;
            quotes.TryGetValue(ticker, out quote);
            if (quote == null)
            {
                errorString = "нет котировки " + ticker;
                return null;
            }
            if (useAvgPrice)
            {
                var price = quote.GetPrice(QuoteType.Middle);
                quote.ask = price;
                quote.bid = price;
            }

            return divideSrcVolume
                    ? inverse ? (decimal)quote.bid * (decimal)srcVolume : (decimal)srcVolume / (decimal)quote.ask
                    : !inverse ? (decimal)quote.bid * (decimal)srcVolume : (decimal)srcVolume / (decimal)quote.ask;
        }

        public decimal? ConvertSourceCurrencyToTargetCurrency(
            string destCurrency, string srcCurrency, double srcVolume,
            Dictionary<string, QuoteData> quotes,
            out string errorString, bool divideSrcVolume = true)
        {
            errorString = string.Empty;

            if (srcCurrency == destCurrency)
                return (decimal)srcVolume;

            bool inverse;
            var ticker = FindSymbol(destCurrency, srcCurrency, out inverse);
            if (string.IsNullOrEmpty(ticker))
            {
                errorString = "не найден тикер " + destCurrency + " / " + srcCurrency;
                return null;
            }
            QuoteData quote;
            quotes.TryGetValue(ticker, out quote);
            if (quote == null)
            {
                errorString = "нет котировки " + ticker;
                return null;
            }
            return divideSrcVolume
                    ? inverse ? (decimal)quote.bid * (decimal)srcVolume : (decimal)srcVolume / (decimal)quote.ask
                    : !inverse ? (decimal)quote.bid * (decimal)srcVolume : (decimal)srcVolume / (decimal)quote.ask;
        }

        /// <summary>
        /// посчитать текущую прибыль / убыток по открытым ордерам,
        /// для ордеров заполнить поля: цена выхода, результат в контрвалюте и валюте депо, пункты
        /// </summary>
        public float CalculateOpenedPositionsCurrentResult(
            List<MarketOrder> orders, Dictionary<string, QuoteData> quotes, string depoCurrency)
        {
            bool noQuoteError;
            return CalculateOpenedPositionsCurrentResult(orders, quotes, depoCurrency, out noQuoteError);
        }

        /// <summary>
        /// посчитать текущую прибыль / убыток по открытым ордерам,
        /// для ордеров заполнить поля: цена выхода, результат в контрвалюте и валюте депо, пункты
        /// </summary>
        public float CalculateOpenedPositionsCurrentResult(
            List<MarketOrder> orders, Dictionary<string, QuoteData> quotes, string depoCurrency, out bool noQuoteError)
        {
            noQuoteError = false;
            var profit = 0f;
            foreach (var order in orders.Where(o => o.State == PositionState.Opened))
            {
                // найти котировку с ценой выхода
                QuoteData quote;
                if (!quotes.TryGetValue(order.Symbol, out quote))
                {
                    noQuoteError = true;
                    continue;
                }
                var priceExit = order.Side > 0 ? quote.bid : quote.ask;
                order.PriceExit = priceExit;

                // контрвалюта и пункты
                order.ResultBase = order.Volume*order.Side*(priceExit - order.PriceEnter);
                order.ResultPoints = GetPointsValue(order.Symbol, order.Side*(priceExit - order.PriceEnter));

                // профит в валюте депо
                string errorStr;
                var profitDepo = ConvertToTargetCurrency(order.Symbol, false, depoCurrency,
                    order.ResultBase, quotes, out errorStr);
                if (!profitDepo.HasValue)
                {
                    noQuoteError = true;
                    continue;
                }
                order.ResultDepo = (float)profitDepo.Value;
                profit += order.ResultDepo;
            }
            return profit;
        }

        /// <summary>
        /// вернуть минимальный объем входа для данного тикера - данной группы
        /// </summary>
        public Cortege2<int, int> GetMinStepLot(string ticker, string accountGroup)
        {
            TradeTicker tick;
            if (!tickers.TryGetValue(ticker, out tick))
                return new Cortege2<int, int>(10000, 10000);

            var lot = dictionaryGroupLot.GetMinStepLot(accountGroup, ticker);
            return lot ?? new Cortege2<int, int>(10000, 10000);
        }

        /// <summary>
        /// для Открытого! ордера посчитать его профит в валюте депозита
        /// </summary>
        /// <param name="order">собственно ордер</param>
        /// <param name="quotes">словарь котировок - могут понадобиться 1..2 котировки</param>
        /// <param name="accountCurrency">валюта депозита</param>
        public float? CalculateProfitInDepoCurrency(MarketOrder order, Dictionary<string, QuoteData> quotes, string accountCurrency)
        {
            var curDepo = accountCurrency;
            if (string.IsNullOrEmpty(curDepo)) return null;

            // посчитать профит в контрвалюте
            QuoteData quote;
            if (!quotes.TryGetValue(order.Symbol, out quote)) return null;
            var profitInCounter = order.CalculateProfit(quote);
            
            // пересчет в валюту депо
            bool inverseRate, pairsAreEqual;
            var smbCounterDepo = FindSymbol(order.Symbol, false,
                curDepo, out inverseRate, out pairsAreEqual);
            if (pairsAreEqual) return profitInCounter;

            if (!quotes.TryGetValue(smbCounterDepo, out quote)) return null;
            var price = ((profitInCounter > 0 && inverseRate) || (profitInCounter < 0 && !inverseRate)) ?
                quote.ask : quote.bid;
            return inverseRate ? profitInCounter / price : profitInCounter * price;
        }

        /// <summary>
        /// считать экспозицию по счету, во многом аналогична CalculateProfitInDepoCurrency
        /// </summary>
        public decimal CalculateExposure(List<MarketOrder> orders, Dictionary<string, QuoteData> quotes,
                                          string accountCurrency, List<string> processingErrors)
        {
            var exposure = 0M;

            foreach (var group in orders.GroupBy(o => o.Symbol))
            {
                var ticker = group.Key;
                var sumVolumeBase = Math.Abs(group.Sum(g => g.Volume*g.Side));
                if (sumVolumeBase == 0) continue;

                string strError;
                exposure +=
                    ConvertToTargetCurrency(ticker, true, accountCurrency, sumVolumeBase, quotes, out strError) ?? 0;
                if (!string.IsNullOrEmpty(strError) && processingErrors != null)
                    processingErrors.Add(strError);
            }

            return exposure;
        }

        /// <summary>
        /// считаемтся суммарный ордер
        /// текущая котировка берется из curQuote, если таковая null - из
        /// avgPrice
        /// </summary>
        public MarketOrder CalculateSummaryOrder(List<MarketOrder> orders, 
            QuoteData curQuote, float? avgPrice)
        {
            if (orders.Count == 0) return null;
            if (orders.Count == 1) return orders[0].MakeCopy();
            var symbol = orders[0].Symbol;

            float sumBuys = 0, sumSells = 0;
            var sumVolume = 0;
            var totalProfit = 0f;
            foreach (var order in orders)
            {
                sumVolume += order.Side * order.Volume;
                if (order.Side > 0)
                    sumBuys += order.Volume * order.PriceEnter;
                else
                    sumSells += order.Volume * order.PriceEnter;
                totalProfit += order.ResultDepo;
            }
            var miniPipCost = GetAbsValue(symbol, 1/10f);

            if (sumVolume != 0)
            {
                var averagePrice = (sumBuys - sumSells) / sumVolume;
                var avgOrder = new MarketOrder
                {
                    Symbol = orders[0].Symbol,
                    PriceEnter = averagePrice,
                    ResultDepo = totalProfit,
                    Side = Math.Sign(sumVolume),
                    Volume = Math.Abs(sumVolume),
                    StopLoss = GetDefaultSample(orders.Select(o => o.StopLoss), true, miniPipCost),
                    TakeProfit = GetDefaultSample(orders.Select(o => o.TakeProfit), true, miniPipCost),
                };

                var price = avgPrice;
                if (curQuote != null)
                    price = avgOrder.Side > 0 ? curQuote.bid : curQuote.ask;

                avgOrder.ResultPoints = price.HasValue
                    ? (int)Math.Round(GetPointsValue(symbol, avgOrder.Side * (price.Value -
                    avgOrder.PriceEnter))) : 0;
                return avgOrder;
            }

            return null;
        }

        /// <summary>
        /// если передан список null, 1.3520, 1.3520 вернется null
        /// если передан список 1.3410, 1.34103 - вернется 1.3410
        /// ... 1.3420, 1.3315 - вернется null
        /// </summary>
        private static float? GetDefaultSample(IEnumerable<float?> values,
            bool nilAsNull, float maxDeltaToCountSame)
        {
            float? sample = null;
            foreach (var val in values)
            {
                if (val == null || (val == 0 && nilAsNull)) continue;
                if (sample.HasValue)
                {
                    var delta = Math.Abs(sample.Value - val.Value);
                    if (delta > maxDeltaToCountSame)
                        return null;
                    continue;
                }
                sample = val;
            }

            return sample;
        }
    }
}
