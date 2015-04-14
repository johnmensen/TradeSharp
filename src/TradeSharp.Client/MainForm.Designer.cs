using System;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Client.Controls.Bookmark;

namespace TradeSharp.Client
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Timer timerStatus;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemWallet = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSelectAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuNewChart = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSaveWorkspace = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLoadWorkspace = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemManageAccounts = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuMinimizeInTray = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.видToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemViewAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemLiveQuotes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemNavPane = new System.Windows.Forms.ToolStripMenuItem();
            this.обозревательToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.торговляToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemNewOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAccountStat = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemJournals = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemTradeSignals = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemRobotsSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemRobotTester = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemRobotPortfolio = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemRobotState = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemScript = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemToolPanel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemQuoteArchive = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSettingsChart = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTemplates = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSaveTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuApplyTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuManageTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemCommonParams = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemRiskSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCascade = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMinimizeAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemStatusBar = new System.Windows.Forms.ToolStripMenuItem();
            this.menutopicHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemShowHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemShowAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemHelpTooltip = new System.Windows.Forms.ToolStripMenuItem();
            this.panelTools = new System.Windows.Forms.Panel();
            this.panelCommonTools = new System.Windows.Forms.Panel();
            this.btnFlipPanelCommonTools = new System.Windows.Forms.Button();
            this.lstGlyph32 = new System.Windows.Forms.ImageList(this.components);
            this.panelChartTools = new System.Windows.Forms.Panel();
            this.btnFlipPanelChartTools = new System.Windows.Forms.Button();
            this.toolBtnCross = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.lstGlyth = new System.Windows.Forms.ImageList(this.components);
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.btnLoadConfig = new System.Windows.Forms.Button();
            this.btnNewChart = new System.Windows.Forms.Button();
            this.cbTimeFrame = new System.Windows.Forms.ComboBox();
            this.panelStatus = new TradeSharp.UI.Util.Control.SizeablePanel();
            this.splitContainerStatus = new System.Windows.Forms.SplitContainer();
            this.tbStatus = new TradeSharp.Client.BL.RichTextBoxEx();
            this.panelStatusRight = new System.Windows.Forms.Panel();
            this.bookmarkStrip = new TradeSharp.Client.Controls.Bookmark.BookmarkStripControl();
            this.btnShowTradeSignals = new System.Windows.Forms.Button();
            this.btnChat = new System.Windows.Forms.Button();
            this.btnShrinkStatusPanel = new System.Windows.Forms.Button();
            this.btnExpandStatusPanel = new System.Windows.Forms.Button();
            this.panelConnect = new System.Windows.Forms.Panel();
            this.lblConnectStatus = new System.Windows.Forms.LinkLabel();
            this.lblAccountData = new System.Windows.Forms.Label();
            this.connectionStatusImage = new TradeSharp.Client.Controls.ConnectionStatusImageControl();
            this.dlgWorkspaceSave = new System.Windows.Forms.SaveFileDialog();
            this.dlgWorkspaceLoad = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.imgListMenu = new System.Windows.Forms.ImageList(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuTrayRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTrayQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTrayCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.panelNavi = new TradeSharp.UI.Util.Control.SizeablePanel();
            this.navPanelControl = new TradeSharp.Client.Controls.NavPanel.NavPanelControl();
            this.toolTipStatus = new System.Windows.Forms.ToolTip(this.components);
            timerStatus = new System.Windows.Forms.Timer(this.components);
            this.menuMain.SuspendLayout();
            this.panelTools.SuspendLayout();
            this.panelCommonTools.SuspendLayout();
            this.panelChartTools.SuspendLayout();
            this.panelStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStatus)).BeginInit();
            this.splitContainerStatus.Panel1.SuspendLayout();
            this.splitContainerStatus.Panel2.SuspendLayout();
            this.splitContainerStatus.SuspendLayout();
            this.panelConnect.SuspendLayout();
            this.panel1.SuspendLayout();
            this.contextMenuTray.SuspendLayout();
            this.panelNavi.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerStatus
            // 
            timerStatus.Enabled = true;
            timerStatus.Interval = 500;
            timerStatus.Tick += new System.EventHandler(this.TimerStatusTick);
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.видToolStripMenuItem,
            this.торговляToolStripMenuItem,
            this.menuSettings,
            this.menuWindows,
            this.menutopicHelp});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.MdiWindowListItem = this.menuWindows;
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(1247, 24);
            this.menuMain.TabIndex = 1;
            this.menuMain.Text = "menuStrip1";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemAccount,
            this.menuitemWallet,
            this.menuitemSelectAccount,
            this.menuitemLogout,
            this.toolStripSeparator3,
            this.menuNewChart,
            this.menuSave,
            this.menuSaveWorkspace,
            this.menuLoadWorkspace,
            this.menuitemManageAccounts,
            this.menuitemPrint,
            this.toolStripMenuItem1,
            this.menuMinimizeInTray,
            this.menuExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(45, 20);
            this.menuFile.Tag = "TitleFile";
            this.menuFile.Text = "Файл";
            // 
            // menuitemAccount
            // 
            this.menuitemAccount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuitemAccount.Name = "menuitemAccount";
            this.menuitemAccount.Size = new System.Drawing.Size(259, 22);
            this.menuitemAccount.Tag = "TitleLoginMenu";
            this.menuitemAccount.Text = "Логин...";
            this.menuitemAccount.Click += new System.EventHandler(this.MenuItemAccountClick);
            // 
            // menuitemWallet
            // 
            this.menuitemWallet.Name = "menuitemWallet";
            this.menuitemWallet.Size = new System.Drawing.Size(259, 22);
            this.menuitemWallet.Tag = "TitleWallet";
            this.menuitemWallet.Text = "Кошелек...";
            this.menuitemWallet.Click += new System.EventHandler(this.MenuitemWalletClick);
            // 
            // menuitemSelectAccount
            // 
            this.menuitemSelectAccount.Name = "menuitemSelectAccount";
            this.menuitemSelectAccount.Size = new System.Drawing.Size(259, 22);
            this.menuitemSelectAccount.Tag = "TitleChangeAccountMenu";
            this.menuitemSelectAccount.Text = "Сменить счет...";
            this.menuitemSelectAccount.Click += new System.EventHandler(this.MenuitemSelectAccountClick);
            // 
            // menuitemLogout
            // 
            this.menuitemLogout.Name = "menuitemLogout";
            this.menuitemLogout.Size = new System.Drawing.Size(259, 22);
            this.menuitemLogout.Tag = "TitleLogoutMenu";
            this.menuitemLogout.Text = "Логаут...";
            this.menuitemLogout.Click += new System.EventHandler(this.MenuitemLogoutClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(256, 6);
            // 
            // menuNewChart
            // 
            this.menuNewChart.Name = "menuNewChart";
            this.menuNewChart.Size = new System.Drawing.Size(259, 22);
            this.menuNewChart.Tag = "TitleNewMenu";
            this.menuNewChart.Text = "Новый";
            this.menuNewChart.Click += new System.EventHandler(this.MenuNewChartClick);
            // 
            // menuSave
            // 
            this.menuSave.Name = "menuSave";
            this.menuSave.Size = new System.Drawing.Size(259, 22);
            this.menuSave.Tag = "TitleSave";
            this.menuSave.Text = "Сохранить";
            this.menuSave.Click += new System.EventHandler(this.MenuSaveClick);
            // 
            // menuSaveWorkspace
            // 
            this.menuSaveWorkspace.Name = "menuSaveWorkspace";
            this.menuSaveWorkspace.Size = new System.Drawing.Size(259, 22);
            this.menuSaveWorkspace.Tag = "TitleSaveWorkspaceMenu";
            this.menuSaveWorkspace.Text = "Сохранить рабочее пространство...";
            this.menuSaveWorkspace.Click += new System.EventHandler(this.MenuSaveWorkspaceClick);
            // 
            // menuLoadWorkspace
            // 
            this.menuLoadWorkspace.Name = "menuLoadWorkspace";
            this.menuLoadWorkspace.Size = new System.Drawing.Size(259, 22);
            this.menuLoadWorkspace.Tag = "TitleLoadWorkspaceMenu";
            this.menuLoadWorkspace.Text = "Загрузить рабочее пространство...";
            this.menuLoadWorkspace.Click += new System.EventHandler(this.MenuLoadWorkspaceClick);
            // 
            // menuitemManageAccounts
            // 
            this.menuitemManageAccounts.Name = "menuitemManageAccounts";
            this.menuitemManageAccounts.Size = new System.Drawing.Size(259, 22);
            this.menuitemManageAccounts.Tag = "TitleAccountsMenu";
            this.menuitemManageAccounts.Text = "Счета...";
            this.menuitemManageAccounts.Click += new System.EventHandler(this.MenuitemManageAccountsClick);
            // 
            // menuitemPrint
            // 
            this.menuitemPrint.Name = "menuitemPrint";
            this.menuitemPrint.Size = new System.Drawing.Size(259, 22);
            this.menuitemPrint.Tag = "TitlePrintMenu";
            this.menuitemPrint.Text = "Печать ...";
            this.menuitemPrint.Click += new System.EventHandler(this.MenuitemPrintClick);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(256, 6);
            // 
            // menuMinimizeInTray
            // 
            this.menuMinimizeInTray.Name = "menuMinimizeInTray";
            this.menuMinimizeInTray.Size = new System.Drawing.Size(259, 22);
            this.menuMinimizeInTray.Tag = "TitleMinimizeMenu";
            this.menuMinimizeInTray.Text = "Свернуть";
            this.menuMinimizeInTray.Click += new System.EventHandler(this.MenuMinimizeInTrayClick);
            // 
            // menuExit
            // 
            this.menuExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(259, 22);
            this.menuExit.Tag = "TitleExitMenu";
            this.menuExit.Text = "Выход";
            this.menuExit.Click += new System.EventHandler(this.MenuExitClick);
            // 
            // видToolStripMenuItem
            // 
            this.видToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemViewAccount,
            this.menuitemLiveQuotes,
            this.menuitemNavPane,
            this.обозревательToolStripMenuItem});
            this.видToolStripMenuItem.Name = "видToolStripMenuItem";
            this.видToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.видToolStripMenuItem.Tag = "TitleView";
            this.видToolStripMenuItem.Text = "Вид";
            // 
            // menuitemViewAccount
            // 
            this.menuitemViewAccount.Name = "menuitemViewAccount";
            this.menuitemViewAccount.Size = new System.Drawing.Size(147, 22);
            this.menuitemViewAccount.Tag = "TitleAccount";
            this.menuitemViewAccount.Text = "Счет";
            this.menuitemViewAccount.Click += new System.EventHandler(this.MenuWindowAccountClick);
            // 
            // menuitemLiveQuotes
            // 
            this.menuitemLiveQuotes.Name = "menuitemLiveQuotes";
            this.menuitemLiveQuotes.Size = new System.Drawing.Size(147, 22);
            this.menuitemLiveQuotes.Tag = "TitleQuotes";
            this.menuitemLiveQuotes.Text = "Котировки";
            this.menuitemLiveQuotes.Click += new System.EventHandler(this.MenuWindowQuoteClick);
            // 
            // menuitemNavPane
            // 
            this.menuitemNavPane.Name = "menuitemNavPane";
            this.menuitemNavPane.Size = new System.Drawing.Size(147, 22);
            this.menuitemNavPane.Tag = "TitleInstruments";
            this.menuitemNavPane.Text = "Инструменты";
            this.menuitemNavPane.Click += new System.EventHandler(this.MenuitemNavPaneClick);
            // 
            // обозревательToolStripMenuItem
            // 
            this.обозревательToolStripMenuItem.Name = "обозревательToolStripMenuItem";
            this.обозревательToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.обозревательToolStripMenuItem.Tag = "TitleWebObserver";
            this.обозревательToolStripMenuItem.Text = "Обозреватель";
            this.обозревательToolStripMenuItem.Click += new System.EventHandler(this.WebBrowserToolStripMenuItemClick);
            // 
            // торговляToolStripMenuItem
            // 
            this.торговляToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNewOrder,
            this.menuItemAccountStat,
            this.menuitemJournals,
            this.menuitemTradeSignals,
            this.toolStripMenuItem2,
            this.menuItemRobotsSetup,
            this.toolStripSeparator2,
            this.menuitemScripts,
            this.menuitemScript});
            this.торговляToolStripMenuItem.Name = "торговляToolStripMenuItem";
            this.торговляToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.торговляToolStripMenuItem.Tag = "TitleTrade";
            this.торговляToolStripMenuItem.Text = "Торговля";
            // 
            // menuItemNewOrder
            // 
            this.menuItemNewOrder.Name = "menuItemNewOrder";
            this.menuItemNewOrder.Size = new System.Drawing.Size(153, 22);
            this.menuItemNewOrder.Tag = "TitleNewOrderMenu";
            this.menuItemNewOrder.Text = "Новый ордер...";
            this.menuItemNewOrder.Click += new System.EventHandler(this.MenuItemNewOrderClick);
            // 
            // menuItemAccountStat
            // 
            this.menuItemAccountStat.Name = "menuItemAccountStat";
            this.menuItemAccountStat.Size = new System.Drawing.Size(153, 22);
            this.menuItemAccountStat.Tag = "TitleStatisticsMenu";
            this.menuItemAccountStat.Text = "Статистика...";
            this.menuItemAccountStat.Click += new System.EventHandler(this.MenuItemAccountStatClick);
            // 
            // menuitemJournals
            // 
            this.menuitemJournals.Name = "menuitemJournals";
            this.menuitemJournals.Size = new System.Drawing.Size(153, 22);
            this.menuitemJournals.Tag = "TitleJournalsMenu";
            this.menuitemJournals.Text = "Журналы...";
            this.menuitemJournals.Click += new System.EventHandler(this.ЖурналыToolStripMenuItemClick);
            // 
            // menuitemTradeSignals
            // 
            this.menuitemTradeSignals.Name = "menuitemTradeSignals";
            this.menuitemTradeSignals.Size = new System.Drawing.Size(153, 22);
            this.menuitemTradeSignals.Tag = "TitleStrategiesMenu";
            this.menuitemTradeSignals.Text = "Стратегии...";
            this.menuitemTradeSignals.Click += new System.EventHandler(this.MenuitemTradeSignalsClick);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(150, 6);
            // 
            // menuItemRobotsSetup
            // 
            this.menuItemRobotsSetup.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemRobotTester,
            this.menuitemRobotPortfolio,
            this.menuitemRobotState});
            this.menuItemRobotsSetup.Name = "menuItemRobotsSetup";
            this.menuItemRobotsSetup.Size = new System.Drawing.Size(153, 22);
            this.menuItemRobotsSetup.Tag = "TitleRobots";
            this.menuItemRobotsSetup.Text = "Роботы";
            // 
            // menuitemRobotTester
            // 
            this.menuitemRobotTester.Name = "menuitemRobotTester";
            this.menuitemRobotTester.Size = new System.Drawing.Size(185, 22);
            this.menuitemRobotTester.Tag = "TitleRobotTesterMenu";
            this.menuitemRobotTester.Text = "Тестер роботов...";
            this.menuitemRobotTester.Click += new System.EventHandler(this.RobotTesterToolStripMenuItemClick);
            // 
            // menuitemRobotPortfolio
            // 
            this.menuitemRobotPortfolio.Name = "menuitemRobotPortfolio";
            this.menuitemRobotPortfolio.Size = new System.Drawing.Size(185, 22);
            this.menuitemRobotPortfolio.Tag = "TitleRobotPortfolioMenu";
            this.menuitemRobotPortfolio.Text = "Портфель роботов...";
            this.menuitemRobotPortfolio.Click += new System.EventHandler(this.MenuItemRobotsSetupClick);
            // 
            // menuitemRobotState
            // 
            this.menuitemRobotState.Enabled = false;
            this.menuitemRobotState.Name = "menuitemRobotState";
            this.menuitemRobotState.Size = new System.Drawing.Size(185, 22);
            this.menuitemRobotState.Tag = "TitleRobotStateMenu";
            this.menuitemRobotState.Text = "Состояние роботов...";
            this.menuitemRobotState.Click += new System.EventHandler(this.MenuitemRobotStateClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(150, 6);
            // 
            // menuitemScripts
            // 
            this.menuitemScripts.Name = "menuitemScripts";
            this.menuitemScripts.Size = new System.Drawing.Size(153, 22);
            this.menuitemScripts.Tag = "TitleScriptsMenu";
            this.menuitemScripts.Text = "Скрипты";
            this.menuitemScripts.Click += new System.EventHandler(this.MenuitemScriptsClick);
            // 
            // menuitemScript
            // 
            this.menuitemScript.Name = "menuitemScript";
            this.menuitemScript.Size = new System.Drawing.Size(153, 22);
            this.menuitemScript.Tag = "TitleScript";
            this.menuitemScript.Text = "Скрипт";
            // 
            // menuSettings
            // 
            this.menuSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemToolPanel,
            this.menuItemQuoteArchive,
            this.menuItemSettingsChart,
            this.menuTemplates,
            this.menuitemCommonParams,
            this.menuitemRiskSetup});
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(73, 20);
            this.menuSettings.Tag = "TitleSettings";
            this.menuSettings.Text = "Настройки";
            // 
            // menuitemToolPanel
            // 
            this.menuitemToolPanel.Name = "menuitemToolPanel";
            this.menuitemToolPanel.Size = new System.Drawing.Size(197, 22);
            this.menuitemToolPanel.Tag = "TitleToolbarMenu";
            this.menuitemToolPanel.Text = "Панель инструментов...";
            this.menuitemToolPanel.Click += new System.EventHandler(this.MenuitemToolPanelClick);
            // 
            // menuItemQuoteArchive
            // 
            this.menuItemQuoteArchive.Name = "menuItemQuoteArchive";
            this.menuItemQuoteArchive.Size = new System.Drawing.Size(197, 22);
            this.menuItemQuoteArchive.Tag = "TitleQuoteArchiveMenu";
            this.menuItemQuoteArchive.Text = "Архив котировок...";
            this.menuItemQuoteArchive.Click += new System.EventHandler(this.MenuItemQuoteQrchiveClick);
            // 
            // menuItemSettingsChart
            // 
            this.menuItemSettingsChart.Name = "menuItemSettingsChart";
            this.menuItemSettingsChart.Size = new System.Drawing.Size(197, 22);
            this.menuItemSettingsChart.Tag = "TitleChartSettingsMenu";
            this.menuItemSettingsChart.Text = "Настройки графиков...";
            this.menuItemSettingsChart.Click += new System.EventHandler(this.MenuItemSettingsChartClick);
            // 
            // menuTemplates
            // 
            this.menuTemplates.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuSaveTemplate,
            this.menuApplyTemplate,
            this.menuManageTemplate});
            this.menuTemplates.Name = "menuTemplates";
            this.menuTemplates.Size = new System.Drawing.Size(197, 22);
            this.menuTemplates.Tag = "TitleTemplates";
            this.menuTemplates.Text = "Шаблоны";
            // 
            // menuSaveTemplate
            // 
            this.menuSaveTemplate.Name = "menuSaveTemplate";
            this.menuSaveTemplate.Size = new System.Drawing.Size(203, 22);
            this.menuSaveTemplate.Tag = "TitleSaveTemplateMenu";
            this.menuSaveTemplate.Text = "Сохранить как шаблон...";
            this.menuSaveTemplate.Click += new System.EventHandler(this.MenuSaveTemplateClick);
            // 
            // menuApplyTemplate
            // 
            this.menuApplyTemplate.Name = "menuApplyTemplate";
            this.menuApplyTemplate.Size = new System.Drawing.Size(203, 22);
            this.menuApplyTemplate.Tag = "TitleApplyTemplateMenu";
            this.menuApplyTemplate.Text = "Применить шаблон...";
            this.menuApplyTemplate.Click += new System.EventHandler(this.MenuLoadTemplateClick);
            // 
            // menuManageTemplate
            // 
            this.menuManageTemplate.Name = "menuManageTemplate";
            this.menuManageTemplate.Size = new System.Drawing.Size(203, 22);
            this.menuManageTemplate.Tag = "TitleManageTemplates";
            this.menuManageTemplate.Text = "Управление шаблонами";
            this.menuManageTemplate.Click += new System.EventHandler(this.MenuManageTemplateClick);
            // 
            // menuitemCommonParams
            // 
            this.menuitemCommonParams.Name = "menuitemCommonParams";
            this.menuitemCommonParams.Size = new System.Drawing.Size(197, 22);
            this.menuitemCommonParams.Tag = "TitleParametersMenu";
            this.menuitemCommonParams.Text = "Параметры...";
            this.menuitemCommonParams.Click += new System.EventHandler(this.ПараметрыToolStripMenuItemClick);
            // 
            // menuitemRiskSetup
            // 
            this.menuitemRiskSetup.Name = "menuitemRiskSetup";
            this.menuitemRiskSetup.Size = new System.Drawing.Size(197, 22);
            this.menuitemRiskSetup.Tag = "TitleRiskMenu";
            this.menuitemRiskSetup.Text = "Риск...";
            this.menuitemRiskSetup.Click += new System.EventHandler(this.MenuitemRiskSetupClick);
            // 
            // menuWindows
            // 
            this.menuWindows.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCascade,
            this.menuItemVertical,
            this.menuItemHorizontal,
            this.menuItemMinimizeAll,
            this.toolStripSeparator1,
            this.menuitemStatusBar});
            this.menuWindows.Name = "menuWindows";
            this.menuWindows.Size = new System.Drawing.Size(45, 20);
            this.menuWindows.Tag = "TitleWindows";
            this.menuWindows.Text = "Окна";
            // 
            // menuItemCascade
            // 
            this.menuItemCascade.Name = "menuItemCascade";
            this.menuItemCascade.Size = new System.Drawing.Size(220, 22);
            this.menuItemCascade.Tag = "TitleArrangeCascade";
            this.menuItemCascade.Text = "Расположить каскадом";
            this.menuItemCascade.Click += new System.EventHandler(this.MenuItemCascadeClick);
            // 
            // menuItemVertical
            // 
            this.menuItemVertical.Name = "menuItemVertical";
            this.menuItemVertical.Size = new System.Drawing.Size(220, 22);
            this.menuItemVertical.Tag = "TitleArrangeVertical";
            this.menuItemVertical.Text = "Расположить вертикально";
            this.menuItemVertical.Click += new System.EventHandler(this.MenuItemVerticalClick);
            // 
            // menuItemHorizontal
            // 
            this.menuItemHorizontal.Name = "menuItemHorizontal";
            this.menuItemHorizontal.Size = new System.Drawing.Size(220, 22);
            this.menuItemHorizontal.Tag = "TitleArrangeHorizontal";
            this.menuItemHorizontal.Text = "Расположить горизонтально";
            this.menuItemHorizontal.Click += new System.EventHandler(this.MenuItemHorizontalClick);
            // 
            // menuItemMinimizeAll
            // 
            this.menuItemMinimizeAll.Name = "menuItemMinimizeAll";
            this.menuItemMinimizeAll.Size = new System.Drawing.Size(220, 22);
            this.menuItemMinimizeAll.Tag = "TitleMinimizeAll";
            this.menuItemMinimizeAll.Text = "Свернуть все";
            this.menuItemMinimizeAll.Click += new System.EventHandler(this.MenuItemMinimizeAllClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // menuitemStatusBar
            // 
            this.menuitemStatusBar.Name = "menuitemStatusBar";
            this.menuitemStatusBar.Size = new System.Drawing.Size(220, 22);
            this.menuitemStatusBar.Tag = "TitleStatusBar";
            this.menuitemStatusBar.Text = "Статусная строка";
            this.menuitemStatusBar.Click += new System.EventHandler(this.MenuitemStatusBarClick);
            // 
            // menutopicHelp
            // 
            this.menutopicHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemShowHelp,
            this.menuitemShowAbout,
            this.menuitemHelpTooltip});
            this.menutopicHelp.Name = "menutopicHelp";
            this.menutopicHelp.Size = new System.Drawing.Size(62, 20);
            this.menutopicHelp.Tag = "TitleHelp";
            this.menutopicHelp.Text = "Справка";
            // 
            // menuitemShowHelp
            // 
            this.menuitemShowHelp.Name = "menuitemShowHelp";
            this.menuitemShowHelp.Size = new System.Drawing.Size(150, 22);
            this.menuitemShowHelp.Tag = "TitleShowHelp";
            this.menuitemShowHelp.Text = "Вызов справки";
            this.menuitemShowHelp.Click += new System.EventHandler(this.MenuitemShowHelpClick);
            // 
            // menuitemShowAbout
            // 
            this.menuitemShowAbout.Name = "menuitemShowAbout";
            this.menuitemShowAbout.Size = new System.Drawing.Size(150, 22);
            this.menuitemShowAbout.Tag = "TitleAboutMenu";
            this.menuitemShowAbout.Text = "О программе...";
            this.menuitemShowAbout.Click += new System.EventHandler(this.ОПрограммеToolStripMenuItemClick);
            // 
            // menuitemHelpTooltip
            // 
            this.menuitemHelpTooltip.Name = "menuitemHelpTooltip";
            this.menuitemHelpTooltip.Size = new System.Drawing.Size(150, 22);
            this.menuitemHelpTooltip.Tag = "TitleTipsMenu";
            this.menuitemHelpTooltip.Text = "Подсказки...";
            this.menuitemHelpTooltip.Click += new System.EventHandler(this.MenuitemHelpTooltipClick);
            // 
            // panelTools
            // 
            this.panelTools.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTools.Controls.Add(this.panelCommonTools);
            this.panelTools.Controls.Add(this.panelChartTools);
            this.panelTools.Controls.Add(this.btnPrint);
            this.panelTools.Controls.Add(this.btnSaveConfig);
            this.panelTools.Controls.Add(this.btnLoadConfig);
            this.panelTools.Controls.Add(this.btnNewChart);
            this.panelTools.Controls.Add(this.cbTimeFrame);
            this.panelTools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTools.Location = new System.Drawing.Point(0, 0);
            this.panelTools.Name = "panelTools";
            this.panelTools.Size = new System.Drawing.Size(1247, 33);
            this.panelTools.TabIndex = 2;
            // 
            // panelCommonTools
            // 
            this.panelCommonTools.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCommonTools.Controls.Add(this.btnFlipPanelCommonTools);
            this.panelCommonTools.Location = new System.Drawing.Point(304, 1);
            this.panelCommonTools.Name = "panelCommonTools";
            this.panelCommonTools.Size = new System.Drawing.Size(31, 29);
            this.panelCommonTools.TabIndex = 23;
            // 
            // btnFlipPanelCommonTools
            // 
            this.btnFlipPanelCommonTools.FlatAppearance.BorderSize = 0;
            this.btnFlipPanelCommonTools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFlipPanelCommonTools.ImageIndex = 22;
            this.btnFlipPanelCommonTools.ImageList = this.lstGlyph32;
            this.btnFlipPanelCommonTools.Location = new System.Drawing.Point(0, 2);
            this.btnFlipPanelCommonTools.Margin = new System.Windows.Forms.Padding(2);
            this.btnFlipPanelCommonTools.Name = "btnFlipPanelCommonTools";
            this.btnFlipPanelCommonTools.Size = new System.Drawing.Size(22, 22);
            this.btnFlipPanelCommonTools.TabIndex = 25;
            this.btnFlipPanelCommonTools.UseVisualStyleBackColor = true;
            this.btnFlipPanelCommonTools.Click += new System.EventHandler(this.BtnFlipPanelChartToolsClick);
            // 
            // lstGlyph32
            // 
            this.lstGlyph32.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lstGlyph32.ImageStream")));
            this.lstGlyph32.TransparentColor = System.Drawing.Color.Transparent;
            this.lstGlyph32.Images.SetKeyName(0, "cursor.png");
            this.lstGlyph32.Images.SetKeyName(1, "trend_line2.png");
            this.lstGlyph32.Images.SetKeyName(2, "deal_mark.png");
            this.lstGlyph32.Images.SetKeyName(3, "tag.png");
            this.lstGlyph32.Images.SetKeyName(4, "fibo_channel.png");
            this.lstGlyph32.Images.SetKeyName(5, "ellipse.png");
            this.lstGlyph32.Images.SetKeyName(6, "horz_span.png");
            this.lstGlyph32.Images.SetKeyName(7, "turnBar.png");
            this.lstGlyph32.Images.SetKeyName(8, "fibo_vert.png");
            this.lstGlyph32.Images.SetKeyName(9, "trend_line.png");
            this.lstGlyph32.Images.SetKeyName(10, "asteriks 1.bmp");
            this.lstGlyph32.Images.SetKeyName(11, "tmp1.bmp");
            this.lstGlyph32.Images.SetKeyName(12, "tmp2.bmp");
            this.lstGlyph32.Images.SetKeyName(13, "tmp3.bmp");
            this.lstGlyph32.Images.SetKeyName(14, "tmp4.bmp");
            this.lstGlyph32.Images.SetKeyName(15, "tmp5.bmp");
            this.lstGlyph32.Images.SetKeyName(16, "1123.bmp");
            this.lstGlyph32.Images.SetKeyName(17, "signMove.png");
            this.lstGlyph32.Images.SetKeyName(18, "magnifier_zoom_in_7351.png");
            this.lstGlyph32.Images.SetKeyName(19, "magnifier_zoom_out_6028.png");
            this.lstGlyph32.Images.SetKeyName(20, "cursor_2132_.png");
            this.lstGlyph32.Images.SetKeyName(21, "tablet_left.png");
            this.lstGlyph32.Images.SetKeyName(22, "tablet_right.png");
            this.lstGlyph32.Images.SetKeyName(23, "ico_doubledollar.png");
            this.lstGlyph32.Images.SetKeyName(24, "ico_angle.png");
            this.lstGlyph32.Images.SetKeyName(25, "ico_square.png");
            this.lstGlyph32.Images.SetKeyName(26, "ico_s.png");
            this.lstGlyph32.Images.SetKeyName(27, "ico_tortoise.png");
            this.lstGlyph32.Images.SetKeyName(28, "ico_16_arrow_black.png");
            this.lstGlyph32.Images.SetKeyName(29, "ico_16_arrow_white.png");
            this.lstGlyph32.Images.SetKeyName(30, "ico_16_tag.png");
            this.lstGlyph32.Images.SetKeyName(31, "ico_16_scissors.png");
            this.lstGlyph32.Images.SetKeyName(32, "ico_16_lightbulb.png");
            this.lstGlyph32.Images.SetKeyName(33, "ico_16_lightening.png");
            this.lstGlyph32.Images.SetKeyName(34, "ico_16_edelveis.png");
            this.lstGlyph32.Images.SetKeyName(35, "ico_16_ellipse_blue.png");
            this.lstGlyph32.Images.SetKeyName(36, "ico_16_ellipse_green.png");
            this.lstGlyph32.Images.SetKeyName(37, "ico_line_marker.png");
            this.lstGlyph32.Images.SetKeyName(38, "ico_fibo_grid.png");
            this.lstGlyph32.Images.SetKeyName(39, "ico_bubbles_text.png");
            this.lstGlyph32.Images.SetKeyName(40, "ico_bubbles.png");
            this.lstGlyph32.Images.SetKeyName(41, "ico_letter_a.png");
            this.lstGlyph32.Images.SetKeyName(42, "ico_red_down_arrow.png");
            this.lstGlyph32.Images.SetKeyName(43, "ico_green_down_arrow.png");
            this.lstGlyph32.Images.SetKeyName(44, "ico_green_up_arrow.png");
            this.lstGlyph32.Images.SetKeyName(45, "ico_red_up_arrow.png");
            this.lstGlyph32.Images.SetKeyName(46, "ico_fibo_proj.png");
            this.lstGlyph32.Images.SetKeyName(47, "ico_fibo_corr.png");
            this.lstGlyph32.Images.SetKeyName(48, "ico_circle_blue.png");
            this.lstGlyph32.Images.SetKeyName(49, "ico_circle_red.png");
            this.lstGlyph32.Images.SetKeyName(50, "ico_trian_left.png");
            this.lstGlyph32.Images.SetKeyName(51, "ico_trian_right.png");
            this.lstGlyph32.Images.SetKeyName(52, "ico_buy.png");
            this.lstGlyph32.Images.SetKeyName(53, "ico_price02.png");
            this.lstGlyph32.Images.SetKeyName(54, "ico_price01.png");
            this.lstGlyph32.Images.SetKeyName(55, "ico_two_slope_clb_lines.png");
            this.lstGlyph32.Images.SetKeyName(56, "ico_two_slope_cl_lines.png");
            this.lstGlyph32.Images.SetKeyName(57, "ico_two_slope_lines.png");
            this.lstGlyph32.Images.SetKeyName(58, "ico_two_vert_lines.png");
            this.lstGlyph32.Images.SetKeyName(59, "ico_two_horz_lines.png");
            this.lstGlyph32.Images.SetKeyName(60, "IconAC.ico");
            this.lstGlyph32.Images.SetKeyName(61, "IconFS.ico");
            this.lstGlyph32.Images.SetKeyName(62, "IconFI.ico");
            this.lstGlyph32.Images.SetKeyName(63, "disconnected.png");
            this.lstGlyph32.Images.SetKeyName(64, "globe_connected.png");
            this.lstGlyph32.Images.SetKeyName(65, "setOrder.ico");
            this.lstGlyph32.Images.SetKeyName(66, "rollGraph.ico");
            this.lstGlyph32.Images.SetKeyName(67, "rollAllGraph.ico");
            this.lstGlyph32.Images.SetKeyName(68, "indi.ico");
            this.lstGlyph32.Images.SetKeyName(69, "indi.ico");
            this.lstGlyph32.Images.SetKeyName(70, "ico_16_expand.png");
            this.lstGlyph32.Images.SetKeyName(71, "ico_16_shrink.png");
            this.lstGlyph32.Images.SetKeyName(72, "gear_16.png");
            this.lstGlyph32.Images.SetKeyName(73, "ico_expand_high.png");
            this.lstGlyph32.Images.SetKeyName(74, "ico_text_signal.png");
            this.lstGlyph32.Images.SetKeyName(75, "ico_chart_object.png");
            this.lstGlyph32.Images.SetKeyName(76, "ico_portfolio_small.png");
            this.lstGlyph32.Images.SetKeyName(77, "ico_patch.png");
            this.lstGlyph32.Images.SetKeyName(78, "ico_reload.png");
            this.lstGlyph32.Images.SetKeyName(79, "ico_cancel.png");
            this.lstGlyph32.Images.SetKeyName(80, "ico_find.png");
            this.lstGlyph32.Images.SetKeyName(81, "alert.png");
            this.lstGlyph32.Images.SetKeyName(82, "ico_ruler.png");
            this.lstGlyph32.Images.SetKeyName(83, "ico_arrow_red_down.png");
            this.lstGlyph32.Images.SetKeyName(84, "ico_arrow_grey_right.png");
            this.lstGlyph32.Images.SetKeyName(85, "ico_close_gray.png");
            this.lstGlyph32.Images.SetKeyName(86, "ico_navigator.png");
            this.lstGlyph32.Images.SetKeyName(87, "ico_arrow_green_down.png");
            this.lstGlyph32.Images.SetKeyName(88, "ico_wizzard.png");
            this.lstGlyph32.Images.SetKeyName(89, "ico_lamp.png");
            this.lstGlyph32.Images.SetKeyName(90, "ico_alert.png");
            this.lstGlyph32.Images.SetKeyName(91, "ico_unfavorite.png");
            this.lstGlyph32.Images.SetKeyName(92, "email.png");
            this.lstGlyph32.Images.SetKeyName(93, "facebook.png");
            this.lstGlyph32.Images.SetKeyName(94, "gear_blue.png");
            this.lstGlyph32.Images.SetKeyName(95, "googleplus.png");
            this.lstGlyph32.Images.SetKeyName(96, "ico minus.png");
            this.lstGlyph32.Images.SetKeyName(97, "ico_calculator.png");
            this.lstGlyph32.Images.SetKeyName(98, "ico_flag_australia.png");
            this.lstGlyph32.Images.SetKeyName(99, "ico_flag_britain.png");
            this.lstGlyph32.Images.SetKeyName(100, "ico_flag_canada.png");
            this.lstGlyph32.Images.SetKeyName(101, "ico_flag_eur.png");
            this.lstGlyph32.Images.SetKeyName(102, "ico_flag_japan.png");
            this.lstGlyph32.Images.SetKeyName(103, "ico_flag_swiss.png");
            this.lstGlyph32.Images.SetKeyName(104, "ico_flag_usa.png");
            this.lstGlyph32.Images.SetKeyName(105, "ico_pin_16.png");
            this.lstGlyph32.Images.SetKeyName(106, "ico_up_lite.png");
            this.lstGlyph32.Images.SetKeyName(107, "ico_wizzard.png");
            this.lstGlyph32.Images.SetKeyName(108, "mailru.png");
            this.lstGlyph32.Images.SetKeyName(109, "odnoklassniki.png");
            this.lstGlyph32.Images.SetKeyName(110, "skype.png");
            this.lstGlyph32.Images.SetKeyName(111, "twitter.png");
            this.lstGlyph32.Images.SetKeyName(112, "vk.png");
            this.lstGlyph32.Images.SetKeyName(113, "ico_robot_condition.png");
            this.lstGlyph32.Images.SetKeyName(114, "ico_down_color.png");
            this.lstGlyph32.Images.SetKeyName(115, "ico_up_color.png");
            this.lstGlyph32.Images.SetKeyName(116, "ico_archive_color.png");
            this.lstGlyph32.Images.SetKeyName(117, "ico_chart.png");
            this.lstGlyph32.Images.SetKeyName(118, "ico_signal_vista.png");
            this.lstGlyph32.Images.SetKeyName(119, "ico_chat.png");
            // 
            // panelChartTools
            // 
            this.panelChartTools.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelChartTools.Controls.Add(this.btnFlipPanelChartTools);
            this.panelChartTools.Controls.Add(this.toolBtnCross);
            this.panelChartTools.Location = new System.Drawing.Point(212, 1);
            this.panelChartTools.Name = "panelChartTools";
            this.panelChartTools.Size = new System.Drawing.Size(88, 29);
            this.panelChartTools.TabIndex = 22;
            // 
            // btnFlipPanelChartTools
            // 
            this.btnFlipPanelChartTools.FlatAppearance.BorderSize = 0;
            this.btnFlipPanelChartTools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFlipPanelChartTools.ImageIndex = 22;
            this.btnFlipPanelChartTools.ImageList = this.lstGlyph32;
            this.btnFlipPanelChartTools.Location = new System.Drawing.Point(0, 2);
            this.btnFlipPanelChartTools.Margin = new System.Windows.Forms.Padding(2);
            this.btnFlipPanelChartTools.Name = "btnFlipPanelChartTools";
            this.btnFlipPanelChartTools.Size = new System.Drawing.Size(22, 22);
            this.btnFlipPanelChartTools.TabIndex = 25;
            this.btnFlipPanelChartTools.UseVisualStyleBackColor = true;
            this.btnFlipPanelChartTools.Click += new System.EventHandler(this.BtnFlipPanelChartToolsClick);
            // 
            // toolBtnCross
            // 
            this.toolBtnCross.ImageIndex = 20;
            this.toolBtnCross.ImageList = this.lstGlyph32;
            this.toolBtnCross.Location = new System.Drawing.Point(94, 1);
            this.toolBtnCross.Margin = new System.Windows.Forms.Padding(2);
            this.toolBtnCross.Name = "toolBtnCross";
            this.toolBtnCross.Size = new System.Drawing.Size(28, 25);
            this.toolBtnCross.TabIndex = 24;
            this.toolBtnCross.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrint.ImageIndex = 3;
            this.btnPrint.ImageList = this.lstGlyth;
            this.btnPrint.Location = new System.Drawing.Point(72, 3);
            this.btnPrint.Margin = new System.Windows.Forms.Padding(2);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(28, 25);
            this.btnPrint.TabIndex = 17;
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.BtnPrintClick);
            // 
            // lstGlyth
            // 
            this.lstGlyth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lstGlyth.ImageStream")));
            this.lstGlyth.TransparentColor = System.Drawing.Color.Transparent;
            this.lstGlyth.Images.SetKeyName(0, "ico addchart.png");
            this.lstGlyth.Images.SetKeyName(1, "ico_load.png");
            this.lstGlyth.Images.SetKeyName(2, "ico_save.png");
            this.lstGlyth.Images.SetKeyName(3, "icon_printer.png");
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveConfig.ImageIndex = 2;
            this.btnSaveConfig.ImageList = this.lstGlyth;
            this.btnSaveConfig.Location = new System.Drawing.Point(39, 3);
            this.btnSaveConfig.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(28, 25);
            this.btnSaveConfig.TabIndex = 16;
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.BtnSaveConfigClick);
            // 
            // btnLoadConfig
            // 
            this.btnLoadConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoadConfig.ImageIndex = 1;
            this.btnLoadConfig.ImageList = this.lstGlyth;
            this.btnLoadConfig.Location = new System.Drawing.Point(6, 3);
            this.btnLoadConfig.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoadConfig.Name = "btnLoadConfig";
            this.btnLoadConfig.Size = new System.Drawing.Size(28, 25);
            this.btnLoadConfig.TabIndex = 15;
            this.btnLoadConfig.UseVisualStyleBackColor = true;
            this.btnLoadConfig.Click += new System.EventHandler(this.BtnLoadConfigClick);
            // 
            // btnNewChart
            // 
            this.btnNewChart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewChart.ImageIndex = 0;
            this.btnNewChart.ImageList = this.lstGlyth;
            this.btnNewChart.Location = new System.Drawing.Point(105, 3);
            this.btnNewChart.Margin = new System.Windows.Forms.Padding(2);
            this.btnNewChart.Name = "btnNewChart";
            this.btnNewChart.Size = new System.Drawing.Size(28, 25);
            this.btnNewChart.TabIndex = 1;
            this.btnNewChart.UseVisualStyleBackColor = true;
            this.btnNewChart.Click += new System.EventHandler(this.MenuNewChartClick);
            // 
            // cbTimeFrame
            // 
            this.cbTimeFrame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTimeFrame.FormattingEnabled = true;
            this.cbTimeFrame.Location = new System.Drawing.Point(141, 4);
            this.cbTimeFrame.Name = "cbTimeFrame";
            this.cbTimeFrame.Size = new System.Drawing.Size(65, 21);
            this.cbTimeFrame.TabIndex = 0;
            this.cbTimeFrame.SelectedIndexChanged += new System.EventHandler(this.CbTimeFrameSelectedIndexChanged);
            // 
            // panelStatus
            // 
            this.panelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelStatus.Controls.Add(this.splitContainerStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 466);
            this.panelStatus.MinDimension = 5;
            this.panelStatus.MinimumSize = new System.Drawing.Size(4, 35);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.panelStatus.SizableBottom = false;
            this.panelStatus.SizableLeft = false;
            this.panelStatus.SizableRight = false;
            this.panelStatus.SizableTop = true;
            this.panelStatus.Size = new System.Drawing.Size(1247, 80);
            this.panelStatus.TabIndex = 3;
            // 
            // splitContainerStatus
            // 
            this.splitContainerStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerStatus.Location = new System.Drawing.Point(0, 5);
            this.splitContainerStatus.Name = "splitContainerStatus";
            // 
            // splitContainerStatus.Panel1
            // 
            this.splitContainerStatus.Panel1.Controls.Add(this.tbStatus);
            this.splitContainerStatus.Panel1.Controls.Add(this.panelStatusRight);
            this.splitContainerStatus.Panel1.Controls.Add(this.bookmarkStrip);
            // 
            // splitContainerStatus.Panel2
            // 
            this.splitContainerStatus.Panel2.Controls.Add(this.btnShowTradeSignals);
            this.splitContainerStatus.Panel2.Controls.Add(this.btnChat);
            this.splitContainerStatus.Panel2.Controls.Add(this.btnShrinkStatusPanel);
            this.splitContainerStatus.Panel2.Controls.Add(this.btnExpandStatusPanel);
            this.splitContainerStatus.Panel2.Controls.Add(this.panelConnect);
            this.splitContainerStatus.Size = new System.Drawing.Size(1243, 71);
            this.splitContainerStatus.SplitterDistance = 686;
            this.splitContainerStatus.TabIndex = 0;
            // 
            // tbStatus
            // 
            this.tbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbStatus.Location = new System.Drawing.Point(0, 22);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.tbStatus.Size = new System.Drawing.Size(674, 47);
            this.tbStatus.TabIndex = 9;
            this.tbStatus.Text = " ";
            this.tbStatus.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.TbStatusLinkClicked);
            // 
            // panelStatusRight
            // 
            this.panelStatusRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelStatusRight.Location = new System.Drawing.Point(674, 22);
            this.panelStatusRight.Name = "panelStatusRight";
            this.panelStatusRight.Size = new System.Drawing.Size(10, 47);
            this.panelStatusRight.TabIndex = 8;
            // 
            // bookmarkStrip
            // 
            this.bookmarkStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.bookmarkStrip.Location = new System.Drawing.Point(0, 0);
            this.bookmarkStrip.Name = "bookmarkStrip";
            this.bookmarkStrip.SelectedBookmark = null;
            this.bookmarkStrip.Size = new System.Drawing.Size(684, 22);
            this.bookmarkStrip.TabIndex = 7;
            this.bookmarkStrip.SelectedTabChanged += new System.Action<TradeSharp.Client.Controls.Bookmark.TerminalBookmark, TradeSharp.Client.Controls.Bookmark.TerminalBookmark>(this.BookmarkSelected);
            // 
            // btnShowTradeSignals
            // 
            this.btnShowTradeSignals.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShowTradeSignals.ImageIndex = 118;
            this.btnShowTradeSignals.ImageList = this.lstGlyph32;
            this.btnShowTradeSignals.Location = new System.Drawing.Point(32, 31);
            this.btnShowTradeSignals.Name = "btnShowTradeSignals";
            this.btnShowTradeSignals.Size = new System.Drawing.Size(22, 22);
            this.btnShowTradeSignals.TabIndex = 7;
            this.btnShowTradeSignals.UseVisualStyleBackColor = true;
            this.btnShowTradeSignals.Click += new System.EventHandler(this.BtnShowTradeSignalsClick);
            // 
            // btnChat
            // 
            this.btnChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChat.ImageIndex = 119;
            this.btnChat.ImageList = this.lstGlyph32;
            this.btnChat.Location = new System.Drawing.Point(32, 4);
            this.btnChat.Name = "btnChat";
            this.btnChat.Size = new System.Drawing.Size(22, 22);
            this.btnChat.TabIndex = 4;
            this.btnChat.UseVisualStyleBackColor = true;
            this.btnChat.Click += new System.EventHandler(this.BtnChatClick);
            // 
            // btnShrinkStatusPanel
            // 
            this.btnShrinkStatusPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShrinkStatusPanel.ImageIndex = 71;
            this.btnShrinkStatusPanel.ImageList = this.lstGlyph32;
            this.btnShrinkStatusPanel.Location = new System.Drawing.Point(4, 31);
            this.btnShrinkStatusPanel.Name = "btnShrinkStatusPanel";
            this.btnShrinkStatusPanel.Size = new System.Drawing.Size(22, 22);
            this.btnShrinkStatusPanel.TabIndex = 3;
            this.btnShrinkStatusPanel.UseVisualStyleBackColor = true;
            this.btnShrinkStatusPanel.Click += new System.EventHandler(this.BtnShrinkStatusPanelClick);
            // 
            // btnExpandStatusPanel
            // 
            this.btnExpandStatusPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExpandStatusPanel.ImageIndex = 70;
            this.btnExpandStatusPanel.ImageList = this.lstGlyph32;
            this.btnExpandStatusPanel.Location = new System.Drawing.Point(4, 4);
            this.btnExpandStatusPanel.Name = "btnExpandStatusPanel";
            this.btnExpandStatusPanel.Size = new System.Drawing.Size(22, 22);
            this.btnExpandStatusPanel.TabIndex = 2;
            this.btnExpandStatusPanel.UseVisualStyleBackColor = true;
            this.btnExpandStatusPanel.Click += new System.EventHandler(this.BtnExpandStatusPanelClick);
            // 
            // panelConnect
            // 
            this.panelConnect.Controls.Add(this.lblConnectStatus);
            this.panelConnect.Controls.Add(this.lblAccountData);
            this.panelConnect.Controls.Add(this.connectionStatusImage);
            this.panelConnect.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelConnect.Location = new System.Drawing.Point(302, 0);
            this.panelConnect.Name = "panelConnect";
            this.panelConnect.Size = new System.Drawing.Size(249, 69);
            this.panelConnect.TabIndex = 1;
            // 
            // lblConnectStatus
            // 
            this.lblConnectStatus.AutoSize = true;
            this.lblConnectStatus.Location = new System.Drawing.Point(42, 6);
            this.lblConnectStatus.Name = "lblConnectStatus";
            this.lblConnectStatus.Size = new System.Drawing.Size(57, 13);
            this.lblConnectStatus.TabIndex = 8;
            this.lblConnectStatus.TabStop = true;
            this.lblConnectStatus.Text = "Отключен";
            this.lblConnectStatus.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LblConnectStatusLinkClicked);
            // 
            // lblAccountData
            // 
            this.lblAccountData.AutoSize = true;
            this.lblAccountData.Location = new System.Drawing.Point(43, 25);
            this.lblAccountData.Name = "lblAccountData";
            this.lblAccountData.Size = new System.Drawing.Size(10, 13);
            this.lblAccountData.TabIndex = 7;
            this.lblAccountData.Text = "-";
            // 
            // connectionStatusImage
            // 
            this.connectionStatusImage.HasQuotes = false;
            this.connectionStatusImage.HasServer = false;
            this.connectionStatusImage.Location = new System.Drawing.Point(5, 5);
            this.connectionStatusImage.Name = "connectionStatusImage";
            this.connectionStatusImage.Size = new System.Drawing.Size(32, 32);
            this.connectionStatusImage.TabIndex = 0;
            // 
            // dlgWorkspaceSave
            // 
            this.dlgWorkspaceSave.DefaultExt = "wsp";
            this.dlgWorkspaceSave.Filter = "Рабочее пространство (*.zip)|*.zip|Все файлы|*.*";
            this.dlgWorkspaceSave.FilterIndex = 0;
            this.dlgWorkspaceSave.Title = "Сохранить рабочее пространство";
            // 
            // dlgWorkspaceLoad
            // 
            this.dlgWorkspaceLoad.DefaultExt = "wsp";
            this.dlgWorkspaceLoad.Filter = "Рабочее пространство (*.zip)|*.zip|Все файлы|*.*";
            this.dlgWorkspaceLoad.FilterIndex = 0;
            this.dlgWorkspaceLoad.Title = "Загрузить рабочее пространство";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelTools);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1247, 33);
            this.panel1.TabIndex = 4;
            // 
            // imgListMenu
            // 
            this.imgListMenu.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListMenu.ImageStream")));
            this.imgListMenu.TransparentColor = System.Drawing.Color.Transparent;
            this.imgListMenu.Images.SetKeyName(0, "ico_archive.png");
            this.imgListMenu.Images.SetKeyName(1, "ico_chart.png");
            this.imgListMenu.Images.SetKeyName(2, "ico_chart_2.png");
            this.imgListMenu.Images.SetKeyName(3, "ico_curx_eur.png");
            this.imgListMenu.Images.SetKeyName(4, "ico_key.png");
            this.imgListMenu.Images.SetKeyName(5, "ico_load.png");
            this.imgListMenu.Images.SetKeyName(6, "ico_new.png");
            this.imgListMenu.Images.SetKeyName(7, "ico_quit.png");
            this.imgListMenu.Images.SetKeyName(8, "ico_receipt.png");
            this.imgListMenu.Images.SetKeyName(9, "ico_robot.png");
            this.imgListMenu.Images.SetKeyName(10, "ico_robot_2.png");
            this.imgListMenu.Images.SetKeyName(11, "ico_save.png");
            this.imgListMenu.Images.SetKeyName(12, "ico_script.png");
            this.imgListMenu.Images.SetKeyName(13, "ico_settings.png");
            this.imgListMenu.Images.SetKeyName(14, "ico_settings_2.png");
            this.imgListMenu.Images.SetKeyName(15, "ico_signal.png");
            this.imgListMenu.Images.SetKeyName(16, "ico_table.png");
            this.imgListMenu.Images.SetKeyName(17, "ico_wallet.png");
            this.imgListMenu.Images.SetKeyName(18, "ico_window_cascade.png");
            this.imgListMenu.Images.SetKeyName(19, "ico_window_horz.png");
            this.imgListMenu.Images.SetKeyName(20, "ico_window_vertical.png");
            this.imgListMenu.Images.SetKeyName(21, "ico_portfolio_small.png");
            this.imgListMenu.Images.SetKeyName(22, "ico_robot_condition.png");
            this.imgListMenu.Images.SetKeyName(23, "icon_printer.png");
            this.imgListMenu.Images.SetKeyName(24, "ico_navigator.png");
            this.imgListMenu.Images.SetKeyName(25, "ico_drawing.png");
            this.imgListMenu.Images.SetKeyName(26, "ico_alert.png");
            this.imgListMenu.Images.SetKeyName(27, "ico_wallet.png");
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipTitle = "TRADE#";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "TRADE#";
            this.notifyIcon.Visible = true;
            this.notifyIcon.BalloonTipClicked += new System.EventHandler(this.NotifyIconBalloonTipClicked);
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMouseDoubleClick);
            this.notifyIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMouseUp);
            // 
            // contextMenuTray
            // 
            this.contextMenuTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuTrayRestore,
            this.menuTrayQuit,
            this.menuTrayCancel});
            this.contextMenuTray.Name = "contextMenuTray";
            this.contextMenuTray.Size = new System.Drawing.Size(173, 70);
            // 
            // menuTrayRestore
            // 
            this.menuTrayRestore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuTrayRestore.Name = "menuTrayRestore";
            this.menuTrayRestore.Size = new System.Drawing.Size(172, 22);
            this.menuTrayRestore.Tag = "TitleOpenTradeSharpMenu";
            this.menuTrayRestore.Text = "Открыть TRADE#";
            this.menuTrayRestore.Click += new System.EventHandler(this.MenuTrayRestoreClick);
            // 
            // menuTrayQuit
            // 
            this.menuTrayQuit.Name = "menuTrayQuit";
            this.menuTrayQuit.Size = new System.Drawing.Size(172, 22);
            this.menuTrayQuit.Tag = "TitleExitMenu";
            this.menuTrayQuit.Text = "Выход";
            this.menuTrayQuit.Click += new System.EventHandler(this.MenuTrayQuitClick);
            // 
            // menuTrayCancel
            // 
            this.menuTrayCancel.Name = "menuTrayCancel";
            this.menuTrayCancel.Size = new System.Drawing.Size(172, 22);
            this.menuTrayCancel.Tag = "TitleCancel";
            this.menuTrayCancel.Text = "Отмена";
            this.menuTrayCancel.Click += new System.EventHandler(this.MenuTrayCancelClick);
            // 
            // panelNavi
            // 
            this.panelNavi.Controls.Add(this.navPanelControl);
            this.panelNavi.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelNavi.Location = new System.Drawing.Point(0, 57);
            this.panelNavi.MinDimension = 5;
            this.panelNavi.MinimumSize = new System.Drawing.Size(40, 0);
            this.panelNavi.Name = "panelNavi";
            this.panelNavi.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.panelNavi.SizableBottom = false;
            this.panelNavi.SizableLeft = false;
            this.panelNavi.SizableRight = true;
            this.panelNavi.SizableTop = false;
            this.panelNavi.Size = new System.Drawing.Size(218, 409);
            this.panelNavi.TabIndex = 5;
            this.panelNavi.Visible = false;
            // 
            // navPanelControl
            // 
            this.navPanelControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.navPanelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navPanelControl.Location = new System.Drawing.Point(0, 0);
            this.navPanelControl.Name = "navPanelControl";
            this.navPanelControl.Size = new System.Drawing.Size(213, 409);
            this.navPanelControl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1247, 546);
            this.Controls.Add(this.panelNavi);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.menuMain);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuMain;
            this.Name = "MainForm";
            this.Text = "TRADE#";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.MdiChildActivate += new System.EventHandler(this.MainFormMdiChildActivate);
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.MainFormHelpRequested);
            this.Resize += new System.EventHandler(this.MainFormResize);
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.panelTools.ResumeLayout(false);
            this.panelCommonTools.ResumeLayout(false);
            this.panelChartTools.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            this.splitContainerStatus.Panel1.ResumeLayout(false);
            this.splitContainerStatus.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStatus)).EndInit();
            this.splitContainerStatus.ResumeLayout(false);
            this.panelConnect.ResumeLayout(false);
            this.panelConnect.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.contextMenuTray.ResumeLayout(false);
            this.panelNavi.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuMain;
        private System.Windows.Forms.Panel panelTools;
        private TradeSharp.UI.Util.Control.SizeablePanel panelStatus;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuWindows;
        private System.Windows.Forms.ToolStripMenuItem menuItemCascade;
        private System.Windows.Forms.ToolStripMenuItem menuItemVertical;
        private System.Windows.Forms.ToolStripMenuItem menuItemHorizontal;
        private System.Windows.Forms.ToolStripMenuItem menuItemMinimizeAll;
        private System.Windows.Forms.ToolStripMenuItem menuNewChart;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.Button btnNewChart;
        private System.Windows.Forms.ComboBox cbTimeFrame;
        private System.Windows.Forms.ImageList lstGlyth;
        private System.Windows.Forms.ToolStripMenuItem menuSettings;
        private System.Windows.Forms.ToolStripMenuItem menuItemSettingsChart;
        private System.Windows.Forms.ImageList lstGlyph32;
        private System.Windows.Forms.ToolStripMenuItem menuSave;
        private System.Windows.Forms.ToolStripMenuItem торговляToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemNewOrder;
        private System.Windows.Forms.SplitContainer splitContainerStatus;
        private System.Windows.Forms.ToolStripMenuItem menuitemCommonParams;
        private System.Windows.Forms.ToolStripMenuItem menuItemQuoteArchive;
        private System.Windows.Forms.ToolStripMenuItem menuSaveWorkspace;
        private System.Windows.Forms.SaveFileDialog dlgWorkspaceSave;
        private System.Windows.Forms.ToolStripMenuItem menuLoadWorkspace;
        private System.Windows.Forms.OpenFileDialog dlgWorkspaceLoad;
        private System.Windows.Forms.ToolStripMenuItem menuItemRobotsSetup;
        private System.Windows.Forms.ToolStripMenuItem menuItemAccountStat;
        private System.Windows.Forms.ToolStripMenuItem menuitemJournals;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.Button btnLoadConfig;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.ToolStripMenuItem видToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuitemViewAccount;
        private System.Windows.Forms.ToolStripMenuItem menuitemLiveQuotes;
        private System.Windows.Forms.ToolStripMenuItem menutopicHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Panel panelCommonTools;
        private System.Windows.Forms.Button btnFlipPanelCommonTools;
        private System.Windows.Forms.Panel panelChartTools;
        private System.Windows.Forms.Button btnFlipPanelChartTools;
        private System.Windows.Forms.Button toolBtnCross;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelConnect;
        private System.Windows.Forms.ToolStripMenuItem menuitemManageAccounts;
        private System.Windows.Forms.ToolStripMenuItem menuitemTradeSignals;
        private System.Windows.Forms.ToolStripMenuItem menuitemStatusBar;
        private System.Windows.Forms.Button btnShrinkStatusPanel;
        private System.Windows.Forms.Button btnExpandStatusPanel;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem menuitemScripts;
        private System.Windows.Forms.Button btnChat;
        private System.Windows.Forms.ToolStripMenuItem menuitemToolPanel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuitemAccount;
        private System.Windows.Forms.ToolStripMenuItem menuitemSelectAccount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuitemLogout;
        private System.Windows.Forms.ImageList imgListMenu;
        private System.Windows.Forms.ToolStripMenuItem menuitemScript;
        private System.Windows.Forms.ToolStripMenuItem menuitemShowHelp;
        private System.Windows.Forms.ToolStripMenuItem menuitemShowAbout;
        private System.Windows.Forms.ToolStripMenuItem menuitemRobotTester;
        private System.Windows.Forms.ToolStripMenuItem menuitemRobotPortfolio;
        private System.Windows.Forms.ToolStripMenuItem menuitemRobotState;
        private System.Windows.Forms.ToolStripMenuItem menuitemPrint;
        private System.Windows.Forms.ToolStripMenuItem menuMinimizeInTray;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuTray;
        private System.Windows.Forms.ToolStripMenuItem menuTrayRestore;
        private System.Windows.Forms.ToolStripMenuItem menuTrayQuit;
        private System.Windows.Forms.ToolStripMenuItem menuTrayCancel;
        private System.Windows.Forms.ToolStripMenuItem menuitemNavPane;
        private UI.Util.Control.SizeablePanel panelNavi;
        private Controls.NavPanel.NavPanelControl navPanelControl;
        private System.Windows.Forms.ToolStripMenuItem menuTemplates;
        private System.Windows.Forms.ToolStripMenuItem menuSaveTemplate;
        private System.Windows.Forms.ToolStripMenuItem menuApplyTemplate;
        private System.Windows.Forms.ToolStripMenuItem menuManageTemplate;
        private ToolStripMenuItem menuitemRiskSetup;
        private BL.RichTextBoxEx tbStatus;
        private Panel panelStatusRight;
        private Controls.Bookmark.BookmarkStripControl bookmarkStrip;
        private ToolStripMenuItem menuitemHelpTooltip;
        private Button btnShowTradeSignals;
        private ToolStripMenuItem menuitemWallet;
        private Controls.ConnectionStatusImageControl connectionStatusImage;
        private LinkLabel lblConnectStatus;
        private Label lblAccountData;
        private ToolStripMenuItem обозревательToolStripMenuItem;
        private ToolTip toolTipStatus;
    }
}

