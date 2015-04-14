namespace TradeSharp.Client.Forms
{
    partial class SelectItemsDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectItemsDlg));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.BtnUnselect = new System.Windows.Forms.Button();
            this.BtnSelect = new System.Windows.Forms.Button();
            this.lbColumns = new System.Windows.Forms.ListBox();
            this.lbSelected = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(256, 206);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(126, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Отменить";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(112, 206);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(126, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Применить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // BtnUnselect
            // 
            this.BtnUnselect.Location = new System.Drawing.Point(183, 114);
            this.BtnUnselect.Name = "BtnUnselect";
            this.BtnUnselect.Size = new System.Drawing.Size(28, 23);
            this.BtnUnselect.TabIndex = 29;
            this.BtnUnselect.Text = "<<";
            this.BtnUnselect.UseVisualStyleBackColor = true;
            this.BtnUnselect.Click += new System.EventHandler(this.BtnUnselect_Click);
            // 
            // BtnSelect
            // 
            this.BtnSelect.Location = new System.Drawing.Point(183, 85);
            this.BtnSelect.Name = "BtnSelect";
            this.BtnSelect.Size = new System.Drawing.Size(28, 23);
            this.BtnSelect.TabIndex = 28;
            this.BtnSelect.Text = ">>";
            this.BtnSelect.UseVisualStyleBackColor = true;
            this.BtnSelect.Click += new System.EventHandler(this.BtnSelect_Click);
            // 
            // lbColumns
            // 
            this.lbColumns.FormattingEnabled = true;
            this.lbColumns.Location = new System.Drawing.Point(11, 27);
            this.lbColumns.Name = "lbColumns";
            this.lbColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbColumns.Size = new System.Drawing.Size(165, 173);
            this.lbColumns.TabIndex = 27;
            this.lbColumns.DoubleClick += new System.EventHandler(this.lbColumns_DoubleClick);
            // 
            // lbSelected
            // 
            this.lbSelected.FormattingEnabled = true;
            this.lbSelected.Location = new System.Drawing.Point(217, 27);
            this.lbSelected.Name = "lbSelected";
            this.lbSelected.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbSelected.Size = new System.Drawing.Size(165, 173);
            this.lbSelected.TabIndex = 30;
            this.lbSelected.DoubleClick += new System.EventHandler(this.lbSelected_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "Все";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(214, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Выбранные";
            // 
            // SelectItemsDlg
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(394, 237);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbSelected);
            this.Controls.Add(this.BtnUnselect);
            this.Controls.Add(this.BtnSelect);
            this.Controls.Add(this.lbColumns);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectItemsDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки столбцов";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button BtnUnselect;
        private System.Windows.Forms.Button BtnSelect;
        private System.Windows.Forms.ListBox lbColumns;
        private System.Windows.Forms.ListBox lbSelected;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}