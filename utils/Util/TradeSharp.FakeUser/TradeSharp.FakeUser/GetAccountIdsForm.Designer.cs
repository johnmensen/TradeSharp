namespace TradeSharp.FakeUser
{
    partial class GetAccountIdsForm
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
            this.cbGroup = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dpTimeStart = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dpTimeEnd = new System.Windows.Forms.DateTimePicker();
            this.cbCheckPassword = new System.Windows.Forms.CheckBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnCopyAndClose = new System.Windows.Forms.Button();
            this.cbSignallersOnly = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbGroup
            // 
            this.cbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGroup.FormattingEnabled = true;
            this.cbGroup.Location = new System.Drawing.Point(12, 26);
            this.cbGroup.Name = "cbGroup";
            this.cbGroup.Size = new System.Drawing.Size(222, 21);
            this.cbGroup.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "группа счетов";
            // 
            // dpTimeStart
            // 
            this.dpTimeStart.Location = new System.Drawing.Point(15, 72);
            this.dpTimeStart.Name = "dpTimeStart";
            this.dpTimeStart.Size = new System.Drawing.Size(219, 20);
            this.dpTimeStart.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "дата создания, от";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "дата создания, до";
            // 
            // dpTimeEnd
            // 
            this.dpTimeEnd.Location = new System.Drawing.Point(15, 111);
            this.dpTimeEnd.Name = "dpTimeEnd";
            this.dpTimeEnd.Size = new System.Drawing.Size(219, 20);
            this.dpTimeEnd.TabIndex = 4;
            // 
            // cbCheckPassword
            // 
            this.cbCheckPassword.AutoSize = true;
            this.cbCheckPassword.Checked = true;
            this.cbCheckPassword.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCheckPassword.Location = new System.Drawing.Point(15, 137);
            this.cbCheckPassword.Name = "cbCheckPassword";
            this.cbCheckPassword.Size = new System.Drawing.Size(62, 17);
            this.cbCheckPassword.TabIndex = 6;
            this.cbCheckPassword.Text = "пароль";
            this.cbCheckPassword.UseVisualStyleBackColor = true;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(15, 160);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(219, 20);
            this.tbPassword.TabIndex = 7;
            this.tbPassword.Text = "Trader01";
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(12, 236);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(75, 23);
            this.btnCheck.TabIndex = 8;
            this.btnCheck.Text = "Найти";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnCopyAndClose
            // 
            this.btnCopyAndClose.Location = new System.Drawing.Point(103, 236);
            this.btnCopyAndClose.Name = "btnCopyAndClose";
            this.btnCopyAndClose.Size = new System.Drawing.Size(146, 23);
            this.btnCopyAndClose.TabIndex = 9;
            this.btnCopyAndClose.Text = "Копировать и закрыть";
            this.btnCopyAndClose.UseVisualStyleBackColor = true;
            this.btnCopyAndClose.Click += new System.EventHandler(this.btnCopyAndClose_Click);
            // 
            // cbSignallersOnly
            // 
            this.cbSignallersOnly.AutoSize = true;
            this.cbSignallersOnly.Location = new System.Drawing.Point(15, 186);
            this.cbSignallersOnly.Name = "cbSignallersOnly";
            this.cbSignallersOnly.Size = new System.Drawing.Size(132, 17);
            this.cbSignallersOnly.TabIndex = 10;
            this.cbSignallersOnly.Text = "только сигнальщики";
            this.cbSignallersOnly.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(264, 236);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // GetAccountIdsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 271);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.cbSignallersOnly);
            this.Controls.Add(this.btnCopyAndClose);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.cbCheckPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dpTimeEnd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dpTimeStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbGroup);
            this.Name = "GetAccountIdsForm";
            this.Text = "Счета для фермы роботов";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dpTimeStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dpTimeEnd;
        private System.Windows.Forms.CheckBox cbCheckPassword;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Button btnCopyAndClose;
        private System.Windows.Forms.CheckBox cbSignallersOnly;
        private System.Windows.Forms.Button btnCancel;
    }
}