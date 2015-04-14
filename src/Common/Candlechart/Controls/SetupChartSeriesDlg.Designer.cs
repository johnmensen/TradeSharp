namespace Candlechart.Controls
{
    partial class SetupChartSeriesDlg
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbChart = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cbSeriesSrc = new System.Windows.Forms.ComboBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.cbSeriesDest = new System.Windows.Forms.ComboBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPeriodExtremum = new System.Windows.Forms.TextBox();
            this.cbDiverType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbMaxPastExtremum = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbIntervalMargins = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbInverse = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbChart);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(392, 21);
            this.panel1.TabIndex = 0;
            // 
            // cbChart
            // 
            this.cbChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbChart.FormattingEnabled = true;
            this.cbChart.Location = new System.Drawing.Point(0, 0);
            this.cbChart.Name = "cbChart";
            this.cbChart.Size = new System.Drawing.Size(234, 21);
            this.cbChart.TabIndex = 1;
            this.cbChart.SelectedIndexChanged += new System.EventHandler(this.CbChartSelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(234, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(158, 21);
            this.panel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "TitleChartSmall";
            this.label1.Text = "график";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.cbSeriesSrc);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 21);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(392, 21);
            this.panel3.TabIndex = 1;
            // 
            // cbSeriesSrc
            // 
            this.cbSeriesSrc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSeriesSrc.FormattingEnabled = true;
            this.cbSeriesSrc.Location = new System.Drawing.Point(0, 0);
            this.cbSeriesSrc.Name = "cbSeriesSrc";
            this.cbSeriesSrc.Size = new System.Drawing.Size(234, 21);
            this.cbSeriesSrc.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label2);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel4.Location = new System.Drawing.Point(234, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(158, 21);
            this.panel4.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 13);
            this.label2.TabIndex = 0;
            this.label2.Tag = "TitleSourceSeriesOrIndexSmall";
            this.label2.Text = "серия-источник (\"индекс\")";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.cbSeriesDest);
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 42);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(392, 21);
            this.panel5.TabIndex = 2;
            // 
            // cbSeriesDest
            // 
            this.cbSeriesDest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSeriesDest.FormattingEnabled = true;
            this.cbSeriesDest.Location = new System.Drawing.Point(0, 0);
            this.cbSeriesDest.Name = "cbSeriesDest";
            this.cbSeriesDest.Size = new System.Drawing.Size(234, 21);
            this.cbSeriesDest.TabIndex = 1;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label3);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel6.Location = new System.Drawing.Point(234, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(158, 21);
            this.panel6.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 13);
            this.label3.TabIndex = 0;
            this.label3.Tag = "TitleDisplaySeriesOrCourseSmall";
            this.label3.Text = "серия отображения (\"курс\")";
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnOk);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 153);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(392, 30);
            this.panelBottom.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(132, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(3, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Tag = "TitleOK";
            this.btnOk.Text = "ОК";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(65, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 4;
            this.label4.Tag = "TitleExtremumPeriodSmall";
            this.label4.Text = "период экстремума";
            // 
            // tbPeriodExtremum
            // 
            this.tbPeriodExtremum.Location = new System.Drawing.Point(3, 96);
            this.tbPeriodExtremum.Name = "tbPeriodExtremum";
            this.tbPeriodExtremum.Size = new System.Drawing.Size(56, 20);
            this.tbPeriodExtremum.TabIndex = 5;
            this.tbPeriodExtremum.Text = "6";
            // 
            // cbDiverType
            // 
            this.cbDiverType.FormattingEnabled = true;
            this.cbDiverType.Location = new System.Drawing.Point(3, 69);
            this.cbDiverType.Name = "cbDiverType";
            this.cbDiverType.Size = new System.Drawing.Size(121, 21);
            this.cbDiverType.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(130, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 7;
            this.label5.Tag = "TitleDivergenceTypeSmall";
            this.label5.Text = "диверг.";
            // 
            // tbMaxPastExtremum
            // 
            this.tbMaxPastExtremum.Location = new System.Drawing.Point(198, 96);
            this.tbMaxPastExtremum.Name = "tbMaxPastExtremum";
            this.tbMaxPastExtremum.Size = new System.Drawing.Size(56, 20);
            this.tbMaxPastExtremum.TabIndex = 9;
            this.tbMaxPastExtremum.Text = "18";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(260, 99);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 13);
            this.label6.TabIndex = 8;
            this.label6.Tag = "TitleMaximumDistanceFromExtremumShortSmall";
            this.label6.Text = "макс. расст. от экстр.";
            // 
            // tbIntervalMargins
            // 
            this.tbIntervalMargins.Location = new System.Drawing.Point(3, 122);
            this.tbIntervalMargins.Name = "tbIntervalMargins";
            this.tbIntervalMargins.Size = new System.Drawing.Size(56, 20);
            this.tbIntervalMargins.TabIndex = 10;
            this.tbIntervalMargins.Text = "-1.0 1.0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(65, 125);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 13);
            this.label7.TabIndex = 11;
            this.label7.Tag = "TitleIntervalBordersSmall";
            this.label7.Text = "границы интервала";
            // 
            // cbInverse
            // 
            this.cbInverse.AutoSize = true;
            this.cbInverse.Location = new System.Drawing.Point(198, 124);
            this.cbInverse.Name = "cbInverse";
            this.cbInverse.Size = new System.Drawing.Size(105, 17);
            this.cbInverse.TabIndex = 12;
            this.cbInverse.Tag = "TitleInvertSmall";
            this.cbInverse.Text = "инвертировать!";
            this.cbInverse.UseVisualStyleBackColor = true;
            // 
            // SetupChartSeriesDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 183);
            this.Controls.Add(this.cbInverse);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbIntervalMargins);
            this.Controls.Add(this.tbMaxPastExtremum);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbDiverType);
            this.Controls.Add(this.tbPeriodExtremum);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(400, 210);
            this.Name = "SetupChartSeriesDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleSeriesSetup";
            this.Text = "Настройка серий";
            this.Load += new System.EventHandler(this.SetupChartSeriesDlgLoad);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cbChart;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ComboBox cbSeriesSrc;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ComboBox cbSeriesDest;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPeriodExtremum;
        private System.Windows.Forms.ComboBox cbDiverType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbMaxPastExtremum;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbIntervalMargins;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cbInverse;
    }
}