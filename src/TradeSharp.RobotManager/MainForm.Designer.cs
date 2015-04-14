namespace TradeSharp.RobotManager
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuTrayRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRedirectToSite = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTrayQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTrayCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnServiceStart = new System.Windows.Forms.Button();
            this.btnServiseStop = new System.Windows.Forms.Button();
            this.txtServiceStatys = new System.Windows.Forms.Label();
            this.contextMenuTray.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipTitle = "TRADE#";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "TRADE#";
            this.notifyIcon.Visible = true;
            this.notifyIcon.BalloonTipClicked += new System.EventHandler(this.NotifyIconBalloonTipClicked);
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMouseDoubleClick);
            this.notifyIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.NotifyIconMouseUp);
            // 
            // menuTrayRestore
            // 
            this.menuTrayRestore.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.menuTrayRestore.Name = "menuTrayRestore";
            this.menuTrayRestore.Size = new System.Drawing.Size(164, 22);
            this.menuTrayRestore.Text = "Развернуть";
            this.menuTrayRestore.Click += new System.EventHandler(this.MenuTrayRestoreClick);
            // 
            // menuRedirectToSite
            // 
            this.menuRedirectToSite.Name = "menuRedirectToSite";
            this.menuRedirectToSite.Size = new System.Drawing.Size(164, 22);
            this.menuRedirectToSite.Text = "Перейти на сайт";
            this.menuRedirectToSite.Click += new System.EventHandler(this.MenuRedirectToSiteClick);
            // 
            // menuTrayQuit
            // 
            this.menuTrayQuit.Name = "menuTrayQuit";
            this.menuTrayQuit.Size = new System.Drawing.Size(164, 22);
            this.menuTrayQuit.Text = "Выход";
            this.menuTrayQuit.Click += new System.EventHandler(this.MenuTrayQuitClick);
            // 
            // menuTrayCancel
            // 
            this.menuTrayCancel.Name = "menuTrayCancel";
            this.menuTrayCancel.Size = new System.Drawing.Size(164, 22);
            this.menuTrayCancel.Text = "Отмена";
            this.menuTrayCancel.Click += new System.EventHandler(this.MenuTrayCancelClick);
            // 
            // contextMenuTray
            // 
            this.contextMenuTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuTrayRestore,
            this.menuRedirectToSite,
            this.menuTrayQuit,
            this.menuTrayCancel});
            this.contextMenuTray.Name = "contextMenuTray";
            this.contextMenuTray.Size = new System.Drawing.Size(165, 92);
            // 
            // btnServiceStart
            // 
            this.btnServiceStart.Location = new System.Drawing.Point(12, 72);
            this.btnServiceStart.Name = "btnServiceStart";
            this.btnServiceStart.Size = new System.Drawing.Size(107, 23);
            this.btnServiceStart.TabIndex = 1;
            this.btnServiceStart.Text = "Запустить";
            this.btnServiceStart.UseVisualStyleBackColor = true;
            this.btnServiceStart.Click += new System.EventHandler(this.BtnServiceStartClick);
            // 
            // btnServiseStop
            // 
            this.btnServiseStop.Location = new System.Drawing.Point(12, 102);
            this.btnServiseStop.Name = "btnServiseStop";
            this.btnServiseStop.Size = new System.Drawing.Size(107, 23);
            this.btnServiseStop.TabIndex = 2;
            this.btnServiseStop.Text = "Остановить";
            this.btnServiseStop.UseVisualStyleBackColor = true;
            this.btnServiseStop.Click += new System.EventHandler(this.BtnServiseStopClick);
            // 
            // txtServiceStatys
            // 
            this.txtServiceStatys.AutoSize = true;
            this.txtServiceStatys.Location = new System.Drawing.Point(13, 13);
            this.txtServiceStatys.Name = "txtServiceStatys";
            this.txtServiceStatys.Size = new System.Drawing.Size(0, 13);
            this.txtServiceStatys.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 252);
            this.Controls.Add(this.txtServiceStatys);
            this.Controls.Add(this.btnServiseStop);
            this.Controls.Add(this.btnServiceStart);
            this.Name = "MainForm";
            this.Text = "менеджер торговых роботов";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
            this.Resize += new System.EventHandler(this.Form1Resize);
            this.contextMenuTray.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ToolStripMenuItem menuRedirectToSite;
        private System.Windows.Forms.ToolStripMenuItem menuTrayRestore;
        private System.Windows.Forms.ToolStripMenuItem menuTrayQuit;
        private System.Windows.Forms.ToolStripMenuItem menuTrayCancel;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuTray;
        #endregion
        private System.Windows.Forms.Button btnServiceStart;
        private System.Windows.Forms.Button btnServiseStop;
        private System.Windows.Forms.Label txtServiceStatys;
    }
}

