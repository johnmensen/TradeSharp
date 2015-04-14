using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.QuoteHistory;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Util;

namespace MarkupCalculator.CommonClasses
{
    public class StatisticMaker
    {
        private string depoCurrency = "USD";
        public string DepoCurrency
        {
            get { return depoCurrency; }
            set { depoCurrency = value; }
        }

        /// <summary>
        /// Имена файлов со сделками
        /// </summary>
        private IEnumerable<string> DealFileNames { get; set; }

        /// <summary>
        /// Полное имя каталога с котировками
        /// </summary>
        private string QuoteFolderName { get; set; }


        /// <param name="dealFileNames">Имена файлов со сделками и транзакциями</param>
        /// <param name="quoteFolderName">Имя каталога с котировками</param>
        public StatisticMaker(IEnumerable<string> dealFileNames, string quoteFolderName)
        {
            QuoteFolderName = quoteFolderName;
            DealFileNames = dealFileNames;
        }

        readonly EquityCurveCalculator equityCurveCalculator = new EquityCurveCalculator();

        /// <summary>
        /// Основной метод, рассчитыввающий статистику
        /// </summary>
        public Dictionary<int, AccountPerformanceRaw> GetPerformanceStatistic(BackgroundWorker worker)
        {
            var accountPerformance = new Dictionary<int, AccountPerformanceRaw>();
            const int ProgressForQuotes = 50;
            const int ProgressForDeals = 50;

            try
            {
                var dicQuote = new Dictionary<string, List<QuoteData>>();
                var symbolsUsed = DalSpot.Instance.GetTickerNames();
                var progressCounter = 0;
                foreach (var smb in symbolsUsed)
                {
                    if (worker.CancellationPending) return accountPerformance;
                    dicQuote.Add(smb, LoadTickerQuotesFromFile(smb).Select(q => new QuoteData(q.b, q.b, q.a)).ToList());
                    var progress = (++progressCounter)*100/symbolsUsed.Length*ProgressForQuotes/100;
                    worker.ReportProgress(progress);
                }
                var quoteArc = new QuoteArchive(dicQuote);

                AccountDealContainer.SymbolsUsed = symbolsUsed;
                var containers = DealFileNames.Select(x => new AccountDealContainer(x)).ToList();

                progressCounter = 0;
                foreach (var container in containers)
                {
                    if (worker.CancellationPending) return accountPerformance;
                    accountPerformance.Add(container.Id,
                        equityCurveCalculator.CalculateEquityCurve(
                            container.Deals,
                            DepoCurrency,
                            quoteArc,
                            container.Transactions,
                            container.TimeStart,
                            container.TimeEnd)
                        );
                    var progress = ProgressForQuotes + (++progressCounter) * 100 / containers.Count * ProgressForDeals / 100;
                    worker.ReportProgress(progress);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetStat()", ex);
            }
            return accountPerformance;
        }
        

        /// <summary>
        /// загрузка данных о котировках из файлов каталога 'quotes'
        /// </summary>
        private IEnumerable<Cortege2<DateTime, float>> LoadTickerQuotesFromFile(string ticker)
        {
            var tickerQuotes = new List<Cortege2<DateTime, float>>();
            var fileName = QuoteFolderName + "\\" + ticker + ".txt";
            if (!File.Exists(fileName))
                return tickerQuotes;

            try
            {
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    var separators = new[] { ' ' };
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        var parts = line.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2) continue;
                        var price = parts[1].ToFloatUniformSafe();
                        if (!price.HasValue) continue;

                        DateTime time;
                        if (!DateTime.TryParseExact(parts[0], "ddMMyyyy", CultureProvider.Common, DateTimeStyles.None,
                                                    out time))
                            continue;
                        tickerQuotes.Add(new Cortege2<DateTime, float>(time, price.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in LoadTickerQuotesFromFile()", ex);
            }
            return tickerQuotes;
        }

        /// <summary>
        /// принудительное обновление котировок в каталоге 'quotes' данными с сервера
        /// </summary>
        private void UpdateQuotesInFolder()
        {
            var dailyQuoteStorage = new DailyQuoteStorage();
            dailyQuoteStorage.UpdateStorageSync();
        }
    }
}