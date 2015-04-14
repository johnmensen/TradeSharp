namespace TradeSharp.QuoteAdmin
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.gridQuote = new FastGrid.FastGrid();
            this.gridCandles = new FastGrid.FastGrid();
            this.panelTop = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnGetCandles = new System.Windows.Forms.Button();
            this.tbCountCandles = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dpEndHist = new System.Windows.Forms.DateTimePicker();
            this.dpStartHist = new System.Windows.Forms.DateTimePicker();
            this.btnGetHistoryRange = new System.Windows.Forms.Button();
            this.tbFormat = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbActiveCounter = new System.Windows.Forms.TextBox();
            this.tbActiveBase = new System.Windows.Forms.TextBox();
            this.menuQuote = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemLoadInDb = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLoadFromCsv = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.menuQuote.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.gridQuote);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.gridCandles);
            this.splitContainer.Panel2.Controls.Add(this.panelTop);
            this.splitContainer.Size = new System.Drawing.Size(486, 389);
            this.splitContainer.SplitterDistance = 169;
            this.splitContainer.TabIndex = 0;
            // 
            // gridQuote
            // 
            this.gridQuote.CaptionHeight = 20;
            this.gridQuote.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridQuote.CellHeight = 18;
            this.gridQuote.CellPadding = 5;
            this.gridQuote.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridQuote.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridQuote.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridQuote.ColorCellFont = System.Drawing.Color.Black;
            this.gridQuote.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridQuote.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridQuote.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridQuote.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridQuote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridQuote.FitWidth = false;
            this.gridQuote.FontAnchoredRow = null;
            this.gridQuote.FontCell = null;
            this.gridQuote.FontHeader = null;
            this.gridQuote.FontSelectedCell = null;
            this.gridQuote.Location = new System.Drawing.Point(0, 0);
            this.gridQuote.MinimumTableWidth = null;
            this.gridQuote.MultiSelectEnabled = false;
            this.gridQuote.Name = "gridQuote";
            this.gridQuote.SelectEnabled = true;
            this.gridQuote.Size = new System.Drawing.Size(169, 389);
            this.gridQuote.StickFirst = false;
            this.gridQuote.StickLast = false;
            this.gridQuote.TabIndex = 0;
            this.gridQuote.UserHitCell += new FastGrid.UserHitCellDel(this.GridQuoteUserHitCell);
            // 
            // gridCandles
            // 
            this.gridCandles.CaptionHeight = 20;
            this.gridCandles.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridCandles.CellHeight = 18;
            this.gridCandles.CellPadding = 5;
            this.gridCandles.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridCandles.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridCandles.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridCandles.ColorCellFont = System.Drawing.Color.Black;
            this.gridCandles.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridCandles.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridCandles.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridCandles.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridCandles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCandles.FitWidth = false;
            this.gridCandles.FontAnchoredRow = null;
            this.gridCandles.FontCell = null;
            this.gridCandles.FontHeader = null;
            this.gridCandles.FontSelectedCell = null;
            this.gridCandles.Location = new System.Drawing.Point(0, 110);
            this.gridCandles.MinimumTableWidth = null;
            this.gridCandles.MultiSelectEnabled = false;
            this.gridCandles.Name = "gridCandles";
            this.gridCandles.SelectEnabled = true;
            this.gridCandles.Size = new System.Drawing.Size(313, 279);
            this.gridCandles.StickFirst = false;
            this.gridCandles.StickLast = false;
            this.gridCandles.TabIndex = 14;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.btnGetCandles);
            this.panelTop.Controls.Add(this.tbCountCandles);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.dpEndHist);
            this.panelTop.Controls.Add(this.dpStartHist);
            this.panelTop.Controls.Add(this.btnGetHistoryRange);
            this.panelTop.Controls.Add(this.tbFormat);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.tbActiveCounter);
            this.panelTop.Controls.Add(this.tbActiveBase);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(313, 110);
            this.panelTop.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(168, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "свеч";
            // 
            // btnGetCandles
            // 
            this.btnGetCandles.Location = new System.Drawing.Point(1, 81);
            this.btnGetCandles.Name = "btnGetCandles";
            this.btnGetCandles.Size = new System.Drawing.Size(117, 23);
            this.btnGetCandles.TabIndex = 22;
            this.btnGetCandles.Text = "Прочитать первые";
            this.btnGetCandles.UseVisualStyleBackColor = true;
            this.btnGetCandles.Click += new System.EventHandler(this.BtnGetCandlesClick);
            // 
            // tbCountCandles
            // 
            this.tbCountCandles.Location = new System.Drawing.Point(124, 83);
            this.tbCountCandles.Name = "tbCountCandles";
            this.tbCountCandles.Size = new System.Drawing.Size(38, 20);
            this.tbCountCandles.TabIndex = 21;
            this.tbCountCandles.Text = "20";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(124, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "-";
            // 
            // dpEndHist
            // 
            this.dpEndHist.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpEndHist.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpEndHist.Location = new System.Drawing.Point(136, 55);
            this.dpEndHist.Name = "dpEndHist";
            this.dpEndHist.Size = new System.Drawing.Size(123, 20);
            this.dpEndHist.TabIndex = 19;
            // 
            // dpStartHist
            // 
            this.dpStartHist.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpStartHist.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpStartHist.Location = new System.Drawing.Point(4, 55);
            this.dpStartHist.Name = "dpStartHist";
            this.dpStartHist.Size = new System.Drawing.Size(118, 20);
            this.dpStartHist.TabIndex = 18;
            // 
            // btnGetHistoryRange
            // 
            this.btnGetHistoryRange.Location = new System.Drawing.Point(2, 26);
            this.btnGetHistoryRange.Name = "btnGetHistoryRange";
            this.btnGetHistoryRange.Size = new System.Drawing.Size(132, 23);
            this.btnGetHistoryRange.TabIndex = 17;
            this.btnGetHistoryRange.Tag = "TitleReadBounds";
            this.btnGetHistoryRange.Text = "Прочитать границы";
            this.btnGetHistoryRange.UseVisualStyleBackColor = true;
            this.btnGetHistoryRange.Click += new System.EventHandler(this.BtnGetHistoryRangeClick);
            // 
            // tbFormat
            // 
            this.tbFormat.Location = new System.Drawing.Point(126, 3);
            this.tbFormat.Name = "tbFormat";
            this.tbFormat.Size = new System.Drawing.Size(67, 20);
            this.tbFormat.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "/";
            // 
            // tbActiveCounter
            // 
            this.tbActiveCounter.Location = new System.Drawing.Point(69, 3);
            this.tbActiveCounter.Name = "tbActiveCounter";
            this.tbActiveCounter.Size = new System.Drawing.Size(51, 20);
            this.tbActiveCounter.TabIndex = 14;
            // 
            // tbActiveBase
            // 
            this.tbActiveBase.Location = new System.Drawing.Point(3, 3);
            this.tbActiveBase.Name = "tbActiveBase";
            this.tbActiveBase.Size = new System.Drawing.Size(51, 20);
            this.tbActiveBase.TabIndex = 13;
            // 
            // menuQuote
            // 
            this.menuQuote.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemLoadInDb,
            this.menuLoadFromCsv});
            this.menuQuote.Name = "menuQuote";
            this.menuQuote.Size = new System.Drawing.Size(217, 48);
            // 
            // menuitemLoadInDb
            // 
            this.menuitemLoadInDb.Name = "menuitemLoadInDb";
            this.menuitemLoadInDb.Size = new System.Drawing.Size(216, 22);
            this.menuitemLoadInDb.Text = "Загрузить историю в БД...";
            this.menuitemLoadInDb.Click += new System.EventHandler(this.MenuitemLoadInDbClick);
            // 
            // menuLoadFromCsv
            // 
            this.menuLoadFromCsv.Name = "menuLoadFromCsv";
            this.menuLoadFromCsv.Size = new System.Drawing.Size(216, 22);
            this.menuLoadFromCsv.Text = "Загрузить из *.CSV...";
            this.menuLoadFromCsv.Click += new System.EventHandler(this.menuLoadFromCsv_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "csv";
            this.openFileDialog.Filter = "*.csv|*.csv|*.*|Все файлы";
            this.openFileDialog.FilterIndex = 0;
            this.openFileDialog.Title = "Файл архива котировок";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 389);
            this.Controls.Add(this.splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Котировки";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.menuQuote.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private FastGrid.FastGrid gridQuote;
        private System.Windows.Forms.ContextMenuStrip menuQuote;
        private System.Windows.Forms.ToolStripMenuItem menuitemLoadInDb;
        private FastGrid.FastGrid gridCandles;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnGetCandles;
        private System.Windows.Forms.TextBox tbCountCandles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dpEndHist;
        private System.Windows.Forms.DateTimePicker dpStartHist;
        private System.Windows.Forms.Button btnGetHistoryRange;
        private System.Windows.Forms.TextBox tbFormat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbActiveCounter;
        private System.Windows.Forms.TextBox tbActiveBase;
        private System.Windows.Forms.ToolStripMenuItem menuLoadFromCsv;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}

