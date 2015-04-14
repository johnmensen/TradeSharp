namespace TradeSharp.NewsServiceGUI
{
    partial class NewsForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.PutNewsBtn = new System.Windows.Forms.Button();
            this.NewsTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // PutNewsBtn
            // 
            this.PutNewsBtn.Location = new System.Drawing.Point(34, 44);
            this.PutNewsBtn.Name = "PutNewsBtn";
            this.PutNewsBtn.Size = new System.Drawing.Size(107, 23);
            this.PutNewsBtn.TabIndex = 0;
            this.PutNewsBtn.Text = "PutNews";
            this.PutNewsBtn.UseVisualStyleBackColor = true;
            this.PutNewsBtn.Click += new System.EventHandler(this.PutNewsBtn_Click);
            // 
            // NewsTextBox
            // 
            this.NewsTextBox.Location = new System.Drawing.Point(34, 145);
            this.NewsTextBox.Multiline = true;
            this.NewsTextBox.Name = "NewsTextBox";
            this.NewsTextBox.Size = new System.Drawing.Size(335, 114);
            this.NewsTextBox.TabIndex = 1;
            // 
            // NewsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 305);
            this.Controls.Add(this.NewsTextBox);
            this.Controls.Add(this.PutNewsBtn);
            this.Name = "NewsForm";
            this.Text = "Новостной портал";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button PutNewsBtn;
        private System.Windows.Forms.TextBox NewsTextBox;
    }
}

