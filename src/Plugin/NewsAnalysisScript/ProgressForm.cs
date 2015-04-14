using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Entity;
using NewsRobot;
using TradeSharp.Client;
using TradeSharp.Util;

namespace NewsAnalysisScript
{
    public partial class ProgressForm : Form
    {
        private List<RobotNews> robotNews = new List<RobotNews>();
        private List<string> quotesFileNames;
        private readonly Dictionary<string, List<CandleData>> fileQuotes = new Dictionary<string, List<CandleData>>();
        private readonly List<RobotNewsStat> robotNewsStats = new List<RobotNewsStat>();
        private readonly BackgroundWorker bw = new BackgroundWorker();
        private readonly object pendingMessagesLocker = new object();
        private readonly List<string> pendingMessages = new List<string>();
        private List<string> workingCurrencies;
        private DateTime startTime;
        private DateTime endTime;
        private bool onlyValuableNews;

        public ProgressForm()
        {
            InitializeComponent();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Work;
            bw.ProgressChanged += OnProgressChange;
            bw.RunWorkerCompleted += OnComplete;
        }

        public void SetNewsFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;
            robotNews = RobotNews.LoadFromFile(fileName);
            AddPendingMessage(string.Format("Прочитано новостей: {0} из {1}", robotNews.Count, fileName));
        }

        public void SetQuotesFileNames(string[] fileNames)
        {
            quotesFileNames = fileNames.ToList();
        }

        public void Start(DateTime start, DateTime end, List<string> currencies, bool valuableNews)
        {
            startTime = start;
            endTime = end;
            workingCurrencies = currencies;
            onlyValuableNews = valuableNews;
            bw.RunWorkerAsync();
            closeButton.Text = "Прервать";
        }

        public List<RobotNews> GetRobotNews()
        {
            return bw.IsBusy ? null : robotNews;
        }

        public List<RobotNewsStat> GetRobotNewsStats()
        {
            return bw.IsBusy ? null : robotNewsStats;
        }

        private void Work(object sender, DoWorkEventArgs args)
        {
            var newsRobotFileName = ExecutablePath.ExecPath + "\\plugin\\NewsRobot.dll.xml";
            string error;
            var settings = NewsRobot.CurrencySettings.LoadCurrencySettings(newsRobotFileName, out error);
            if (error != null)
            {
                AddPendingMessage(error);
                return;
            }
            var calc = new CurrencyIndexCalculator();
            var selectedCurrencies = new List<string>();
            foreach (var selectedCurrency in workingCurrencies)
            {
                var c = selectedCurrency.Split(new[] { ' ' })[1];
                if (!selectedCurrencies.Contains(c))
                    selectedCurrencies.Add(c);
            }

            //if (fileQuotes.Count == 0)
            if(quotesFileNames.Count != 0)
            {
                for (var i = 0; i < quotesFileNames.Count; i++)
                {
                    var fileName = quotesFileNames[i];
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                    var info = new FileInfo(fileName);
                    if (!info.Exists)
                    {
                        AddPendingMessage(string.Format("Файл не найден: {0}", fileName));
                        continue;
                    }
                    var smb = info.Name.Replace(info.Extension, null);
                    var candles = CandleData.LoadFromFile(fileName, smb);
                    fileQuotes[smb] = candles;
                    AddPendingMessage(string.Format("Прочитано котировок {0}: {1} из {2}", smb, candles.Count, fileName));
                    bw.ReportProgress((i + 1) * 100 / quotesFileNames.Count);
                }
            }
            else
            {
                // составляем валютные пары по выбранным валютам
                var tickersToUpdate = new List<string>();
                /*var allTickers = DalSpot.Instance.GetTickerNames();
                foreach (var с in selectedCurrencies)
                {
                    var currency = с;
                    tickersToUpdate.AddRange(allTickers.Where(t => t.StartsWith(currency)));
                    tickersToUpdate.AddRange(allTickers.Where(t => t.EndsWith(currency)));
                }*/
                foreach (var c in selectedCurrencies)
                {
                    tickersToUpdate.AddRange(calc.GetIndexDependencies(c, out error));
                    AddPendingMessage(error);
                }
                // обновляем данные по валютным парам
                var tickersAndDates = new Dictionary<string, DateTime>();
                foreach (var ticker in tickersToUpdate)
                {
                    if (!tickersAndDates.ContainsKey(ticker))
                        tickersAndDates.Add(ticker, startTime);
                }
                AddPendingMessage("Получение котировок");
                MainForm.Instance.UpdateTickersCacheForRobots(tickersAndDates, 60);
            }

            // читаем новости в кэш
            if(robotNews.Count == 0)
            {
                AddPendingMessage("Получение новостей");
                for (var day = startTime; day <= endTime; day += new TimeSpan(1, 0, 0, 0))
                {
                    List<string> errors;
                    var dailyNews = NewsRobot.NewsRobot.GrabNews(day, out errors);
                    robotNews.AddRange(dailyNews);
                    AddPendingMessages(errors);
                    bw.ReportProgress((int) ((day - startTime).TotalDays + 1) * 100 /
                                      (int) ((endTime - startTime).TotalDays + 1));
                }
            }

            // обрабатываем новости
            AddPendingMessage("Обработка новостей");
            for (var day = startTime; day <= endTime; day += new TimeSpan(1, 0, 0, 0))
            {
                var newsDay = day;
                var dailyNews = robotNews.Where(news => news.Time.Date == newsDay.Date);
                foreach (var dn in dailyNews)
                {
                    var news = dn;
                    if (news.Valuable != onlyValuableNews)
                        continue;
                    var countrySettings = settings.Where(s => s.CountryCode == news.CountryCode).ToList();
                    if (countrySettings.Count() == 0)
                    {
                        //AddPendingMessage("CurrencyIndexCalculator:ActivateScript: no associated currency with country " + news.CountryCode);
                        continue;
                    }
                    var currencyCode = countrySettings[0].CurrencyCode;
                    if (!selectedCurrencies.Contains(currencyCode))
                        continue;
                    List<string> errors;
                    var delta = calc.GetIndexDelta(currencyCode, news.Time, 15, fileQuotes, out errors);
                    //AddPendingMessages(errors);
                    if (delta.HasValue)
                    {
                        var newsValueSign = 0;
                        if (news.Value > news.ProjectedValue)
                            newsValueSign = 1;
                        else if (news.Value < news.ProjectedValue)
                            newsValueSign = -1;
                        if (newsValueSign != 0 && delta.Value != 0)
                            AddStat(news.CountryCode, news.Title, delta.Value, (delta.Value > 0) == (news.Value > news.ProjectedValue), news.Time);
                    }
                }
                if (bw.CancellationPending)
                {
                    args.Cancel = true;
                    break;
                }
                bw.ReportProgress((int)((day - startTime).TotalDays + 1) * 100 / (int)((endTime - startTime).TotalDays + 1));
            }
        }

        private void AddStat(string countryCode, string newsTitle, double deltaIndex, bool indexFollowByNews, DateTime time)
        {
            var newsStat = robotNewsStats.FirstOrDefault(rns => rns.CountryCode == countryCode && rns.Title == newsTitle);
            if (newsStat == null)
            {
                newsStat = new RobotNewsStat { CountryCode = countryCode, Title = newsTitle };
                robotNewsStats.Add(newsStat);
            }
            newsStat.DeltaIndexes.Add(deltaIndex);
            newsStat.IndexFollowByNewsFlags.Add(indexFollowByNews);
            newsStat.Times.Add(time);
            AddPendingMessage(string.Format("ProgressForm::AddStat: {2}\t{3}\t{4}\t[{0}] {1}",
                                            countryCode, newsTitle, deltaIndex, indexFollowByNews, time));
        }

        private void OnProgressChange(object sender, ProgressChangedEventArgs args)
        {
            UpdateMessages();
            progressBar1.Value = args.ProgressPercentage;
        }

        private void OnComplete(object sender, RunWorkerCompletedEventArgs args)
        {
            UpdateMessages();
            if (args.Cancelled)
                richTextBox1.AppendText("Прервано");
            else
                richTextBox1.AppendText("Готово");
            closeButton.Text = "Готово";
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            if(bw.IsBusy)
                bw.CancelAsync();
            else
                DialogResult = DialogResult.OK;
        }

        private void AddPendingMessage(string error)
        {
            if (string.IsNullOrEmpty(error))
                return;
            lock (pendingMessagesLocker)
                pendingMessages.Add(error);
        }

        private void AddPendingMessages(IEnumerable<string> errors)
        {
            if (errors == null)
                return;
            lock (pendingMessagesLocker)
                pendingMessages.AddRange(errors);
        }

        private void UpdateMessages()
        {
            lock (pendingMessagesLocker)
            {
                foreach (var pendingMessage in pendingMessages)
                    richTextBox1.AppendText(pendingMessage + "\n");
                pendingMessages.Clear();
            }
        }
    }
}
