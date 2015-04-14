using TradeSharp.Client.Controls;

namespace TradeSharp.Client.Forms
{
    partial class EditAccountDataForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditAccountDataForm));
            this.userInfoControl = new TradeSharp.Client.Controls.UserInfoControl();
            this.SuspendLayout();
            // 
            // userInfoControl
            // 
            this.userInfoControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userInfoControl.Location = new System.Drawing.Point(0, 0);
            this.userInfoControl.Name = "userInfoControl";
            this.userInfoControl.Size = new System.Drawing.Size(500, 499);
            this.userInfoControl.TabIndex = 0;
            // 
            // EditAccountDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 499);
            this.Controls.Add(this.userInfoControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 500);
            this.Name = "EditAccountDataForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleCredentials";
            this.Text = "Учетные данные";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.UserInfoControl userInfoControl;
    }
}