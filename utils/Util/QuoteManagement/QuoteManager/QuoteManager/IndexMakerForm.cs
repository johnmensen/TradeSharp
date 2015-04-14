using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Entity;
using MTS.Live.Contract.Entity;
using MTS.Live.Util;
using QuoteManager.BL;
using QuoteManager.Index;

namespace QuoteManager
{
    public partial class IndexMakerForm : Form
    {
        private readonly int quoteMakerCode = AppConfig.GetIntParam("Quote.DefaultMaker", 1);
        private readonly string quoteFolder;
        private readonly List<QuoteFileInfo> quoteFiles;

        public IndexMakerForm()
        {
            InitializeComponent();
        }

        public IndexMakerForm(string quoteFolder, List<QuoteFileInfo> quoteFiles)
        {
            InitializeComponent();
            this.quoteFolder = quoteFolder;
            this.quoteFiles = quoteFiles;
        }

        private void IndexMakerFormLoad(object sender, EventArgs e)
        {
            // загрузить "подсказку" по индексам
            var fileName = string.Format("{0}\\indicies.txt", ExecutablePath.ExecPath);
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName))
                {
                    rtbHelper.Text = sr.ReadToEnd();
                }
            }

            // даты
            dpStart.Value = dpEnd.Value.AddDays(-365);
        }

        private void BtnCheckExistingClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbTickerFormula.Text)) return;
            // проверить наличие нужных тикеров в файловой БД
            // разобрать формулу
            ExpressionResolver resv;
            try
            {
                resv = new ExpressionResolver(tbTickerFormula.Text);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка разбора выражения", ex);
                MessageBox.Show(string.Format("Ошибка разбора выражения ({0})", ex.Message));
                return;
            }
            
            var names = resv.GetVariableNames();
            // проверить доступность - каждое имя должно быть тикером
            var allNames = DalSpot.Instance.GetCurrencyNames();
            var errorsStr = new StringBuilder();
            foreach (var name in names)
            {
                var tickerName = name;
                if (!allNames.Any(n => n.Equals(tickerName, StringComparison.OrdinalIgnoreCase)))                
                    errorsStr.AppendLine(string.Format("Тикер \"{0}\" не найден", name));                
            }
            if (errorsStr.Length > 0)
            {
                MessageBox.Show(errorsStr.ToString());
                return;
            }
            
            // наличие истории по тикерам
            tbExistingTickers.Text = string.Empty;
            var sbExist = new StringBuilder();
            foreach (var name in names)
            {
                var tickerName = name;
                var fileInfo = quoteFiles.FirstOrDefault(f => f.TickerName.Equals(tickerName, 
                    StringComparison.OrdinalIgnoreCase));
                DateTime? start = null, end = null;
                if (fileInfo != null)
                {
                    start = fileInfo.StartDate;
                    end = fileInfo.EndDate;
                }
                var isInsufficient = !start.HasValue
                                         ? true
                                         : start.Value > dpStart.Value;
                if (!isInsufficient)
                    isInsufficient = (dpEnd.Value - end.Value).TotalMinutes > 30;
                sbExist.AppendLine(string.Format("{0}{1} - {2} - {3}",
                                                 isInsufficient ? "[!]" : "", name,
                                                 start.HasValue ? start.Value.ToString("dd.MM.yyyy") : "...",
                                                 end.HasValue ? end.Value.ToString("dd.MM.yyyy HH:mm") : "..."));
            }
            tbExistingTickers.Text = sbExist.ToString();
        }

        private void BtnSelectTickerClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbTicker.Text)) return;
            tbTickerId.Text = DalSpot.Instance.GetFXICodeBySymbol(tbTicker.Text).ToString();
        }

        private void BtnMakeIndexClick(object sender, EventArgs e)
        {
            ExpressionResolver resv;
            try
            {
                resv = new ExpressionResolver(tbTickerFormula.Text);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка разбора выражения", ex);
                MessageBox.Show(string.Format("Ошибка разбора выражения ({0})", ex.Message));
                return;
            }

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            var fileName = saveFileDialog.FileName;
            
            var names = resv.GetVariableNames();
            var curs = new BacktestTickerCursor();
            try
            {
                curs.SetupCursor(quoteFolder, names, dpStart.Value);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка установки курсора", ex);
                return;
            }

            StreamWriter sw;
            try
            {
                sw = new StreamWriter(fileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка открытия файла на запись", ex);
                curs.Close();
                MessageBox.Show("Ошибка открытия файла на запись");
                return;
            }
            var saver = new QuoteSaver(tbTicker.Text);

            try
            {
                while (true)
                {
                    // посчитать индекс
                    var quotes = curs.GetCurrentQuotes();
                    if (quotes.Count == 0) continue;
                    var date = quotes.Max(q => q.b.time);

                    var quoteDic = quotes.ToDictionary(q => q.a, q => (double)q.b.bid);
                    double result;
                    resv.Calculate(quoteDic, out result);
                    
                    // занести индекс в файл
                    saver.SaveQuote(sw, (float)result, (float)result, date);
                    
                    if (!curs.MoveNext()) break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка формирования индекса", ex);                
            }
            finally
            {
                curs.Close();
                sw.Close();
            }

            MessageBox.Show("Формирование индекса завершено");
            openFileDialog.FileName = saveFileDialog.FileName;
        }

        /// <summary>
        /// загрузить котировки из файла и сохранить их в БД, пачками
        /// </summary>        
        private void BtnSaveInDbClick(object sender, EventArgs e)
        {
            var commandsBlock = new List<string>();
            const int maxCommandsInBlock = 50;
            var ticker = tbTickerId.Text.ToInt();
            SqlConnection connection = null;
            var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
            var startTime = dpStart.Value;
            var endTime = dpEnd.Value;

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var newFormat = false;
                connection = new SqlConnection(connectionString);
                connection.Open();
                using (var sr = new StreamReader(openFileDialog.FileName))
                {
                    DateTime? date = null;
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        DateTime? fileDate;
                        var isNew = QuoteData.IsNewFormatDateRecord(line, out fileDate);
                        if (fileDate.HasValue) date = fileDate;
                        if (isNew)
                        {
                            newFormat = true;
                            continue;
                        }
                        var quote = newFormat ? QuoteData.ParseQuoteStringNewFormat(line, date.Value) : 
                              QuoteData.ParseQuoteStringOldFormat(line);
                        if (quote == null) continue;
                        if (quote.time < startTime) continue;
                        if (quote.time > endTime) break;

                        var cmd = MakeInsertQuoteCmd(ticker, quote);
                        commandsBlock.Add(cmd);
                        if (commandsBlock.Count > maxCommandsInBlock)
                        {
                            ExecuteSaveCommands(commandsBlock, connection);
                            commandsBlock.Clear();
                        }
                    }
                }
                if (commandsBlock.Count > 0) ExecuteSaveCommands(commandsBlock, connection);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения в БД", ex);
                MessageBox.Show(string.Format("Ошибка сохранения в БД: {0}", ex.Message));
                return;
            }
            finally
            {
                if (connection != null) connection.Close();
            }
            MessageBox.Show("Котировки сохранены в БД");
        }

        private static void ExecuteSaveCommands(List<string> commands, SqlConnection connection)
        {
            var commStr = string.Join(" \n", commands);
            try
            {
                var cmd = new SqlCommand(commStr, connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения в БД", ex);
            }
        }

        private string MakeInsertQuoteCmd(int ticker, QuoteData q)
        {
            return string.Format("insert into QUOTE (trd_currency, date, bid, ask, volume, maker) values " +
                                 "('{0}', '{1:yyyyMMdd HH:mm:ss}', '{2:f4}', '{3:f4}', '0', '{4}')",
                                 ticker, q.time, q.bid, q.ask, quoteMakerCode);
        }

        private void BtnShowLastDbQuoteClick(object sender, EventArgs e)
        {
            var tickerId = tbTickerId.Text.ToIntSafe() ?? 0;
            if (tickerId == 0)
            {
                MessageBox.Show("Не выбран тикер ID");
                return;
            }

            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    var commandText = string.Format("select min(date) from QUOTE where trd_currency='{0}'",
                                                    tickerId);
                    var command = new SqlCommand(commandText, connection);
                    connection.Open();
                    var objTime = command.ExecuteScalar();
                    if (objTime != null)
                        dpStart.Value = (DateTime) objTime;
                    else                    
                        MessageBox.Show("Нет ответа от БД");
                    commandText = string.Format("select max(date) from QUOTE where trd_currency='{0}'",
                                                    tickerId);
                    command = new SqlCommand(commandText, connection);
                    objTime = command.ExecuteScalar();
                    if (objTime != null)
                        dpEnd.Value = (DateTime)objTime;
                    else
                        MessageBox.Show("Нет ответа от БД");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка обращения к БД", ex);
                MessageBox.Show(string.Format("Ошибка обращения к БД: {0}", ex.Message));
                return;
            }
        }

        private void BtnShowDbQuotesClick(object sender, EventArgs e)
        {
            var tickerId = tbTickerId.Text.ToIntSafe() ?? 0;
            if (tickerId == 0)
            {
                MessageBox.Show("Не указан Id тикера");
                return;
            }
            var tickerName = DalSpot.Instance.GetSymbolByFXICode(tickerId);
            if (string.IsNullOrEmpty(tickerName))
            {
                MessageBox.Show("Тикер с указанным Id не найден");
                return;
            }

            new TickerBaseDataForm(tickerName, tickerId).ShowDialog();
        }
    }
}
