namespace TradeSharp.Client.Subscription.Dialog
{
    partial class PerformersSummaryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformersSummaryForm));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.gridStat = new FastGrid.FastGrid();
            this.popupTextBox1 = new FastGrid.PopupTextBox();
            this.chartProfit1000 = new FastMultiChart.FastMultiChart();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.gridStat);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.chartProfit1000);
            this.splitContainer.Size = new System.Drawing.Size(543, 335);
            this.splitContainer.SplitterDistance = 240;
            this.splitContainer.TabIndex = 0;
            // 
            // gridStat
            // 
            this.gridStat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
            this.gridStat.FitWidth = false;
            this.gridStat.FontAnchoredRow = null;
            this.gridStat.FontCell = null;
            this.gridStat.FontHeader = null;
            this.gridStat.FontSelectedCell = null;
            this.gridStat.Location = new System.Drawing.Point(0, 0);
            this.gridStat.MinimumTableWidth = null;
            this.gridStat.MultiSelectEnabled = false;
            this.gridStat.Name = "gridStat";
            this.gridStat.SelectEnabled = true;
            this.gridStat.Size = new System.Drawing.Size(240, 335);
            this.gridStat.StickFirst = false;
            this.gridStat.StickLast = false;
            this.gridStat.TabIndex = 0;
            // 
            // popupTextBox1
            // 
            this.popupTextBox1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.popupTextBox1.Name = "popupTextBox1";
            this.popupTextBox1.Size = new System.Drawing.Size(2, 4);
            // 
            // chartProfit1000
            // 
            this.chartProfit1000.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.chartProfit1000.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartProfit1000.InnerMarginXLeft = 10;
            this.chartProfit1000.Location = new System.Drawing.Point(0, 0);
            this.chartProfit1000.MarginXLeft = 10;
            this.chartProfit1000.MarginXRight = 10;
            this.chartProfit1000.MarginYBottom = 10;
            this.chartProfit1000.MarginYTop = 10;
            this.chartProfit1000.Name = "chartProfit1000";
            this.chartProfit1000.RenderPolygons = true;
            this.chartProfit1000.ScaleDivisionXMaxPixel = -1;
            this.chartProfit1000.ScaleDivisionXMinPixel = 70;
            this.chartProfit1000.ScaleDivisionYMaxPixel = 60;
            this.chartProfit1000.ScaleDivisionYMinPixel = 20;
            this.chartProfit1000.ScrollBarHeight = 80;
            this.chartProfit1000.ShowHints = true;
            this.chartProfit1000.ShowScaleDivisionXLabel = true;
            this.chartProfit1000.ShowScaleDivisionYLabel = true;
            this.chartProfit1000.Size = new System.Drawing.Size(299, 335);
            this.chartProfit1000.TabIndex = 0;
            // 
            // PerformersSummaryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 335);
            this.Controls.Add(this.splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PerformersSummaryForm";
            this.Text = "Суммарный результат стратегий";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PerformersSummaryForm_FormClosing);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private FastGrid.FastGrid gridStat;
        private FastMultiChart.FastMultiChart chartProfit1000;
        private FastGrid.PopupTextBox popupTextBox1;
    }
}