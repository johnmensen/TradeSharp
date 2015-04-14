namespace TestApp
{
    partial class TestForm1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm1));
            this.button1 = new System.Windows.Forms.Button();
            this.singleParametersFastGrid = new FastGrid.FastGrid();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Top;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(292, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // singleParametersFastGrid
            // 
            this.singleParametersFastGrid.CaptionHeight = 20;
            this.singleParametersFastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.singleParametersFastGrid.CellHeight = 18;
            this.singleParametersFastGrid.CellPadding = 5;
            this.singleParametersFastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.singleParametersFastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.singleParametersFastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.singleParametersFastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.singleParametersFastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.singleParametersFastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.singleParametersFastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.singleParametersFastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.singleParametersFastGrid.Columns = ((System.Collections.Generic.List<FastGrid.FastColumn>)(resources.GetObject("singleParametersFastGrid.Columns")));
            this.singleParametersFastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.singleParametersFastGrid.FitWidth = false;
            this.singleParametersFastGrid.FontAnchoredRow = null;
            this.singleParametersFastGrid.FontCell = null;
            this.singleParametersFastGrid.FontHeader = null;
            this.singleParametersFastGrid.FontSelectedCell = null;
            this.singleParametersFastGrid.Location = new System.Drawing.Point(0, 23);
            this.singleParametersFastGrid.MinimumTableWidth = null;
            this.singleParametersFastGrid.MultiSelectEnabled = false;
            this.singleParametersFastGrid.Name = "singleParametersFastGrid";
            this.singleParametersFastGrid.SelectEnabled = true;
            this.singleParametersFastGrid.Size = new System.Drawing.Size(292, 250);
            this.singleParametersFastGrid.StickFirst = false;
            this.singleParametersFastGrid.StickLast = false;
            this.singleParametersFastGrid.TabIndex = 2;
            // 
            // TestForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.singleParametersFastGrid);
            this.Controls.Add(this.button1);
            this.Name = "TestForm1";
            this.Text = "TestForm1";
            this.Load += new System.EventHandler(this.TestForm1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private FastGrid.FastGrid singleParametersFastGrid;
    }
}