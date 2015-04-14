namespace TestApp
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.fastCombo = new FastGrid.FastGridCombo();
            this.btnTest = new System.Windows.Forms.Button();
            this.fastGrid = new FastGrid.FastGrid();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.fastCombo);
            this.panelTop.Controls.Add(this.btnTest);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(416, 31);
            this.panelTop.TabIndex = 0;
            // 
            // fastCombo
            // 
            this.fastCombo.AltBackColor = System.Drawing.Color.WhiteSmoke;
            this.fastCombo.ButtonText = "...";
            this.fastCombo.CellBackColor = System.Drawing.Color.White;
            this.fastCombo.FixedTableWidth = 0;
            this.fastCombo.FontColor = System.Drawing.Color.Black;
            this.fastCombo.GridItemHeight = 20;
            this.fastCombo.Location = new System.Drawing.Point(110, 3);
            this.fastCombo.MaxDropHeight = 200;
            this.fastCombo.MinDropHeight = 30;
            this.fastCombo.MinTableWidth = 0;
            this.fastCombo.Name = "fastCombo";
            this.fastCombo.SelectedText = "";
            this.fastCombo.Size = new System.Drawing.Size(198, 21);
            this.fastCombo.TabIndex = 1;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(3, 3);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Speed test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.BtnTestClick);
            // 
            // fastGrid
            // 
            this.fastGrid.CaptionHeight = 22;
            this.fastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.fastGrid.CellHeight = 18;
            this.fastGrid.CellPadding = 5;
            this.fastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.fastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.fastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.fastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.fastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.fastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.fastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.fastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.fastGrid.Cursor = System.Windows.Forms.Cursors.Default;
            this.fastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastGrid.FitWidth = true;
            this.fastGrid.FontAnchoredRow = null;
            this.fastGrid.FontCell = null;
            this.fastGrid.FontHeader = null;
            this.fastGrid.FontSelectedCell = null;
            this.fastGrid.Location = new System.Drawing.Point(0, 31);
            this.fastGrid.MinimumTableWidth = null;
            this.fastGrid.MultiSelectEnabled = true;
            this.fastGrid.Name = "fastGrid";
            this.fastGrid.SelectEnabled = true;
            this.fastGrid.Size = new System.Drawing.Size(416, 360);
            this.fastGrid.StickFirst = false;
            this.fastGrid.StickLast = false;
            this.fastGrid.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 391);
            this.Controls.Add(this.fastGrid);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.Text = "Grid simple test";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnTest;
        private FastGrid.FastGrid fastGrid;
        private FastGrid.FastGridCombo fastCombo;
    }
}

