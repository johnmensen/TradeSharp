namespace TradeSharp.QuoteAdmin.Forms
{
    partial class MakeCandlesFromBidAskForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MakeCandlesFromBidAskForm));
            this.btnLoadCandlesToFile = new System.Windows.Forms.Button();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.tbTicker = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoadFileToDb = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFillAuto = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLoadCandlesToFile
            // 
            this.btnLoadCandlesToFile.Location = new System.Drawing.Point(273, 48);
            this.btnLoadCandlesToFile.Name = "btnLoadCandlesToFile";
            this.btnLoadCandlesToFile.Size = new System.Drawing.Size(75, 23);
            this.btnLoadCandlesToFile.TabIndex = 0;
            this.btnLoadCandlesToFile.Text = "БД -> файл";
            this.btnLoadCandlesToFile.UseVisualStyleBackColor = true;
            this.btnLoadCandlesToFile.Click += new System.EventHandler(this.BtnLoadCandlesToFileClick);
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(12, 77);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(336, 20);
            this.tbFilePath.TabIndex = 1;
            // 
            // tbTicker
            // 
            this.tbTicker.Location = new System.Drawing.Point(53, 12);
            this.tbTicker.Name = "tbTicker";
            this.tbTicker.Size = new System.Drawing.Size(295, 20);
            this.tbTicker.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "тикер";
            // 
            // btnLoadFileToDb
            // 
            this.btnLoadFileToDb.Location = new System.Drawing.Point(273, 103);
            this.btnLoadFileToDb.Name = "btnLoadFileToDb";
            this.btnLoadFileToDb.Size = new System.Drawing.Size(75, 23);
            this.btnLoadFileToDb.TabIndex = 4;
            this.btnLoadFileToDb.Text = "файл -> БД";
            this.btnLoadFileToDb.UseVisualStyleBackColor = true;
            this.btnLoadFileToDb.Click += new System.EventHandler(this.BtnLoadFileToDbClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Каталог";
            // 
            // btnFillAuto
            // 
            this.btnFillAuto.Location = new System.Drawing.Point(192, 103);
            this.btnFillAuto.Name = "btnFillAuto";
            this.btnFillAuto.Size = new System.Drawing.Size(75, 23);
            this.btnFillAuto.TabIndex = 6;
            this.btnFillAuto.Text = "Авто";
            this.btnFillAuto.UseVisualStyleBackColor = true;
            this.btnFillAuto.Click += new System.EventHandler(this.BtnFillAutoClick);
            // 
            // MakeCandlesFromBidAskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 135);
            this.Controls.Add(this.btnFillAuto);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnLoadFileToDb);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbTicker);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.btnLoadCandlesToFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MakeCandlesFromBidAskForm";
            this.Text = "Сформировать свечи m1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoadCandlesToFile;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.TextBox tbTicker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoadFileToDb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnFillAuto;
    }
}