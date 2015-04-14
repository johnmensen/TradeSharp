namespace TradeSharp.Client.Subscription.Dialog
{
    partial class AutoTradeSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoTradeSettingsForm));
            this.tradeSettings = new TradeSharp.Client.Subscription.Control.TradeSetting();
            this.SuspendLayout();
            // 
            // tradeSettings
            // 
            this.tradeSettings.DataContext = ((TradeSharp.Contract.Entity.AutoTradeSettings)(resources.GetObject("tradeSettings.DataContext")));
            this.tradeSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tradeSettings.Location = new System.Drawing.Point(0, 0);
            this.tradeSettings.Name = "tradeSettings";
            this.tradeSettings.Size = new System.Drawing.Size(384, 342);
            this.tradeSettings.TabIndex = 0;
            this.tradeSettings.ButtonOkClicked += new System.Action(this.TradeSettingsButtonOkClicked);
            // 
            // AutoTradeSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 342);
            this.Controls.Add(this.tradeSettings);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(400, 380);
            this.MinimumSize = new System.Drawing.Size(400, 380);
            this.Name = "AutoTradeSettingsForm";
            this.Text = "Авто-торговля";
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.AutoTradeSettingsFormHelpRequested);
            this.ResumeLayout(false);

        }

        #endregion

        private Control.TradeSetting tradeSettings;

    }
}