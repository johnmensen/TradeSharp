namespace TradeSharp.Chat.Client.Control
{
    partial class ChatMessagingControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatMessagingControl));
            this.allMessagesRichTextBox = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.myMessageTextBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.sendButton = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.leaveRoomButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // allMessagesRichTextBox
            // 
            this.allMessagesRichTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.allMessagesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.allMessagesRichTextBox.Location = new System.Drawing.Point(0, 0);
            this.allMessagesRichTextBox.Name = "allMessagesRichTextBox";
            this.allMessagesRichTextBox.ReadOnly = true;
            this.allMessagesRichTextBox.Size = new System.Drawing.Size(574, 404);
            this.allMessagesRichTextBox.TabIndex = 0;
            this.allMessagesRichTextBox.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.allMessagesRichTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.myMessageTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(574, 495);
            this.splitContainer1.SplitterDistance = 404;
            this.splitContainer1.TabIndex = 2;
            // 
            // myMessageTextBox
            // 
            this.myMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myMessageTextBox.Location = new System.Drawing.Point(0, 0);
            this.myMessageTextBox.Multiline = true;
            this.myMessageTextBox.Name = "myMessageTextBox";
            this.myMessageTextBox.Size = new System.Drawing.Size(545, 87);
            this.myMessageTextBox.TabIndex = 3;
            this.myMessageTextBox.TextChanged += new System.EventHandler(this.MyMessageTextBoxTextChanged);
            this.myMessageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MyMessageTextBoxKeyPress);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.sendButton);
            this.flowLayoutPanel1.Controls.Add(this.leaveRoomButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(545, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(29, 87);
            this.flowLayoutPanel1.TabIndex = 2;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // sendButton
            // 
            this.sendButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.sendButton.Enabled = false;
            this.sendButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sendButton.ImageIndex = 0;
            this.sendButton.ImageList = this.imageList;
            this.sendButton.Location = new System.Drawing.Point(3, 3);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(23, 23);
            this.sendButton.TabIndex = 0;
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.SendButtonClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "send.png");
            this.imageList.Images.SetKeyName(1, "exit.png");
            // 
            // leaveRoomButton
            // 
            this.leaveRoomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.leaveRoomButton.ImageIndex = 1;
            this.leaveRoomButton.ImageList = this.imageList;
            this.leaveRoomButton.Location = new System.Drawing.Point(3, 32);
            this.leaveRoomButton.Name = "leaveRoomButton";
            this.leaveRoomButton.Size = new System.Drawing.Size(23, 23);
            this.leaveRoomButton.TabIndex = 1;
            this.leaveRoomButton.UseVisualStyleBackColor = true;
            this.leaveRoomButton.Click += new System.EventHandler(this.LeaveRoomButtonClick);
            // 
            // ChatMessagingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ChatMessagingControl";
            this.Size = new System.Drawing.Size(574, 495);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox allMessagesRichTextBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Button leaveRoomButton;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TextBox myMessageTextBox;
    }
}
