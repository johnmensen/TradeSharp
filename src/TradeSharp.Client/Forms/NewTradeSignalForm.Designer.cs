namespace TradeSharp.Client.Forms
{
    partial class NewTradeSignalForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewTradeSignalForm));
            this.panel4 = new System.Windows.Forms.Panel();
            this.cbEnableTradeSignals = new System.Windows.Forms.CheckBox();
            this.panelSignalsDetail = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnMakeSignalService = new System.Windows.Forms.Button();
            this.lblCurrency = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tbFeeSignalDay = new System.Windows.Forms.TextBox();
            this.tbFeeSignalMonth = new System.Windows.Forms.TextBox();
            this.cbPaidSignals = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.panel4.SuspendLayout();
            this.panelSignalsDetail.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.nameTextBox);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.cbEnableTradeSignals);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(600, 57);
            this.panel4.TabIndex = 3;
            // 
            // cbEnableTradeSignals
            // 
            this.cbEnableTradeSignals.AutoSize = true;
            this.cbEnableTradeSignals.Location = new System.Drawing.Point(10, 29);
            this.cbEnableTradeSignals.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbEnableTradeSignals.Name = "cbEnableTradeSignals";
            this.cbEnableTradeSignals.Size = new System.Drawing.Size(446, 20);
            this.cbEnableTradeSignals.TabIndex = 1;
            this.cbEnableTradeSignals.Text = "Разрешить трейдерам подписываться на мои торговые сигналы";
            this.cbEnableTradeSignals.UseVisualStyleBackColor = true;
            this.cbEnableTradeSignals.CheckStateChanged += new System.EventHandler(this.CbEnableTradeSignalsCheckStateChanged);
            // 
            // panelSignalsDetail
            // 
            this.panelSignalsDetail.Controls.Add(this.btnCancel);
            this.panelSignalsDetail.Controls.Add(this.btnMakeSignalService);
            this.panelSignalsDetail.Controls.Add(this.lblCurrency);
            this.panelSignalsDetail.Controls.Add(this.label19);
            this.panelSignalsDetail.Controls.Add(this.label18);
            this.panelSignalsDetail.Controls.Add(this.label17);
            this.panelSignalsDetail.Controls.Add(this.label16);
            this.panelSignalsDetail.Controls.Add(this.tbFeeSignalDay);
            this.panelSignalsDetail.Controls.Add(this.tbFeeSignalMonth);
            this.panelSignalsDetail.Controls.Add(this.cbPaidSignals);
            this.panelSignalsDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSignalsDetail.Enabled = false;
            this.panelSignalsDetail.Location = new System.Drawing.Point(0, 57);
            this.panelSignalsDetail.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelSignalsDetail.Name = "panelSignalsDetail";
            this.panelSignalsDetail.Size = new System.Drawing.Size(600, 159);
            this.panelSignalsDetail.TabIndex = 4;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(458, 125);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(132, 28);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnMakeSignalService
            // 
            this.btnMakeSignalService.Location = new System.Drawing.Point(10, 125);
            this.btnMakeSignalService.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMakeSignalService.Name = "btnMakeSignalService";
            this.btnMakeSignalService.Size = new System.Drawing.Size(440, 28);
            this.btnMakeSignalService.TabIndex = 8;
            this.btnMakeSignalService.Text = "Предоставить торговые сигналы";
            this.btnMakeSignalService.UseVisualStyleBackColor = true;
            this.btnMakeSignalService.Click += new System.EventHandler(this.BtnMakeSignalServiceClick);
            // 
            // lblCurrency
            // 
            this.lblCurrency.AutoSize = true;
            this.lblCurrency.Location = new System.Drawing.Point(291, 98);
            this.lblCurrency.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCurrency.Name = "lblCurrency";
            this.lblCurrency.Size = new System.Drawing.Size(12, 16);
            this.lblCurrency.TabIndex = 7;
            this.lblCurrency.Text = "-";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(151, 70);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(12, 16);
            this.label19.TabIndex = 6;
            this.label19.Text = "/";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label18.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label18.Location = new System.Drawing.Point(206, 42);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(13, 16);
            this.label18.TabIndex = 5;
            this.label18.Text = "*";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(104, 98);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(39, 16);
            this.label17.TabIndex = 4;
            this.label17.Text = "день";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(151, 45);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 16);
            this.label16.TabIndex = 3;
            this.label16.Text = "месяц";
            // 
            // tbFeeSignalDay
            // 
            this.tbFeeSignalDay.Enabled = false;
            this.tbFeeSignalDay.Location = new System.Drawing.Point(151, 95);
            this.tbFeeSignalDay.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbFeeSignalDay.Name = "tbFeeSignalDay";
            this.tbFeeSignalDay.Size = new System.Drawing.Size(132, 22);
            this.tbFeeSignalDay.TabIndex = 2;
            this.tbFeeSignalDay.TextChanged += new System.EventHandler(this.TbFeeSignalMonthTextChanged);
            // 
            // tbFeeSignalMonth
            // 
            this.tbFeeSignalMonth.Enabled = false;
            this.tbFeeSignalMonth.Location = new System.Drawing.Point(11, 39);
            this.tbFeeSignalMonth.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbFeeSignalMonth.Name = "tbFeeSignalMonth";
            this.tbFeeSignalMonth.Size = new System.Drawing.Size(132, 22);
            this.tbFeeSignalMonth.TabIndex = 1;
            this.tbFeeSignalMonth.TextChanged += new System.EventHandler(this.TbFeeSignalMonthTextChanged);
            // 
            // cbPaidSignals
            // 
            this.cbPaidSignals.AutoSize = true;
            this.cbPaidSignals.Location = new System.Drawing.Point(11, 11);
            this.cbPaidSignals.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbPaidSignals.Name = "cbPaidSignals";
            this.cbPaidSignals.Size = new System.Drawing.Size(311, 20);
            this.cbPaidSignals.TabIndex = 0;
            this.cbPaidSignals.Text = "Комиссия за использование моих сигналов";
            this.cbPaidSignals.UseVisualStyleBackColor = true;
            this.cbPaidSignals.CheckedChanged += new System.EventHandler(this.CbPaidSignalsCheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Название";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(84, 4);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(367, 22);
            this.nameTextBox.TabIndex = 3;
            this.nameTextBox.Text = "Торговые сигналы";
            // 
            // NewTradeSignalForm
            // 
            this.AcceptButton = this.btnMakeSignalService;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(600, 216);
            this.Controls.Add(this.panelSignalsDetail);
            this.Controls.Add(this.panel4);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "NewTradeSignalForm";
            this.Text = "Предоставить торговые сигналы";
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panelSignalsDetail.ResumeLayout(false);
            this.panelSignalsDetail.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckBox cbEnableTradeSignals;
        private System.Windows.Forms.Panel panelSignalsDetail;
        private System.Windows.Forms.Button btnMakeSignalService;
        private System.Windows.Forms.Label lblCurrency;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox tbFeeSignalDay;
        private System.Windows.Forms.TextBox tbFeeSignalMonth;
        private System.Windows.Forms.CheckBox cbPaidSignals;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label label1;

    }
}