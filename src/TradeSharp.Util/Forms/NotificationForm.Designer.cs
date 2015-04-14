using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    partial class NotificationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationForm));
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnIgnore = new System.Windows.Forms.Button();
            this.btnRetry = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.repeatNotification = new System.Windows.Forms.CheckBox();
            this.pnlHader = new System.Windows.Forms.Panel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.title = new System.Windows.Forms.Label();
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.mainText = new System.Windows.Forms.RichTextBox();
            this.pnlFooter.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlHader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.pnlMainContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFooter
            // 
            this.pnlFooter.Controls.Add(this.pnlButtons);
            this.pnlFooter.Controls.Add(this.repeatNotification);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 158);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(10);
            this.pnlFooter.Size = new System.Drawing.Size(444, 90);
            this.pnlFooter.TabIndex = 1;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnNo);
            this.pnlButtons.Controls.Add(this.btnYes);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnIgnore);
            this.pnlButtons.Controls.Add(this.btnRetry);
            this.pnlButtons.Controls.Add(this.btnAbort);
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(10, 43);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(424, 37);
            this.pnlButtons.TabIndex = 1;
            // 
            // btnNo
            // 
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnNo.Location = new System.Drawing.Point(336, 4);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(75, 23);
            this.btnNo.TabIndex = 6;
            this.btnNo.Tag = "TitleNo";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Visible = false;
            // 
            // btnYes
            // 
            this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnYes.Location = new System.Drawing.Point(276, 4);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(75, 23);
            this.btnYes.TabIndex = 5;
            this.btnYes.Tag = "TitleYes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(195, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            // 
            // btnIgnore
            // 
            this.btnIgnore.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.btnIgnore.Location = new System.Drawing.Point(131, 4);
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.Size = new System.Drawing.Size(75, 23);
            this.btnIgnore.TabIndex = 4;
            this.btnIgnore.Tag = "TitleIgnore";
            this.btnIgnore.UseVisualStyleBackColor = true;
            this.btnIgnore.Visible = false;
            // 
            // btnRetry
            // 
            this.btnRetry.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.btnRetry.Location = new System.Drawing.Point(84, 4);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(75, 23);
            this.btnRetry.TabIndex = 3;
            this.btnRetry.Tag = "TitleRetry";
            this.btnRetry.UseVisualStyleBackColor = true;
            this.btnRetry.Visible = false;
            // 
            // btnAbort
            // 
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnAbort.Location = new System.Drawing.Point(50, 4);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 2;
            this.btnAbort.Tag = "TitleAbort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Visible = false;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(3, 4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Tag = "TitleOK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Visible = false;
            // 
            // repeatNotification
            // 
            this.repeatNotification.AutoSize = true;
            this.repeatNotification.Checked = true;
            this.repeatNotification.CheckState = System.Windows.Forms.CheckState.Checked;
            this.repeatNotification.Location = new System.Drawing.Point(13, 13);
            this.repeatNotification.Name = "repeatNotification";
            this.repeatNotification.Size = new System.Drawing.Size(253, 17);
            this.repeatNotification.TabIndex = 0;
            this.repeatNotification.Text = "показывать это уведомление в дальнейшем";
            this.repeatNotification.UseVisualStyleBackColor = true;
            // 
            // pnlHader
            // 
            this.pnlHader.Controls.Add(this.pictureBox);
            this.pnlHader.Controls.Add(this.title);
            this.pnlHader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHader.Location = new System.Drawing.Point(0, 0);
            this.pnlHader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHader.Name = "pnlHader";
            this.pnlHader.Padding = new System.Windows.Forms.Padding(30, 12, 12, 3);
            this.pnlHader.Size = new System.Drawing.Size(444, 46);
            this.pnlHader.TabIndex = 2;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(12, 10);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(32, 32);
            this.pictureBox.TabIndex = 2;
            this.pictureBox.TabStop = false;
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Matura MT Script Capitals", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.title.Location = new System.Drawing.Point(53, 17);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(0, 19);
            this.title.TabIndex = 1;
            // 
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.mainText);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(0, 46);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Padding = new System.Windows.Forms.Padding(12);
            this.pnlMainContent.Size = new System.Drawing.Size(444, 112);
            this.pnlMainContent.TabIndex = 3;
            // 
            // mainText
            // 
            this.mainText.BackColor = System.Drawing.SystemColors.Control;
            this.mainText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mainText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainText.Location = new System.Drawing.Point(12, 12);
            this.mainText.Margin = new System.Windows.Forms.Padding(30);
            this.mainText.Name = "mainText";
            this.mainText.Size = new System.Drawing.Size(420, 88);
            this.mainText.TabIndex = 4;
            this.mainText.Text = "";
            // 
            // NotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 248);
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.pnlHader);
            this.Controls.Add(this.pnlFooter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(450, 270);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 270);
            this.Name = "NotificationForm";
            this.Text = "Уведомление";
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlHader.ResumeLayout(false);
            this.pnlHader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.pnlMainContent.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel pnlHader;
        private System.Windows.Forms.CheckBox repeatNotification;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.RichTextBox mainText;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnIgnore;
        private PictureBox pictureBox;
    }
}