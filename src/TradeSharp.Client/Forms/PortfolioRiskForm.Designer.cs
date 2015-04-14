using TradeSharp.Client.Controls;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    partial class PortfolioRiskForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortfolioRiskForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPagePortfolio = new System.Windows.Forms.TabPage();
            this.gridPortfolio = new FastGrid.FastGrid();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnSetup = new System.Windows.Forms.Button();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.tabPageCorrelation = new System.Windows.Forms.TabPage();
            this.rtbCorr = new System.Windows.Forms.RichTextBox();
            this.panelCorTop = new System.Windows.Forms.Panel();
            this.btnCalcRisk = new System.Windows.Forms.Button();
            this.tabPageChart = new System.Windows.Forms.TabPage();
            this.splitContainerChart = new System.Windows.Forms.SplitContainer();
            this.panelPcTop = new System.Windows.Forms.Panel();
            this.panelChartLeft = new System.Windows.Forms.Panel();
            this.cbPercentiles = new TradeSharp.Util.CheckedListBoxColorableDraggable();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDrawChart = new System.Windows.Forms.Button();
            this.chartPercent = new FastMultiChart.FastMultiChart();
            this.tabControl.SuspendLayout();
            this.tabPagePortfolio.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.tabPageCorrelation.SuspendLayout();
            this.panelCorTop.SuspendLayout();
            this.tabPageChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerChart)).BeginInit();
            this.splitContainerChart.Panel1.SuspendLayout();
            this.splitContainerChart.Panel2.SuspendLayout();
            this.splitContainerChart.SuspendLayout();
            this.panelPcTop.SuspendLayout();
            this.panelChartLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPagePortfolio);
            this.tabControl.Controls.Add(this.tabPageCorrelation);
            this.tabControl.Controls.Add(this.tabPageChart);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(426, 364);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControlSelectedIndexChanged);
            // 
            // tabPagePortfolio
            // 
            this.tabPagePortfolio.Controls.Add(this.gridPortfolio);
            this.tabPagePortfolio.Controls.Add(this.panelTop);
            this.tabPagePortfolio.Location = new System.Drawing.Point(4, 22);
            this.tabPagePortfolio.Name = "tabPagePortfolio";
            this.tabPagePortfolio.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePortfolio.Size = new System.Drawing.Size(418, 338);
            this.tabPagePortfolio.TabIndex = 0;
            this.tabPagePortfolio.Text = "Шаг 1. Портфель";
            this.tabPagePortfolio.UseVisualStyleBackColor = true;
            // 
            // gridPortfolio
            // 
            this.gridPortfolio.CaptionHeight = 20;
            this.gridPortfolio.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridPortfolio.CellHeight = 18;
            this.gridPortfolio.CellPadding = 5;
            this.gridPortfolio.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridPortfolio.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridPortfolio.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridPortfolio.ColorCellFont = System.Drawing.Color.Black;
            this.gridPortfolio.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridPortfolio.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridPortfolio.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.gridPortfolio.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridPortfolio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPortfolio.FontAnchoredRow = null;
            this.gridPortfolio.FontCell = null;
            this.gridPortfolio.FontHeader = null;
            this.gridPortfolio.FontSelectedCell = null;
            this.gridPortfolio.Location = new System.Drawing.Point(3, 33);
            this.gridPortfolio.MinimumTableWidth = null;
            this.gridPortfolio.MultiSelectEnabled = false;
            this.gridPortfolio.Name = "gridPortfolio";
            this.gridPortfolio.SelectEnabled = true;
            this.gridPortfolio.Size = new System.Drawing.Size(412, 302);
            this.gridPortfolio.StickFirst = false;
            this.gridPortfolio.StickLast = false;
            this.gridPortfolio.TabIndex = 1;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnSetup);
            this.panelTop.Controls.Add(this.btnCalculate);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(3, 3);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(412, 30);
            this.panelTop.TabIndex = 0;
            // 
            // btnSetup
            // 
            this.btnSetup.Location = new System.Drawing.Point(153, 3);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(101, 23);
            this.btnSetup.TabIndex = 1;
            this.btnSetup.Text = "Настроить...";
            this.btnSetup.UseVisualStyleBackColor = true;
            this.btnSetup.Click += new System.EventHandler(this.BtnSetupClick);
            // 
            // btnCalculate
            // 
            this.btnCalculate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCalculate.Location = new System.Drawing.Point(5, 3);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(142, 23);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "Оценить риски";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.BtnCalculateClick);
            // 
            // tabPageCorrelation
            // 
            this.tabPageCorrelation.Controls.Add(this.rtbCorr);
            this.tabPageCorrelation.Controls.Add(this.panelCorTop);
            this.tabPageCorrelation.Location = new System.Drawing.Point(4, 22);
            this.tabPageCorrelation.Name = "tabPageCorrelation";
            this.tabPageCorrelation.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCorrelation.Size = new System.Drawing.Size(418, 338);
            this.tabPageCorrelation.TabIndex = 1;
            this.tabPageCorrelation.Text = "Шаг 2. Корреляция";
            this.tabPageCorrelation.UseVisualStyleBackColor = true;
            // 
            // rtbCorr
            // 
            this.rtbCorr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCorr.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbCorr.Location = new System.Drawing.Point(3, 34);
            this.rtbCorr.Name = "rtbCorr";
            this.rtbCorr.Size = new System.Drawing.Size(412, 301);
            this.rtbCorr.TabIndex = 1;
            this.rtbCorr.Text = "";
            // 
            // panelCorTop
            // 
            this.panelCorTop.Controls.Add(this.btnCalcRisk);
            this.panelCorTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCorTop.Location = new System.Drawing.Point(3, 3);
            this.panelCorTop.Name = "panelCorTop";
            this.panelCorTop.Size = new System.Drawing.Size(412, 31);
            this.panelCorTop.TabIndex = 0;
            // 
            // btnCalcRisk
            // 
            this.btnCalcRisk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCalcRisk.Location = new System.Drawing.Point(5, 3);
            this.btnCalcRisk.Name = "btnCalcRisk";
            this.btnCalcRisk.Size = new System.Drawing.Size(107, 23);
            this.btnCalcRisk.TabIndex = 0;
            this.btnCalcRisk.Text = "Расчитать риск";
            this.btnCalcRisk.UseVisualStyleBackColor = true;
            this.btnCalcRisk.Click += new System.EventHandler(this.BtnCalcRiskClick);
            // 
            // tabPageChart
            // 
            this.tabPageChart.Controls.Add(this.splitContainerChart);
            this.tabPageChart.Location = new System.Drawing.Point(4, 22);
            this.tabPageChart.Name = "tabPageChart";
            this.tabPageChart.Size = new System.Drawing.Size(418, 338);
            this.tabPageChart.TabIndex = 2;
            this.tabPageChart.Text = "Шаг 3. Перцентили";
            this.tabPageChart.UseVisualStyleBackColor = true;
            // 
            // splitContainerChart
            // 
            this.splitContainerChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerChart.Location = new System.Drawing.Point(0, 0);
            this.splitContainerChart.Name = "splitContainerChart";
            this.splitContainerChart.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerChart.Panel1
            // 
            this.splitContainerChart.Panel1.Controls.Add(this.panelPcTop);
            // 
            // splitContainerChart.Panel2
            // 
            this.splitContainerChart.Panel2.Controls.Add(this.chartPercent);
            this.splitContainerChart.Size = new System.Drawing.Size(418, 338);
            this.splitContainerChart.SplitterDistance = 65;
            this.splitContainerChart.TabIndex = 0;
            // 
            // panelPcTop
            // 
            this.panelPcTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPcTop.Controls.Add(this.panelChartLeft);
            this.panelPcTop.Controls.Add(this.btnDrawChart);
            this.panelPcTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPcTop.Location = new System.Drawing.Point(0, 0);
            this.panelPcTop.Name = "panelPcTop";
            this.panelPcTop.Size = new System.Drawing.Size(418, 65);
            this.panelPcTop.TabIndex = 1;
            // 
            // panelChartLeft
            // 
            this.panelChartLeft.Controls.Add(this.cbPercentiles);
            this.panelChartLeft.Controls.Add(this.label1);
            this.panelChartLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelChartLeft.Location = new System.Drawing.Point(0, 0);
            this.panelChartLeft.Name = "panelChartLeft";
            this.panelChartLeft.Size = new System.Drawing.Size(187, 63);
            this.panelChartLeft.TabIndex = 2;
            // 
            // cbPercentiles
            // 
            this.cbPercentiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbPercentiles.DraggingEnabled = false;
            this.cbPercentiles.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbPercentiles.FormattingEnabled = true;
            this.cbPercentiles.Location = new System.Drawing.Point(0, 13);
            this.cbPercentiles.MarkPadding = 2;
            this.cbPercentiles.MarkSize = 8;
            this.cbPercentiles.Name = "cbPercentiles";
            this.cbPercentiles.RowColors = null;
            this.cbPercentiles.Size = new System.Drawing.Size(187, 50);
            this.cbPercentiles.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Перцентили";
            // 
            // btnDrawChart
            // 
            this.btnDrawChart.Location = new System.Drawing.Point(193, 5);
            this.btnDrawChart.Name = "btnDrawChart";
            this.btnDrawChart.Size = new System.Drawing.Size(75, 23);
            this.btnDrawChart.TabIndex = 1;
            this.btnDrawChart.Text = "Построить";
            this.btnDrawChart.UseVisualStyleBackColor = true;
            this.btnDrawChart.Click += new System.EventHandler(this.BtnDrawChartClick);
            // 
            // chartPercent
            // 
            this.chartPercent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.chartPercent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartPercent.Location = new System.Drawing.Point(0, 0);
            this.chartPercent.MarginXLeft = 10;
            this.chartPercent.MarginXRight = 10;
            this.chartPercent.MarginYBottom = 10;
            this.chartPercent.MarginYTop = 10;
            this.chartPercent.Name = "chartPercent";
            this.chartPercent.ScaleDivisionXMaxPixel = -1;
            this.chartPercent.ScaleDivisionXMinPixel = 70;
            this.chartPercent.ScaleDivisionYMaxPixel = 60;
            this.chartPercent.ScaleDivisionYMinPixel = 20;
            this.chartPercent.ScrollBarHeight = 80;
            this.chartPercent.Size = new System.Drawing.Size(418, 269);
            this.chartPercent.TabIndex = 0;
            // 
            // PortfolioRiskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 364);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PortfolioRiskForm";
            this.Text = "Оценка рисков по портфелю";
            this.Load += new System.EventHandler(this.PortfolioRiskFormLoad);
            this.tabControl.ResumeLayout(false);
            this.tabPagePortfolio.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.tabPageCorrelation.ResumeLayout(false);
            this.panelCorTop.ResumeLayout(false);
            this.tabPageChart.ResumeLayout(false);
            this.splitContainerChart.Panel1.ResumeLayout(false);
            this.splitContainerChart.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerChart)).EndInit();
            this.splitContainerChart.ResumeLayout(false);
            this.panelPcTop.ResumeLayout(false);
            this.panelChartLeft.ResumeLayout(false);
            this.panelChartLeft.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPagePortfolio;
        private FastGrid.FastGrid gridPortfolio;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.TabPage tabPageCorrelation;
        private System.Windows.Forms.Panel panelCorTop;
        private System.Windows.Forms.RichTextBox rtbCorr;
        private System.Windows.Forms.Button btnCalcRisk;
        private System.Windows.Forms.TabPage tabPageChart;
        private System.Windows.Forms.SplitContainer splitContainerChart;
        private System.Windows.Forms.Panel panelPcTop;
        private System.Windows.Forms.Panel panelChartLeft;
        private CheckedListBoxColorableDraggable cbPercentiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDrawChart;
        private System.Windows.Forms.Button btnSetup;
        private FastMultiChart.FastMultiChart chartPercent;
    }
}