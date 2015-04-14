namespace TradeSharp.Client.Forms
{
    partial class OptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            this.ApplyBtn = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.btnBrowserRegistration = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbGapHistoryHours = new System.Windows.Forms.TextBox();
            this.cbHideInTray = new System.Windows.Forms.CheckBox();
            this.cbEnableExtendedThemes = new System.Windows.Forms.CheckBox();
            this.cbSyncAllSymbols = new System.Windows.Forms.CheckBox();
            this.cbSynchGraphics = new System.Windows.Forms.CheckBox();
            this.tabPageChat = new System.Windows.Forms.TabPage();
            this.gridEvents = new FastGrid.FastGrid();
            this.panel4 = new System.Windows.Forms.Panel();
            this.cbDeleteEventsOnRead = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPageSounds = new System.Windows.Forms.TabPage();
            this.btnSounds = new System.Windows.Forms.Button();
            this.tabPageCandle = new System.Windows.Forms.TabPage();
            this.gridCandles = new FastGrid.FastGrid();
            this.panelCandle = new System.Windows.Forms.Panel();
            this.buttonCandleRemove = new System.Windows.Forms.Button();
            this.buttonCandleChange = new System.Windows.Forms.Button();
            this.buttonCandleAdd = new System.Windows.Forms.Button();
            this.tabPageChartIcons = new System.Windows.Forms.TabPage();
            this.chartIconSetupControl = new TradeSharp.Client.Controls.ChartIconSetupControl();
            this.tabPageTrade = new System.Windows.Forms.TabPage();
            this.gridTradeSettings = new FastGrid.FastGrid();
            this.tabPageHotKeys = new System.Windows.Forms.TabPage();
            this.hotKeysTableControl = new TradeSharp.Client.Controls.HotKeysTableControl();
            this.tabPagePrompt = new System.Windows.Forms.TabPage();
            this.cbConfirmGapFill = new System.Windows.Forms.CheckBox();
            this.cbConfirmTerminalClosing = new System.Windows.Forms.CheckBox();
            this.cbCloseConfirm = new System.Windows.Forms.CheckBox();
            this.contextMenuAction = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.imageListSetsGrid = new System.Windows.Forms.ImageList(this.components);
            this.toolTipBrowserRegistration = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabControl.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageChat.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabPageSounds.SuspendLayout();
            this.tabPageCandle.SuspendLayout();
            this.panelCandle.SuspendLayout();
            this.tabPageChartIcons.SuspendLayout();
            this.tabPageTrade.SuspendLayout();
            this.tabPageHotKeys.SuspendLayout();
            this.tabPagePrompt.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            this.CancelBtn.AutoSize = true;
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(409, 3);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 2;
            this.CancelBtn.Tag = "TitleCancel";
            this.CancelBtn.Text = "Отмена";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtnClick);
            // 
            // OkBtn
            // 
            this.OkBtn.AutoSize = true;
            this.OkBtn.Location = new System.Drawing.Point(328, 3);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(75, 23);
            this.OkBtn.TabIndex = 1;
            this.OkBtn.Tag = "TitleOK";
            this.OkBtn.Text = "ОК";
            this.OkBtn.UseVisualStyleBackColor = true;
            this.OkBtn.Click += new System.EventHandler(this.OkBtnClick);
            // 
            // ApplyBtn
            // 
            this.ApplyBtn.AutoSize = true;
            this.ApplyBtn.Location = new System.Drawing.Point(490, 3);
            this.ApplyBtn.Name = "ApplyBtn";
            this.ApplyBtn.Size = new System.Drawing.Size(75, 23);
            this.ApplyBtn.TabIndex = 0;
            this.ApplyBtn.Tag = "TitleApply";
            this.ApplyBtn.Text = "Применить";
            this.ApplyBtn.UseVisualStyleBackColor = true;
            this.ApplyBtn.Click += new System.EventHandler(this.ApplyBtnClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageMain);
            this.tabControl.Controls.Add(this.tabPageChat);
            this.tabControl.Controls.Add(this.tabPageSounds);
            this.tabControl.Controls.Add(this.tabPageCandle);
            this.tabControl.Controls.Add(this.tabPageChartIcons);
            this.tabControl.Controls.Add(this.tabPageTrade);
            this.tabControl.Controls.Add(this.tabPageHotKeys);
            this.tabControl.Controls.Add(this.tabPagePrompt);
            this.tabControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(568, 272);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.btnBrowserRegistration);
            this.tabPageMain.Controls.Add(this.label3);
            this.tabPageMain.Controls.Add(this.tbGapHistoryHours);
            this.tabPageMain.Controls.Add(this.cbHideInTray);
            this.tabPageMain.Controls.Add(this.cbEnableExtendedThemes);
            this.tabPageMain.Controls.Add(this.cbSyncAllSymbols);
            this.tabPageMain.Controls.Add(this.cbSynchGraphics);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(560, 246);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Tag = "TitleMain";
            this.tabPageMain.Text = "Основные";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // btnBrowserRegistration
            // 
            this.btnBrowserRegistration.Location = new System.Drawing.Point(6, 100);
            this.btnBrowserRegistration.Name = "btnBrowserRegistration";
            this.btnBrowserRegistration.Size = new System.Drawing.Size(195, 23);
            this.btnBrowserRegistration.TabIndex = 15;
            this.btnBrowserRegistration.Tag = "TitleT#ObserverRegistration";
            this.btnBrowserRegistration.Text = "Регистрация обозревателя T#";
            this.btnBrowserRegistration.UseVisualStyleBackColor = true;
            this.btnBrowserRegistration.Click += new System.EventHandler(this.BtnBrowserRegistrationClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(65, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(169, 13);
            this.label3.TabIndex = 14;
            this.label3.Tag = "TitleReloadQuotesInHoursSmall";
            this.label3.Text = "перечитывать котировки, часов";
            // 
            // tbGapHistoryHours
            // 
            this.tbGapHistoryHours.Location = new System.Drawing.Point(6, 74);
            this.tbGapHistoryHours.Name = "tbGapHistoryHours";
            this.tbGapHistoryHours.Size = new System.Drawing.Size(53, 20);
            this.tbGapHistoryHours.TabIndex = 13;
            this.tbGapHistoryHours.Text = "480";
            // 
            // cbHideInTray
            // 
            this.cbHideInTray.AutoSize = true;
            this.cbHideInTray.Location = new System.Drawing.Point(6, 51);
            this.cbHideInTray.Name = "cbHideInTray";
            this.cbHideInTray.Size = new System.Drawing.Size(125, 17);
            this.cbHideInTray.TabIndex = 12;
            this.cbHideInTray.Tag = "TitleHideToTraySmall";
            this.cbHideInTray.Text = "сворачивать в трей";
            this.cbHideInTray.UseVisualStyleBackColor = true;
            // 
            // cbEnableExtendedThemes
            // 
            this.cbEnableExtendedThemes.AutoSize = true;
            this.cbEnableExtendedThemes.Location = new System.Drawing.Point(6, 29);
            this.cbEnableExtendedThemes.Name = "cbEnableExtendedThemes";
            this.cbEnableExtendedThemes.Size = new System.Drawing.Size(251, 17);
            this.cbEnableExtendedThemes.TabIndex = 2;
            this.cbEnableExtendedThemes.Tag = "TitleAllowExtentedDecorThemesSmall";
            this.cbEnableExtendedThemes.Text = "разрешить расширенные темы оформления";
            this.cbEnableExtendedThemes.UseVisualStyleBackColor = true;
            // 
            // cbSyncAllSymbols
            // 
            this.cbSyncAllSymbols.AutoSize = true;
            this.cbSyncAllSymbols.Location = new System.Drawing.Point(167, 6);
            this.cbSyncAllSymbols.Name = "cbSyncAllSymbols";
            this.cbSyncAllSymbols.Size = new System.Drawing.Size(99, 17);
            this.cbSyncAllSymbols.TabIndex = 1;
            this.cbSyncAllSymbols.Tag = "TitleAllSymbolsSmall";
            this.cbSyncAllSymbols.Text = "(все символы)";
            this.cbSyncAllSymbols.UseVisualStyleBackColor = true;
            // 
            // cbSynchGraphics
            // 
            this.cbSynchGraphics.AutoSize = true;
            this.cbSynchGraphics.Location = new System.Drawing.Point(6, 6);
            this.cbSynchGraphics.Name = "cbSynchGraphics";
            this.cbSynchGraphics.Size = new System.Drawing.Size(155, 17);
            this.cbSynchGraphics.TabIndex = 0;
            this.cbSynchGraphics.Tag = "TitleChartSynchronizationSmall";
            this.cbSynchGraphics.Text = "синхронизация графиков";
            this.cbSynchGraphics.UseVisualStyleBackColor = true;
            // 
            // tabPageChat
            // 
            this.tabPageChat.Controls.Add(this.gridEvents);
            this.tabPageChat.Controls.Add(this.panel4);
            this.tabPageChat.Location = new System.Drawing.Point(4, 22);
            this.tabPageChat.Name = "tabPageChat";
            this.tabPageChat.Size = new System.Drawing.Size(560, 246);
            this.tabPageChat.TabIndex = 1;
            this.tabPageChat.Tag = "TitleMessages";
            this.tabPageChat.Text = "Сообщения";
            this.tabPageChat.UseVisualStyleBackColor = true;
            // 
            // gridEvents
            // 
            this.gridEvents.CaptionHeight = 20;
            this.gridEvents.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridEvents.CellHeight = 18;
            this.gridEvents.CellPadding = 5;
            this.gridEvents.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridEvents.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridEvents.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridEvents.ColorCellFont = System.Drawing.Color.Black;
            this.gridEvents.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridEvents.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridEvents.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridEvents.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEvents.FitWidth = true;
            this.gridEvents.FontAnchoredRow = null;
            this.gridEvents.FontCell = null;
            this.gridEvents.FontHeader = null;
            this.gridEvents.FontSelectedCell = null;
            this.gridEvents.Location = new System.Drawing.Point(0, 30);
            this.gridEvents.MinimumTableWidth = null;
            this.gridEvents.MultiSelectEnabled = false;
            this.gridEvents.Name = "gridEvents";
            this.gridEvents.SelectEnabled = true;
            this.gridEvents.Size = new System.Drawing.Size(560, 216);
            this.gridEvents.StickFirst = false;
            this.gridEvents.StickLast = false;
            this.gridEvents.TabIndex = 5;
            this.gridEvents.UserHitCell += new FastGrid.UserHitCellDel(this.GridEventsUserHitCell);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.cbDeleteEventsOnRead);
            this.panel4.Controls.Add(this.label2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(560, 30);
            this.panel4.TabIndex = 4;
            // 
            // cbDeleteEventsOnRead
            // 
            this.cbDeleteEventsOnRead.AutoSize = true;
            this.cbDeleteEventsOnRead.Location = new System.Drawing.Point(101, 8);
            this.cbDeleteEventsOnRead.Name = "cbDeleteEventsOnRead";
            this.cbDeleteEventsOnRead.Size = new System.Drawing.Size(135, 17);
            this.cbDeleteEventsOnRead.TabIndex = 1;
            this.cbDeleteEventsOnRead.Tag = "TitleRemoveReadSmall";
            this.cbDeleteEventsOnRead.Text = "удалять прочитанные";
            this.cbDeleteEventsOnRead.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 0;
            this.label2.Tag = "TitleNotices";
            this.label2.Text = "Уведомления";
            // 
            // tabPageSounds
            // 
            this.tabPageSounds.Controls.Add(this.btnSounds);
            this.tabPageSounds.Location = new System.Drawing.Point(4, 22);
            this.tabPageSounds.Name = "tabPageSounds";
            this.tabPageSounds.Size = new System.Drawing.Size(560, 246);
            this.tabPageSounds.TabIndex = 2;
            this.tabPageSounds.Tag = "TitleSounds";
            this.tabPageSounds.Text = "Звуки";
            this.tabPageSounds.UseVisualStyleBackColor = true;
            // 
            // btnSounds
            // 
            this.btnSounds.Location = new System.Drawing.Point(8, 7);
            this.btnSounds.Name = "btnSounds";
            this.btnSounds.Size = new System.Drawing.Size(134, 23);
            this.btnSounds.TabIndex = 0;
            this.btnSounds.Tag = "TitleConfigureSoundsMenu";
            this.btnSounds.Text = "Настроить звуки...";
            this.btnSounds.UseVisualStyleBackColor = true;
            this.btnSounds.Click += new System.EventHandler(this.BtnSoundsClick);
            // 
            // tabPageCandle
            // 
            this.tabPageCandle.Controls.Add(this.gridCandles);
            this.tabPageCandle.Controls.Add(this.panelCandle);
            this.tabPageCandle.Location = new System.Drawing.Point(4, 22);
            this.tabPageCandle.Name = "tabPageCandle";
            this.tabPageCandle.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCandle.Size = new System.Drawing.Size(560, 246);
            this.tabPageCandle.TabIndex = 3;
            this.tabPageCandle.Tag = "TitleCandles";
            this.tabPageCandle.Text = "Свечи";
            this.tabPageCandle.UseVisualStyleBackColor = true;
            // 
            // gridCandles
            // 
            this.gridCandles.CaptionHeight = 20;
            this.gridCandles.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridCandles.CellHeight = 18;
            this.gridCandles.CellPadding = 5;
            this.gridCandles.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridCandles.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridCandles.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridCandles.ColorCellFont = System.Drawing.Color.Black;
            this.gridCandles.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridCandles.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridCandles.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridCandles.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridCandles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCandles.FitWidth = false;
            this.gridCandles.FontAnchoredRow = null;
            this.gridCandles.FontCell = null;
            this.gridCandles.FontHeader = null;
            this.gridCandles.FontSelectedCell = null;
            this.gridCandles.Location = new System.Drawing.Point(3, 3);
            this.gridCandles.MinimumTableWidth = null;
            this.gridCandles.MultiSelectEnabled = false;
            this.gridCandles.Name = "gridCandles";
            this.gridCandles.SelectEnabled = true;
            this.gridCandles.Size = new System.Drawing.Size(525, 240);
            this.gridCandles.StickFirst = false;
            this.gridCandles.StickLast = false;
            this.gridCandles.TabIndex = 3;
            this.gridCandles.UserHitCell += new FastGrid.UserHitCellDel(this.GridCandlesUserHitCell);
            this.gridCandles.SelectionChanged += new FastGrid.SelectedChangedDel(this.GridCandlesSelectionChanged);
            // 
            // panelCandle
            // 
            this.panelCandle.AutoSize = true;
            this.panelCandle.Controls.Add(this.buttonCandleRemove);
            this.panelCandle.Controls.Add(this.buttonCandleChange);
            this.panelCandle.Controls.Add(this.buttonCandleAdd);
            this.panelCandle.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelCandle.Location = new System.Drawing.Point(528, 3);
            this.panelCandle.Name = "panelCandle";
            this.panelCandle.Size = new System.Drawing.Size(29, 240);
            this.panelCandle.TabIndex = 2;
            // 
            // buttonCandleRemove
            // 
            this.buttonCandleRemove.Enabled = false;
            this.buttonCandleRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCandleRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonCandleRemove.Image")));
            this.buttonCandleRemove.Location = new System.Drawing.Point(3, 61);
            this.buttonCandleRemove.Name = "buttonCandleRemove";
            this.buttonCandleRemove.Size = new System.Drawing.Size(23, 23);
            this.buttonCandleRemove.TabIndex = 2;
            this.buttonCandleRemove.UseVisualStyleBackColor = true;
            this.buttonCandleRemove.Click += new System.EventHandler(this.ButtonCandleRemoveClick);
            // 
            // buttonCandleChange
            // 
            this.buttonCandleChange.Enabled = false;
            this.buttonCandleChange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCandleChange.Image = ((System.Drawing.Image)(resources.GetObject("buttonCandleChange.Image")));
            this.buttonCandleChange.Location = new System.Drawing.Point(3, 32);
            this.buttonCandleChange.Name = "buttonCandleChange";
            this.buttonCandleChange.Size = new System.Drawing.Size(23, 23);
            this.buttonCandleChange.TabIndex = 1;
            this.buttonCandleChange.UseVisualStyleBackColor = true;
            this.buttonCandleChange.Click += new System.EventHandler(this.ButtonCandleChangeClick);
            // 
            // buttonCandleAdd
            // 
            this.buttonCandleAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCandleAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonCandleAdd.Image")));
            this.buttonCandleAdd.Location = new System.Drawing.Point(3, 3);
            this.buttonCandleAdd.Name = "buttonCandleAdd";
            this.buttonCandleAdd.Size = new System.Drawing.Size(23, 23);
            this.buttonCandleAdd.TabIndex = 0;
            this.buttonCandleAdd.UseVisualStyleBackColor = true;
            this.buttonCandleAdd.Click += new System.EventHandler(this.ButtonCandleAddClick);
            // 
            // tabPageChartIcons
            // 
            this.tabPageChartIcons.Controls.Add(this.chartIconSetupControl);
            this.tabPageChartIcons.Location = new System.Drawing.Point(4, 22);
            this.tabPageChartIcons.Name = "tabPageChartIcons";
            this.tabPageChartIcons.Size = new System.Drawing.Size(560, 246);
            this.tabPageChartIcons.TabIndex = 4;
            this.tabPageChartIcons.Tag = "TitleQuickCall";
            this.tabPageChartIcons.Text = "Быстрый вызов";
            this.tabPageChartIcons.UseVisualStyleBackColor = true;
            // 
            // chartIconSetupControl
            // 
            this.chartIconSetupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartIconSetupControl.Location = new System.Drawing.Point(0, 0);
            this.chartIconSetupControl.Name = "chartIconSetupControl";
            this.chartIconSetupControl.Size = new System.Drawing.Size(560, 246);
            this.chartIconSetupControl.TabIndex = 0;
            // 
            // tabPageTrade
            // 
            this.tabPageTrade.Controls.Add(this.gridTradeSettings);
            this.tabPageTrade.Location = new System.Drawing.Point(4, 22);
            this.tabPageTrade.Name = "tabPageTrade";
            this.tabPageTrade.Size = new System.Drawing.Size(560, 246);
            this.tabPageTrade.TabIndex = 5;
            this.tabPageTrade.Tag = "TitleTrade";
            this.tabPageTrade.Text = "Торговля";
            this.tabPageTrade.UseVisualStyleBackColor = true;
            // 
            // gridTradeSettings
            // 
            this.gridTradeSettings.CaptionHeight = 20;
            this.gridTradeSettings.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridTradeSettings.CellHeight = 18;
            this.gridTradeSettings.CellPadding = 5;
            this.gridTradeSettings.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridTradeSettings.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridTradeSettings.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridTradeSettings.ColorCellFont = System.Drawing.Color.Black;
            this.gridTradeSettings.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridTradeSettings.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridTradeSettings.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridTradeSettings.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridTradeSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTradeSettings.FitWidth = true;
            this.gridTradeSettings.FontAnchoredRow = null;
            this.gridTradeSettings.FontCell = null;
            this.gridTradeSettings.FontHeader = null;
            this.gridTradeSettings.FontSelectedCell = null;
            this.gridTradeSettings.Location = new System.Drawing.Point(0, 0);
            this.gridTradeSettings.MinimumTableWidth = null;
            this.gridTradeSettings.MultiSelectEnabled = false;
            this.gridTradeSettings.Name = "gridTradeSettings";
            this.gridTradeSettings.SelectEnabled = true;
            this.gridTradeSettings.Size = new System.Drawing.Size(560, 246);
            this.gridTradeSettings.StickFirst = false;
            this.gridTradeSettings.StickLast = false;
            this.gridTradeSettings.TabIndex = 0;
            // 
            // tabPageHotKeys
            // 
            this.tabPageHotKeys.Controls.Add(this.hotKeysTableControl);
            this.tabPageHotKeys.Location = new System.Drawing.Point(4, 22);
            this.tabPageHotKeys.Name = "tabPageHotKeys";
            this.tabPageHotKeys.Size = new System.Drawing.Size(560, 246);
            this.tabPageHotKeys.TabIndex = 6;
            this.tabPageHotKeys.Tag = "TitleHotKeys";
            this.tabPageHotKeys.Text = "Горячие клавиши";
            this.tabPageHotKeys.UseVisualStyleBackColor = true;
            // 
            // hotKeysTableControl
            // 
            this.hotKeysTableControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hotKeysTableControl.Location = new System.Drawing.Point(0, 0);
            this.hotKeysTableControl.Name = "hotKeysTableControl";
            this.hotKeysTableControl.Size = new System.Drawing.Size(560, 246);
            this.hotKeysTableControl.TabIndex = 0;
            // 
            // tabPagePrompt
            // 
            this.tabPagePrompt.Controls.Add(this.cbConfirmGapFill);
            this.tabPagePrompt.Controls.Add(this.cbConfirmTerminalClosing);
            this.tabPagePrompt.Controls.Add(this.cbCloseConfirm);
            this.tabPagePrompt.Location = new System.Drawing.Point(4, 22);
            this.tabPagePrompt.Name = "tabPagePrompt";
            this.tabPagePrompt.Size = new System.Drawing.Size(560, 246);
            this.tabPagePrompt.TabIndex = 7;
            this.tabPagePrompt.Tag = "TitleNotices";
            this.tabPagePrompt.Text = "Уведомления";
            this.tabPagePrompt.UseVisualStyleBackColor = true;
            // 
            // cbConfirmGapFill
            // 
            this.cbConfirmGapFill.AutoSize = true;
            this.cbConfirmGapFill.Checked = true;
            this.cbConfirmGapFill.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConfirmGapFill.Location = new System.Drawing.Point(8, 51);
            this.cbConfirmGapFill.Name = "cbConfirmGapFill";
            this.cbConfirmGapFill.Size = new System.Drawing.Size(187, 17);
            this.cbConfirmGapFill.TabIndex = 14;
            this.cbConfirmGapFill.Tag = "TitleConfirmGapFillingSmall";
            this.cbConfirmGapFill.Text = "подтверждать заполнение гэпа";
            this.cbConfirmGapFill.UseVisualStyleBackColor = true;
            // 
            // cbConfirmTerminalClosing
            // 
            this.cbConfirmTerminalClosing.AutoSize = true;
            this.cbConfirmTerminalClosing.Checked = true;
            this.cbConfirmTerminalClosing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConfirmTerminalClosing.Location = new System.Drawing.Point(8, 28);
            this.cbConfirmTerminalClosing.Name = "cbConfirmTerminalClosing";
            this.cbConfirmTerminalClosing.Size = new System.Drawing.Size(208, 17);
            this.cbConfirmTerminalClosing.TabIndex = 13;
            this.cbConfirmTerminalClosing.Tag = "TitleConfirmTerminalClosingSmall";
            this.cbConfirmTerminalClosing.Text = "подтверждать закрытие терминала";
            this.cbConfirmTerminalClosing.UseVisualStyleBackColor = true;
            // 
            // cbCloseConfirm
            // 
            this.cbCloseConfirm.AutoSize = true;
            this.cbCloseConfirm.Checked = true;
            this.cbCloseConfirm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCloseConfirm.Location = new System.Drawing.Point(8, 5);
            this.cbCloseConfirm.Name = "cbCloseConfirm";
            this.cbCloseConfirm.Size = new System.Drawing.Size(196, 17);
            this.cbCloseConfirm.TabIndex = 12;
            this.cbCloseConfirm.Tag = "TitleConfirmChartClosingSmall";
            this.cbCloseConfirm.Text = "подтверждать закрытие графика";
            this.cbCloseConfirm.UseVisualStyleBackColor = true;
            // 
            // contextMenuAction
            // 
            this.contextMenuAction.Name = "contextMenuAction";
            this.contextMenuAction.Size = new System.Drawing.Size(61, 4);
            // 
            // imageListSetsGrid
            // 
            this.imageListSetsGrid.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSetsGrid.ImageStream")));
            this.imageListSetsGrid.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSetsGrid.Images.SetKeyName(0, "ico edit.png");
            this.imageListSetsGrid.Images.SetKeyName(1, "ico ok sign.png");
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.ApplyBtn);
            this.flowLayoutPanel1.Controls.Add(this.CancelBtn);
            this.flowLayoutPanel1.Controls.Add(this.OkBtn);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 272);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(568, 29);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.OkBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(568, 301);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleParameters";
            this.Text = "Параметры";
            this.Load += new System.EventHandler(this.OptionsFormLoad);
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.OptionsFormHelpRequested);
            this.tabControl.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageMain.PerformLayout();
            this.tabPageChat.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.tabPageSounds.ResumeLayout(false);
            this.tabPageCandle.ResumeLayout(false);
            this.tabPageCandle.PerformLayout();
            this.panelCandle.ResumeLayout(false);
            this.tabPageChartIcons.ResumeLayout(false);
            this.tabPageTrade.ResumeLayout(false);
            this.tabPageHotKeys.ResumeLayout(false);
            this.tabPagePrompt.ResumeLayout(false);
            this.tabPagePrompt.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button ApplyBtn;
        private System.Windows.Forms.CheckBox cbSynchGraphics;
        private System.Windows.Forms.CheckBox cbSyncAllSymbols;
        private System.Windows.Forms.CheckBox cbEnableExtendedThemes;
        private System.Windows.Forms.TabPage tabPageChat;
        private System.Windows.Forms.TabPage tabPageSounds;
        private System.Windows.Forms.Button btnSounds;
        private FastGrid.FastGrid gridEvents;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ContextMenuStrip contextMenuAction;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbDeleteEventsOnRead;
        private System.Windows.Forms.TabPage tabPageCandle;
        private System.Windows.Forms.Button buttonCandleAdd;
        private System.Windows.Forms.Button buttonCandleChange;
        private System.Windows.Forms.Button buttonCandleRemove;
        private System.Windows.Forms.Panel panelCandle;
        private FastGrid.FastGrid gridCandles;
        private System.Windows.Forms.CheckBox cbHideInTray;
        private System.Windows.Forms.TabPage tabPageChartIcons;
        private Controls.ChartIconSetupControl chartIconSetupControl;
        private System.Windows.Forms.TabPage tabPageTrade;
        private FastGrid.FastGrid gridTradeSettings;
        private System.Windows.Forms.ImageList imageListSetsGrid;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbGapHistoryHours;
        private System.Windows.Forms.TabPage tabPageHotKeys;
        private Controls.HotKeysTableControl hotKeysTableControl;
        private System.Windows.Forms.Button btnBrowserRegistration;
        private System.Windows.Forms.ToolTip toolTipBrowserRegistration;
        private System.Windows.Forms.TabPage tabPagePrompt;
        private System.Windows.Forms.CheckBox cbConfirmGapFill;
        private System.Windows.Forms.CheckBox cbConfirmTerminalClosing;
        private System.Windows.Forms.CheckBox cbCloseConfirm;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}