namespace TestMTTradeServer
{
    partial class MainForm
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
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.адресToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.теToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.memo = new DevExpress.XtraEditors.MemoEdit();
            this.tbShiftPP = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbShiftPP.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.tbShiftPP);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 24);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(517, 30);
            this.panelControl1.TabIndex = 0;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.настройкиToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(517, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.адресToolStripMenuItem,
            this.теToolStripMenuItem});
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            // 
            // адресToolStripMenuItem
            // 
            this.адресToolStripMenuItem.Name = "адресToolStripMenuItem";
            this.адресToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.адресToolStripMenuItem.Text = "Адрес";
            this.адресToolStripMenuItem.Click += new System.EventHandler(this.адресToolStripMenuItem_Click);
            // 
            // теToolStripMenuItem
            // 
            this.теToolStripMenuItem.Name = "теToolStripMenuItem";
            this.теToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.теToolStripMenuItem.Text = "Тест доставки";
            this.теToolStripMenuItem.Click += new System.EventHandler(this.теToolStripMenuItem_Click);
            // 
            // memo
            // 
            this.memo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memo.EditValue = "";
            this.memo.Location = new System.Drawing.Point(0, 54);
            this.memo.Name = "memo";
            this.memo.Properties.Appearance.BackColor = System.Drawing.Color.Black;
            this.memo.Properties.Appearance.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.memo.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.memo.Properties.Appearance.Options.UseBackColor = true;
            this.memo.Properties.Appearance.Options.UseFont = true;
            this.memo.Properties.Appearance.Options.UseForeColor = true;
            this.memo.Size = new System.Drawing.Size(517, 346);
            this.memo.TabIndex = 2;
            // 
            // tbShiftPP
            // 
            this.tbShiftPP.EditValue = "0";
            this.tbShiftPP.Location = new System.Drawing.Point(5, 5);
            this.tbShiftPP.Name = "tbShiftPP";
            this.tbShiftPP.Size = new System.Drawing.Size(63, 20);
            this.tbShiftPP.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(75, 8);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(89, 13);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "смещение пп, абс";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 400);
            this.Controls.Add(this.memo);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "Тестовый торговый сервер";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbShiftPP.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem адресToolStripMenuItem;
        private DevExpress.XtraEditors.MemoEdit memo;
        private System.Windows.Forms.ToolStripMenuItem теToolStripMenuItem;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit tbShiftPP;
    }
}

