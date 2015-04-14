namespace NewsAnalysisScript
{
    partial class ResultsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResultsForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.newsFastGrid = new FastGrid.FastGrid();
            this.indexFastChart = new FastChart.Chart.FastChart();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.saveNewsButton = new System.Windows.Forms.Button();
            this.toXmlButton = new System.Windows.Forms.Button();
            this.imageListGrid = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(7, 7);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.newsFastGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.indexFastChart);
            this.splitContainer1.Size = new System.Drawing.Size(769, 475);
            this.splitContainer1.SplitterDistance = 233;
            this.splitContainer1.TabIndex = 3;
            // 
            // newsFastGrid
            // 
            this.newsFastGrid.CaptionHeight = 20;
            this.newsFastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.newsFastGrid.CellHeight = 18;
            this.newsFastGrid.CellPadding = 5;
            this.newsFastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.newsFastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.newsFastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.newsFastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.newsFastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.newsFastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.newsFastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.newsFastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.newsFastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.newsFastGrid.FontAnchoredRow = null;
            this.newsFastGrid.FontCell = null;
            this.newsFastGrid.FontHeader = null;
            this.newsFastGrid.FontSelectedCell = null;
            this.newsFastGrid.Location = new System.Drawing.Point(0, 0);
            this.newsFastGrid.MinimumTableWidth = null;
            this.newsFastGrid.MultiSelectEnabled = false;
            this.newsFastGrid.Name = "newsFastGrid";
            this.newsFastGrid.SelectEnabled = true;
            this.newsFastGrid.Size = new System.Drawing.Size(769, 233);
            this.newsFastGrid.StickFirst = false;
            this.newsFastGrid.StickLast = false;
            this.newsFastGrid.TabIndex = 0;
            this.newsFastGrid.UserHitCell += new FastGrid.UserHitCellDel(this.NewsFastGridUserHitCell);
            // 
            // indexFastChart
            // 
            this.indexFastChart.BackgroundImageCoords = "5;5";
            this.indexFastChart.ClGradEnd = System.Drawing.Color.Snow;
            this.indexFastChart.ClGradStart = System.Drawing.Color.Silver;
            this.indexFastChart.CurrentTheme = FastChart.Chart.FastThemeName.Нет;
            this.indexFastChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indexFastChart.DrawLegendMargin = false;
            this.indexFastChart.DrawMargin = true;
            this.indexFastChart.DrawShadow = false;
            this.indexFastChart.GradAngle = 90F;
            this.indexFastChart.LegendBackground = System.Drawing.Color.White;
            this.indexFastChart.LegendInnerPadding = 10;
            this.indexFastChart.LegendMarginColor = System.Drawing.Color.Black;
            this.indexFastChart.LegendMarginWidth = 1F;
            this.indexFastChart.LegendPadding = 15;
            this.indexFastChart.LegendPlacement = FastChart.Chart.FastChart.ChartLegendPlacement.Вверху;
            this.indexFastChart.LegendShadow = System.Drawing.Color.DimGray;
            this.indexFastChart.LengthMinWidth = 100;
            this.indexFastChart.Location = new System.Drawing.Point(0, 0);
            this.indexFastChart.Name = "indexFastChart";
            this.indexFastChart.PenMarginColor = System.Drawing.Color.Black;
            this.indexFastChart.PenMarginWidth = 1F;
            this.indexFastChart.ShowLegend = false;
            this.indexFastChart.Size = new System.Drawing.Size(769, 238);
            this.indexFastChart.TabIndex = 0;
            this.indexFastChart.UseGradient = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.flowLayoutPanel2);
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(7, 453);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(769, 29);
            this.panel1.TabIndex = 4;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.okButton);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(688, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(81, 29);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(3, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "Закрыть";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.saveNewsButton);
            this.flowLayoutPanel1.Controls.Add(this.toXmlButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(342, 29);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // saveNewsButton
            // 
            this.saveNewsButton.AutoSize = true;
            this.saveNewsButton.Location = new System.Drawing.Point(3, 3);
            this.saveNewsButton.Name = "saveNewsButton";
            this.saveNewsButton.Size = new System.Drawing.Size(114, 23);
            this.saveNewsButton.TabIndex = 1;
            this.saveNewsButton.Text = "Сохранить новости";
            this.saveNewsButton.UseVisualStyleBackColor = true;
            this.saveNewsButton.Click += new System.EventHandler(this.SaveNewsButtonClick);
            // 
            // toXmlButton
            // 
            this.toXmlButton.AutoSize = true;
            this.toXmlButton.Location = new System.Drawing.Point(123, 3);
            this.toXmlButton.Name = "toXmlButton";
            this.toXmlButton.Size = new System.Drawing.Size(132, 23);
            this.toXmlButton.TabIndex = 0;
            this.toXmlButton.Text = "Сохранить результаты";
            this.toXmlButton.UseVisualStyleBackColor = true;
            this.toXmlButton.Click += new System.EventHandler(this.ToXmlButtonClick);
            // 
            // imageListGrid
            // 
            this.imageListGrid.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGrid.ImageStream")));
            this.imageListGrid.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGrid.Images.SetKeyName(0, "True");
            // 
            // ResultsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 489);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ResultsForm";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Text = "Результаты анализа новостей";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private FastGrid.FastGrid newsFastGrid;
        private FastChart.Chart.FastChart indexFastChart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button toXmlButton;
        private System.Windows.Forms.Button saveNewsButton;
        private System.Windows.Forms.ImageList imageListGrid;
    }
}