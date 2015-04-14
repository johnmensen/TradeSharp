namespace IndexSpectrum
{
    partial class SynthCurxForm
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
            this.btnBrowse = new System.Windows.Forms.Button();
            this.tbFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbEur = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbUsd = new System.Windows.Forms.TextBox();
            this.tbBars = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbIndexType = new System.Windows.Forms.CheckBox();
            this.btnFormQuote = new System.Windows.Forms.Button();
            this.tbSession = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbMultiplier = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSaveInDb = new System.Windows.Forms.Button();
            this.tbDbCurrencyCode = new System.Windows.Forms.TextBox();
            this.tbDbMakerCode = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(306, 20);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 21);
            this.btnBrowse.TabIndex = 18;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // tbFolder
            // 
            this.tbFolder.Location = new System.Drawing.Point(3, 21);
            this.tbFolder.Name = "tbFolder";
            this.tbFolder.Size = new System.Drawing.Size(297, 20);
            this.tbFolder.TabIndex = 17;
            this.tbFolder.Text = "D:\\index.quote";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Путь к файлу результата";
            // 
            // tbEur
            // 
            this.tbEur.Location = new System.Drawing.Point(4, 80);
            this.tbEur.Name = "tbEur";
            this.tbEur.Size = new System.Drawing.Size(457, 20);
            this.tbEur.TabIndex = 20;
            this.tbEur.Text = "eur0.3155 eurgbp0.3056 eurjpy0.1891 eurchf0.1113 eursek0.0785";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "EURX";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "USDX";
            // 
            // tbUsd
            // 
            this.tbUsd.Location = new System.Drawing.Point(4, 119);
            this.tbUsd.Name = "tbUsd";
            this.tbUsd.Size = new System.Drawing.Size(457, 20);
            this.tbUsd.TabIndex = 22;
            this.tbUsd.Text = "eur0.576# jpy0.136 gbp0.119# cad0.091 sek0.042 chf0.036";
            // 
            // tbBars
            // 
            this.tbBars.Location = new System.Drawing.Point(4, 145);
            this.tbBars.Name = "tbBars";
            this.tbBars.Size = new System.Drawing.Size(75, 20);
            this.tbBars.TabIndex = 25;
            this.tbBars.Text = "20";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(85, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "баров для оценки движения";
            // 
            // cbIndexType
            // 
            this.cbIndexType.AutoSize = true;
            this.cbIndexType.Location = new System.Drawing.Point(4, 171);
            this.cbIndexType.Name = "cbIndexType";
            this.cbIndexType.Size = new System.Drawing.Size(87, 17);
            this.cbIndexType.TabIndex = 27;
            this.cbIndexType.Text = "аддитивный";
            this.cbIndexType.UseVisualStyleBackColor = true;
            // 
            // btnFormQuote
            // 
            this.btnFormQuote.Location = new System.Drawing.Point(3, 264);
            this.btnFormQuote.Name = "btnFormQuote";
            this.btnFormQuote.Size = new System.Drawing.Size(98, 23);
            this.btnFormQuote.TabIndex = 28;
            this.btnFormQuote.Text = "Формировать";
            this.btnFormQuote.UseVisualStyleBackColor = true;
            this.btnFormQuote.Click += new System.EventHandler(this.btnFormQuote_Click);
            // 
            // tbSession
            // 
            this.tbSession.Location = new System.Drawing.Point(241, 145);
            this.tbSession.Name = "tbSession";
            this.tbSession.Size = new System.Drawing.Size(68, 20);
            this.tbSession.TabIndex = 29;
            this.tbSession.Text = "1 1 : 6 1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(315, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "сессия (день-час)";
            // 
            // tbMultiplier
            // 
            this.tbMultiplier.Location = new System.Drawing.Point(97, 171);
            this.tbMultiplier.Name = "tbMultiplier";
            this.tbMultiplier.Size = new System.Drawing.Size(212, 20);
            this.tbMultiplier.TabIndex = 31;
            this.tbMultiplier.Text = "34.38805726 50.14348112 1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(315, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "множитель";
            // 
            // btnSaveInDb
            // 
            this.btnSaveInDb.Location = new System.Drawing.Point(150, 264);
            this.btnSaveInDb.Name = "btnSaveInDb";
            this.btnSaveInDb.Size = new System.Drawing.Size(98, 23);
            this.btnSaveInDb.TabIndex = 33;
            this.btnSaveInDb.Text = "Сохранить в БД";
            this.btnSaveInDb.UseVisualStyleBackColor = true;
            this.btnSaveInDb.Click += new System.EventHandler(this.btnSaveInDb_Click);
            // 
            // tbDbCurrencyCode
            // 
            this.tbDbCurrencyCode.Location = new System.Drawing.Point(150, 215);
            this.tbDbCurrencyCode.Name = "tbDbCurrencyCode";
            this.tbDbCurrencyCode.Size = new System.Drawing.Size(75, 20);
            this.tbDbCurrencyCode.TabIndex = 34;
            this.tbDbCurrencyCode.Text = "10";
            // 
            // tbDbMakerCode
            // 
            this.tbDbMakerCode.Location = new System.Drawing.Point(150, 238);
            this.tbDbMakerCode.Name = "tbDbMakerCode";
            this.tbDbMakerCode.Size = new System.Drawing.Size(75, 20);
            this.tbDbMakerCode.TabIndex = 35;
            this.tbDbMakerCode.Text = "10";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(231, 222);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "код тикера";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(231, 245);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 13);
            this.label8.TabIndex = 37;
            this.label8.Text = "код поставщика";
            // 
            // SynthCurxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 290);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbDbMakerCode);
            this.Controls.Add(this.tbDbCurrencyCode);
            this.Controls.Add(this.btnSaveInDb);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbMultiplier);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbSession);
            this.Controls.Add(this.btnFormQuote);
            this.Controls.Add(this.cbIndexType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbBars);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbUsd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbEur);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.tbFolder);
            this.Name = "SynthCurxForm";
            this.Text = "Тест синтетического индекса";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbEur;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbUsd;
        private System.Windows.Forms.TextBox tbBars;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbIndexType;
        private System.Windows.Forms.Button btnFormQuote;
        private System.Windows.Forms.TextBox tbSession;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbMultiplier;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSaveInDb;
        private System.Windows.Forms.TextBox tbDbCurrencyCode;
        private System.Windows.Forms.TextBox tbDbMakerCode;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}