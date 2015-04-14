namespace TradeSharp.Client.Controls.NavPanel
{
    partial class NavPageAccountControl
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
            this.grid = new FastGrid.FastGrid();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.CaptionHeight = 20;
            this.grid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.grid.CellHeight = 18;
            this.grid.CellPadding = 5;
            this.grid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.grid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.grid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.grid.ColorCellFont = System.Drawing.Color.Black;
            this.grid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.grid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.grid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.grid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.FontAnchoredRow = null;
            this.grid.FontCell = null;
            this.grid.FontHeader = null;
            this.grid.FontSelectedCell = null;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.MinimumTableWidth = null;
            this.grid.MultiSelectEnabled = false;
            this.grid.Name = "grid";
            this.grid.SelectEnabled = true;
            this.grid.Size = new System.Drawing.Size(221, 235);
            this.grid.StickFirst = false;
            this.grid.StickLast = false;
            this.grid.TabIndex = 0;
            // 
            // timer
            // 
            this.timer.Interval = 2000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // NavPageAccountControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grid);
            this.Name = "NavPageAccountControl";
            this.Size = new System.Drawing.Size(221, 235);
            this.ResumeLayout(false);

        }

        #endregion

        private FastGrid.FastGrid grid;
        private System.Windows.Forms.Timer timer;
    }
}
