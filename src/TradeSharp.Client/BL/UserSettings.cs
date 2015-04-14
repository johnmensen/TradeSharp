using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Candlechart.ChartIcon;
using Entity;
using TradeSharp.Client.BL.Sound;
using TradeSharp.Client.Controls.Bookmark;
using TradeSharp.Client.Controls.NavPanel;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.QuoteHistory;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    public partial class UserSettings : ILoginUserSettings
    {
        public readonly ThreadSafeTimeStamp lastTimeModified;

        private readonly SymCipher cipher = new SymCipher(Encoding.Unicode, typeof(UserSettings).ToString());        
        
        private static UserSettings instance;
        public static UserSettings Instance
        {
            get { return instance ?? (instance = new UserSettings()); }
        }

        private long terminalId;
        public long TerminalId
        {
            get
            {
                if (terminalId != 0)
                    return terminalId;
                return terminalId = TerminalIdGenerator.MakeTerminalId();
            }
        }

        [PropertyXMLTag("FirstStarted")]
        public bool FirstStarted { get; set; }

        [PropertyXMLTag("TimeClosed")]
        public DateTime TimeClosed { get; set; }
        
        private Size windowSize;
        [PropertyXMLTag("Window.Size")]
        public Size WindowSize
        {
            get { return windowSize; }
            set
            {
                windowSize = value; 
                lastTimeModified.Touch();
            }
        }

        private Point windowPos;
        [PropertyXMLTag("Window.Pos")]
        public Point WindowPos
        {
            get { return windowPos; }
            set
            {
                windowPos = value; 
                lastTimeModified.Touch();
            }
        }

        private int statusSplitterPositionRel = 60;
        [PropertyXMLTag("StatusSplitterPositionRel")]
        public int StatusSplitterPositionRel
        {
            get { return statusSplitterPositionRel; }
            set
            {
                statusSplitterPositionRel = value;
                lastTimeModified.Touch();
            }
        }

        private bool hideToTray;
        [PropertyXMLTag("HideToTray")]
        public bool HideToTray
        {
            get { return hideToTray; }
            set
            {
                hideToTray = value;
                lastTimeModified.Touch();
            }
        }

        private int statusBarHeight = 30;
        [PropertyXMLTag("StatusBar.Height")]
        public int StatusBarHeight
        {
            get { return statusBarHeight; }
            set
            {
                statusBarHeight = value;
                lastTimeModified.Touch();
            }
        }

        private bool useTestPeriod;
        [PropertyXMLTag("Robots.UseTestPeriod")]
        public bool UseTestPeriod
        {
            get { return useTestPeriod; }
            set
            {
                useTestPeriod = value;
                lastTimeModified.Touch();
            }
        }

        private bool saveLog;
        [PropertyXMLTag("Robots.SaveLog")]
        public bool SaveLog
        {
            get { return saveLog; }
            set { saveLog = value; lastTimeModified.Touch(); }
        }

        private DateTime testPeriodFrom;
        [PropertyXMLTag("Robots.TestPeriodFrom")]
        public DateTime TestPeriodFrom
        {
            get { return testPeriodFrom; }
            set { testPeriodFrom = value; lastTimeModified.Touch(); }
        }

        private DateTime testPeriodTo;
        [PropertyXMLTag("Robots.TestPeriodTo")]
        public DateTime TestPeriodTo
        {
            get { return testPeriodTo; }
            set { testPeriodTo = value; lastTimeModified.Touch(); }
        }

        private bool accountResultsShowEquityCurve;
        [PropertyXMLTag("ShowEquityCurve")]
        public bool AccountResultsShowEquityCurve
        {
            get { return accountResultsShowEquityCurve; }
            set { accountResultsShowEquityCurve = value; lastTimeModified.Touch(); }
        }

        private bool accountResultsUpdateQuotes = true;
        [PropertyXMLTag("AccountResultsUpdateQuotes")]
        public bool AccountResultsUpdateQuotes
        {
            get { return accountResultsUpdateQuotes; }
            set { accountResultsUpdateQuotes = value; lastTimeModified.Touch(); }
        }

        private List<ChartWindowSettings> chartSetsList = new List<ChartWindowSettings>();
        [PropertyXMLTag("Chart")]
        public List<ChartWindowSettings> ChartSetsList
        {
            get
            {
                return chartSetsList;
            }
            set
            {
                chartSetsList = value;
                lastTimeModified.Touch();
            }
        }

        private List<NonChartWindowSettings> windowSetsList = new List<NonChartWindowSettings>();
        [PropertyXMLTag("Windows")]
        public List<NonChartWindowSettings> WindowSetsList
        {
            get
            {
                return windowSetsList;
            }
            set
            {
                windowSetsList = value;
                lastTimeModified.Touch();
            }
        }

        private List<TerminalBookmark> tabPages = new List<TerminalBookmark>();
        [PropertyXMLTag("Tabs")]
        public List<TerminalBookmark> TabPages
        {
            get { return tabPages; }
            set
            {
                tabPages = value;
                lastTimeModified.Touch();
            }
        }
        
        private List<VocalizedEventFileName> vocalEvents = new List<VocalizedEventFileName>();
        [PropertyXMLTag("VocalEvents")]
        public List<VocalizedEventFileName> VocalEvents
        {
            get { return vocalEvents; }
            set { vocalEvents = value; lastTimeModified.Touch(); }
        }

        private bool deleteAccountEventsOnRead = false;
        [PropertyXMLTag("DeleteAccountEventsOnRead")]
        public bool DeleteAccountEventsOnRead
        {
            get { return deleteAccountEventsOnRead; }
            set { deleteAccountEventsOnRead = value; lastTimeModified.Touch(); }
        }

        private List<AccountEventSettings> accountEventAction = new List<AccountEventSettings>();
        [PropertyXMLTag("AccountEvents")]
        public List<AccountEventSettings> AccountEventAction
        {
            get { return accountEventAction; }
            set { accountEventAction = value; lastTimeModified.Touch(); }
        }

        private bool mute;
        [PropertyXMLTag("Mute")]
        public bool Mute
        {
            get { return mute; }
            set
            {
                mute = value;
                lastTimeModified.Touch();
            }
        }

        private List<string> timeframeList = new List<string>();
        [PropertyXMLTag("TimeframeList")]
        public List<string> TimeframeList
        {
            get { return timeframeList; }
            set { timeframeList = value; lastTimeModified.Touch(); }
        }

        private string chartIconSet = ChartIcon.chartButtonIndexAutoScroll + ";" +
                                      ChartIcon.chartButtonIndicators + ";" + ChartIcon.chartButtonNewOrder;
        [PropertyXMLTag("ChartIconSet")]
        public string ChartIconSet
        {
            get { return chartIconSet; }
            set
            {
                chartIconSet = value;
                lastTimeModified.Touch();
            }
        }

        public string[] SelectedChartIcons
        {
            get { return (chartIconSet ?? "").Split(';'); }
            set { chartIconSet = string.Join(";", value); }
        }

        private string hotKeyList = string.Empty;
        [PropertyXMLTag("HotKeyList")]
        public string HotKeyList
        {
            get
            {
                return  hotKeyList;
            }
            set
            {
                hotKeyList = value;
                lastTimeModified.Touch();
            }
        }

        private string fastDealVolumesStr = "10000,20000,30000,50000,100000,200000,500000,1000000";
        [PropertyXMLTag("FastDealVolumes")]
        public string FastDealVolumesStr
        {
            get { return fastDealVolumesStr; }
            set { fastDealVolumesStr = value; }
        }

        public int[] FastDealVolumes
        {
            get { return (fastDealVolumesStr ?? "10000").ToIntArrayUniform(); }
            set
            {
                if (value == null || value.Length == 0) return;
                fastDealVolumesStr = string.Join(",", value);
            }
        }

        [PropertyXMLTag("VolumeByTickers")]
        public string VolumeByTickers { get; set; }

        /// <summary>
        /// Возможные значения для выпадающих списков, в контролах быстрой торговли, которые добавил пользователь
        /// </summary>    
        public Dictionary<string, int[]> VolumeByTicker
        {
            get
            {
                try
                {   
                    return string.IsNullOrEmpty(VolumeByTickers) ? new Dictionary<string, int[]>() : 
                        VolumeByTickers.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).
                                        ToDictionary(x => x.Split(':')[0], 
                                                     x => Array.ConvertAll(x.Split(':')[1].Split(','), int.Parse));
                }
                catch
                {
                    return new Dictionary<string, int[]>();
                }
            }
            set
            {
                if (value == null) return;
                VolumeByTickers = string.Join(";", value.Select(x => x.Key + ":" + string.Join(",", x.Value)).ToArray());
                lastTimeModified.Touch();
            }
        }

        private string fastDealSelectVolumeStr;
        [PropertyXMLTag("FastDealSelectVolume")]
        public string FastDealSelectVolumeStr
        {
            get { return fastDealSelectVolumeStr; }
            set
            {
                fastDealSelectVolumeStr = value;
                //lastTimeModified.Touch();
            }
        }

        /// <summary>
        /// выбранные торговые объёмы для валютных катеровок в контроле для быстрой торговли
        /// </summary>
        public Dictionary<string, int> FastDealSelectedVolumeDict
        {
            get
            {
                try
                {
                    return string.IsNullOrEmpty(FastDealSelectVolumeStr)
                           ? new Dictionary<string, int>()
                           : FastDealSelectVolumeStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(
                            pairs => pairs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).Where(
                            p => p.Length == 2).ToDictionary(p => p[0], p => p[1].ToIntSafe() ?? 10000);
                }
                catch
                {
                    return new Dictionary<string, int>();
                }
            }
            set
            {
                FastDealSelectVolumeStr = string.Join(";", value.Select(x => x.Key + "," + x.Value).ToArray());
                lastTimeModified.Touch();
            }
        }
        
        private const int MaxWorkspacesInList = 6;

        private List<string> lastWorkspaces = new List<string>();
        [PropertyXMLTag("LastWorkspaces")]
        public List<string> LastWorkspaces
        {
            get { return lastWorkspaces; }
            set
            {
                lastWorkspaces = value;
                lastTimeModified.Touch();
            }
        }

        private List<string> visitedUrls = new List<string>();
        [PropertyXMLTag("VisitedUrls")]
        public List<string> VisitedUrls
        {
            get { return visitedUrls; }
            set { visitedUrls = value; lastTimeModified.Touch(); }
        }

        /// <summary>
        /// обновить список последних N workspace, вернуть признак того, что список обновился
        /// </summary>        
        public bool AddWorkspace(string filePath)
        {
            filePath = filePath.ToLower();
            
            if (lastWorkspaces.Count > MaxWorkspacesInList)
                lastWorkspaces.RemoveAt(lastWorkspaces.Count - 1);

            var index = lastWorkspaces.IndexOf(filePath);
            if (index == 0) return false;

            if (index > 0)
            {
                // поменять элементы местами
                var names = new List<string> {filePath};
                names.AddRange(lastWorkspaces.Where(w => w != filePath));
                lastTimeModified.Touch();
                return true;
            }

            // дописать в начало
            lastWorkspaces.Insert(0, filePath);
            if (lastWorkspaces.Count > MaxWorkspacesInList)
                lastWorkspaces.RemoveAt(lastWorkspaces.Count - 1);
            lastTimeModified.Touch();
            return true;
        }

        public void VerifyWorkspaceListFilesAreExist()
        {
            try
            {
                lastWorkspaces = lastWorkspaces.Where(File.Exists).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in VerifyWorkspaceListFilesAreExist", ex);
            }
        }

        /// <summary>
        /// берется из строки-списка LastAccounts
        /// </summary>
        public string Account
        {
            get
            {
                if (string.IsNullOrEmpty(LastAccounts)) return "";
                var parts = LastAccounts.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0) return parts[0];
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                if (string.IsNullOrEmpty(LastAccounts))
                {
                    LastAccounts = value;
                    return;
                }
                var parts = LastAccounts.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var newList = new List<string> { value };
                for (var i = 0; i < parts.Length; i++)
                {
                    if (parts[i] != value) newList.Add(parts[i]);
                }
                if (newList.Count > 20) newList.RemoveAt(newList.Count - 1);
                var sb = new StringBuilder();
                foreach (var login in newList)
                    sb.AppendFormat("{0};", login);
                LastAccounts = sb.ToString();
                lastTimeModified.Touch();
            }
        }

        private string lastAccounts;
        [PropertyXMLTag("LastAccounts")]
        public string LastAccounts
        {
            get { return lastAccounts; }
            set
            {
                lastAccounts = value;
                lastTimeModified.Touch();
            }
        }

        private bool synchGraphics = true;
        [PropertyXMLTag("SynchGraphics")]
        public bool SynchGraphics
        {
            get { return synchGraphics; }
            set
            {
                synchGraphics = value; 
                lastTimeModified.Touch();
            }
        }

        private bool closeChartPrompt = false;
        [PropertyXMLTag("CloseChartPrompt")]
        public bool CloseChartPrompt
        {
            get { return closeChartPrompt; }
            set
            {
                closeChartPrompt = value; 
                lastTimeModified.Touch();
            }
        }

        private bool closeTerminalPrompt = true;
        [PropertyXMLTag("CloseTerminalPrompt")]
        public bool CloseTerminalPrompt
        {
            get { return closeTerminalPrompt; }
            set
            {
                closeTerminalPrompt = value; 
                lastTimeModified.Touch();
            }
        }

        private bool confirmGapFilling = true;
        [PropertyXMLTag("ConfirmGapFilling")]
        public bool ConfirmGapFilling
        {
            get { return confirmGapFilling; }
            set
            {
                confirmGapFilling = value;
                lastTimeModified.Touch();
            }
        }

        private bool showRobotMessageInYellowWindow = false;
        [PropertyXMLTag("ShowRobotMessageInYellowWindow")]
        public bool ShowRobotMessageInYellowWindow
        {
            get { return showRobotMessageInYellowWindow; }
            set
            {
                showRobotMessageInYellowWindow = value;
                lastTimeModified.Touch();
            }
        }

        private bool synchGraphicsAnyPair;
        [PropertyXMLTag("SynchGraphicsAnyPair")]
        public bool SynchGraphicsAnyPair
        {
            get { return synchGraphicsAnyPair; }
            set
            {
                synchGraphicsAnyPair = value; 
                lastTimeModified.Touch();
            }
        }

        private bool enableExtendedThemes;
        [PropertyXMLTag("EnableExtendedThemes")]
        public bool EnableExtendedThemes
        {
            get { return enableExtendedThemes; }
            set
            {
                enableExtendedThemes = value; 
                lastTimeModified.Touch();
            }
        }

        private List<GridSettings> gridList = new List<GridSettings>();
        [PropertyXMLTag("Grids")]
        public List<GridSettings> GridList
        {
            get { return gridList; }
            set
            {
                gridList = value;
                lastTimeModified.Touch();
            }
        }

        private bool hidePasswordChars;
        [PropertyXMLTag("HidePasswordChars")]
        public bool HidePasswordChars
        {
            get { return hidePasswordChars; }
            set
            {
                if (hidePasswordChars == value) return;
                hidePasswordChars = value; 
                lastTimeModified.Touch();
            }
        }

        private List<string> favoriteTickers = new List<string>();
        [PropertyXMLTag("FavoriteTickers")]
        public List<string> FavoriteTickers
        {
            get { return favoriteTickers; }
            set
            {
                favoriteTickers = value;
                lastTimeModified.Touch();
            }
        }

        private List<string> favoriteIndicators = new List<string>
                                                      {
                                                          "IndicatorOrders", "IndicatorMA", "IndicatorRSI", "IndicatorRandomWalk"
                                                      };
        [PropertyXMLTag("FavoriteIndicators")]
        public List<string> FavoriteIndicators
        {
            get { return favoriteIndicators; }
            set
            {
                favoriteIndicators = value;
                lastTimeModified.Touch();
            }
        }

        private bool navPanelIsVisible;
        [PropertyXMLTag("NavPanelIsVisible")]
        public bool NavPanelIsVisible
        {
            get { return navPanelIsVisible; }
            set 
            { 
                navPanelIsVisible = value;
                lastTimeModified.Touch();
            }
        }

        [PropertyXMLTag("NavPanelPages")]
        public string NavTablePageStr { get; set; }
        public string[] NavPanelPages
        {
            get
            {
                return string.IsNullOrEmpty(NavTablePageStr) ? 
                    NavPanelPageControl.pageCodes.ToArray() : NavTablePageStr.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                if (value != null && value.Length > 0)
                {
                    NavTablePageStr = string.Join(",", value);
                    lastTimeModified.Touch();
                }
            }
        }

        #region Настройки торговли

        private bool promptTradeFromQuoteWindow = true;
        [PropertyXMLTag("PromptTradeFromQuoteWindow")]
        public bool PromptTradeFromQuoteWindow
        {
            get { return promptTradeFromQuoteWindow; }
            set
            {
                promptTradeFromQuoteWindow = value;
                lastTimeModified.Touch();
            }
        }

        private bool promptFastButtonTrade = true;
        [PropertyXMLTag("PromptFastButtonTrade")]
        public bool PromptFastButtonTrade
        {
            get { return promptFastButtonTrade; }
            set
            {
                promptFastButtonTrade = value;
                lastTimeModified.Touch();
            }
        }

        private bool checkInstTradePrice = true;
        [PropertyXMLTag("CheckInstTradePrice")]
        public bool CheckInstTradePrice
        {
            get { return checkInstTradePrice; }
            set
            {
                checkInstTradePrice = value;
                lastTimeModified.Touch();
            }
        }
        #endregion

        #region Таблица котировок

        private List<QuoteTableCellSettings> tickerCellList = new List<QuoteTableCellSettings>();
        [PropertyXMLTag("TickerCellList")] 
        public List<QuoteTableCellSettings> TickerCellList
        {
            get { return tickerCellList; }
            set
            {
                tickerCellList = value;
                lastTimeModified.Touch();
            }
        }

        private int quoteCellSize = 50;
        [PropertyXMLTag("QuoteCellStyle")]
        public int QuoteCellSize
        {
            get { return quoteCellSize; }
            set
            {
                quoteCellSize = value;
                lastTimeModified.Touch();
            }
        }        

        #endregion

        [PropertyXMLTag("GapRecordActualMinutes")]
        public int GapRecordActualMinutes
        {
            get { return GapMap.Instance.gapRecordActualMinutes; }
            set
            {
                GapMap.Instance.gapRecordActualMinutes = value;
                lastTimeModified.Touch();
            }
        }

        private List<string> performersGridColumns = new List<string>();
        [PropertyXMLTag("PerformersGridColumns")]
        public List<string> PerformersGridColumns
        {
            get { return performersGridColumns; }
            set
            {
                performersGridColumns = value;
                lastTimeModified.Touch();
            }
        }

        private int tooltipIndex = 1;
        [PropertyXMLTag("TooltipIndex")]
        public int TooltipIndex
        {
            get { return tooltipIndex; }
            set
            {
                tooltipIndex = value;
                lastTimeModified.Touch();
            }
        }

        private ActionOnSignal actionOnSignal = ActionOnSignal.ПредложитьОткрытьГрафик;
        [PropertyXMLTag("ActionOnSignal")]
        public ActionOnSignal ActionOnSignal
        {
            get { return actionOnSignal; }
            set
            {
                actionOnSignal = value;
                lastTimeModified.Touch();
            }
        }

        public AccountEventAction GetAccountEventAction(AccountEventCode code)
        {
            var act = accountEventAction.FirstOrDefault(a => a.EventCode == code);
            if (act != null)
                return act.EventAction;
            return Contract.Entity.AccountEventAction.DefaultAction;
        }

        public void SwitchAccountEventAction(AccountEventCode accountEventCode, bool off = true)
        {
            var accountEventSettings = accountEventAction.FirstOrDefault(a => a.EventCode == accountEventCode);
            if (accountEventSettings != null)
                accountEventSettings.EventAction = off ? Contract.Entity.AccountEventAction.DoNothing : Contract.Entity.AccountEventAction.DefaultAction;
        }

        private UserSettings()
        {
            lastTimeModified = new ThreadSafeTimeStamp();            
        }

        public void LoadProperties()
        {
            // прочитать файл
            var fileName = ExecutablePath.ExecPath + TerminalEnvironment.UserSettingsFileName;
            if (!File.Exists(fileName)) return;
            var doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch
            {
                return;
            }
            // загрузить из документа
            PropertyXMLTagAttribute.InitObjectProperties(this, doc.DocumentElement);
            LoadLoginPassword(doc);
        }

        public void SaveSettings(string fileName)
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("settings"));
            PropertyXMLTagAttribute.SaveObjectProperties(this, doc.DocumentElement);
            SaveLoginPassword(doc);
            using (var sw = new StreamWriterLog(fileName, false, Encoding.Unicode))
            {
                using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    doc.Save(xw);
                }
            }
        }
    
        public void SaveSettings()
        {
            var fileName = ExecutablePath.ExecPath + TerminalEnvironment.UserSettingsFileName;
            SaveSettings(fileName);
        }
    }    
}
