namespace TradeSharp.Client.Forms
{
    partial class WalletWithdrawForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WalletWithdrawForm));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageDeposit = new System.Windows.Forms.TabPage();
            this.btnDepositAll = new System.Windows.Forms.Button();
            this.lblDepositTarget = new System.Windows.Forms.Label();
            this.lblAccountAmount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblDepositWalletCurrency = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbDepositAmount = new System.Windows.Forms.TextBox();
            this.tabPageWithdraw = new System.Windows.Forms.TabPage();
            this.lblAccountRemains = new System.Windows.Forms.Label();
            this.lblMargin = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblWithdrawCurrency = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbWithdraw = new System.Windows.Forms.TextBox();
            this.cbWithdrawAll = new System.Windows.Forms.CheckBox();
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageDeposit.SuspendLayout();
            this.tabPageWithdraw.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnOK);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 239);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(398, 35);
            this.panelBottom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(153, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "Подтвердить";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageDeposit);
            this.tabControl.Controls.Add(this.tabPageWithdraw);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(398, 239);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageDeposit
            // 
            this.tabPageDeposit.Controls.Add(this.btnDepositAll);
            this.tabPageDeposit.Controls.Add(this.lblDepositTarget);
            this.tabPageDeposit.Controls.Add(this.lblAccountAmount);
            this.tabPageDeposit.Controls.Add(this.label2);
            this.tabPageDeposit.Controls.Add(this.lblDepositWalletCurrency);
            this.tabPageDeposit.Controls.Add(this.label1);
            this.tabPageDeposit.Controls.Add(this.tbDepositAmount);
            this.tabPageDeposit.Location = new System.Drawing.Point(4, 22);
            this.tabPageDeposit.Name = "tabPageDeposit";
            this.tabPageDeposit.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDeposit.Size = new System.Drawing.Size(390, 213);
            this.tabPageDeposit.TabIndex = 0;
            this.tabPageDeposit.Text = "Пополнить счет";
            this.tabPageDeposit.UseVisualStyleBackColor = true;
            // 
            // btnDepositAll
            // 
            this.btnDepositAll.Location = new System.Drawing.Point(203, 10);
            this.btnDepositAll.Name = "btnDepositAll";
            this.btnDepositAll.Size = new System.Drawing.Size(48, 23);
            this.btnDepositAll.TabIndex = 6;
            this.btnDepositAll.Text = "все";
            this.btnDepositAll.UseVisualStyleBackColor = true;
            this.btnDepositAll.Click += new System.EventHandler(this.BtnDepositAllClick);
            // 
            // lblDepositTarget
            // 
            this.lblDepositTarget.AutoSize = true;
            this.lblDepositTarget.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.lblDepositTarget.Location = new System.Drawing.Point(104, 62);
            this.lblDepositTarget.Name = "lblDepositTarget";
            this.lblDepositTarget.Size = new System.Drawing.Size(10, 13);
            this.lblDepositTarget.TabIndex = 5;
            this.lblDepositTarget.Text = "-";
            // 
            // lblAccountAmount
            // 
            this.lblAccountAmount.AutoSize = true;
            this.lblAccountAmount.Location = new System.Drawing.Point(104, 42);
            this.lblAccountAmount.Name = "lblAccountAmount";
            this.lblAccountAmount.Size = new System.Drawing.Size(10, 13);
            this.lblAccountAmount.TabIndex = 4;
            this.lblAccountAmount.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Сумма на счете:";
            // 
            // lblDepositWalletCurrency
            // 
            this.lblDepositWalletCurrency.AutoSize = true;
            this.lblDepositWalletCurrency.Location = new System.Drawing.Point(146, 14);
            this.lblDepositWalletCurrency.Name = "lblDepositWalletCurrency";
            this.lblDepositWalletCurrency.Size = new System.Drawing.Size(10, 13);
            this.lblDepositWalletCurrency.TabIndex = 2;
            this.lblDepositWalletCurrency.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Сумма";
            // 
            // tbDepositAmount
            // 
            this.tbDepositAmount.Location = new System.Drawing.Point(55, 11);
            this.tbDepositAmount.Name = "tbDepositAmount";
            this.tbDepositAmount.Size = new System.Drawing.Size(85, 20);
            this.tbDepositAmount.TabIndex = 0;
            this.tbDepositAmount.TextChanged += new System.EventHandler(this.TbDepositAmountTextChanged);
            // 
            // tabPageWithdraw
            // 
            this.tabPageWithdraw.Controls.Add(this.cbWithdrawAll);
            this.tabPageWithdraw.Controls.Add(this.lblAccountRemains);
            this.tabPageWithdraw.Controls.Add(this.lblMargin);
            this.tabPageWithdraw.Controls.Add(this.label5);
            this.tabPageWithdraw.Controls.Add(this.label3);
            this.tabPageWithdraw.Controls.Add(this.lblWithdrawCurrency);
            this.tabPageWithdraw.Controls.Add(this.label4);
            this.tabPageWithdraw.Controls.Add(this.tbWithdraw);
            this.tabPageWithdraw.Location = new System.Drawing.Point(4, 22);
            this.tabPageWithdraw.Name = "tabPageWithdraw";
            this.tabPageWithdraw.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWithdraw.Size = new System.Drawing.Size(390, 213);
            this.tabPageWithdraw.TabIndex = 1;
            this.tabPageWithdraw.Text = "Вывести средства в кошелек";
            this.tabPageWithdraw.UseVisualStyleBackColor = true;
            // 
            // lblAccountRemains
            // 
            this.lblAccountRemains.AutoSize = true;
            this.lblAccountRemains.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.lblAccountRemains.Location = new System.Drawing.Point(131, 67);
            this.lblAccountRemains.Name = "lblAccountRemains";
            this.lblAccountRemains.Size = new System.Drawing.Size(10, 13);
            this.lblAccountRemains.TabIndex = 14;
            this.lblAccountRemains.Text = "-";
            // 
            // lblMargin
            // 
            this.lblMargin.AutoSize = true;
            this.lblMargin.Location = new System.Drawing.Point(131, 45);
            this.lblMargin.Name = "lblMargin";
            this.lblMargin.Size = new System.Drawing.Size(10, 13);
            this.lblMargin.TabIndex = 13;
            this.lblMargin.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Марж. залог:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Остаток на счете:";
            // 
            // lblWithdrawCurrency
            // 
            this.lblWithdrawCurrency.AutoSize = true;
            this.lblWithdrawCurrency.Location = new System.Drawing.Point(147, 9);
            this.lblWithdrawCurrency.Name = "lblWithdrawCurrency";
            this.lblWithdrawCurrency.Size = new System.Drawing.Size(10, 13);
            this.lblWithdrawCurrency.TabIndex = 9;
            this.lblWithdrawCurrency.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Сумма";
            // 
            // tbWithdraw
            // 
            this.tbWithdraw.Location = new System.Drawing.Point(56, 6);
            this.tbWithdraw.Name = "tbWithdraw";
            this.tbWithdraw.Size = new System.Drawing.Size(85, 20);
            this.tbWithdraw.TabIndex = 7;
            this.tbWithdraw.TextChanged += new System.EventHandler(this.TbWithdrawTextChanged);
            // 
            // cbWithdrawAll
            // 
            this.cbWithdrawAll.AutoSize = true;
            this.cbWithdrawAll.Location = new System.Drawing.Point(203, 9);
            this.cbWithdrawAll.Name = "cbWithdrawAll";
            this.cbWithdrawAll.Size = new System.Drawing.Size(90, 17);
            this.cbWithdrawAll.TabIndex = 15;
            this.cbWithdrawAll.Text = "вывести всё";
            this.cbWithdrawAll.UseVisualStyleBackColor = true;
            this.cbWithdrawAll.CheckedChanged += new System.EventHandler(this.cbWithdrawAll_CheckedChanged);
            // 
            // WalletWithdrawForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(398, 274);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WalletWithdrawForm";
            this.Text = "WalletWithdrawForm";
            this.panelBottom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageDeposit.ResumeLayout(false);
            this.tabPageDeposit.PerformLayout();
            this.tabPageWithdraw.ResumeLayout(false);
            this.tabPageWithdraw.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageDeposit;
        private System.Windows.Forms.Button btnDepositAll;
        private System.Windows.Forms.Label lblDepositTarget;
        private System.Windows.Forms.Label lblAccountAmount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblDepositWalletCurrency;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDepositAmount;
        private System.Windows.Forms.TabPage tabPageWithdraw;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblWithdrawCurrency;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbWithdraw;
        private System.Windows.Forms.Label lblAccountRemains;
        private System.Windows.Forms.Label lblMargin;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbWithdrawAll;

    }
}