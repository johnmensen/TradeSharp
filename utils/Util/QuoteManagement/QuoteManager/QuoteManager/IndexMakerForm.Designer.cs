namespace QuoteManager
{
    partial class IndexMakerForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.tbTickerFormula = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelControl = new System.Windows.Forms.Panel();
            this.btnShowLastDBQuote = new System.Windows.Forms.Button();
            this.btnSaveInDB = new System.Windows.Forms.Button();
            this.btnSelectTicker = new System.Windows.Forms.Button();
            this.tbTicker = new System.Windows.Forms.TextBox();
            this.btnMakeIndex = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.dpEnd = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dpStart = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTickerId = new System.Windows.Forms.TextBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tbExistingTickers = new System.Windows.Forms.RichTextBox();
            this.panelLeftTop = new System.Windows.Forms.Panel();
            this.btnCheckExisting = new System.Windows.Forms.Button();
            this.rtbHelper = new System.Windows.Forms.RichTextBox();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnShowDbQuotes = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelLeftTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.tbTickerFormula);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.panelControl);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(661, 149);
            this.panelTop.TabIndex = 0;
            // 
            // tbTickerFormula
            // 
            this.tbTickerFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbTickerFormula.Location = new System.Drawing.Point(274, 13);
            this.tbTickerFormula.Name = "tbTickerFormula";
            this.tbTickerFormula.Size = new System.Drawing.Size(387, 136);
            this.tbTickerFormula.TabIndex = 2;
            this.tbTickerFormula.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(274, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Формула";
            // 
            // panelControl
            // 
            this.panelControl.Controls.Add(this.btnShowDbQuotes);
            this.panelControl.Controls.Add(this.btnShowLastDBQuote);
            this.panelControl.Controls.Add(this.btnSaveInDB);
            this.panelControl.Controls.Add(this.btnSelectTicker);
            this.panelControl.Controls.Add(this.tbTicker);
            this.panelControl.Controls.Add(this.btnMakeIndex);
            this.panelControl.Controls.Add(this.label4);
            this.panelControl.Controls.Add(this.dpEnd);
            this.panelControl.Controls.Add(this.label3);
            this.panelControl.Controls.Add(this.dpStart);
            this.panelControl.Controls.Add(this.label2);
            this.panelControl.Controls.Add(this.tbTickerId);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelControl.Location = new System.Drawing.Point(0, 0);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(274, 149);
            this.panelControl.TabIndex = 0;
            // 
            // btnShowLastDBQuote
            // 
            this.btnShowLastDBQuote.Location = new System.Drawing.Point(3, 54);
            this.btnShowLastDBQuote.Name = "btnShowLastDBQuote";
            this.btnShowLastDBQuote.Size = new System.Drawing.Size(139, 22);
            this.btnShowLastDBQuote.TabIndex = 12;
            this.btnShowLastDBQuote.Text = "последня запись БД >";
            this.btnShowLastDBQuote.UseVisualStyleBackColor = true;
            this.btnShowLastDBQuote.Click += new System.EventHandler(this.BtnShowLastDbQuoteClick);
            // 
            // btnSaveInDB
            // 
            this.btnSaveInDB.Location = new System.Drawing.Point(113, 107);
            this.btnSaveInDB.Name = "btnSaveInDB";
            this.btnSaveInDB.Size = new System.Drawing.Size(104, 36);
            this.btnSaveInDB.TabIndex = 11;
            this.btnSaveInDB.Text = "Записать в БД";
            this.btnSaveInDB.UseVisualStyleBackColor = true;
            this.btnSaveInDB.Click += new System.EventHandler(this.BtnSaveInDbClick);
            // 
            // btnSelectTicker
            // 
            this.btnSelectTicker.Location = new System.Drawing.Point(57, 17);
            this.btnSelectTicker.Name = "btnSelectTicker";
            this.btnSelectTicker.Size = new System.Drawing.Size(22, 22);
            this.btnSelectTicker.TabIndex = 10;
            this.btnSelectTicker.Text = "<";
            this.btnSelectTicker.UseVisualStyleBackColor = true;
            this.btnSelectTicker.Click += new System.EventHandler(this.BtnSelectTickerClick);
            // 
            // tbTicker
            // 
            this.tbTicker.Location = new System.Drawing.Point(82, 19);
            this.tbTicker.Name = "tbTicker";
            this.tbTicker.Size = new System.Drawing.Size(51, 20);
            this.tbTicker.TabIndex = 9;
            this.tbTicker.Text = "USDX";
            // 
            // btnMakeIndex
            // 
            this.btnMakeIndex.Location = new System.Drawing.Point(3, 107);
            this.btnMakeIndex.Name = "btnMakeIndex";
            this.btnMakeIndex.Size = new System.Drawing.Size(104, 36);
            this.btnMakeIndex.TabIndex = 8;
            this.btnMakeIndex.Text = "Формировать";
            this.btnMakeIndex.UseVisualStyleBackColor = true;
            this.btnMakeIndex.Click += new System.EventHandler(this.BtnMakeIndexClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(145, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Конец";
            // 
            // dpEnd
            // 
            this.dpEnd.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpEnd.Location = new System.Drawing.Point(148, 55);
            this.dpEnd.Name = "dpEnd";
            this.dpEnd.Size = new System.Drawing.Size(120, 20);
            this.dpEnd.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(145, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Начало";
            // 
            // dpStart
            // 
            this.dpStart.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpStart.Location = new System.Drawing.Point(148, 18);
            this.dpStart.Name = "dpStart";
            this.dpStart.Size = new System.Drawing.Size(120, 20);
            this.dpStart.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "ID в БД";
            // 
            // tbTickerId
            // 
            this.tbTickerId.Location = new System.Drawing.Point(3, 19);
            this.tbTickerId.Name = "tbTickerId";
            this.tbTickerId.Size = new System.Drawing.Size(51, 20);
            this.tbTickerId.TabIndex = 2;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 149);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tbExistingTickers);
            this.splitContainer.Panel1.Controls.Add(this.panelLeftTop);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.rtbHelper);
            this.splitContainer.Size = new System.Drawing.Size(661, 340);
            this.splitContainer.SplitterDistance = 274;
            this.splitContainer.TabIndex = 1;
            // 
            // tbExistingTickers
            // 
            this.tbExistingTickers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbExistingTickers.Location = new System.Drawing.Point(0, 32);
            this.tbExistingTickers.Name = "tbExistingTickers";
            this.tbExistingTickers.Size = new System.Drawing.Size(274, 308);
            this.tbExistingTickers.TabIndex = 1;
            this.tbExistingTickers.Text = "";
            // 
            // panelLeftTop
            // 
            this.panelLeftTop.Controls.Add(this.btnCheckExisting);
            this.panelLeftTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLeftTop.Location = new System.Drawing.Point(0, 0);
            this.panelLeftTop.Name = "panelLeftTop";
            this.panelLeftTop.Size = new System.Drawing.Size(274, 32);
            this.panelLeftTop.TabIndex = 0;
            // 
            // btnCheckExisting
            // 
            this.btnCheckExisting.Location = new System.Drawing.Point(3, 3);
            this.btnCheckExisting.Name = "btnCheckExisting";
            this.btnCheckExisting.Size = new System.Drawing.Size(159, 23);
            this.btnCheckExisting.TabIndex = 1;
            this.btnCheckExisting.Text = "Проверить доступность";
            this.btnCheckExisting.UseVisualStyleBackColor = true;
            this.btnCheckExisting.Click += new System.EventHandler(this.BtnCheckExistingClick);
            // 
            // rtbHelper
            // 
            this.rtbHelper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbHelper.Location = new System.Drawing.Point(0, 0);
            this.rtbHelper.Name = "rtbHelper";
            this.rtbHelper.Size = new System.Drawing.Size(383, 340);
            this.rtbHelper.TabIndex = 0;
            this.rtbHelper.Text = "";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "quote";
            this.saveFileDialog.Filter = "Quote Files (*.quote)|*.quote|Test Files (*.txt)|*.txt|All Files|*.txt";
            this.saveFileDialog.FilterIndex = 0;
            this.saveFileDialog.Title = "Сохранить результат";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "quote";
            this.openFileDialog.Filter = "Quote Files (*.quote)|*.quote|Test Files (*.txt)|*.txt|All Files|*.txt";
            this.openFileDialog.FilterIndex = 0;
            this.openFileDialog.Title = "Загрузить котировки";
            // 
            // btnShowDbQuotes
            // 
            this.btnShowDbQuotes.Location = new System.Drawing.Point(3, 79);
            this.btnShowDbQuotes.Name = "btnShowDbQuotes";
            this.btnShowDbQuotes.Size = new System.Drawing.Size(139, 22);
            this.btnShowDbQuotes.TabIndex = 13;
            this.btnShowDbQuotes.Text = "содержимое БД ...";
            this.btnShowDbQuotes.UseVisualStyleBackColor = true;
            this.btnShowDbQuotes.Click += new System.EventHandler(this.BtnShowDbQuotesClick);
            // 
            // IndexMakerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 489);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelTop);
            this.Name = "IndexMakerForm";
            this.Text = "Валютные индексы";
            this.Load += new System.EventHandler(this.IndexMakerFormLoad);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelControl.ResumeLayout(false);
            this.panelControl.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelLeftTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox tbTickerFormula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.Button btnMakeIndex;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dpEnd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dpStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbTickerId;
        private System.Windows.Forms.RichTextBox tbExistingTickers;
        private System.Windows.Forms.Panel panelLeftTop;
        private System.Windows.Forms.Button btnCheckExisting;
        private System.Windows.Forms.RichTextBox rtbHelper;
        private System.Windows.Forms.Button btnSelectTicker;
        private System.Windows.Forms.TextBox tbTicker;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button btnSaveInDB;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnShowLastDBQuote;
        private System.Windows.Forms.Button btnShowDbQuotes;
    }
}