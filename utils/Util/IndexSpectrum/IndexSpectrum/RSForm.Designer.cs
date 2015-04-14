namespace IndexSpectrum
{
    partial class RSForm
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
            DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.PointSeriesLabel pointSeriesLabel1 = new DevExpress.XtraCharts.PointSeriesLabel();
            DevExpress.XtraCharts.PointSeriesView pointSeriesView1 = new DevExpress.XtraCharts.PointSeriesView();
            DevExpress.XtraCharts.Series series2 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.PointSeriesLabel pointSeriesLabel2 = new DevExpress.XtraCharts.PointSeriesLabel();
            DevExpress.XtraCharts.LineSeriesView lineSeriesView1 = new DevExpress.XtraCharts.LineSeriesView();
            DevExpress.XtraCharts.PointSeriesView pointSeriesView2 = new DevExpress.XtraCharts.PointSeriesView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.tbTimeframe = new System.Windows.Forms.TextBox();
            this.btnCalcHurst = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.chartHurst = new DevExpress.XtraCharts.ChartControl();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartHurst)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesView2)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.tbTimeframe);
            this.panelTop.Controls.Add(this.btnCalcHurst);
            this.panelTop.Controls.Add(this.btnBrowse);
            this.panelTop.Controls.Add(this.tbFileName);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(522, 29);
            this.panelTop.TabIndex = 0;
            // 
            // tbTimeframe
            // 
            this.tbTimeframe.Location = new System.Drawing.Point(3, 3);
            this.tbTimeframe.Name = "tbTimeframe";
            this.tbTimeframe.Size = new System.Drawing.Size(61, 20);
            this.tbTimeframe.TabIndex = 3;
            this.tbTimeframe.Text = "240";
            // 
            // btnCalcHurst
            // 
            this.btnCalcHurst.Location = new System.Drawing.Point(437, 2);
            this.btnCalcHurst.Name = "btnCalcHurst";
            this.btnCalcHurst.Size = new System.Drawing.Size(75, 23);
            this.btnCalcHurst.TabIndex = 2;
            this.btnCalcHurst.Text = "считать";
            this.btnCalcHurst.UseVisualStyleBackColor = true;
            this.btnCalcHurst.Click += new System.EventHandler(this.btnCalcHurst_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(397, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(25, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(86, 3);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(308, 20);
            this.tbFileName.TabIndex = 0;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "txt";
            this.openFileDialog.Filter = "Quotes|*.quote|*.*|*.*";
            this.openFileDialog.Title = "Котировки";
            // 
            // chartHurst
            // 
            xyDiagram1.AxisX.Range.SideMarginsEnabled = true;
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.Range.AlwaysShowZeroLevel = false;
            xyDiagram1.AxisY.Range.SideMarginsEnabled = true;
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            this.chartHurst.Diagram = xyDiagram1;
            this.chartHurst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartHurst.Legend.Visible = false;
            this.chartHurst.Location = new System.Drawing.Point(0, 29);
            this.chartHurst.Name = "chartHurst";
            series1.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            pointSeriesLabel1.OverlappingOptionsTypeName = "PointOverlappingOptions";
            pointSeriesLabel1.Visible = false;
            series1.Label = pointSeriesLabel1;
            series1.Name = "seriesRS";
            pointSeriesView1.PointMarkerOptions.Size = 4;
            series1.View = pointSeriesView1;
            series2.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            pointSeriesLabel2.OverlappingOptionsTypeName = "PointOverlappingOptions";
            pointSeriesLabel2.Visible = false;
            series2.Label = pointSeriesLabel2;
            series2.Name = "seriesReg";
            lineSeriesView1.LineMarkerOptions.Size = 8;
            lineSeriesView1.LineMarkerOptions.Visible = false;
            series2.View = lineSeriesView1;
            this.chartHurst.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
            pointSeriesView2.PointMarkerOptions.Size = 10;
            this.chartHurst.SeriesTemplate.View = pointSeriesView2;
            this.chartHurst.Size = new System.Drawing.Size(522, 426);
            this.chartHurst.TabIndex = 1;
            // 
            // RSForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 455);
            this.Controls.Add(this.chartHurst);
            this.Controls.Add(this.panelTop);
            this.Name = "RSForm";
            this.Text = "RS-анализ";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesLabel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(lineSeriesView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pointSeriesView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartHurst)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private DevExpress.XtraCharts.ChartControl chartHurst;
        private System.Windows.Forms.Button btnCalcHurst;
        private System.Windows.Forms.TextBox tbTimeframe;
    }
}