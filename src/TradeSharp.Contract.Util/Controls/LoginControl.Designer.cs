namespace TradeSharp.Contract.Util.Controls
{
    partial class LoginControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbLogin = new System.Windows.Forms.ComboBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbSavePwrd = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbHidePassword = new System.Windows.Forms.CheckBox();
            this.linkRegistration = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // cbLogin
            // 
            this.cbLogin.FormattingEnabled = true;
            this.cbLogin.Location = new System.Drawing.Point(73, 3);
            this.cbLogin.Name = "cbLogin";
            this.cbLogin.Size = new System.Drawing.Size(121, 21);
            this.cbLogin.TabIndex = 0;
            this.cbLogin.SelectedIndexChanged += new System.EventHandler(this.CbLoginSelectedIndexChanged);
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(73, 30);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(121, 20);
            this.tbPassword.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 3;
            this.label1.Tag = "TitleLogin";
            this.label1.Text = "логин";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 4;
            this.label2.Tag = "TitlePassword";
            this.label2.Text = "пароль";
            // 
            // cbSavePwrd
            // 
            this.cbSavePwrd.AutoSize = true;
            this.cbSavePwrd.Location = new System.Drawing.Point(73, 77);
            this.cbSavePwrd.Name = "cbSavePwrd";
            this.cbSavePwrd.Size = new System.Drawing.Size(117, 17);
            this.cbSavePwrd.TabIndex = 4;
            this.cbSavePwrd.Tag = "TitleSavePassword";
            this.cbSavePwrd.Text = "сохранять пароль";
            this.cbSavePwrd.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(28, 115);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(75, 23);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Tag = "TitleLogin";
            this.btnLogin.Text = "Логин";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.BtnLoginClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(119, 115);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // cbHidePassword
            // 
            this.cbHidePassword.AutoSize = true;
            this.cbHidePassword.Location = new System.Drawing.Point(73, 56);
            this.cbHidePassword.Name = "cbHidePassword";
            this.cbHidePassword.Size = new System.Drawing.Size(112, 17);
            this.cbHidePassword.TabIndex = 7;
            this.cbHidePassword.Tag = "TitleHideText";
            this.cbHidePassword.Text = "скрыть символы";
            this.cbHidePassword.UseVisualStyleBackColor = true;
            this.cbHidePassword.CheckedChanged += new System.EventHandler(this.CbHidePasswordCheckedChanged);
            // 
            // linkRegistration
            // 
            this.linkRegistration.AutoSize = true;
            this.linkRegistration.Location = new System.Drawing.Point(70, 97);
            this.linkRegistration.Name = "linkRegistration";
            this.linkRegistration.Size = new System.Drawing.Size(74, 13);
            this.linkRegistration.TabIndex = 8;
            this.linkRegistration.TabStop = true;
            this.linkRegistration.Tag = "TitleCreateAccountLower";
            this.linkRegistration.Text = "открыть счет";
            this.linkRegistration.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkRegistrationLinkClicked);
            // 
            // LoginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkRegistration);
            this.Controls.Add(this.cbHidePassword);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.cbSavePwrd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.cbLogin);
            this.Name = "LoginControl";
            this.Size = new System.Drawing.Size(235, 145);
            this.Load += new System.EventHandler(this.LoginControlLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbLogin;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbSavePwrd;
        public System.Windows.Forms.Button btnLogin;
        public System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbHidePassword;
        private System.Windows.Forms.LinkLabel linkRegistration;
    }
}
