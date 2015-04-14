using System.Windows.Forms;

namespace TradeSharp.Client.Controls
{
    /// <summary>
    /// Summary description for AboutCtrl.
    /// </summary>
    partial class AboutCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutCtrl));
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.ultraLabel1 = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.ultraLabel2 = new System.Windows.Forms.Label();
            this.nameProgramLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlText;
            this.panel1.Location = new System.Drawing.Point(249, 8);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1, 192);
            this.panel1.TabIndex = 1;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(256, 31);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(106, 13);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://tradesharp.net";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1LinkClicked);
            // 
            // ultraLabel1
            // 
            this.ultraLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ultraLabel1.Location = new System.Drawing.Point(256, 57);
            this.ultraLabel1.Name = "ultraLabel1";
            this.ultraLabel1.Size = new System.Drawing.Size(308, 71);
            this.ultraLabel1.TabIndex = 3;
            this.ultraLabel1.Text = resources.GetString("ultraLabel1.Text");
            // 
            // closeBtn
            // 
            this.closeBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.closeBtn.Location = new System.Drawing.Point(481, 166);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(90, 29);
            this.closeBtn.TabIndex = 4;
            this.closeBtn.Tag = "TitleClose";
            this.closeBtn.Text = "Закрыть";
            // 
            // ultraLabel2
            // 
            this.ultraLabel2.AutoSize = true;
            this.ultraLabel2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ultraLabel2.Location = new System.Drawing.Point(256, 137);
            this.ultraLabel2.Name = "ultraLabel2";
            this.ultraLabel2.Size = new System.Drawing.Size(200, 14);
            this.ultraLabel2.TabIndex = 6;
            this.ultraLabel2.Text = "Copyright © 2002-2013, Forexinvest Ltd";
            // 
            // nameProgramLabel
            // 
            this.nameProgramLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nameProgramLabel.Location = new System.Drawing.Point(6, 129);
            this.nameProgramLabel.Name = "nameProgramLabel";
            this.nameProgramLabel.Size = new System.Drawing.Size(200, 29);
            this.nameProgramLabel.TabIndex = 7;
            this.nameProgramLabel.Text = "Имя программы";
            // 
            // versionLabel
            // 
            this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.versionLabel.ForeColor = System.Drawing.Color.Black;
            this.versionLabel.Location = new System.Drawing.Point(7, 158);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(240, 16);
            this.versionLabel.TabIndex = 8;
            this.versionLabel.Text = "версия билда и дата";
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Image = global::TradeSharp.Client.Properties.Resources.terminal_logo_100;
            this.pictureBoxLogo.Location = new System.Drawing.Point(7, 8);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(243, 120);
            this.pictureBoxLogo.TabIndex = 11;
            this.pictureBoxLogo.TabStop = false;
            // 
            // AboutCtrl
            // 
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.nameProgramLabel);
            this.Controls.Add(this.ultraLabel2);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.ultraLabel1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.panel1);
            this.Name = "AboutCtrl";
            this.Size = new System.Drawing.Size(588, 210);
            this.Load += new System.EventHandler(this.AboutCtrlLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private Label ultraLabel1;
        private Button closeBtn;
        private Label ultraLabel2;
        private Label nameProgramLabel;
        private Label versionLabel;

        private string _name = string.Empty;
        private string _version = string.Empty;
        private PictureBox pictureBoxLogo;
    }
}