using TradeSharp.Chat.Client.Control;

namespace TradeSharp.Chat.StandAloneClient
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
            this.chatControl1 = new TradeSharp.Chat.Client.Control.ChatControl();
            this.SuspendLayout();
            // 
            // chatControl1
            // 
            this.chatControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.chatControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatControl1.Engine = null;
            this.chatControl1.Location = new System.Drawing.Point(0, 0);
            this.chatControl1.Name = "chatControl1";
            this.chatControl1.Size = new System.Drawing.Size(697, 498);
            this.chatControl1.TabIndex = 0;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 498);
            this.Controls.Add(this.chatControl1);
            this.Name = "ChatForm";
            this.Text = "Chat-FXI-Client";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChatFormFormClosed);
            this.Shown += new System.EventHandler(this.ChatFormShown);
            this.ResumeLayout(false);

        }

        #endregion

        private ChatControl chatControl1;


    }
}