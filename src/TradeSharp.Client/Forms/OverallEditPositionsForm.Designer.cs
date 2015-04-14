namespace TradeSharp.Client.Forms
{
    partial class OverallEditPositionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OverallEditPositionsForm));
            this.CancelBtn = new System.Windows.Forms.Button();
            this.ApplyBtn = new System.Windows.Forms.Button();
            this.cbTake = new System.Windows.Forms.CheckBox();
            this.cbStop = new System.Windows.Forms.CheckBox();
            this.tbTake = new System.Windows.Forms.TextBox();
            this.tbStop = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(139, 36);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(116, 23);
            this.CancelBtn.TabIndex = 4;
            this.CancelBtn.Text = "Отменить";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // ApplyBtn
            // 
            this.ApplyBtn.Location = new System.Drawing.Point(12, 36);
            this.ApplyBtn.Name = "ApplyBtn";
            this.ApplyBtn.Size = new System.Drawing.Size(117, 23);
            this.ApplyBtn.TabIndex = 3;
            this.ApplyBtn.Text = "Применить";
            this.ApplyBtn.UseVisualStyleBackColor = true;
            this.ApplyBtn.Click += new System.EventHandler(this.ApplyBtn_Click);
            // 
            // cbTake
            // 
            this.cbTake.AutoSize = true;
            this.cbTake.Location = new System.Drawing.Point(12, 12);
            this.cbTake.Name = "cbTake";
            this.cbTake.Size = new System.Drawing.Size(45, 17);
            this.cbTake.TabIndex = 5;
            this.cbTake.Text = "T/P";
            this.cbTake.UseVisualStyleBackColor = true;
            this.cbTake.CheckedChanged += new System.EventHandler(this.cbTake_CheckedChanged);
            // 
            // cbStop
            // 
            this.cbStop.AutoSize = true;
            this.cbStop.Location = new System.Drawing.Point(139, 13);
            this.cbStop.Name = "cbStop";
            this.cbStop.Size = new System.Drawing.Size(44, 17);
            this.cbStop.TabIndex = 6;
            this.cbStop.Text = "S/L";
            this.cbStop.UseVisualStyleBackColor = true;
            this.cbStop.CheckedChanged += new System.EventHandler(this.cbStop_CheckedChanged);
            // 
            // tbTake
            // 
            this.tbTake.Enabled = false;
            this.tbTake.Location = new System.Drawing.Point(63, 10);
            this.tbTake.Name = "tbTake";
            this.tbTake.Size = new System.Drawing.Size(66, 20);
            this.tbTake.TabIndex = 7;
            // 
            // tbStop
            // 
            this.tbStop.Enabled = false;
            this.tbStop.Location = new System.Drawing.Point(189, 9);
            this.tbStop.Name = "tbStop";
            this.tbStop.Size = new System.Drawing.Size(66, 20);
            this.tbStop.TabIndex = 8;
            // 
            // OverallEditPositionsForm
            // 
            this.AcceptButton = this.ApplyBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(264, 70);
            this.Controls.Add(this.tbStop);
            this.Controls.Add(this.tbTake);
            this.Controls.Add(this.cbStop);
            this.Controls.Add(this.cbTake);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.ApplyBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OverallEditPositionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Редактировать ордеры ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button ApplyBtn;
        private System.Windows.Forms.CheckBox cbTake;
        private System.Windows.Forms.CheckBox cbStop;
        private System.Windows.Forms.TextBox tbTake;
        private System.Windows.Forms.TextBox tbStop;
    }
}