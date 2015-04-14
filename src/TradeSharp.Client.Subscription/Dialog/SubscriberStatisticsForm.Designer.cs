using TradeSharp.Client.Subscription.Control;

namespace TradeSharp.Client.Subscription.Dialog
{
    partial class SubscriberStatisticsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubscriberStatisticsForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.closeButton = new System.Windows.Forms.Button();
            this.standByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.performerStatistic = new TradeSharp.Client.Subscription.Control.PerformerStatistic();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.closeButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 641);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(786, 29);
            this.flowLayoutPanel1.TabIndex = 63;
            // 
            // closeButton
            // 
            this.closeButton.AutoSize = true;
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(708, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Tag = "TitleClose";
            this.closeButton.Text = "Закрыть";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // standByControl
            // 
            this.standByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.standByControl.IsShown = true;
            this.standByControl.Location = new System.Drawing.Point(3, 3);
            this.standByControl.Name = "standByControl";
            this.standByControl.Size = new System.Drawing.Size(786, 638);
            this.standByControl.TabIndex = 67;
            this.standByControl.Tag = "TitleLoading";
            this.standByControl.Text = "Загрузка...";
            this.standByControl.TransparentForm = null;
            // 
            // performerStatistic
            // 
            this.performerStatistic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.performerStatistic.Location = new System.Drawing.Point(3, 3);
            this.performerStatistic.Name = "performerStatistic";
            this.performerStatistic.Size = new System.Drawing.Size(786, 638);
            this.performerStatistic.TabIndex = 68;
            this.performerStatistic.Visible = false;
            // 
            // SubscriberStatisticsForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 673);
            this.Controls.Add(this.standByControl);
            this.Controls.Add(this.performerStatistic);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "SubscriberStatisticsForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Tag = "TitleSignalStatistics";
            this.Text = "Статистика по сигналу";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button closeButton;
        private UI.Util.Control.StandByControl standByControl;
        private PerformerStatistic performerStatistic;
    }
}