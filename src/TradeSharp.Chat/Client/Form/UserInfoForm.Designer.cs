namespace TradeSharp.Chat.Client.Form
{
    partial class UserInfoForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserInfoForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.userInfoFastGrid = new FastGrid.FastGrid();
            this.aboutRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.bigAvatarPanel = new System.Windows.Forms.Panel();
            this.smallAvatarPanel = new System.Windows.Forms.Panel();
            this.contactsListView = new System.Windows.Forms.ListView();
            this.socialNetworksImageList = new System.Windows.Forms.ImageList(this.components);
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(7, 337);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(378, 29);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(300, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(219, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Controls.Add(this.flowLayoutPanel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(7, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(378, 330);
            this.panel1.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(156, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.userInfoFastGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.aboutRichTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(222, 330);
            this.splitContainer1.SplitterDistance = 145;
            this.splitContainer1.TabIndex = 9;
            // 
            // userInfoFastGrid
            // 
            this.userInfoFastGrid.CaptionHeight = 20;
            this.userInfoFastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.userInfoFastGrid.CellHeight = 18;
            this.userInfoFastGrid.CellPadding = 5;
            this.userInfoFastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.userInfoFastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.userInfoFastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.userInfoFastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.userInfoFastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.userInfoFastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.userInfoFastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.userInfoFastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.userInfoFastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userInfoFastGrid.FontAnchoredRow = null;
            this.userInfoFastGrid.FontCell = null;
            this.userInfoFastGrid.FontHeader = null;
            this.userInfoFastGrid.FontSelectedCell = null;
            this.userInfoFastGrid.Location = new System.Drawing.Point(0, 0);
            this.userInfoFastGrid.MinimumTableWidth = null;
            this.userInfoFastGrid.MultiSelectEnabled = false;
            this.userInfoFastGrid.Name = "userInfoFastGrid";
            this.userInfoFastGrid.SelectEnabled = true;
            this.userInfoFastGrid.Size = new System.Drawing.Size(222, 145);
            this.userInfoFastGrid.StickFirst = false;
            this.userInfoFastGrid.StickLast = false;
            this.userInfoFastGrid.TabIndex = 6;
            // 
            // aboutRichTextBox
            // 
            this.aboutRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aboutRichTextBox.Location = new System.Drawing.Point(0, 13);
            this.aboutRichTextBox.Name = "aboutRichTextBox";
            this.aboutRichTextBox.Size = new System.Drawing.Size(222, 168);
            this.aboutRichTextBox.TabIndex = 0;
            this.aboutRichTextBox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Подробная информация";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.bigAvatarPanel);
            this.flowLayoutPanel2.Controls.Add(this.smallAvatarPanel);
            this.flowLayoutPanel2.Controls.Add(this.contactsListView);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(156, 330);
            this.flowLayoutPanel2.TabIndex = 8;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // bigAvatarPanel
            // 
            this.bigAvatarPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.bigAvatarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bigAvatarPanel.Location = new System.Drawing.Point(3, 3);
            this.bigAvatarPanel.Name = "bigAvatarPanel";
            this.bigAvatarPanel.Size = new System.Drawing.Size(100, 100);
            this.bigAvatarPanel.TabIndex = 78;
            // 
            // smallAvatarPanel
            // 
            this.smallAvatarPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.smallAvatarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.smallAvatarPanel.Location = new System.Drawing.Point(3, 109);
            this.smallAvatarPanel.Name = "smallAvatarPanel";
            this.smallAvatarPanel.Size = new System.Drawing.Size(32, 32);
            this.smallAvatarPanel.TabIndex = 79;
            // 
            // contactsListView
            // 
            this.contactsListView.BackColor = System.Drawing.SystemColors.Control;
            this.contactsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.contactsListView.Location = new System.Drawing.Point(3, 147);
            this.contactsListView.MultiSelect = false;
            this.contactsListView.Name = "contactsListView";
            this.contactsListView.Size = new System.Drawing.Size(150, 150);
            this.contactsListView.SmallImageList = this.socialNetworksImageList;
            this.contactsListView.TabIndex = 77;
            this.contactsListView.UseCompatibleStateImageBehavior = false;
            this.contactsListView.View = System.Windows.Forms.View.SmallIcon;
            // 
            // socialNetworksImageList
            // 
            this.socialNetworksImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("socialNetworksImageList.ImageStream")));
            this.socialNetworksImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.socialNetworksImageList.Images.SetKeyName(0, "skype");
            this.socialNetworksImageList.Images.SetKeyName(1, "email");
            this.socialNetworksImageList.Images.SetKeyName(2, "twitter");
            this.socialNetworksImageList.Images.SetKeyName(3, "googleplus");
            this.socialNetworksImageList.Images.SetKeyName(4, "mailru");
            this.socialNetworksImageList.Images.SetKeyName(5, "odnoklassniki");
            this.socialNetworksImageList.Images.SetKeyName(6, "facebook");
            this.socialNetworksImageList.Images.SetKeyName(7, "vk");
            // 
            // UserInfoForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(392, 373);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "UserInfoForm";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Информация о пользователе";
            this.Load += new System.EventHandler(this.UserInfoFormLoad);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FastGrid.FastGrid userInfoFastGrid;
        private System.Windows.Forms.RichTextBox aboutRichTextBox;
        private System.Windows.Forms.ImageList socialNetworksImageList;
        private System.Windows.Forms.Panel bigAvatarPanel;
        private System.Windows.Forms.Panel smallAvatarPanel;
        private System.Windows.Forms.ListView contactsListView;
        private System.Windows.Forms.Label label1;
    }
}