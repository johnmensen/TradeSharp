using TradeSharp.UI.Util.Control;

namespace TradeSharp.Client.Forms
{
    partial class WalletForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WalletForm));
            this.imageListButtonGlypth = new System.Windows.Forms.ImageList(this.components);
            this.imageListPaymentSys = new System.Windows.Forms.ImageList(this.components);
            this.timerProgress = new System.Windows.Forms.Timer(this.components);
            this.imageListGrid = new System.Windows.Forms.ImageList(this.components);
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.panelBottom = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnRefreshAll = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageWallet = new System.Windows.Forms.TabPage();
            this.gridAccount = new FastGrid.FastGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.lblWallet = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPageSubscription = new System.Windows.Forms.TabPage();
            this.gridSubscription = new FastGrid.FastGrid();
            this.tabPageTransfer = new System.Windows.Forms.TabPage();
            this.standByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.transferFastGrid = new FastGrid.FastGrid();
            this.transferTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.stopTransfersDownloadButton = new System.Windows.Forms.Button();
            this.summaryTransfersCollapsiblePanel = new TradeSharp.Util.Controls.CollapsiblePanel();
            this.summaryTransfersFastGrid = new FastGrid.FastGrid();
            this.tabPagePaymentSystem = new System.Windows.Forms.TabPage();
            this.panelWebMoney = new System.Windows.Forms.Panel();
            this.webMoneyPursePanelControl = new TradeSharp.Client.Controls.WebMoneyPursePanelControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblWmidValid = new System.Windows.Forms.Label();
            this.btnCancelWebMoney = new System.Windows.Forms.Button();
            this.btnSaveWebMoney = new System.Windows.Forms.Button();
            this.cbWMID = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnShowPaymentSystems = new System.Windows.Forms.Button();
            this.panelPaymentSystems = new System.Windows.Forms.Panel();
            this.btnWebMoney = new TradeSharp.UI.Util.Control.GrayStyledButton();
            this.imageListGridChart = new System.Windows.Forms.ImageList(this.components);
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageWallet.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPageSubscription.SuspendLayout();
            this.tabPageTransfer.SuspendLayout();
            this.transferTableLayoutPanel.SuspendLayout();
            this.summaryTransfersCollapsiblePanel.SuspendLayout();
            this.tabPagePaymentSystem.SuspendLayout();
            this.panelWebMoney.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panelPaymentSystems.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListButtonGlypth
            // 
            this.imageListButtonGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListButtonGlypth.ImageStream")));
            this.imageListButtonGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListButtonGlypth.Images.SetKeyName(0, "ico_arrow_green_left.png");
            this.imageListButtonGlypth.Images.SetKeyName(1, "ico_save.png");
            this.imageListButtonGlypth.Images.SetKeyName(2, "ico refresh.png");
            // 
            // imageListPaymentSys
            // 
            this.imageListPaymentSys.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListPaymentSys.ImageStream")));
            this.imageListPaymentSys.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListPaymentSys.Images.SetKeyName(0, "img_48_webmoney.png");
            // 
            // timerProgress
            // 
            this.timerProgress.Tick += new System.EventHandler(this.TimerProgressTick);
            // 
            // imageListGrid
            // 
            this.imageListGrid.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGrid.ImageStream")));
            this.imageListGrid.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGrid.Images.SetKeyName(0, "True");
            this.imageListGrid.Images.SetKeyName(1, "False");
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 2000;
            this.timerUpdate.Tick += new System.EventHandler(this.TimerUpdateTick);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.progressBar);
            this.panelBottom.Controls.Add(this.btnRefreshAll);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 270);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(546, 24);
            this.panelBottom.TabIndex = 2;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 1);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(514, 23);
            this.progressBar.Step = 5;
            this.progressBar.TabIndex = 2;
            // 
            // btnRefreshAll
            // 
            this.btnRefreshAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRefreshAll.ImageIndex = 2;
            this.btnRefreshAll.ImageList = this.imageListButtonGlypth;
            this.btnRefreshAll.Location = new System.Drawing.Point(514, 0);
            this.btnRefreshAll.Name = "btnRefreshAll";
            this.btnRefreshAll.Size = new System.Drawing.Size(32, 24);
            this.btnRefreshAll.TabIndex = 0;
            this.btnRefreshAll.UseVisualStyleBackColor = true;
            this.btnRefreshAll.Click += new System.EventHandler(this.btnRefreshAll_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageWallet);
            this.tabControl.Controls.Add(this.tabPageSubscription);
            this.tabControl.Controls.Add(this.tabPageTransfer);
            this.tabControl.Controls.Add(this.tabPagePaymentSystem);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(546, 270);
            this.tabControl.TabIndex = 3;
            // 
            // tabPageWallet
            // 
            this.tabPageWallet.Controls.Add(this.gridAccount);
            this.tabPageWallet.Controls.Add(this.panel1);
            this.tabPageWallet.Location = new System.Drawing.Point(4, 22);
            this.tabPageWallet.Name = "tabPageWallet";
            this.tabPageWallet.Size = new System.Drawing.Size(538, 244);
            this.tabPageWallet.TabIndex = 0;
            this.tabPageWallet.Tag = "TitleFunds";
            this.tabPageWallet.Text = "Средства";
            this.tabPageWallet.UseVisualStyleBackColor = true;
            // 
            // gridAccount
            // 
            this.gridAccount.CaptionHeight = 20;
            this.gridAccount.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridAccount.CellHeight = 18;
            this.gridAccount.CellPadding = 5;
            this.gridAccount.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridAccount.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridAccount.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridAccount.ColorCellFont = System.Drawing.Color.Black;
            this.gridAccount.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridAccount.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridAccount.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridAccount.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridAccount.FitWidth = false;
            this.gridAccount.FontAnchoredRow = null;
            this.gridAccount.FontCell = null;
            this.gridAccount.FontHeader = null;
            this.gridAccount.FontSelectedCell = null;
            this.gridAccount.Location = new System.Drawing.Point(0, 50);
            this.gridAccount.MinimumTableWidth = null;
            this.gridAccount.MultiSelectEnabled = false;
            this.gridAccount.Name = "gridAccount";
            this.gridAccount.SelectEnabled = true;
            this.gridAccount.Size = new System.Drawing.Size(538, 194);
            this.gridAccount.StickFirst = false;
            this.gridAccount.StickLast = false;
            this.gridAccount.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lblWallet);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(538, 50);
            this.panel1.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(3, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 4;
            this.label3.Tag = "TitleTradeAccounts";
            this.label3.Text = "Торговые счета";
            // 
            // lblWallet
            // 
            this.lblWallet.AutoSize = true;
            this.lblWallet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblWallet.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblWallet.Location = new System.Drawing.Point(64, 7);
            this.lblWallet.Name = "lblWallet";
            this.lblWallet.Size = new System.Drawing.Size(10, 13);
            this.lblWallet.TabIndex = 3;
            this.lblWallet.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 2;
            this.label1.Tag = "TitleFunds";
            this.label1.Text = "Средства:";
            // 
            // tabPageSubscription
            // 
            this.tabPageSubscription.Controls.Add(this.gridSubscription);
            this.tabPageSubscription.Location = new System.Drawing.Point(4, 22);
            this.tabPageSubscription.Name = "tabPageSubscription";
            this.tabPageSubscription.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSubscription.Size = new System.Drawing.Size(538, 244);
            this.tabPageSubscription.TabIndex = 1;
            this.tabPageSubscription.Tag = "TitleSubscription";
            this.tabPageSubscription.Text = "Подписка";
            this.tabPageSubscription.UseVisualStyleBackColor = true;
            // 
            // gridSubscription
            // 
            this.gridSubscription.CaptionHeight = 20;
            this.gridSubscription.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridSubscription.CellHeight = 18;
            this.gridSubscription.CellPadding = 5;
            this.gridSubscription.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridSubscription.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridSubscription.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridSubscription.ColorCellFont = System.Drawing.Color.Black;
            this.gridSubscription.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridSubscription.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridSubscription.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridSubscription.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridSubscription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSubscription.FitWidth = false;
            this.gridSubscription.FontAnchoredRow = null;
            this.gridSubscription.FontCell = null;
            this.gridSubscription.FontHeader = null;
            this.gridSubscription.FontSelectedCell = null;
            this.gridSubscription.Location = new System.Drawing.Point(3, 3);
            this.gridSubscription.MinimumTableWidth = null;
            this.gridSubscription.MultiSelectEnabled = false;
            this.gridSubscription.Name = "gridSubscription";
            this.gridSubscription.SelectEnabled = true;
            this.gridSubscription.Size = new System.Drawing.Size(532, 238);
            this.gridSubscription.StickFirst = false;
            this.gridSubscription.StickLast = false;
            this.gridSubscription.TabIndex = 0;
            // 
            // tabPageTransfer
            // 
            this.tabPageTransfer.Controls.Add(this.standByControl);
            this.tabPageTransfer.Controls.Add(this.transferFastGrid);
            this.tabPageTransfer.Controls.Add(this.transferTableLayoutPanel);
            this.tabPageTransfer.Controls.Add(this.summaryTransfersCollapsiblePanel);
            this.tabPageTransfer.Location = new System.Drawing.Point(4, 22);
            this.tabPageTransfer.Name = "tabPageTransfer";
            this.tabPageTransfer.Size = new System.Drawing.Size(538, 244);
            this.tabPageTransfer.TabIndex = 2;
            this.tabPageTransfer.Tag = "TitlePayments";
            this.tabPageTransfer.Text = "Платежи";
            this.tabPageTransfer.UseVisualStyleBackColor = true;
            // 
            // standByControl
            // 
            this.standByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.standByControl.IsShown = true;
            this.standByControl.Location = new System.Drawing.Point(0, 0);
            this.standByControl.Name = "standByControl";
            this.standByControl.Size = new System.Drawing.Size(538, 185);
            this.standByControl.TabIndex = 7;
            this.standByControl.Tag = "TitleLoading";
            this.standByControl.Text = "Загрузка...";
            this.standByControl.TransparentForm = null;
            // 
            // transferFastGrid
            // 
            this.transferFastGrid.CaptionHeight = 20;
            this.transferFastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.transferFastGrid.CellHeight = 18;
            this.transferFastGrid.CellPadding = 5;
            this.transferFastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.transferFastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.transferFastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.transferFastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.transferFastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.transferFastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.transferFastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.transferFastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.transferFastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transferFastGrid.FitWidth = true;
            this.transferFastGrid.FontAnchoredRow = null;
            this.transferFastGrid.FontCell = null;
            this.transferFastGrid.FontHeader = null;
            this.transferFastGrid.FontSelectedCell = null;
            this.transferFastGrid.Location = new System.Drawing.Point(0, 0);
            this.transferFastGrid.MinimumTableWidth = null;
            this.transferFastGrid.MultiSelectEnabled = false;
            this.transferFastGrid.Name = "transferFastGrid";
            this.transferFastGrid.SelectEnabled = true;
            this.transferFastGrid.Size = new System.Drawing.Size(538, 185);
            this.transferFastGrid.StickFirst = false;
            this.transferFastGrid.StickLast = false;
            this.transferFastGrid.TabIndex = 10;
            this.transferFastGrid.Visible = false;
            // 
            // transferTableLayoutPanel
            // 
            this.transferTableLayoutPanel.AutoSize = true;
            this.transferTableLayoutPanel.ColumnCount = 3;
            this.transferTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.transferTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.transferTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.transferTableLayoutPanel.Controls.Add(this.stopTransfersDownloadButton, 1, 0);
            this.transferTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.transferTableLayoutPanel.Location = new System.Drawing.Point(0, 185);
            this.transferTableLayoutPanel.Name = "transferTableLayoutPanel";
            this.transferTableLayoutPanel.RowCount = 1;
            this.transferTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.transferTableLayoutPanel.Size = new System.Drawing.Size(538, 29);
            this.transferTableLayoutPanel.TabIndex = 6;
            // 
            // stopTransfersDownloadButton
            // 
            this.stopTransfersDownloadButton.AutoSize = true;
            this.stopTransfersDownloadButton.Location = new System.Drawing.Point(212, 3);
            this.stopTransfersDownloadButton.Name = "stopTransfersDownloadButton";
            this.stopTransfersDownloadButton.Size = new System.Drawing.Size(114, 23);
            this.stopTransfersDownloadButton.TabIndex = 0;
            this.stopTransfersDownloadButton.Tag = "TitleCancelLoading";
            this.stopTransfersDownloadButton.Text = "Прервать загрузку";
            this.stopTransfersDownloadButton.UseVisualStyleBackColor = true;
            this.stopTransfersDownloadButton.Click += new System.EventHandler(this.StopTransfersDownloadButtonClick);
            // 
            // summaryTransfersCollapsiblePanel
            // 
            this.summaryTransfersCollapsiblePanel.BackColor = System.Drawing.Color.Transparent;
            this.summaryTransfersCollapsiblePanel.Collapse = true;
            this.summaryTransfersCollapsiblePanel.Controls.Add(this.summaryTransfersFastGrid);
            this.summaryTransfersCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.summaryTransfersCollapsiblePanel.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.summaryTransfersCollapsiblePanel.HeaderImage = null;
            this.summaryTransfersCollapsiblePanel.HeaderText = null;
            this.summaryTransfersCollapsiblePanel.HeaderTextColor = System.Drawing.Color.Black;
            this.summaryTransfersCollapsiblePanel.Location = new System.Drawing.Point(0, 214);
            this.summaryTransfersCollapsiblePanel.Name = "summaryTransfersCollapsiblePanel";
            this.summaryTransfersCollapsiblePanel.OriginalHeight = 129;
            this.summaryTransfersCollapsiblePanel.ShowExpandCollapseBox = false;
            this.summaryTransfersCollapsiblePanel.Size = new System.Drawing.Size(538, 30);
            this.summaryTransfersCollapsiblePanel.TabIndex = 3;
            this.summaryTransfersCollapsiblePanel.Tag = "TitleStatisticsOnAllPayments";
            this.summaryTransfersCollapsiblePanel.Text = "Статистика по всем платежам";
            this.summaryTransfersCollapsiblePanel.UseAnimation = true;
            this.summaryTransfersCollapsiblePanel.Visible = false;
            // 
            // summaryTransfersFastGrid
            // 
            this.summaryTransfersFastGrid.CaptionHeight = 20;
            this.summaryTransfersFastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.summaryTransfersFastGrid.CellHeight = 18;
            this.summaryTransfersFastGrid.CellPadding = 5;
            this.summaryTransfersFastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.summaryTransfersFastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.summaryTransfersFastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.summaryTransfersFastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.summaryTransfersFastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.summaryTransfersFastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.summaryTransfersFastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.summaryTransfersFastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.summaryTransfersFastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryTransfersFastGrid.FitWidth = true;
            this.summaryTransfersFastGrid.FontAnchoredRow = null;
            this.summaryTransfersFastGrid.FontCell = null;
            this.summaryTransfersFastGrid.FontHeader = null;
            this.summaryTransfersFastGrid.FontSelectedCell = null;
            this.summaryTransfersFastGrid.Location = new System.Drawing.Point(0, 30);
            this.summaryTransfersFastGrid.MinimumTableWidth = null;
            this.summaryTransfersFastGrid.MultiSelectEnabled = false;
            this.summaryTransfersFastGrid.Name = "summaryTransfersFastGrid";
            this.summaryTransfersFastGrid.SelectEnabled = true;
            this.summaryTransfersFastGrid.Size = new System.Drawing.Size(538, 0);
            this.summaryTransfersFastGrid.StickFirst = false;
            this.summaryTransfersFastGrid.StickLast = false;
            this.summaryTransfersFastGrid.TabIndex = 1;
            // 
            // tabPagePaymentSystem
            // 
            this.tabPagePaymentSystem.Controls.Add(this.panelWebMoney);
            this.tabPagePaymentSystem.Controls.Add(this.panelPaymentSystems);
            this.tabPagePaymentSystem.Location = new System.Drawing.Point(4, 22);
            this.tabPagePaymentSystem.Name = "tabPagePaymentSystem";
            this.tabPagePaymentSystem.Size = new System.Drawing.Size(538, 244);
            this.tabPagePaymentSystem.TabIndex = 3;
            this.tabPagePaymentSystem.Tag = "TitlePaymentSystems";
            this.tabPagePaymentSystem.Text = "Платежные системы";
            this.tabPagePaymentSystem.UseVisualStyleBackColor = true;
            // 
            // panelWebMoney
            // 
            this.panelWebMoney.Controls.Add(this.webMoneyPursePanelControl);
            this.panelWebMoney.Controls.Add(this.panel2);
            this.panelWebMoney.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWebMoney.Location = new System.Drawing.Point(0, 113);
            this.panelWebMoney.Name = "panelWebMoney";
            this.panelWebMoney.Size = new System.Drawing.Size(538, 131);
            this.panelWebMoney.TabIndex = 1;
            this.panelWebMoney.Visible = false;
            // 
            // webMoneyPursePanelControl
            // 
            this.webMoneyPursePanelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webMoneyPursePanelControl.Location = new System.Drawing.Point(0, 60);
            this.webMoneyPursePanelControl.Name = "webMoneyPursePanelControl";
            this.webMoneyPursePanelControl.Size = new System.Drawing.Size(538, 71);
            this.webMoneyPursePanelControl.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblWmidValid);
            this.panel2.Controls.Add(this.btnCancelWebMoney);
            this.panel2.Controls.Add(this.btnSaveWebMoney);
            this.panel2.Controls.Add(this.cbWMID);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.btnShowPaymentSystems);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(538, 60);
            this.panel2.TabIndex = 3;
            // 
            // lblWmidValid
            // 
            this.lblWmidValid.AutoSize = true;
            this.lblWmidValid.Location = new System.Drawing.Point(222, 37);
            this.lblWmidValid.Name = "lblWmidValid";
            this.lblWmidValid.Size = new System.Drawing.Size(11, 13);
            this.lblWmidValid.TabIndex = 6;
            this.lblWmidValid.Text = "*";
            // 
            // btnCancelWebMoney
            // 
            this.btnCancelWebMoney.AutoSize = true;
            this.btnCancelWebMoney.Location = new System.Drawing.Point(392, 3);
            this.btnCancelWebMoney.Name = "btnCancelWebMoney";
            this.btnCancelWebMoney.Size = new System.Drawing.Size(100, 23);
            this.btnCancelWebMoney.TabIndex = 0;
            this.btnCancelWebMoney.Tag = "TitleCancel";
            this.btnCancelWebMoney.Text = "Отмена";
            this.btnCancelWebMoney.UseVisualStyleBackColor = true;
            this.btnCancelWebMoney.Click += new System.EventHandler(this.BtnCancelWebMoneyClick);
            // 
            // btnSaveWebMoney
            // 
            this.btnSaveWebMoney.AutoSize = true;
            this.btnSaveWebMoney.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSaveWebMoney.ImageIndex = 1;
            this.btnSaveWebMoney.ImageList = this.imageListButtonGlypth;
            this.btnSaveWebMoney.Location = new System.Drawing.Point(263, 3);
            this.btnSaveWebMoney.Name = "btnSaveWebMoney";
            this.btnSaveWebMoney.Size = new System.Drawing.Size(100, 23);
            this.btnSaveWebMoney.TabIndex = 1;
            this.btnSaveWebMoney.Tag = "TitleSave";
            this.btnSaveWebMoney.Text = "Сохранить";
            this.btnSaveWebMoney.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSaveWebMoney.UseVisualStyleBackColor = true;
            this.btnSaveWebMoney.Click += new System.EventHandler(this.BtnSaveWebMoneyClick);
            // 
            // cbWMID
            // 
            this.cbWMID.FormattingEnabled = true;
            this.cbWMID.Location = new System.Drawing.Point(44, 33);
            this.cbWMID.Name = "cbWMID";
            this.cbWMID.Size = new System.Drawing.Size(174, 21);
            this.cbWMID.TabIndex = 5;
            this.cbWMID.TextChanged += new System.EventHandler(this.CbWmidTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "WMID";
            // 
            // btnShowPaymentSystems
            // 
            this.btnShowPaymentSystems.AutoSize = true;
            this.btnShowPaymentSystems.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnShowPaymentSystems.ImageIndex = 0;
            this.btnShowPaymentSystems.ImageList = this.imageListButtonGlypth;
            this.btnShowPaymentSystems.Location = new System.Drawing.Point(3, 3);
            this.btnShowPaymentSystems.Name = "btnShowPaymentSystems";
            this.btnShowPaymentSystems.Size = new System.Drawing.Size(140, 23);
            this.btnShowPaymentSystems.TabIndex = 3;
            this.btnShowPaymentSystems.Tag = "TitlePaymentSystems";
            this.btnShowPaymentSystems.Text = "Платежные системы";
            this.btnShowPaymentSystems.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnShowPaymentSystems.UseVisualStyleBackColor = true;
            this.btnShowPaymentSystems.Click += new System.EventHandler(this.BtnShowPaymentSystemsClick);
            // 
            // panelPaymentSystems
            // 
            this.panelPaymentSystems.Controls.Add(this.btnWebMoney);
            this.panelPaymentSystems.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPaymentSystems.Location = new System.Drawing.Point(0, 0);
            this.panelPaymentSystems.Name = "panelPaymentSystems";
            this.panelPaymentSystems.Size = new System.Drawing.Size(538, 113);
            this.panelPaymentSystems.TabIndex = 0;
            // 
            // btnWebMoney
            // 
            this.btnWebMoney.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWebMoney.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnWebMoney.ForeColor = System.Drawing.Color.SteelBlue;
            this.btnWebMoney.GrayStyle = false;
            this.btnWebMoney.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnWebMoney.ImageIndex = 0;
            this.btnWebMoney.ImageList = this.imageListPaymentSys;
            this.btnWebMoney.Location = new System.Drawing.Point(3, 3);
            this.btnWebMoney.Name = "btnWebMoney";
            this.btnWebMoney.Size = new System.Drawing.Size(105, 74);
            this.btnWebMoney.TabIndex = 0;
            this.btnWebMoney.Text = "WebMoney";
            this.btnWebMoney.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnWebMoney.UseVisualStyleBackColor = true;
            // 
            // imageListGridChart
            // 
            this.imageListGridChart.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGridChart.ImageStream")));
            this.imageListGridChart.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGridChart.Images.SetKeyName(0, "True");
            this.imageListGridChart.Images.SetKeyName(1, "False");
            // 
            // WalletForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 294);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WalletForm";
            this.Tag = "TitleWallet";
            this.Text = "Кошелек";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WalletFormFormClosing);
            this.Load += new System.EventHandler(this.WalletFormLoad);
            this.ResizeEnd += new System.EventHandler(this.WalletFormResizeEnd);
            this.Move += new System.EventHandler(this.WalletFormMove);
            this.panelBottom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageWallet.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPageSubscription.ResumeLayout(false);
            this.tabPageTransfer.ResumeLayout(false);
            this.tabPageTransfer.PerformLayout();
            this.transferTableLayoutPanel.ResumeLayout(false);
            this.transferTableLayoutPanel.PerformLayout();
            this.summaryTransfersCollapsiblePanel.ResumeLayout(false);
            this.tabPagePaymentSystem.ResumeLayout(false);
            this.panelWebMoney.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panelPaymentSystems.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timerProgress;
        private System.Windows.Forms.ImageList imageListGrid;
        private System.Windows.Forms.ImageList imageListPaymentSys;
        private System.Windows.Forms.ImageList imageListButtonGlypth;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnRefreshAll;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageWallet;
        private FastGrid.FastGrid gridAccount;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblWallet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageSubscription;
        private FastGrid.FastGrid gridSubscription;
        private System.Windows.Forms.TabPage tabPageTransfer;
        private StandByControl standByControl;
        private FastGrid.FastGrid transferFastGrid;
        private System.Windows.Forms.TableLayoutPanel transferTableLayoutPanel;
        private System.Windows.Forms.Button stopTransfersDownloadButton;
        private TradeSharp.Util.Controls.CollapsiblePanel summaryTransfersCollapsiblePanel;
        private FastGrid.FastGrid summaryTransfersFastGrid;
        private System.Windows.Forms.TabPage tabPagePaymentSystem;
        private System.Windows.Forms.Panel panelWebMoney;
        private Controls.WebMoneyPursePanelControl webMoneyPursePanelControl;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblWmidValid;
        private System.Windows.Forms.Button btnCancelWebMoney;
        private System.Windows.Forms.Button btnSaveWebMoney;
        private System.Windows.Forms.ComboBox cbWMID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnShowPaymentSystems;
        private System.Windows.Forms.Panel panelPaymentSystems;
        private GrayStyledButton btnWebMoney;
        private System.Windows.Forms.ImageList imageListGridChart;
    }
}