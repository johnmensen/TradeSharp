namespace TradeSharp.Client.Forms
{
    partial class ApplyChartTemplate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyChartTemplate));
            this.cbTemplateName = new System.Windows.Forms.ComboBox();
            this.chbOnlyCurrentTickerTemplate = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtTemplateName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbTemplateName
            // 
            this.cbTemplateName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTemplateName.FormattingEnabled = true;
            this.cbTemplateName.Location = new System.Drawing.Point(64, 10);
            this.cbTemplateName.Name = "cbTemplateName";
            this.cbTemplateName.Size = new System.Drawing.Size(266, 21);
            this.cbTemplateName.TabIndex = 0;
            // 
            // chbOnlyCurrentTickerTemplate
            // 
            this.chbOnlyCurrentTickerTemplate.AutoSize = true;
            this.chbOnlyCurrentTickerTemplate.Location = new System.Drawing.Point(15, 49);
            this.chbOnlyCurrentTickerTemplate.Name = "chbOnlyCurrentTickerTemplate";
            this.chbOnlyCurrentTickerTemplate.Size = new System.Drawing.Size(313, 17);
            this.chbOnlyCurrentTickerTemplate.TabIndex = 1;
            this.chbOnlyCurrentTickerTemplate.Tag = "TitleOnlyTemplateBindToCurrentInstumentSmall";
            this.chbOnlyCurrentTickerTemplate.Text = "только шаблоны, привязанные к текущему инструменту";
            this.chbOnlyCurrentTickerTemplate.UseVisualStyleBackColor = true;
            this.chbOnlyCurrentTickerTemplate.CheckedChanged += new System.EventHandler(this.ChbOnlyCurrentTickerTemplateCheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(163, 77);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(86, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Tag = "TitleApply";
            this.btnOk.Text = "Применить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCancel.Location = new System.Drawing.Point(255, 77);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.AutoSize = true;
            this.txtTemplateName.Location = new System.Drawing.Point(12, 13);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(46, 13);
            this.txtTemplateName.TabIndex = 4;
            this.txtTemplateName.Tag = "TitleTemplate";
            this.txtTemplateName.Text = "Шаблон";
            // 
            // ApplyChartTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 123);
            this.Controls.Add(this.txtTemplateName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chbOnlyCurrentTickerTemplate);
            this.Controls.Add(this.cbTemplateName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(360, 150);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(360, 150);
            this.Name = "ApplyChartTemplate";
            this.Tag = "TitleApplyTemplateToChart";
            this.Text = "Применить шаблон к графику";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbTemplateName;
        private System.Windows.Forms.CheckBox chbOnlyCurrentTickerTemplate;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label txtTemplateName;
    }
}