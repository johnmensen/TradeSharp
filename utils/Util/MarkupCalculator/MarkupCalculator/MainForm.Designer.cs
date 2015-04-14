namespace MarkupCalculator
{
    partial class FrmMarkupCalculator
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.MainTab = new System.Windows.Forms.TabControl();
            this.inputPage = new System.Windows.Forms.TabPage();
            this.namesDealFiles = new System.Windows.Forms.CheckedListBox();
            this.textDepoCurrency = new System.Windows.Forms.Label();
            this.DepoCurrency = new System.Windows.Forms.ComboBox();
            this.calcPage = new System.Windows.Forms.TabPage();
            this.panelEquityHost = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.EquityPage = new System.Windows.Forms.TabPage();
            this.panelEquityGraph = new System.Windows.Forms.Panel();
            this.EquityGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panelEquityControl = new System.Windows.Forms.Panel();
            this.btnEquityZoomReduce = new System.Windows.Forms.Button();
            this.btnEquityZoomAdd = new System.Windows.Forms.Button();
            this.LeveragePage = new System.Windows.Forms.TabPage();
            this.panelLeverageGraph = new System.Windows.Forms.Panel();
            this.LeverageGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panelLeverageControl = new System.Windows.Forms.Panel();
            this.btnLeverageZoomReduce = new System.Windows.Forms.Button();
            this.btnLeverageZoomAdd = new System.Windows.Forms.Button();
            this.panelGraphControl = new System.Windows.Forms.Panel();
            this.btnCalc = new System.Windows.Forms.Button();
            this.CalculationProgress = new System.Windows.Forms.ProgressBar();
            this.MainTab.SuspendLayout();
            this.inputPage.SuspendLayout();
            this.calcPage.SuspendLayout();
            this.panelEquityHost.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.EquityPage.SuspendLayout();
            this.panelEquityGraph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EquityGraph)).BeginInit();
            this.panelEquityControl.SuspendLayout();
            this.LeveragePage.SuspendLayout();
            this.panelLeverageGraph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeverageGraph)).BeginInit();
            this.panelLeverageControl.SuspendLayout();
            this.panelGraphControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTab
            // 
            this.MainTab.Controls.Add(this.inputPage);
            this.MainTab.Controls.Add(this.calcPage);
            this.MainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTab.Location = new System.Drawing.Point(0, 0);
            this.MainTab.Name = "MainTab";
            this.MainTab.SelectedIndex = 0;
            this.MainTab.Size = new System.Drawing.Size(717, 404);
            this.MainTab.TabIndex = 0;
            // 
            // inputPage
            // 
            this.inputPage.Controls.Add(this.namesDealFiles);
            this.inputPage.Controls.Add(this.textDepoCurrency);
            this.inputPage.Controls.Add(this.DepoCurrency);
            this.inputPage.Location = new System.Drawing.Point(4, 22);
            this.inputPage.Name = "inputPage";
            this.inputPage.Padding = new System.Windows.Forms.Padding(3);
            this.inputPage.Size = new System.Drawing.Size(709, 378);
            this.inputPage.TabIndex = 0;
            this.inputPage.Text = "Исходные данные";
            this.inputPage.UseVisualStyleBackColor = true;
            // 
            // namesDealFiles
            // 
            this.namesDealFiles.FormattingEnabled = true;
            this.namesDealFiles.Location = new System.Drawing.Point(15, 37);
            this.namesDealFiles.Name = "namesDealFiles";
            this.namesDealFiles.Size = new System.Drawing.Size(326, 304);
            this.namesDealFiles.TabIndex = 5;
            // 
            // textDepoCurrency
            // 
            this.textDepoCurrency.AutoSize = true;
            this.textDepoCurrency.Location = new System.Drawing.Point(12, 13);
            this.textDepoCurrency.Name = "textDepoCurrency";
            this.textDepoCurrency.Size = new System.Drawing.Size(95, 13);
            this.textDepoCurrency.TabIndex = 1;
            this.textDepoCurrency.Text = "Валюта депозита";
            // 
            // DepoCurrency
            // 
            this.DepoCurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DepoCurrency.FormattingEnabled = true;
            this.DepoCurrency.Items.AddRange(new object[] {
            "USD"});
            this.DepoCurrency.Location = new System.Drawing.Point(166, 10);
            this.DepoCurrency.Name = "DepoCurrency";
            this.DepoCurrency.Size = new System.Drawing.Size(175, 21);
            this.DepoCurrency.TabIndex = 0;
            // 
            // calcPage
            // 
            this.calcPage.Controls.Add(this.panelEquityHost);
            this.calcPage.Controls.Add(this.panelGraphControl);
            this.calcPage.Location = new System.Drawing.Point(4, 22);
            this.calcPage.Name = "calcPage";
            this.calcPage.Padding = new System.Windows.Forms.Padding(3);
            this.calcPage.Size = new System.Drawing.Size(709, 378);
            this.calcPage.TabIndex = 1;
            this.calcPage.Text = "Расчёт";
            this.calcPage.UseVisualStyleBackColor = true;
            // 
            // panelEquityHost
            // 
            this.panelEquityHost.Controls.Add(this.tabControl1);
            this.panelEquityHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEquityHost.Location = new System.Drawing.Point(3, 35);
            this.panelEquityHost.Name = "panelEquityHost";
            this.panelEquityHost.Size = new System.Drawing.Size(703, 340);
            this.panelEquityHost.TabIndex = 5;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.EquityPage);
            this.tabControl1.Controls.Add(this.LeveragePage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(703, 340);
            this.tabControl1.TabIndex = 3;
            // 
            // EquityPage
            // 
            this.EquityPage.Controls.Add(this.panelEquityGraph);
            this.EquityPage.Controls.Add(this.panelEquityControl);
            this.EquityPage.Location = new System.Drawing.Point(4, 22);
            this.EquityPage.Name = "EquityPage";
            this.EquityPage.Padding = new System.Windows.Forms.Padding(3);
            this.EquityPage.Size = new System.Drawing.Size(695, 314);
            this.EquityPage.TabIndex = 0;
            this.EquityPage.Text = "Equity (капитал)";
            this.EquityPage.UseVisualStyleBackColor = true;
            // 
            // panelEquityGraph
            // 
            this.panelEquityGraph.Controls.Add(this.EquityGraph);
            this.panelEquityGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEquityGraph.Location = new System.Drawing.Point(3, 3);
            this.panelEquityGraph.Name = "panelEquityGraph";
            this.panelEquityGraph.Size = new System.Drawing.Size(689, 268);
            this.panelEquityGraph.TabIndex = 3;
            // 
            // EquityGraph
            // 
            chartArea1.Name = "ChartArea1";
            this.EquityGraph.ChartAreas.Add(chartArea1);
            this.EquityGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.EquityGraph.Legends.Add(legend1);
            this.EquityGraph.Location = new System.Drawing.Point(0, 0);
            this.EquityGraph.Name = "EquityGraph";
            this.EquityGraph.Size = new System.Drawing.Size(689, 268);
            this.EquityGraph.TabIndex = 2;
            // 
            // panelEquityControl
            // 
            this.panelEquityControl.Controls.Add(this.btnEquityZoomReduce);
            this.panelEquityControl.Controls.Add(this.btnEquityZoomAdd);
            this.panelEquityControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEquityControl.Location = new System.Drawing.Point(3, 271);
            this.panelEquityControl.Name = "panelEquityControl";
            this.panelEquityControl.Size = new System.Drawing.Size(689, 40);
            this.panelEquityControl.TabIndex = 2;
            // 
            // btnEquityZoomReduce
            // 
            this.btnEquityZoomReduce.Location = new System.Drawing.Point(35, 6);
            this.btnEquityZoomReduce.Name = "btnEquityZoomReduce";
            this.btnEquityZoomReduce.Size = new System.Drawing.Size(26, 23);
            this.btnEquityZoomReduce.TabIndex = 5;
            this.btnEquityZoomReduce.Text = "-";
            this.btnEquityZoomReduce.UseVisualStyleBackColor = true;
            this.btnEquityZoomReduce.Click += new System.EventHandler(this.BtnEquityZoomReduceClick);
            // 
            // btnEquityZoomAdd
            // 
            this.btnEquityZoomAdd.Location = new System.Drawing.Point(3, 6);
            this.btnEquityZoomAdd.Name = "btnEquityZoomAdd";
            this.btnEquityZoomAdd.Size = new System.Drawing.Size(26, 23);
            this.btnEquityZoomAdd.TabIndex = 4;
            this.btnEquityZoomAdd.Text = "+";
            this.btnEquityZoomAdd.UseVisualStyleBackColor = true;
            this.btnEquityZoomAdd.Click += new System.EventHandler(this.BtnEquityZoomAddClick);
            // 
            // LeveragePage
            // 
            this.LeveragePage.Controls.Add(this.panelLeverageGraph);
            this.LeveragePage.Controls.Add(this.panelLeverageControl);
            this.LeveragePage.Location = new System.Drawing.Point(4, 22);
            this.LeveragePage.Name = "LeveragePage";
            this.LeveragePage.Padding = new System.Windows.Forms.Padding(3);
            this.LeveragePage.Size = new System.Drawing.Size(695, 314);
            this.LeveragePage.TabIndex = 1;
            this.LeveragePage.Text = "Leverage (плечо)";
            this.LeveragePage.UseVisualStyleBackColor = true;
            // 
            // panelLeverageGraph
            // 
            this.panelLeverageGraph.Controls.Add(this.LeverageGraph);
            this.panelLeverageGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeverageGraph.Location = new System.Drawing.Point(3, 3);
            this.panelLeverageGraph.Name = "panelLeverageGraph";
            this.panelLeverageGraph.Size = new System.Drawing.Size(689, 268);
            this.panelLeverageGraph.TabIndex = 2;
            // 
            // LeverageGraph
            // 
            chartArea2.Name = "ChartArea1";
            this.LeverageGraph.ChartAreas.Add(chartArea2);
            this.LeverageGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.LeverageGraph.Legends.Add(legend2);
            this.LeverageGraph.Location = new System.Drawing.Point(0, 0);
            this.LeverageGraph.Name = "LeverageGraph";
            this.LeverageGraph.Size = new System.Drawing.Size(689, 268);
            this.LeverageGraph.TabIndex = 2;
            this.LeverageGraph.Text = "chart1";
            // 
            // panelLeverageControl
            // 
            this.panelLeverageControl.Controls.Add(this.btnLeverageZoomReduce);
            this.panelLeverageControl.Controls.Add(this.btnLeverageZoomAdd);
            this.panelLeverageControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLeverageControl.Location = new System.Drawing.Point(3, 271);
            this.panelLeverageControl.Name = "panelLeverageControl";
            this.panelLeverageControl.Size = new System.Drawing.Size(689, 40);
            this.panelLeverageControl.TabIndex = 1;
            // 
            // btnLeverageZoomReduce
            // 
            this.btnLeverageZoomReduce.Location = new System.Drawing.Point(40, 7);
            this.btnLeverageZoomReduce.Name = "btnLeverageZoomReduce";
            this.btnLeverageZoomReduce.Size = new System.Drawing.Size(35, 23);
            this.btnLeverageZoomReduce.TabIndex = 1;
            this.btnLeverageZoomReduce.Text = "-";
            this.btnLeverageZoomReduce.UseVisualStyleBackColor = true;
            this.btnLeverageZoomReduce.Click += new System.EventHandler(this.BtnLeverageZoomReduceClick);
            // 
            // btnLeverageZoomAdd
            // 
            this.btnLeverageZoomAdd.Location = new System.Drawing.Point(4, 7);
            this.btnLeverageZoomAdd.Name = "btnLeverageZoomAdd";
            this.btnLeverageZoomAdd.Size = new System.Drawing.Size(30, 23);
            this.btnLeverageZoomAdd.TabIndex = 0;
            this.btnLeverageZoomAdd.Text = "+";
            this.btnLeverageZoomAdd.UseVisualStyleBackColor = true;
            this.btnLeverageZoomAdd.Click += new System.EventHandler(this.BtnLeverageZoomAddClick);
            // 
            // panelGraphControl
            // 
            this.panelGraphControl.Controls.Add(this.btnCalc);
            this.panelGraphControl.Controls.Add(this.CalculationProgress);
            this.panelGraphControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelGraphControl.Location = new System.Drawing.Point(3, 3);
            this.panelGraphControl.Name = "panelGraphControl";
            this.panelGraphControl.Size = new System.Drawing.Size(703, 32);
            this.panelGraphControl.TabIndex = 4;
            // 
            // btnCalc
            // 
            this.btnCalc.Location = new System.Drawing.Point(3, 4);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(104, 23);
            this.btnCalc.TabIndex = 0;
            this.btnCalc.Text = "рассчитать";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.BtnCalcClick);
            // 
            // CalculationProgress
            // 
            this.CalculationProgress.Location = new System.Drawing.Point(113, 3);
            this.CalculationProgress.Name = "CalculationProgress";
            this.CalculationProgress.Size = new System.Drawing.Size(315, 23);
            this.CalculationProgress.TabIndex = 2;
            // 
            // FrmMarkupCalculator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 404);
            this.Controls.Add(this.MainTab);
            this.Name = "FrmMarkupCalculator";
            this.Text = "Markup calculator";
            this.MainTab.ResumeLayout(false);
            this.inputPage.ResumeLayout(false);
            this.inputPage.PerformLayout();
            this.calcPage.ResumeLayout(false);
            this.panelEquityHost.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.EquityPage.ResumeLayout(false);
            this.panelEquityGraph.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EquityGraph)).EndInit();
            this.panelEquityControl.ResumeLayout(false);
            this.LeveragePage.ResumeLayout(false);
            this.panelLeverageGraph.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LeverageGraph)).EndInit();
            this.panelLeverageControl.ResumeLayout(false);
            this.panelGraphControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTab;
        private System.Windows.Forms.TabPage inputPage;
        private System.Windows.Forms.TabPage calcPage;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.ProgressBar CalculationProgress;
        private System.Windows.Forms.Label textDepoCurrency;
        private System.Windows.Forms.ComboBox DepoCurrency;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage EquityPage;
        private System.Windows.Forms.TabPage LeveragePage;
        private System.Windows.Forms.Panel panelGraphControl;
        private System.Windows.Forms.Panel panelEquityHost;
        private System.Windows.Forms.Panel panelEquityGraph;
        private System.Windows.Forms.DataVisualization.Charting.Chart EquityGraph;
        private System.Windows.Forms.Panel panelEquityControl;
        private System.Windows.Forms.Button btnEquityZoomAdd;
        private System.Windows.Forms.Panel panelLeverageGraph;
        private System.Windows.Forms.DataVisualization.Charting.Chart LeverageGraph;
        private System.Windows.Forms.Panel panelLeverageControl;
        private System.Windows.Forms.Button btnEquityZoomReduce;
        private System.Windows.Forms.Button btnLeverageZoomReduce;
        private System.Windows.Forms.Button btnLeverageZoomAdd;
        private System.Windows.Forms.CheckedListBox namesDealFiles;
    }
}

