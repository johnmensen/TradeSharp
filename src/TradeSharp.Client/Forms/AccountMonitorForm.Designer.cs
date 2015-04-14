namespace TradeSharp.Client.Forms
{
    partial class AccountMonitorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountMonitorForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageAccount = new System.Windows.Forms.TabPage();
            this.dgAccount = new FastGrid.FastGrid();
            this.panelAcTop = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStat = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnEstimateRisk = new System.Windows.Forms.Button();
            this.pageOpenOrders = new System.Windows.Forms.TabPage();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.gridOpenPos = new FastGrid.FastGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgSummary = new FastGrid.FastGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ClosePositionBtn = new System.Windows.Forms.Button();
            this.CloseAllBtn = new System.Windows.Forms.Button();
            this.EditOrderBtn = new System.Windows.Forms.Button();
            this.NewOrderBtn = new System.Windows.Forms.Button();
            this.pageDelayedOrders = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.gridPendingOrders = new FastGrid.FastGrid();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnDeletePendingOrder = new System.Windows.Forms.Button();
            this.btnDeletePendingOrders = new System.Windows.Forms.Button();
            this.btnEditPendingOrder = new System.Windows.Forms.Button();
            this.btnNewPendingOrder = new System.Windows.Forms.Button();
            this.pageHistoryAccount = new System.Windows.Forms.TabPage();
            this.panelHistoryOrdersLoadingStatus = new TradeSharp.UI.Util.Control.StandByControl();
            this.accountHistoryCtrl = new TradeSharp.Client.Controls.AccountHistoryControl();
            this.contextMenuSummaryOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemCloseAll1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemConfigColumns1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemCloseOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemModifyOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSetTrailing = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemConfigColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenuPending = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuNewOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuDeleteOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuChangeOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuDeleteAllOrders = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemConfigColumns2 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl.SuspendLayout();
            this.pageAccount.SuspendLayout();
            this.panelAcTop.SuspendLayout();
            this.pageOpenOrders.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pageDelayedOrders.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pageHistoryAccount.SuspendLayout();
            this.contextMenuSummaryOrder.SuspendLayout();
            this.contextMenuOrder.SuspendLayout();
            this.contextMenuPending.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageAccount);
            this.tabControl.Controls.Add(this.pageOpenOrders);
            this.tabControl.Controls.Add(this.pageDelayedOrders);
            this.tabControl.Controls.Add(this.pageHistoryAccount);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(809, 328);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControlSelectedIndexChanged);
            // 
            // pageAccount
            // 
            this.pageAccount.Controls.Add(this.dgAccount);
            this.pageAccount.Controls.Add(this.panelAcTop);
            this.pageAccount.Location = new System.Drawing.Point(4, 22);
            this.pageAccount.Name = "pageAccount";
            this.pageAccount.Padding = new System.Windows.Forms.Padding(3);
            this.pageAccount.Size = new System.Drawing.Size(801, 302);
            this.pageAccount.TabIndex = 0;
            this.pageAccount.Tag = "TitleBalance";
            this.pageAccount.Text = "Баланс";
            this.pageAccount.UseVisualStyleBackColor = true;
            // 
            // dgAccount
            // 
            this.dgAccount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgAccount.CaptionHeight = 0;
            this.dgAccount.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.dgAccount.CellHeight = 18;
            this.dgAccount.CellPadding = 5;
            this.dgAccount.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.dgAccount.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.dgAccount.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.dgAccount.ColorCellFont = System.Drawing.Color.Black;
            this.dgAccount.ColorCellOutlineLower = System.Drawing.Color.White;
            this.dgAccount.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.dgAccount.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.dgAccount.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.dgAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgAccount.FitWidth = true;
            this.dgAccount.FontAnchoredRow = null;
            this.dgAccount.FontCell = null;
            this.dgAccount.FontHeader = null;
            this.dgAccount.FontSelectedCell = null;
            this.dgAccount.Location = new System.Drawing.Point(3, 32);
            this.dgAccount.MinimumTableWidth = null;
            this.dgAccount.MultiSelectEnabled = false;
            this.dgAccount.Name = "dgAccount";
            this.dgAccount.Padding = new System.Windows.Forms.Padding(3);
            this.dgAccount.SelectEnabled = false;
            this.dgAccount.Size = new System.Drawing.Size(795, 267);
            this.dgAccount.StickFirst = false;
            this.dgAccount.StickLast = false;
            this.dgAccount.TabIndex = 5;
            // 
            // panelAcTop
            // 
            this.panelAcTop.AutoSize = true;
            this.panelAcTop.Controls.Add(this.btnStat);
            this.panelAcTop.Controls.Add(this.btnEstimateRisk);
            this.panelAcTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAcTop.Location = new System.Drawing.Point(3, 3);
            this.panelAcTop.Name = "panelAcTop";
            this.panelAcTop.Size = new System.Drawing.Size(795, 29);
            this.panelAcTop.TabIndex = 4;
            // 
            // btnStat
            // 
            this.btnStat.AutoSize = true;
            this.btnStat.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStat.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStat.ImageIndex = 1;
            this.btnStat.ImageList = this.imageList;
            this.btnStat.Location = new System.Drawing.Point(3, 3);
            this.btnStat.Name = "btnStat";
            this.btnStat.Size = new System.Drawing.Size(91, 23);
            this.btnStat.TabIndex = 1;
            this.btnStat.Tag = "TitleStatistics";
            this.btnStat.Text = "Статистика";
            this.btnStat.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnStat.UseVisualStyleBackColor = true;
            this.btnStat.Click += new System.EventHandler(this.BtnStatClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "alert.png");
            this.imageList.Images.SetKeyName(1, "chart_bar01.png");
            // 
            // btnEstimateRisk
            // 
            this.btnEstimateRisk.AutoSize = true;
            this.btnEstimateRisk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnEstimateRisk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEstimateRisk.ImageIndex = 0;
            this.btnEstimateRisk.ImageList = this.imageList;
            this.btnEstimateRisk.Location = new System.Drawing.Point(100, 3);
            this.btnEstimateRisk.Name = "btnEstimateRisk";
            this.btnEstimateRisk.Size = new System.Drawing.Size(104, 23);
            this.btnEstimateRisk.TabIndex = 0;
            this.btnEstimateRisk.Tag = "TitleRiskAssessment";
            this.btnEstimateRisk.Text = "Оценка риска";
            this.btnEstimateRisk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnEstimateRisk.UseVisualStyleBackColor = true;
            this.btnEstimateRisk.Click += new System.EventHandler(this.BtnEstimateRiskClick);
            // 
            // pageOpenOrders
            // 
            this.pageOpenOrders.Controls.Add(this.panel5);
            this.pageOpenOrders.Controls.Add(this.panel1);
            this.pageOpenOrders.Location = new System.Drawing.Point(4, 22);
            this.pageOpenOrders.Name = "pageOpenOrders";
            this.pageOpenOrders.Padding = new System.Windows.Forms.Padding(3);
            this.pageOpenOrders.Size = new System.Drawing.Size(801, 302);
            this.pageOpenOrders.TabIndex = 1;
            this.pageOpenOrders.Tag = "TitlePositions";
            this.pageOpenOrders.Text = "Позиции";
            this.pageOpenOrders.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Controls.Add(this.panel2);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(3, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(795, 259);
            this.panel5.TabIndex = 4;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.gridOpenPos);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 100);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(3);
            this.panel6.Size = new System.Drawing.Size(795, 159);
            this.panel6.TabIndex = 5;
            // 
            // gridOpenPos
            // 
            this.gridOpenPos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridOpenPos.CaptionHeight = 20;
            this.gridOpenPos.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridOpenPos.CellHeight = 18;
            this.gridOpenPos.CellPadding = 5;
            this.gridOpenPos.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridOpenPos.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridOpenPos.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridOpenPos.ColorCellFont = System.Drawing.Color.Black;
            this.gridOpenPos.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridOpenPos.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridOpenPos.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.gridOpenPos.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridOpenPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridOpenPos.FitWidth = true;
            this.gridOpenPos.FontAnchoredRow = null;
            this.gridOpenPos.FontCell = null;
            this.gridOpenPos.FontHeader = null;
            this.gridOpenPos.FontSelectedCell = null;
            this.gridOpenPos.Location = new System.Drawing.Point(3, 3);
            this.gridOpenPos.MinimumTableWidth = null;
            this.gridOpenPos.MultiSelectEnabled = false;
            this.gridOpenPos.Name = "gridOpenPos";
            this.gridOpenPos.SelectEnabled = true;
            this.gridOpenPos.Size = new System.Drawing.Size(789, 153);
            this.gridOpenPos.StickFirst = false;
            this.gridOpenPos.StickLast = false;
            this.gridOpenPos.TabIndex = 0;
            this.gridOpenPos.ContextMenuRequested += new FastGrid.UserHitCellDel(this.GridOpenPosContextMenuRequested);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgSummary);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3);
            this.panel2.Size = new System.Drawing.Size(795, 100);
            this.panel2.TabIndex = 4;
            // 
            // dgSummary
            // 
            this.dgSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgSummary.CaptionHeight = 20;
            this.dgSummary.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.dgSummary.CellHeight = 18;
            this.dgSummary.CellPadding = 5;
            this.dgSummary.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.dgSummary.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.dgSummary.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.dgSummary.ColorCellFont = System.Drawing.Color.Black;
            this.dgSummary.ColorCellOutlineLower = System.Drawing.Color.White;
            this.dgSummary.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.dgSummary.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.dgSummary.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.dgSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgSummary.FitWidth = true;
            this.dgSummary.FontAnchoredRow = null;
            this.dgSummary.FontCell = null;
            this.dgSummary.FontHeader = null;
            this.dgSummary.FontSelectedCell = null;
            this.dgSummary.Location = new System.Drawing.Point(3, 3);
            this.dgSummary.MinimumTableWidth = null;
            this.dgSummary.MultiSelectEnabled = true;
            this.dgSummary.Name = "dgSummary";
            this.dgSummary.SelectEnabled = true;
            this.dgSummary.Size = new System.Drawing.Size(789, 94);
            this.dgSummary.StickFirst = false;
            this.dgSummary.StickLast = true;
            this.dgSummary.TabIndex = 1;
            this.dgSummary.UserHitCell += new FastGrid.UserHitCellDel(this.DgSummaryUserHitCell);
            this.dgSummary.ContextMenuRequested += new FastGrid.UserHitCellDel(this.DgSummaryContextMenuRequested);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ClosePositionBtn);
            this.panel1.Controls.Add(this.CloseAllBtn);
            this.panel1.Controls.Add(this.EditOrderBtn);
            this.panel1.Controls.Add(this.NewOrderBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 262);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(795, 37);
            this.panel1.TabIndex = 2;
            // 
            // ClosePositionBtn
            // 
            this.ClosePositionBtn.Enabled = false;
            this.ClosePositionBtn.Location = new System.Drawing.Point(213, 7);
            this.ClosePositionBtn.Name = "ClosePositionBtn";
            this.ClosePositionBtn.Size = new System.Drawing.Size(93, 23);
            this.ClosePositionBtn.TabIndex = 3;
            this.ClosePositionBtn.Tag = "TitleClose";
            this.ClosePositionBtn.Text = "Закрыть";
            this.ClosePositionBtn.UseVisualStyleBackColor = true;
            this.ClosePositionBtn.Click += new System.EventHandler(this.BtnClosePositionClick);
            // 
            // CloseAllBtn
            // 
            this.CloseAllBtn.Location = new System.Drawing.Point(365, 7);
            this.CloseAllBtn.Name = "CloseAllBtn";
            this.CloseAllBtn.Size = new System.Drawing.Size(244, 23);
            this.CloseAllBtn.TabIndex = 2;
            this.CloseAllBtn.Tag = "TitleCloseAll";
            this.CloseAllBtn.Text = "Закрыть все";
            this.CloseAllBtn.UseVisualStyleBackColor = true;
            this.CloseAllBtn.Click += new System.EventHandler(this.CloseAllBtnClick);
            // 
            // EditOrderBtn
            // 
            this.EditOrderBtn.Enabled = false;
            this.EditOrderBtn.Location = new System.Drawing.Point(114, 7);
            this.EditOrderBtn.Name = "EditOrderBtn";
            this.EditOrderBtn.Size = new System.Drawing.Size(93, 23);
            this.EditOrderBtn.TabIndex = 1;
            this.EditOrderBtn.Tag = "TitleEdit";
            this.EditOrderBtn.Text = "Редактировать";
            this.EditOrderBtn.UseVisualStyleBackColor = true;
            this.EditOrderBtn.Click += new System.EventHandler(this.EditOrderBtnClick);
            // 
            // NewOrderBtn
            // 
            this.NewOrderBtn.Location = new System.Drawing.Point(15, 7);
            this.NewOrderBtn.Name = "NewOrderBtn";
            this.NewOrderBtn.Size = new System.Drawing.Size(93, 23);
            this.NewOrderBtn.TabIndex = 0;
            this.NewOrderBtn.Tag = "TitleNew";
            this.NewOrderBtn.Text = "Новый";
            this.NewOrderBtn.UseVisualStyleBackColor = true;
            this.NewOrderBtn.Click += new System.EventHandler(this.NewOrderBtnClick);
            // 
            // pageDelayedOrders
            // 
            this.pageDelayedOrders.Controls.Add(this.panel4);
            this.pageDelayedOrders.Controls.Add(this.panel3);
            this.pageDelayedOrders.Location = new System.Drawing.Point(4, 22);
            this.pageDelayedOrders.Name = "pageDelayedOrders";
            this.pageDelayedOrders.Size = new System.Drawing.Size(801, 302);
            this.pageDelayedOrders.TabIndex = 2;
            this.pageDelayedOrders.Tag = "TitlePendingOrders";
            this.pageDelayedOrders.Text = "Отложенные ордера";
            this.pageDelayedOrders.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.gridPendingOrders);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(801, 262);
            this.panel4.TabIndex = 2;
            // 
            // gridPendingOrders
            // 
            this.gridPendingOrders.CaptionHeight = 20;
            this.gridPendingOrders.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridPendingOrders.CellHeight = 18;
            this.gridPendingOrders.CellPadding = 5;
            this.gridPendingOrders.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridPendingOrders.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridPendingOrders.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridPendingOrders.ColorCellFont = System.Drawing.Color.Black;
            this.gridPendingOrders.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridPendingOrders.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridPendingOrders.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.gridPendingOrders.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridPendingOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPendingOrders.FitWidth = true;
            this.gridPendingOrders.FontAnchoredRow = null;
            this.gridPendingOrders.FontCell = null;
            this.gridPendingOrders.FontHeader = null;
            this.gridPendingOrders.FontSelectedCell = null;
            this.gridPendingOrders.Location = new System.Drawing.Point(0, 0);
            this.gridPendingOrders.MinimumTableWidth = null;
            this.gridPendingOrders.MultiSelectEnabled = false;
            this.gridPendingOrders.Name = "gridPendingOrders";
            this.gridPendingOrders.SelectEnabled = true;
            this.gridPendingOrders.Size = new System.Drawing.Size(801, 262);
            this.gridPendingOrders.StickFirst = false;
            this.gridPendingOrders.StickLast = false;
            this.gridPendingOrders.TabIndex = 0;
            this.gridPendingOrders.ContextMenuRequested += new FastGrid.UserHitCellDel(this.GridPendingOrdersContextMenuRequested);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnDeletePendingOrder);
            this.panel3.Controls.Add(this.btnDeletePendingOrders);
            this.panel3.Controls.Add(this.btnEditPendingOrder);
            this.panel3.Controls.Add(this.btnNewPendingOrder);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 262);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(801, 40);
            this.panel3.TabIndex = 0;
            // 
            // btnDeletePendingOrder
            // 
            this.btnDeletePendingOrder.Enabled = false;
            this.btnDeletePendingOrder.Location = new System.Drawing.Point(211, 8);
            this.btnDeletePendingOrder.Name = "btnDeletePendingOrder";
            this.btnDeletePendingOrder.Size = new System.Drawing.Size(93, 23);
            this.btnDeletePendingOrder.TabIndex = 6;
            this.btnDeletePendingOrder.Tag = "TitleDelete";
            this.btnDeletePendingOrder.Text = "Удалить";
            this.btnDeletePendingOrder.UseVisualStyleBackColor = true;
            this.btnDeletePendingOrder.Click += new System.EventHandler(this.BtnDeletePendingOrderClick);
            // 
            // btnDeletePendingOrders
            // 
            this.btnDeletePendingOrders.Location = new System.Drawing.Point(368, 8);
            this.btnDeletePendingOrders.Name = "btnDeletePendingOrders";
            this.btnDeletePendingOrders.Size = new System.Drawing.Size(244, 23);
            this.btnDeletePendingOrders.TabIndex = 5;
            this.btnDeletePendingOrders.Tag = "TitleDeleteAll";
            this.btnDeletePendingOrders.Text = "Удалить все ордера";
            this.btnDeletePendingOrders.UseVisualStyleBackColor = true;
            this.btnDeletePendingOrders.Click += new System.EventHandler(this.BtnDeletePendingOrdersClick);
            // 
            // btnEditPendingOrder
            // 
            this.btnEditPendingOrder.Enabled = false;
            this.btnEditPendingOrder.Location = new System.Drawing.Point(112, 8);
            this.btnEditPendingOrder.Name = "btnEditPendingOrder";
            this.btnEditPendingOrder.Size = new System.Drawing.Size(93, 23);
            this.btnEditPendingOrder.TabIndex = 4;
            this.btnEditPendingOrder.Tag = "TitleEdit";
            this.btnEditPendingOrder.Text = "Редактировать";
            this.btnEditPendingOrder.UseVisualStyleBackColor = true;
            this.btnEditPendingOrder.Click += new System.EventHandler(this.BtnEditPendingOrderClick);
            // 
            // btnNewPendingOrder
            // 
            this.btnNewPendingOrder.Location = new System.Drawing.Point(13, 9);
            this.btnNewPendingOrder.Name = "btnNewPendingOrder";
            this.btnNewPendingOrder.Size = new System.Drawing.Size(93, 23);
            this.btnNewPendingOrder.TabIndex = 3;
            this.btnNewPendingOrder.Tag = "TitleNew";
            this.btnNewPendingOrder.Text = "Новый";
            this.btnNewPendingOrder.UseVisualStyleBackColor = true;
            this.btnNewPendingOrder.Click += new System.EventHandler(this.BtnNewPendingOrderClick);
            // 
            // pageHistoryAccount
            // 
            this.pageHistoryAccount.Controls.Add(this.panelHistoryOrdersLoadingStatus);
            this.pageHistoryAccount.Controls.Add(this.accountHistoryCtrl);
            this.pageHistoryAccount.Location = new System.Drawing.Point(4, 22);
            this.pageHistoryAccount.Name = "pageHistoryAccount";
            this.pageHistoryAccount.Size = new System.Drawing.Size(801, 302);
            this.pageHistoryAccount.TabIndex = 3;
            this.pageHistoryAccount.Tag = "TitleAccountHistory";
            this.pageHistoryAccount.Text = "История счета";
            this.pageHistoryAccount.UseVisualStyleBackColor = true;
            // 
            // panelHistoryOrdersLoadingStatus
            // 
            this.panelHistoryOrdersLoadingStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHistoryOrdersLoadingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.panelHistoryOrdersLoadingStatus.IsShown = false;
            this.panelHistoryOrdersLoadingStatus.Location = new System.Drawing.Point(0, 0);
            this.panelHistoryOrdersLoadingStatus.Name = "panelHistoryOrdersLoadingStatus";
            this.panelHistoryOrdersLoadingStatus.Size = new System.Drawing.Size(801, 302);
            this.panelHistoryOrdersLoadingStatus.TabIndex = 1;
            this.panelHistoryOrdersLoadingStatus.Tag = "TitleLoading";
            this.panelHistoryOrdersLoadingStatus.Text = "Загрузка...";
            this.panelHistoryOrdersLoadingStatus.TransparentForm = null;
            this.panelHistoryOrdersLoadingStatus.Visible = false;
            // 
            // accountHistoryCtrl
            // 
            this.accountHistoryCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.accountHistoryCtrl.Location = new System.Drawing.Point(0, 0);
            this.accountHistoryCtrl.Name = "accountHistoryCtrl";
            this.accountHistoryCtrl.Size = new System.Drawing.Size(801, 302);
            this.accountHistoryCtrl.TabIndex = 0;
            // 
            // contextMenuSummaryOrder
            // 
            this.contextMenuSummaryOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCloseAll1,
            this.menuItemConfigColumns1});
            this.contextMenuSummaryOrder.Name = "contextMenuOrder";
            this.contextMenuSummaryOrder.Size = new System.Drawing.Size(187, 48);
            // 
            // menuItemCloseAll1
            // 
            this.menuItemCloseAll1.Name = "menuItemCloseAll1";
            this.menuItemCloseAll1.Size = new System.Drawing.Size(186, 22);
            this.menuItemCloseAll1.Text = "Закрыть все";
            this.menuItemCloseAll1.Click += new System.EventHandler(this.MenuitemCloseAllClick);
            // 
            // menuItemConfigColumns1
            // 
            this.menuItemConfigColumns1.Name = "menuItemConfigColumns1";
            this.menuItemConfigColumns1.Size = new System.Drawing.Size(186, 22);
            this.menuItemConfigColumns1.Text = "Настроить столбцы...";
            this.menuItemConfigColumns1.Click += new System.EventHandler(this.ConfigDgSummaryGridColumns);
            // 
            // contextMenuOrder
            // 
            this.contextMenuOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemCloseOrder,
            this.menuitemModifyOrder,
            this.menuitemCloseAll,
            this.menuitemSetTrailing,
            this.menuItemConfigColumns});
            this.contextMenuOrder.Name = "contextMenuOrder";
            this.contextMenuOrder.Size = new System.Drawing.Size(187, 114);
            // 
            // menuitemCloseOrder
            // 
            this.menuitemCloseOrder.Name = "menuitemCloseOrder";
            this.menuitemCloseOrder.Size = new System.Drawing.Size(186, 22);
            this.menuitemCloseOrder.Tag = "TitleCloseOrder";
            this.menuitemCloseOrder.Text = "Закрыть ордер";
            this.menuitemCloseOrder.Click += new System.EventHandler(this.MenuitemCloseOrderClick);
            // 
            // menuitemModifyOrder
            // 
            this.menuitemModifyOrder.Name = "menuitemModifyOrder";
            this.menuitemModifyOrder.Size = new System.Drawing.Size(186, 22);
            this.menuitemModifyOrder.Tag = "TitleChangeOrder";
            this.menuitemModifyOrder.Text = "Изменить ордер";
            this.menuitemModifyOrder.Click += new System.EventHandler(this.MenuitemModifyOrderClick);
            // 
            // menuitemCloseAll
            // 
            this.menuitemCloseAll.Name = "menuitemCloseAll";
            this.menuitemCloseAll.Size = new System.Drawing.Size(186, 22);
            this.menuitemCloseAll.Tag = "TitleCloseAll";
            this.menuitemCloseAll.Text = "Закрыть все";
            this.menuitemCloseAll.Click += new System.EventHandler(this.MenuitemCloseAllClick);
            // 
            // menuitemSetTrailing
            // 
            this.menuitemSetTrailing.Name = "menuitemSetTrailing";
            this.menuitemSetTrailing.Size = new System.Drawing.Size(186, 22);
            this.menuitemSetTrailing.Tag = "TitleSetTrailing";
            this.menuitemSetTrailing.Text = "Установить трейлинг";
            this.menuitemSetTrailing.Click += new System.EventHandler(this.MenuitemSetTrailingClick);
            // 
            // menuItemConfigColumns
            // 
            this.menuItemConfigColumns.Name = "menuItemConfigColumns";
            this.menuItemConfigColumns.Size = new System.Drawing.Size(186, 22);
            this.menuItemConfigColumns.Tag = "TitleSetupColumns";
            this.menuItemConfigColumns.Text = "Настроить столбцы...";
            this.menuItemConfigColumns.Visible = false;
            this.menuItemConfigColumns.Click += new System.EventHandler(this.ConfigGridOpenPosColumns);
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 2000;
            this.updateTimer.Tick += new System.EventHandler(this.UpdateTimerTick);
            // 
            // contextMenuPending
            // 
            this.contextMenuPending.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuNewOrder,
            this.MenuDeleteOrder,
            this.MenuChangeOrder,
            this.MenuDeleteAllOrders,
            this.menuItemConfigColumns2});
            this.contextMenuPending.Name = "contextMenuOrder";
            this.contextMenuPending.Size = new System.Drawing.Size(188, 114);
            // 
            // MenuNewOrder
            // 
            this.MenuNewOrder.Name = "MenuNewOrder";
            this.MenuNewOrder.Size = new System.Drawing.Size(187, 22);
            this.MenuNewOrder.Text = "Новый ордер";
            this.MenuNewOrder.Click += new System.EventHandler(this.MenuNewOrderClick);
            // 
            // MenuDeleteOrder
            // 
            this.MenuDeleteOrder.Name = "MenuDeleteOrder";
            this.MenuDeleteOrder.Size = new System.Drawing.Size(187, 22);
            this.MenuDeleteOrder.Text = "Удалить ордер";
            this.MenuDeleteOrder.Click += new System.EventHandler(this.MenuDeleteOrderClick);
            // 
            // MenuChangeOrder
            // 
            this.MenuChangeOrder.Name = "MenuChangeOrder";
            this.MenuChangeOrder.Size = new System.Drawing.Size(187, 22);
            this.MenuChangeOrder.Text = "Редактировать ордер";
            this.MenuChangeOrder.Click += new System.EventHandler(this.MenuChangeOrderClick);
            // 
            // MenuDeleteAllOrders
            // 
            this.MenuDeleteAllOrders.Name = "MenuDeleteAllOrders";
            this.MenuDeleteAllOrders.Size = new System.Drawing.Size(187, 22);
            this.MenuDeleteAllOrders.Text = "Удалить все";
            this.MenuDeleteAllOrders.Click += new System.EventHandler(this.MenuDeleteAllOrdersClick);
            // 
            // menuItemConfigColumns2
            // 
            this.menuItemConfigColumns2.Name = "menuItemConfigColumns2";
            this.menuItemConfigColumns2.Size = new System.Drawing.Size(187, 22);
            this.menuItemConfigColumns2.Text = "Настроить столбцы...";
            this.menuItemConfigColumns2.Visible = false;
            this.menuItemConfigColumns2.Click += new System.EventHandler(this.ConfigGridPendingOrdersColumns);
            // 
            // AccountMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 328);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AccountMonitorForm";
            this.Tag = "TitleAccount";
            this.Text = "Счет";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AccountMonitorFormFormClosing);
            this.Load += new System.EventHandler(this.AccountMonitorFormLoad);
            this.ResizeEnd += new System.EventHandler(this.AccountMonitorFormResizeEnd);
            this.Move += new System.EventHandler(this.AccountMonitorFormMove);
            this.tabControl.ResumeLayout(false);
            this.pageAccount.ResumeLayout(false);
            this.pageAccount.PerformLayout();
            this.panelAcTop.ResumeLayout(false);
            this.panelAcTop.PerformLayout();
            this.pageOpenOrders.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pageDelayedOrders.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.pageHistoryAccount.ResumeLayout(false);
            this.contextMenuSummaryOrder.ResumeLayout(false);
            this.contextMenuOrder.ResumeLayout(false);
            this.contextMenuPending.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pageAccount;
        private System.Windows.Forms.TabPage pageOpenOrders;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.ContextMenuStrip contextMenuOrder;
        private System.Windows.Forms.ToolStripMenuItem menuitemCloseOrder;
        private System.Windows.Forms.ToolStripMenuItem menuitemModifyOrder;
        private System.Windows.Forms.ToolStripMenuItem menuitemCloseAll;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button CloseAllBtn;
        private System.Windows.Forms.Button EditOrderBtn;
        private System.Windows.Forms.Button NewOrderBtn;
        private System.Windows.Forms.TabPage pageDelayedOrders;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnDeletePendingOrders;
        private System.Windows.Forms.Button btnEditPendingOrder;
        private System.Windows.Forms.Button btnNewPendingOrder;
        private System.Windows.Forms.Button ClosePositionBtn;
        private System.Windows.Forms.Button btnDeletePendingOrder;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TabPage pageHistoryAccount;
        private Controls.AccountHistoryControl accountHistoryCtrl;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ContextMenuStrip contextMenuSummaryOrder;
        private System.Windows.Forms.ToolStripMenuItem menuItemCloseAll1;
        private FastGrid.FastGrid gridOpenPos;
        private FastGrid.FastGrid gridPendingOrders;
        private System.Windows.Forms.ToolStripMenuItem menuitemSetTrailing;
        private System.Windows.Forms.Button btnEstimateRisk;
        private FastGrid.FastGrid dgSummary;
        private System.Windows.Forms.Button btnStat;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ContextMenuStrip contextMenuPending;
        private System.Windows.Forms.ToolStripMenuItem MenuDeleteOrder;
        private System.Windows.Forms.ToolStripMenuItem MenuChangeOrder;
        private System.Windows.Forms.ToolStripMenuItem MenuDeleteAllOrders;
        private System.Windows.Forms.ToolStripMenuItem MenuNewOrder;
        private System.Windows.Forms.ToolStripMenuItem menuItemConfigColumns;
        private System.Windows.Forms.ToolStripMenuItem menuItemConfigColumns1;
        private System.Windows.Forms.ToolStripMenuItem menuItemConfigColumns2;
        private UI.Util.Control.StandByControl panelHistoryOrdersLoadingStatus;
        private FastGrid.FastGrid dgAccount;
        private System.Windows.Forms.FlowLayoutPanel panelAcTop;
    }
}