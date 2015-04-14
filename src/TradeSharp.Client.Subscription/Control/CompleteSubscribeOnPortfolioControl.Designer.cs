namespace TradeSharp.Client.Subscription.Control
{
    partial class CompleteSubscribeOnPortfolioControl
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblPortfolioName = new System.Windows.Forms.Label();
            this.formulaBrowser = new System.Windows.Forms.WebBrowser();
            this.lblCount = new System.Windows.Forms.Label();
            this.lblCompleteConfirm = new System.Windows.Forms.Label();
            this.tbCount = new System.Windows.Forms.NumericUpDown();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCount)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblPortfolioName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(476, 28);
            this.panel1.TabIndex = 0;
            // 
            // lblPortfolioName
            // 
            this.lblPortfolioName.AutoSize = true;
            this.lblPortfolioName.Location = new System.Drawing.Point(3, 9);
            this.lblPortfolioName.Name = "lblPortfolioName";
            this.lblPortfolioName.Size = new System.Drawing.Size(10, 13);
            this.lblPortfolioName.TabIndex = 0;
            this.lblPortfolioName.Text = "-";
            // 
            // formulaBrowser
            // 
            this.formulaBrowser.Dock = System.Windows.Forms.DockStyle.Top;
            this.formulaBrowser.Location = new System.Drawing.Point(0, 28);
            this.formulaBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.formulaBrowser.Name = "formulaBrowser";
            this.formulaBrowser.Size = new System.Drawing.Size(476, 128);
            this.formulaBrowser.TabIndex = 1;
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(3, 168);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(73, 13);
            this.lblCount.TabIndex = 2;
            this.lblCount.Tag = "TitlePartisipantsCount";
            this.lblCount.Text = "Участников: ";
            // 
            // lblCompleteConfirm
            // 
            this.lblCompleteConfirm.AutoSize = true;
            this.lblCompleteConfirm.Location = new System.Drawing.Point(3, 195);
            this.lblCompleteConfirm.Name = "lblCompleteConfirm";
            this.lblCompleteConfirm.Size = new System.Drawing.Size(196, 13);
            this.lblCompleteConfirm.TabIndex = 6;
            this.lblCompleteConfirm.Tag = "MessageProceedSubscribeOnPortfolio";
            this.lblCompleteConfirm.Text = "Подтвердить подписку на портфель?";
            // 
            // tbCount
            // 
            this.tbCount.Location = new System.Drawing.Point(82, 164);
            this.tbCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tbCount.Name = "tbCount";
            this.tbCount.Size = new System.Drawing.Size(64, 20);
            this.tbCount.TabIndex = 7;
            this.tbCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnOk);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 292);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(476, 36);
            this.panelBottom.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(169, 7);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(105, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(6, 7);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(105, 23);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // CompleteSubscribeOnPortfolioControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.tbCount);
            this.Controls.Add(this.lblCompleteConfirm);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.formulaBrowser);
            this.Controls.Add(this.panel1);
            this.Name = "CompleteSubscribeOnPortfolioControl";
            this.Size = new System.Drawing.Size(476, 328);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCount)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblPortfolioName;
        private System.Windows.Forms.WebBrowser formulaBrowser;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label lblCompleteConfirm;
        private System.Windows.Forms.NumericUpDown tbCount;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
    }
}
