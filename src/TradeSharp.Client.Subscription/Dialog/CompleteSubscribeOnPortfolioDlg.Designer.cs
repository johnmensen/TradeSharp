namespace TradeSharp.Client.Subscription.Dialog
{
    partial class CompleteSubscribeOnPortfolioDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompleteSubscribeOnPortfolioDlg));
            this.completeSubscribeOnPortfolioControl1 = new TradeSharp.Client.Subscription.Control.CompleteSubscribeOnPortfolioControl();
            this.SuspendLayout();
            // 
            // completeSubscribeOnPortfolioControl1
            // 
            this.completeSubscribeOnPortfolioControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.completeSubscribeOnPortfolioControl1.Location = new System.Drawing.Point(0, 0);
            this.completeSubscribeOnPortfolioControl1.Name = "completeSubscribeOnPortfolioControl1";
            this.completeSubscribeOnPortfolioControl1.Size = new System.Drawing.Size(331, 255);
            this.completeSubscribeOnPortfolioControl1.TabIndex = 0;
            // 
            // CompleteSubscribeOnPortfolioDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 255);
            this.Controls.Add(this.completeSubscribeOnPortfolioControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CompleteSubscribeOnPortfolioDlg";
            this.Tag = "TitleConfirmSubscription";
            this.Text = "Подтвердить подписку";
            this.ResumeLayout(false);

        }

        #endregion

        private Control.CompleteSubscribeOnPortfolioControl completeSubscribeOnPortfolioControl1;
    }
}