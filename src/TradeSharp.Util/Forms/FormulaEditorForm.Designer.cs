namespace TradeSharp.Util.Forms
{
    partial class FormulaEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormulaEditorForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnTemplateStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbTemplates = new System.Windows.Forms.ComboBox();
            this.panelBotm = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.codeEditor = new System.Windows.Forms.RichTextBox();
            this.panelTop.SuspendLayout();
            this.panelBotm.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnTemplateStart);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.cbTemplates);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(439, 32);
            this.panelTop.TabIndex = 0;
            // 
            // btnTemplateStart
            // 
            this.btnTemplateStart.AutoSize = true;
            this.btnTemplateStart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTemplateStart.Location = new System.Drawing.Point(242, 4);
            this.btnTemplateStart.Name = "btnTemplateStart";
            this.btnTemplateStart.Size = new System.Drawing.Size(26, 23);
            this.btnTemplateStart.TabIndex = 2;
            this.btnTemplateStart.Text = "...";
            this.btnTemplateStart.UseVisualStyleBackColor = true;
            this.btnTemplateStart.Click += new System.EventHandler(this.BtnTemplateStartClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 1;
            this.label1.Tag = "TitleTemplateSmall";
            this.label1.Text = "шаблон";
            // 
            // cbTemplates
            // 
            this.cbTemplates.FormattingEnabled = true;
            this.cbTemplates.Location = new System.Drawing.Point(60, 6);
            this.cbTemplates.Name = "cbTemplates";
            this.cbTemplates.Size = new System.Drawing.Size(176, 21);
            this.cbTemplates.TabIndex = 0;
            // 
            // panelBotm
            // 
            this.panelBotm.Controls.Add(this.btnCancel);
            this.panelBotm.Controls.Add(this.btnOK);
            this.panelBotm.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotm.Location = new System.Drawing.Point(0, 302);
            this.panelBotm.Name = "panelBotm";
            this.panelBotm.Size = new System.Drawing.Size(439, 34);
            this.panelBotm.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(119, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(12, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Tag = "TitleOK";
            this.btnOK.Text = "ОК";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // codeEditor
            // 
            this.codeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeEditor.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.codeEditor.ForeColor = System.Drawing.Color.Navy;
            this.codeEditor.Location = new System.Drawing.Point(0, 32);
            this.codeEditor.Name = "codeEditor";
            this.codeEditor.Size = new System.Drawing.Size(439, 270);
            this.codeEditor.TabIndex = 2;
            this.codeEditor.Text = "";
            // 
            // FormulaEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 336);
            this.Controls.Add(this.codeEditor);
            this.Controls.Add(this.panelBotm);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormulaEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleExpressionEditor";
            this.Text = "Редактор формул";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBotm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnTemplateStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbTemplates;
        private System.Windows.Forms.Panel panelBotm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.RichTextBox codeEditor;
    }
}