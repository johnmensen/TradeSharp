namespace TradeSharp.Client.BL.Script
{
    partial class SetupStopRobotDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupStopRobotDlg));
            this.cbRobots = new System.Windows.Forms.ComboBox();
            this.cbPriceType = new System.Windows.Forms.ComboBox();
            this.tbPrice = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tbPrice)).BeginInit();
            this.SuspendLayout();
            // 
            // cbRobots
            // 
            this.cbRobots.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRobots.FormattingEnabled = true;
            this.cbRobots.Location = new System.Drawing.Point(12, 12);
            this.cbRobots.Name = "cbRobots";
            this.cbRobots.Size = new System.Drawing.Size(211, 21);
            this.cbRobots.TabIndex = 0;
            // 
            // cbPriceType
            // 
            this.cbPriceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPriceType.FormattingEnabled = true;
            this.cbPriceType.Location = new System.Drawing.Point(12, 39);
            this.cbPriceType.Name = "cbPriceType";
            this.cbPriceType.Size = new System.Drawing.Size(102, 21);
            this.cbPriceType.TabIndex = 1;
            // 
            // tbPrice
            // 
            this.tbPrice.Location = new System.Drawing.Point(12, 66);
            this.tbPrice.Name = "tbPrice";
            this.tbPrice.Size = new System.Drawing.Size(102, 20);
            this.tbPrice.TabIndex = 2;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 116);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "ОК";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(127, 116);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SetupStopRobotDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(267, 145);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbPrice);
            this.Controls.Add(this.cbPriceType);
            this.Controls.Add(this.cbRobots);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetupStopRobotDlg";
            this.Text = "Настройки робота - CS";
            this.Load += new System.EventHandler(this.SetupStopRobotDlgLoad);
            ((System.ComponentModel.ISupportInitialize)(this.tbPrice)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbRobots;
        private System.Windows.Forms.ComboBox cbPriceType;
        private System.Windows.Forms.NumericUpDown tbPrice;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}