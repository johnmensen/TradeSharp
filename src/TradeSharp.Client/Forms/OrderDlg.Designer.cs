using TradeSharp.UI.Util.Control;

namespace TradeSharp.Client.Forms
{
    partial class OrderDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderDlg));
            this.label15 = new System.Windows.Forms.Label();
            this.cbVolume = new System.Windows.Forms.ComboBox();
            this.btnBuyMarket = new TradeSharp.Client.Controls.CustomTextButton();
            this.btnSellMarket = new TradeSharp.Client.Controls.CustomTextButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbTP = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSL = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMagic = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.lblPrice = new System.Windows.Forms.Label();
            this.tbPrice = new System.Windows.Forms.TextBox();
            this.cbSlippage = new System.Windows.Forms.CheckBox();
            this.udSlippagePoints = new System.Windows.Forms.NumericUpDown();
            this.cbMode = new System.Windows.Forms.ComboBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.tabControlExtra = new System.Windows.Forms.TabControl();
            this.tabPageTrail = new System.Windows.Forms.TabPage();
            this.gridTrailing = new System.Windows.Forms.DataGridView();
            this.colPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPips = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTarget = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPagePending = new System.Windows.Forms.TabPage();
            this.tbPriceTo = new System.Windows.Forms.TextBox();
            this.cbPriceTo = new System.Windows.Forms.CheckBox();
            this.cbEndTime = new System.Windows.Forms.CheckBox();
            this.dpTimeTo = new System.Windows.Forms.DateTimePicker();
            this.cbStartFrom = new System.Windows.Forms.CheckBox();
            this.dpTimeFrom = new System.Windows.Forms.DateTimePicker();
            this.cbOrderOCO = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPageOrderXtra = new System.Windows.Forms.TabPage();
            this.tbExpertComment = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbExtra = new System.Windows.Forms.CheckBox();
            this.imageListGlyph = new System.Windows.Forms.ImageList(this.components);
            this.btnCalcVolume = new System.Windows.Forms.Button();
            this.cbCurx = new TradeSharp.UI.Util.Control.TickerComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.mainPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.udSlippagePoints)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.tabControlExtra.SuspendLayout();
            this.tabPageTrail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTrailing)).BeginInit();
            this.tabPagePending.SuspendLayout();
            this.tabPageOrderXtra.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 42);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(38, 13);
            this.label15.TabIndex = 33;
            this.label15.Tag = "TitleTicker";
            this.label15.Text = "Тикер";
            // 
            // cbVolume
            // 
            this.cbVolume.FormattingEnabled = true;
            this.cbVolume.Items.AddRange(new object[] {
            "10 000",
            "20 000",
            "30 000",
            "50 000",
            "70 000",
            "100 000",
            "150 000",
            "200 000",
            "250 000",
            "300 000",
            "400 000",
            "500 000",
            "750 000",
            "1 000 000"});
            this.cbVolume.Location = new System.Drawing.Point(140, 91);
            this.cbVolume.Name = "cbVolume";
            this.cbVolume.Size = new System.Drawing.Size(89, 21);
            this.cbVolume.TabIndex = 35;
            this.cbVolume.Text = "10 000";
            // 
            // btnBuyMarket
            // 
            this.btnBuyMarket.Location = new System.Drawing.Point(235, 66);
            this.btnBuyMarket.Name = "btnBuyMarket";
            this.btnBuyMarket.Size = new System.Drawing.Size(122, 85);
            this.btnBuyMarket.TabIndex = 39;
            this.btnBuyMarket.UseVisualStyleBackColor = true;
            this.btnBuyMarket.Click += new System.EventHandler(this.BtnBuyMarketClick);
            // 
            // btnSellMarket
            // 
            this.btnSellMarket.Location = new System.Drawing.Point(12, 66);
            this.btnSellMarket.Name = "btnSellMarket";
            this.btnSellMarket.Size = new System.Drawing.Size(122, 85);
            this.btnSellMarket.TabIndex = 38;
            this.btnSellMarket.UseVisualStyleBackColor = true;
            this.btnSellMarket.Click += new System.EventHandler(this.BtnSellMarketClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(140, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 40;
            this.label1.Tag = "TitleVolume";
            this.label1.Text = "объем";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(116, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 13);
            this.label6.TabIndex = 44;
            this.label6.Text = "TP";
            // 
            // tbTP
            // 
            this.tbTP.Location = new System.Drawing.Point(143, 162);
            this.tbTP.Name = "tbTP";
            this.tbTP.Size = new System.Drawing.Size(64, 20);
            this.tbTP.TabIndex = 42;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 43;
            this.label5.Text = "SL";
            // 
            // tbSL
            // 
            this.tbSL.Location = new System.Drawing.Point(38, 162);
            this.tbSL.Name = "tbSL";
            this.tbSL.Size = new System.Drawing.Size(64, 20);
            this.tbSL.TabIndex = 41;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(227, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 46;
            this.label2.Text = "MAGIC";
            // 
            // tbMagic
            // 
            this.tbMagic.Location = new System.Drawing.Point(273, 162);
            this.tbMagic.Name = "tbMagic";
            this.tbMagic.Size = new System.Drawing.Size(84, 20);
            this.tbMagic.TabIndex = 45;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 47;
            this.label3.Tag = "TitleComment";
            this.label3.Text = "Комментарий";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(95, 190);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(262, 20);
            this.tbComment.TabIndex = 48;
            // 
            // lblPrice
            // 
            this.lblPrice.AutoSize = true;
            this.lblPrice.Location = new System.Drawing.Point(140, 115);
            this.lblPrice.Name = "lblPrice";
            this.lblPrice.Size = new System.Drawing.Size(31, 13);
            this.lblPrice.TabIndex = 50;
            this.lblPrice.Tag = "TitlePrice";
            this.lblPrice.Text = "цена";
            // 
            // tbPrice
            // 
            this.tbPrice.Enabled = false;
            this.tbPrice.Location = new System.Drawing.Point(140, 131);
            this.tbPrice.Name = "tbPrice";
            this.tbPrice.Size = new System.Drawing.Size(89, 20);
            this.tbPrice.TabIndex = 51;
            // 
            // cbSlippage
            // 
            this.cbSlippage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbSlippage.AutoSize = true;
            this.cbSlippage.Location = new System.Drawing.Point(3, 4);
            this.cbSlippage.Name = "cbSlippage";
            this.cbSlippage.Size = new System.Drawing.Size(133, 17);
            this.cbSlippage.TabIndex = 54;
            this.cbSlippage.Tag = "TitleMaximumSlippageInPointsShort";
            this.cbSlippage.Text = "макс. проскальз., пп";
            this.cbSlippage.UseVisualStyleBackColor = true;
            this.cbSlippage.CheckedChanged += new System.EventHandler(this.CbSlippageCheckedChanged);
            // 
            // udSlippagePoints
            // 
            this.udSlippagePoints.DecimalPlaces = 1;
            this.udSlippagePoints.Enabled = false;
            this.udSlippagePoints.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udSlippagePoints.Location = new System.Drawing.Point(142, 3);
            this.udSlippagePoints.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.udSlippagePoints.Name = "udSlippagePoints";
            this.udSlippagePoints.Size = new System.Drawing.Size(64, 20);
            this.udSlippagePoints.TabIndex = 55;
            this.udSlippagePoints.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // cbMode
            // 
            this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMode.FormattingEnabled = true;
            this.cbMode.Location = new System.Drawing.Point(12, 12);
            this.cbMode.Name = "cbMode";
            this.cbMode.Size = new System.Drawing.Size(345, 21);
            this.cbMode.TabIndex = 56;
            this.cbMode.SelectedIndexChanged += new System.EventHandler(this.CbModeSelectedIndexChanged);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnEdit);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 270);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(370, 0);
            this.panelBottom.TabIndex = 57;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCancel.Location = new System.Drawing.Point(170, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(125, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отменить";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            this.btnEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnEdit.Location = new System.Drawing.Point(12, 6);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(125, 23);
            this.btnEdit.TabIndex = 0;
            this.btnEdit.Text = "Редактировать";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.BtnEditClick);
            // 
            // tabControlExtra
            // 
            this.tabControlExtra.Controls.Add(this.tabPageTrail);
            this.tabControlExtra.Controls.Add(this.tabPagePending);
            this.tabControlExtra.Controls.Add(this.tabPageOrderXtra);
            this.tabControlExtra.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlExtra.Location = new System.Drawing.Point(0, 275);
            this.tabControlExtra.Name = "tabControlExtra";
            this.tabControlExtra.SelectedIndex = 0;
            this.tabControlExtra.Size = new System.Drawing.Size(370, 0);
            this.tabControlExtra.TabIndex = 3;
            this.tabControlExtra.Visible = false;
            this.tabControlExtra.SelectedIndexChanged += new System.EventHandler(this.TabControlExtraSelectedIndexChanged);
            // 
            // tabPageTrail
            // 
            this.tabPageTrail.Controls.Add(this.gridTrailing);
            this.tabPageTrail.Location = new System.Drawing.Point(4, 22);
            this.tabPageTrail.Name = "tabPageTrail";
            this.tabPageTrail.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTrail.Size = new System.Drawing.Size(362, 0);
            this.tabPageTrail.TabIndex = 0;
            this.tabPageTrail.Tag = "TitleTrailing";
            this.tabPageTrail.Text = "Трейлинг";
            this.tabPageTrail.UseVisualStyleBackColor = true;
            // 
            // gridTrailing
            // 
            this.gridTrailing.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTrailing.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colPrice,
            this.colPips,
            this.colTarget});
            this.gridTrailing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTrailing.Location = new System.Drawing.Point(3, 3);
            this.gridTrailing.Name = "gridTrailing";
            this.gridTrailing.Size = new System.Drawing.Size(356, 0);
            this.gridTrailing.TabIndex = 1;
            this.gridTrailing.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridTrailingCellEndEdit);
            this.gridTrailing.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.GridTrailingCellValidating);
            this.gridTrailing.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.GridTrailingRowsRemoved);
            // 
            // colPrice
            // 
            this.colPrice.DataPropertyName = "Price";
            this.colPrice.HeaderText = "уровень";
            this.colPrice.Name = "colPrice";
            // 
            // colPips
            // 
            this.colPips.DataPropertyName = "Points";
            this.colPips.HeaderText = "уровень (пп)";
            this.colPips.Name = "colPips";
            // 
            // colTarget
            // 
            this.colTarget.DataPropertyName = "Target";
            this.colTarget.HeaderText = "SL (пп)";
            this.colTarget.Name = "colTarget";
            // 
            // tabPagePending
            // 
            this.tabPagePending.Controls.Add(this.tbPriceTo);
            this.tabPagePending.Controls.Add(this.cbPriceTo);
            this.tabPagePending.Controls.Add(this.cbEndTime);
            this.tabPagePending.Controls.Add(this.dpTimeTo);
            this.tabPagePending.Controls.Add(this.cbStartFrom);
            this.tabPagePending.Controls.Add(this.dpTimeFrom);
            this.tabPagePending.Controls.Add(this.cbOrderOCO);
            this.tabPagePending.Controls.Add(this.label7);
            this.tabPagePending.Location = new System.Drawing.Point(4, 22);
            this.tabPagePending.Name = "tabPagePending";
            this.tabPagePending.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePending.Size = new System.Drawing.Size(362, 0);
            this.tabPagePending.TabIndex = 1;
            this.tabPagePending.Tag = "TitleOrder";
            this.tabPagePending.Text = "Ордер";
            this.tabPagePending.UseVisualStyleBackColor = true;
            // 
            // tbPriceTo
            // 
            this.tbPriceTo.Location = new System.Drawing.Point(122, 90);
            this.tbPriceTo.Name = "tbPriceTo";
            this.tbPriceTo.Size = new System.Drawing.Size(100, 20);
            this.tbPriceTo.TabIndex = 9;
            // 
            // cbPriceTo
            // 
            this.cbPriceTo.AutoSize = true;
            this.cbPriceTo.Enabled = false;
            this.cbPriceTo.Location = new System.Drawing.Point(4, 93);
            this.cbPriceTo.Name = "cbPriceTo";
            this.cbPriceTo.Size = new System.Drawing.Size(96, 17);
            this.cbPriceTo.TabIndex = 8;
            this.cbPriceTo.Tag = "TitlePriceLimit";
            this.cbPriceTo.Text = "граница цены";
            this.cbPriceTo.UseVisualStyleBackColor = true;
            this.cbPriceTo.CheckedChanged += new System.EventHandler(this.CbPriceToCheckedChanged);
            // 
            // cbEndTime
            // 
            this.cbEndTime.AutoSize = true;
            this.cbEndTime.Enabled = false;
            this.cbEndTime.Location = new System.Drawing.Point(4, 69);
            this.cbEndTime.Name = "cbEndTime";
            this.cbEndTime.Size = new System.Drawing.Size(79, 17);
            this.cbEndTime.TabIndex = 7;
            this.cbEndTime.Tag = "TitleEnd";
            this.cbEndTime.Text = "окончание";
            this.cbEndTime.UseVisualStyleBackColor = true;
            this.cbEndTime.CheckedChanged += new System.EventHandler(this.CbEndTimeCheckedChanged);
            // 
            // dpTimeTo
            // 
            this.dpTimeTo.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dpTimeTo.Enabled = false;
            this.dpTimeTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpTimeTo.Location = new System.Drawing.Point(89, 67);
            this.dpTimeTo.Name = "dpTimeTo";
            this.dpTimeTo.Size = new System.Drawing.Size(133, 20);
            this.dpTimeTo.TabIndex = 6;
            // 
            // cbStartFrom
            // 
            this.cbStartFrom.AutoSize = true;
            this.cbStartFrom.Enabled = false;
            this.cbStartFrom.Location = new System.Drawing.Point(4, 46);
            this.cbStartFrom.Name = "cbStartFrom";
            this.cbStartFrom.Size = new System.Drawing.Size(61, 17);
            this.cbStartFrom.TabIndex = 5;
            this.cbStartFrom.Tag = "TitleBeginning";
            this.cbStartFrom.Text = "начало";
            this.cbStartFrom.UseVisualStyleBackColor = true;
            this.cbStartFrom.CheckedChanged += new System.EventHandler(this.CbStartFromCheckedChanged);
            // 
            // dpTimeFrom
            // 
            this.dpTimeFrom.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dpTimeFrom.Enabled = false;
            this.dpTimeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpTimeFrom.Location = new System.Drawing.Point(89, 44);
            this.dpTimeFrom.Name = "dpTimeFrom";
            this.dpTimeFrom.Size = new System.Drawing.Size(133, 20);
            this.dpTimeFrom.TabIndex = 4;
            // 
            // cbOrderOCO
            // 
            this.cbOrderOCO.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOrderOCO.Enabled = false;
            this.cbOrderOCO.FormattingEnabled = true;
            this.cbOrderOCO.Location = new System.Drawing.Point(3, 20);
            this.cbOrderOCO.Name = "cbOrderOCO";
            this.cbOrderOCO.Size = new System.Drawing.Size(342, 21);
            this.cbOrderOCO.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 4);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 2;
            this.label7.Tag = "TitleOrderOCO";
            this.label7.Text = "ордер OCO";
            // 
            // tabPageOrderXtra
            // 
            this.tabPageOrderXtra.Controls.Add(this.tbExpertComment);
            this.tabPageOrderXtra.Controls.Add(this.label4);
            this.tabPageOrderXtra.Location = new System.Drawing.Point(4, 22);
            this.tabPageOrderXtra.Name = "tabPageOrderXtra";
            this.tabPageOrderXtra.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOrderXtra.Size = new System.Drawing.Size(362, 0);
            this.tabPageOrderXtra.TabIndex = 2;
            this.tabPageOrderXtra.Tag = "TitleAdditional";
            this.tabPageOrderXtra.Text = "Дополнительно";
            this.tabPageOrderXtra.UseVisualStyleBackColor = true;
            // 
            // tbExpertComment
            // 
            this.tbExpertComment.Location = new System.Drawing.Point(6, 21);
            this.tbExpertComment.Name = "tbExpertComment";
            this.tbExpertComment.Size = new System.Drawing.Size(342, 20);
            this.tbExpertComment.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 2;
            this.label4.Tag = "TitleRobotComment";
            this.label4.Text = "комментарий робота";
            // 
            // cbExtra
            // 
            this.cbExtra.AutoSize = true;
            this.cbExtra.Location = new System.Drawing.Point(15, 248);
            this.cbExtra.Name = "cbExtra";
            this.cbExtra.Size = new System.Drawing.Size(180, 17);
            this.cbExtra.TabIndex = 1;
            this.cbExtra.Tag = "TitleAdditionalParameters";
            this.cbExtra.Text = "дополнительные параметры...";
            this.cbExtra.UseVisualStyleBackColor = true;
            this.cbExtra.CheckedChanged += new System.EventHandler(this.CbExtraCheckedChanged);
            // 
            // imageListGlyph
            // 
            this.imageListGlyph.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlyph.ImageStream")));
            this.imageListGlyph.TransparentColor = System.Drawing.Color.DarkKhaki;
            this.imageListGlyph.Images.SetKeyName(0, "ico_wizzard.png");
            // 
            // btnCalcVolume
            // 
            this.btnCalcVolume.BackColor = System.Drawing.SystemColors.Control;
            this.btnCalcVolume.ImageIndex = 0;
            this.btnCalcVolume.ImageList = this.imageListGlyph;
            this.btnCalcVolume.Location = new System.Drawing.Point(185, 62);
            this.btnCalcVolume.Name = "btnCalcVolume";
            this.btnCalcVolume.Size = new System.Drawing.Size(25, 23);
            this.btnCalcVolume.TabIndex = 59;
            this.btnCalcVolume.UseVisualStyleBackColor = false;
            this.btnCalcVolume.Click += new System.EventHandler(this.BtnCalcVolumeClick);
            // 
            // cbCurx
            // 
            this.cbCurx.FormattingEnabled = true;
            this.cbCurx.Location = new System.Drawing.Point(74, 39);
            this.cbCurx.Name = "cbCurx";
            this.cbCurx.Size = new System.Drawing.Size(121, 21);
            this.cbCurx.TabIndex = 60;
            this.cbCurx.SelectedIndexChanged += new System.EventHandler(this.CbCurxSelectedIndexChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cbSlippage);
            this.flowLayoutPanel1.Controls.Add(this.udSlippagePoints);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 216);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(345, 26);
            this.flowLayoutPanel1.TabIndex = 61;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.cbExtra);
            this.mainPanel.Controls.Add(this.cbMode);
            this.mainPanel.Controls.Add(this.flowLayoutPanel1);
            this.mainPanel.Controls.Add(this.label15);
            this.mainPanel.Controls.Add(this.cbCurx);
            this.mainPanel.Controls.Add(this.cbVolume);
            this.mainPanel.Controls.Add(this.btnCalcVolume);
            this.mainPanel.Controls.Add(this.btnSellMarket);
            this.mainPanel.Controls.Add(this.btnBuyMarket);
            this.mainPanel.Controls.Add(this.label1);
            this.mainPanel.Controls.Add(this.tbSL);
            this.mainPanel.Controls.Add(this.tbPrice);
            this.mainPanel.Controls.Add(this.label5);
            this.mainPanel.Controls.Add(this.lblPrice);
            this.mainPanel.Controls.Add(this.tbTP);
            this.mainPanel.Controls.Add(this.tbComment);
            this.mainPanel.Controls.Add(this.label6);
            this.mainPanel.Controls.Add(this.label3);
            this.mainPanel.Controls.Add(this.tbMagic);
            this.mainPanel.Controls.Add(this.label2);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(370, 275);
            this.mainPanel.TabIndex = 62;
            // 
            // OrderDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 270);
            this.Controls.Add(this.tabControlExtra);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(378, 297);
            this.Name = "OrderDlg";
            this.Tag = "TitleNewOrder";
            this.Text = "Новый ордер";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrderDialogFormClosing);
            this.Load += new System.EventHandler(this.OrderDialogLoad);
            ((System.ComponentModel.ISupportInitialize)(this.udSlippagePoints)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.tabControlExtra.ResumeLayout(false);
            this.tabPageTrail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTrailing)).EndInit();
            this.tabPagePending.ResumeLayout(false);
            this.tabPagePending.PerformLayout();
            this.tabPageOrderXtra.ResumeLayout(false);
            this.tabPageOrderXtra.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox cbVolume;
        private Controls.CustomTextButton btnBuyMarket;
        private Controls.CustomTextButton btnSellMarket;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbTP;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSL;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMagic;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.TextBox tbPrice;
        private System.Windows.Forms.CheckBox cbSlippage;
        private System.Windows.Forms.NumericUpDown udSlippagePoints;
        private System.Windows.Forms.ComboBox cbMode;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.TabControl tabControlExtra;
        private System.Windows.Forms.TabPage tabPageTrail;
        private System.Windows.Forms.TabPage tabPagePending;
        private System.Windows.Forms.CheckBox cbExtra;
        private System.Windows.Forms.DataGridView gridTrailing;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPips;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTarget;
        private System.Windows.Forms.ComboBox cbOrderOCO;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbPriceTo;
        private System.Windows.Forms.CheckBox cbPriceTo;
        private System.Windows.Forms.CheckBox cbEndTime;
        private System.Windows.Forms.DateTimePicker dpTimeTo;
        private System.Windows.Forms.CheckBox cbStartFrom;
        private System.Windows.Forms.DateTimePicker dpTimeFrom;
        private System.Windows.Forms.TabPage tabPageOrderXtra;
        private System.Windows.Forms.TextBox tbExpertComment;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ImageList imageListGlyph;
        private System.Windows.Forms.Button btnCalcVolume;
        private TickerComboBox cbCurx;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel mainPanel;
    }
}