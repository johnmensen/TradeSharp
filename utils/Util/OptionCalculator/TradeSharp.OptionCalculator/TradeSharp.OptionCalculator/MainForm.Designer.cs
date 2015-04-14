namespace TradeSharp.OptionCalculator
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageCalc = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.cbOptionType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbEnterPrice = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbStrikePrice = new System.Windows.Forms.TextBox();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btnPickQuoteFile = new System.Windows.Forms.Button();
            this.tbQuoteFileName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.dpTradeTime = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dpExpireTime = new System.Windows.Forms.DateTimePicker();
            this.cbRemoveTrend = new System.Windows.Forms.CheckBox();
            this.tabPageSets = new System.Windows.Forms.TabPage();
            this.btnSetQuoteFolder = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbQuoteFolder = new System.Windows.Forms.TextBox();
            this.btnDiscardSettings = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.tbQuoteTimezoneShift = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbLog = new System.Windows.Forms.RichTextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tbHighStepPercent = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbIterations = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageCalc.SuspendLayout();
            this.tabPageSets.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.tabControl);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(607, 219);
            this.panelTop.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageCalc);
            this.tabControl.Controls.Add(this.tabPageSets);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(607, 219);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageCalc
            // 
            this.tabPageCalc.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageCalc.Controls.Add(this.label8);
            this.tabPageCalc.Controls.Add(this.cbOptionType);
            this.tabPageCalc.Controls.Add(this.label7);
            this.tabPageCalc.Controls.Add(this.tbEnterPrice);
            this.tabPageCalc.Controls.Add(this.label6);
            this.tabPageCalc.Controls.Add(this.tbStrikePrice);
            this.tabPageCalc.Controls.Add(this.btnCalculate);
            this.tabPageCalc.Controls.Add(this.label5);
            this.tabPageCalc.Controls.Add(this.btnPickQuoteFile);
            this.tabPageCalc.Controls.Add(this.tbQuoteFileName);
            this.tabPageCalc.Controls.Add(this.label4);
            this.tabPageCalc.Controls.Add(this.dpTradeTime);
            this.tabPageCalc.Controls.Add(this.label3);
            this.tabPageCalc.Controls.Add(this.dpExpireTime);
            this.tabPageCalc.Controls.Add(this.cbRemoveTrend);
            this.tabPageCalc.Location = new System.Drawing.Point(4, 22);
            this.tabPageCalc.Name = "tabPageCalc";
            this.tabPageCalc.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCalc.Size = new System.Drawing.Size(599, 193);
            this.tabPageCalc.TabIndex = 0;
            this.tabPageCalc.Tag = "``";
            this.tabPageCalc.Text = "Расчет";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(235, 108);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Тип опциона";
            // 
            // cbOptionType
            // 
            this.cbOptionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOptionType.FormattingEnabled = true;
            this.cbOptionType.Items.AddRange(new object[] {
            "EU-CALL",
            "EU-PUT",
            "AM-CALL",
            "AM-PUT"});
            this.cbOptionType.Location = new System.Drawing.Point(235, 123);
            this.cbOptionType.Name = "cbOptionType";
            this.cbOptionType.Size = new System.Drawing.Size(121, 21);
            this.cbOptionType.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(102, 108);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "цена на момент сделки";
            // 
            // tbEnterPrice
            // 
            this.tbEnterPrice.Location = new System.Drawing.Point(102, 124);
            this.tbEnterPrice.Name = "tbEnterPrice";
            this.tbEnterPrice.Size = new System.Drawing.Size(127, 20);
            this.tbEnterPrice.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 108);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "страйк";
            // 
            // tbStrikePrice
            // 
            this.tbStrikePrice.Location = new System.Drawing.Point(8, 124);
            this.tbStrikePrice.Name = "tbStrikePrice";
            this.tbStrikePrice.Size = new System.Drawing.Size(88, 20);
            this.tbStrikePrice.TabIndex = 10;
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(8, 163);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(148, 23);
            this.btnCalculate.TabIndex = 9;
            this.btnCalculate.Text = "Рассчитать";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "котировка";
            // 
            // btnPickQuoteFile
            // 
            this.btnPickQuoteFile.Location = new System.Drawing.Point(280, 61);
            this.btnPickQuoteFile.Name = "btnPickQuoteFile";
            this.btnPickQuoteFile.Size = new System.Drawing.Size(30, 23);
            this.btnPickQuoteFile.TabIndex = 7;
            this.btnPickQuoteFile.Text = "...";
            this.btnPickQuoteFile.UseVisualStyleBackColor = true;
            this.btnPickQuoteFile.Click += new System.EventHandler(this.btnPickQuoteFile_Click);
            // 
            // tbQuoteFileName
            // 
            this.tbQuoteFileName.Location = new System.Drawing.Point(8, 63);
            this.tbQuoteFileName.Name = "tbQuoteFileName";
            this.tbQuoteFileName.Size = new System.Drawing.Size(266, 20);
            this.tbQuoteFileName.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "сделка";
            // 
            // dpTradeTime
            // 
            this.dpTradeTime.CustomFormat = "dd MMM yyyy HH:mm";
            this.dpTradeTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpTradeTime.Location = new System.Drawing.Point(8, 24);
            this.dpTradeTime.Name = "dpTradeTime";
            this.dpTradeTime.Size = new System.Drawing.Size(148, 20);
            this.dpTradeTime.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(162, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "экспирация";
            // 
            // dpExpireTime
            // 
            this.dpExpireTime.CustomFormat = "dd MMM yyyy HH:mm";
            this.dpExpireTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpExpireTime.Location = new System.Drawing.Point(162, 24);
            this.dpExpireTime.Name = "dpExpireTime";
            this.dpExpireTime.Size = new System.Drawing.Size(148, 20);
            this.dpExpireTime.TabIndex = 2;
            // 
            // cbRemoveTrend
            // 
            this.cbRemoveTrend.AutoSize = true;
            this.cbRemoveTrend.Checked = true;
            this.cbRemoveTrend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRemoveTrend.Location = new System.Drawing.Point(8, 89);
            this.cbRemoveTrend.Name = "cbRemoveTrend";
            this.cbRemoveTrend.Size = new System.Drawing.Size(112, 17);
            this.cbRemoveTrend.TabIndex = 1;
            this.cbRemoveTrend.Text = "исключить тренд";
            this.cbRemoveTrend.UseVisualStyleBackColor = true;
            // 
            // tabPageSets
            // 
            this.tabPageSets.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageSets.Controls.Add(this.label10);
            this.tabPageSets.Controls.Add(this.tbIterations);
            this.tabPageSets.Controls.Add(this.tbHighStepPercent);
            this.tabPageSets.Controls.Add(this.label9);
            this.tabPageSets.Controls.Add(this.btnSetQuoteFolder);
            this.tabPageSets.Controls.Add(this.label2);
            this.tabPageSets.Controls.Add(this.tbQuoteFolder);
            this.tabPageSets.Controls.Add(this.btnDiscardSettings);
            this.tabPageSets.Controls.Add(this.btnSaveSettings);
            this.tabPageSets.Controls.Add(this.tbQuoteTimezoneShift);
            this.tabPageSets.Controls.Add(this.label1);
            this.tabPageSets.Location = new System.Drawing.Point(4, 22);
            this.tabPageSets.Name = "tabPageSets";
            this.tabPageSets.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSets.Size = new System.Drawing.Size(599, 193);
            this.tabPageSets.TabIndex = 1;
            this.tabPageSets.Text = "Настройки";
            // 
            // btnSetQuoteFolder
            // 
            this.btnSetQuoteFolder.Location = new System.Drawing.Point(323, 63);
            this.btnSetQuoteFolder.Name = "btnSetQuoteFolder";
            this.btnSetQuoteFolder.Size = new System.Drawing.Size(31, 23);
            this.btnSetQuoteFolder.TabIndex = 9;
            this.btnSetQuoteFolder.Text = "...";
            this.btnSetQuoteFolder.UseVisualStyleBackColor = true;
            this.btnSetQuoteFolder.Click += new System.EventHandler(this.btnSetQuoteFolder_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Каталог котировок";
            // 
            // tbQuoteFolder
            // 
            this.tbQuoteFolder.Location = new System.Drawing.Point(11, 65);
            this.tbQuoteFolder.Name = "tbQuoteFolder";
            this.tbQuoteFolder.Size = new System.Drawing.Size(306, 20);
            this.tbQuoteFolder.TabIndex = 7;
            // 
            // btnDiscardSettings
            // 
            this.btnDiscardSettings.Location = new System.Drawing.Point(92, 143);
            this.btnDiscardSettings.Name = "btnDiscardSettings";
            this.btnDiscardSettings.Size = new System.Drawing.Size(75, 23);
            this.btnDiscardSettings.TabIndex = 6;
            this.btnDiscardSettings.Text = "Отменить";
            this.btnDiscardSettings.UseVisualStyleBackColor = true;
            this.btnDiscardSettings.Click += new System.EventHandler(this.btnDiscardSettings_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(11, 143);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSaveSettings.TabIndex = 5;
            this.btnSaveSettings.Text = "Сохранить";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // tbQuoteTimezoneShift
            // 
            this.tbQuoteTimezoneShift.Location = new System.Drawing.Point(11, 24);
            this.tbQuoteTimezoneShift.Name = "tbQuoteTimezoneShift";
            this.tbQuoteTimezoneShift.Size = new System.Drawing.Size(75, 20);
            this.tbQuoteTimezoneShift.TabIndex = 4;
            this.tbQuoteTimezoneShift.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(215, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Опережение (+) отставание (-) котировки";
            // 
            // tbLog
            // 
            this.tbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLog.Location = new System.Drawing.Point(0, 219);
            this.tbLog.Name = "tbLog";
            this.tbLog.Size = new System.Drawing.Size(607, 105);
            this.tbLog.TabIndex = 1;
            this.tbLog.Text = "";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "hst";
            this.openFileDialog.Filter = "MT4 Quote (*.hst)|*.hst|MT4 Server Quote (*.fxh)|*.fxh|All Files|*.*";
            this.openFileDialog.FilterIndex = 0;
            this.openFileDialog.Title = "Выбрать файл котировок";
            // 
            // tbHighStepPercent
            // 
            this.tbHighStepPercent.Location = new System.Drawing.Point(232, 24);
            this.tbHighStepPercent.Name = "tbHighStepPercent";
            this.tbHighStepPercent.Size = new System.Drawing.Size(75, 20);
            this.tbHighStepPercent.TabIndex = 11;
            this.tbHighStepPercent.Text = "1.5";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(229, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(162, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Процент больших приращений";
            // 
            // tbIterations
            // 
            this.tbIterations.Location = new System.Drawing.Point(11, 107);
            this.tbIterations.Name = "tbIterations";
            this.tbIterations.Size = new System.Drawing.Size(75, 20);
            this.tbIterations.TabIndex = 12;
            this.tbIterations.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 91);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(64, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Испытаний";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 324);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.Text = "Опционный калькулятор T#";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelTop.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageCalc.ResumeLayout(false);
            this.tabPageCalc.PerformLayout();
            this.tabPageSets.ResumeLayout(false);
            this.tabPageSets.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageCalc;
        private System.Windows.Forms.TabPage tabPageSets;
        private System.Windows.Forms.RichTextBox tbLog;
        private System.Windows.Forms.TextBox tbQuoteTimezoneShift;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDiscardSettings;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.TextBox tbQuoteFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSetQuoteFolder;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbOptionType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbEnterPrice;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbStrikePrice;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnPickQuoteFile;
        private System.Windows.Forms.TextBox tbQuoteFileName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dpTradeTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dpExpireTime;
        private System.Windows.Forms.CheckBox cbRemoveTrend;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox tbHighStepPercent;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbIterations;
    }
}

