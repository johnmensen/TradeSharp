namespace Candlechart.Controls
{
    partial class PublishForecastWindow
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
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbTicker = new System.Windows.Forms.ComboBox();
            this.tbComment = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSignal = new System.Windows.Forms.ComboBox();
            this.cbSubscribers = new System.Windows.Forms.CheckBox();
            this.cbPublishing = new System.Windows.Forms.CheckBox();
            this.cbTypePublishing = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(12, 207);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(83, 23);
            this.btnPublish.TabIndex = 0;
            this.btnPublish.Text = "Разместить";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.BtnPublishClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(118, 207);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(83, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cbTicker
            // 
            this.cbTicker.FormattingEnabled = true;
            this.cbTicker.Location = new System.Drawing.Point(12, 11);
            this.cbTicker.Name = "cbTicker";
            this.cbTicker.Size = new System.Drawing.Size(97, 21);
            this.cbTicker.TabIndex = 3;
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(12, 68);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(303, 133);
            this.tbComment.TabIndex = 4;
            this.tbComment.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "\"сигнал\"";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "тикер";
            // 
            // tbSignal
            // 
            this.tbSignal.FormattingEnabled = true;
            this.tbSignal.Items.AddRange(new object[] {
            "4H",
            "Daily",
            "Weekly"});
            this.tbSignal.Location = new System.Drawing.Point(12, 38);
            this.tbSignal.Name = "tbSignal";
            this.tbSignal.Size = new System.Drawing.Size(97, 21);
            this.tbSignal.TabIndex = 8;
            // 
            // cbSubscribers
            // 
            this.cbSubscribers.AutoSize = true;
            this.cbSubscribers.Checked = true;
            this.cbSubscribers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSubscribers.Location = new System.Drawing.Point(249, 38);
            this.cbSubscribers.Name = "cbSubscribers";
            this.cbSubscribers.Size = new System.Drawing.Size(74, 17);
            this.cbSubscribers.TabIndex = 9;
            this.cbSubscribers.Text = "sms e-mail";
            this.cbSubscribers.UseVisualStyleBackColor = true;
            // 
            // cbPublishing
            // 
            this.cbPublishing.AutoSize = true;
            this.cbPublishing.Checked = true;
            this.cbPublishing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbPublishing.Location = new System.Drawing.Point(179, 38);
            this.cbPublishing.Name = "cbPublishing";
            this.cbPublishing.Size = new System.Drawing.Size(64, 17);
            this.cbPublishing.TabIndex = 10;
            this.cbPublishing.Text = "на сайт";
            this.cbPublishing.UseVisualStyleBackColor = true;
            // 
            // cbTypePublishing
            // 
            this.cbTypePublishing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTypePublishing.FormattingEnabled = true;
            this.cbTypePublishing.Items.AddRange(new object[] {
            "Прогноз",
            "Предвар. сигнал"});
            this.cbTypePublishing.Location = new System.Drawing.Point(179, 11);
            this.cbTypePublishing.MaxDropDownItems = 2;
            this.cbTypePublishing.Name = "cbTypePublishing";
            this.cbTypePublishing.Size = new System.Drawing.Size(136, 21);
            this.cbTypePublishing.TabIndex = 11;
            this.cbTypePublishing.SelectedIndexChanged += new System.EventHandler(this.CbTypePublishingSelectedIndexChanged);
            // 
            // PublishForecastWindow
            // 
            this.AcceptButton = this.btnPublish;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(324, 236);
            this.Controls.Add(this.cbTypePublishing);
            this.Controls.Add(this.cbPublishing);
            this.Controls.Add(this.cbSubscribers);
            this.Controls.Add(this.tbSignal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.cbTicker);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPublish);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PublishForecastWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Разместить публикацию";
            this.Load += new System.EventHandler(this.PublishForecastWindowLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cbTicker;
        private System.Windows.Forms.RichTextBox tbComment;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox tbSignal;
        private System.Windows.Forms.CheckBox cbSubscribers;
        private System.Windows.Forms.CheckBox cbPublishing;
        private System.Windows.Forms.ComboBox cbTypePublishing;
    }
}