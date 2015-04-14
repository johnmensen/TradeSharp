namespace TradeSharp.Client.Forms
{
    partial class PortfolioRiskCalcSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortfolioRiskCalcSettingsForm));
            this.tbTimeframe = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbCorrTimeframeCount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTestTimeframeCount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTestsCount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbPercentiles = new System.Windows.Forms.TextBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbUploadQuotes = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tbTimeframe
            // 
            this.tbTimeframe.Location = new System.Drawing.Point(12, 12);
            this.tbTimeframe.Name = "tbTimeframe";
            this.tbTimeframe.Size = new System.Drawing.Size(100, 20);
            this.tbTimeframe.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "таймфрейм, минут";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(183, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "оценка корреляций, таймфреймов";
            // 
            // tbCorrTimeframeCount
            // 
            this.tbCorrTimeframeCount.Location = new System.Drawing.Point(12, 38);
            this.tbCorrTimeframeCount.Name = "tbCorrTimeframeCount";
            this.tbCorrTimeframeCount.Size = new System.Drawing.Size(100, 20);
            this.tbCorrTimeframeCount.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "таймфреймов в тесте";
            // 
            // tbTestTimeframeCount
            // 
            this.tbTestTimeframeCount.Location = new System.Drawing.Point(12, 64);
            this.tbTestTimeframeCount.Name = "tbTestTimeframeCount";
            this.tbTestTimeframeCount.Size = new System.Drawing.Size(100, 20);
            this.tbTestTimeframeCount.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(118, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "количество тестов";
            // 
            // tbTestsCount
            // 
            this.tbTestsCount.Location = new System.Drawing.Point(12, 90);
            this.tbTestsCount.Name = "tbTestsCount";
            this.tbTestsCount.Size = new System.Drawing.Size(100, 20);
            this.tbTestsCount.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(118, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "перцентили";
            // 
            // tbPercentiles
            // 
            this.tbPercentiles.Location = new System.Drawing.Point(12, 116);
            this.tbPercentiles.Name = "tbPercentiles";
            this.tbPercentiles.Size = new System.Drawing.Size(100, 20);
            this.tbPercentiles.TabIndex = 8;
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(12, 188);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 10;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(161, 188);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cbUploadQuotes
            // 
            this.cbUploadQuotes.AutoSize = true;
            this.cbUploadQuotes.Checked = true;
            this.cbUploadQuotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUploadQuotes.Location = new System.Drawing.Point(12, 142);
            this.cbUploadQuotes.Name = "cbUploadQuotes";
            this.cbUploadQuotes.Size = new System.Drawing.Size(149, 17);
            this.cbUploadQuotes.TabIndex = 12;
            this.cbUploadQuotes.Text = "запрашивать котировки";
            this.cbUploadQuotes.UseVisualStyleBackColor = true;
            // 
            // PortfolioRiskCalcSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 215);
            this.Controls.Add(this.cbUploadQuotes);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbPercentiles);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbTestsCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbTestTimeframeCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbCorrTimeframeCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbTimeframe);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PortfolioRiskCalcSettingsForm";
            this.Text = "Настройки теста - оценки риска";
            this.Load += new System.EventHandler(this.PortfolioRiskCalcSettingsFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbTimeframe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbCorrTimeframeCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTestTimeframeCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTestsCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbPercentiles;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbUploadQuotes;
    }
}