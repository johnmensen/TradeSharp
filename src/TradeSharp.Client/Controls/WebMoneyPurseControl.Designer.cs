namespace TradeSharp.Client.Controls
{
    partial class WebMoneyPurseControl
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebMoneyPurseControl));
            this.label1 = new System.Windows.Forms.Label();
            this.cbPurseCurrency = new System.Windows.Forms.ComboBox();
            this.tbPurse = new System.Windows.Forms.TextBox();
            this.lblValid = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.imageListGlypth = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "TitleWallet";
            this.label1.Text = "Кошелек";
            // 
            // cbPurseCurrency
            // 
            this.cbPurseCurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPurseCurrency.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbPurseCurrency.FormattingEnabled = true;
            this.cbPurseCurrency.Items.AddRange(new object[] {
            "R",
            "Z",
            "E",
            "U",
            "Y",
            "B",
            "G",
            "X"});
            this.cbPurseCurrency.Location = new System.Drawing.Point(60, 6);
            this.cbPurseCurrency.Name = "cbPurseCurrency";
            this.cbPurseCurrency.Size = new System.Drawing.Size(64, 21);
            this.cbPurseCurrency.TabIndex = 1;
            // 
            // tbPurse
            // 
            this.tbPurse.Location = new System.Drawing.Point(130, 6);
            this.tbPurse.Name = "tbPurse";
            this.tbPurse.Size = new System.Drawing.Size(172, 20);
            this.tbPurse.TabIndex = 2;
            this.tbPurse.TextChanged += new System.EventHandler(this.TbPurseTextChanged);
            // 
            // lblValid
            // 
            this.lblValid.AutoSize = true;
            this.lblValid.Location = new System.Drawing.Point(308, 12);
            this.lblValid.Name = "lblValid";
            this.lblValid.Size = new System.Drawing.Size(11, 13);
            this.lblValid.TabIndex = 3;
            this.lblValid.Text = "*";
            // 
            // btnClose
            // 
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClose.ImageIndex = 0;
            this.btnClose.ImageList = this.imageListGlypth;
            this.btnClose.Location = new System.Drawing.Point(330, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(32, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // imageListGlypth
            // 
            this.imageListGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlypth.ImageStream")));
            this.imageListGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlypth.Images.SetKeyName(0, "ico delete.png");
            // 
            // WebMoneyPurseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblValid);
            this.Controls.Add(this.tbPurse);
            this.Controls.Add(this.cbPurseCurrency);
            this.Controls.Add(this.label1);
            this.Name = "WebMoneyPurseControl";
            this.Size = new System.Drawing.Size(372, 31);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbPurseCurrency;
        private System.Windows.Forms.TextBox tbPurse;
        private System.Windows.Forms.Label lblValid;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ImageList imageListGlypth;
    }
}
