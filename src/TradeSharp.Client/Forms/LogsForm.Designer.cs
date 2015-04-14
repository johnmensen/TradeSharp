namespace TradeSharp.Client.Forms
{
    partial class LogsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogsForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.robotsPage = new System.Windows.Forms.TabPage();
            this.tbRobotsLog = new System.Windows.Forms.TextBox();
            this.terminalPage = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.robotsPage.SuspendLayout();
            this.terminalPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 406);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(694, 43);
            this.panel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 23);
            this.button1.TabIndex = 0;
            this.button1.Tag = "TitleJournalRepository";
            this.button1.Text = "Хранилище журналов";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.robotsPage);
            this.tabControl1.Controls.Add(this.terminalPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.ShowToolTips = true;
            this.tabControl1.Size = new System.Drawing.Size(694, 406);
            this.tabControl1.TabIndex = 1;
            // 
            // robotsPage
            // 
            this.robotsPage.Controls.Add(this.tbRobotsLog);
            this.robotsPage.Location = new System.Drawing.Point(4, 22);
            this.robotsPage.Name = "robotsPage";
            this.robotsPage.Padding = new System.Windows.Forms.Padding(3);
            this.robotsPage.Size = new System.Drawing.Size(686, 380);
            this.robotsPage.TabIndex = 1;
            this.robotsPage.Tag = "TitleRobots";
            this.robotsPage.Text = "Роботы";
            this.robotsPage.UseVisualStyleBackColor = true;
            // 
            // tbRobotsLog
            // 
            this.tbRobotsLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbRobotsLog.Location = new System.Drawing.Point(3, 3);
            this.tbRobotsLog.Multiline = true;
            this.tbRobotsLog.Name = "tbRobotsLog";
            this.tbRobotsLog.ReadOnly = true;
            this.tbRobotsLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbRobotsLog.Size = new System.Drawing.Size(680, 374);
            this.tbRobotsLog.TabIndex = 0;
            // 
            // terminalPage
            // 
            this.terminalPage.Controls.Add(this.textBox1);
            this.terminalPage.Location = new System.Drawing.Point(4, 22);
            this.terminalPage.Name = "terminalPage";
            this.terminalPage.Padding = new System.Windows.Forms.Padding(3);
            this.terminalPage.Size = new System.Drawing.Size(686, 380);
            this.terminalPage.TabIndex = 0;
            this.terminalPage.Tag = "TitleTerminal";
            this.terminalPage.Text = "Терминал";
            this.terminalPage.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(680, 374);
            this.textBox1.TabIndex = 0;
            // 
            // LogsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 449);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleJournals";
            this.Text = "Журналы";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogsForm_FormClosing);
            this.Load += new System.EventHandler(this.LogsForm_Load);
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.robotsPage.ResumeLayout(false);
            this.robotsPage.PerformLayout();
            this.terminalPage.ResumeLayout(false);
            this.terminalPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage terminalPage;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage robotsPage;
        private System.Windows.Forms.TextBox tbRobotsLog;
        private System.Windows.Forms.Button button1;
    }
}