namespace TradeSharp.Client.Forms
{
    partial class ProfitByTickerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfitByTickerForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.cbTicker = new System.Windows.Forms.ComboBox();
            this.chart = new FastMultiChart.FastMultiChart();
            this.standByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.cbTicker);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(629, 29);
            this.panelTop.TabIndex = 0;
            // 
            // cbTicker
            // 
            this.cbTicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTicker.FormattingEnabled = true;
            this.cbTicker.Location = new System.Drawing.Point(3, 3);
            this.cbTicker.Name = "cbTicker";
            this.cbTicker.Size = new System.Drawing.Size(121, 21);
            this.cbTicker.TabIndex = 0;
            this.cbTicker.SelectedIndexChanged += new System.EventHandler(this.cbTicker_SelectedIndexChanged);
            // 
            // chart
            // 
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.InnerMarginXLeft = 10;
            this.chart.Location = new System.Drawing.Point(0, 29);
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
            this.chart.Size = new System.Drawing.Size(629, 331);
            this.chart.TabIndex = 1;
            // 
            // standByControl
            // 
            this.standByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.standByControl.IsShown = true;
            this.standByControl.Location = new System.Drawing.Point(0, 29);
            this.standByControl.Name = "standByControl";
            this.standByControl.Size = new System.Drawing.Size(629, 331);
            this.standByControl.TabIndex = 2;
            this.standByControl.Tag = "TitleLoading";
            this.standByControl.Text = "Загрузка...";
            this.standByControl.TransparentForm = null;
            this.standByControl.Visible = false;
            // 
            // ProfitByTickerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 360);
            this.Controls.Add(this.standByControl);
            this.Controls.Add(this.chart);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProfitByTickerForm";
            this.Text = "Доходность по инструменту";
            this.Load += new System.EventHandler(this.ProfitByTickerForm_Load);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ComboBox cbTicker;
        private FastMultiChart.FastMultiChart chart;
        private UI.Util.Control.StandByControl standByControl;
    }
}