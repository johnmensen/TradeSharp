using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Candlechart;
using Candlechart.Controls;
using Candlechart.Indicator;
using Candlechart.Series;
using Entity;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Client.BL;
using TradeSharp.Client.BL.Script;
using TradeSharp.Client.BL.Sound;
using TradeSharp.Client.Controls.Bookmark;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.Robot;
using TradeSharp.UI.Util.Forms;
using TradeSharp.Util;
using System.Linq;
using TradeSharp.Util.NotificationControl;

namespace TradeSharp.Client
{
    // ReSharper disable ParameterTypeCanBeEnumerable.Local
    // контроль версий:
    // SubWCRev.exe $(SolutionDir) $(ProjectDir)SubversionInfoTemplate.txt $(ProjectDir)Resources\SubversionInfo.txt
    // ReSharper disable LocalizableElement
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public partial class MainForm : Form, ITradeSharpServerCallback, IApplicationMessageTarget
    {
        public event Action<Keys> KeyPressedEvent;

        #region Константы
        public const string FileNameIndiSettings = "indicator.xml";
        public const string FileNameObjectSettings = "objects.xml";
        #endregion

        #region Переменные и свойства

        private volatile bool workspaceIsLoadedOk;

        private string terminalVersion;

        private string terminalTitle = "TradeSharp";
        /// <summary>
        /// время запуска терминала
        /// </summary>
        private readonly DateTime timeStarted = DateTime.Now;
        /// <summary>
        /// завершена загрузка рабочего пространства
        /// </summary>
        private volatile bool terminalIsLoaded;
        public static TradeSharpServerTrade serverProxyTrade;
        private readonly TcpQuoteReceiver quoteReceiver;
        
        private readonly RobotFarm robotFarm;        

        public RobotFarm RobotFarm
        {
            get { return robotFarm; }
        }

        public ChatControlBackEnd ChatEngine { get; private set; }

        /// <summary>
        /// мин. интервал гэпа для закачки котировок с сервера
        /// </summary>
        private readonly int minMinutesToUpdateQuotes = AppConfig.GetIntParam("MinMinutesToUpdateQuotes", 5);
        private readonly string autosaveFolder;
        private readonly Thread threadSchedule;
        /// <summary>
        /// частота опроса потока-планировщика заданий
        /// </summary>
        private readonly int scheduleThreadTimeout = AppConfig.GetIntParam("Schedule.TimeoutMils", 100);
        /// <summary>
        /// работа терминала завершается
        /// </summary>
        private volatile bool isExiting;

        /// <summary>
        /// время получения последней котировки
        /// </summary>
        private readonly ThreadSafeTimeStamp lastQuoteReceived = new ThreadSafeTimeStamp();

        private const int MaxMillsBetweenQuotes = 1000 * 3;

        private static readonly Color colorStatusHasQuote = Color.DarkBlue;
        
        private static readonly Color colorStatusNoQuote = Color.DarkRed;

        private OrderDlg dlgOrder;
        
        private List<ChartForm> Charts
        {
            get
            {
                return MdiChildren.Where(child => child is ChartForm).Cast<ChartForm>().ToList();
            }
        }

        private ChartForm lastActiveChart;

        private readonly PrintDocument printDocument = new PrintDocument { DefaultPageSettings = {Landscape = true}};

        private readonly ToolTip buttonToolTip = new ToolTip();
        /// <summary>
        /// список меню, выпадающих по кнопкам на панели инструментов
        /// </summary>
        private readonly List<ContextMenuStrip> buttonMenus = new List<ContextMenuStrip>();

        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 10);

        private const int LogMsgPingError = 1;

        private DateTime lastTimePinged = DateTime.Now;

        private const int PingIntervalSeconds = 20;

        /// <summary>
        /// в отдельных случаях (например, рестарт терминала) сообщение о закрытии явно излишне
        /// </summary>
        private bool skipFormClosingPrompt;
        #endregion

        #region Переменные и константы - новости
        private static NewsStorageProxy newsProxy;
        private const int MaxDaysInNewsRequest = 250;
        #endregion

        #region Переменные и свойства - автосохранение
        private readonly bool autosavingEnabled = AppConfig.GetBooleanParam("Schedule.AutosaveEnabled", true);
        private readonly int autosaveIntervalMils = AppConfig.GetIntParam("Schedule.AutosaveIntervalMils", 1000 * 20);
        private readonly int maxAutosaveSessions = AppConfig.GetIntParam("Schedule.MaxAutosaveSessions", 25);
        private readonly int maxAutosavesInSession = AppConfig.GetIntParam("Schedule.MaxAutosavesInSession", 15);
        private ThreadSafeTimeStamp timeLastAutosave;
        private static readonly AutoResetEvent loadChartsEvent = new AutoResetEvent(true);
        private readonly int timeoutLockSaveMs = AppConfig.GetIntParam("Timeout.LockSave",  1000 * 15 * 60);
        #endregion

        public static MainForm Instance { get; private set; }

        private static void CheckAppFolders()
        {
            var saveFolder = AppConfig.GetStringParam("AutosaveFolder", "\\autosave");
            if (saveFolder.StartsWith("\\")) saveFolder = ExecutablePath.ExecPath + saveFolder;
            if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
            if (!Directory.Exists(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder)) 
                Directory.CreateDirectory(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder);
            if (!Directory.Exists(ExecutablePath.ExecPath + TerminalEnvironment.NewsCacheFolder)) 
                Directory.CreateDirectory(ExecutablePath.ExecPath + TerminalEnvironment.NewsCacheFolder);
            if (!Directory.Exists(ExecutablePath.ExecPath + TerminalEnvironment.RobotCacheFolder)) 
                Directory.CreateDirectory(ExecutablePath.ExecPath + TerminalEnvironment.RobotCacheFolder);
        }

        public MainForm(bool debugData)
        {
            
        }

        public MainForm()
        {
            Instance = this;
            if (AppConfig.GetBooleanParam("SingleInstance", true))
            {
                var prc = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

                if (prc.Length > 1)
                {
                    MessageBox.Show(Localizer.GetString("MessageTerminalAlreadyRunning"), Localizer.GetString("TitleWarning"));
                    Process.GetCurrentProcess().Kill();
                    return;
                }
            }

            // показать окно загрузки
            SplashScreen.ShowSplash(StartupStage.Started);
            
            InitializeComponent();

            Localizer.LocalizeControl(this);

            bookmarkStrip.ImgList = lstGlyph32;
            SetupMenuImages();
            try
            {
                MakeFormMdi();
                Candlechart.Core.PluginManager.Instance.Initialize();
                RobotCollection.Initialize();
                ScriptManager.Initialize();
                ScriptManager.Instance.scriptListIsUpdated += ActualizeScriptMenu;
                CheckAppFolders();
                Logger.Info("Старт терминала (" + ExecutablePath.ExecPath + ")");
                
                InitializeTooltips();
                InitializeToolButtons(ChartToolButtonStorage.Instance.selButtons, panelChartTools);
                InitializeToolButtons(ChartToolButtonStorage.Instance.selSystemButtons, panelCommonTools);
                InitializeSeriesSettings();
                BtnFlipPanelChartToolsClick(btnFlipPanelCommonTools, null);                
                BookmarkStorage.Instance.BookmarkDeleted += InstanceOnBookmarkDeleted;
                
                // шаг 1 завершен - обновить статус
                SplashScreen.UpdateState(StartupStage.MakeInstrumentalPanels);

                UserSettings.Instance.LoadProperties();
                EventSoundPlayer.Instance.LoadSounds();
                robotFarm = new RobotFarm(UpdateTickersCacheForRobots);
                robotFarm.OnStateChanged += RobotFarmStateChanged;
                quoteReceiver = new TcpQuoteReceiver();
                quoteReceiver.OnQuotesReceived += OnQuotesReceived;
                quoteReceiver.OnNewsReceived += OnNewsReceived;
                NewsCache.Instance.ActualizationCompleted +=
                    newsCount =>
                    AddMessageToStatusPanelSafe(DateTime.Now,
                                                string.Format("{0} ({1})", Localizer.GetString("MessageNewsRead"), newsCount));
                Contract.Util.BL.QuoteStorage.Instance.LoadQuotes(string.Format("{0}\\lastquote.txt", ExecutablePath.ExecPath));
                DalSpot.Instance.SetFavoritesList(UserSettings.Instance.FavoriteTickers.ToArray());
                navPanelControl.SetupPages(UserSettings.Instance.NavPanelPages);
                navPanelControl.closePanelClicked += () => MenuitemNavPaneClick(this, EventArgs.Empty);
                //AccountStatus.Instance.ConnectionStatusIsUpdated += InstanceOnConnectionStatusIsUpdated;
                AccountStatus.Instance.ServerConnectionStatus = 
                    DalSpot.Instance.ConnectionWasEstablished ? ServerConnectionStatus.Connected : ServerConnectionStatus.NotConnected;
                
                // шаг 2 завершен - обновить статус
                SplashScreen.UpdateState(StartupStage.MakeQuoteStream);
                ResumeConnectionWithServer();

                // запуск чата
                ChatEngine = new ChatControlBackEnd();
                ChatEngine.Start();
                SetupChat();                

                // шаг 3 завершен - обновить статус
                toolTipStatus.SetToolTip(lblConnectStatus, "");
                toolTipStatus.SetToolTip(connectionStatusImage, "");
                SplashScreen.UpdateState(StartupStage.MakeServerConnection);

                AccountStatus.Instance.OnAccountInfoUpdated += OnAccountInfoUpdated;
                AccountStatus.Instance.OnConnectionAborted += ResumeConnectionWithServer;
                AccountStatus.Instance.OnAuthenticationFailed += AuthenticationFailed;
                // вывести список открытых/закрытых за последнее время ордеров                
                AccountStatus.Instance.OnAccountDataFirstReceived += ShowLast24HrOrders;
                autosaveFolder = AppConfig.GetStringParam("AutosaveFolder", "\\autosave");
                autosaveFolder = autosaveFolder.TrimEnd('\\');
                if (autosaveFolder.StartsWith("\\")) autosaveFolder = ExecutablePath.ExecPath + autosaveFolder;

                CrossChartDivergenciesSettingsWindow.GetChartsList += GetChartsList;
                printDocument.PrintPage += PrintDocumentPrintPage;
                serverProxyTrade.OnConnectionReestablished += ReviveFactory;
                SetupDragTargets();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharp - ошибка инициализации ", ex);
                Close();
                return;
            }
            
            // шаг 4 завершен - обновить статус
            SplashScreen.UpdateState(StartupStage.FillDictionaries);

            // запуск процесса-планировщика
            threadSchedule = new Thread(ScheduleThreadRoutine);
            threadSchedule.Start();            

            AppMessageFilter.ApplyDiffString(UserSettings.Instance.HotKeyList);
        }

        private void SetupMenuImages()
        {
            // Файл
            menuMain.ImageList = imgListMenu;

            menuitemAccount.Image = imgListMenu.Images[4];
            menuExit.Image = imgListMenu.Images[7];
            menuSaveWorkspace.Image = imgListMenu.Images[11];
            menuSave.Image = imgListMenu.Images[11];
            menuLoadWorkspace.Image = imgListMenu.Images[5];
            menuNewChart.Image = imgListMenu.Images[1];
            menuitemPrint.Image = imgListMenu.Images[23];
            menuitemWallet.Image = imgListMenu.Images[27];

            // Вид
            menuitemViewAccount.Image = imgListMenu.Images[16];
            menuitemLiveQuotes.Image = imgListMenu.Images[3];
            menuitemNavPane.Image = imgListMenu.Images[24];

            // Торговля
            menuItemNewOrder.Image = imgListMenu.Images[6];
            menuItemAccountStat.Image = imgListMenu.Images[8];
            menuitemTradeSignals.Image = imgListMenu.Images[15];
            menuItemRobotsSetup.Image = imgListMenu.Images[9];
            menuitemRobotTester.Image = imgListMenu.Images[10];
            menuitemRobotPortfolio.Image = imgListMenu.Images[21];
            menuitemRobotState.Image = imgListMenu.Images[22];
            menuitemScripts.Image = imgListMenu.Images[12];

            // Настройки
            menuitemToolPanel.Image = imgListMenu.Images[13];
            menuItemQuoteArchive.Image = imgListMenu.Images[0];
            menuItemSettingsChart.Image = imgListMenu.Images[2];
            menuitemCommonParams.Image = imgListMenu.Images[14];
            menuTemplates.Image = imgListMenu.Images[25];
            menuitemRiskSetup.Image = imgListMenu.Images[26];

            // Окна
            menuItemCascade.Image = imgListMenu.Images[18];
            menuItemVertical.Image = imgListMenu.Images[19];
            menuItemHorizontal.Image = imgListMenu.Images[20];
        }
        
        /// <summary>
        /// обновить заголовок окна (в потокобезопасном режиме)
        /// </summary>
        private void OnAccountInfoUpdated(Account account)
        {
            MainWindowTitle.Instance.Account = account;

            // обновить контекст для роботов
            RobotFarm.UpdateRobotContext(account);
        }

        private void GetChartsList(out List<CandleChartControl> chartList)
        {
            chartList = Charts.Select(c => c.chart).ToList();
        }

        private void GetChartsListByIds(List<string> chartIds, 
            out List<CandleChartControl> chartList)
        {
            chartList = (from chart in Charts
                         let chartId = chart.chart.UniqueId
                         where chartIds.Contains(chartId)
                         select chart.chart).ToList();
        }

        private void ScheduleThreadRoutine()
        {
            try
            {
                while (!isExiting)
                {
                    // автосохраниение?
                    if (autosavingEnabled) CheckAutosave();
                    // авто-апдейт
                    CheckUpdates();

                    Thread.Sleep(scheduleThreadTimeout);
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        // todo: simplify to list.firstOr Default, remove
        private ButtonBase GetPressedCommonButton(SystemToolButton btnType)
        {
            var coll = panelCommonTools.Controls;
            foreach (Control c in coll)
            {
                var btn = c as ButtonBase;
                if (btn == null)
                    continue;
                if (btn.Tag == null)
                    continue;
                var sets = btn.Tag as ChartToolButtonSettings;
                if (sets == null)
                    continue;
                if (btnType == sets.SystemTool)
                    return btn;
            }
            return null;
        }

        private void AuthenticationFailed()
        {
            SetStatusLabel(AccountStatus.Instance.connectionStatus);
            AccountStatus.Instance.isAuthorized = false;
        }

        private void ResumeConnectionWithServer()
        {
            try
            {
                serverProxyTrade = new TradeSharpServerTrade(this);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка подключения к серверу МТС.Live (Trade)", ex);
            }
            SubscriptionModel.Instance.ServerProxy = serverProxyTrade.proxy;

            try
            {
                newsProxy = new NewsStorageProxy("INewsStorageBinding");
            }
            catch (Exception)
            {
                Logger.Error("Связь с сервером (INewsStorageBinding) не установлена");
            }
        }

        /// <summary>
        /// получена котировка - одна или несколько
        /// обновить графики
        /// </summary>        
        private void OnQuotesReceived(string[] names, QuoteData[] quotes)
        {
            // определить, сколько времени прошло с момента получения последней котировки
            // предложить заполнить дырки
            var lastQuoteTime = lastQuoteReceived.GetLastHitIfHitted() ?? timeStarted;
            var deltaMinutes = (DateTime.Now - lastQuoteTime).TotalMinutes;
            if (deltaMinutes > MinutesOfGapInQuoteStream)
                ReportOnGapFound(lastQuoteTime);
            // обновить время последней котировки
            lastQuoteReceived.Touch();
            robotFarm.OnQuotesReceived(names, quotes);
            try
            {
                for (var i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    var charts = Charts;
                    for (var j = 0; j < charts.Count; j++)
                    {
                        var child = charts[j];
                        if (child.chart.Symbol != name) continue;
                        // обновить асинхронно                        
                        child.UpdateQuoteAsynch(new[] {quotes[i]});
                    }
                }
            } 
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {// окно может быть еще не сформировано, когда в него попадет котировка                
            }
            
            // обработать котировки в скриптах
            CheckScriptTriggerQuote(names, quotes);
        }

        private void OnNewsReceived(News[] news)
        {
            try
            {
                var charts = Charts;
                for (var j = 0; j < charts.Count; j++)
                    charts[j].UpdateNewsAsynch(news);
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {// окно может быть еще не сформировано, когда в него попадет новость                
            }
        }
        
        private void OnOrdersReceived(MarketOrder[] posList)
        {
            try
            {
                var charts = Charts;
                for (var j = 0; j < charts.Count; j++)
                    charts[j].UpdateOrdersListAsynch(posList.Where(o => o.Symbol == charts[j].chart.Symbol).ToArray());
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {// окно может быть еще не сформировано, когда в него попадет список ордеров
            }
        }

        private void OnPendingOrdersReceived(PendingOrder[] ordList)
        {
            try
            {
                var charts = Charts;
                for (var j = 0; j < charts.Count; j++)
                    charts[j].UpdatePendingOrdersListAsynch(ordList.Where(o => o.Symbol == charts[j].chart.Symbol).ToArray());
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {// окно может быть еще не сформировано, когда в него попадет список ордеров
            }
        } 

        private void MakeFormMdi()
        {
            IsMdiContainer = true;            
            foreach (Control control in Controls)
            {
                if (control is MdiClient)
                {
                    var client = (MdiClient)control;
                    client.Dock = DockStyle.Fill;
                    return;
                }
            }
        }

        #region Обработчики меню

        private void MenuExitClick(object sender, EventArgs e)
        {
            Close();
        }
        
        public int GetCountTabChildren(TabPage page)
        {
            return Charts.Count(child => child.bookmarkId == (long) page.Tag);
        }

        private void SetupChartForm(ChartForm child)
        {
            child.MdiParent = this;
            child.bookmarkId = bookmarkStrip.SelectedBookmark.Id;
            child.chart.receiveMarketOrders += () => MarketOrdersStorage.Instance.MarketOrders;
            child.chart.receivePendingOrders += () => MarketOrdersStorage.Instance.PendingOrders;
            child.chart.enforceClosedOrdersUpdate += HistoryOrderStorage.Instance.HurryUpUpdate;
            child.chart.getOuterCharts += GetChartsListByIds;
            child.chart.EnableExtendedVisualStyles = UserSettings.Instance.EnableExtendedThemes;
            child.chart.OnScaleChanged += ChartScaleChanged;
            child.chart.CursorCrossIsUpdated += ChartCrossChanged;
            child.chart.ChartToolChanged += OnChartToolChanged;
            child.chart.PublishTradeSignal += OnPublishTradeSignal;
            child.chart.MakeNewTextSignal += OnMakeNewSignalMessage;
            child.chart.ScriptMenuItemActivated += ChartScriptMenuItemActivated;
            child.chart.getRobotsByTicker += ticker =>
                {
                    var allRobots = RobotFarm.GetRobotCopies();
                    return allRobots.Where(r => r.Graphics.Any(g => g.a == ticker)).Select(r => r.GetUniqueName()).ToList();
                };
            child.chart.onRobotSelected += robotUniqueName =>
                {
                    var state = robotFarm.State;
                    if (state == RobotFarm.RobotFarmState.Started)
                        ShowRobotStateDialog(robotUniqueName);
                    else if (state == RobotFarm.RobotFarmState.Stopped)
                        ShowRobotPortfolioDialog(robotUniqueName);
                };
                
            AddScriptsToChart(child.chart);

            child.chart.getAccountId += () => 
                {
                    if (AccountStatus.Instance.isAuthorized)
                        return AccountStatus.Instance.accountID;
                    OpenLoginDialog();
                    if (AccountStatus.Instance.isAuthorized)
                        return AccountStatus.Instance.accountID;
                    return null;
                };
            child.chart.makeTrade += MakeTrade;
            child.chart.NewOrder += PlaceNewOrder;
            child.chart.ShowQuoteArchive += delegate(string symbol)
                                                {
                                                    new QuoteBaseForm(symbol).ShowDialog();
                                                };
            child.chart.SyncQuoteHistory += delegate(List<string> tickers, DateTime timeStart, bool showIntervalDlg)
                                                {
                                                    UpdateTickersCacheForRobotsChooseIntervals(
                                                        tickers.ToDictionary(t => t,
                                                                             t => timeStart), showIntervalDlg, true);
                                                };
            child.chart.EditMarketOrders += orderIds => new MultyOrdersEditForm(orderIds).ShowDialog();
            child.chart.getDefaultFastDealVolumes += () => UserSettings.Instance.FastDealVolumes;
            child.chart.getFavoriteIndicators += () => UserSettings.Instance.FavoriteIndicators;
            child.chart.getAllNewsByChannel += channel => NewsLocalStorage.Instance.GetNews(channel);
            child.chart.FavoriteIndicatorsAreUpdated += indis =>
                                                            {
                                                                UserSettings.Instance.FavoriteIndicators = indis.Distinct().ToList();
                                                                UserSettings.Instance.SaveSettings();
                                                            };
            child.chart.UpdateFastVolumes += volumes =>
                                                        {
                                                            UserSettings.Instance.FastDealVolumes = volumes;
                                                            UserSettings.Instance.SaveSettings();
                                                        };
            child.chart.ProfitByTickerRequested += s =>
                                                   new ProfitByTickerForm(s).ShowDialog();
            child.chart.OpenScriptSettingsDialog += () => 
                new ChartToolsSetupForm(lstGlyph32, ChartToolsSetupForm.ChartToolsFormTab.Scripts).ShowDialog();
            child.chart.VisualSettingsSetupCalled += ShowVisualSettingsDialog;
            child.chart.OnShowWindowEditMarketOrder += order => new OrderDlg(OrderDlg.OrderDialogMode.OrderEditMarket, order.ID).ShowDialog();
            child.chart.OnShowWindowInfoOnClosedOrder += order => new PositionForm(order).ShowDialog();
            child.chart.OnEditMarketOrderRequest += order => SendEditMarketRequestSafe(order);
            child.chart.PendingOrdersUpdated += OnPendingOrdersUpdatedChanged;
            child.chart.MarketOrdersUpdated += OnMarketOrdersChanged;
            child.chart.CloseMarketOrders += delegate(List<int> orderList)
                {
                    if (orderList.Count == 0) return;
                    if (orderList.Count == 1)
                    {
                        var accountId = AccountStatus.Instance.accountID;
                        if (accountId == 0) return;
                        if (MessageBox.Show(Localizer.GetString("MessageCloseDealQuestion"), Localizer.GetString("TitleConfirmation"),
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            return;
                        SendCloseRequestSafe(accountId, orderList[0], PositionExitReason.ClosedFromUI);
                        return;
                    }

                    if (MessageBox.Show(
                        string.Format(Localizer.GetString("MessageCloseNDealsQuestion"), orderList.Count),
                        Localizer.GetString("TitleConfirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                        DialogResult.No)
                        return;
                    var dlg = new ClosePositionsForm(orderList);
                    dlg.ShowDialog();
                };
            child.formMoved += OnChildMove;
            child.resizeEnded += OnChildResizeEnd;
            // иконки-кнопки
            var iconSet = UserSettings.Instance.SelectedChartIcons;
            child.chart.ChooseChartIconsToShow(iconSet);
            
            HistoryOrderStorage.Instance.OrdersUpdated += child.UpdateClosedOrdersListAsynch;
        }

        private void SetupNonMdiForm(IMdiNonChartWindow child)
        {
            child.FormMoved += OnChildMove;
            child.ResizeEnded += OnChildResizeEnd;
            ((Form) child).MdiParent = this;
        }

        private void MenuNewChartClick(object sender, EventArgs e)
        {
            var dlg = new NewChartForm();
            if (dlg.ShowDialog() == DialogResult.Cancel) return;
            var smb = dlg.Ticker;
            var timeframe = dlg.Timeframe;

            // если символ введен с несоблюдением регистра - заменить на оригинал
            // если символ отсутствует - ругнуться
            var allSymbols = DalSpot.Instance.GetTickerNames();
            var chartSmb = allSymbols.FirstOrDefault(s => s.Equals(smb, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(chartSmb))
            {
                MessageBox.Show(string.Format(Localizer.GetString("MessageSymbolSNotFound"), smb), Localizer.GetString("TitleError"));
                return;
            }
            
            var child = new ChartForm(this, chartSmb)
                            {
                                chart = {Timeframe = timeframe},
                                prefferedCountDaysInRequest = dlg.DaysInRequest
                            };
            SetupChartForm(child);

            
            ApplyTemplate(dlg.TemplateName, child);

            child.Show();
            child.LoadQuotesAndBuildChart(true);
        }
        
        private void MenuItemSettingsChartClick(object sender, EventArgs e)
        {
            ShowVisualSettingsDialog();
        }
        
        private void ShowVisualSettingsDialog()
        {
            if (ActiveMdiChild == null) return;
            if (ActiveMdiChild is ChartForm == false) return;
            var chart = (ChartForm)ActiveMdiChild;

            var dlg = new ChartSettingsForm(chart, Charts);
            dlg.ShowDialog();
        }
        #endregion

        #region Открытие - закрытие главного окна
        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !skipFormClosingPrompt)
            {
                // если нажали на кнопку "Закрыть"
                if (UserSettings.Instance.CloseTerminalPrompt)
                    if (MessageBox.Show(Localizer.GetString("MessageCloseTerminalQuestion"),
                                        Localizer.GetString("TitleConfirmation"), MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
            }

            var shouldSave = workspaceIsLoadedOk;
            if (!shouldSave && e.CloseReason == CloseReason.UserClosing)
            {
                // был сбой при старте - настройки не загружены
                // пользователь открывал окна?
                var hasWindows = (MdiChildren.Length + bookmarkStrip.BookmarksCount) > 0;
                if (hasWindows)
                    shouldSave = MessageBox.Show(Localizer.GetString("MessageErrorDuringLoadingAndSaveChangesQuestion"),
                                                 Localizer.GetString("TitleWarning"), MessageBoxButtons.YesNo) == DialogResult.Yes;
            }

            if (shouldSave)
            {
                UserSettings.Instance.TimeClosed = DateTime.Now;
                SaveCurrentSettings(false);
                //UserSettings.Instance.SaveSettings();
            }
            notifyIcon.Visible = false;

            // отвязаться от обработчиков, остановить циклические операции
            NewsCache.Instance.Terminate();
            //TradeSignalMonitorStream.Instance.Stop();
            TradeSignalReceiver.Instance.TradeSignalsUpdated -= InstanceTradeSignalsUpdated;
            TradeSignalReceiver.Instance.Stop();
            serverProxyTrade.OnConnectionReestablished -= ReviveFactory;
            AccountStatus.Instance.OnAccountDataFirstReceived -= ShowLast24HrOrders;
            AccountStatus.Instance.OnAccountInfoUpdated -= OnAccountInfoUpdated;
            AccountStatus.Instance.OnConnectionAborted -= ResumeConnectionWithServer;
            AccountStatus.Instance.OnAuthenticationFailed -= AuthenticationFailed;
            // остановить авто-обновления
            MarketOrdersStorage.Instance.MarketOrdersUpdated -= OnMarketOrdersChanged;
            MarketOrdersStorage.Instance.PendingOrdersUpdated -= OnPendingOrdersUpdatedChanged;
            MarketOrdersStorage.Instance.Stop();
            HistoryOrderStorage.Instance.StopUpdating();
            
            quoteReceiver.OnQuotesReceived -= OnQuotesReceived;
            quoteReceiver.OnNewsReceived -= OnNewsReceived;
            quoteReceiver.Stop();
            isExiting = true;
            if (!threadSchedule.Join(1000))
            {
                threadSchedule.Abort();
            }
            SaveObjectsAndIndicators(ExecutablePath.ExecPath);
            //SaveCurrentSettings(false);
            Contract.Util.BL.QuoteStorage.Instance.SaveQuotes(string.Format("{0}\\lastquote.txt", ExecutablePath.ExecPath));
            try
            {
                NewsStorage.Instance.SaveNews();
            }
            catch (Exception ex)
            {
                Logger.Error("ошибка сохранения новостей", ex);
            }
            // останов "фермы" роботов
            robotFarm.Stop();
            // остановка чата
            ChatEngine.Stop();
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            try
            {
                // шаг 6 - загрузка окон
                SplashScreen.UpdateState(StartupStage.LoadingWorkspace);
                // обработка кнопки печать
                if (!HiddenModes.ManagerMode)
                {
                    btnPrint.Visible = false;
                    btnNewChart.Left -= btnPrint.Width;
                    cbTimeFrame.Left -= btnPrint.Width;
                    panelChartTools.Left -= btnPrint.Width;
                    panelCommonTools.Left -= btnPrint.Width;
                }

                LoadVersionInfo();
                MainWindowTitle.Instance.Initialize(
                    terminalTitle, 
                    title => BeginInvoke(new Action<string>(t => Text = t), title));
                HelpManager.Instance.Initialize(ShowHelp);

                MarketOrdersStorage.Instance.MarketOrdersUpdated += OnMarketOrdersChanged;
                MarketOrdersStorage.Instance.PendingOrdersUpdated += OnPendingOrdersUpdatedChanged;
                MarketOrdersStorage.Instance.Start();
                LoadWorkspace();

                // шаг 7 - окончание инициализации
                SplashScreen.UpdateState(StartupStage.Finalizing);

                // настроить меню скриптов в окошках и в главном меню
                ActualizeScriptMenu();
                
                // получение торговых сигналов
                TradeSignalReceiver.Instance.TradeSignalsUpdated += InstanceTradeSignalsUpdated;
                TradeSignalReceiver.Instance.Start();
                // запустить поток проверки ордеров
                //TradeSignalMonitorStream.Instance.showMessageInUI += AddMessageToStatusPanelSafe;
                //TradeSignalMonitorStream.Instance.Start();
                // проиграть звук - терминал стартовал
                EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.Started);

                // если это первый запуск - показать диалог открытия счета
                if (UserSettings.Instance.FirstStarted)
                    BrowserSwitch.SetBrowserFeatureControl();

                if (UserSettings.Instance.FirstStarted && 
                    string.IsNullOrEmpty(UserSettings.Instance.Login))
                {
                    var form = new EditAccountDataForm();
                    form.SetMode(true);
                    form.ShowDialog();
                    var loginPwrd = form.AuthData;
                    if (loginPwrd.HasValue)
                    {
                        // авторизоваться
                        OpenLoginDialog(loginPwrd.Value.a, loginPwrd.Value.b);
                    }
                }
                UserSettings.Instance.FirstStarted = false;
                workspaceIsLoadedOk = true;
            }
            catch (Exception ex)
            {
                Logger.Error("MainFormLoad error", ex);
                workspaceIsLoadedOk = false;
            }
            finally
            {
                // закрыть диалог загрузки терминала
                SplashScreen.CloseSplashScreen();
            }
            // показать тултип
            ShowTooltip();
        }

        private void OnPendingOrdersUpdatedChanged()
        {
            var pOrders = MarketOrdersStorage.Instance.PendingOrders;
            var pStatus = MarketOrdersStorage.Instance.RequestOrderStatus;

            if (pStatus == RequestStatus.OK && pOrders != null)
                OnPendingOrdersReceived(pOrders.ToArray());   
        }

        private void OnMarketOrdersChanged()
        {
            var orders = MarketOrdersStorage.Instance.MarketOrders;
            var status = MarketOrdersStorage.Instance.RequestOrderStatus;

            if (status == RequestStatus.OK && orders != null)
                OnOrdersReceived(orders.ToArray());
        }

        private void LoadWorkspace(bool wasInitiated = false)
        {
            if (!loadChartsEvent.WaitOne(timeoutLockSaveMs))
            {
                Logger.Error("Таймаут на загрузке конфигурации (LoadWorkspace)");
                return;
            }
            terminalIsLoaded = false;
            loadChartsEvent.Reset(); // запретить авто-сохранение
            LoadEnabledTimeframes();

            // инициализировать контекст запросов
            if (!wasInitiated)
            {
                UserSettings.Instance.SaveSettings();
                CurrentProtectedContext.Instance.Initialize(UserSettings.Instance.TerminalId);
            }

            // главное окно
            ScaleWindowOnStart();

            // статусная панель
            StatusPanelHeight = UserSettings.Instance.StatusBarHeight;
            splitContainerStatus.SplitterDistance = splitContainerStatus.Width*
                                                    UserSettings.Instance.StatusSplitterPositionRel / 100;
            
            // документ - объекты графиков
            var docObjects = LoadObjectsDoc();
            // документ - настройки индюков
            var docIndi = LoadIndicatorSettingsXmlDoc();
            
            // шаг 6.5 - подкачка котировок
            if (wasInitiated)
            {
                StartupForm.Reset();
                SplashScreen.ShowSplash(StartupStage.LoadingWorkspace);
            }
            SplashScreen.UpdateState(StartupStage.LoadingWorkspace);
            
            // подкачать недостающие котировки
            PreloadChildChartsQuotes(docIndi);

            // дочерние формы - графики
            EnsureTabPages();
            var chartInfList = UserSettings.Instance.ChartSetsList.ToList();
            var chartNumber = 1;
            foreach (var chartInf in chartInfList)
            {
                SplashScreen.UpdateState(string.Format(
                    Localizer.GetString("MessageLoadingNChartFmt"),
                    chartNumber++, chartInfList.Count));
                using (new TimeLogger("LoadChildChart"))
                    LoadChildChart(chartInf, docObjects, docIndi, null, null, false);
            }
            
            // загружаем остальные окна
            nonChartWindows = UserSettings.Instance.WindowSetsList;
            using (new TimeLogger("ShowFirstBookmarkChildren"))
                ShowFirstBookmarkChildren();
            
            // обработать новости
            using (new TimeLogger("news"))
            {
                NewsStorage.Instance.LoadNews();
                List<News> lstNews;
                Logger.Info("Новости прочитаны из кэша");
                NewsStorage.Instance.ReadNews(null, out lstNews);
                if (lstNews != null)
                    if (lstNews.Count > 0)
                        OnNewsReceived(lstNews.ToArray());
                Logger.Info("Новости обработаны");
            }

            // логиним клиента если стоит опция авторегистрации
            if (AccountStatus.Instance.ServerConnectionStatus == ServerConnectionStatus.Connected &&
                !string.IsNullOrEmpty(UserSettings.Instance.Login) && 
                !string.IsNullOrEmpty(UserSettings.Instance.Account))
            {
                var pwrd = UserSettings.Instance.GetPasswordForLogin(UserSettings.Instance.Login);
                if (!string.IsNullOrEmpty(pwrd))
                {

                    // Login to account server
                    string authResultString;
                    AuthenticationResponse response;
                    bool success;
                    using (new TimeLogger("news"))
                        success = Authenticate(UserSettings.Instance.Login, pwrd,
                                               out response, out authResultString);
                    if (!success)
                    {
                        MessageBox.Show(authResultString, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                    var account = UserSettings.Instance.Account.ToIntSafe();
                    // подключиться к счету
                    if (account.HasValue && account.Value > 0)
                        SelectAccount(account.Value);
                }
            }
            HistoryOrderStorage.Instance.HurryUpUpdate();
            UpdateLastWorkspaceMenu();
            // закрыть диалог загрузки терминала
            if (wasInitiated) 
                SplashScreen.CloseSplashScreen();

            ShowOrHideNavPanel(UserSettings.Instance.NavPanelIsVisible, false);
            // выставить флаг - терминал загружен
            terminalIsLoaded = true;
            loadChartsEvent.Set();
            timeLastAutosave = new ThreadSafeTimeStamp();
            timeLastAutosave.Touch();
        }

        private void ScaleWindowOnStart()
        {
            // установить размеры, как есть
            if (Screen.AllScreens.Length > 1 && !UserSettings.Instance.FirstStarted)
            {
                if (UserSettings.Instance.WindowSize.Width > 50 && UserSettings.Instance.WindowSize.Height > 50)
                    SetBounds(UserSettings.Instance.WindowPos.X, UserSettings.Instance.WindowPos.Y,
                        UserSettings.Instance.WindowSize.Width, UserSettings.Instance.WindowSize.Height);
                return;
            }

            // скорректировать размеры под разрешение монитора
            var bounds = Screen.FromControl(this).Bounds;
            var x = UserSettings.Instance.WindowPos.X;
            if (x < bounds.X) x = bounds.X;
            var y = UserSettings.Instance.WindowPos.Y;
            if (y < bounds.Y) y = bounds.Y;

            var width = Math.Min(UserSettings.Instance.WindowSize.Width, bounds.Width);
            var height = Math.Min(UserSettings.Instance.WindowSize.Height, bounds.Height);

            var right = x + width;
            var bottom = y + height;

            var deltaX = right - bounds.Right;
            if (deltaX > 0) x -= deltaX;

            var deltaY = bottom - bounds.Bottom;
            if (deltaY > 0) y -= deltaY;

            if (UserSettings.Instance.WindowSize.Width > 50 && UserSettings.Instance.WindowSize.Height > 50)
                SetBounds(x, y, width, height);
        }

        /// <summary>
        /// обновить список последних сохраненных - загруженных workspace-ов
        /// </summary>
        private void UpdateLastWorkspaceMenu(string filePath = "")
        {
            const int maxCharsInPath = 45;
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!UserSettings.Instance.AddWorkspace(filePath)) return;
            }
            else
                UserSettings.Instance.VerifyWorkspaceListFilesAreExist();
            // обновить подпункты меню
            menuLoadWorkspace.DropDownItems.Clear();
            menuLoadWorkspace.Click -= MenuLoadWorkspaceClick;

            // добавить пункт - выбор из файла
            if (UserSettings.Instance.LastWorkspaces.Count > 0)
            {
                var item = menuLoadWorkspace.DropDownItems.Add(Localizer.GetString("TitleSelectFileMenu"));
                item.Font = new Font(item.Font, FontStyle.Bold);
                item.Click += MenuLoadWorkspaceClick;                
            }
            else
            {
                menuLoadWorkspace.Click += MenuLoadWorkspaceClick;
            }
            foreach (var path in UserSettings.Instance.LastWorkspaces)
            {
                // из полного пути к файлу сделать короткий огрызочек
                var shortPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
                if (path.Length > maxCharsInPath)
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    if (fileName.Length > maxCharsInPath)
                    {
                        fileName = fileName.Substring(0, 4) + ".." + fileName.Substring(fileName.Length - 38);
                        shortPath = fileName;
                    }
                    else if (fileName.Length < maxCharsInPath - 6)
                    {
                        var numCharsFromFolder = maxCharsInPath - fileName.Length - 6;
                        var folder = Path.GetDirectoryName(path);
                        var folderPreffix = folder.Length <= numCharsFromFolder
                                                ? folder
                                                : ".." + folder.Substring(folder.Length - numCharsFromFolder);
                        shortPath = folderPreffix + "\\" + fileName;
                    }
                }

                var item = menuLoadWorkspace.DropDownItems.Add(shortPath);
                item.Tag = path;
                item.Click += (sender, args) =>
                                  {
                                      var itemSender = (ToolStripItem) sender;
                                      LoadWorkspaceFromFile((string) itemSender.Tag);
                                  };
            }            
        }

        /// <summary>
        /// подгрузить котировки для дочерних форс
        /// вызывается перед загрузкой окошек
        /// </summary>
        private void PreloadChildChartsQuotes(XmlDocument docIndi)
        {
            var dicQuoteToRefresh = new Dictionary<string, DateTime>();
            // время самой первой свечки (котировки) графика
            var firstChartCandleTime = new Dictionary<string, DateTime>();
            var chartIndicators = new Dictionary<ChartWindowSettings, List<IHistoryQueryIndicator>>();

            var nowTime = DateTime.Now;

            // получить все тикеры по графикам и подгрузить их в хранилище котировок            
            var charts = UserSettings.Instance.ChartSetsList.ToList();
            foreach (var chartInf in charts)
            {
                if (string.IsNullOrEmpty(chartInf.Symbol))
                {
                    Logger.ErrorFormat("PreloadChildChartsQuotes - chart #{0} has no symbol",
                                       chartInf.UniqueId);
                    continue;
                }

                // тикер самого графика
                if (!dicQuoteToRefresh.ContainsKey(chartInf.Symbol))
                {
                    var dateRange = AtomCandleStorage.Instance.LoadFromFile(ExecutablePath.ExecPath + 
                        TerminalEnvironment.QuoteCacheFolder, chartInf.Symbol);
                    
                    // здесь учтутся дырки в последней истории котировок
                    var downloadStartTime = ChartForm.GetQuoteRefreshingTimeBounds(dateRange.HasValue ? dateRange.Value.a : (DateTime?)null,
                                                           dateRange.HasValue ? dateRange.Value.b : (DateTime?)null, nowTime,
                                                           chartInf.Symbol);

                    // обновить время старта загрузки котировок для индикатора
                    var timeFromFile = dateRange.HasValue ? dateRange.Value.a : DateTime.Now;
                    var timeRequested = downloadStartTime ?? timeFromFile;
                    var firstCandleTime = timeFromFile < timeRequested ? timeFromFile : timeRequested;
                    
                    if (!firstChartCandleTime.ContainsKey(chartInf.Symbol))
                        firstChartCandleTime.Add(chartInf.Symbol, firstCandleTime);
                    else
                    {
                        if (firstChartCandleTime[chartInf.Symbol] > firstCandleTime)
                            firstChartCandleTime[chartInf.Symbol] = firstCandleTime;
                    }
                        
                    if (!downloadStartTime.HasValue)
                        downloadStartTime = nowTime;
                    
                    dicQuoteToRefresh.Add(chartInf.Symbol, downloadStartTime.Value);                    
                }                
            }

            foreach (var chartInf in UserSettings.Instance.ChartSetsList)
            {
                // тикеры, запрашиваемые индикаторами
                var chartIndis = new List<IHistoryQueryIndicator>();
                chartIndicators.Add(chartInf, chartIndis);

                if (docIndi == null) continue;
                var chartNode = (XmlElement)docIndi.SelectSingleNode(string.Format(
                        "/indicators/chart[@Id='{0}']", chartInf.UniqueId));
                if (chartNode == null) continue;

                // индикатор должен знать, с какой отметки строится чарт
                var timeStart = firstChartCandleTime[chartInf.Symbol];
                    //dicQuoteToRefresh.ContainsKey(chartInf.Symbol)
                    //                ? dicQuoteToRefresh[chartInf.Symbol]
                    //                : ChartForm.GetDefaultQuoteRefreshStartDate(nowTime);
                
                foreach (XmlElement node in chartNode)
                {
                    var indi = BaseChartIndicator.LoadIndicator(node);
                    if (indi == null) continue;
                    if (indi is IHistoryQueryIndicator == false) continue;
                    var histIndi = (IHistoryQueryIndicator) indi;
                    chartIndis.Add(histIndi);

                    // получить список тикеров для индикатора
                    var indiTickers = histIndi.GetRequiredTickersHistory(timeStart);
                    foreach (var indiTicker in indiTickers)
                    {
                        if (string.IsNullOrEmpty(indiTicker.Key)) continue;
                        if (!dicQuoteToRefresh.ContainsKey(indiTicker.Key))
                        {
                            dicQuoteToRefresh.Add(indiTicker.Key, indiTicker.Value);
                            // подгрузить котировки по тикеру из файла
                            AtomCandleStorage.Instance.LoadFromFile(ExecutablePath.ExecPath + 
                                TerminalEnvironment.QuoteCacheFolder, indiTicker.Key);
                        }
                        else
                        {
                            if (dicQuoteToRefresh[indiTicker.Key] > indiTicker.Value)
                                dicQuoteToRefresh[indiTicker.Key] = indiTicker.Value;
                        }
                    }                                        
                }
            }
            
            if (dicQuoteToRefresh.Count == 0) return;
            if (AccountStatus.Instance.ServerConnectionStatus == ServerConnectionStatus.NotConnected)
                return;

            // загрузить недостающие котировки
            UpdateTickersCacheForRobots(dicQuoteToRefresh, minMinutesToUpdateQuotes);

            // сохранить прочитанные котировки
            // AtomCandleStorage.Instance.FlushInFiles(ExecutablePath.ExecPath + TerminalEnvironment.QuoteCacheFolder);
        }

        private void LoadVersionInfo()
        {
            var versionParams = new Dictionary<string, string>();
            using (var stream = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream("TradeSharp.Client.Resources.SubversionInfo.txt"))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    versionParams.Add(parts[0], parts[1]);
                }
            }

            var strRev = versionParams.ContainsKey("Revision") ? versionParams["Revision"] : string.Empty;
            var strBuilt = versionParams.ContainsKey("Built") ? versionParams["Built"] : string.Empty;

            terminalVersion = strRev;

            if (!string.IsNullOrEmpty(strRev) && !string.IsNullOrEmpty(strBuilt))
            {
                terminalTitle = string.Format("TRADE# r{0} [{1}]", strRev, strBuilt);
            }
        }

        ///// <summary>
        ///// индикатор может затребовать данные с других графиков
        ///// </summary>        
        //private void ProvideChartsForIndicator(List<string> chartIds, out List<CandleChartControl> requestedCharts)
        //{
        //    requestedCharts = Charts.Where(c => chartIds.Any(id => id == c.UniqueId)).Select(c => c.chart).ToList();
        //}

        private void LoadChildChartSafe(ChartWindowSettings chartInf, XmlElement nodeObjects,
            XmlElement nodeIndi, bool loadQuotesFromServer)
        {
            Invoke((MethodInvoker) (() => LoadChildChart(chartInf, nodeObjects, nodeIndi,
                                                         loadQuotesFromServer)));                
        }

        private void LoadChildChart(ChartWindowSettings chartInf, XmlElement nodeObjects,
            XmlElement nodeIndi, bool loadQuotesFromServer)
        {
            foreach (var childold in Charts.Where(childold => childold.UniqueId == chartInf.UniqueId 
                && childold.bookmarkId == chartInf.TabPageId && childold.chart.Symbol == chartInf.Symbol))
            {
                childold.Close();
            }

            ChartForm child;
            using (new TimeLogger("new ChartForm"))
            {
                child = new ChartForm(this, chartInf) { UniqueId = chartInf.UniqueId };            
                SetupChartForm(child);
                child.bookmarkId = chartInf.TabPageId;
                // если окно ссылается на несуществующую вкладку - приписать его к существующей
                var bookmark = BookmarkStorage.Instance[child.bookmarkId];
                if (bookmark == null)
                    child.bookmarkId = BookmarkStorage.Instance.bookmarks[0].Id;
            }

            using (new TimeLogger("nodeObjects"))
            {
                if (!string.IsNullOrEmpty(chartInf.Timeframe))
                    child.chart.Timeframe = new BarSettings(chartInf.Timeframe);

                // указать ветку объектов для документа
                if (nodeObjects != null) child.xmlRootObjects = nodeObjects;

                // загрузить индюки для чарта
                if (nodeIndi != null)
                {
                    child.chart.LoadIndicatorSettings(nodeIndi);
                    child.chart.UpdateIndicatorPanesAndSeries();
                }
            }

            using (new TimeLogger("child.Show()"))
            {
                // выставить размеры панелей
                child.LoadPaneLocations(chartInf);

                // применить тему оформления
                child.chart.chart.visualSettings.ApplyTheme();

                child.Show();
            }
            using (new TimeLogger("LoadQuotesAndBuildChart"))            
                child.LoadQuotesAndBuildChart(loadQuotesFromServer);
            if (chartInf.FirstCandleIndex == chartInf.LastCandleIndex)
                child.chart.chart.FitChart();
            // выставить размеры и позицию
            if (chartInf.WindowSize.Width > 20 && chartInf.WindowSize.Height > 10)
                child.SetBounds(chartInf.WindowPos.X, chartInf.WindowPos.Y,
                                chartInf.WindowSize.Width, chartInf.WindowSize.Height);
            child.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), chartInf.WindowState);
            //tabCtrl.SelectedTab = tp;
        }

        private void LoadChildChart(ChartWindowSettings chartInf, XmlDocument docObjects,
            XmlDocument docIndi, string prefixObjects, string prefixIndi, bool loadQuotesFromServer)
        {
            XmlElement nodeObjects = null, nodeChart = null;
            if (docObjects != null)            
                nodeObjects = (XmlElement)docObjects.SelectSingleNode(
                    string.Format("{0}/objects/chart[@Id='{1}']", string.IsNullOrEmpty(prefixObjects) ? "" : prefixObjects, chartInf.UniqueId));                            

            // загрузить индюки для чарта
            if (docIndi != null)            
                nodeChart = (XmlElement)docIndi.SelectSingleNode(string.Format(
                    "{0}/indicators/chart[@Id='{1}']", string.IsNullOrEmpty(prefixIndi) ? "" : prefixIndi, chartInf.UniqueId));

            LoadChildChart(chartInf, nodeObjects, nodeChart, loadQuotesFromServer);
        }

        private static XmlDocument LoadIndicatorSettingsXmlDoc()
        {
            var doc = new XmlDocument();
            var path = string.Format("{0}\\{1}", ExecutablePath.ExecPath, 
                CandleChartControl.IndicatorsFileName);
            if (!File.Exists(path)) return null;
            try
            {
                doc.Load(path);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Ошибка загрузки из файла {0}", path), ex);
                return null;
            }
            return doc;
        }

        private static XmlDocument LoadObjectsDoc()
        {
            var pathObjects = string.Format("{0}\\{1}", ExecutablePath.ExecPath, FileNameObjectSettings);
            var docObjects = new XmlDocument();
            try
            {
                if (File.Exists(pathObjects)) docObjects.Load(pathObjects);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка загрузки документа \"{0}\": {1}", pathObjects, ex);
            }
            return docObjects.DocumentElement == null ? null : docObjects;
        }
        #endregion
        
        /// <summary>
        /// переключить кнопку инструмента
        /// </summary>        
        private void OnChartToolChanged(CandleChartControl.ChartTool newTool)
        {
            // назначить инструмент графикам
            foreach (var child in Charts)
            {
                child.chart.ActiveChartTool = newTool;
                child.chart.seriesEditParameters = new List<SeriesEditParameter>();
            }
            // ищем все кнопки/п.меню с указанным инструментом и выделяем их
            // (надееемся, что сюда не попадут кнопки/п.меню,
            // которым соответствуют одинаковые инструменты, но с разными настройками)
            foreach (Control btn in panelChartTools.Controls)
            {
                if (btn.Tag == null)
                    continue;
                var chartButton = btn.Tag as ChartToolButtonSettings;
                if (chartButton == null)
                    continue;
                var checkBox = btn as CheckBox;
                if (checkBox == null)
                    continue;
                checkBox.Checked = newTool == chartButton.Tool;
            }
            foreach (var menu in buttonMenus)
            {
                foreach (var item in menu.Items)
                {
                    var menuButton = item as ToolStripButton;
                    if (menuButton == null)
                        continue;
                    if (menuButton.Tag == null)
                        continue;
                    var chartMenuButton = menuButton.Tag as ChartToolButtonSettings;
                    if (chartMenuButton == null)
                        continue;
                    menuButton.Checked = newTool == chartMenuButton.Tool;
                }
            }
        }

        private void ChartScaleChanged(DateTime start, DateTime end, CandleChartControl chart)
        {
            if (!UserSettings.Instance.SynchGraphics) return;
            // синхронно поменять границы другим чартам
            foreach (var child in MdiChildren)
            {
                if (child is ChartForm == false) continue;
                var childChart = ((ChartForm) child).chart;
                if (((ChartForm)child).bookmarkId != ((ChartForm)chart.Parent).bookmarkId) continue;

                if (childChart == chart ||
                    (childChart.Symbol != chart.Symbol &&
                        UserSettings.Instance.SynchGraphicsAnyPair == false)) continue;
                childChart.SetScale(start, end);
            }
        }

        private void ChartCrossChanged(DateTime? time, double? price, CandleChartControl sender)
        {
            foreach (var child in MdiChildren)
            {
                if (child is ChartForm == false) continue;
                var chartForm = (ChartForm) child;
                var childChart = chartForm.chart;
                // не перерисовывать крестик для исходного чарта
                if (childChart == sender) continue;
                // пропустить чарты с другой вкладки, а также свернутые чарты
                if (chartForm.bookmarkId != ((ChartForm)sender.Parent).bookmarkId) continue;
                // обновить перекрестие
                childChart.OnCursorCrossChanged(time, price);
            }
        }

        /// <summary>
        /// закрыть окно инструмента и снова открыть его
        /// не подгружать котировки с сервера
        /// </summary>        
        public void ReopenChild(ChartForm child)
        {
            // получить настройки графика
            var childSets = GetChildSettings(child);           
            UserSettings.Instance.ChartSetsList.Add(childSets);                        
            // закрыть окошко
            child.Close();
            // открыть окошко
            // !! не загружаются объекты и индикаторы
            LoadChildChart(childSets, null, null, null, null, false);
        }

        private static ChartWindowSettings GetChildSettings(ChartForm child)
        {
            var childSets = new ChartWindowSettings
                                {                                        
                                    WindowSize = new Size(
                                        child.WindowState != FormWindowState.Normal ?
                                                child.RestoreBounds.Width : child.Width,
                                        child.WindowState != FormWindowState.Normal  ?
                                                child.RestoreBounds.Height : child.Height),
                                    WindowPos = new Point(
                                        child.WindowState != FormWindowState.Normal ?
                                        child.RestoreBounds.Left : child.Left,
                                        child.WindowState != FormWindowState.Normal ?
                                        child.RestoreBounds.Top : child.Top),                                        
                                    WindowState = child.WindowState.ToString(),
                                    UniqueId = child.UniqueId,
                                    TabPageId = child.bookmarkId
                                };
            child.InitChartSettings(childSets);
            return childSets;
        }

        private void LoadEnabledTimeframes()
        {
            cbTimeFrame.Items.Clear();
            foreach (var bars in BarSettingsStorage.Instance.GetCollection())
            {
                cbTimeFrame.Items.Add(new ObjectWithTag<BarSettings>(bars.Title, bars));
            }
        }

        private void CbTimeFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTimeFrame.SelectedItem == null) return;
            var tf = (ObjectWithTag<BarSettings>) cbTimeFrame.SelectedItem;
            // поменять TF для графика
            if (ActiveMdiChild == null) return;
            if (ActiveMdiChild is ChartForm == false) return;
            var chart = (ChartForm) ActiveMdiChild;
            chart.chart.Timeframe = tf.value;
            chart.chart.RebuildChart(false);
        }

        private void MainFormMdiChildActivate(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null) return;
            if (ActiveMdiChild is ChartForm == false) return;
            
            var chart = (ChartForm) ActiveMdiChild;
            lastActiveChart = chart;
            
            for (var i = 0; i < cbTimeFrame.Items.Count; i++)
            {
                var tf = (ObjectWithTag<BarSettings>) cbTimeFrame.Items[i];
                if ((tf.value == chart.chart.Timeframe) == false) continue;
                // показать таймфрейм                
                cbTimeFrame.SelectedIndexChanged -= CbTimeFrameSelectedIndexChanged;
                cbTimeFrame.SelectedIndex = i;
                cbTimeFrame.SelectedIndexChanged += CbTimeFrameSelectedIndexChanged;
                // показать заголовок
                return;
            }
        }

        /// <summary>
        /// настроить серии (статические свойства)
        /// </summary>
        private void InitializeSeriesSettings()
        {
            var node = ToolSettingsStorageFile.LoadNode(ToolSettingsStorageFile.NodeNameSeries);
            if (node == null) return;
            CandleChartControl.LoadSeriesSettingsFromXml(node);
        }

        /// <summary>
        /// задать кнопкам подсказки
        /// </summary>
        private void InitializeTooltips()
        {
            buttonToolTip.SetToolTip(btnLoadConfig, Localizer.GetString("TitleApplySettings"));
            buttonToolTip.SetToolTip(btnSaveConfig, Localizer.GetString("TitleSaveSettings"));
            buttonToolTip.SetToolTip(btnPrint, Localizer.GetString("TitlePrintSelectedWindow"));
            buttonToolTip.SetToolTip(btnNewChart, Localizer.GetString("TitleNewChart"));
            
            buttonToolTip.SetToolTip(btnChat, Localizer.GetString("TitleChatWindow"));
            buttonToolTip.SetToolTip(btnExpandStatusPanel, Localizer.GetString("TitleExpandStatusBar"));
            buttonToolTip.SetToolTip(btnShrinkStatusPanel, Localizer.GetString("TitleCollapseStatusBar"));
        }

        /// <summary>
        /// показать выбранные кнопки инструментальной панели
        /// </summary>
        private void InitializeToolButtons(List<ChartToolButtonSettings> buttons, Control panel)
        {
            const int buttonMarging = 3;
            var buttonTop = toolBtnCross.Top;
            var firstUserButtonX = btnFlipPanelChartTools.Right + buttonMarging;
            var buttonSize = toolBtnCross.Size;
            
            // удалить кнопки
            while (panel.Controls.Count > 1)
                panel.Controls.RemoveAt(1);

            // добавить кнопки
            var left = firstUserButtonX;
            foreach (var btnDescr in buttons)
            {
                if (btnDescr.Group != null)
                    continue;
                var button = new CheckBox
                {
                    Parent = panel,
                    ImageIndex = btnDescr.Image,
                    Tag = btnDescr,
                    Appearance = Appearance.Button,
                    //FlatStyle = FlatStyle.Flat,
                    ImageList = lstGlyph32,
                };

                // кнопка "Курсор" нажата по-умолчанию
                if (btnDescr.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart &&
                    btnDescr.Tool == CandleChartControl.ChartTool.Cursor)
                    button.Checked = true;

                // кнопки "Запустить роботов", "Портфель роботов", "Состояние роботов"
                UpdateRobotIconUnsafe(robotFarm != null ? robotFarm.State : RobotFarm.RobotFarmState.Stopped);

                buttonToolTip.SetToolTip(button, btnDescr.ToString());
                if (btnDescr.IsVisibleDisplayName)
                {
                    button.Text = btnDescr.ToString();
                    var len = (int) button.CreateGraphics().MeasureString(button.Text, button.Font).Width +
                              lstGlyph32.ImageSize.Width + 16;
                    button.SetBounds(left, buttonTop, len, buttonSize.Height);
                    left += (len + buttonMarging);
                    button.ImageAlign = ContentAlignment.MiddleLeft;
                    button.TextAlign = ContentAlignment.MiddleRight;
                }
                else
                {
                    button.SetBounds(left, buttonTop, buttonSize.Width, buttonSize.Height);
                    left += (buttonSize.Width + buttonMarging);
                }
                button.Click += ToolStripBtnClick;
                // добавить в панель
                panel.Controls.Add(button);
            }

            // добавить кнопки-менюшки
            var groupBtnWidth = buttonSize.Width + 12;
            var buttonGroups = buttons.Where(b => b.Group != null).Select(b => b.Group).Distinct().ToList();
            foreach (var group in buttonGroups)
            {
                var groupBtn = new Button
                                   {
                                       Parent = panel,
                                       ImageIndex = group.ImageIndex,
                                       Tag = group,
                                       ImageList = lstGlyph32,
                                       FlatStyle = FlatStyle.Flat,
                                       Text = " ...",
                                       ImageAlign = ContentAlignment.MiddleLeft,
                                       TextAlign = ContentAlignment.MiddleRight                                       
                                   };
                buttonToolTip.SetToolTip(groupBtn, group.Title);
                groupBtn.SetBounds(left, buttonTop, groupBtnWidth, buttonSize.Height);
                left += (groupBtn.Width + buttonMarging);

                // создать менюшку
                groupBtn.Click += GroupBtnClick;
                var btnMenu = new ContextMenuStrip {/*Parent = this,*/ Tag = group.Title};
                var thisGroup = group;
                var groupButtons = buttons.Where(b => b.Group == thisGroup);
                foreach (var btn in groupButtons)
                {
                    var item = new ToolStripButton(btn.ToString(), lstGlyph32.Images[btn.Image],
                                                   ToolStripBtnClick) {Tag = btn};
                    btnMenu.Items.Add(item);
                    if (btnMenu.Width < item.Width)
                        btnMenu.Width = item.Width;
                }
                // добавить пустышку, чтобы размер меню был посчитан корректно
                if (btnMenu.Items.Count == 1)
                {
                    var item = new ToolStripLabel("empty") {Visible = false};
                    btnMenu.Items.Add(item);
                }
                
                buttonMenus.Add(btnMenu);
            }
            
            // назначить дефолтовый инструмент графикам
            foreach (var child in Charts)
            {
                child.chart.ActiveChartTool = CandleChartControl.ChartTool.Cursor;
            }
            // развернуть панель
            BtnFlipPanelChartToolsClick(panel.Controls[0], EventArgs.Empty);
        }

        private void GroupBtnClick(object sender, EventArgs e)
        {
            var button = (Control)sender;
            var sets = (ToolButtonGroup) button.Tag;
            // открыть меню с "кнопками"
            var menu = buttonMenus.FirstOrDefault(m => (string) m.Tag == sets.Title);
            if (menu == null)
                return;
            var coords = button.PointToScreen(new Point(0, button.Height));
            //coords = PointToClient(coords);
            menu.Show(coords.X, coords.Y);
        }
        
        private void ToolStripBtnClick(object sender, EventArgs e)
        {
            ChartToolButtonSettings sets;
            CheckBox button = null;

            if (sender is ToolStripButton)
            {
                sets = ((ToolStripButton) sender).Tag as ChartToolButtonSettings;
            }
            else
            {
                button = sender as CheckBox;
                if (button == null)
                    return;
                sets = button.Tag as ChartToolButtonSettings;
            }

            if (sets == null)
                return;
            if (sets.ButtonType == ChartToolButtonSettings.ToolButtonType.System)
            {
                bool newBtnState;
                ExecuteCommonButtonsCommand(sets.SystemTool, out newBtnState);
                if (button != null)
                    button.Checked = newBtnState;
                return;
            }

            // особое поведение у ChartToolButtonSettings.ToolButtonType.Chart
            // выделяем только эту кнопку - все остальные сбрасываем
            foreach (Control btn in panelChartTools.Controls)
            {
                if (btn.Tag == null)
                    continue;
                var chartButton = btn.Tag as ChartToolButtonSettings;
                if (chartButton == null)
                    continue;
                var checkBox = btn as CheckBox;
                if (checkBox == null)
                    continue;
                checkBox.Checked = sets == chartButton;
            }
            foreach (var menu in buttonMenus)
            {
                foreach (var item in menu.Items)
                {
                    var menuButton = item as ToolStripButton;
                    if (menuButton == null)
                        continue;
                    if (menuButton.Tag == null)
                        continue;
                    var chartMenuButton = menuButton.Tag as ChartToolButtonSettings;
                    if (chartMenuButton == null)
                        continue;
                    menuButton.Checked = sets == chartMenuButton;
                }
            }

            // назначить инструмент графикам
            foreach (var child in Charts)
            {
                child.chart.ActiveChartTool = sets.Tool;
                child.chart.seriesEditParameters = sets.toolParams;
            }
        }

        private void MenuSaveClick(object sender, EventArgs e)
        {
            SaveCurrentSettings();
        }
        
        private void MenuManageTemplateClick(object sender, EventArgs e)
        {
            var manageChartTemplate = new ManageTemplate();
            manageChartTemplate.ShowDialog();
        }
        
        private void MenuItemNewOrderClick(object sender, EventArgs e)
        {
            if (!AccountStatus.Instance.isAuthorized)
            {
                MessageBox.Show(Localizer.GetString("MessagePleaseLogInOnServer"), Localizer.GetString("TitleError"));
                return;
            }
            if (ActiveMdiChild == null || ActiveMdiChild is ChartForm == false)
            {
                if (dlgOrder == null || !dlgOrder.IsHandleCreated)
                {
                    dlgOrder = new OrderDlg();
                    dlgOrder.ShowDialog();
                }
                else
                    dlgOrder.Visible = true;
                return;
            }
            var chart = (ChartForm)ActiveMdiChild;
            PlaceNewOrder(chart.chart.Symbol);
        }

        public static void PlaceNewOrder(string symbol)
        {
            if (Instance.dlgOrder == null || !Instance.dlgOrder.IsHandleCreated)
            {
                Instance.dlgOrder = new OrderDlg(symbol);
                Instance.dlgOrder.ShowDialog();
            }
            else
            {
                Instance.dlgOrder.Visible = true;
            }
        }
        
        public void MakeTrade(DealType side, string symbol, int volume, bool shouldPrompt)
        {
            // вывести запрос - подтверждение трейда
            if (shouldPrompt)
            {
                var request = Localizer.GetString("MessagePositionWillBeOpened") + ": " + side + " " + volume.ToStringUniformMoneyFormat()
                              + " " + symbol + ". " + Localizer.GetString("MessageContinueQuestion");
                if (MessageBox.Show(request, Localizer.GetString("TitleConfirmation"), MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            try
            {
                SendNewOrderRequestSafe(RequestUniqueId.Next(),
                                        AccountStatus.Instance.accountID,
                                        new MarketOrder
                                            {
                                                Volume = volume,
                                                Side = (int) side,
                                                Symbol = symbol,


                                            },
                                        0, 0, OrderType.Market);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в MakeTrade: SendNewOrderRequest error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageErrorSendingRequest"), Localizer.GetString("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void MakeTrade(DealType side, string symbol, int volume)
        {
            MakeTrade(side, symbol, volume, UserSettings.Instance.PromptFastButtonTrade);
        }

        private void MenuItemAccountClick(object sender, EventArgs e)
        {
            OpenLoginDialog();
        }

        public void Ping()
        {
        }

        /// <summary>
        /// пришло служебное сообщение от сервиса
        /// </summary>
        public void ProcessServiceMessage(ServiceMessageCategory cat, string message)
        {
            if (cat == ServiceMessageCategory.LogMessage)
            {
                Logger.InfoFormat("Service objectBinding: {0}", message);
                return;
            }

            var action = UserSettings.Instance.GetAccountEventAction(AccountEventCode.ServerMessage);
            if (action == AccountEventAction.DoNothing) return;

            if (cat == ServiceMessageCategory.DialogBox)
            {
                bool repeatNotification;
                NotificationBox.Show(message, Localizer.GetString("TitleServerMessage"), MessageBoxIcon.Information, out repeatNotification);
                if (!repeatNotification)
                {
                    UserSettings.Instance.SwitchAccountEventAction(AccountEventCode.ServerMessage);
                    UserSettings.Instance.SaveSettings();
                }
                return;
            }
            if (cat == ServiceMessageCategory.ServiceMessage)
            {
                ShowMsgWindowSafe(new AccountEvent("", message, AccountEventCode.ServerMessage));
                // отправить в некоторое окошко... пока не знаю, куда !!
                return;
            }
        }

        public void NewOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            if (status != RequestStatus.OK)
            {
                var statStr = EnumFriendlyName<RequestStatus>.GetString(status);
                var msg = string.Format(Localizer.GetString("MessageOrderRejected") + 
                    " ({0} {1}{2}): {3}, \"{4}\"", 
                    order.Side > 0 ? "BUY" : "SELL", order.Volume, order.Symbol,
                    statStr, detailMessage);
                ShowMsgWindowSafe(new AccountEvent(
                    Localizer.GetString("TitleError"),
                    msg, AccountEventCode.ServerMessage));
                Logger.InfoFormat(msg);                
            }
            else
            {
                var sideStr = order.Side > 0 ? "BUY" : "SELL";
                var title =
                    string.Format(Localizer.GetString("MessageDealOpeningFmt"),
                                  sideStr, order.Symbol);                    
                    
                var body = new StringBuilder();
                body.AppendLine(string.Format(
                    Localizer.GetString("MessageOrderNotificationParamsFmt"),
                    order.Side > 0 ? Localizer.GetString("TitlePurchase") : Localizer.GetString("TitleSelling"),
                    sideStr,
                    order.Volume.ToStringUniformMoneyFormat(),
                    order.Symbol,
                    order.PriceEnter.ToStringUniformPriceFormat(true)));
                if (order.StopLoss > 0 || order.TakeProfit > 0)                
                    body.AppendLine(string.Format("SL: {0}, TP: {1}",
                                                  order.StopLoss > 0
                                                      ? order.StopLoss.Value.ToStringUniformPriceFormat(false)
                                                      : "-",
                                                  order.TakeProfit > 0
                                                      ? order.TakeProfit.Value.ToStringUniformPriceFormat(false)
                                                      : "-"));
                if (order.TrailLevel1.HasValue)
                    body.AppendLine(string.Format(Localizer.GetString("MessageOrderNotificationSlTpFmt"), 
                        (order.PriceEnter + DalSpot.Instance.GetAbsValue(
                            order.Symbol, (float)(order.TrailLevel1)*order.Side)).ToStringUniformPriceFormat(),
                        order.TrailTarget1));
                body.AppendLine(string.Format(Localizer.GetString("MessageNotificationOrderAccountIDFmt"), 
                    order.ID, order.AccountID));

                ShowMsgWindowSafe(new AccountEvent(title, body.ToString(), 
                    AccountEventCode.DealOpened));
                
                // проиграть звук - торговый ордер
                EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.TradeResponse);
                CheckScriptTriggerOrder(ScriptTriggerDealEventType.НовыйОрдер, order);
            }
        }

        public void NewPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            var accountEvent = new AccountEvent(
                string.Format(Localizer.GetString("MessageNotificationPendingPlacedTitleFmt"), order.ID),

                string.Format(Localizer.GetString("MessageNotificationPendingPlacedBodyFmt"), 
                    order.Side > 0 ? "BUY" : "SELL",
                    order.Volume, order.Symbol, order.PriceFrom, order.ID, order.AccountID), AccountEventCode.PendingOrder);
            ShowMsgWindowSafe(accountEvent);
            // проиграть звук - торговый ордер
            EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.TradeResponse);
        }

        public void NewCloseOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            HistoryOrderStorage.Instance.HurryUpUpdate();
            var acData = AccountStatus.Instance.AccountData;
            if (acData == null) return;
            
            var depoCurx = AccountStatus.Instance.AccountData.Currency;
            var percentProf = acData.Equity == 0 ? 0 : 100*order.ResultDepo/(float)acData.Equity;

            ShowMsgWindowSafe(new AccountEvent(
                string.Format(Localizer.GetString("MessageNotificationCloseTitleFmt"), order.ID),
                               string.Format(
                                   Localizer.GetString("MessageNotificationCloseBodyFmt"),
                                   order.ID, order.Side > 0 ? "BUY" : "SELL",
                                   order.Volume.ToStringUniformMoneyFormat(),
                                   order.Symbol,
                                   order.TimeEnter.ToStringUniform(),
                                   DalSpot.Instance.FormatPrice(order.Symbol, order.PriceEnter),
                                   DalSpot.Instance.FormatPrice(order.Symbol, order.PriceExit ?? 0),
                                   order.ResultDepo.ToStringUniformMoneyFormat(), // {7}
                                   depoCurx, // {8}
                                   order.ResultPoints,
                                   percentProf,
                                   acData.Equity.ToStringUniformMoneyFormat(true),
                                   depoCurx), AccountEventCode.DealClosed));
            // проиграть звук - торговый ордер
            EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.TradeResponse);
            CheckScriptTriggerOrder(ScriptTriggerDealEventType.ЗакрытиеОрдера, order);
        }

        public void EditOrderResponse(MarketOrder order, RequestStatus status, string detailMessage)
        {
            ShowMsgWindowSafe(new AccountEvent(
                Localizer.GetString("MessageNotificationOrderChangedTitle"),
                string.Format(Localizer.GetString("MessageNotificationOrderModifiedBodyFmt"), 
                    order.ID, order.Side > 0 ? "BUY" : "SELL",
                    order.Symbol, order.PriceEnter, order.StopLoss.HasValue ? order.StopLoss.ToString() : Localizer.GetString("TitleNo").ToLower(), 
                    order.TakeProfit.HasValue ? order.TakeProfit.ToString() : Localizer.GetString("TitleNo").ToLower(), 
                    order.AccountID,
                    order.TrailLevel1 == null 
                        ? ""
                        : "\n" + string.Format(Localizer.GetString("MessageNotificationModifiedAutoTrailFmt"), 
                    order.PriceEnter + DalSpot.Instance.GetAbsValue(order.Symbol, (float) (order.TrailLevel1) * order.Side), order.TrailTarget1)),
                    AccountEventCode.DealModified));
            CheckScriptTriggerOrder(ScriptTriggerDealEventType.РедактированиеОрдера, order);
        }

        public void EditPendingOrderResponse(PendingOrder order, RequestStatus status, string detailMessage)
        {
            ShowMsgWindowSafe(new AccountEvent(
                Localizer.GetString("MessageNotificationPendingChangedTitle"), 
                string.Format(Localizer.GetString("MessageNotificationPendingChangedBodyFmt"), 
                order.ID, order.Symbol, order.AccountID), AccountEventCode.PendingOrderModified));
        }

        /// <summary>
        /// обновить статусную панель
        /// </summary>        
        private void TimerStatusTick(object sender, EventArgs e)
        {
            if (!IsHandleCreated) return;
            SetStatusLabel(AccountStatus.Instance.connectionStatus);
            var lastQuoteTime = lastQuoteReceived.GetLastHitIfHitted();
            var milsSinceQuote = lastQuoteTime.HasValue
                                     ? (DateTime.Now - lastQuoteTime.Value).TotalMilliseconds
                                     : int.MaxValue;
            var hasQuoteString = milsSinceQuote <= MaxMillsBetweenQuotes;
            // если нет котировок...
            connectionStatusImage.HasQuotes = hasQuoteString;
            ChangeStatusLabelColorSafe(hasQuoteString ? colorStatusHasQuote : colorStatusNoQuote);
            // данные по балансу...
            var title = "TRADE#";
            var acc = AccountStatus.Instance.AccountData;
            if (acc != null)
            {
                title = string.Format("{0}{1}, {2}: {3} {4}", Localizer.GetString("TitleAccountNumberSmall"), acc.ID,
                                      Localizer.GetString("TitleFundsSmall"),
                                      acc.Equity.ToStringUniformMoneyFormat(false),
                                      acc.Currency);
                UpdateAccountLabel(title);
            }

            // тултип для иконки котировок и строки подключения
            var tooltipStatusStr = AccountStatus.Instance.isAuthorized
                                       ? Localizer.GetString("TitleConectedToServerSmall")
                                       : Localizer.GetString("TitleNotConnectedSmall");
            if (!hasQuoteString)
                tooltipStatusStr = tooltipStatusStr + ", " + Localizer.GetString("TitleNoQuotes");
            toolTipStatus.SetToolTip(connectionStatusImage, tooltipStatusStr);
            toolTipStatus.SetToolTip(lblConnectStatus, tooltipStatusStr);

            // пинг
            PingServerAsynch();

            // балун на иконке в трее
            if (notifyIcon.Visible)
            {
                if (acc != null)
                    title = title + Environment.NewLine + string.Format("{0}: {1} {2}",
                                                                        Localizer.GetString("TitleProfitSmall"),
                                                                        (acc.Equity - acc.Balance).
                                                                            ToStringUniformMoneyFormat(),
                                                                        acc.Currency);
                notifyIcon.BalloonTipText = title;
                notifyIcon.Text = title;
            }
        }

        private void PingServerAsynch()
        {
            var timeNow = DateTime.Now;
            var deltaSec = (timeNow - lastTimePinged).TotalSeconds;
            if (deltaSec < PingIntervalSeconds) return;
            try
            {
                ThreadPool.QueueUserWorkItem(PingServerSynch);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в QueueUserWorkItem(PingServerSynch)", ex);
            }
            lastTimePinged = timeNow;
        }

        private void PingServerSynch(object o)
        {
            try
            {
                if (!AccountStatus.Instance.isAuthorized ||
                    string.IsNullOrEmpty(UserSettings.Instance.Login)) return;

                var ctx = CurrentProtectedContext.Instance.MakeProtectedContext();
                if (ctx == null) return;
                serverProxyTrade.proxy.ReviveChannel(ctx, UserSettings.Instance.Login, AccountStatus.Instance.accountID, terminalVersion);                
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgPingError, 1000 * 60 * 30, "Ошибка в proxy.Ping(): {0}", ex);
                throw;
            }
        }

        private void SetStatusLabel(AccountConnectionStatus status)
        {
            connectionStatusImage.HasServer = status == AccountConnectionStatus.Connected;
            Invoke(new Action<AccountConnectionStatus>(SetStatusLabelUnsafe), status);
        }

        private void SetStatusLabelUnsafe(AccountConnectionStatus status)
        {
            lblConnectStatus.Text = EnumFriendlyName<AccountConnectionStatus>.GetString(status);
        }

        private void UpdateAccountLabel(string text)
        {
            Invoke(new Action<string>(UpdateAccountLabelUnsafe), text);
        }

        private void UpdateAccountLabelUnsafe(string text)
        {
            lblAccountData.Text = text;
        }

        private void ChangeStatusLabelColorSafe(Color cl)
        {
            Invoke(new Action<Color>(ChangeStatusLabelColorUnsafe), cl);
        }

        private void ChangeStatusLabelColorUnsafe(Color cl)
        {
            if (lblConnectStatus.LinkColor != cl)
                lblConnectStatus.LinkColor = cl;
        }

        private void OnAuthenticated()
        {
            AccountStatus.Instance.isAuthorized = true;
            // проиграть звук - авторизовался
            EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.LoggedIn);
            // авторизоваться в чате
            EnterChat();
            // уведомить об авторизации окно с подписками
            SubscriptionModel.Instance.LoadSubscribedCategories();
            // авторизоваться на сайте
            if (BrowserForm.Instance != null)
                BrowserForm.Instance.OnUserAuthenticated();
        }

        private void LoadNewsFromServer(int accountId)
        {
            NewsCache.Instance.ActualizeAsync(accountId);
        }

        //private static List<News> GetNewsFromProxy(int accountId, DateTime timeFrom)
        //{
        //    const int daysInRequest = 25;
        //    const int minDaysInRequest = 3;
        //    const int maximumFaults = 4;

        //    var wholeList = new List<News>();
            
        //    var dateLast = DateTime.Now;
        //    int numFaultsLeft = maximumFaults;
        //    for (var dateStart = timeFrom; dateStart < dateLast; )
        //    {
        //        var dateEnd = dateStart.AddDays(daysInRequest);
        //        if ((dateLast - dateEnd).TotalDays < minDaysInRequest) dateEnd = dateLast;
        //        try
        //        {
        //            var news = newsProxy.GetNews(accountId, dateStart, dateEnd);
        //            wholeList.AddRange(news);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error("GetNewsFromProxy: ошибка получения новостей", ex);
        //            numFaultsLeft--;
        //            if (numFaultsLeft == 0) break;
        //        }
        //        dateStart = dateEnd;
        //    }
        //    return wholeList;
        //}

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void ПараметрыToolStripMenuItemClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            var dlg = new OptionsForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var chart in Charts)
                {
                    chart.chart.EnableExtendedVisualStyles = UserSettings.Instance.EnableExtendedThemes;
                }
            }
        }

        private void MenuItemQuoteQrchiveClick(object sender, EventArgs e)
        {
            var dlg = new QuoteBaseForm();
            dlg.ShowDialog();
        }

        private void RobotTesterToolStripMenuItemClick(object sender, EventArgs e)
        {
            EnsureRoboTesterForm();
        }

        /// <summary>
        /// tab name, symbol, timeframe
        /// </summary>
        /// <returns></returns>
        public List<ChartWindowSettings> GetChartSymbolTimeframeList()
        {
            return Charts.Select(c => new ChartWindowSettings
                {
                    Symbol = c.chart.Symbol,
                    TabPageId = c.bookmarkId,
                    Timeframe = c.chart.timeframeString
                }).ToList();
        }

        public List<CandleChartControl> GetChartList(bool visibleOnly)
        {
            return visibleOnly
                       ? Charts.Where(c => c.Visible).Select(c => c.chart).ToList()
                       : Charts.Select(c => c.chart).ToList();
        }
        
        private void MenuSaveWorkspaceClick(object sender, EventArgs e)
        {
            if (dlgWorkspaceSave.ShowDialog() != DialogResult.OK) return;
            // сохранить настройки в локальные файлы
            SaveCurrentSettings();
            // упаковать в файл все настройки - основные, индикаторы, объекты
            if (!SettingsAutosaver.PackAndSaveSettingsFiles(ExecutablePath.ExecPath, dlgWorkspaceSave.FileName, true))
            {
                MessageBox.Show("Возникла ошибка при сохранении конфигурации!", "Предупреждение");
                return;
            }
            // обновить список последних вокспейсов
            UpdateLastWorkspaceMenu(dlgWorkspaceSave.FileName);
        }

        private void MenuLoadWorkspaceClick(object sender, EventArgs e)
        {
            if (dlgWorkspaceLoad.ShowDialog() != DialogResult.OK) return;
            LoadWorkspaceFromFile(dlgWorkspaceLoad.FileName);
        }

        private void LoadWorkspaceFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;
            // распаковать файл, переписав текущие настройки
            var folder = ExecutablePath.ExecPath;
            CompressionHelper.UnzipFile(filePath, folder);
            // применить настройки
            UserSettings.Instance.LoadProperties();
            var shouldCloseCharts = UserSettings.Instance.CloseChartPrompt;
            UserSettings.Instance.CloseChartPrompt = false;
            // закрыть все окошки...
            foreach (var child in MdiChildren) child.Close();
            UserSettings.Instance.CloseChartPrompt = shouldCloseCharts;
            // и загрузить рабочее пространство
            LoadWorkspace(true);
            // обновить список последних вокспейсов
            UpdateLastWorkspaceMenu(filePath);
        }

        private void ExecuteCommonButtonsCommand(SystemToolButton btn, out bool newBtnState)
        {
            var pressedBtn = GetPressedCommonButton(btn);
            CheckBox checkBox = null;
            if (pressedBtn != null)
                checkBox = pressedBtn as CheckBox;
            // стандартное поведение терминальной кнопки - сразу отпуститься
            newBtnState = false;

            switch (btn)
            {
                case SystemToolButton.RobotsStart:
                    StartStopRobotFarm();
                    if (checkBox == null)
                        return;
                    newBtnState = checkBox.Checked;
                    break;
                case SystemToolButton.Indicators:
                case SystemToolButton.ZoomIn:
                case SystemToolButton.ZoomOut:
                    if (ActiveMdiChild == null || !(ActiveMdiChild is ChartForm))
                        return;
                    var chart = (ChartForm) ActiveMdiChild;
                    switch (btn)
                    {
                        case SystemToolButton.Indicators:
                            chart.chart.MenuitemIndicatorsDlgClick(null, null);
                            break;
                        case SystemToolButton.ZoomIn:
                            chart.chart.ZoomIn();
                            break;
                        case SystemToolButton.ZoomOut:
                            chart.chart.ZoomOut();
                            break;
                    }
                    break;
                case SystemToolButton.Account:
                    MenuWindowAccountClick(this, null);
                    break;
                case SystemToolButton.SetOrder:
                    MenuItemNewOrderClick(null, null);
                    break;
                case SystemToolButton.RollGraph:
                    if (checkBox == null)
                        return;
                    if (ActiveMdiChild == null || ActiveMdiChild is ChartForm == false)
                        return;
                    chart = (ChartForm) ActiveMdiChild;
                    newBtnState = checkBox.Checked;
                    chart.chart.chart.StockSeries.AutoScroll = newBtnState;
                    break;
                case SystemToolButton.RollAllGraphs:
                    if (checkBox == null)
                        return;
                    newBtnState = checkBox.Checked;
                    foreach (var ch in Charts)
                        ch.chart.chart.StockSeries.AutoScroll = newBtnState;
                    break;
                case SystemToolButton.Forecast:
                    if (ActiveMdiChild == null || !(ActiveMdiChild is ChartForm))
                        return;
                    chart = (ChartForm) ActiveMdiChild;
                    chart.chart.PublishForecast();
                    break;

                case SystemToolButton.MakeSignalChart:
                    if (ActiveMdiChild == null || !(ActiveMdiChild is ChartForm))
                        return;
                    chart = (ChartForm) ActiveMdiChild;
                    OnPublishTradeSignal(chart.chart);                    
                    break;

                case SystemToolButton.MakeSignalText:
                    if (ActiveMdiChild == null || !(ActiveMdiChild is ChartForm))
                        return;
                    chart = (ChartForm) ActiveMdiChild;
                    OnMakeNewSignalMessage(chart.chart);                    
                    break;
                case SystemToolButton.ChatWindow:
                    EnsureChatWindow();
                    break;
                case SystemToolButton.RobotPortfolio:
                    if (menuitemRobotPortfolio.Enabled)
                        MenuItemRobotsSetupClick(this, new EventArgs());
                    break;
                case SystemToolButton.RobotState:
                    if (menuitemRobotState.Enabled)
                        MenuitemRobotStateClick(this, new EventArgs());
                    break;
                case SystemToolButton.TradeSignals:
                    EnsureSubscriptionForm(SubscriptionControl.ActivePage.Subscription);
                    break;
            }
        }

        private void MenuItemRobotsSetupClick(object sender, EventArgs e)
        {
            ShowRobotPortfolioDialog();
        }

        public void ShowRobotPortfolioDialog(string robotUniqueName = "")
        {
            // настроить роботов
            if (RobotFarm.State == RobotFarm.RobotFarmState.Stopped)
            {
                var dlg =
                    string.IsNullOrEmpty(robotUniqueName)
                        ? new RobotPortfolioForm(robotFarm)
                        : new RobotPortfolioForm(robotFarm, robotUniqueName);
                dlg.ShowDialog();
                return;
            }

            // посмотреть настройки
            ShowRobotStateDialog(robotUniqueName);
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void MenuItemAccountStatClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            EnsureStatisticsWindow();
        }

        private void ЖурналыToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new LogsForm {MdiParent = this};
            dlg.Show();
        }

        private void MainFormHelpRequested(object sender, HelpEventArgs hlpevent)
        {
            HelpManager.Instance.ShowHelp(sender);
        }

        public void ProcessApplicationMessageRegisteredKey(ApplicationMessageBinding objectBinding)
        {
            bool newBtnState;
            switch (objectBinding.Message)
            {
                case ApplicationMessage.SetupIndicators:
                    ExecuteCommonButtonsCommand(SystemToolButton.Indicators, out newBtnState);
                    break;
                case ApplicationMessage.ChartSnapshot:
                    if (lastActiveChart != null)
                        lastActiveChart.chart.SaveAsImageCurrentFolder();
                    break;
                case ApplicationMessage.NewOrder:
                    MenuItemNewOrderClick(null, null);
                    break;
                case ApplicationMessage.Quotes:
                    MenuWindowQuoteClick(null, null);
                    break;
                case ApplicationMessage.Chat:
                    EnsureChatWindow();
                    break;
                case ApplicationMessage.Statistics:
                    EnsureStatisticsWindow();
                    break;
                case ApplicationMessage.RobotSetup:
                    break;
            }
        }

        /// <summary>
        /// Если есть активные подписчики - значит сейчас открыта форма смены горячих клавиш или что то ей подобное.
        /// В этом случае в этом случае вызывать действия,привязанные к горячим клавишам не нужно.
        /// </summary>
        /// <param name="key">нажатая клавиша</param>
        public bool CheckLockMessage(Keys key)
        {
            if (KeyPressedEvent != null)
            {
                KeyPressedEvent(key);
                return true;
            }
            return false;
        }


        private void BtnLoadConfigClick(object sender, EventArgs e)
        {
            MenuLoadWorkspaceClick(sender, e);
        }

        private void BtnSaveConfigClick(object sender, EventArgs e)
        {
            SaveCurrentSettings();
            MenuSaveWorkspaceClick(sender, e);
        }

        private void BtnPrintClick(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null) return;
            if (ActiveMdiChild is ChartForm == false) return;
            var chart = (ChartForm)ActiveMdiChild;

            chart.chart.SaveAsImage("print.png", ImageFormat.Png);
            //chart.chart.SaveAsImage("print.png", ImageFormat.Png, (int)printDocument.DefaultPageSettings.PrintableArea.Height,
              //  (int)printDocument.DefaultPageSettings.PrintableArea.Width);
            
            //printDocument.DocumentName = chart.Text;
            
            //var dlg = new PrintPreviewDialog {Document = printDocument, Size = chart.Size};
            //dlg.ShowDialog();
            Process.Start("print.png");
        }
        
        // ReSharper disable MemberCanBeMadeStatic.Local
        private void PrintDocumentPrintPage(object sender, PrintPageEventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            e.Graphics.DrawImage(Image.FromFile("print.png"), 0, 0);
            // Check to see if more pages are to be printed.
            e.HasMorePages = false;
        }
        
        private void MenuitemLogoutClick(object sender, EventArgs e)
        {
            AccountStatus.Instance.isAuthorized = false;
            AccountStatus.Instance.isAccountSelected = false;
            AccountStatus.Instance.connectionStatus = AccountConnectionStatus.NotConnected;
            AccountStatus.Instance.AccountData = null;
            SetStatusLabel(AccountStatus.Instance.connectionStatus);
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void MenuitemSelectAccountClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            if (!AccountStatus.Instance.isAuthorized) return;
            var dlg = new SelectAccountForm();
            dlg.ShowDialog();
            HistoryOrderStorage.Instance.HurryUpUpdate();
        }

        private void ОПрограммеToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new AboutDlg {NameProgram = Text};
            dlg.ShowDialog();
        }

        private void BtnFlipPanelChartToolsClick(object sender, EventArgs e)
        {
            const int minPanelWd = 150;
            const int minToolsPanelWd = 88;
            const int panelRightPadding = 4;
            const int panelMargin = 6;
            const int buttonImageShrink = 22;
            const int buttonImageExpand = 21;

            var panel = ((Control) sender).Parent;
            var minWd = panel == panelChartTools ? minToolsPanelWd : minPanelWd;
            
            // свернуть - развернуть панель и поменять иконку кнопке
            var isShrunk = panel.Width == minWd;
            if (isShrunk)
            {
                var fullWd = panel.Controls.Cast<Control>().Max(c => c.Right) + panelRightPadding;
                panel.Width = fullWd;
                ((Button) sender).ImageIndex = buttonImageExpand;
            }
            else
            {
                panel.Width = minWd;
                ((Button)sender).ImageIndex = buttonImageShrink;
            }

            // выстроить панели
            if (panel == panelChartTools)
                panelCommonTools.Left = panelChartTools.Right + panelMargin;
        }

        private void MenuitemToolPanelClick(object sender, EventArgs e)
        {
            if (new ChartToolsSetupForm(lstGlyph32).ShowDialog() != DialogResult.OK)
                return;
            // настроить инструментальную панель
            buttonMenus.Clear();
            InitializeToolButtons(ChartToolButtonStorage.Instance.selButtons, panelChartTools);
            InitializeToolButtons(ChartToolButtonStorage.Instance.selSystemButtons, panelCommonTools);
            // настраиваем ширину панелей
            BtnFlipPanelChartToolsClick(btnFlipPanelChartTools, null);
            BtnFlipPanelChartToolsClick(btnFlipPanelChartTools, null);
            BtnFlipPanelChartToolsClick(btnFlipPanelCommonTools, null);
            BtnFlipPanelChartToolsClick(btnFlipPanelCommonTools, null);
        }

        private void MenuitemManageAccountsClick(object sender, EventArgs e)
        {
            var dlg = new ManageAccountForm();
            dlg.ShowDialog();
        }

        public void ShowMsgWindowSafe(AccountEvent accountEvent)
        {
            var updateAction = UserSettings.Instance.GetAccountEventAction(accountEvent.AccountEventCode);
            if (updateAction == AccountEventAction.DoNothing)
                return;

            if (!AccountStatus.Instance.isAuthorized)
                return;
            if (InvokeRequired)
                BeginInvoke(new Action<IWin32Window, AccountEvent>(ShowMsgWindow), this, accountEvent);
            else
                ShowMsgWindow(this, accountEvent);
        }

        private void ShowMsgWindow(IWin32Window owner, AccountEvent accountEvent)
        {
            foreach (var form in OwnedForms.OfType<SplashMessagesForm>())
            {
                form.AppendMessage(accountEvent);
                return;
            }
            var wnd = new SplashMessagesForm(accountEvent);
            wnd.Show(owner);
        }       

        public void ShowMsgWindowSafe(List<AccountEvent> msgList)
        {
            if (!AccountStatus.Instance.isAuthorized) return;

            msgList = msgList.Where(m => UserSettings.Instance.GetAccountEventAction(m.AccountEventCode) != AccountEventAction.DoNothing).ToList();
            if (msgList.Count == 0) return;

            if (InvokeRequired)
                BeginInvoke(new Action<IWin32Window, List<AccountEvent>>(ShowListMsgWindow), this, msgList);
            else
                ShowListMsgWindow(this, msgList);
        }

        private void ShowListMsgWindow(IWin32Window owner, List<AccountEvent> msg)
        {
            foreach (var form in OwnedForms.OfType<SplashMessagesForm>())
            {
                form.EnqueueMessages(msg);
                return;
            }
            var wnd = new SplashMessagesForm(msg);
            wnd.Show(owner);
        }
        
        // ReSharper disable MemberCanBeMadeStatic.Local
        private void MenuitemTradeSignalsClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            EnsureSubscriptionForm(SubscriptionControl.ActivePage.Subscription);
        }

        public void ShowSubscriptionDialogSafe(SubscriptionControl.ActivePage page)
        {
            Invoke(new Action<SubscriptionControl.ActivePage>(EnsureSubscriptionForm), page);
        }
        
        private void BtnChatClick(object sender, EventArgs e)        
        {
            EnsureChatWindow();
        }

        private void MenuitemScriptsClick(object sender, EventArgs e)
        {
            if (!menuitemScripts.HasDropDownItems)
            new ChartToolsSetupForm(lstGlyph32, ChartToolsSetupForm.ChartToolsFormTab.Scripts).ShowDialog();
        }

        private void MenuitemShowHelpClick(object sender, EventArgs e)
        {
            HelpManager.Instance.ShowHelp(this);
        }

        private static void ShowHelp(object sender, string topicId = null)
        {
            var helpPath = ExecutablePath.ExecPath + TerminalEnvironment.HelpFile;
            if (!File.Exists(helpPath)) return;
            if (string.IsNullOrEmpty(topicId))
                Help.ShowHelp(sender is Control ? (Control)sender : Instance, "file://" + helpPath);
            else
                Help.ShowHelp(sender is Control ? (Control)sender : Instance, "file://" + helpPath, HelpNavigator.TopicId, topicId);
        }        

        public void AccountDataUpdated(Account account)
        {
            AccountStatus.Instance.OnAccountUpdated(account);
        }

        public void ReopenChartsSafe(string ticker)
        {
            Invoke(new Action<string>(ReopenCharts), ticker);
        }

        public void ReopenCharts(string ticker)
        {
            foreach (var chart in Charts.Where(c => c.chart.Symbol == ticker))
            {
                chart.LoadQuotesAndBuildChart(false);
            }
        }

        private void MenuitemPrintClick(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null) return;
            if (ActiveMdiChild is ChartForm == false) return;
            var chart = (ChartForm)ActiveMdiChild;

            var printDoc = new PrintDocument();
            printDoc.PrintPage += (o, args) =>
            {
                var ulCorner = new Point(0, 0);
                using (var img = chart.chart.MakeImage())
                {
                    args.Graphics.DrawImage(img, ulCorner);
                }
            };

            var printDialog = new PrintDialog { Document = printDoc };
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        /// <summary>
        /// обновить отображаемые на графиках иконки-кнопки
        /// (их набор хранится в UserSettings)
        /// </summary>
        public void UpdateChartIconSet()
        {
            var iconSet = UserSettings.Instance.SelectedChartIcons;
            var charts = Charts.ToList();
            foreach (var chart in charts)
            {
                chart.chart.ChooseChartIconsToShow(iconSet);
            }
        }

        private void MenuitemRiskSetupClick(object sender, EventArgs e)
        {
            EnsureRiskSetupForm();
        }

        private void MenuitemHelpTooltipClick(object sender, EventArgs e)
        {
            ShowTooltip(true);
        }

        private void ShowTooltip(bool forceShow = false)
        {
            if (forceShow)
                if (UserSettings.Instance.TooltipIndex == 0)
                    UserSettings.Instance.TooltipIndex = 1;
            if (UserSettings.Instance.TooltipIndex == 0) return;
            const string tooltipLibFileName = "TradeSharp.Client.Tooltip.dll";
            if (!File.Exists(tooltipLibFileName)) return;

            BeginInvoke(new Action(() =>
                {
                    var existDlg = Application.OpenForms.OfType<TooltipForm>().FirstOrDefault();
                    if (existDlg != null)
                    {
                        existDlg.WindowState = FormWindowState.Normal;
                        existDlg.Focus();
                        return;
                    }

                    var dlg = new TooltipForm(UserSettings.Instance.TooltipIndex, tooltipLibFileName,
                        i => { UserSettings.Instance.TooltipIndex = i; });
                    dlg.Show();                    
                }), null);
        }

        private void LblConnectStatusLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLoginDialog();
        }

        private void BtnShowTradeSignalsClick(object sender, EventArgs e)
        {
            EnsureSubscriptionForm(SubscriptionControl.ActivePage.Subscription);
        }

        private void MenuitemWalletClick(object sender, EventArgs e)
        {
            EnsureWalletWindow();
        }

        private void WebBrowserToolStripMenuItemClick(object sender, EventArgs e)
        {
            EnsureWebBrowseWindow();
        }
    
        public void OpenInvestInPAMMDialog(PerformerStat stat, bool invest)
        {
            EnsureWalletWindow();
            var walletForm = MdiChildren.FirstOrDefault(f => f is WalletForm);
            if (walletForm == null) return;
            ((WalletForm)walletForm).OpenInvestInPAMMDialog(stat, invest);
        }
    }
    // ReSharper restore ParameterTypeCanBeEnumerable.Local
}