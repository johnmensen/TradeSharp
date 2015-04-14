namespace QuoteManager
{
    partial class TickerBaseDataForm
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
            this.dpStart = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dpEnd = new System.Windows.Forms.DateTimePicker();
            this.btnReadStartEnd = new System.Windows.Forms.Button();
            this.btnDeleteInter = new System.Windows.Forms.Button();
            this.btnDeleteFromBegin = new System.Windows.Forms.Button();
            this.btnDeleteToEnd = new System.Windows.Forms.Button();
            this.btnDeleteAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dpStart
            // 
            this.dpStart.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpStart.Location = new System.Drawing.Point(12, 25);
            this.dpStart.Name = "dpStart";
            this.dpStart.Size = new System.Drawing.Size(131, 20);
            this.dpStart.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Начало истории";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Конец истории";
            // 
            // dpEnd
            // 
            this.dpEnd.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpEnd.Location = new System.Drawing.Point(12, 67);
            this.dpEnd.Name = "dpEnd";
            this.dpEnd.Size = new System.Drawing.Size(131, 20);
            this.dpEnd.TabIndex = 2;
            // 
            // btnReadStartEnd
            // 
            this.btnReadStartEnd.Location = new System.Drawing.Point(149, 41);
            this.btnReadStartEnd.Name = "btnReadStartEnd";
            this.btnReadStartEnd.Size = new System.Drawing.Size(75, 23);
            this.btnReadStartEnd.TabIndex = 4;
            this.btnReadStartEnd.Text = "Получить";
            this.btnReadStartEnd.UseVisualStyleBackColor = true;
            this.btnReadStartEnd.Click += new System.EventHandler(this.BtnReadStartEndClick);
            // 
            // btnDeleteInter
            // 
            this.btnDeleteInter.Location = new System.Drawing.Point(12, 105);
            this.btnDeleteInter.Name = "btnDeleteInter";
            this.btnDeleteInter.Size = new System.Drawing.Size(295, 23);
            this.btnDeleteInter.TabIndex = 5;
            this.btnDeleteInter.Text = "Удалить на интервале";
            this.btnDeleteInter.UseVisualStyleBackColor = true;
            this.btnDeleteInter.Click += new System.EventHandler(this.BtnDeleteInterClick);
            // 
            // btnDeleteFromBegin
            // 
            this.btnDeleteFromBegin.Location = new System.Drawing.Point(12, 134);
            this.btnDeleteFromBegin.Name = "btnDeleteFromBegin";
            this.btnDeleteFromBegin.Size = new System.Drawing.Size(295, 23);
            this.btnDeleteFromBegin.TabIndex = 6;
            this.btnDeleteFromBegin.Text = "Удалить от начала до наст. времени";
            this.btnDeleteFromBegin.UseVisualStyleBackColor = true;
            this.btnDeleteFromBegin.Click += new System.EventHandler(this.BtnDeleteFromBeginClick);
            // 
            // btnDeleteToEnd
            // 
            this.btnDeleteToEnd.Location = new System.Drawing.Point(12, 163);
            this.btnDeleteToEnd.Name = "btnDeleteToEnd";
            this.btnDeleteToEnd.Size = new System.Drawing.Size(295, 23);
            this.btnDeleteToEnd.TabIndex = 7;
            this.btnDeleteToEnd.Text = "Удалить до начала";
            this.btnDeleteToEnd.UseVisualStyleBackColor = true;
            this.btnDeleteToEnd.Click += new System.EventHandler(this.BtnDeleteToEndClick);
            // 
            // btnDeleteAll
            // 
            this.btnDeleteAll.Location = new System.Drawing.Point(12, 192);
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.Size = new System.Drawing.Size(295, 23);
            this.btnDeleteAll.TabIndex = 8;
            this.btnDeleteAll.Text = "Удалить все";
            this.btnDeleteAll.UseVisualStyleBackColor = true;
            this.btnDeleteAll.Click += new System.EventHandler(this.BtnDeleteAllClick);
            // 
            // TickerBaseDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 229);
            this.Controls.Add(this.btnDeleteAll);
            this.Controls.Add(this.btnDeleteToEnd);
            this.Controls.Add(this.btnDeleteFromBegin);
            this.Controls.Add(this.btnDeleteInter);
            this.Controls.Add(this.btnReadStartEnd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dpEnd);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dpStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "TickerBaseDataForm";
            this.Text = "Тикер в БД";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dpStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dpEnd;
        private System.Windows.Forms.Button btnReadStartEnd;
        private System.Windows.Forms.Button btnDeleteInter;
        private System.Windows.Forms.Button btnDeleteFromBegin;
        private System.Windows.Forms.Button btnDeleteToEnd;
        private System.Windows.Forms.Button btnDeleteAll;
    }
}