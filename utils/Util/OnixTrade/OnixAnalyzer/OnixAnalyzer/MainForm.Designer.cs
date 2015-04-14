namespace OnixAnalyzer
{
    partial class MainForm
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
            this.tbNumbers = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFolder = new System.Windows.Forms.TextBox();
            this.tbTradersHTML = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnLoadAccounts = new System.Windows.Forms.Button();
            this.btnStat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbNumbers
            // 
            this.tbNumbers.Location = new System.Drawing.Point(12, 101);
            this.tbNumbers.Multiline = true;
            this.tbNumbers.Name = "tbNumbers";
            this.tbNumbers.Size = new System.Drawing.Size(243, 169);
            this.tbNumbers.TabIndex = 0;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 316);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "Закачать";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Номера счетов";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 274);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Каталог сохранения";
            // 
            // tbFolder
            // 
            this.tbFolder.Location = new System.Drawing.Point(12, 290);
            this.tbFolder.Name = "tbFolder";
            this.tbFolder.Size = new System.Drawing.Size(243, 20);
            this.tbFolder.TabIndex = 4;
            // 
            // tbTradersHTML
            // 
            this.tbTradersHTML.Location = new System.Drawing.Point(12, 25);
            this.tbTradersHTML.Name = "tbTradersHTML";
            this.tbTradersHTML.Size = new System.Drawing.Size(243, 20);
            this.tbTradersHTML.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "HTML-файл запроса трейдеров";
            // 
            // btnLoadAccounts
            // 
            this.btnLoadAccounts.Location = new System.Drawing.Point(12, 57);
            this.btnLoadAccounts.Name = "btnLoadAccounts";
            this.btnLoadAccounts.Size = new System.Drawing.Size(130, 23);
            this.btnLoadAccounts.TabIndex = 7;
            this.btnLoadAccounts.Text = "Получить счета";
            this.btnLoadAccounts.UseVisualStyleBackColor = true;
            this.btnLoadAccounts.Click += new System.EventHandler(this.btnLoadAccounts_Click);
            // 
            // btnStat
            // 
            this.btnStat.Location = new System.Drawing.Point(104, 316);
            this.btnStat.Name = "btnStat";
            this.btnStat.Size = new System.Drawing.Size(89, 23);
            this.btnStat.TabIndex = 8;
            this.btnStat.Text = "Статистика...";
            this.btnStat.UseVisualStyleBackColor = true;
            this.btnStat.Click += new System.EventHandler(this.btnStat_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 352);
            this.Controls.Add(this.btnStat);
            this.Controls.Add(this.btnLoadAccounts);
            this.Controls.Add(this.tbTradersHTML);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.tbNumbers);
            this.Name = "MainForm";
            this.Text = "ONIX-ANALYZER";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbNumbers;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFolder;
        private System.Windows.Forms.TextBox tbTradersHTML;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnLoadAccounts;
        private System.Windows.Forms.Button btnStat;
    }
}

