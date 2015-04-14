using System.Windows.Forms;

namespace FIXViewer
{
    partial class UpdateBaseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateBaseForm));
            this.tbURL = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.labelControl1 = new System.Windows.Forms.Label();
            this.tbNumStart = new System.Windows.Forms.TextBox();
            this.tbNumEnd = new System.Windows.Forms.TextBox();
            this.labelControl3 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.tbURLMsgType = new System.Windows.Forms.TextBox();
            this.labelControl2 = new System.Windows.Forms.Label();
            this.labelControl4 = new System.Windows.Forms.Label();
            this.labelControl5 = new System.Windows.Forms.Label();
            this.tbMessages = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbURL
            // 
            this.tbURL.Location = new System.Drawing.Point(12, 26);
            this.tbURL.Name = "tbURL";
            this.tbURL.Size = new System.Drawing.Size(397, 20);
            this.tbURL.TabIndex = 0;
            this.tbURL.Text = "http://www.onixs.biz/tools/fixdictionary/4.4/tagNum_<num>.html";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 201);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Обновить";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(417, 29);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(31, 13);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "URL";
            // 
            // tbNumStart
            // 
            this.tbNumStart.Location = new System.Drawing.Point(12, 52);
            this.tbNumStart.Name = "tbNumStart";
            this.tbNumStart.Size = new System.Drawing.Size(32, 20);
            this.tbNumStart.TabIndex = 3;
            this.tbNumStart.Text = "1";
            // 
            // tbNumEnd
            // 
            this.tbNumEnd.Location = new System.Drawing.Point(62, 52);
            this.tbNumEnd.Name = "tbNumEnd";
            this.tbNumEnd.Size = new System.Drawing.Size(34, 20);
            this.tbNumEnd.TabIndex = 4;
            this.tbNumEnd.Text = "956";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(50, 55);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(4, 13);
            this.labelControl3.TabIndex = 6;
            this.labelControl3.Text = "-";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(162, 201);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Закрыть";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // tbURLMsgType
            // 
            this.tbURLMsgType.Location = new System.Drawing.Point(12, 115);
            this.tbURLMsgType.Name = "tbURLMsgType";
            this.tbURLMsgType.Size = new System.Drawing.Size(397, 20);
            this.tbURLMsgType.TabIndex = 8;
            this.tbURLMsgType.Text = "http://www.onixs.biz/tools/fixdictionary/4.4/msgType_<tag>_<tagcode>.html";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(417, 118);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(31, 13);
            this.labelControl2.TabIndex = 9;
            this.labelControl2.Text = "URL";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(12, 7);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(25, 13);
            this.labelControl4.TabIndex = 10;
            this.labelControl4.Text = "Тэги";
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(12, 96);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(65, 13);
            this.labelControl5.TabIndex = 11;
            this.labelControl5.Text = "Сообщения";
            // 
            // tbMessages
            // 
            this.tbMessages.Location = new System.Drawing.Point(12, 141);
            this.tbMessages.Name = "tbMessages";
            this.tbMessages.Size = new System.Drawing.Size(397, 20);
            this.tbMessages.TabIndex = 13;
            this.tbMessages.Text = resources.GetString("tbMessages.Text");
            // 
            // UpdateBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 243);
            this.Controls.Add(this.tbMessages);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.tbURLMsgType);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.tbNumEnd);
            this.Controls.Add(this.tbNumStart);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.tbURL);
            this.Name = "UpdateBaseForm";
            this.Text = "Обновить базу";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox tbURL;
        private Button btnStart;
        private Label labelControl1;
        private TextBox tbNumStart;
        private TextBox tbNumEnd;
        private Label labelControl3;
        private Button btnClose;
        private TextBox tbURLMsgType;
        private Label labelControl2;
        private Label labelControl4;
        private Label labelControl5;
        private TextBox tbMessages;
    }
}