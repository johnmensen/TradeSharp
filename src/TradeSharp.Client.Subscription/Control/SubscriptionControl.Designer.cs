namespace TradeSharp.Client.Subscription.Control
{
    partial class SubscriptionControl
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubscriptionControl));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageSubscription = new System.Windows.Forms.TabPage();
            this.subscriptionGrid = new TradeSharp.Client.Subscription.Control.SubscriptionFastGrid();
            this.tabPagePerformers = new System.Windows.Forms.TabPage();
            this.performerPanel = new System.Windows.Forms.Panel();
            this.performerSplitContainer = new System.Windows.Forms.SplitContainer();
            this.topFilterControl = new TradeSharp.Client.Subscription.Control.TopFilterControl();
            this.performerGridCtrl = new TradeSharp.Client.Subscription.Control.PerformersFastGrid();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.expandTopFilterControlButton = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.createPortfolioButton = new System.Windows.Forms.Button();
            this.sortLabel = new System.Windows.Forms.Label();
            this.sortComboBox = new System.Windows.Forms.ComboBox();
            this.sortOrderComboBox = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.criteriaComboBox = new System.Windows.Forms.ComboBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.countComboBox = new System.Windows.Forms.ComboBox();
            this.findButton = new System.Windows.Forms.Button();
            this.parametersButton = new System.Windows.Forms.Button();
            this.parametersContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.signalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pammsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.performersStandByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.tabPageSignal = new System.Windows.Forms.TabPage();
            this.signalFastGrid = new TradeSharp.Client.Subscription.Control.SignalFastGrid();
            this.tabPageCompanyPortfolios = new System.Windows.Forms.TabPage();
            this.portfolioContainer = new TradeSharp.Client.Subscription.Control.TopPortfolioListControl();
            this.portfoliosStandByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.tabPageMyPortfolios = new System.Windows.Forms.TabPage();
            this.myPortfolioStandByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.timerWhistlerFarter = new System.Windows.Forms.Timer(this.components);
            this.tabControl.SuspendLayout();
            this.tabPageSubscription.SuspendLayout();
            this.tabPagePerformers.SuspendLayout();
            this.performerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.performerSplitContainer)).BeginInit();
            this.performerSplitContainer.Panel1.SuspendLayout();
            this.performerSplitContainer.Panel2.SuspendLayout();
            this.performerSplitContainer.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.parametersContextMenuStrip.SuspendLayout();
            this.tabPageSignal.SuspendLayout();
            this.tabPageCompanyPortfolios.SuspendLayout();
            this.tabPageMyPortfolios.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageSubscription);
            this.tabControl.Controls.Add(this.tabPagePerformers);
            this.tabControl.Controls.Add(this.tabPageSignal);
            this.tabControl.Controls.Add(this.tabPageCompanyPortfolios);
            this.tabControl.Controls.Add(this.tabPageMyPortfolios);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1012, 563);
            this.tabControl.TabIndex = 2;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControlSelectedIndexChanged);
            // 
            // tabPageSubscription
            // 
            this.tabPageSubscription.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageSubscription.Controls.Add(this.subscriptionGrid);
            this.tabPageSubscription.Location = new System.Drawing.Point(4, 22);
            this.tabPageSubscription.Name = "tabPageSubscription";
            this.tabPageSubscription.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSubscription.Size = new System.Drawing.Size(1004, 537);
            this.tabPageSubscription.TabIndex = 0;
            this.tabPageSubscription.Tag = "TitleSubscription";
            this.tabPageSubscription.Text = "Подписка";
            // 
            // subscriptionGrid
            // 
            this.subscriptionGrid.BackColor = System.Drawing.SystemColors.Control;
            this.subscriptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subscriptionGrid.Location = new System.Drawing.Point(3, 3);
            this.subscriptionGrid.Name = "subscriptionGrid";
            this.subscriptionGrid.Size = new System.Drawing.Size(998, 531);
            this.subscriptionGrid.TabIndex = 0;
            // 
            // tabPagePerformers
            // 
            this.tabPagePerformers.BackColor = System.Drawing.SystemColors.Control;
            this.tabPagePerformers.Controls.Add(this.performerPanel);
            this.tabPagePerformers.Controls.Add(this.performersStandByControl);
            this.tabPagePerformers.Location = new System.Drawing.Point(4, 22);
            this.tabPagePerformers.Name = "tabPagePerformers";
            this.tabPagePerformers.Size = new System.Drawing.Size(1004, 537);
            this.tabPagePerformers.TabIndex = 1;
            this.tabPagePerformers.Tag = "TitleStrategies";
            this.tabPagePerformers.Text = "Стратегии";
            // 
            // performerPanel
            // 
            this.performerPanel.Controls.Add(this.performerSplitContainer);
            this.performerPanel.Controls.Add(this.flowLayoutPanel2);
            this.performerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performerPanel.Location = new System.Drawing.Point(0, 0);
            this.performerPanel.Name = "performerPanel";
            this.performerPanel.Size = new System.Drawing.Size(1004, 537);
            this.performerPanel.TabIndex = 4;
            // 
            // performerSplitContainer
            // 
            this.performerSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.performerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performerSplitContainer.Location = new System.Drawing.Point(0, 29);
            this.performerSplitContainer.Name = "performerSplitContainer";
            this.performerSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // performerSplitContainer.Panel1
            // 
            this.performerSplitContainer.Panel1.Controls.Add(this.topFilterControl);
            this.performerSplitContainer.Panel1MinSize = 0;
            // 
            // performerSplitContainer.Panel2
            // 
            this.performerSplitContainer.Panel2.Controls.Add(this.performerGridCtrl);
            this.performerSplitContainer.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.performerSplitContainer.Panel2MinSize = 0;
            this.performerSplitContainer.Size = new System.Drawing.Size(1004, 508);
            this.performerSplitContainer.SplitterDistance = 268;
            this.performerSplitContainer.TabIndex = 5;
            // 
            // topFilterControl
            // 
            this.topFilterControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topFilterControl.Location = new System.Drawing.Point(0, 0);
            this.topFilterControl.MinimumSize = new System.Drawing.Size(600, 0);
            this.topFilterControl.Name = "topFilterControl";
            this.topFilterControl.Size = new System.Drawing.Size(1000, 264);
            this.topFilterControl.SortField = null;
            this.topFilterControl.SortFieldIndex = -1;
            this.topFilterControl.TabIndex = 8;
            // 
            // performerGridCtrl
            // 
            this.performerGridCtrl.BackColor = System.Drawing.SystemColors.Control;
            this.performerGridCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performerGridCtrl.FitWidth = false;
            this.performerGridCtrl.Location = new System.Drawing.Point(0, 29);
            this.performerGridCtrl.MiniChartSize = new System.Drawing.Size(130, 32);
            this.performerGridCtrl.Name = "performerGridCtrl";
            this.performerGridCtrl.ShowLabelsInMiniChart = true;
            this.performerGridCtrl.Size = new System.Drawing.Size(1000, 203);
            this.performerGridCtrl.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.expandTopFilterControlButton);
            this.flowLayoutPanel1.Controls.Add(this.createPortfolioButton);
            this.flowLayoutPanel1.Controls.Add(this.sortLabel);
            this.flowLayoutPanel1.Controls.Add(this.sortComboBox);
            this.flowLayoutPanel1.Controls.Add(this.sortOrderComboBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1000, 29);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // expandTopFilterControlButton
            // 
            this.expandTopFilterControlButton.AutoSize = true;
            this.expandTopFilterControlButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.expandTopFilterControlButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.expandTopFilterControlButton.ImageIndex = 0;
            this.expandTopFilterControlButton.ImageList = this.imageList;
            this.expandTopFilterControlButton.Location = new System.Drawing.Point(3, 3);
            this.expandTopFilterControlButton.Name = "expandTopFilterControlButton";
            this.expandTopFilterControlButton.Size = new System.Drawing.Size(92, 23);
            this.expandTopFilterControlButton.TabIndex = 2;
            this.expandTopFilterControlButton.Tag = "TitleExpand";
            this.expandTopFilterControlButton.Text = "Развернуть";
            this.expandTopFilterControlButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.expandTopFilterControlButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.expandTopFilterControlButton.UseVisualStyleBackColor = true;
            this.expandTopFilterControlButton.Click += new System.EventHandler(this.ExpandTopFilterControlButtonClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Yellow;
            this.imageList.Images.SetKeyName(0, "ico16 movedown.bmp");
            this.imageList.Images.SetKeyName(1, "ico16 moveup.bmp");
            // 
            // createPortfolioButton
            // 
            this.createPortfolioButton.AutoSize = true;
            this.createPortfolioButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.createPortfolioButton.Image = ((System.Drawing.Image)(resources.GetObject("createPortfolioButton.Image")));
            this.createPortfolioButton.Location = new System.Drawing.Point(101, 3);
            this.createPortfolioButton.Name = "createPortfolioButton";
            this.createPortfolioButton.Size = new System.Drawing.Size(127, 23);
            this.createPortfolioButton.TabIndex = 1;
            this.createPortfolioButton.Tag = "TitleCreatePortfolio";
            this.createPortfolioButton.Text = "Создать портфель";
            this.createPortfolioButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.createPortfolioButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.createPortfolioButton.UseVisualStyleBackColor = true;
            this.createPortfolioButton.Click += new System.EventHandler(this.CreatePortfolioButtonClick);
            // 
            // sortLabel
            // 
            this.sortLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.sortLabel.AutoSize = true;
            this.sortLabel.Location = new System.Drawing.Point(234, 8);
            this.sortLabel.Name = "sortLabel";
            this.sortLabel.Size = new System.Drawing.Size(88, 13);
            this.sortLabel.TabIndex = 4;
            this.sortLabel.Tag = "TitleOrderBy";
            this.sortLabel.Text = "Упорядочить по";
            this.sortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.sortLabel.Visible = false;
            // 
            // sortComboBox
            // 
            this.sortComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.sortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortComboBox.FormattingEnabled = true;
            this.sortComboBox.Location = new System.Drawing.Point(328, 4);
            this.sortComboBox.Name = "sortComboBox";
            this.sortComboBox.Size = new System.Drawing.Size(129, 21);
            this.sortComboBox.TabIndex = 5;
            this.sortComboBox.Visible = false;
            // 
            // sortOrderComboBox
            // 
            this.sortOrderComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.sortOrderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortOrderComboBox.FormattingEnabled = true;
            this.sortOrderComboBox.Location = new System.Drawing.Point(463, 4);
            this.sortOrderComboBox.Name = "sortOrderComboBox";
            this.sortOrderComboBox.Size = new System.Drawing.Size(121, 21);
            this.sortOrderComboBox.TabIndex = 6;
            this.sortOrderComboBox.Visible = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.label1);
            this.flowLayoutPanel2.Controls.Add(this.criteriaComboBox);
            this.flowLayoutPanel2.Controls.Add(this.refreshButton);
            this.flowLayoutPanel2.Controls.Add(this.countComboBox);
            this.flowLayoutPanel2.Controls.Add(this.findButton);
            this.flowLayoutPanel2.Controls.Add(this.parametersButton);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(1004, 29);
            this.flowLayoutPanel2.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "TitlePresetCriterion";
            this.label1.Text = "Предустановленный критерий";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // criteriaComboBox
            // 
            this.criteriaComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.criteriaComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.criteriaComboBox.FormattingEnabled = true;
            this.criteriaComboBox.Location = new System.Drawing.Point(170, 4);
            this.criteriaComboBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.criteriaComboBox.Name = "criteriaComboBox";
            this.criteriaComboBox.Size = new System.Drawing.Size(403, 21);
            this.criteriaComboBox.TabIndex = 1;
            this.criteriaComboBox.SelectedIndexChanged += new System.EventHandler(this.CriteriaComboBoxSelectedIndexChanged);
            // 
            // refreshButton
            // 
            this.refreshButton.AutoSize = true;
            this.refreshButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.refreshButton.Enabled = false;
            this.refreshButton.Image = ((System.Drawing.Image)(resources.GetObject("refreshButton.Image")));
            this.refreshButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.refreshButton.Location = new System.Drawing.Point(579, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(144, 23);
            this.refreshButton.TabIndex = 0;
            this.refreshButton.Tag = "TitleShowResults";
            this.refreshButton.Text = "Показать результаты";
            this.refreshButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.refreshButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButtonClick);
            // 
            // countComboBox
            // 
            this.countComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.countComboBox.FormattingEnabled = true;
            this.countComboBox.Items.AddRange(new object[] {
            "10",
            "20",
            "50",
            "100"});
            this.countComboBox.Location = new System.Drawing.Point(729, 4);
            this.countComboBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.countComboBox.Name = "countComboBox";
            this.countComboBox.Size = new System.Drawing.Size(80, 21);
            this.countComboBox.TabIndex = 7;
            this.countComboBox.Text = "30";
            this.countComboBox.TextChanged += new System.EventHandler(this.CountComboBoxTextChanged);
            // 
            // findButton
            // 
            this.findButton.AutoSize = true;
            this.findButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.findButton.Image = ((System.Drawing.Image)(resources.GetObject("findButton.Image")));
            this.findButton.Location = new System.Drawing.Point(815, 3);
            this.findButton.Name = "findButton";
            this.findButton.Size = new System.Drawing.Size(74, 23);
            this.findButton.TabIndex = 8;
            this.findButton.Tag = "TitleFindMenu";
            this.findButton.Text = "Поиск...";
            this.findButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.findButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.findButton.UseVisualStyleBackColor = true;
            this.findButton.Click += new System.EventHandler(this.FindButtonClick);
            // 
            // parametersButton
            // 
            this.parametersButton.AutoSize = true;
            this.parametersButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.parametersButton.ContextMenuStrip = this.parametersContextMenuStrip;
            this.parametersButton.Image = ((System.Drawing.Image)(resources.GetObject("parametersButton.Image")));
            this.parametersButton.Location = new System.Drawing.Point(895, 3);
            this.parametersButton.Name = "parametersButton";
            this.parametersButton.Size = new System.Drawing.Size(52, 23);
            this.parametersButton.TabIndex = 9;
            this.parametersButton.Tag = "TitleAll";
            this.parametersButton.Text = "Все";
            this.parametersButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.parametersButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.parametersButton.UseVisualStyleBackColor = true;
            this.parametersButton.Click += new System.EventHandler(this.ParametersButtonClick);
            // 
            // parametersContextMenuStrip
            // 
            this.parametersContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.signalsToolStripMenuItem,
            this.pammsToolStripMenuItem});
            this.parametersContextMenuStrip.Name = "parametersContextMenuStrip";
            this.parametersContextMenuStrip.Size = new System.Drawing.Size(119, 48);
            this.parametersContextMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ParametersContextMenuStripItemClicked);
            // 
            // signalsToolStripMenuItem
            // 
            this.signalsToolStripMenuItem.CheckOnClick = true;
            this.signalsToolStripMenuItem.Name = "signalsToolStripMenuItem";
            this.signalsToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.signalsToolStripMenuItem.Tag = "TitleSignals";
            this.signalsToolStripMenuItem.Text = "Сигналы";
            // 
            // pammsToolStripMenuItem
            // 
            this.pammsToolStripMenuItem.CheckOnClick = true;
            this.pammsToolStripMenuItem.Name = "pammsToolStripMenuItem";
            this.pammsToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.pammsToolStripMenuItem.Tag = "TitlePAMMs";
            this.pammsToolStripMenuItem.Text = "ПАММы";
            // 
            // performersStandByControl
            // 
            this.performersStandByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performersStandByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.performersStandByControl.IsShown = true;
            this.performersStandByControl.Location = new System.Drawing.Point(0, 0);
            this.performersStandByControl.Name = "performersStandByControl";
            this.performersStandByControl.Size = new System.Drawing.Size(1004, 537);
            this.performersStandByControl.TabIndex = 3;
            this.performersStandByControl.Tag = "TitleLoading";
            this.performersStandByControl.Text = "Загрузка...";
            this.performersStandByControl.TransparentForm = null;
            // 
            // tabPageSignal
            // 
            this.tabPageSignal.Controls.Add(this.signalFastGrid);
            this.tabPageSignal.Location = new System.Drawing.Point(4, 22);
            this.tabPageSignal.Name = "tabPageSignal";
            this.tabPageSignal.Size = new System.Drawing.Size(1004, 537);
            this.tabPageSignal.TabIndex = 2;
            this.tabPageSignal.Tag = "TitleSignals";
            this.tabPageSignal.Text = "Сигналы";
            this.tabPageSignal.UseVisualStyleBackColor = true;
            // 
            // signalFastGrid
            // 
            this.signalFastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.signalFastGrid.Location = new System.Drawing.Point(0, 0);
            this.signalFastGrid.Name = "signalFastGrid";
            this.signalFastGrid.Size = new System.Drawing.Size(1004, 537);
            this.signalFastGrid.TabIndex = 1;
            // 
            // tabPageCompanyPortfolios
            // 
            this.tabPageCompanyPortfolios.Controls.Add(this.portfolioContainer);
            this.tabPageCompanyPortfolios.Controls.Add(this.portfoliosStandByControl);
            this.tabPageCompanyPortfolios.Location = new System.Drawing.Point(4, 22);
            this.tabPageCompanyPortfolios.Name = "tabPageCompanyPortfolios";
            this.tabPageCompanyPortfolios.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCompanyPortfolios.Size = new System.Drawing.Size(1004, 537);
            this.tabPageCompanyPortfolios.TabIndex = 3;
            this.tabPageCompanyPortfolios.Tag = "TitleTOP5100";
            this.tabPageCompanyPortfolios.Text = "ТОП 5..100";
            this.tabPageCompanyPortfolios.UseVisualStyleBackColor = true;
            // 
            // portfolioContainer
            // 
            this.portfolioContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portfolioContainer.Location = new System.Drawing.Point(3, 3);
            this.portfolioContainer.Name = "portfolioContainer";
            this.portfolioContainer.Size = new System.Drawing.Size(998, 531);
            this.portfolioContainer.TabIndex = 4;
            // 
            // portfoliosStandByControl
            // 
            this.portfoliosStandByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portfoliosStandByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.portfoliosStandByControl.IsShown = true;
            this.portfoliosStandByControl.Location = new System.Drawing.Point(3, 3);
            this.portfoliosStandByControl.Name = "portfoliosStandByControl";
            this.portfoliosStandByControl.Size = new System.Drawing.Size(998, 531);
            this.portfoliosStandByControl.TabIndex = 3;
            this.portfoliosStandByControl.Tag = "TitleLoading";
            this.portfoliosStandByControl.Text = "Загрузка...";
            this.portfoliosStandByControl.TransparentForm = null;
            this.portfoliosStandByControl.Visible = false;
            // 
            // tabPageMyPortfolios
            // 
            this.tabPageMyPortfolios.Controls.Add(this.myPortfolioStandByControl);
            this.tabPageMyPortfolios.Location = new System.Drawing.Point(4, 22);
            this.tabPageMyPortfolios.Name = "tabPageMyPortfolios";
            this.tabPageMyPortfolios.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMyPortfolios.Size = new System.Drawing.Size(1004, 537);
            this.tabPageMyPortfolios.TabIndex = 4;
            this.tabPageMyPortfolios.Tag = "TitleMyChoice";
            this.tabPageMyPortfolios.Text = "Мой выбор";
            this.tabPageMyPortfolios.UseVisualStyleBackColor = true;
            // 
            // myPortfolioStandByControl
            // 
            this.myPortfolioStandByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myPortfolioStandByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.myPortfolioStandByControl.IsShown = true;
            this.myPortfolioStandByControl.Location = new System.Drawing.Point(3, 3);
            this.myPortfolioStandByControl.Name = "myPortfolioStandByControl";
            this.myPortfolioStandByControl.Size = new System.Drawing.Size(998, 531);
            this.myPortfolioStandByControl.TabIndex = 0;
            this.myPortfolioStandByControl.Tag = "TitleLoading";
            this.myPortfolioStandByControl.Text = "Загрузка...";
            this.myPortfolioStandByControl.TransparentForm = null;
            // 
            // timerWhistlerFarter
            // 
            this.timerWhistlerFarter.Enabled = true;
            this.timerWhistlerFarter.Interval = 200;
            this.timerWhistlerFarter.Tick += new System.EventHandler(this.TimerWhistlerFarterTick);
            // 
            // SubscriptionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Name = "SubscriptionControl";
            this.Size = new System.Drawing.Size(1012, 563);
            this.tabControl.ResumeLayout(false);
            this.tabPageSubscription.ResumeLayout(false);
            this.tabPagePerformers.ResumeLayout(false);
            this.performerPanel.ResumeLayout(false);
            this.performerPanel.PerformLayout();
            this.performerSplitContainer.Panel1.ResumeLayout(false);
            this.performerSplitContainer.Panel2.ResumeLayout(false);
            this.performerSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.performerSplitContainer)).EndInit();
            this.performerSplitContainer.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.parametersContextMenuStrip.ResumeLayout(false);
            this.tabPageSignal.ResumeLayout(false);
            this.tabPageCompanyPortfolios.ResumeLayout(false);
            this.tabPageMyPortfolios.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageSubscription;
        private SubscriptionFastGrid subscriptionGrid;
        private System.Windows.Forms.TabPage tabPagePerformers;
        private System.Windows.Forms.TabPage tabPageSignal;
        private SignalFastGrid signalFastGrid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox criteriaComboBox;
        private System.Windows.Forms.Button expandTopFilterControlButton;
        private UI.Util.Control.StandByControl performersStandByControl;
        private System.Windows.Forms.Panel performerPanel;
        private System.Windows.Forms.TabPage tabPageCompanyPortfolios;
        private System.Windows.Forms.TabPage tabPageMyPortfolios;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.SplitContainer performerSplitContainer;
        private PerformersFastGrid performerGridCtrl;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ComboBox countComboBox;
        private UI.Util.Control.StandByControl portfoliosStandByControl;
        private TopPortfolioListControl portfolioContainer;
        private System.Windows.Forms.Button createPortfolioButton;
        private UI.Util.Control.StandByControl myPortfolioStandByControl;
        private System.Windows.Forms.Timer timerWhistlerFarter;
        private System.Windows.Forms.Label sortLabel;
        private System.Windows.Forms.ComboBox sortComboBox;
        private System.Windows.Forms.ComboBox sortOrderComboBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private TopFilterControl topFilterControl;
        private System.Windows.Forms.Button findButton;
        private System.Windows.Forms.Button parametersButton;
        private System.Windows.Forms.ContextMenuStrip parametersContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem signalsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pammsToolStripMenuItem;
    }
}
