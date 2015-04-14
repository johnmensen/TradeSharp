namespace TradeSharp.Client.Forms
{
    partial class RoboTesterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RoboTesterForm));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveRobotsPortfolioDlg = new System.Windows.Forms.SaveFileDialog();
            this.openRobotsPortfolioDlg = new System.Windows.Forms.OpenFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.cbUpdateQuotes = new System.Windows.Forms.CheckBox();
            this.cbLogTrace = new System.Windows.Forms.CheckBox();
            this.dtDateTo = new System.Windows.Forms.DateTimePicker();
            this.dtDateFrom = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbUseSelectedDate = new System.Windows.Forms.CheckBox();
            this.btnShowDealPointsForm = new System.Windows.Forms.Button();
            this.btnShowRobotMarkers = new System.Windows.Forms.Button();
            this.btnResult = new System.Windows.Forms.Button();
            this.btnAccountSettings = new System.Windows.Forms.Button();
            this.btnSetupTestGroup = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.robotPortfolioControl = new TradeSharp.Client.Controls.RobotPortfolioSetupControl();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveRobotsPortfolioDlg
            // 
            this.saveRobotsPortfolioDlg.DefaultExt = "*.pxml";
            this.saveRobotsPortfolioDlg.Filter = "Robots portfolio|*.pxml|All files|*.*";
            this.saveRobotsPortfolioDlg.Title = "Портфель роботов";
            // 
            // openRobotsPortfolioDlg
            // 
            this.openRobotsPortfolioDlg.DefaultExt = "*.pxml";
            this.openRobotsPortfolioDlg.Filter = "Robots portfolio|*.pxml|All files|*.*";
            this.openRobotsPortfolioDlg.Title = "Портфель роботов";
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 292);
            this.progressBar.Maximum = 500;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(728, 23);
            this.progressBar.TabIndex = 36;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.cbUpdateQuotes);
            this.panelBottom.Controls.Add(this.cbLogTrace);
            this.panelBottom.Controls.Add(this.dtDateTo);
            this.panelBottom.Controls.Add(this.dtDateFrom);
            this.panelBottom.Controls.Add(this.label4);
            this.panelBottom.Controls.Add(this.label3);
            this.panelBottom.Controls.Add(this.cbUseSelectedDate);
            this.panelBottom.Controls.Add(this.btnShowDealPointsForm);
            this.panelBottom.Controls.Add(this.btnShowRobotMarkers);
            this.panelBottom.Controls.Add(this.btnResult);
            this.panelBottom.Controls.Add(this.btnAccountSettings);
            this.panelBottom.Controls.Add(this.btnSetupTestGroup);
            this.panelBottom.Controls.Add(this.btnStart);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 184);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(728, 108);
            this.panelBottom.TabIndex = 38;
            // 
            // cbUpdateQuotes
            // 
            this.cbUpdateQuotes.AutoSize = true;
            this.cbUpdateQuotes.Checked = true;
            this.cbUpdateQuotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUpdateQuotes.Location = new System.Drawing.Point(3, 88);
            this.cbUpdateQuotes.Name = "cbUpdateQuotes";
            this.cbUpdateQuotes.Size = new System.Drawing.Size(137, 17);
            this.cbUpdateQuotes.TabIndex = 45;
            this.cbUpdateQuotes.Tag = "TitleUpdateQuotes";
            this.cbUpdateQuotes.Text = "Обновлять котировки";
            this.cbUpdateQuotes.UseVisualStyleBackColor = true;
            // 
            // cbLogTrace
            // 
            this.cbLogTrace.AutoSize = true;
            this.cbLogTrace.Location = new System.Drawing.Point(3, 72);
            this.cbLogTrace.Name = "cbLogTrace";
            this.cbLogTrace.Size = new System.Drawing.Size(99, 17);
            this.cbLogTrace.TabIndex = 44;
            this.cbLogTrace.Tag = "TitleSaveLog";
            this.cbLogTrace.Text = "Сохранять лог";
            this.cbLogTrace.UseVisualStyleBackColor = true;
            // 
            // dtDateTo
            // 
            this.dtDateTo.Location = new System.Drawing.Point(27, 46);
            this.dtDateTo.Name = "dtDateTo";
            this.dtDateTo.Size = new System.Drawing.Size(123, 20);
            this.dtDateTo.TabIndex = 43;
            // 
            // dtDateFrom
            // 
            this.dtDateFrom.Location = new System.Drawing.Point(27, 22);
            this.dtDateFrom.Name = "dtDateFrom";
            this.dtDateFrom.Size = new System.Drawing.Size(123, 20);
            this.dtDateFrom.TabIndex = 42;
            this.dtDateFrom.Value = new System.DateTime(2009, 1, 5, 0, 0, 0, 0);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 13);
            this.label4.TabIndex = 41;
            this.label4.Tag = "TitleToSmall";
            this.label4.Text = "до:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 40;
            this.label3.Tag = "TitleFromSmall";
            this.label3.Text = "от";
            // 
            // cbUseSelectedDate
            // 
            this.cbUseSelectedDate.AutoSize = true;
            this.cbUseSelectedDate.Location = new System.Drawing.Point(3, 3);
            this.cbUseSelectedDate.Name = "cbUseSelectedDate";
            this.cbUseSelectedDate.Size = new System.Drawing.Size(197, 17);
            this.cbUseSelectedDate.TabIndex = 39;
            this.cbUseSelectedDate.Tag = "TitleUseTestDate";
            this.cbUseSelectedDate.Text = "Использовать дату тестирования";
            this.cbUseSelectedDate.UseVisualStyleBackColor = true;
            // 
            // btnShowDealPointsForm
            // 
            this.btnShowDealPointsForm.Location = new System.Drawing.Point(369, 61);
            this.btnShowDealPointsForm.Name = "btnShowDealPointsForm";
            this.btnShowDealPointsForm.Size = new System.Drawing.Size(160, 23);
            this.btnShowDealPointsForm.TabIndex = 38;
            this.btnShowDealPointsForm.Tag = "TitleDealsChartMenu";
            this.btnShowDealPointsForm.Text = "График сделок...";
            this.btnShowDealPointsForm.UseVisualStyleBackColor = true;
            this.btnShowDealPointsForm.Click += new System.EventHandler(this.BtnShowDealPointsFormClick);
            // 
            // btnShowRobotMarkers
            // 
            this.btnShowRobotMarkers.Location = new System.Drawing.Point(369, 32);
            this.btnShowRobotMarkers.Name = "btnShowRobotMarkers";
            this.btnShowRobotMarkers.Size = new System.Drawing.Size(160, 23);
            this.btnShowRobotMarkers.TabIndex = 35;
            this.btnShowRobotMarkers.Tag = "TitleShowOnChart";
            this.btnShowRobotMarkers.Text = "Показать на графике";
            this.btnShowRobotMarkers.UseVisualStyleBackColor = true;
            this.btnShowRobotMarkers.Click += new System.EventHandler(this.BtnShowRobotMarkersClick);
            // 
            // btnResult
            // 
            this.btnResult.Location = new System.Drawing.Point(369, 3);
            this.btnResult.Name = "btnResult";
            this.btnResult.Size = new System.Drawing.Size(160, 23);
            this.btnResult.TabIndex = 34;
            this.btnResult.Tag = "TitleTestResults";
            this.btnResult.Text = "Результаты тестирования";
            this.btnResult.UseVisualStyleBackColor = true;
            this.btnResult.Click += new System.EventHandler(this.BtnResultClick);
            // 
            // btnAccountSettings
            // 
            this.btnAccountSettings.Location = new System.Drawing.Point(203, 3);
            this.btnAccountSettings.Name = "btnAccountSettings";
            this.btnAccountSettings.Size = new System.Drawing.Size(160, 23);
            this.btnAccountSettings.TabIndex = 33;
            this.btnAccountSettings.Tag = "TitleAccountSettings";
            this.btnAccountSettings.Text = "Настройки счета";
            this.btnAccountSettings.UseVisualStyleBackColor = true;
            this.btnAccountSettings.Click += new System.EventHandler(this.BtnAccountSettingsClick);
            // 
            // btnSetupTestGroup
            // 
            this.btnSetupTestGroup.Location = new System.Drawing.Point(203, 32);
            this.btnSetupTestGroup.Name = "btnSetupTestGroup";
            this.btnSetupTestGroup.Size = new System.Drawing.Size(160, 23);
            this.btnSetupTestGroup.TabIndex = 32;
            this.btnSetupTestGroup.Tag = "TitleTestGroupSettings";
            this.btnSetupTestGroup.Text = "Настройки тестовой группы";
            this.btnSetupTestGroup.UseVisualStyleBackColor = true;
            this.btnSetupTestGroup.Click += new System.EventHandler(this.BtnSetupTestGroupClick);
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnStart.Location = new System.Drawing.Point(203, 61);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(160, 23);
            this.btnStart.TabIndex = 17;
            this.btnStart.Tag = "TitleLAUNCH";
            this.btnStart.Text = "ПУСК";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // robotPortfolioControl
            // 
            this.robotPortfolioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.robotPortfolioControl.LivePortfolioMode = false;
            this.robotPortfolioControl.Location = new System.Drawing.Point(0, 0);
            this.robotPortfolioControl.Name = "robotPortfolioControl";
            this.robotPortfolioControl.Size = new System.Drawing.Size(728, 184);
            this.robotPortfolioControl.TabIndex = 39;
            // 
            // RoboTesterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 315);
            this.Controls.Add(this.robotPortfolioControl);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RoboTesterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleRobotPortfolioTesting";
            this.Text = "Тестирование портфеля роботов";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RoboTesterFormFormClosing);
            this.Load += new System.EventHandler(this.RoboTesterFormLoad);
            this.ResizeEnd += new System.EventHandler(this.RoboTesterFormResizeEnd);
            this.Move += new System.EventHandler(this.RoboTesterFormMove);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveRobotsPortfolioDlg;
        private System.Windows.Forms.OpenFileDialog openRobotsPortfolioDlg;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.CheckBox cbUpdateQuotes;
        private System.Windows.Forms.CheckBox cbLogTrace;
        private System.Windows.Forms.DateTimePicker dtDateTo;
        private System.Windows.Forms.DateTimePicker dtDateFrom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbUseSelectedDate;
        private System.Windows.Forms.Button btnShowDealPointsForm;
        private System.Windows.Forms.Button btnShowRobotMarkers;
        private System.Windows.Forms.Button btnResult;
        private System.Windows.Forms.Button btnAccountSettings;
        private System.Windows.Forms.Button btnSetupTestGroup;
        private System.Windows.Forms.Button btnStart;
        private Controls.RobotPortfolioSetupControl robotPortfolioControl;
    }
}