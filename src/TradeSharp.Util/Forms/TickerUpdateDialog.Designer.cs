namespace TradeSharp.Util.Forms
{
    partial class TickerUpdateDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TickerUpdateDialog));
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnAcceptAll = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtbxCurentCustomValues = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(10, 36);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(91, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Text = "Применить";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // btnAcceptAll
            // 
            this.btnAcceptAll.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAcceptAll.Location = new System.Drawing.Point(107, 36);
            this.btnAcceptAll.Name = "btnAcceptAll";
            this.btnAcceptAll.Size = new System.Drawing.Size(136, 23);
            this.btnAcceptAll.TabIndex = 1;
            this.btnAcceptAll.Text = "Применить ко всем";
            this.btnAcceptAll.UseVisualStyleBackColor = true;
            this.btnAcceptAll.Click += new System.EventHandler(this.BtnAcceptAllClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(249, 36);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtbxCurentCustomValues
            // 
            this.txtbxCurentCustomValues.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtbxCurentCustomValues.Location = new System.Drawing.Point(10, 10);
            this.txtbxCurentCustomValues.Name = "txtbxCurentCustomValues";
            this.txtbxCurentCustomValues.Size = new System.Drawing.Size(510, 20);
            this.txtbxCurentCustomValues.TabIndex = 3;
            // 
            // TickerUpdateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 67);
            this.Controls.Add(this.txtbxCurentCustomValues);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAcceptAll);
            this.Controls.Add(this.btnAccept);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 105);
            this.Name = "TickerUpdateDialog";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "Объёмы торгов";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnAcceptAll;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtbxCurentCustomValues;
    }
}