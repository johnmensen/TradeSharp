namespace TradeSharp.Client.Forms
{
    partial class StatInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatInfoForm));
            this.btnClose = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.contextMenuPositions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabStat = new System.Windows.Forms.TabControl();
            this.tabPageTrades = new System.Windows.Forms.TabPage();
            this.AccountHistoryCtrl = new TradeSharp.Client.Controls.AccountHistoryControl();
            this.tabPageEquity = new System.Windows.Forms.TabPage();
            this.chart = new FastMultiChart.FastMultiChart();
            this.tabPageStat = new System.Windows.Forms.TabPage();
            this.gridStat = new FastGrid.FastGrid();
            this.panelExport = new System.Windows.Forms.Panel();
            this.btnExportStat = new System.Windows.Forms.Button();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.gridLog = new FastGrid.FastGrid();
            this.contextMenuPositions.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabStat.SuspendLayout();
            this.tabPageTrades.SuspendLayout();
            this.tabPageEquity.SuspendLayout();
            this.tabPageStat.SuspendLayout();
            this.panelExport.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(802, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Tag = "TitleClose";
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "xls";
            this.saveFileDialog.Filter = "Excel CSV (*.xls)|*.xls|Text file (*.txt)|*.txt";
            this.saveFileDialog.FilterIndex = 0;
            this.saveFileDialog.Title = "Экспорт результата";
            // 
            // contextMenuPositions
            // 
            this.contextMenuPositions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemExport});
            this.contextMenuPositions.Name = "contextMenuPositions";
            this.contextMenuPositions.Size = new System.Drawing.Size(129, 26);
            // 
            // menuitemExport
            // 
            this.menuitemExport.Name = "menuitemExport";
            this.menuitemExport.Size = new System.Drawing.Size(128, 22);
            this.menuitemExport.Text = "Экспорт...";
            this.menuitemExport.Click += new System.EventHandler(this.MenuitemExportClick);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnClose);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 459);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(880, 29);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // tabStat
            // 
            this.tabStat.Controls.Add(this.tabPageTrades);
            this.tabStat.Controls.Add(this.tabPageEquity);
            this.tabStat.Controls.Add(this.tabPageStat);
            this.tabStat.Controls.Add(this.tabPageLog);
            this.tabStat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabStat.Location = new System.Drawing.Point(0, 0);
            this.tabStat.Name = "tabStat";
            this.tabStat.SelectedIndex = 0;
            this.tabStat.Size = new System.Drawing.Size(880, 459);
            this.tabStat.TabIndex = 2;
            // 
            // tabPageTrades
            // 
            this.tabPageTrades.Controls.Add(this.AccountHistoryCtrl);
            this.tabPageTrades.Location = new System.Drawing.Point(4, 22);
            this.tabPageTrades.Name = "tabPageTrades";
            this.tabPageTrades.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTrades.Size = new System.Drawing.Size(872, 433);
            this.tabPageTrades.TabIndex = 0;
            this.tabPageTrades.Tag = "TitleOperationsHistory";
            this.tabPageTrades.Text = "История операций";
            this.tabPageTrades.UseVisualStyleBackColor = true;
            // 
            // AccountHistoryCtrl
            // 
            this.AccountHistoryCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AccountHistoryCtrl.Location = new System.Drawing.Point(3, 3);
            this.AccountHistoryCtrl.Name = "AccountHistoryCtrl";
            this.AccountHistoryCtrl.Size = new System.Drawing.Size(866, 427);
            this.AccountHistoryCtrl.TabIndex = 0;
            // 
            // tabPageEquity
            // 
            this.tabPageEquity.Controls.Add(this.chart);
            this.tabPageEquity.Location = new System.Drawing.Point(4, 22);
            this.tabPageEquity.Name = "tabPageEquity";
            this.tabPageEquity.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageEquity.Size = new System.Drawing.Size(872, 433);
            this.tabPageEquity.TabIndex = 1;
            this.tabPageEquity.Tag = "TitleProfitChart";
            this.tabPageEquity.Text = "График Equity";
            this.tabPageEquity.UseVisualStyleBackColor = true;
            // 
            // chart
            // 
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.InnerMarginXLeft = 10;
            this.chart.Location = new System.Drawing.Point(3, 3);
            this.chart.MarginXLeft = 10;
            this.chart.MarginXRight = 10;
            this.chart.MarginYBottom = 10;
            this.chart.MarginYTop = 10;
            this.chart.Name = "chart";
            this.chart.RenderPolygons = true;
            this.chart.ScaleDivisionXMaxPixel = -1;
            this.chart.ScaleDivisionXMinPixel = 70;
            this.chart.ScaleDivisionYMaxPixel = 60;
            this.chart.ScaleDivisionYMinPixel = 20;
            this.chart.ScrollBarHeight = 80;
            this.chart.ShowHints = true;
            this.chart.ShowScaleDivisionXLabel = true;
            this.chart.ShowScaleDivisionYLabel = true;
            this.chart.Size = new System.Drawing.Size(866, 427);
            this.chart.TabIndex = 0;
            // 
            // tabPageStat
            // 
            this.tabPageStat.Controls.Add(this.gridStat);
            this.tabPageStat.Controls.Add(this.panelExport);
            this.tabPageStat.Location = new System.Drawing.Point(4, 22);
            this.tabPageStat.Name = "tabPageStat";
            this.tabPageStat.Size = new System.Drawing.Size(872, 433);
            this.tabPageStat.TabIndex = 2;
            this.tabPageStat.Tag = "TitleStatistics";
            this.tabPageStat.Text = "Стат. показатели";
            this.tabPageStat.UseVisualStyleBackColor = true;
            // 
            // gridStat
            // 
            this.gridStat.CaptionHeight = 20;
            this.gridStat.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridStat.CellHeight = 18;
            this.gridStat.CellPadding = 5;
            this.gridStat.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridStat.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridStat.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridStat.ColorCellFont = System.Drawing.Color.Black;
            this.gridStat.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridStat.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridStat.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridStat.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridStat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridStat.FitWidth = true;
            this.gridStat.FontAnchoredRow = null;
            this.gridStat.FontCell = null;
            this.gridStat.FontHeader = null;
            this.gridStat.FontSelectedCell = null;
            this.gridStat.Location = new System.Drawing.Point(0, 0);
            this.gridStat.MinimumTableWidth = null;
            this.gridStat.MultiSelectEnabled = false;
            this.gridStat.Name = "gridStat";
            this.gridStat.SelectEnabled = true;
            this.gridStat.Size = new System.Drawing.Size(872, 402);
            this.gridStat.StickFirst = false;
            this.gridStat.StickLast = false;
            this.gridStat.TabIndex = 2;
            // 
            // panelExport
            // 
            this.panelExport.Controls.Add(this.btnExportStat);
            this.panelExport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelExport.Location = new System.Drawing.Point(0, 402);
            this.panelExport.Name = "panelExport";
            this.panelExport.Size = new System.Drawing.Size(872, 31);
            this.panelExport.TabIndex = 1;
            // 
            // btnExportStat
            // 
            this.btnExportStat.Location = new System.Drawing.Point(7, 4);
            this.btnExportStat.Name = "btnExportStat";
            this.btnExportStat.Size = new System.Drawing.Size(75, 23);
            this.btnExportStat.TabIndex = 0;
            this.btnExportStat.Tag = "TitleExportMenu";
            this.btnExportStat.Text = "Экспорт...";
            this.btnExportStat.UseVisualStyleBackColor = true;
            this.btnExportStat.Click += new System.EventHandler(this.BtnExportStatClick);
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.gridLog);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Size = new System.Drawing.Size(872, 433);
            this.tabPageLog.TabIndex = 3;
            this.tabPageLog.Tag = "TitleLog";
            this.tabPageLog.Text = "Лог";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // gridLog
            // 
            this.gridLog.CaptionHeight = 20;
            this.gridLog.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridLog.CellHeight = 18;
            this.gridLog.CellPadding = 5;
            this.gridLog.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridLog.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridLog.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridLog.ColorCellFont = System.Drawing.Color.Black;
            this.gridLog.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridLog.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridLog.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridLog.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLog.FitWidth = true;
            this.gridLog.FontAnchoredRow = null;
            this.gridLog.FontCell = null;
            this.gridLog.FontHeader = null;
            this.gridLog.FontSelectedCell = null;
            this.gridLog.Location = new System.Drawing.Point(0, 0);
            this.gridLog.MinimumTableWidth = null;
            this.gridLog.MultiSelectEnabled = true;
            this.gridLog.Name = "gridLog";
            this.gridLog.SelectEnabled = true;
            this.gridLog.Size = new System.Drawing.Size(872, 433);
            this.gridLog.StickFirst = false;
            this.gridLog.StickLast = false;
            this.gridLog.TabIndex = 0;
            // 
            // StatInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 488);
            this.Controls.Add(this.tabStat);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StatInfoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleAccountStatisticsInTestMode";
            this.Text = "Статистика по счету (режим тестирования)";
            this.contextMenuPositions.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tabStat.ResumeLayout(false);
            this.tabPageTrades.ResumeLayout(false);
            this.tabPageEquity.ResumeLayout(false);
            this.tabPageStat.ResumeLayout(false);
            this.panelExport.ResumeLayout(false);
            this.tabPageLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ContextMenuStrip contextMenuPositions;
        private System.Windows.Forms.ToolStripMenuItem menuitemExport;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TabControl tabStat;
        private System.Windows.Forms.TabPage tabPageTrades;
        private Controls.AccountHistoryControl AccountHistoryCtrl;
        private System.Windows.Forms.TabPage tabPageEquity;
        private FastMultiChart.FastMultiChart chart;
        private System.Windows.Forms.TabPage tabPageStat;
        private FastGrid.FastGrid gridStat;
        private System.Windows.Forms.Panel panelExport;
        private System.Windows.Forms.Button btnExportStat;
        private System.Windows.Forms.TabPage tabPageLog;
        private FastGrid.FastGrid gridLog;
    }
}