namespace TradeSharp.Client.Forms
{
    partial class AccountShareHistoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountShareHistoryForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageChart = new System.Windows.Forms.TabPage();
            this.chartShareTrack = new FastMultiChart.FastMultiChart();
            this.tabControl.SuspendLayout();
            this.tabPageChart.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageChart);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(538, 380);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageChart
            // 
            this.tabPageChart.Controls.Add(this.chartShareTrack);
            this.tabPageChart.Location = new System.Drawing.Point(4, 22);
            this.tabPageChart.Name = "tabPageChart";
            this.tabPageChart.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChart.Size = new System.Drawing.Size(530, 354);
            this.tabPageChart.TabIndex = 0;
            this.tabPageChart.Text = "Стоимость пая";
            this.tabPageChart.UseVisualStyleBackColor = true;
            // 
            // chartShareTrack
            // 
            this.chartShareTrack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartShareTrack.InnerMarginXLeft = 10;
            this.chartShareTrack.Location = new System.Drawing.Point(3, 3);
            this.chartShareTrack.MarginXLeft = 10;
            this.chartShareTrack.MarginXRight = 10;
            this.chartShareTrack.MarginYBottom = 10;
            this.chartShareTrack.MarginYTop = 10;
            this.chartShareTrack.Name = "chartShareTrack";
            this.chartShareTrack.RenderPolygons = true;
            this.chartShareTrack.ScaleDivisionXMaxPixel = -1;
            this.chartShareTrack.ScaleDivisionXMinPixel = 70;
            this.chartShareTrack.ScaleDivisionYMaxPixel = 60;
            this.chartShareTrack.ScaleDivisionYMinPixel = 20;
            this.chartShareTrack.ScrollBarHeight = 80;
            this.chartShareTrack.ShowHints = true;
            this.chartShareTrack.ShowScaleDivisionXLabel = true;
            this.chartShareTrack.ShowScaleDivisionYLabel = true;
            this.chartShareTrack.Size = new System.Drawing.Size(524, 348);
            this.chartShareTrack.TabIndex = 0;
            // 
            // AccountShareHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 380);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AccountShareHistoryForm";
            this.Text = "Стоимость пая";
            this.tabControl.ResumeLayout(false);
            this.tabPageChart.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageChart;
        private FastMultiChart.FastMultiChart chartShareTrack;
    }
}