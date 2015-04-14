namespace TradeSharp.Client.Forms
{
    partial class SaveChartTemplate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveChartTemplate));
            this.chebxSaveIndicators = new System.Windows.Forms.CheckBox();
            this.chbxSaveChartSettings = new System.Windows.Forms.CheckBox();
            this.cbTemplateName = new System.Windows.Forms.ComboBox();
            this.txtTemplateName = new System.Windows.Forms.Label();
            this.txtCurrentTickerLabel = new System.Windows.Forms.Label();
            this.txtCurrentTickerValue = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chbxBindCurrencyTicket = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chebxSaveIndicators
            // 
            this.chebxSaveIndicators.AutoSize = true;
            this.chebxSaveIndicators.Checked = true;
            this.chebxSaveIndicators.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chebxSaveIndicators.Location = new System.Drawing.Point(13, 114);
            this.chebxSaveIndicators.Name = "chebxSaveIndicators";
            this.chebxSaveIndicators.Size = new System.Drawing.Size(142, 17);
            this.chebxSaveIndicators.TabIndex = 0;
            this.chebxSaveIndicators.Tag = "TitleSaveIndicatorsSmall";
            this.chebxSaveIndicators.Text = "сохранить индикаторы";
            this.chebxSaveIndicators.UseVisualStyleBackColor = true;
            this.chebxSaveIndicators.CheckedChanged += new System.EventHandler(this.ChebxSaveCheckedChanged);
            // 
            // chbxSaveChartSettings
            // 
            this.chbxSaveChartSettings.AutoSize = true;
            this.chbxSaveChartSettings.Checked = true;
            this.chbxSaveChartSettings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbxSaveChartSettings.Location = new System.Drawing.Point(13, 137);
            this.chbxSaveChartSettings.Name = "chbxSaveChartSettings";
            this.chbxSaveChartSettings.Size = new System.Drawing.Size(180, 17);
            this.chbxSaveChartSettings.TabIndex = 1;
            this.chbxSaveChartSettings.Tag = "TitleSaveChartSettingsSmall";
            this.chbxSaveChartSettings.Text = "сохранить настройки графика";
            this.chbxSaveChartSettings.UseVisualStyleBackColor = true;
            this.chbxSaveChartSettings.CheckedChanged += new System.EventHandler(this.ChebxSaveCheckedChanged);
            // 
            // cbTemplateName
            // 
            this.cbTemplateName.FormattingEnabled = true;
            this.cbTemplateName.Location = new System.Drawing.Point(12, 47);
            this.cbTemplateName.Name = "cbTemplateName";
            this.cbTemplateName.Size = new System.Drawing.Size(260, 21);
            this.cbTemplateName.TabIndex = 2;
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.AutoSize = true;
            this.txtTemplateName.Location = new System.Drawing.Point(9, 31);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(104, 13);
            this.txtTemplateName.TabIndex = 3;
            this.txtTemplateName.Tag = "TitleName";
            this.txtTemplateName.Text = "Название шаблона";
            // 
            // txtCurrentTickerLabel
            // 
            this.txtCurrentTickerLabel.AutoSize = true;
            this.txtCurrentTickerLabel.Location = new System.Drawing.Point(9, 9);
            this.txtCurrentTickerLabel.Name = "txtCurrentTickerLabel";
            this.txtCurrentTickerLabel.Size = new System.Drawing.Size(128, 13);
            this.txtCurrentTickerLabel.TabIndex = 4;
            this.txtCurrentTickerLabel.Tag = "TitleSelectedInstument";
            this.txtCurrentTickerLabel.Text = "Выбранный инструмент";
            // 
            // txtCurrentTickerValue
            // 
            this.txtCurrentTickerValue.AutoSize = true;
            this.txtCurrentTickerValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtCurrentTickerValue.Location = new System.Drawing.Point(146, 9);
            this.txtCurrentTickerValue.Name = "txtCurrentTickerValue";
            this.txtCurrentTickerValue.Size = new System.Drawing.Size(84, 13);
            this.txtCurrentTickerValue.TabIndex = 5;
            this.txtCurrentTickerValue.Text = "(инструмент)";
            // 
            // btnSave
            // 
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(12, 160);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 6;
            this.btnSave.Tag = "TitleSave";
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSaveClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(93, 160);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chbxBindCurrencyTicket
            // 
            this.chbxBindCurrencyTicket.AutoSize = true;
            this.chbxBindCurrencyTicket.Location = new System.Drawing.Point(13, 74);
            this.chbxBindCurrencyTicket.Name = "chbxBindCurrencyTicket";
            this.chbxBindCurrencyTicket.Size = new System.Drawing.Size(208, 17);
            this.chbxBindCurrencyTicket.TabIndex = 8;
            this.chbxBindCurrencyTicket.Tag = "TitleBindTemplateToPairSmall";
            this.chbxBindCurrencyTicket.Text = "привязать шаблон к валютной паре";
            this.chbxBindCurrencyTicket.UseVisualStyleBackColor = true;
            this.chbxBindCurrencyTicket.CheckedChanged += new System.EventHandler(this.ChebxSaveCheckedChanged);
            // 
            // SaveChartTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 193);
            this.Controls.Add(this.chbxBindCurrencyTicket);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtCurrentTickerValue);
            this.Controls.Add(this.txtCurrentTickerLabel);
            this.Controls.Add(this.txtTemplateName);
            this.Controls.Add(this.cbTemplateName);
            this.Controls.Add(this.chbxSaveChartSettings);
            this.Controls.Add(this.chebxSaveIndicators);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(290, 220);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(290, 220);
            this.Name = "SaveChartTemplate";
            this.Tag = "TitleSaveTemplate";
            this.Text = "Сохранить шаблон";
            this.Load += new System.EventHandler(this.SaveChartTemplateLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chebxSaveIndicators;
        private System.Windows.Forms.CheckBox chbxSaveChartSettings;
        private System.Windows.Forms.ComboBox cbTemplateName;
        private System.Windows.Forms.Label txtTemplateName;
        private System.Windows.Forms.Label txtCurrentTickerLabel;
        private System.Windows.Forms.Label txtCurrentTickerValue;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chbxBindCurrencyTicket;
    }
}