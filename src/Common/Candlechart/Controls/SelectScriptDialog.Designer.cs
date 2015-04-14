namespace Candlechart.Controls
{
    partial class SelectScriptDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectScriptDialog));
            this.panelTop = new System.Windows.Forms.Panel();
            this.tbTime = new System.Windows.Forms.TextBox();
            this.tbPrice = new System.Windows.Forms.TextBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grid = new FastGrid.FastGrid();
            this.btnSetupScripts = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnSetupScripts);
            this.panelTop.Controls.Add(this.tbTime);
            this.panelTop.Controls.Add(this.tbPrice);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(293, 27);
            this.panelTop.TabIndex = 0;
            // 
            // tbTime
            // 
            this.tbTime.Location = new System.Drawing.Point(90, 3);
            this.tbTime.Name = "tbTime";
            this.tbTime.Size = new System.Drawing.Size(119, 20);
            this.tbTime.TabIndex = 1;
            // 
            // tbPrice
            // 
            this.tbPrice.Location = new System.Drawing.Point(3, 3);
            this.tbPrice.Name = "tbPrice";
            this.tbPrice.Size = new System.Drawing.Size(68, 20);
            this.tbPrice.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 219);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(293, 36);
            this.panelBottom.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(10, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
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
            this.grid.Location = new System.Drawing.Point(0, 27);
            this.grid.MinimumTableWidth = null;
            this.grid.MultiSelectEnabled = false;
            this.grid.Name = "grid";
            this.grid.SelectEnabled = true;
            this.grid.Size = new System.Drawing.Size(293, 192);
            this.grid.StickFirst = false;
            this.grid.StickLast = false;
            this.grid.TabIndex = 2;
            // 
            // btnSetupScripts
            // 
            this.btnSetupScripts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupScripts.ImageIndex = 0;
            this.btnSetupScripts.ImageList = this.imageList;
            this.btnSetupScripts.Location = new System.Drawing.Point(223, 3);
            this.btnSetupScripts.Name = "btnSetupScripts";
            this.btnSetupScripts.Size = new System.Drawing.Size(25, 23);
            this.btnSetupScripts.TabIndex = 2;
            this.btnSetupScripts.UseVisualStyleBackColor = true;
            this.btnSetupScripts.Click += new System.EventHandler(this.btnSetupScripts_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "gear_16.png");
            // 
            // SelectScriptDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(293, 255);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(230, 180);
            this.Name = "SelectScriptDialog";
            this.Text = "Выбрать скрипт";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox tbTime;
        private System.Windows.Forms.TextBox tbPrice;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private FastGrid.FastGrid grid;
        private System.Windows.Forms.Button btnSetupScripts;
        private System.Windows.Forms.ImageList imageList;
    }
}