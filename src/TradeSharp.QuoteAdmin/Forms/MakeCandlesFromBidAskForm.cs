using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.QuoteAdmin.Forms
{
    public partial class MakeCandlesFromBidAskForm : Form
    {
        private readonly IQuoteStorage quoteStorage;
        private readonly Dictionary<string, DateSpan> histOnServer;

        public MakeCandlesFromBidAskForm()
        {
            InitializeComponent();
            try
            {
                quoteStorage = Contract.Util.Proxy.QuoteStorage.Instance.proxy;
            }
            catch
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
                return;
            }

            histOnServer = quoteStorage.GetTickersHistoryStarts();
        }

        private void BtnLoadCandlesToFileClick(object sender, EventArgs e)
        {
            //if (quoteStorage == null)
            //{
            //    MessageBox.Show("Нет связи с сервером котировок");
            //    return;
            //}

            //if (string.IsNullOrEmpty(tbTicker.Text))
            //{
            //    MessageBox.Show("Не указан тикер");
            //    return;
            //}

            //var symbol = tbTicker.Text.Trim(new[] {' ', (char) 9}).ToUpper();
            //if (!DalSpot.Instance.GetTickerNames().Any(n => n == symbol))
            //{
            //    MessageBox.Show(string.Format("Тикер \"{0}\" не найден в словаре", symbol));
            //    return;
            //}

            //var nowTime = DateTime.Now;
            //var dateStart = nowTime;

            //DateSpan span;
            //if (histOnServer.TryGetValue(symbol, out span))
            //{
            //    dateStart = span.start;
            //}

            //if ((nowTime - dateStart).TotalMinutes < 5)
            //{
            //    MessageBox.Show(string.Format("Котировка \"{0}\" отсутствует в БД", symbol));
            //    return;
            //}

            //var quotes = new List<QuoteData>();
            //for (; dateStart < nowTime; dateStart = dateStart.AddDays(10))
            //{
            //    var dateEnd = dateStart.AddDays(10);
            //    if (dateEnd > nowTime) dateEnd = nowTime;
            //    if ((dateEnd - dateStart).TotalMinutes < 2) break;

            //    try
            //    {
            //        var quotesDb = quoteStorage.GetDayQuotes(symbol, dateStart, dateEnd);
            //        if (quotesDb == null || quotesDb.count == 0) continue;
            //        var quotesUnpacked = quotesDb.GetQuotes();
            //        if (quotesUnpacked != null && quotesUnpacked.Count > 0)
            //            quotes.AddRange(quotesUnpacked);
            //    }
            //    catch
            //    {
            //    }
            //}

            //if (quotes.Count == 0)
            //{
            //    MessageBox.Show(string.Format("Котировка \"{0}\" не прочитана", symbol));
            //    return;
            //}

            //// сохранить в файл
            //var dlg = new SaveFileDialog
            //              {
            //                  Title = "Сохранить " + symbol, 
            //                  DefaultExt = "quote", 
            //                  Filter = "Котировки (*.quote)|*.quote",
            //                  FilterIndex = 0
            //              };
            //if (!string.IsNullOrEmpty(tbFilePath.Text))
            //    dlg.FileName = tbFilePath.Text;
            //if (dlg.ShowDialog() == DialogResult.OK)
            //    tbFilePath.Text = dlg.FileName;

            //// слепить котировки m1 и сохранить их
            //var candles = quotes.Select(q => new CandleData(q.bid, q.bid, q.bid, q.bid, q.time, q.time)).ToList();
            //CandleData.SaveInFile(tbFilePath.Text, symbol, candles);
        }

        private void BtnLoadFileToDbClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbTicker.Text))
            {
                MessageBox.Show("Не указан тикер");
                return;
            }

            var symbol = tbTicker.Text.Trim(new[] { ' ', (char)9 }).ToUpper();
            if (!DalSpot.Instance.GetTickerNames().Any(n => n == symbol))
            {
                MessageBox.Show(string.Format("Тикер \"{0}\" не найден в словаре", symbol));
                return;
            }
            var pointCost = DalSpot.Instance.GetPrecision10(symbol);
            var code = DalSpot.Instance.GetFXICodeBySymbol(symbol);
            if (code == 0)
            {
                MessageBox.Show(string.Format("Тикер \"{0}\" не прописан в словаре", symbol));
                return;
            }

            if (string.IsNullOrEmpty(tbFilePath.Text))
            {
                MessageBox.Show("Не указан файл котировок");
                return;
            }

            if (!File.Exists(tbFilePath.Text))
            {
                MessageBox.Show("Указанный файл \"" + tbFilePath.Text + "\" не существует");
                return;
            }

            var candles = CandleData.LoadFromFile(tbFilePath.Text, symbol);
            if (candles == null || candles.Count == 0) return;

            // записать свечи в DB, используя BulkCopy
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ticker", typeof(short)));
            table.Columns.Add(new DataColumn("date", typeof(DateTime)));
            table.Columns.Add(new DataColumn("open", typeof(float)));
            table.Columns.Add(new DataColumn("HLC", typeof(int)));
            foreach (var candle in candles)
            {
                table.Rows.Add(new object[] { code, candle.timeOpen, candle.open, candle.GetHlcOffset16(pointCost)});
            }

            var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (new TimeLogger("insert into QUOTE table"))
                {
                    try
                    {
                        using (var cpy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null)
                                             {
                                                 DestinationTableName = "QUOTE",
                                                 BulkCopyTimeout = 1000 * 60 * 5
                                             })
                            cpy.WriteToServer(table);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Load file to DB() - error in bulk copy", ex);
                    }
                }
            }

            MessageBox.Show(string.Format("Записано {0} котировок", candles.Count));
        }

        /// <summary>
        /// Определить тикеры, по которым в таблице QUOTENEW история полней, чем в таблице QUOTE,
        /// и заполнить дыры в таблице QUOTE
        /// </summary>
        private void BtnFillAutoClick(object sender, EventArgs e)
        {
            //var tickersToStore = new Dictionary<string, DateTime>();
            //var minQuotesToStore = 1440 * 200;
            
            //// определить список котировок, по которым недостаточно истории
            //var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
            //using (var connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();
            //    foreach (var symbol in DalSpot.Instance.GetTickerNames())
            //    {
            //        var code = DalSpot.Instance.GetFXICodeBySymbol(symbol);
            //        if (code == 0) continue;

            //        var cmdCountQuote = new SqlCommand(string.Format("select COUNT(*) from QUOTE where ticker={0}", code)) 
            //            { Connection = connection };
            //        var countCandles = (int) cmdCountQuote.ExecuteScalar();
            //        if (countCandles >= minQuotesToStore) continue;
                    
            //        var cmdCountQuoteNew =
            //            new SqlCommand(string.Format("select COUNT(*) from QUOTENEW where ticker={0}", code))
            //                {Connection = connection};
            //        var countQuotes = (int) cmdCountQuoteNew.ExecuteScalar();
                    
            //        if ((countQuotes - countCandles) > 100) 
            //        {
            //            var cmdFirstDate = new SqlCommand(string.Format("select min(date) from QUOTENEW where ticker={0}", code)) 
            //                { Connection = connection };
            //            var startTime = (DateTime) cmdFirstDate.ExecuteScalar();
            //            tickersToStore.Add(symbol, startTime);
            //        }
            //    }
            //}

            //// цикл: выкачать - закачать
            //var table = new DataTable();
            //table.Columns.Add(new DataColumn("ticker", typeof(short)));
            //table.Columns.Add(new DataColumn("date", typeof(DateTime)));
            //table.Columns.Add(new DataColumn("open", typeof(float)));
            //table.Columns.Add(new DataColumn("HLC", typeof(int)));

            //using (var connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();

            //    foreach (var symbol in tickersToStore)
            //    {
            //        var quotes = new List<QuoteData>();
            //        var nowTime = DateTime.Now;
            //        var dateStart = symbol.Value;
            //        var code = DalSpot.Instance.GetFXICodeBySymbol(symbol.Key);

            //        for (; dateStart < nowTime; dateStart = dateStart.AddDays(10))
            //        {
            //            var dateEnd = dateStart.AddDays(10);
            //            if (dateEnd > nowTime) dateEnd = nowTime;
            //            if ((dateEnd - dateStart).TotalMinutes < 2) break;

            //            try
            //            {
            //                var quotesDb = quoteStorage.GetDayQuotes(symbol.Key, dateStart, dateEnd);
            //                if (quotesDb == null || quotesDb.count == 0) continue;
            //                var quotesUnpacked = quotesDb.GetQuotes();
            //                if (quotesUnpacked != null && quotesUnpacked.Count > 0)
            //                    quotes.AddRange(quotesUnpacked);
            //            }
            //            catch
            //            {
            //            }
            //        }

            //        if (quotes.Count == 0) continue;

            //        table.Rows.Clear();
            //        foreach (var quoteData in quotes)
            //        {
            //            table.Rows.Add((short) code, quoteData.time, quoteData.bid, 0x7F7F7F);
            //        }

            //        using (new TimeLogger("insert into QUOTE table"))
            //        {
            //            try
            //            {
            //                using (var cpy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null)
            //                                     {
            //                                         DestinationTableName = "QUOTE",
            //                                         BulkCopyTimeout = 1000*60*5
            //                                     })
            //                    cpy.WriteToServer(table);
            //            }
            //            catch (Exception ex)
            //            {
            //                Logger.Error("Load file to DB() - error in bulk copy", ex);
            //            }
            //        }
            //    } // foreach (symbol
            //} // using (connection
        }        
    }
}
