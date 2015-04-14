namespace Candlechart.Controls
{
    partial class CandleMenuWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CandleMenuWindow));
            this.panelTop = new System.Windows.Forms.Panel();
            this.cbShowHL = new System.Windows.Forms.CheckBox();
            this.btnRefreshChart = new System.Windows.Forms.Button();
            this.tbInfo = new System.Windows.Forms.TextBox();
            this.chart = new FastChart.Chart.FastChart();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.cbShowHL);
            this.panelTop.Controls.Add(this.btnRefreshChart);
            this.panelTop.Controls.Add(this.tbInfo);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(460, 56);
            this.panelTop.TabIndex = 0;
            // 
            // cbShowHL
            // 
            this.cbShowHL.AutoSize = true;
            this.cbShowHL.Location = new System.Drawing.Point(250, 7);
            this.cbShowHL.Name = "cbShowHL";
            this.cbShowHL.Size = new System.Drawing.Size(107, 17);
            this.cbShowHL.TabIndex = 2;
            this.cbShowHL.Text = "показывать H-L";
            this.cbShowHL.UseVisualStyleBackColor = true;
            // 
            // btnRefreshChart
            // 
            this.btnRefreshChart.Location = new System.Drawing.Point(249, 30);
            this.btnRefreshChart.Name = "btnRefreshChart";
            this.btnRefreshChart.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshChart.TabIndex = 1;
            this.btnRefreshChart.Text = "Обновить";
            this.btnRefreshChart.UseVisualStyleBackColor = true;
            this.btnRefreshChart.Click += new System.EventHandler(this.BtnRefreshChartClick);
            // 
            // tbInfo
            // 
            this.tbInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbInfo.Location = new System.Drawing.Point(0, 0);
            this.tbInfo.Multiline = true;
            this.tbInfo.Name = "tbInfo";
            this.tbInfo.ReadOnly = true;
            this.tbInfo.Size = new System.Drawing.Size(243, 56);
            this.tbInfo.TabIndex = 0;
            // 
            // chart
            // 
            this.chart.BackgroundImageCoords = "5;5";
            this.chart.ClGradEnd = System.Drawing.Color.Snow;
            this.chart.ClGradStart = System.Drawing.Color.Silver;
            this.chart.CurrentTheme = FastChart.Chart.FastThemeName.Нет;
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.DrawLegendMargin = false;
            this.chart.DrawMargin = true;
            this.chart.DrawShadow = false;
            this.chart.GradAngle = 90F;
            this.chart.LegendBackground = System.Drawing.Color.White;
            this.chart.LegendInnerPadding = 10;
            this.chart.LegendMarginColor = System.Drawing.Color.Black;
            this.chart.LegendMarginWidth = 1F;
            this.chart.LegendPadding = 15;
            this.chart.LegendPlacement = FastChart.Chart.FastChart.ChartLegendPlacement.Вверху;
            this.chart.LegendShadow = System.Drawing.Color.DimGray;
            this.chart.LengthMinWidth = 100;
            this.chart.Location = new System.Drawing.Point(0, 56);
            this.chart.Name = "chart";
            this.chart.PenMarginColor = System.Drawing.Color.Black;
            this.chart.PenMarginWidth = 1F;
            this.chart.ShowLegend = false;
            this.chart.Size = new System.Drawing.Size(460, 293);
            this.chart.TabIndex = 1;
            this.chart.UseGradient = false;
            // 
            // CandleMenuWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 349);
            this.Controls.Add(this.chart);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CandleMenuWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CandleMenuWindow";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox tbInfo;
        private System.Windows.Forms.CheckBox cbShowHL;
        private System.Windows.Forms.Button btnRefreshChart;
        private FastChart.Chart.FastChart chart;
    }
}