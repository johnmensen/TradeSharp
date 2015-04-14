using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.FakeUser.BL;
using TradeSharp.Linq;
using TradeSharp.QuoteHistory;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;
using Transliteration = TradeSharp.Util.Transliteration;

namespace TradeSharp.FakeUser
{
    /// <summary>
    /// весь этот проект создан для наполнения базы трейдунов
    /// чтобы платформа не стала "голым философом в вакууме"
    /// <image url="http://copypast.ru/uploads/posts/1370670187_gimage_843.jpg" scale="0,6" />
    /// </summary>
    public partial class MainForm : Form
    {
        class BackgroundWorkerTask
        {
            public enum BackgroundWorkerTaskType
            {
                LoadFromForexite = 0,
                LoadFromTradeSharp,
                FixTransactions,
                MakeJSON,
                MakeTradeHistoryInDB
            }

            public BackgroundWorkerTaskType Task { get; set; }

            public object DataParam { get; set; }

            public BackgroundWorkerTask() {}

            public BackgroundWorkerTask(BackgroundWorkerTaskType taskType, object ptr)
            {
                Task = taskType;
                DataParam = ptr;
            }
        }

        class HistoryMakerTaskParam
        {
            public TradeHistoryMaker historyMaker;

            public XmlDocument portfolioDoc;
        }

        #region Fake Data
        private static readonly Random rand = new Random();

        private static readonly string[] mailDomains = new string[]
            {
                "@mail.ru", "@gmail.com", "@yandex.ru"
            };

        private readonly List<int> accountIds = new List<int>();
        #endregion

        private readonly BackgroundWorker workerQuote = new BackgroundWorker();

        public MainForm()
        {
            InitializeComponent();
            workerQuote.WorkerSupportsCancellation = true;
            workerQuote.WorkerReportsProgress = true;
            workerQuote.DoWork += WorkerQuoteOnDoWork;
            workerQuote.ProgressChanged += (sender, args) =>
            {
                lblWorkerProgress.Text = "прогресс: " + args.ProgressPercentage;
            };
            workerQuote.RunWorkerCompleted += WorkerQuoteOnRunWorkerCompleted;

            RobotCollection.Initialize();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var users = MakeUsersWithNames();
            if (users.Count == 0)
            {
                MessageBox.Show("Пользователи не созданы (нет записей)");
                return;
            }
            accountIds.Clear();

            var depoPercentiles = tbStartDepo.Text.ToIntArrayUniform();

            // каждого занести в БД
            // для каждого завести кошелек, положить немного денег
            // завести каждому счет
            // опционально - завести каждому сигнальный сервис
            var group = (string) cbGroup.SelectedItem;
            const int moneyOnWallet = 1250;
            var currency = tbCurrency.Text;
            var signalPrice = tbSignalCost.Text.ToDecimalUniformSafe() ?? 1;
            var timeParts = tbNewAccountOpenTime.Text.Split(new[] {'-'},
                StringSplitOptions.RemoveEmptyEntries);
            var timeStartMin = timeParts[0].Trim(' ').ToDateTimeUniform();
            var timeStartMax = timeParts[1].Trim(' ').ToDateTimeUniform();
            var minutesBackMax = (int)(timeStartMax - timeStartMin).TotalMinutes;

            using (var ctx = DatabaseContext.Instance.Make())
            foreach (var user in users)
            {
                // сам пользователь
                var usrUnd = LinqToEntity.UndecoratePlatformUser(user);
                ctx.PLATFORM_USER.Add(usrUnd);
                ctx.SaveChanges();
                var userId = usrUnd.ID;
                var modelTime = timeStartMax.AddMinutes(-rand.Next(minutesBackMax));

                // кошелек
                var wallet = new WALLET
                    {
                        Balance = moneyOnWallet,
                        Currency = currency,
                        Password = user.Password,
                        User = userId
                    };
                ctx.WALLET.Add(wallet);

                // счет
                var money = GetRandomDepositSize(depoPercentiles);
                var roughMoneyPercent = 0.0;
                if (rand.Next(100) < 20)
                    roughMoneyPercent = rand.NextDouble() - 0.5;
                else if (rand.Next(100) < 30)
                    roughMoneyPercent = rand.NextDouble() * 100 - 50;
                money += (int)(money * roughMoneyPercent / 100);

                var account = new ACCOUNT
                    {
                        AccountGroup = group,
                        Balance = money,
                        Currency = currency,
                        Status = (int) Account.AccountStatus.Created,
                        TimeCreated = modelTime,
                        ReadonlyPassword = user.Password
                    };
                ctx.ACCOUNT.Add(account);
                ctx.SaveChanges();
                accountIds.Add(account.ID);

                // денежный перевод на счет - начальный баланс
                ctx.BALANCE_CHANGE.Add(new BALANCE_CHANGE
                    {
                        AccountID = account.ID,
                        Amount = money,
                        ChangeType = (int) BalanceChangeType.Deposit,
                        Description = "Initial deposit",
                        ValueDate = account.TimeCreated
                    });
                // пользователь-счет
                ctx.PLATFORM_USER_ACCOUNT.Add(new PLATFORM_USER_ACCOUNT
                    {
                        Account = account.ID,
                        PlatformUser = userId,
                        RightsMask = (int) AccountRights.Управление
                    });

                // сервис торг. сигналов
                if (cbSignallers.Checked)
                {
                    ctx.SERVICE.Add(new SERVICE
                        {
                            AccountId = account.ID,
                            Currency = currency,
                            FixedPrice = signalPrice,
                            User = userId,
                            ServiceType = (int) PaidServiceType.Signals,
                            Comment = "Сигналы " + user.Login
                        });
                }
                ctx.SaveChanges();
            }
        }

        private int GetRandomDepositSize(int[] percentiles)
        {
            var rndVal = rand.Next(100);
            var percent = 0;
            for (var i = 0; i < percentiles.Length / 2; i++)
            {
                percent += percentiles[i*2];
                if (rndVal <= percent) return percentiles[i * 2 + 1];
            }
            return 0;
        }

        private List<PlatformUser> MakeUsersWithNames()
        {
            var users = new List<PlatformUser>();
            var pwrd = tbPassword.Text;

            foreach (var line in tbUserNames.Lines)
            {
                if (string.IsNullOrEmpty(line) || line.Length < 10) continue;
                var parts = line.Split(
                    new[] {' ', (char) Keys.Tab, ';', ','}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3) continue;

                var login = parts.FirstOrDefault(p => (p[0] >= 'a' && p[0] <= 'z') || (p[0] >= 'A' && p[0] <= 'Z'));
                if (string.IsNullOrEmpty(login)) continue;

                parts = parts.Except(new[] {login}).ToArray();
                var user = new PlatformUser
                    {
                        Login = login,
                        Name = parts[0],
                        Surname = parts[1],
                        Password = pwrd,
                        Title = "Trader",
                        RightsMask = UserAccountRights.Trade,
                        RegistrationDate = DateTime.Now
                    };
                MakeEmail(user);
                users.Add(user);
            }

            return users;
        }

        private static void MakeEmail(PlatformUser us)
        {
            var domain = mailDomains.GetRandomValue();
            
            // имя вида Mikhail.Krymov@mail.ru
            if (rand.Next(100) < 30)
            {
                if (rand.Next(100) < 60)
                    us.Email = Transliteration.Front(us.Name) + "." + Transliteration.Front(us.Surname) + domain;
                else // либо Krymov.Mikhail@mail.ru
                    us.Email = Transliteration.Front(us.Surname) + "." + Transliteration.Front(us.Name) + domain;
                return;
            }

            // имя вида mkrymov1987@yandex.ru
            if (rand.Next(100) < 40)
            {
                us.Email = Transliteration.Front(us.Name[0].ToString()) + Transliteration.Front(us.Surname);
                // или mkrymov87@yandex.ru
                if (rand.Next(100) < 66)
                    us.Email += (1996 - rand.Next(25));
                else
                    us.Email += (1996 - rand.Next(25)).ToString().Substring(2, 2);
                us.Email += domain;
                return;
            }

            // имя из логина XpenoB@mail.ru
            if (rand.Next(100) < 65)
            {
                us.Email = us.Login + domain;
                return;
            }

            // XpenoB1982@mail.ru или XpenoB1991@mail.ru
            us.Email = us.Login;
            if (rand.Next(100) < 66)
                us.Email += (1996 - rand.Next(25));
            else
                us.Email += (1996 - rand.Next(25)).ToString().Substring(2, 2);
            us.Email += domain;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var groups = TradeSharpDictionary.Instance.proxy.GetAccountGroupsWithSessionInfo();
            cbGroup.DataSource = groups.Select(g => g.Code).ToArray();
            var indexDemo = groups.FindIndex(g => !g.IsReal && g.Code.Equals("demo", StringComparison.OrdinalIgnoreCase));
            cbGroup.SelectedIndex = indexDemo < 0 ? 0 : indexDemo;
            tbDestinationQuoteFolder.Text = 
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\quotes";

            tbNewAccountOpenTime.Text = string.Format("{0:dd.MM.yyyy HH:mm:ss}-{1:dd.MM.yyyy HH:mm:ss}",
                DateTime.Now.Date.AddYears(-4), DateTime.Now.AddYears(-2));
            dpModelStartegiesStart.Value = DateTime.Now.AddYears(-1);
        }

        private void btnCopyIds_Click(object sender, EventArgs e)
        {
            if (accountIds == null || accountIds.Count == 0) return;
            Clipboard.SetText(string.Join(", ", accountIds));
        }
    
        private void SaveFarmSettings()
        {
            var acIds = richTextBoxAccountIds.Text.ToIntArrayUniform();
            if (acIds.Length == 0)
                acIds = accountIds.ToArray();
            if (acIds.Length == 0)
            {
                MessageBox.Show("Не указаны номера счетов");
                return;
            }

            XmlDocument robotsDoc = null;
            if (!string.IsNullOrEmpty(tbPortfolioSetsPath.Text))
            {
                try
                {
                    robotsDoc = new XmlDocument();
                    using (var sr = new StreamReader(tbPortfolioSetsPath.Text, Encoding.UTF8))
                        robotsDoc.Load(sr);
                    if (robotsDoc.DocumentElement == null)
                        robotsDoc = null;
                    else if (robotsDoc.DocumentElement.ChildNodes.Count == 0)
                        robotsDoc = null;
                }
                catch (Exception ex)
                {
                    robotsDoc = null;
                    MessageBox.Show("Ошибка загрузка документа роботов: " + ex.Message);
                }
            }

            var robotCountProb = tbRobotsCount.Text.ToIntArrayUniform();
            for (var i = 1; i < robotCountProb.Length; i++)
                robotCountProb[i] += robotCountProb[i - 1];

            var tickerProb = tbTickerProb.Lines.Select(new Func<string, Cortege2<string, int>?>(
                l =>
                    {
                        if (string.IsNullOrEmpty(l)) return null;
                        var pair =
                            l.Split(new[] {':', ' ', ';', (char) Keys.Tab},
                                    StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) return null;
                        return new Cortege2<string, int>(pair[0],
                                                        pair[1].ToIntSafe() ??
                                                        25);
                    })).Where(pro => pro != null).Cast<Cortege2<string, int>>().ToArray();

            for (var i = 1; i < tickerProb.Length; i++)
                tickerProb[i] = new Cortege2<string, int>(tickerProb[i].a, tickerProb[i - 1].b + tickerProb[i].b);

            var doc = new XmlDocument();
            var docNode = doc.AppendChild(doc.CreateElement("farm"));
            
            using (var conn = DatabaseContext.Instance.Make())
            foreach (var acId in acIds)
            {
                var accountId = acId;
                var owner = (from pa in conn.PLATFORM_USER_ACCOUNT
                             join u in conn.PLATFORM_USER on pa.PlatformUser equals u.ID
                             where pa.Account == accountId && pa.RightsMask == (int) AccountRights.Управление
                             select u).FirstOrDefault();
                if (owner == null) continue;

                var nodeAccount = (XmlElement) docNode.AppendChild(doc.CreateElement("Account"));
                var nodeAcId = (XmlElement)nodeAccount.AppendChild(doc.CreateElement("AccountId"));
                nodeAcId.Attributes.Append(doc.CreateAttribute("value")).Value = acId.ToString();

                var nodeLogin = (XmlElement)nodeAccount.AppendChild(doc.CreateElement("UserLogin"));
                nodeLogin.Attributes.Append(doc.CreateAttribute("value")).Value = owner.Login;
                
                var nodePwrd = (XmlElement)nodeAccount.AppendChild(doc.CreateElement("UserPassword"));
                nodePwrd.Attributes.Append(doc.CreateAttribute("value")).Value = owner.Password;

                var nodeTrade = (XmlElement)nodeAccount.AppendChild(doc.CreateElement("TradeEnabled"));
                nodeTrade.Attributes.Append(doc.CreateAttribute("value")).Value = "True";

                // роботы
                if (robotsDoc == null) continue;
                var rVal = rand.Next(100);
                var robotsCount = robotCountProb.FindIndex(v => rVal < v) + 1;
                
                for (var i = 0; i < robotsCount; i++)
                {
                    var robItem =
                        (XmlElement)robotsDoc.DocumentElement.ChildNodes[rand.Next(robotsDoc.DocumentElement.ChildNodes.Count)];
                    
                    // настроить тикер (ТФ не менять)
                    var nodeTicker = robItem.ChildNodes.Cast<XmlElement>().First(n => n.Name == "Robot.TimeSeries");
                    var valAttr = nodeTicker.Attributes["value"];
                    var pairTimeframe = valAttr.Value.Split(new[] { ' ', ',', ':', ';', '.', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pairTimeframe.Length == 2)
                    {
                        rVal = rand.Next(100);
                        var ticker = tickerProb.First(p => rVal < p.b).a;
                        valAttr.Value = ticker + ":" + pairTimeframe[1];
                    }

                    // добавить робота
                    var imported = doc.ImportNode(robItem, true);
                    nodeAccount.AppendChild(imported);
                }
            }

            // сохранить документ
            if (saveFarmsetsDialog.ShowDialog() != DialogResult.OK) return;
            tbFarmSetsPath.Text = saveFarmsetsDialog.FileName;

            using (var sw = new StreamWriter(saveFarmsetsDialog.FileName, false, Encoding.UTF8))
            using (var xtw = new XmlTextWriter(sw) { Indentation = 4, Formatting = Formatting.Indented })
                doc.Save(xtw);
        }

        private void btnMakeRobotFarmSettings_Click(object sender, EventArgs e)
        {
            SaveFarmSettings();
        }

        private void btnLoadPortfolio_Click(object sender, EventArgs e)
        {
            if (portfolioDialog.ShowDialog() != DialogResult.OK) return;
            tbPortfolioSetsPath.Text = portfolioDialog.FileName;
        }

        private void btnCheckLogins_Click(object sender, EventArgs e)
        {
            var logins = rtbLogins.Lines.Where(l => !string.IsNullOrEmpty(l)).ToList();
            if (logins.Count == 0) return;

            var wrongLogins = new List<string>();

            using (var ctx = DatabaseContext.Instance.Make())
                for (var i = 0; i < logins.Count; i++)
                {
                    var login = logins[i];
                    var isOk = true;
                    if (login.Length < PlatformUser.LoginLenMin || login.Length > PlatformUser.LoginLenMax)
                        isOk = false;
                    else
                        isOk = !ctx.PLATFORM_USER.Any(u => u.Login == login);
                    if (isOk) continue;
                    wrongLogins.Add(login);
                    logins.RemoveAt(i);
                    i--;
                }

            var text = new StringBuilder();
            if (logins.Count > 0)
            {
                logins.Insert(0, "Корректные имена:");
                text.Append(string.Join(Environment.NewLine, logins));
                text.AppendLine();
            }
            if (wrongLogins.Count > 0)
            {
                wrongLogins.Insert(0, "Некорректные имена:");
                text.AppendLine();
                text.Append(string.Join(Environment.NewLine, wrongLogins));
            }
            rtbLogins.Text = text.ToString();
        }

        private void btnGetAccountIdsForFarm_Click(object sender, EventArgs e)
        {
            var dlg = new GetAccountIdsForm(cbGroup.Items.Cast<string>().ToArray());
            if (dlg.ShowDialog() != DialogResult.OK) return;
            richTextBoxAccountIds.Text = Clipboard.GetText();
        }

        private void btnChangeDestinationQuoteFolder_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog
            {
                SelectedPath = ExecutablePath.ExecPath,
                Description = "Укажите каталог назначения"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                tbDestinationQuoteFolder.Text = dlg.SelectedPath;
        }

        private void ButtonLoadClick(object sender, EventArgs e)
        {
            if (workerQuote.IsBusy)
            {
                workerQuote.CancelAsync();
                return;
            }

            var taskType = sender == btnLoadFromTradeSharpServer
                ? BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromTradeSharp
                : BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromForexite;

            var task = taskType == BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromForexite
                ? new BackgroundWorkerTask(BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromForexite,
                    new Cortege2<string, int>(tbDestinationQuoteFolder.Text,
                        tbQuoteLoadStartYear.Text.ToIntSafe() ?? 2009))
                : new BackgroundWorkerTask(BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromTradeSharp, 
                    tbDestinationQuoteFolder.Text);
            workerQuote.RunWorkerAsync(task);

            btnLoadQuotes.Text = "Остановить";
        }

        private void WorkerQuoteOnRunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            btnLoadQuotes.Text = "Загрузить";
            lblWorkerProgress.Text = "Завершено";
        }

        private void WorkerQuoteOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var taskParam = (BackgroundWorkerTask) doWorkEventArgs.Argument;

            // закачать котировки с Forexite
            if (taskParam.Task == BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromForexite)
            {
                DoLoadQuotesFromForexite(taskParam);
                return;
            }

            // обновить котировки из БД TradeSharp
            if (taskParam.Task == BackgroundWorkerTask.BackgroundWorkerTaskType.LoadFromTradeSharp)
            {
                DoLoadQuotesFromTradeSharp(taskParam);
                return;
            }

            // поправить балансовые операции
            if (taskParam.Task == BackgroundWorkerTask.BackgroundWorkerTaskType.FixTransactions)
            {
                DoFixtransactions(taskParam);
                return;
            }

            // сформировать JSON-трек доходности
            if (taskParam.Task == BackgroundWorkerTask.BackgroundWorkerTaskType.MakeJSON)
            {
                DoMakeJSON(taskParam);
                return;
            }

            // сформировать JSON-трек доходности
            if (taskParam.Task == BackgroundWorkerTask.BackgroundWorkerTaskType.MakeTradeHistoryInDB)
            {
                DoMakeTradeHistoryInDB(taskParam);
                return;
            }
        }

        private void DoMakeTradeHistoryInDB(BackgroundWorkerTask taskParam)
        {
            var taskPtr = (HistoryMakerTaskParam) taskParam.DataParam;
            var totalCount = taskPtr.portfolioDoc.DocumentElement.ChildNodes.Count;
            var currentNode = 0;
            foreach (XmlElement node in taskPtr.portfolioDoc.DocumentElement.ChildNodes)
            {
                if (workerQuote.CancellationPending)
                    break;
                Logger.InfoFormat("Старт формирования истории по счету - {0} из {1}", currentNode + 1, totalCount);
                taskPtr.historyMaker.MakeHistory(node);
                currentNode ++;
                workerQuote.ReportProgress(100 * currentNode / totalCount);
            }
            workerQuote.ReportProgress(100);

            if (taskPtr.historyMaker.testOnly)
            {
                using (var sw = new StreamWriter(ExecutablePath.ExecPath + "\\performance_test.txt", false, Encoding.UTF8))
                {
                    sw.WriteLine(AccountPerformance.MakeTableHeader());
                    taskPtr.historyMaker.performance.ForEach(sw.WriteLine);
                }                
            }
        }

        private void DoLoadQuotesFromForexite(BackgroundWorkerTask taskParam)
        {
            var folderYear = (Cortege2<string, int>) taskParam.DataParam;
            var endYear = DateTime.Now.Year;
            for (var year = folderYear.b; year < endYear; year++)
            {
                if (workerQuote.CancellationPending) break;
                QuoteDownloader.DownloadForYear(year, folderYear.a);
                workerQuote.ReportProgress(year);
            }
        }

        private void DoLoadQuotesFromTradeSharp(BackgroundWorkerTask taskParam)
        {
            var folder = (string) taskParam.DataParam;
            var totalCount = Directory.GetFiles(folder, "*.quote").Count();
            var count = 0;
            workerQuote.ReportProgress(0);
            foreach (var fileName in Directory.GetFiles(folder, "*.quote"))
            {
                var lastDate = GetFileLastDate(fileName);
                if (!lastDate.HasValue) continue;

                var existFileName = fileName + ".temp";
                File.Move(fileName, existFileName);

                var ticker = Path.GetFileNameWithoutExtension(fileName).ToUpper();
                FileGapActualizator.FillGapsByTicker(ticker, lastDate.Value,
                    new List<GapInfo>
                    {
                        new GapInfo
                        {
                            start = lastDate.Value.AddMinutes(1),
                            end = DateTime.Now,
                            status = GapInfo.GapStatus.Gap
                        }
                    }, folder, workerQuote,
                    (s, list) => { });

                QuoteDownloader.Merge2Files(existFileName, fileName);

                workerQuote.ReportProgress(100*(count++)/totalCount);
            }
        }

        private void DoMakeJSON(BackgroundWorkerTask taskParam)
        {
            var data = (Cortege3<List<int>, string, int>) taskParam.DataParam;
            var accountList = data.a;
            var targetFilePath = data.b;
            var intervalMinutes = data.c;

            int accountNumber = 0;
            foreach (var id in accountList)
            {
                if (workerQuote.CancellationPending) break;
                JsonTrackMaker.BuildTrack(
                    id, 
                    targetFilePath + "_" + id + ".txt", 
                    intervalMinutes);
                workerQuote.ReportProgress(100*(accountNumber++)/accountList.Count);
            }
        }

        private void DoFixtransactions(BackgroundWorkerTask taskParam)
        {
            var accountList = (List<int>) taskParam.DataParam;
            int accountNumber = 0;
            foreach (var id in accountList)
            {
                if (workerQuote.CancellationPending) break;
                var accountId = id;
                using (var db = new TradeSharpConnection())
                {
                    foreach (var order in db.POSITION_CLOSED.Where(p => p.AccountID == accountId))
                    {
                        if (workerQuote.CancellationPending) break;
                        var orderId = order.ID;
                        if (db.BALANCE_CHANGE.Any(bc => bc.Position == orderId)) continue;

                        var changeType = (int) (order.ResultDepo > 0 ? BalanceChangeType.Profit : BalanceChangeType.Loss);
                        var closeTime = order.TimeExit;
                        var orderResult = Math.Abs(order.ResultDepo);
                        var missIdBc = db.BALANCE_CHANGE.FirstOrDefault(b => b.AccountID == accountId &&
                                                                             b.ValueDate == closeTime &&
                                                                             b.Amount == orderResult);
                        if (missIdBc != null)
                        {
                            missIdBc.Position = orderId;
                            missIdBc.ChangeType = changeType;
                        }
                        else
                            db.BALANCE_CHANGE.Add(new BALANCE_CHANGE
                            {
                                AccountID = order.AccountID,
                                Position = order.ID,
                                Amount = order.ResultDepo,
                                ChangeType = changeType,
                                ValueDate = order.TimeExit
                            });
                    }
                    db.SaveChanges();
                }
                workerQuote.ReportProgress(100*(accountNumber++)/accountList.Count);
            }
        }

        private DateTime? GetFileLastDate(string fileName)
        {
            DateTime? startDate, endDate;
            bool endsNewLine;
            QuoteCacheManager.GetFirstAndLastFileDates(fileName, out startDate, out endDate, out endsNewLine);
            return endDate;
        }

        private void btnPickFarmSetsFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbFarmSetsPath.Text))
                openFarmSetsDialog.FileName = tbFarmSetsPath.Text;
            if (openFarmSetsDialog.ShowDialog() == DialogResult.OK)
                tbFarmSetsPath.Text = openFarmSetsDialog.FileName;
        }

        private void btnMakeHistoryOperations_Click(object sender, EventArgs e)
        {
            MakeOrTestStrategies(true);
        }

        private void MakeOrTestStrategies(bool makeNotTest)
        {
            if (workerQuote.IsBusy)
            {
                workerQuote.CancelAsync();
                return;
            }

            if (!File.Exists(tbFarmSetsPath.Text)) return;

            var doc = new XmlDocument();
            using (var sr = new StreamReader(tbFarmSetsPath.Text, Encoding.UTF8))
                doc.Load(sr);
            if (doc.DocumentElement == null)
                return;

            if (string.IsNullOrEmpty(richTextBoxAccountIds.Text)) return;
            var accIds = richTextBoxAccountIds.Text.ToIntArrayUniform();
            if (accIds.Length == 0) return;

            var quoteFolder = tbDestinationQuoteFolder.Text;
            if (string.IsNullOrEmpty(quoteFolder)) return;
            if (!Directory.Exists(quoteFolder)) return;

            var probWithdrawOnLossProfit = tbWdthProbOnLoss.Text.ToIntArrayUniform();
            var histMaker = new TradeHistoryMaker(tbSkipLossProb.Text.ToInt(), tbWithdrawProb.Text.ToInt(),
                probWithdrawOnLossProfit[0], probWithdrawOnLossProfit[1],
                !makeNotTest, tbStartTestDepo.Text.ToInt(),
                dpModelStartegiesStart.Value, dpModelStrategiesEnd.Value);

            var histPtr = new HistoryMakerTaskParam
            {
                historyMaker = histMaker,
                portfolioDoc = doc
            };
            var task = new BackgroundWorkerTask(BackgroundWorkerTask.BackgroundWorkerTaskType.MakeTradeHistoryInDB,
                    histPtr);
            workerQuote.RunWorkerAsync(task);       
        }

        private void btnFixBalanceChanges_Click(object sender, EventArgs e)
        {
            if (workerQuote.IsBusy)
            {
                workerQuote.CancelAsync();
                return;
            }

            if (!File.Exists(tbFarmSetsPath.Text)) return;

            List<int> actIds = GetAccountIdsFromFarmSetsFile();
            if (actIds == null || actIds.Count == 0)
                return;

            var task = new BackgroundWorkerTask(BackgroundWorkerTask.BackgroundWorkerTaskType.FixTransactions, actIds);
            workerQuote.RunWorkerAsync(task);
        }

        private List<int> GetAccountIdsFromFarmSetsFile()
        {
            List<int> actIds = null;
            var doc = new XmlDocument();
            using (var sr = new StreamReader(tbFarmSetsPath.Text, Encoding.UTF8))
                doc.Load(sr);
            if (doc.DocumentElement == null)
                return actIds;

            actIds = doc.SelectNodes("/farm/Account/AccountId").Cast<XmlElement>().Select(n =>
                n.Attributes["value"].Value.ToInt()).ToList();
            return actIds;
        }

        private void btnMakeJson_Click(object sender, EventArgs e)
        {
            if (workerQuote.IsBusy)
            {
                workerQuote.CancelAsync();
                return;
            }

            var actIds = GetAccountIdsFromFarmSetsFile();

            if (actIds.Count == 0)
                return;

            lblWorkerProgress.Text = "Обработка счетов (JSON)";

            var task = new BackgroundWorkerTask(BackgroundWorkerTask.BackgroundWorkerTaskType.MakeJSON, 
                new Cortege3<List<int>, string, int>(actIds, tbFarmSetsPath.Text + "_json_",
                    tbTrackIntervalMinutes.Text.ToInt()));
            workerQuote.RunWorkerAsync(task);
        }

        private void btnMakeDayQuotes_Click(object sender, EventArgs e)
        {
            var outDir = tbDestinationQuoteFolder.Text.Trim('\\') + "\\daily";
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            foreach (var file in Directory.GetFiles(tbDestinationQuoteFolder.Text, "*.quote"))
            {
                var fileName = Path.GetFileName(file);
                var ticker = Path.GetFileNameWithoutExtension(file).ToUpper();
                var precision = DalSpot.Instance.GetPrecision10(ticker);

                using (var sw = new StreamWriter(outDir + "\\" + fileName, false, Encoding.UTF8))
                using (var sr = new StreamReader(file, Encoding.UTF8))
                {
                    DateTime? time = null, lastDate = null;
                    CandleData prevCandle = null;

                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        prevCandle = CandleData.ParseLine(line, ref time, precision, ref prevCandle);
                        if (prevCandle == null)
                            continue;
                        if (lastDate.HasValue && prevCandle.timeClose.Date > lastDate.Value)
                            sw.WriteLine("{0:ddMMyyyy} {1}",
                                prevCandle.timeClose.Date, prevCandle.close.ToStringUniformPriceFormat(true));
                        
                        lastDate = prevCandle.timeClose.Date;
                    }
                }
            }
        }

        private void btnTestStrategies_Click(object sender, EventArgs e)
        {
            MakeOrTestStrategies(false);
        }

        private void btnCorrectEquity_Click(object sender, EventArgs e)
        {
            CorrectBalanceOrEquity(true);
        }

        private void btnCorrectBalance_Click(object sender, EventArgs e)
        {
            CorrectBalanceOrEquity(false);
        }

        private void CorrectBalanceOrEquity(bool correctEquity)
        {
            var amounts = tbTargetCorrectedAmount.Text.ToIntArrayUniform();
            if (amounts.Length != 3) return;
            var minAmount = amounts[0];
            var maxAmount = amounts[1];
            var maxDelta = amounts[2];

            if (string.IsNullOrEmpty(tbFarmSetsPath.Text)) return;
            if (!File.Exists(tbFarmSetsPath.Text)) return;

            List<int> actIds = GetAccountIdsFromFarmSetsFile();
            if (actIds == null || actIds.Count == 0)
                return;

            var messages = BalanceCorrector.CorrectBalance(actIds, minAmount, maxAmount, maxDelta);
            MessageBox.Show(string.Join(",  ", messages));
        }

        private void btnClearHistoryByAccounts_Click(object sender, EventArgs e)
        {
            // удалить всю историю по выбранным счетам
            var accounts = richTextBoxAccountIds.Text.ToIntArrayUniform();
            if (accounts.Length == 0)
                return;
            var delCount = TradeHistoryCleaner.ClearAllRecordsByAccounts(accounts);
            MessageBox.Show("Удалено " + delCount + " записей");
        }

        private void btnAmendHistory_Click(object sender, EventArgs e)
        {
            var flipPercent = tbFlipOrdersPercent.Text.ToInt();
            if (string.IsNullOrEmpty(tbFarmSetsPath.Text)) return;
            if (!File.Exists(tbFarmSetsPath.Text)) return;

            List<int> actIds = GetAccountIdsFromFarmSetsFile();
            if (actIds == null || actIds.Count == 0)
                return;

            var amender = new TradeHistoryAmender();
            var messages = amender.AmendHistory(actIds, flipPercent, false);
            MessageBox.Show(string.Join("\n", messages));
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            var dlg = new NewAccountForm(cbGroup.Items.Cast<string>().ToArray());
            dlg.ShowDialog();
        }    
    }
}
