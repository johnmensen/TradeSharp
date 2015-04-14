namespace TradeSharp.Client.Forms
{
    partial class ExportSetupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportSetupForm));
            this.cbEncoding = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTimeFormat = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbFloatPoint = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbColumnSeparator = new System.Windows.Forms.ComboBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbEncoding
            // 
            this.cbEncoding.FormattingEnabled = true;
            this.cbEncoding.Location = new System.Drawing.Point(12, 12);
            this.cbEncoding.Name = "cbEncoding";
            this.cbEncoding.Size = new System.Drawing.Size(121, 21);
            this.cbEncoding.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(139, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "кодировка";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(139, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "формат времени";
            // 
            // tbTimeFormat
            // 
            this.tbTimeFormat.Location = new System.Drawing.Point(12, 39);
            this.tbTimeFormat.Name = "tbTimeFormat";
            this.tbTimeFormat.Size = new System.Drawing.Size(121, 20);
            this.tbTimeFormat.TabIndex = 4;
            this.tbTimeFormat.Text = "dd.MM.yyyy HH:mm:ss";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(139, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "разделитель дроби";
            // 
            // cbFloatPoint
            // 
            this.cbFloatPoint.FormattingEnabled = true;
            this.cbFloatPoint.Items.AddRange(new object[] {
            "Точка",
            "Запятая"});
            this.cbFloatPoint.Location = new System.Drawing.Point(12, 65);
            this.cbFloatPoint.Name = "cbFloatPoint";
            this.cbFloatPoint.Size = new System.Drawing.Size(121, 21);
            this.cbFloatPoint.TabIndex = 6;
            this.cbFloatPoint.Text = "Точка";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(139, 95);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "разделитель столбцов";
            // 
            // cbColumnSeparator
            // 
            this.cbColumnSeparator.FormattingEnabled = true;
            this.cbColumnSeparator.Items.AddRange(new object[] {
            "TAB",
            "Пробел",
            ";",
            ","});
            this.cbColumnSeparator.Location = new System.Drawing.Point(12, 92);
            this.cbColumnSeparator.Name = "cbColumnSeparator";
            this.cbColumnSeparator.Size = new System.Drawing.Size(121, 21);
            this.cbColumnSeparator.TabIndex = 8;
            this.cbColumnSeparator.Text = "TAB";
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(12, 143);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 10;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(115, 143);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "Отмена";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // ExportSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 174);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbColumnSeparator);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbFloatPoint);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbTimeFormat);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbEncoding);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportSetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки экспорта";
            this.Load += new System.EventHandler(this.ExportSetupForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbEncoding;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTimeFormat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbFloatPoint;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbColumnSeparator;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button button2;
    }
}