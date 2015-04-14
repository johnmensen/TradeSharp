namespace TradeSharp.Reports.ManagerApp
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.btnSendToServer = new System.Windows.Forms.Button();
            this.btnMakeMonthly = new System.Windows.Forms.Button();
            this.tabPageSets = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.tbServerSide = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbBenchmarks = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbAccountId = new System.Windows.Forms.TextBox();
            this.btnDeclineSettings = new System.Windows.Forms.Button();
            this.btnAcceptSettings = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowseTempFolder = new System.Windows.Forms.Button();
            this.tbFolderTemp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBrowseDestFolder = new System.Windows.Forms.Button();
            this.tbFolderDest = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBrowseTemplateFolder = new System.Windows.Forms.Button();
            this.tbFolderTemplate = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageSets.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageMain);
            this.tabControl.Controls.Add(this.tabPageSets);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(457, 304);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.btnSendToServer);
            this.tabPageMain.Controls.Add(this.btnMakeMonthly);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(449, 278);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Отчеты";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // btnSendToServer
            // 
            this.btnSendToServer.Location = new System.Drawing.Point(0, 35);
            this.btnSendToServer.Name = "btnSendToServer";
            this.btnSendToServer.Size = new System.Drawing.Size(191, 23);
            this.btnSendToServer.TabIndex = 1;
            this.btnSendToServer.Text = "Отправить на сервер";
            this.btnSendToServer.UseVisualStyleBackColor = true;
            this.btnSendToServer.Click += new System.EventHandler(this.BtnSendToServerClick);
            // 
            // btnMakeMonthly
            // 
            this.btnMakeMonthly.Location = new System.Drawing.Point(0, 6);
            this.btnMakeMonthly.Name = "btnMakeMonthly";
            this.btnMakeMonthly.Size = new System.Drawing.Size(191, 23);
            this.btnMakeMonthly.TabIndex = 0;
            this.btnMakeMonthly.Text = "Сформировать ежемес.";
            this.btnMakeMonthly.UseVisualStyleBackColor = true;
            this.btnMakeMonthly.Click += new System.EventHandler(this.btnMakeMonthly_Click);
            // 
            // tabPageSets
            // 
            this.tabPageSets.Controls.Add(this.label6);
            this.tabPageSets.Controls.Add(this.tbServerSide);
            this.tabPageSets.Controls.Add(this.label5);
            this.tabPageSets.Controls.Add(this.tbBenchmarks);
            this.tabPageSets.Controls.Add(this.label4);
            this.tabPageSets.Controls.Add(this.tbAccountId);
            this.tabPageSets.Controls.Add(this.btnDeclineSettings);
            this.tabPageSets.Controls.Add(this.btnAcceptSettings);
            this.tabPageSets.Controls.Add(this.label3);
            this.tabPageSets.Controls.Add(this.btnBrowseTempFolder);
            this.tabPageSets.Controls.Add(this.tbFolderTemp);
            this.tabPageSets.Controls.Add(this.label2);
            this.tabPageSets.Controls.Add(this.btnBrowseDestFolder);
            this.tabPageSets.Controls.Add(this.tbFolderDest);
            this.tabPageSets.Controls.Add(this.label1);
            this.tabPageSets.Controls.Add(this.btnBrowseTemplateFolder);
            this.tabPageSets.Controls.Add(this.tbFolderTemplate);
            this.tabPageSets.Location = new System.Drawing.Point(4, 22);
            this.tabPageSets.Name = "tabPageSets";
            this.tabPageSets.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSets.Size = new System.Drawing.Size(449, 278);
            this.tabPageSets.TabIndex = 1;
            this.tabPageSets.Text = "Настройки";
            this.tabPageSets.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 124);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(133, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Размещение на сервере";
            // 
            // tbServerSide
            // 
            this.tbServerSide.Location = new System.Drawing.Point(8, 140);
            this.tbServerSide.Name = "tbServerSide";
            this.tbServerSide.Size = new System.Drawing.Size(346, 20);
            this.tbServerSide.TabIndex = 18;
            this.tbServerSide.Text = "\\\\ferrari\\c$\\WEBSite\\FXI.Web.Base\\Download\\FundPerformance";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(116, 173);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Индексы (через \',\')";
            // 
            // tbBenchmarks
            // 
            this.tbBenchmarks.Location = new System.Drawing.Point(116, 188);
            this.tbBenchmarks.Name = "tbBenchmarks";
            this.tbBenchmarks.Size = new System.Drawing.Size(101, 20);
            this.tbBenchmarks.TabIndex = 16;
            this.tbBenchmarks.Text = "DJCTA,IASG";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 173);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Счет";
            // 
            // tbAccountId
            // 
            this.tbAccountId.Location = new System.Drawing.Point(8, 188);
            this.tbAccountId.Name = "tbAccountId";
            this.tbAccountId.Size = new System.Drawing.Size(73, 20);
            this.tbAccountId.TabIndex = 14;
            // 
            // btnDeclineSettings
            // 
            this.btnDeclineSettings.Location = new System.Drawing.Point(167, 249);
            this.btnDeclineSettings.Name = "btnDeclineSettings";
            this.btnDeclineSettings.Size = new System.Drawing.Size(125, 23);
            this.btnDeclineSettings.TabIndex = 13;
            this.btnDeclineSettings.Text = "Отменить изменения";
            this.btnDeclineSettings.UseVisualStyleBackColor = true;
            this.btnDeclineSettings.Click += new System.EventHandler(this.BtnDeclineSettingsClick);
            // 
            // btnAcceptSettings
            // 
            this.btnAcceptSettings.Location = new System.Drawing.Point(6, 249);
            this.btnAcceptSettings.Name = "btnAcceptSettings";
            this.btnAcceptSettings.Size = new System.Drawing.Size(124, 23);
            this.btnAcceptSettings.TabIndex = 12;
            this.btnAcceptSettings.Text = "Принять настройки";
            this.btnAcceptSettings.UseVisualStyleBackColor = true;
            this.btnAcceptSettings.Click += new System.EventHandler(this.BtnAcceptSettingsClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Временный каталог";
            // 
            // btnBrowseTempFolder
            // 
            this.btnBrowseTempFolder.Location = new System.Drawing.Point(263, 95);
            this.btnBrowseTempFolder.Name = "btnBrowseTempFolder";
            this.btnBrowseTempFolder.Size = new System.Drawing.Size(29, 23);
            this.btnBrowseTempFolder.TabIndex = 7;
            this.btnBrowseTempFolder.Text = "...";
            this.btnBrowseTempFolder.UseVisualStyleBackColor = true;
            this.btnBrowseTempFolder.Click += new System.EventHandler(this.BtnBrowseTemplateFolderClick);
            // 
            // tbFolderTemp
            // 
            this.tbFolderTemp.Location = new System.Drawing.Point(8, 97);
            this.tbFolderTemp.Name = "tbFolderTemp";
            this.tbFolderTemp.Size = new System.Drawing.Size(249, 20);
            this.tbFolderTemp.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Каталог назначения";
            // 
            // btnBrowseDestFolder
            // 
            this.btnBrowseDestFolder.Location = new System.Drawing.Point(263, 56);
            this.btnBrowseDestFolder.Name = "btnBrowseDestFolder";
            this.btnBrowseDestFolder.Size = new System.Drawing.Size(29, 23);
            this.btnBrowseDestFolder.TabIndex = 4;
            this.btnBrowseDestFolder.Text = "...";
            this.btnBrowseDestFolder.UseVisualStyleBackColor = true;
            this.btnBrowseDestFolder.Click += new System.EventHandler(this.BtnBrowseTemplateFolderClick);
            // 
            // tbFolderDest
            // 
            this.tbFolderDest.Location = new System.Drawing.Point(8, 58);
            this.tbFolderDest.Name = "tbFolderDest";
            this.tbFolderDest.Size = new System.Drawing.Size(249, 20);
            this.tbFolderDest.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Каталог шаблонов";
            // 
            // btnBrowseTemplateFolder
            // 
            this.btnBrowseTemplateFolder.Location = new System.Drawing.Point(263, 17);
            this.btnBrowseTemplateFolder.Name = "btnBrowseTemplateFolder";
            this.btnBrowseTemplateFolder.Size = new System.Drawing.Size(29, 23);
            this.btnBrowseTemplateFolder.TabIndex = 1;
            this.btnBrowseTemplateFolder.Text = "...";
            this.btnBrowseTemplateFolder.UseVisualStyleBackColor = true;
            this.btnBrowseTemplateFolder.Click += new System.EventHandler(this.BtnBrowseTemplateFolderClick);
            // 
            // tbFolderTemplate
            // 
            this.tbFolderTemplate.Location = new System.Drawing.Point(8, 19);
            this.tbFolderTemplate.Name = "tbFolderTemplate";
            this.tbFolderTemplate.Size = new System.Drawing.Size(249, 20);
            this.tbFolderTemplate.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 304);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.Text = "Управление отчетами";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.tabControl.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageSets.ResumeLayout(false);
            this.tabPageSets.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageSets;
        private System.Windows.Forms.Button btnDeclineSettings;
        private System.Windows.Forms.Button btnAcceptSettings;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowseTempFolder;
        private System.Windows.Forms.TextBox tbFolderTemp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowseDestFolder;
        private System.Windows.Forms.TextBox tbFolderDest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowseTemplateFolder;
        private System.Windows.Forms.TextBox tbFolderTemplate;
        private System.Windows.Forms.Button btnMakeMonthly;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbAccountId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbBenchmarks;
        private System.Windows.Forms.Button btnSendToServer;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbServerSide;
    }
}

