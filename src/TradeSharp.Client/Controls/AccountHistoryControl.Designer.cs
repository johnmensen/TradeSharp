namespace TradeSharp.Client.Controls
{
    partial class AccountHistoryControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.фильтрToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConfigColumnsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemSaveInFile = new System.Windows.Forms.ToolStripMenuItem();
            this.historyGrid = new FastGrid.FastGrid();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.фильтрToolStripMenuItem,
            this.ConfigColumnsMenuItem,
            this.menuitemSaveInFile});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(205, 70);
            // 
            // фильтрToolStripMenuItem
            // 
            this.фильтрToolStripMenuItem.Name = "фильтрToolStripMenuItem";
            this.фильтрToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.фильтрToolStripMenuItem.Text = "Фильтр...";
            this.фильтрToolStripMenuItem.Click += new System.EventHandler(this.ФильтрToolStripMenuItemClick);
            // 
            // ConfigColumnsMenuItem
            // 
            this.ConfigColumnsMenuItem.Name = "ConfigColumnsMenuItem";
            this.ConfigColumnsMenuItem.Size = new System.Drawing.Size(204, 22);
            this.ConfigColumnsMenuItem.Text = "Настроить столбцы...";
            this.ConfigColumnsMenuItem.Click += new System.EventHandler(this.ConfigColumnsMenuItemClick);
            // 
            // menuitemSaveInFile
            // 
            this.menuitemSaveInFile.Name = "menuitemSaveInFile";
            this.menuitemSaveInFile.Size = new System.Drawing.Size(204, 22);
            this.menuitemSaveInFile.Text = "Сохранить в файл (*.csv)";
            this.menuitemSaveInFile.Click += new System.EventHandler(this.MenuitemSaveInFileClick);
            // 
            // historyGrid
            // 
            this.historyGrid.CaptionHeight = 20;
            this.historyGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.historyGrid.CellHeight = 18;
            this.historyGrid.CellPadding = 5;
            this.historyGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.historyGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.historyGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.historyGrid.ColorCellFont = System.Drawing.Color.Black;
            this.historyGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.historyGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.historyGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.historyGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.historyGrid.ContextMenuStrip = this.contextMenuStrip;
            this.historyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyGrid.FitWidth = true;
            this.historyGrid.FontAnchoredRow = null;
            this.historyGrid.FontCell = null;
            this.historyGrid.FontHeader = null;
            this.historyGrid.FontSelectedCell = null;
            this.historyGrid.Location = new System.Drawing.Point(0, 0);
            this.historyGrid.MinimumTableWidth = null;
            this.historyGrid.MultiSelectEnabled = false;
            this.historyGrid.Name = "historyGrid";
            this.historyGrid.SelectEnabled = true;
            this.historyGrid.Size = new System.Drawing.Size(546, 351);
            this.historyGrid.StickFirst = false;
            this.historyGrid.StickLast = false;
            this.historyGrid.TabIndex = 1;
            this.historyGrid.UserHitCell += new FastGrid.UserHitCellDel(this.HistoryGridUserHitCell);
            // 
            // AccountHistoryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.historyGrid);
            this.Name = "AccountHistoryControl";
            this.Size = new System.Drawing.Size(546, 351);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem фильтрToolStripMenuItem;
        public FastGrid.FastGrid historyGrid;
        private System.Windows.Forms.ToolStripMenuItem ConfigColumnsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuitemSaveInFile;
    }
}
