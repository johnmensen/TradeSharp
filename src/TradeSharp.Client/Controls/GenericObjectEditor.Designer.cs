namespace TradeSharp.Client.Controls
{
    partial class GenericObjectEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelLeft = new System.Windows.Forms.Panel();
            this.lblComment = new System.Windows.Forms.Label();
            this.editCombo = new System.Windows.Forms.ComboBox();
            this.editDateTime = new System.Windows.Forms.DateTimePicker();
            this.editCheck = new System.Windows.Forms.CheckBox();
            this.editText = new System.Windows.Forms.TextBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.editColorPick = new System.Windows.Forms.Button();
            this.panelLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.lblComment);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(165, 30);
            this.panelLeft.TabIndex = 5;
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(3, 4);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(22, 13);
            this.lblComment.TabIndex = 1;
            this.lblComment.Text = "- - -";
            // 
            // editCombo
            // 
            this.editCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.editCombo.FormattingEnabled = true;
            this.editCombo.Location = new System.Drawing.Point(349, 3);
            this.editCombo.Name = "editCombo";
            this.editCombo.Size = new System.Drawing.Size(70, 21);
            this.editCombo.TabIndex = 9;
            this.editCombo.Visible = false;
            // 
            // editDateTime
            // 
            this.editDateTime.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.editDateTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.editDateTime.Location = new System.Drawing.Point(273, 4);
            this.editDateTime.Name = "editDateTime";
            this.editDateTime.Size = new System.Drawing.Size(70, 20);
            this.editDateTime.TabIndex = 8;
            this.editDateTime.Visible = false;
            // 
            // editCheck
            // 
            this.editCheck.AutoSize = true;
            this.editCheck.Location = new System.Drawing.Point(252, 7);
            this.editCheck.Name = "editCheck";
            this.editCheck.Size = new System.Drawing.Size(15, 14);
            this.editCheck.TabIndex = 7;
            this.editCheck.UseVisualStyleBackColor = true;
            this.editCheck.Visible = false;
            // 
            // editText
            // 
            this.editText.Location = new System.Drawing.Point(174, 4);
            this.editText.Name = "editText";
            this.editText.Size = new System.Drawing.Size(68, 20);
            this.editText.TabIndex = 6;
            this.editText.Visible = false;
            // 
            // editColorPick
            // 
            this.editColorPick.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.editColorPick.Location = new System.Drawing.Point(425, 3);
            this.editColorPick.Name = "editColorPick";
            this.editColorPick.Size = new System.Drawing.Size(29, 23);
            this.editColorPick.TabIndex = 10;
            this.editColorPick.UseVisualStyleBackColor = true;
            this.editColorPick.Visible = false;
            this.editColorPick.Click += new System.EventHandler(this.EditColorPickClick);
            // 
            // GenericObjectEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.editColorPick);
            this.Controls.Add(this.editCombo);
            this.Controls.Add(this.editDateTime);
            this.Controls.Add(this.editCheck);
            this.Controls.Add(this.editText);
            this.Controls.Add(this.panelLeft);
            this.Name = "GenericObjectEditor";
            this.Size = new System.Drawing.Size(463, 30);
            this.Load += new System.EventHandler(this.GenericObjectEditorLoad);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.ComboBox editCombo;
        private System.Windows.Forms.DateTimePicker editDateTime;
        private System.Windows.Forms.CheckBox editCheck;
        private System.Windows.Forms.TextBox editText;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button editColorPick;

    }
}
