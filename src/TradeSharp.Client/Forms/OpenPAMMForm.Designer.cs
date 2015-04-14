namespace TradeSharp.Client.Forms
{
    partial class OpenPAMMForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenPAMMForm));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.cbEnablePAMM = new System.Windows.Forms.CheckBox();
            this.cbMoreRate = new System.Windows.Forms.CheckBox();
            this.tbPercentMore = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLargeDepo = new System.Windows.Forms.TextBox();
            this.lblDepoCurrency = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbPercent = new System.Windows.Forms.TextBox();
            this.panelBottom.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.button2);
            this.panelBottom.Controls.Add(this.btnAccept);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 310);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(580, 31);
            this.panelBottom.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(157, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Отмена";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(5, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(90, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.cbEnablePAMM);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(580, 30);
            this.panelTop.TabIndex = 4;
            // 
            // cbEnablePAMM
            // 
            this.cbEnablePAMM.AutoSize = true;
            this.cbEnablePAMM.Location = new System.Drawing.Point(5, 5);
            this.cbEnablePAMM.Margin = new System.Windows.Forms.Padding(5);
            this.cbEnablePAMM.Name = "cbEnablePAMM";
            this.cbEnablePAMM.Size = new System.Drawing.Size(414, 20);
            this.cbEnablePAMM.TabIndex = 3;
            this.cbEnablePAMM.Text = "Разрешить трейдерам инвестировать в мой торговый счет";
            this.cbEnablePAMM.UseVisualStyleBackColor = true;
            // 
            // cbMoreRate
            // 
            this.cbMoreRate.AutoSize = true;
            this.cbMoreRate.Location = new System.Drawing.Point(93, 64);
            this.cbMoreRate.Name = "cbMoreRate";
            this.cbMoreRate.Size = new System.Drawing.Size(15, 14);
            this.cbMoreRate.TabIndex = 5;
            this.cbMoreRate.UseVisualStyleBackColor = true;
            this.cbMoreRate.CheckedChanged += new System.EventHandler(this.cbMoreRate_CheckedChanged);
            // 
            // tbPercentMore
            // 
            this.tbPercentMore.Enabled = false;
            this.tbPercentMore.Location = new System.Drawing.Point(5, 59);
            this.tbPercentMore.Name = "tbPercentMore";
            this.tbPercentMore.Size = new System.Drawing.Size(56, 22);
            this.tbPercentMore.TabIndex = 6;
            this.tbPercentMore.Text = "10";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(67, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "%";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(114, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "для депозита от";
            // 
            // tbLargeDepo
            // 
            this.tbLargeDepo.Enabled = false;
            this.tbLargeDepo.Location = new System.Drawing.Point(235, 59);
            this.tbLargeDepo.Name = "tbLargeDepo";
            this.tbLargeDepo.Size = new System.Drawing.Size(100, 22);
            this.tbLargeDepo.TabIndex = 9;
            this.tbLargeDepo.Text = "10 000";
            // 
            // lblDepoCurrency
            // 
            this.lblDepoCurrency.AutoSize = true;
            this.lblDepoCurrency.Location = new System.Drawing.Point(341, 62);
            this.lblDepoCurrency.Name = "lblDepoCurrency";
            this.lblDepoCurrency.Size = new System.Drawing.Size(37, 16);
            this.lblDepoCurrency.TabIndex = 10;
            this.lblDepoCurrency.Text = "USD";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(67, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "%";
            // 
            // tbPercent
            // 
            this.tbPercent.Location = new System.Drawing.Point(5, 33);
            this.tbPercent.Name = "tbPercent";
            this.tbPercent.Size = new System.Drawing.Size(56, 22);
            this.tbPercent.TabIndex = 11;
            this.tbPercent.Text = "15";
            // 
            // OpenPAMMForm
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(580, 341);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbPercent);
            this.Controls.Add(this.lblDepoCurrency);
            this.Controls.Add(this.tbLargeDepo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbPercentMore);
            this.Controls.Add(this.cbMoreRate);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OpenPAMMForm";
            this.Text = "Открыть ПАММ счет";
            this.panelBottom.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.CheckBox cbEnablePAMM;
        private System.Windows.Forms.CheckBox cbMoreRate;
        private System.Windows.Forms.TextBox tbPercentMore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLargeDepo;
        private System.Windows.Forms.Label lblDepoCurrency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbPercent;
    }
}