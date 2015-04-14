namespace TradeSharp.QuoteAdmin.Forms
{
    partial class FillHistoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FillHistoryForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnFill = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbQuoteSource = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTickers = new System.Windows.Forms.TextBox();
            this.tbGMT = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dpEnd = new System.Windows.Forms.DateTimePicker();
            this.dpStart = new System.Windows.Forms.DateTimePicker();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tbStatus = new System.Windows.Forms.RichTextBox();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnFill);
            this.panelTop.Controls.Add(this.label4);
            this.panelTop.Controls.Add(this.cbQuoteSource);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.tbTickers);
            this.panelTop.Controls.Add(this.tbGMT);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.dpEnd);
            this.panelTop.Controls.Add(this.dpStart);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(364, 206);
            this.panelTop.TabIndex = 0;
            // 
            // btnFill
            // 
            this.btnFill.Location = new System.Drawing.Point(6, 173);
            this.btnFill.Name = "btnFill";
            this.btnFill.Size = new System.Drawing.Size(92, 27);
            this.btnFill.TabIndex = 19;
            this.btnFill.Text = "Заполнить";
            this.btnFill.UseVisualStyleBackColor = true;
            this.btnFill.Click += new System.EventHandler(this.BtnFillClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Источник";
            // 
            // cbQuoteSource
            // 
            this.cbQuoteSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbQuoteSource.FormattingEnabled = true;
            this.cbQuoteSource.Location = new System.Drawing.Point(64, 120);
            this.cbQuoteSource.Name = "cbQuoteSource";
            this.cbQuoteSource.Size = new System.Drawing.Size(121, 21);
            this.cbQuoteSource.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Котировки";
            // 
            // tbTickers
            // 
            this.tbTickers.Location = new System.Drawing.Point(6, 71);
            this.tbTickers.Multiline = true;
            this.tbTickers.Name = "tbTickers";
            this.tbTickers.Size = new System.Drawing.Size(284, 43);
            this.tbTickers.TabIndex = 15;
            // 
            // tbGMT
            // 
            this.tbGMT.Location = new System.Drawing.Point(104, 33);
            this.tbGMT.Name = "tbGMT";
            this.tbGMT.Size = new System.Drawing.Size(35, 20);
            this.tbGMT.TabIndex = 14;
            this.tbGMT.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Коррекция (GMT)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(141, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "-";
            // 
            // dpEnd
            // 
            this.dpEnd.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpEnd.Location = new System.Drawing.Point(155, 3);
            this.dpEnd.Name = "dpEnd";
            this.dpEnd.Size = new System.Drawing.Size(135, 20);
            this.dpEnd.TabIndex = 11;
            // 
            // dpStart
            // 
            this.dpStart.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpStart.Location = new System.Drawing.Point(3, 3);
            this.dpStart.Name = "dpStart";
            this.dpStart.Size = new System.Drawing.Size(136, 20);
            this.dpStart.TabIndex = 10;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 338);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(364, 23);
            this.progressBar.TabIndex = 1;
            // 
            // tbStatus
            // 
            this.tbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbStatus.Location = new System.Drawing.Point(0, 206);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.Size = new System.Drawing.Size(364, 132);
            this.tbStatus.TabIndex = 2;
            this.tbStatus.Text = "";
            // 
            // FillHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 361);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FillHistoryForm";
            this.Text = "Заполнить историю";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnFill;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbQuoteSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTickers;
        private System.Windows.Forms.TextBox tbGMT;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dpEnd;
        private System.Windows.Forms.DateTimePicker dpStart;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.RichTextBox tbStatus;

    }
}