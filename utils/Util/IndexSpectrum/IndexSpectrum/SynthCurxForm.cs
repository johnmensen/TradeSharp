using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FXI.Client.MtsBase.Common;
using IndexSpectrum.Index;
using CurrencyIndexInfo=IndexSpectrum.Index.CurrencyIndexInfo;
using CurrencyStream=IndexSpectrum.Index.CurrencyStream;

namespace IndexSpectrum
{
    public partial class SynthCurxForm : Form
    {
        private readonly string quotePath;
        private readonly int intervalMinutes;
        private readonly int maxDataLen;
        private DateTime? startDate;

        public SynthCurxForm()
        {
            InitializeComponent();
        }

        public SynthCurxForm(string quotePath, int intervalMinutes, int maxDataLen,
            DateTime? startDate)
        {
            InitializeComponent();
            this.quotePath = quotePath;
            this.intervalMinutes = intervalMinutes;
            this.maxDataLen = maxDataLen;
            this.startDate = startDate;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog {CheckFileExists = false, Title = "Сохранить результат", DefaultExt = "xls"};
            if (dlg.ShowDialog() == DialogResult.OK) tbFolder.Text = dlg.FileName;
        }

        private void btnFormQuote_Click(object sender, EventArgs e)
        {
            var ciEur = new CurrencyIndexInfo(tbEur.Text);
            var ciUsd = new CurrencyIndexInfo(tbUsd.Text);            

            var currencyStream = new Dictionary<string, CurrencyStream>();
            // открыть потоки чтения
            var allPairs = ciEur.pairs.Union(ciUsd.pairs).ToList();
            if (!allPairs.Contains("eur")) allPairs.Add("eur");
            foreach (var curName in allPairs)
            {
                currencyStream.Add(curName, new CurrencyStream(curName, quotePath));
            }

            // сессия (разрешенные часы)
            int startDay = 1, endDay = 6;
            int startHr = 1, endHr = 1;
            var sessParts = tbSession.Text.Split(new[] {' ', ':', '-', ',', '.'}, StringSplitOptions.RemoveEmptyEntries);
            if (sessParts.Length == 4)
            {
                startDay = int.Parse(sessParts[0]);
                startHr = int.Parse(sessParts[1]);
                endDay = int.Parse(sessParts[2]);
                endHr = int.Parse(sessParts[3]);
            }
            var k = tbMultiplier.Text.ToDecimalArrayUniform();                
            // считать индекс            
            try
            {
                if (!startDate.HasValue)
                    startDate = IndexFFT.GetFirstDateFromCurStreams(currencyStream, false);
                DateTime date = startDate.Value;

                using (var swEur = new StreamWriter(tbFolder.Text + ".i1.quote"))
                using (var swUsd = new StreamWriter(tbFolder.Text + ".i2.quote"))
                using (var swEurUsd = new StreamWriter(tbFolder.Text + ".fr.quote"))
                {// eur0.3349# jpy0.1652 gbp0.082# cad0.2887 sek0.0189 chf0.027
                    double? prevEur = null, prevUsd = null;
                    for (var i = 0; i < maxDataLen; i++)
                    {
                        var eur = ciEur.CalculateIndexMultiplicative(date, currencyStream);
                        var usd = ciUsd.CalculateIndexMultiplicative(date, currencyStream);
                        if (ciEur.EndOfStream || ciUsd.EndOfStream) break;
                        eur = eur * (double)k[0];
                        usd = usd * (double)k[1];

                        var duplicates = false;
                        if (prevEur.HasValue)
                            if (prevEur.Value == eur && prevUsd.Value == usd)
                                duplicates = true;

                        if (!duplicates)
                        {
                            prevEur = eur;
                            prevUsd = usd;

                            // 11.370;11.350;03.01.2009 23:04:00
                            var dateStr = date.ToString("dd.MM.yyyy HH:mm:ss");
                            var line = string.Format(CultureInfo.InvariantCulture,
                                                     "{0:f4};{0:f4};{1}", eur, dateStr);
                            swEur.WriteLine(line);
                            line = string.Format(CultureInfo.InvariantCulture,
                                                 "{0:f4};{0:f4};{1}", usd, dateStr);
                            swUsd.WriteLine(line);
                            if (eur > 0 && usd > 0)
                            {
                                var fract = (double) k[2]*eur/usd;
                                line = string.Format(CultureInfo.InvariantCulture,
                                                     "{0:f4};{0:f4};{1}", fract, dateStr);
                                swEurUsd.WriteLine(line);
                            }
                        }

                        date = date.AddMinutes(intervalMinutes);
                        // пропуск выходных
                        var weekDay = (int) date.DayOfWeek;
                        if (weekDay == 0) weekDay = 7;

                        if ((weekDay > endDay || (weekDay == endDay && date.Hour >= endHr))
                            ||
                            (weekDay < startDay || (weekDay == startDay && date.Hour < startHr)))
                        {
                            var newWeekDay = date.Date.AddDays(7 - weekDay + startDay);
                            date = newWeekDay.AddHours(startHr);
                        }
                    }
                }
            }
            finally
            {   // закрыть потоки чтения
                foreach (var cs in currencyStream) cs.Value.CloseStream();
            }
        }

        private void btnSaveInDb_Click(object sender, EventArgs e)
        {
            if (!File.Exists(tbFolder.Text)) return;
            const int commandsPackSize = 50;
            int packSize = 0, rowsWritten = 0, linesTotal = 0;
            var cmdPackTxt = new StringBuilder();
            var timeStart = DateTime.Now;
            int codeMaker = int.Parse(tbDbMakerCode.Text);
            int codeCurrency = int.Parse(tbDbCurrencyCode.Text);

            // прочитать указанный файл, сформировать записи вида INSERT INTO QUOTE ...
            var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    using (var sr = new StreamReader(tbFolder.Text))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            if (string.IsNullOrEmpty(line)) continue;
                            // получить котировку из строки
                            var cmd = MakeInsertCmdText(line, codeCurrency, codeMaker);
                            if (string.IsNullOrEmpty(cmd)) continue;
                            cmdPackTxt.AppendLine(cmd);
                            packSize++;
                            linesTotal++;
                            if (packSize > commandsPackSize)
                            {
                                packSize = 0;
                                var sqlCmd = new SqlCommand(cmdPackTxt.ToString()) {Connection = connection};
                                cmdPackTxt = new StringBuilder();
                                rowsWritten += sqlCmd.ExecuteNonQuery();
                            }
                        }
                        if (packSize > 0)
                        {
                            var sqlCmd = new SqlCommand(cmdPackTxt.ToString()) { Connection = connection };
                            rowsWritten += sqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            var timeElapsed = DateTime.Now - timeStart;

            MessageBox.Show(string.Format("Записано {0} строк из {1} за {2} минут {3} секунд",
                                          rowsWritten, linesTotal, timeElapsed.TotalMinutes, timeElapsed.Seconds));
        }
        private static string MakeInsertCmdText(string strQuote, int trdCurrency, int maker)
        {
            if (string.IsNullOrEmpty(strQuote)) return string.Empty;
            var parts = strQuote.Split(';');
            if (parts.Length != 3) return string.Empty;
            var price = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
            var date = DateTime.ParseExact(parts[2], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Format("INSERT INTO QUOTE(trd_currency,maker,date,volume,bid,ask) VALUES('{0}'," +
                "'{1}','{2:yyyyMMdd HH:mm}','0','{3}','{3}')",
                trdCurrency, maker, date, price.ToStringUniform());
        }
    }    
}
