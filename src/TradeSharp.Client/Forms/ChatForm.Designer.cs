namespace TradeSharp.Client.Forms
{
    partial class ChatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            this.chatControl = new TradeSharp.Chat.Client.Control.ChatControl();
            this.SuspendLayout();
            // 
            // chatControl
            // 
            this.chatControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.chatControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatControl.Engine = null;
            this.chatControl.Location = new System.Drawing.Point(0, 0);
            this.chatControl.Name = "chatControl";
            this.chatControl.Size = new System.Drawing.Size(492, 373);
            this.chatControl.TabIndex = 0;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 373);
            this.Controls.Add(this.chatControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChatForm";
            this.Text = "Чат";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChatFormFormClosing);
            this.Load += new System.EventHandler(this.ChatFormLoad);
            this.ResizeEnd += new System.EventHandler(this.ChatFormResizeEnd);
            this.Move += new System.EventHandler(this.ChatFormMove);
            this.ResumeLayout(false);

        }

        #endregion

        private Chat.Client.Control.ChatControl chatControl;
    }
}