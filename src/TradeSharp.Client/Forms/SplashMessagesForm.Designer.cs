namespace TradeSharp.Client.Forms
{
    partial class SplashMessagesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashMessagesForm));
            this.panelBtn = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnPrint = new System.Windows.Forms.Button();
            this.rbMessages = new System.Windows.Forms.RichTextBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.pnlChbx = new System.Windows.Forms.Panel();
            this.configuringNotifications = new System.Windows.Forms.LinkLabel();
            this.panelBtn.SuspendLayout();
            this.pnlChbx.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBtn
            // 
            this.panelBtn.Controls.Add(this.btnClose);
            this.panelBtn.Controls.Add(this.btnSave);
            this.panelBtn.Controls.Add(this.btnPrint);
            this.panelBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBtn.Location = new System.Drawing.Point(0, 262);
            this.panelBtn.Name = "panelBtn";
            this.panelBtn.Size = new System.Drawing.Size(377, 28);
            this.panelBtn.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnClose.Location = new System.Drawing.Point(43, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(291, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Tag = "TitleClose";
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSave.ImageIndex = 0;
            this.btnSave.ImageList = this.imageList;
            this.btnSave.Location = new System.Drawing.Point(334, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(43, 28);
            this.btnSave.TabIndex = 2;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico save.png");
            this.imageList.Images.SetKeyName(1, "icon_printer.png");
            // 
            // btnPrint
            // 
            this.btnPrint.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPrint.ImageIndex = 1;
            this.btnPrint.ImageList = this.imageList;
            this.btnPrint.Location = new System.Drawing.Point(0, 0);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(43, 28);
            this.btnPrint.TabIndex = 4;
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.BtnPrintClick);
            // 
            // rbMessages
            // 
            this.rbMessages.BackColor = System.Drawing.SystemColors.Info;
            this.rbMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbMessages.HideSelection = false;
            this.rbMessages.Location = new System.Drawing.Point(0, 0);
            this.rbMessages.Name = "rbMessages";
            this.rbMessages.ReadOnly = true;
            this.rbMessages.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rbMessages.Size = new System.Drawing.Size(377, 262);
            this.rbMessages.TabIndex = 2;
            this.rbMessages.Text = "";
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Текст (*.txt)|*.txt|Все файлы (*.*)|*.*";
            this.saveFileDialog.FilterIndex = 0;
            this.saveFileDialog.Title = "Сохранить уведомления";
            // 
            // pnlChbx
            // 
            this.pnlChbx.Controls.Add(this.configuringNotifications);
            this.pnlChbx.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlChbx.Location = new System.Drawing.Point(0, 290);
            this.pnlChbx.Name = "pnlChbx";
            this.pnlChbx.Size = new System.Drawing.Size(377, 28);
            this.pnlChbx.TabIndex = 3;
            // 
            // configuringNotifications
            // 
            this.configuringNotifications.ActiveLinkColor = System.Drawing.Color.Blue;
            this.configuringNotifications.AutoSize = true;
            this.configuringNotifications.Location = new System.Drawing.Point(4, 7);
            this.configuringNotifications.Name = "configuringNotifications";
            this.configuringNotifications.Size = new System.Drawing.Size(301, 13);
            this.configuringNotifications.TabIndex = 0;
            this.configuringNotifications.TabStop = true;
            this.configuringNotifications.Tag = "TitleServerNotificationSettings";
            this.configuringNotifications.Text = "Настройка отображения уведомлений торгового сервера";
            this.configuringNotifications.Click += new System.EventHandler(this.ConfiguringNotificationsClick);
            // 
            // SplashMessagesForm
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(377, 318);
            this.Controls.Add(this.rbMessages);
            this.Controls.Add(this.panelBtn);
            this.Controls.Add(this.pnlChbx);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashMessagesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleTradeServerNotifications";
            this.Text = "Уведомления торгового сервера";
            this.panelBtn.ResumeLayout(false);
            this.pnlChbx.ResumeLayout(false);
            this.pnlChbx.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBtn;
        private System.Windows.Forms.RichTextBox rbMessages;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Panel pnlChbx;
        private System.Windows.Forms.LinkLabel configuringNotifications;
    }
}