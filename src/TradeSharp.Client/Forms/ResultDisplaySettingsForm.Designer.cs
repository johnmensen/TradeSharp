namespace TradeSharp.Client.Forms
{
    partial class ResultDisplaySettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResultDisplaySettingsForm));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbRemoveOld = new System.Windows.Forms.CheckBox();
            this.cbShowEnters = new System.Windows.Forms.CheckBox();
            this.cbShowExits = new System.Windows.Forms.CheckBox();
            this.cbDetailedEnters = new System.Windows.Forms.CheckBox();
            this.cbDetailedExits = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(12, 118);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "ОК";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(146, 118);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cbRemoveOld
            // 
            this.cbRemoveOld.AutoSize = true;
            this.cbRemoveOld.Checked = true;
            this.cbRemoveOld.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRemoveOld.Location = new System.Drawing.Point(3, 3);
            this.cbRemoveOld.Name = "cbRemoveOld";
            this.cbRemoveOld.Size = new System.Drawing.Size(152, 17);
            this.cbRemoveOld.TabIndex = 2;
            this.cbRemoveOld.Text = "Убрать старые маркеры";
            this.cbRemoveOld.UseVisualStyleBackColor = true;
            // 
            // cbShowEnters
            // 
            this.cbShowEnters.AutoSize = true;
            this.cbShowEnters.Checked = true;
            this.cbShowEnters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbShowEnters.Location = new System.Drawing.Point(3, 26);
            this.cbShowEnters.Name = "cbShowEnters";
            this.cbShowEnters.Size = new System.Drawing.Size(123, 17);
            this.cbShowEnters.TabIndex = 3;
            this.cbShowEnters.Text = "Показывать входы";
            this.cbShowEnters.UseVisualStyleBackColor = true;
            // 
            // cbShowExits
            // 
            this.cbShowExits.AutoSize = true;
            this.cbShowExits.Checked = true;
            this.cbShowExits.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbShowExits.Location = new System.Drawing.Point(3, 49);
            this.cbShowExits.Name = "cbShowExits";
            this.cbShowExits.Size = new System.Drawing.Size(131, 17);
            this.cbShowExits.TabIndex = 4;
            this.cbShowExits.Text = "Показывать выходы";
            this.cbShowExits.UseVisualStyleBackColor = true;
            // 
            // cbDetailedEnters
            // 
            this.cbDetailedEnters.AutoSize = true;
            this.cbDetailedEnters.Checked = true;
            this.cbDetailedEnters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDetailedEnters.Location = new System.Drawing.Point(175, 26);
            this.cbDetailedEnters.Name = "cbDetailedEnters";
            this.cbDetailedEnters.Size = new System.Drawing.Size(76, 17);
            this.cbDetailedEnters.TabIndex = 6;
            this.cbDetailedEnters.Text = "Подробно";
            this.cbDetailedEnters.UseVisualStyleBackColor = true;
            // 
            // cbDetailedExits
            // 
            this.cbDetailedExits.AutoSize = true;
            this.cbDetailedExits.Checked = true;
            this.cbDetailedExits.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDetailedExits.Location = new System.Drawing.Point(175, 49);
            this.cbDetailedExits.Name = "cbDetailedExits";
            this.cbDetailedExits.Size = new System.Drawing.Size(76, 17);
            this.cbDetailedExits.TabIndex = 7;
            this.cbDetailedExits.Text = "Подробно";
            this.cbDetailedExits.UseVisualStyleBackColor = true;
            // 
            // ResultDisplaySettingsForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(286, 150);
            this.Controls.Add(this.cbDetailedExits);
            this.Controls.Add(this.cbDetailedEnters);
            this.Controls.Add(this.cbShowExits);
            this.Controls.Add(this.cbShowEnters);
            this.Controls.Add(this.cbRemoveOld);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResultDisplaySettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Результаты моделирования";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbRemoveOld;
        private System.Windows.Forms.CheckBox cbShowEnters;
        private System.Windows.Forms.CheckBox cbShowExits;
        private System.Windows.Forms.CheckBox cbDetailedEnters;
        private System.Windows.Forms.CheckBox cbDetailedExits;
    }
}