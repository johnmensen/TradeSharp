namespace IndexSpectrum
{
    partial class ChartForm
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
            DevExpress.XtraCharts.XYDiagram xyDiagram10 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series10 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.PointSeriesLabel pointSeriesLabel7 = new DevExpress.XtraCharts.PointSeriesLabel();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView13 = new DevExpress.XtraCharts.LineSeriesView();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView14 = new DevExpress.XtraCharts.LineSeriesView();
            DevExpress.XtraCharts.XYDiagram xyDiagram11 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series11 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.PointSeriesLabel pointSeriesLabel8 = new DevExpress.XtraCharts.PointSeriesLabel();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView15 = new DevExpress.XtraCharts.LineSeriesView();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView16 = new DevExpress.XtraCharts.LineSeriesView();
            DevExpress.XtraCharts.XYDiagram xyDiagram12 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series12 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.SideBySideBarSeriesLabel sideBySideBarSeriesLabel4 = new DevExpress.XtraCharts.SideBySideBarSeriesLabel();
            DevExpress.XtraCharts.SideBySideBarSeriesView sideBySideBarSeriesView4 = new DevExpress.XtraCharts.SideBySideBarSeriesView();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnHurst = new System.Windows.Forms.Button();
            this.btnTimeVolume = new System.Windows.Forms.Button();
            this.btnSynthTest = new System.Windows.Forms.Button();
            this.cbIndexType = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cbSpectrX = new System.Windows.Forms.ComboBox();
            this.cbUseStartDate = new System.Windows.Forms.CheckBox();
            this.dpStartDate = new System.Windows.Forms.DateTimePicker();
            this.tbHarmAmpls = new System.Windows.Forms.RichTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbHarmPeak = new System.Windows.Forms.TextBox();
            this.tbHarmonics = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbDataCount = new System.Windows.Forms.TextBox();
            this.tbInterval = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbIndexFunction = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMAPeriod = new System.Windows.Forms.TextBox();
            this.cbTrendType = new System.Windows.Forms.ComboBox();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFolder = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chartIndex = new DevExpress.XtraCharts.ChartControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.chartIndexWoTrend = new DevExpress.XtraCharts.ChartControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.chartSpectrum = new DevExpress.XtraCharts.ChartControl();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView14)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartIndexWoTrend)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView15)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView16)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartSpectrum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(sideBySideBarSeriesLabel4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(sideBySideBarSeriesView4)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Индекс";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(489, 379);
            this.tabControl.TabIndex = 14;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnHurst);
            this.tabPage1.Controls.Add(this.btnTimeVolume);
            this.tabPage1.Controls.Add(this.btnSynthTest);
            this.tabPage1.Controls.Add(this.cbIndexType);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.cbSpectrX);
            this.tabPage1.Controls.Add(this.cbUseStartDate);
            this.tabPage1.Controls.Add(this.dpStartDate);
            this.tabPage1.Controls.Add(this.tbHarmAmpls);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.tbHarmPeak);
            this.tabPage1.Controls.Add(this.tbHarmonics);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.tbDataCount);
            this.tabPage1.Controls.Add(this.tbInterval);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.cbIndexFunction);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.tbMAPeriod);
            this.tabPage1.Controls.Add(this.cbTrendType);
            this.tabPage1.Controls.Add(this.btnCalculate);
            this.tabPage1.Controls.Add(this.btnBrowse);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.tbFolder);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(481, 353);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Настройки";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnHurst
            // 
            this.btnHurst.Location = new System.Drawing.Point(433, 127);
            this.btnHurst.Name = "btnHurst";
            this.btnHurst.Size = new System.Drawing.Size(45, 23);
            this.btnHurst.TabIndex = 39;
            this.btnHurst.Text = "Hurst";
            this.btnHurst.UseVisualStyleBackColor = true;
            this.btnHurst.Click += new System.EventHandler(this.btnHurst_Click);
            // 
            // btnTimeVolume
            // 
            this.btnTimeVolume.Location = new System.Drawing.Point(433, 103);
            this.btnTimeVolume.Name = "btnTimeVolume";
            this.btnTimeVolume.Size = new System.Drawing.Size(45, 23);
            this.btnTimeVolume.TabIndex = 38;
            this.btnTimeVolume.Text = "T-V";
            this.btnTimeVolume.UseVisualStyleBackColor = true;
            this.btnTimeVolume.Click += new System.EventHandler(this.btnTimeVolume_Click);
            // 
            // btnSynthTest
            // 
            this.btnSynthTest.Location = new System.Drawing.Point(353, 18);
            this.btnSynthTest.Name = "btnSynthTest";
            this.btnSynthTest.Size = new System.Drawing.Size(87, 21);
            this.btnSynthTest.TabIndex = 37;
            this.btnSynthTest.Text = "Тест С.И.";
            this.btnSynthTest.UseVisualStyleBackColor = true;
            this.btnSynthTest.Click += new System.EventHandler(this.btnSynthTest_Click);
            // 
            // cbIndexType
            // 
            this.cbIndexType.FormattingEnabled = true;
            this.cbIndexType.Items.AddRange(new object[] {
            "Сумма",
            "Приращение"});
            this.cbIndexType.Location = new System.Drawing.Point(340, 60);
            this.cbIndexType.Name = "cbIndexType";
            this.cbIndexType.Size = new System.Drawing.Size(100, 21);
            this.cbIndexType.TabIndex = 36;
            this.cbIndexType.Text = "Сумма";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(350, 142);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 13);
            this.label9.TabIndex = 35;
            this.label9.Text = "ось X (спектр)";
            // 
            // cbSpectrX
            // 
            this.cbSpectrX.FormattingEnabled = true;
            this.cbSpectrX.Items.AddRange(new object[] {
            "Частота",
            "Период"});
            this.cbSpectrX.Location = new System.Drawing.Point(244, 139);
            this.cbSpectrX.Name = "cbSpectrX";
            this.cbSpectrX.Size = new System.Drawing.Size(100, 21);
            this.cbSpectrX.TabIndex = 34;
            this.cbSpectrX.Text = "Частота";
            // 
            // cbUseStartDate
            // 
            this.cbUseStartDate.AutoSize = true;
            this.cbUseStartDate.Checked = true;
            this.cbUseStartDate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseStartDate.Location = new System.Drawing.Point(244, 88);
            this.cbUseStartDate.Name = "cbUseStartDate";
            this.cbUseStartDate.Size = new System.Drawing.Size(100, 17);
            this.cbUseStartDate.TabIndex = 33;
            this.cbUseStartDate.Text = "начать с даты:";
            this.cbUseStartDate.UseVisualStyleBackColor = true;
            // 
            // dpStartDate
            // 
            this.dpStartDate.Location = new System.Drawing.Point(244, 113);
            this.dpStartDate.Name = "dpStartDate";
            this.dpStartDate.Size = new System.Drawing.Size(130, 20);
            this.dpStartDate.TabIndex = 32;
            this.dpStartDate.Value = new System.DateTime(2010, 1, 3, 0, 0, 0, 0);
            // 
            // tbHarmAmpls
            // 
            this.tbHarmAmpls.Location = new System.Drawing.Point(246, 206);
            this.tbHarmAmpls.Name = "tbHarmAmpls";
            this.tbHarmAmpls.Size = new System.Drawing.Size(227, 141);
            this.tbHarmAmpls.TabIndex = 31;
            this.tbHarmAmpls.Text = "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(281, 169);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(128, 13);
            this.label8.TabIndex = 30;
            this.label8.Text = "период пика гармоники";
            // 
            // tbHarmPeak
            // 
            this.tbHarmPeak.Location = new System.Drawing.Point(244, 166);
            this.tbHarmPeak.Name = "tbHarmPeak";
            this.tbHarmPeak.Size = new System.Drawing.Size(34, 20);
            this.tbHarmPeak.TabIndex = 29;
            this.tbHarmPeak.Text = "5";
            // 
            // tbHarmonics
            // 
            this.tbHarmonics.Location = new System.Drawing.Point(8, 206);
            this.tbHarmonics.Name = "tbHarmonics";
            this.tbHarmonics.Size = new System.Drawing.Size(222, 141);
            this.tbHarmonics.TabIndex = 28;
            this.tbHarmonics.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 190);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(150, 13);
            this.label7.TabIndex = 27;
            this.label7.Text = "Преобладающие гармоники";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(178, 130);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "отсчетов";
            // 
            // tbDataCount
            // 
            this.tbDataCount.Location = new System.Drawing.Point(138, 127);
            this.tbDataCount.Name = "tbDataCount";
            this.tbDataCount.Size = new System.Drawing.Size(34, 20);
            this.tbDataCount.TabIndex = 25;
            this.tbDataCount.Text = "600000";
            // 
            // tbInterval
            // 
            this.tbInterval.Location = new System.Drawing.Point(7, 127);
            this.tbInterval.Name = "tbInterval";
            this.tbInterval.Size = new System.Drawing.Size(34, 20);
            this.tbInterval.TabIndex = 24;
            this.tbInterval.Text = "1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 130);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "интервал, минут";
            // 
            // cbIndexFunction
            // 
            this.cbIndexFunction.FormattingEnabled = true;
            this.cbIndexFunction.Items.AddRange(new object[] {
            "/*EURX*/ eur eurgbp eurjpy eurchf eurcad euraud",
            "/*USDX*/ eur# gbp# chf jpy cad aud# nzd# "});
            this.cbIndexFunction.Location = new System.Drawing.Point(7, 60);
            this.cbIndexFunction.Name = "cbIndexFunction";
            this.cbIndexFunction.Size = new System.Drawing.Size(327, 21);
            this.cbIndexFunction.TabIndex = 22;
            this.cbIndexFunction.Text = "/*EURX*/ eur eurgbp eurjpy eurchf eurcad euraud";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Удаление тренда:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "период СС";
            // 
            // tbMAPeriod
            // 
            this.tbMAPeriod.Location = new System.Drawing.Point(138, 100);
            this.tbMAPeriod.Name = "tbMAPeriod";
            this.tbMAPeriod.Size = new System.Drawing.Size(34, 20);
            this.tbMAPeriod.TabIndex = 19;
            this.tbMAPeriod.Text = "256";
            // 
            // cbTrendType
            // 
            this.cbTrendType.FormattingEnabled = true;
            this.cbTrendType.Items.AddRange(new object[] {
            "Линейный",
            "СС"});
            this.cbTrendType.Location = new System.Drawing.Point(7, 100);
            this.cbTrendType.Name = "cbTrendType";
            this.cbTrendType.Size = new System.Drawing.Size(101, 21);
            this.cbTrendType.TabIndex = 18;
            this.cbTrendType.Text = "Линейный";
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(7, 160);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(165, 23);
            this.btnCalculate.TabIndex = 17;
            this.btnCalculate.Text = "Считать";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(310, 18);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 21);
            this.btnBrowse.TabIndex = 16;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Каталог котировок";
            // 
            // tbFolder
            // 
            this.tbFolder.Location = new System.Drawing.Point(7, 19);
            this.tbFolder.Name = "tbFolder";
            this.tbFolder.Size = new System.Drawing.Size(297, 20);
            this.tbFolder.TabIndex = 14;
            this.tbFolder.Text = "D:\\Sources\\NonVersioned\\!MTS\\quotes\\mts_quote_base";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chartIndex);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(481, 353);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Индекс";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chartIndex
            // 
            this.chartIndex.AppearanceName = "Light";
            xyDiagram10.AxisX.Range.SideMarginsEnabled = true;
            xyDiagram10.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram10.AxisY.Range.AlwaysShowZeroLevel = false;
            xyDiagram10.AxisY.Range.SideMarginsEnabled = false;
            xyDiagram10.AxisY.VisibleInPanesSerializable = "-1";
            this.chartIndex.Diagram = xyDiagram10;
            this.chartIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartIndex.Legend.Visible = false;
            this.chartIndex.Location = new System.Drawing.Point(3, 3);
            this.chartIndex.Name = "chartIndex";
            this.chartIndex.PaletteBaseColorNumber = 1;
            this.chartIndex.PaletteName = "Black and White";
            series10.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            pointSeriesLabel7.OverlappingOptionsTypeName = "PointOverlappingOptions";
            pointSeriesLabel7.Visible = false;
            series10.Label = pointSeriesLabel7;
            series10.Name = "SeriesIndex";
            lineSeriesView13.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            lineSeriesView13.LineMarkerOptions.Visible = false;
            lineSeriesView13.LineStyle.Thickness = 1;
            series10.View = lineSeriesView13;
            this.chartIndex.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series10};
            this.chartIndex.SeriesTemplate.View = lineSeriesView14;
            this.chartIndex.Size = new System.Drawing.Size(475, 347);
            this.chartIndex.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.chartIndexWoTrend);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(481, 353);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Индекс без тренда";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // chartIndexWoTrend
            // 
            this.chartIndexWoTrend.AppearanceName = "Light";
            xyDiagram11.AxisX.Range.SideMarginsEnabled = true;
            xyDiagram11.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram11.AxisY.Range.AlwaysShowZeroLevel = false;
            xyDiagram11.AxisY.Range.SideMarginsEnabled = false;
            xyDiagram11.AxisY.VisibleInPanesSerializable = "-1";
            this.chartIndexWoTrend.Diagram = xyDiagram11;
            this.chartIndexWoTrend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartIndexWoTrend.Legend.Visible = false;
            this.chartIndexWoTrend.Location = new System.Drawing.Point(0, 0);
            this.chartIndexWoTrend.Name = "chartIndexWoTrend";
            this.chartIndexWoTrend.PaletteBaseColorNumber = 1;
            this.chartIndexWoTrend.PaletteName = "Black and White";
            series11.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            pointSeriesLabel8.OverlappingOptionsTypeName = "PointOverlappingOptions";
            pointSeriesLabel8.Visible = false;
            series11.Label = pointSeriesLabel8;
            series11.Name = "SeriesIndex";
            lineSeriesView15.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            lineSeriesView15.LineMarkerOptions.Visible = false;
            lineSeriesView15.LineStyle.Thickness = 1;
            series11.View = lineSeriesView15;
            this.chartIndexWoTrend.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series11};
            this.chartIndexWoTrend.SeriesTemplate.View = lineSeriesView16;
            this.chartIndexWoTrend.Size = new System.Drawing.Size(481, 353);
            this.chartIndexWoTrend.TabIndex = 1;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.chartSpectrum);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(481, 353);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Спектр";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // chartSpectrum
            // 
            this.chartSpectrum.AppearanceName = "Light";
            xyDiagram12.AxisX.Range.SideMarginsEnabled = true;
            xyDiagram12.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram12.AxisY.Range.AlwaysShowZeroLevel = false;
            xyDiagram12.AxisY.Range.SideMarginsEnabled = false;
            xyDiagram12.AxisY.VisibleInPanesSerializable = "-1";
            this.chartSpectrum.Diagram = xyDiagram12;
            this.chartSpectrum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartSpectrum.Legend.Visible = false;
            this.chartSpectrum.Location = new System.Drawing.Point(0, 0);
            this.chartSpectrum.Name = "chartSpectrum";
            this.chartSpectrum.PaletteBaseColorNumber = 1;
            this.chartSpectrum.PaletteName = "Black and White";
            series12.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            sideBySideBarSeriesLabel4.OverlappingOptionsTypeName = "OverlappingOptions";
            sideBySideBarSeriesLabel4.Visible = false;
            series12.Label = sideBySideBarSeriesLabel4;
            series12.Name = "SeriesIndex";
            sideBySideBarSeriesView4.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            series12.View = sideBySideBarSeriesView4;
            this.chartSpectrum.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series12};
            this.chartSpectrum.Size = new System.Drawing.Size(481, 353);
            this.chartSpectrum.TabIndex = 2;
            this.chartSpectrum.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartSpectrum_MouseClick);
            // 
            // ChartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 379);
            this.Controls.Add(this.tabControl);
            this.Name = "ChartForm";
            this.Text = "ДПФ";
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(xyDiagram10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView14)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartIndex)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(xyDiagram11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView15)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView16)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartIndexWoTrend)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(xyDiagram12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(sideBySideBarSeriesLabel4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(sideBySideBarSeriesView4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartSpectrum)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbDataCount;
        private System.Windows.Forms.TextBox tbInterval;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbIndexFunction;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMAPeriod;
        private System.Windows.Forms.ComboBox cbTrendType;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFolder;
        private System.Windows.Forms.TabPage tabPage2;
        private DevExpress.XtraCharts.ChartControl chartIndex;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private DevExpress.XtraCharts.ChartControl chartIndexWoTrend;
        private DevExpress.XtraCharts.ChartControl chartSpectrum;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbHarmPeak;
        private System.Windows.Forms.RichTextBox tbHarmonics;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RichTextBox tbHarmAmpls;
        private System.Windows.Forms.CheckBox cbUseStartDate;
        private System.Windows.Forms.DateTimePicker dpStartDate;
        private System.Windows.Forms.ComboBox cbSpectrX;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cbIndexType;
        private System.Windows.Forms.Button btnSynthTest;
        private System.Windows.Forms.Button btnTimeVolume;
        private System.Windows.Forms.Button btnHurst;
    }
}

