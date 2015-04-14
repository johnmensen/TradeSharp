namespace Candlechart.Controls
{
    partial class PublishOpenPositionWindow
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
            this.label5 = new System.Windows.Forms.Label();
            this.dtTime = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPrice = new System.Windows.Forms.TextBox();
            this.cbTypeOrder = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.RichTextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPublish = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbStopLoss = new System.Windows.Forms.CheckBox();
            this.tbStopLoss = new System.Windows.Forms.TextBox();
            this.tbTakeProfit = new System.Windows.Forms.TextBox();
            this.cbTakeProfit = new System.Windows.Forms.CheckBox();
            this.cbTicker = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(150, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "Комментарий:";
            // 
            // dtTime
            // 
            this.dtTime.CustomFormat = "dd.MM.yyy HH:mm";
            this.dtTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtTime.Location = new System.Drawing.Point(254, 39);
            this.dtTime.Name = "dtTime";
            this.dtTime.ShowUpDown = true;
            this.dtTime.Size = new System.Drawing.Size(109, 20);
            this.dtTime.TabIndex = 37;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(179, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 36;
            this.label4.Text = "Время входа:";
            // 
            // tbPrice
            // 
            this.tbPrice.Location = new System.Drawing.Point(275, 8);
            this.tbPrice.Name = "tbPrice";
            this.tbPrice.Size = new System.Drawing.Size(88, 20);
            this.tbPrice.TabIndex = 35;
            // 
            // cbTypeOrder
            // 
            this.cbTypeOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTypeOrder.FormattingEnabled = true;
            this.cbTypeOrder.Items.AddRange(new object[] {
            "Buy",
            "Sell"});
            this.cbTypeOrder.Location = new System.Drawing.Point(85, 36);
            this.cbTypeOrder.Name = "cbTypeOrder";
            this.cbTypeOrder.Size = new System.Drawing.Size(88, 21);
            this.cbTypeOrder.TabIndex = 34;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Ордер:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Тикер:";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(10, 116);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(353, 37);
            this.tbComment.TabIndex = 31;
            this.tbComment.Text = "";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(99, 159);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(83, 23);
            this.btnCancel.TabIndex = 29;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(10, 159);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(83, 23);
            this.btnPublish.TabIndex = 28;
            this.btnPublish.Text = "Разместить";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(200, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 43;
            this.label3.Text = "Цена входа:";
            // 
            // cbStopLoss
            // 
            this.cbStopLoss.AutoSize = true;
            this.cbStopLoss.Location = new System.Drawing.Point(13, 69);
            this.cbStopLoss.Name = "cbStopLoss";
            this.cbStopLoss.Size = new System.Drawing.Size(73, 17);
            this.cbStopLoss.TabIndex = 44;
            this.cbStopLoss.Text = "StopLoss:";
            this.cbStopLoss.UseVisualStyleBackColor = true;
            // 
            // tbStopLoss
            // 
            this.tbStopLoss.Location = new System.Drawing.Point(85, 67);
            this.tbStopLoss.Name = "tbStopLoss";
            this.tbStopLoss.Size = new System.Drawing.Size(88, 20);
            this.tbStopLoss.TabIndex = 45;
            // 
            // tbTakeProfit
            // 
            this.tbTakeProfit.Location = new System.Drawing.Point(275, 67);
            this.tbTakeProfit.Name = "tbTakeProfit";
            this.tbTakeProfit.Size = new System.Drawing.Size(88, 20);
            this.tbTakeProfit.TabIndex = 47;
            // 
            // cbTakeProfit
            // 
            this.cbTakeProfit.AutoSize = true;
            this.cbTakeProfit.Location = new System.Drawing.Point(191, 69);
            this.cbTakeProfit.Name = "cbTakeProfit";
            this.cbTakeProfit.Size = new System.Drawing.Size(78, 17);
            this.cbTakeProfit.TabIndex = 46;
            this.cbTakeProfit.Text = "TakeProfit:";
            this.cbTakeProfit.UseVisualStyleBackColor = true;
            // 
            // cbTicker
            // 
            this.cbTicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTicker.FormattingEnabled = true;
            this.cbTicker.Location = new System.Drawing.Point(85, 5);
            this.cbTicker.Name = "cbTicker";
            this.cbTicker.Size = new System.Drawing.Size(88, 21);
            this.cbTicker.TabIndex = 30;
            // 
            // PublishOpenPositionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(372, 188);
            this.Controls.Add(this.tbTakeProfit);
            this.Controls.Add(this.cbTakeProfit);
            this.Controls.Add(this.tbStopLoss);
            this.Controls.Add(this.cbStopLoss);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dtTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbPrice);
            this.Controls.Add(this.cbTypeOrder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.cbTicker);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPublish);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PublishOpenPositionWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Опубликовать открытие позиции";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPrice;
        private System.Windows.Forms.ComboBox cbTypeOrder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox tbComment;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbStopLoss;
        private System.Windows.Forms.TextBox tbStopLoss;
        private System.Windows.Forms.TextBox tbTakeProfit;
        private System.Windows.Forms.CheckBox cbTakeProfit;
        private System.Windows.Forms.ComboBox cbTicker;
    }
}