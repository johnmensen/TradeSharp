namespace TradeSharp.Client.Forms
{
    partial class NewTextSignalForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewTextSignalForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbTimeframe = new System.Windows.Forms.ComboBox();
            this.cbTicker = new System.Windows.Forms.ComboBox();
            this.cbSignalCat = new System.Windows.Forms.ComboBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.panelTemplate = new System.Windows.Forms.Panel();
            this.cbTemplate = new System.Windows.Forms.ComboBox();
            this.panelTemplateLeft = new System.Windows.Forms.Panel();
            this.btnSaveTemplate = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tbMessage = new System.Windows.Forms.RichTextBox();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelTemplate.SuspendLayout();
            this.panelTemplateLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.cbTimeframe);
            this.panelTop.Controls.Add(this.cbTicker);
            this.panelTop.Controls.Add(this.cbSignalCat);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(350, 87);
            this.panelTop.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(103, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 5;
            this.label3.Tag = "TitleTimeframe";
            this.label3.Text = "Таймфрейм";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 4;
            this.label2.Tag = "TitleInstrument";
            this.label2.Text = "Инструмент";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 3;
            this.label1.Tag = "TitleSignal";
            this.label1.Text = "Сигнал";
            // 
            // cbTimeframe
            // 
            this.cbTimeframe.FormattingEnabled = true;
            this.cbTimeframe.Location = new System.Drawing.Point(106, 57);
            this.cbTimeframe.Name = "cbTimeframe";
            this.cbTimeframe.Size = new System.Drawing.Size(86, 21);
            this.cbTimeframe.TabIndex = 2;
            // 
            // cbTicker
            // 
            this.cbTicker.FormattingEnabled = true;
            this.cbTicker.Location = new System.Drawing.Point(6, 57);
            this.cbTicker.Name = "cbTicker";
            this.cbTicker.Size = new System.Drawing.Size(90, 21);
            this.cbTicker.TabIndex = 1;
            // 
            // cbSignalCat
            // 
            this.cbSignalCat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSignalCat.FormattingEnabled = true;
            this.cbSignalCat.Location = new System.Drawing.Point(3, 17);
            this.cbSignalCat.Name = "cbSignalCat";
            this.cbSignalCat.Size = new System.Drawing.Size(306, 21);
            this.cbSignalCat.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnSend);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 309);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(350, 34);
            this.panelBottom.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(121, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(86, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSend
            // 
            this.btnSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSend.Location = new System.Drawing.Point(12, 6);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(84, 23);
            this.btnSend.TabIndex = 0;
            this.btnSend.Tag = "TitleSend";
            this.btnSend.Text = "Отправить";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.BtnSendClick);
            // 
            // panelTemplate
            // 
            this.panelTemplate.Controls.Add(this.cbTemplate);
            this.panelTemplate.Controls.Add(this.panelTemplateLeft);
            this.panelTemplate.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTemplate.Location = new System.Drawing.Point(0, 87);
            this.panelTemplate.Name = "panelTemplate";
            this.panelTemplate.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.panelTemplate.Size = new System.Drawing.Size(350, 26);
            this.panelTemplate.TabIndex = 2;
            // 
            // cbTemplate
            // 
            this.cbTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTemplate.FormattingEnabled = true;
            this.cbTemplate.Location = new System.Drawing.Point(3, 0);
            this.cbTemplate.Name = "cbTemplate";
            this.cbTemplate.Size = new System.Drawing.Size(306, 21);
            this.cbTemplate.Sorted = true;
            this.cbTemplate.TabIndex = 1;
            this.cbTemplate.SelectedIndexChanged += new System.EventHandler(this.CbTemplateSelectedIndexChanged);
            // 
            // panelTemplateLeft
            // 
            this.panelTemplateLeft.Controls.Add(this.btnSaveTemplate);
            this.panelTemplateLeft.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelTemplateLeft.Location = new System.Drawing.Point(309, 0);
            this.panelTemplateLeft.Name = "panelTemplateLeft";
            this.panelTemplateLeft.Size = new System.Drawing.Size(41, 26);
            this.panelTemplateLeft.TabIndex = 0;
            // 
            // btnSaveTemplate
            // 
            this.btnSaveTemplate.ImageIndex = 0;
            this.btnSaveTemplate.ImageList = this.imageList;
            this.btnSaveTemplate.Location = new System.Drawing.Point(6, -1);
            this.btnSaveTemplate.Name = "btnSaveTemplate";
            this.btnSaveTemplate.Size = new System.Drawing.Size(27, 23);
            this.btnSaveTemplate.TabIndex = 0;
            this.btnSaveTemplate.UseVisualStyleBackColor = true;
            this.btnSaveTemplate.Click += new System.EventHandler(this.BtnSaveTemplateClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico save.png");
            // 
            // tbMessage
            // 
            this.tbMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbMessage.Location = new System.Drawing.Point(0, 113);
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.Size = new System.Drawing.Size(350, 196);
            this.tbMessage.TabIndex = 3;
            this.tbMessage.Text = "";
            this.tbMessage.TextChanged += new System.EventHandler(this.TbMessageTextChanged);
            this.tbMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TbMessageKeyDown);
            this.tbMessage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TbMessageKeyPress);
            // 
            // NewTextSignalForm
            // 
            this.AcceptButton = this.btnSend;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 343);
            this.Controls.Add(this.tbMessage);
            this.Controls.Add(this.panelTemplate);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewTextSignalForm";
            this.Tag = "TitleTextSignalMessage";
            this.Text = "Сообщение подписчикам";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelTemplate.ResumeLayout(false);
            this.panelTemplateLeft.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbTimeframe;
        private System.Windows.Forms.ComboBox cbTicker;
        private System.Windows.Forms.ComboBox cbSignalCat;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Panel panelTemplate;
        private System.Windows.Forms.ComboBox cbTemplate;
        private System.Windows.Forms.Panel panelTemplateLeft;
        private System.Windows.Forms.Button btnSaveTemplate;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.RichTextBox tbMessage;
    }
}