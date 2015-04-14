using TradeSharp.Client.Controls;

namespace TradeSharp.Client.Forms
{
    partial class AboutDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDlg));
            this.aboutCtrl1 = new TradeSharp.Client.Controls.AboutCtrl();
            this.SuspendLayout();
            // 
            // aboutCtrl1
            // 
            this.aboutCtrl1.Location = new System.Drawing.Point(-2, -1);
            this.aboutCtrl1.Name = "aboutCtrl1";
            this.aboutCtrl1.NameProgram = "";
            this.aboutCtrl1.Size = new System.Drawing.Size(588, 211);
            this.aboutCtrl1.TabIndex = 0;
            this.aboutCtrl1.VersionProgram = "";
            // 
            // AboutDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 214);
            this.Controls.Add(this.aboutCtrl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleAboutMenu";
            this.Text = "О программе";
            this.Load += new System.EventHandler(this.AboutDlg_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AboutCtrl aboutCtrl1;

    }
}