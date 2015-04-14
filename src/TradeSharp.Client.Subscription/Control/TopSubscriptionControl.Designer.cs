namespace TradeSharp.Client.Subscription.Control
{
    partial class TopSubscriptionControl
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
            this.components = new System.ComponentModel.Container();
            this.nameLabel = new System.Windows.Forms.Label();
            this.titleLabel = new System.Windows.Forms.Label();
            this.gotoStrategiesButton = new System.Windows.Forms.Button();
            this.setupButton = new System.Windows.Forms.Button();
            this.unsubscribeButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.financeLabel = new TradeSharp.UI.Util.Control.FinanceLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 8);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(46, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "(ТОП X)";
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(55, 8);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(61, 13);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "(название)";
            // 
            // gotoStrategiesButton
            // 
            this.gotoStrategiesButton.AutoSize = true;
            this.gotoStrategiesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gotoStrategiesButton.Location = new System.Drawing.Point(3, 3);
            this.gotoStrategiesButton.Name = "gotoStrategiesButton";
            this.gotoStrategiesButton.Size = new System.Drawing.Size(78, 23);
            this.gotoStrategiesButton.TabIndex = 2;
            this.gotoStrategiesButton.Tag = "TitleGoToStrategies";
            this.gotoStrategiesButton.Text = "В стратегии";
            this.gotoStrategiesButton.UseVisualStyleBackColor = true;
            // 
            // setupButton
            // 
            this.setupButton.AutoSize = true;
            this.setupButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.setupButton.Location = new System.Drawing.Point(87, 3);
            this.setupButton.Name = "setupButton";
            this.setupButton.Size = new System.Drawing.Size(130, 23);
            this.setupButton.TabIndex = 3;
            this.setupButton.Tag = "TitleSetupTradeMenu";
            this.setupButton.Text = "Настройка торговли...";
            this.setupButton.UseVisualStyleBackColor = true;
            this.setupButton.Click += new System.EventHandler(this.SetupButtonClick);
            // 
            // unsubscribeButton
            // 
            this.unsubscribeButton.AutoSize = true;
            this.unsubscribeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.unsubscribeButton.Enabled = false;
            this.unsubscribeButton.Location = new System.Drawing.Point(223, 3);
            this.unsubscribeButton.Name = "unsubscribeButton";
            this.unsubscribeButton.Size = new System.Drawing.Size(77, 23);
            this.unsubscribeButton.TabIndex = 4;
            this.unsubscribeButton.Tag = "TitleUnsubscribe";
            this.unsubscribeButton.Text = "Отписаться";
            this.unsubscribeButton.UseVisualStyleBackColor = true;
            this.unsubscribeButton.Click += new System.EventHandler(this.UnsubscribeButtonClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.titleLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.nameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.financeLabel, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(491, 29);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.gotoStrategiesButton);
            this.flowLayoutPanel2.Controls.Add(this.setupButton);
            this.flowLayoutPanel2.Controls.Add(this.unsubscribeButton);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(188, 0);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(303, 29);
            this.flowLayoutPanel2.TabIndex = 6;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // financeLabel
            // 
            this.financeLabel.Amount = 0D;
            this.financeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.financeLabel.AutoSize = true;
            this.financeLabel.ForeColor = System.Drawing.Color.Black;
            this.financeLabel.Format = "g";
            this.financeLabel.Location = new System.Drawing.Point(172, 8);
            this.financeLabel.Name = "financeLabel";
            this.financeLabel.NeedCents = false;
            this.financeLabel.Size = new System.Drawing.Size(13, 13);
            this.financeLabel.Suffix = null;
            this.financeLabel.TabIndex = 7;
            this.financeLabel.Text = "0";
            this.financeLabel.UseMoneyFormat = false;
            this.financeLabel.Visible = false;
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 0;
            this.toolTip.ShowAlways = true;
            // 
            // TopSubscriptionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TopSubscriptionControl";
            this.Size = new System.Drawing.Size(491, 29);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Button gotoStrategiesButton;
        private System.Windows.Forms.Button setupButton;
        private System.Windows.Forms.Button unsubscribeButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.ToolTip toolTip;
        private UI.Util.Control.FinanceLabel financeLabel;
    }
}
