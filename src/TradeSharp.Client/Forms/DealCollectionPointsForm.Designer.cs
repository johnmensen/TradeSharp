namespace TradeSharp.Client.Forms
{
    partial class DealCollectionPointsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DealCollectionPointsForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblIntervals = new System.Windows.Forms.Label();
            this.tbIntervalsCount = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbIntervalLength = new System.Windows.Forms.ComboBox();
            this.btnMoreOptions = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.cbDealType = new System.Windows.Forms.ComboBox();
            this.chart = new FastMultiChart.FastMultiChart();
            this.standByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.lblIntervals);
            this.panelTop.Controls.Add(this.tbIntervalsCount);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.cbIntervalLength);
            this.panelTop.Controls.Add(this.btnMoreOptions);
            this.panelTop.Controls.Add(this.btnApply);
            this.panelTop.Controls.Add(this.cbDealType);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(607, 30);
            this.panelTop.TabIndex = 0;
            // 
            // lblIntervals
            // 
            this.lblIntervals.AutoSize = true;
            this.lblIntervals.Location = new System.Drawing.Point(262, 33);
            this.lblIntervals.Name = "lblIntervals";
            this.lblIntervals.Size = new System.Drawing.Size(66, 13);
            this.lblIntervals.TabIndex = 6;
            this.lblIntervals.Text = "интервалов";
            // 
            // tbIntervalsCount
            // 
            this.tbIntervalsCount.Location = new System.Drawing.Point(209, 31);
            this.tbIntervalsCount.Name = "tbIntervalsCount";
            this.tbIntervalsCount.Size = new System.Drawing.Size(47, 20);
            this.tbIntervalsCount.TabIndex = 5;
            this.tbIntervalsCount.Text = "200";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(111, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "интервал, минут";
            // 
            // cbIntervalLength
            // 
            this.cbIntervalLength.FormattingEnabled = true;
            this.cbIntervalLength.Items.AddRange(new object[] {
            "1",
            "5",
            "15",
            "30",
            "60",
            "60*4",
            "60*24",
            "60*24*5"});
            this.cbIntervalLength.Location = new System.Drawing.Point(3, 30);
            this.cbIntervalLength.Name = "cbIntervalLength";
            this.cbIntervalLength.Size = new System.Drawing.Size(102, 21);
            this.cbIntervalLength.TabIndex = 3;
            this.cbIntervalLength.Text = "60";
            // 
            // btnMoreOptions
            // 
            this.btnMoreOptions.Location = new System.Drawing.Point(126, 3);
            this.btnMoreOptions.Name = "btnMoreOptions";
            this.btnMoreOptions.Size = new System.Drawing.Size(75, 23);
            this.btnMoreOptions.TabIndex = 2;
            this.btnMoreOptions.Text = "Еще...";
            this.btnMoreOptions.UseVisualStyleBackColor = true;
            this.btnMoreOptions.Click += new System.EventHandler(this.BtnMoreOptionsClick);
            // 
            // btnApply
            // 
            this.btnApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnApply.Location = new System.Drawing.Point(3, 3);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(102, 23);
            this.btnApply.TabIndex = 1;
            this.btnApply.Text = "Построить";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.BtnApplyClick);
            // 
            // cbDealType
            // 
            this.cbDealType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDealType.FormattingEnabled = true;
            this.cbDealType.Location = new System.Drawing.Point(480, 5);
            this.cbDealType.Name = "cbDealType";
            this.cbDealType.Size = new System.Drawing.Size(23, 21);
            this.cbDealType.TabIndex = 0;
            this.cbDealType.Visible = false;
            // 
            // chart
            // 
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.InnerMarginXLeft = 10;
            this.chart.Location = new System.Drawing.Point(0, 30);
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
            this.chart.Size = new System.Drawing.Size(607, 415);
            this.chart.TabIndex = 2;
            // 
            // standByControl
            // 
            this.standByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.standByControl.IsShown = true;
            this.standByControl.Location = new System.Drawing.Point(0, 30);
            this.standByControl.Name = "standByControl";
            this.standByControl.Size = new System.Drawing.Size(607, 415);
            this.standByControl.TabIndex = 4;
            this.standByControl.Tag = "TitleLoading";
            this.standByControl.Text = "Загрузка...";
            this.standByControl.TransparentForm = null;
            // 
            // DealCollectionPointsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 445);
            this.Controls.Add(this.standByControl);
            this.Controls.Add(this.chart);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DealCollectionPointsForm";
            this.Text = "Результаты сделок";
            this.Load += new System.EventHandler(this.DealCollectionPointsFormLoad);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.ComboBox cbDealType;
        private FastMultiChart.FastMultiChart chart;
        private System.Windows.Forms.Button btnMoreOptions;
        private System.Windows.Forms.Label lblIntervals;
        private System.Windows.Forms.TextBox tbIntervalsCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbIntervalLength;
        private UI.Util.Control.StandByControl standByControl;
    }
}