using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Chat.Client.Control
{
    partial class ChatControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatControl));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.logTabPage = new System.Windows.Forms.TabPage();
            this.logRichTextBox = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.roomsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.userTreeView = new System.Windows.Forms.TreeView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.beginPrivateConversationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.showUserInfoItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsButton = new System.Windows.Forms.Button();
            this.onOffButton = new System.Windows.Forms.Button();
            this.roomButton = new System.Windows.Forms.Button();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.logTabPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 377);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(561, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "rooms.png");
            this.imageList.Images.SetKeyName(1, "logout.png");
            this.imageList.Images.SetKeyName(2, "room.png");
            this.imageList.Images.SetKeyName(3, "user.png");
            this.imageList.Images.SetKeyName(4, "message.png");
            this.imageList.Images.SetKeyName(5, "room_bw.png");
            this.imageList.Images.SetKeyName(6, "gear_blue.png");
            this.imageList.Images.SetKeyName(7, "power_on.png");
            this.imageList.Images.SetKeyName(8, "power_off.png");
            this.imageList.Images.SetKeyName(9, "cube.png");
            this.imageList.Images.SetKeyName(10, "cube_bw.png");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl);
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1MinSize = 0;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.userTreeView);
            this.splitContainer1.Panel2.Controls.Add(this.settingsButton);
            this.splitContainer1.Panel2.Controls.Add(this.onOffButton);
            this.splitContainer1.Panel2.Controls.Add(this.roomButton);
            this.splitContainer1.Panel2MinSize = 0;
            this.splitContainer1.Size = new System.Drawing.Size(561, 377);
            this.splitContainer1.SplitterDistance = 408;
            this.splitContainer1.TabIndex = 4;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.logTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ImageList = this.imageList;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(408, 377);
            this.tabControl.TabIndex = 1;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControlSelectedIndexChanged);
            // 
            // logTabPage
            // 
            this.logTabPage.Controls.Add(this.logRichTextBox);
            this.logTabPage.Location = new System.Drawing.Point(4, 23);
            this.logTabPage.Name = "logTabPage";
            this.logTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.logTabPage.Size = new System.Drawing.Size(400, 350);
            this.logTabPage.TabIndex = 0;
            this.logTabPage.Text = "Лог";
            this.logTabPage.UseVisualStyleBackColor = true;
            // 
            // logRichTextBox
            // 
            this.logRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logRichTextBox.Location = new System.Drawing.Point(3, 3);
            this.logRichTextBox.Name = "logRichTextBox";
            this.logRichTextBox.ReadOnly = true;
            this.logRichTextBox.Size = new System.Drawing.Size(394, 344);
            this.logRichTextBox.TabIndex = 0;
            this.logRichTextBox.Text = "";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.roomsLinkLabel, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(408, 377);
            this.tableLayoutPanel1.TabIndex = 1;
            this.tableLayoutPanel1.Visible = false;
            // 
            // roomsLinkLabel
            // 
            this.roomsLinkLabel.AutoSize = true;
            this.roomsLinkLabel.Location = new System.Drawing.Point(173, 182);
            this.roomsLinkLabel.Name = "roomsLinkLabel";
            this.roomsLinkLabel.Size = new System.Drawing.Size(62, 13);
            this.roomsLinkLabel.TabIndex = 0;
            this.roomsLinkLabel.TabStop = true;
            this.roomsLinkLabel.Text = "Комнаты...";
            this.roomsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RoomsLinkLabelLinkClicked);
            // 
            // userTreeView
            // 
            this.userTreeView.ContextMenuStrip = this.contextMenuStrip;
            this.userTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userTreeView.ImageIndex = 0;
            this.userTreeView.ImageList = this.imageList;
            this.userTreeView.Location = new System.Drawing.Point(0, 23);
            this.userTreeView.Name = "userTreeView";
            this.userTreeView.SelectedImageIndex = 0;
            this.userTreeView.Size = new System.Drawing.Size(149, 308);
            this.userTreeView.TabIndex = 5;
            this.userTreeView.DoubleClick += new System.EventHandler(this.BeginPrivateConversationToolStripMenuItemClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginPrivateConversationToolStripMenuItem,
            this.toolStripMenuItem1,
            this.showUserInfoItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(191, 54);
            // 
            // beginPrivateConversationToolStripMenuItem
            // 
            this.beginPrivateConversationToolStripMenuItem.Name = "beginPrivateConversationToolStripMenuItem";
            this.beginPrivateConversationToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.beginPrivateConversationToolStripMenuItem.Text = "Начать диалог";
            this.beginPrivateConversationToolStripMenuItem.Click += new System.EventHandler(this.BeginPrivateConversationToolStripMenuItemClick);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(187, 6);
            // 
            // showUserInfoItem
            // 
            this.showUserInfoItem.Name = "showUserInfoItem";
            this.showUserInfoItem.Size = new System.Drawing.Size(190, 22);
            this.showUserInfoItem.Text = "Показать информацию";
            this.showUserInfoItem.Click += new System.EventHandler(this.UserInfoItemClick);
            // 
            // settingsButton
            // 
            this.settingsButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.settingsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.settingsButton.ImageIndex = 6;
            this.settingsButton.ImageList = this.imageList;
            this.settingsButton.Location = new System.Drawing.Point(0, 331);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(149, 23);
            this.settingsButton.TabIndex = 3;
            this.settingsButton.Text = "Настройки...";
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.SettingsButtonClick);
            // 
            // onOffButton
            // 
            this.onOffButton.AutoSize = true;
            this.onOffButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.onOffButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.onOffButton.ImageIndex = 7;
            this.onOffButton.ImageList = this.imageList;
            this.onOffButton.Location = new System.Drawing.Point(0, 354);
            this.onOffButton.Name = "onOffButton";
            this.onOffButton.Size = new System.Drawing.Size(149, 23);
            this.onOffButton.TabIndex = 4;
            this.onOffButton.Text = "Вход";
            this.onOffButton.UseVisualStyleBackColor = true;
            this.onOffButton.Click += new System.EventHandler(this.OnOffButtonClick);
            // 
            // roomButton
            // 
            this.roomButton.AutoSize = true;
            this.roomButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.roomButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.roomButton.ImageIndex = 0;
            this.roomButton.ImageList = this.imageList;
            this.roomButton.Location = new System.Drawing.Point(0, 0);
            this.roomButton.Name = "roomButton";
            this.roomButton.Size = new System.Drawing.Size(149, 23);
            this.roomButton.TabIndex = 2;
            this.roomButton.Text = "Комнаты...";
            this.roomButton.UseVisualStyleBackColor = true;
            this.roomButton.Click += new System.EventHandler(this.RoomButtonClick);
            // 
            // ChatControl
            // 
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Name = "ChatControl";
            this.Size = new System.Drawing.Size(561, 399);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.logTabPage.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private StatusStrip statusStrip;
        private ImageList imageList;
        private SplitContainer splitContainer1;
        private TabControl tabControl;
        private TreeView userTreeView;
        private Button onOffButton;
        private Button roomButton;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem showUserInfoItem;
        private ToolStripStatusLabel toolStripStatusLabel;
        private TabPage logTabPage;
        private RichTextBox logRichTextBox;
        private TableLayoutPanel tableLayoutPanel1;
        private LinkLabel roomsLinkLabel;
        private Button settingsButton;
        private ToolStripMenuItem beginPrivateConversationToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
    }
}
