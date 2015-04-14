namespace TradeSharp.Client.Forms
{
    partial class ChangeBalanceDemoAccountForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeBalanceDemoAccountForm));
            this.label1 = new System.Windows.Forms.Label();
            this.cbAccount = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSumm = new System.Windows.Forms.TextBox();
            this.btnAddSumm = new System.Windows.Forms.Button();
            this.btnNegSumm = new System.Windows.Forms.Button();
            this.lbAccount = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "TitleSelectAccount";
            this.label1.Text = "Выберите счет:";
            // 
            // cbAccount
            // 
            this.cbAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAccount.FormattingEnabled = true;
            this.cbAccount.Location = new System.Drawing.Point(99, 12);
            this.cbAccount.Name = "cbAccount";
            this.cbAccount.Size = new System.Drawing.Size(100, 21);
            this.cbAccount.TabIndex = 1;
            this.cbAccount.SelectedIndexChanged += new System.EventHandler(this.CbAccountSelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 2;
            this.label2.Tag = "TitleSum";
            this.label2.Text = "Сумма:";
            // 
            // tbSumm
            // 
            this.tbSumm.Location = new System.Drawing.Point(99, 71);
            this.tbSumm.Name = "tbSumm";
            this.tbSumm.Size = new System.Drawing.Size(100, 20);
            this.tbSumm.TabIndex = 3;
            // 
            // btnAddSumm
            // 
            this.btnAddSumm.AutoSize = true;
            this.btnAddSumm.Location = new System.Drawing.Point(3, 3);
            this.btnAddSumm.Name = "btnAddSumm";
            this.btnAddSumm.Size = new System.Drawing.Size(107, 23);
            this.btnAddSumm.TabIndex = 4;
            this.btnAddSumm.Tag = "TitleDepositOnAccount";
            this.btnAddSumm.Text = "Добавить на счет";
            this.btnAddSumm.UseVisualStyleBackColor = true;
            this.btnAddSumm.Click += new System.EventHandler(this.BtnAddSummClick);
            // 
            // btnNegSumm
            // 
            this.btnNegSumm.AutoSize = true;
            this.btnNegSumm.Location = new System.Drawing.Point(116, 3);
            this.btnNegSumm.Name = "btnNegSumm";
            this.btnNegSumm.Size = new System.Drawing.Size(107, 23);
            this.btnNegSumm.TabIndex = 5;
            this.btnNegSumm.Tag = "TitleWithdrawFromAccount";
            this.btnNegSumm.Text = "Снять со счета";
            this.btnNegSumm.UseVisualStyleBackColor = true;
            this.btnNegSumm.Click += new System.EventHandler(this.BtnNegSummClick);
            // 
            // lbAccount
            // 
            this.lbAccount.AutoSize = true;
            this.lbAccount.Location = new System.Drawing.Point(101, 45);
            this.lbAccount.Name = "lbAccount";
            this.lbAccount.Size = new System.Drawing.Size(0, 13);
            this.lbAccount.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnAddSumm);
            this.flowLayoutPanel1.Controls.Add(this.btnNegSumm);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 104);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(291, 29);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // ChangeBalanceDemoAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 133);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lbAccount);
            this.Controls.Add(this.tbSumm);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbAccount);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangeBalanceDemoAccountForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleChangeDemoAccountBalance";
            this.Text = "Изменить баланс демо-счета";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbAccount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSumm;
        private System.Windows.Forms.Button btnAddSumm;
        private System.Windows.Forms.Button btnNegSumm;
        private System.Windows.Forms.Label lbAccount;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}