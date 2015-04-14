namespace TradeSharp.Client.Forms
{
    partial class SubscriptionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubscriptionForm));
            this.subscriptionControl = new TradeSharp.Client.Subscription.Control.SubscriptionControl();
            this.SuspendLayout();
            // 
            // subscriptionControl
            // 
            this.subscriptionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subscriptionControl.Location = new System.Drawing.Point(0, 0);
            this.subscriptionControl.Name = "subscriptionControl";
            this.subscriptionControl.Size = new System.Drawing.Size(992, 423);
            this.subscriptionControl.TabIndex = 0;
            // 
            // SubscriptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 423);
            this.Controls.Add(this.subscriptionControl);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "SubscriptionForm";
            this.Tag = "TitleTradeSignals";
            this.Text = "Торговые сигналы";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SubscriptionFormFormClosing);
            this.Load += new System.EventHandler(this.SubscriptionFormLoad);
            this.ResizeEnd += new System.EventHandler(this.SubscriptionFormResizeEnd);
            this.Move += new System.EventHandler(this.SubscriptionFormMove);
            this.ResumeLayout(false);

        }

        #endregion

        private Subscription.Control.SubscriptionControl subscriptionControl;
    }
}