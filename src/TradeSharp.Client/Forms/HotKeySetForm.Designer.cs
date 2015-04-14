namespace TradeSharp.Client.Forms
{
    partial class HotKeySetForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HotKeySetForm));
            this.pnlActionName = new System.Windows.Forms.Panel();
            this.txtActionName = new System.Windows.Forms.Label();
            this.pnlHotKey = new System.Windows.Forms.Panel();
            this.txtHotKey = new System.Windows.Forms.Label();
            this.txtbxHotKeyNewValue = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlActionName.SuspendLayout();
            this.pnlHotKey.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlActionName
            // 
            this.pnlActionName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlActionName.Controls.Add(this.txtActionName);
            this.pnlActionName.Location = new System.Drawing.Point(12, 12);
            this.pnlActionName.Name = "pnlActionName";
            this.pnlActionName.Size = new System.Drawing.Size(392, 77);
            this.pnlActionName.TabIndex = 0;
            // 
            // txtActionName
            // 
            this.txtActionName.AutoSize = true;
            this.txtActionName.Location = new System.Drawing.Point(10, 10);
            this.txtActionName.Margin = new System.Windows.Forms.Padding(10);
            this.txtActionName.Name = "txtActionName";
            this.txtActionName.Size = new System.Drawing.Size(0, 13);
            this.txtActionName.TabIndex = 0;
            // 
            // pnlHotKey
            // 
            this.pnlHotKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlHotKey.Controls.Add(this.txtHotKey);
            this.pnlHotKey.Controls.Add(this.txtbxHotKeyNewValue);
            this.pnlHotKey.Location = new System.Drawing.Point(13, 96);
            this.pnlHotKey.Name = "pnlHotKey";
            this.pnlHotKey.Size = new System.Drawing.Size(391, 77);
            this.pnlHotKey.TabIndex = 1;
            // 
            // txtHotKey
            // 
            this.txtHotKey.AutoSize = true;
            this.txtHotKey.Location = new System.Drawing.Point(12, 14);
            this.txtHotKey.Name = "txtHotKey";
            this.txtHotKey.Size = new System.Drawing.Size(166, 13);
            this.txtHotKey.TabIndex = 1;
            this.txtHotKey.Tag = "TitleKeyOrKeyCombination";
            this.txtHotKey.Text = "Клавиша / комбинация клавиш";
            // 
            // txtbxHotKeyNewValue
            // 
            this.txtbxHotKeyNewValue.Enabled = false;
            this.txtbxHotKeyNewValue.Location = new System.Drawing.Point(12, 41);
            this.txtbxHotKeyNewValue.Name = "txtbxHotKeyNewValue";
            this.txtbxHotKeyNewValue.Size = new System.Drawing.Size(365, 20);
            this.txtbxHotKeyNewValue.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(246, 179);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Tag = "TitleOK";
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(327, 179);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // HotKeySetForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(422, 223);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.pnlHotKey);
            this.Controls.Add(this.pnlActionName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(430, 250);
            this.MinimumSize = new System.Drawing.Size(430, 250);
            this.Name = "HotKeySetForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "TitleKeyCombinationAssignment";
            this.Text = "Выбор комбинации клавиш";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HotKeySetFormFormClosed);
            this.pnlActionName.ResumeLayout(false);
            this.pnlActionName.PerformLayout();
            this.pnlHotKey.ResumeLayout(false);
            this.pnlHotKey.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlActionName;
        private System.Windows.Forms.Label txtActionName;
        private System.Windows.Forms.Panel pnlHotKey;
        private System.Windows.Forms.Label txtHotKey;
        private System.Windows.Forms.TextBox txtbxHotKeyNewValue;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}