namespace TestMTTradeServer
{
    partial class UdpTestForm
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
            this.btnTest = new DevExpress.XtraEditors.SimpleButton();
            this.tbAddress = new DevExpress.XtraEditors.TextEdit();
            this.tbMessage = new DevExpress.XtraEditors.TextEdit();
            this.lblAddress = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.tbAddress.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbMessage.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(12, 100);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Отправить";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // tbAddress
            // 
            this.tbAddress.EditValue = "bugatti:18010";
            this.tbAddress.Location = new System.Drawing.Point(12, 12);
            this.tbAddress.Name = "tbAddress";
            this.tbAddress.Size = new System.Drawing.Size(167, 20);
            this.tbAddress.TabIndex = 1;
            // 
            // tbMessage
            // 
            this.tbMessage.EditValue = "<test>";
            this.tbMessage.Location = new System.Drawing.Point(12, 38);
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.Size = new System.Drawing.Size(322, 20);
            this.tbMessage.TabIndex = 2;
            // 
            // lblAddress
            // 
            this.lblAddress.Location = new System.Drawing.Point(185, 15);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(30, 13);
            this.lblAddress.TabIndex = 3;
            this.lblAddress.Text = "адрес";
            // 
            // UdpTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 135);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.tbMessage);
            this.Controls.Add(this.tbAddress);
            this.Controls.Add(this.btnTest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "UdpTestForm";
            this.Text = "Тест UDP";
            ((System.ComponentModel.ISupportInitialize)(this.tbAddress.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbMessage.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnTest;
        private DevExpress.XtraEditors.TextEdit tbAddress;
        private DevExpress.XtraEditors.TextEdit tbMessage;
        private DevExpress.XtraEditors.LabelControl lblAddress;
    }
}