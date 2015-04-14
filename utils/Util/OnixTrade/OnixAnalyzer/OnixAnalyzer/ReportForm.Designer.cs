namespace OnixAnalyzer
{
    partial class ReportForm
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
            this.btnCalculate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbDealsMin = new System.Windows.Forms.TextBox();
            this.tbHistoryPercent = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(12, 64);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(75, 23);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "Считать";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "минимум сделок";
            // 
            // tbDealsMin
            // 
            this.tbDealsMin.Location = new System.Drawing.Point(12, 12);
            this.tbDealsMin.Name = "tbDealsMin";
            this.tbDealsMin.Size = new System.Drawing.Size(100, 20);
            this.tbDealsMin.TabIndex = 2;
            this.tbDealsMin.Text = "200";
            // 
            // tbHistoryPercent
            // 
            this.tbHistoryPercent.Location = new System.Drawing.Point(12, 38);
            this.tbHistoryPercent.Name = "tbHistoryPercent";
            this.tbHistoryPercent.Size = new System.Drawing.Size(100, 20);
            this.tbHistoryPercent.TabIndex = 4;
            this.tbHistoryPercent.Text = "80";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "процент на \"историю\"";
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(12, 126);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.Size = new System.Drawing.Size(497, 272);
            this.tbResults.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Краткий отчет";
            // 
            // ReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 410);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.tbHistoryPercent);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbDealsMin);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCalculate);
            this.Name = "ReportForm";
            this.Text = "Отчет";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDealsMin;
        private System.Windows.Forms.TextBox tbHistoryPercent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbResults;
        private System.Windows.Forms.Label label3;
    }
}