namespace TradeSharp.Client.Forms
{
    partial class DownloadQuotesForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadQuotesForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblCurrentTicker = new System.Windows.Forms.Label();
            this.btnBreak = new System.Windows.Forms.Button();
            this.panelTicker = new System.Windows.Forms.Panel();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.lblCurrentTicker);
            this.panelTop.Controls.Add(this.btnBreak);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(367, 29);
            this.panelTop.TabIndex = 0;
            // 
            // lblCurrentTicker
            // 
            this.lblCurrentTicker.AutoSize = true;
            this.lblCurrentTicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblCurrentTicker.Location = new System.Drawing.Point(98, 8);
            this.lblCurrentTicker.Name = "lblCurrentTicker";
            this.lblCurrentTicker.Size = new System.Drawing.Size(11, 13);
            this.lblCurrentTicker.TabIndex = 1;
            this.lblCurrentTicker.Text = "-";
            // 
            // btnBreak
            // 
            this.btnBreak.Location = new System.Drawing.Point(3, 3);
            this.btnBreak.Name = "btnBreak";
            this.btnBreak.Size = new System.Drawing.Size(75, 23);
            this.btnBreak.TabIndex = 0;
            this.btnBreak.Tag = "TitleCancel";
            this.btnBreak.Text = "Отменить";
            this.btnBreak.UseVisualStyleBackColor = true;
            this.btnBreak.Click += new System.EventHandler(this.BtnBreakClick);
            // 
            // panelTicker
            // 
            this.panelTicker.AutoScroll = true;
            this.panelTicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTicker.Location = new System.Drawing.Point(0, 29);
            this.panelTicker.Name = "panelTicker";
            this.panelTicker.Size = new System.Drawing.Size(367, 306);
            this.panelTicker.TabIndex = 1;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico_cancel.png");
            // 
            // DownloadQuotesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 335);
            this.Controls.Add(this.panelTicker);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(120, 60);
            this.Name = "DownloadQuotesForm";
            this.Tag = "TitleQuotesLoading";
            this.Text = "Загрузка котировок";
            this.Load += new System.EventHandler(this.DownloadQuotesFormLoad);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnBreak;
        private System.Windows.Forms.Panel panelTicker;
        private System.Windows.Forms.Label lblCurrentTicker;
        private System.Windows.Forms.ImageList imageList;
    }
}