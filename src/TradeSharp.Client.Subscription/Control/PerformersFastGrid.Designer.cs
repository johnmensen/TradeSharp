namespace TradeSharp.Client.Subscription.Control
{
    partial class PerformersFastGrid
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformersFastGrid));
            this.grid = new FastGrid.FastGrid();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemChooseColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemChooseCriteria = new System.Windows.Forms.ToolStripMenuItem();
            this.imgListChartMini = new System.Windows.Forms.ImageList(this.components);
            this.imageListGrid = new System.Windows.Forms.ImageList(this.components);
            this.imgListAvatar = new System.Windows.Forms.ImageList(this.components);
            this.menuitemSelectedSummary = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.CaptionHeight = 20;
            this.grid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.grid.CellHeight = 36;
            this.grid.CellPadding = 5;
            this.grid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.grid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.grid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.grid.ColorCellFont = System.Drawing.Color.Black;
            this.grid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.grid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.grid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.grid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.grid.ContextMenuStrip = this.contextMenu;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.FitWidth = false;
            this.grid.FontAnchoredRow = null;
            this.grid.FontCell = null;
            this.grid.FontHeader = null;
            this.grid.FontSelectedCell = null;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.MinimumTableWidth = null;
            this.grid.MultiSelectEnabled = true;
            this.grid.Name = "grid";
            this.grid.SelectEnabled = true;
            this.grid.Size = new System.Drawing.Size(480, 138);
            this.grid.StickFirst = false;
            this.grid.StickLast = false;
            this.grid.TabIndex = 6;
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemChooseColumns,
            this.menuitemChooseCriteria,
            this.menuitemSelectedSummary});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(197, 92);
            this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // menuitemChooseColumns
            // 
            this.menuitemChooseColumns.Name = "menuitemChooseColumns";
            this.menuitemChooseColumns.Size = new System.Drawing.Size(196, 22);
            this.menuitemChooseColumns.Text = "Выбрать столбцы...";
            this.menuitemChooseColumns.Click += new System.EventHandler(this.MenuitemChooseColumnsClick);
            // 
            // menuitemChooseCriteria
            // 
            this.menuitemChooseCriteria.Name = "menuitemChooseCriteria";
            this.menuitemChooseCriteria.Size = new System.Drawing.Size(196, 22);
            this.menuitemChooseCriteria.Text = "Настроить критерий...";
            this.menuitemChooseCriteria.Visible = false;
            this.menuitemChooseCriteria.Click += new System.EventHandler(this.MenuitemChooseCriteriaClick);
            // 
            // imgListChartMini
            // 
            this.imgListChartMini.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgListChartMini.ImageSize = new System.Drawing.Size(80, 32);
            this.imgListChartMini.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageListGrid
            // 
            this.imageListGrid.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGrid.ImageStream")));
            this.imageListGrid.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGrid.Images.SetKeyName(0, "True");
            this.imageListGrid.Images.SetKeyName(1, "False");
            // 
            // imgListAvatar
            // 
            this.imgListAvatar.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgListAvatar.ImageSize = new System.Drawing.Size(16, 16);
            this.imgListAvatar.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // menuitemSelectedSummary
            // 
            this.menuitemSelectedSummary.Name = "menuitemSelectedSummary";
            this.menuitemSelectedSummary.Size = new System.Drawing.Size(196, 22);
            this.menuitemSelectedSummary.Text = "Совокупный доход...";
            this.menuitemSelectedSummary.Click += new System.EventHandler(this.menuitemSelectedSummary_Click);
            // 
            // PerformersFastGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grid);
            this.Name = "PerformersFastGrid";
            this.Size = new System.Drawing.Size(480, 138);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FastGrid.FastGrid grid;
        private System.Windows.Forms.ImageList imgListChartMini;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem menuitemChooseColumns;
        private System.Windows.Forms.ToolStripMenuItem menuitemChooseCriteria;
        private System.Windows.Forms.ImageList imageListGrid;
        private System.Windows.Forms.ImageList imgListAvatar;
        private System.Windows.Forms.ToolStripMenuItem menuitemSelectedSummary;
    }
}
