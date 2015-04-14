using System;

namespace TradeSharp.Client.Forms
{
    partial class ManageTemplate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageTemplate));
            this.fastGrid = new FastGrid.FastGrid();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // fastGrid
            // 
            this.fastGrid.CaptionHeight = 20;
            this.fastGrid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.fastGrid.CellHeight = 18;
            this.fastGrid.CellPadding = 5;
            this.fastGrid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.fastGrid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.fastGrid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.fastGrid.ColorCellFont = System.Drawing.Color.Black;
            this.fastGrid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.fastGrid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.fastGrid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.fastGrid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.fastGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastGrid.FitWidth = true;
            this.fastGrid.FontAnchoredRow = null;
            this.fastGrid.FontCell = null;
            this.fastGrid.FontHeader = null;
            this.fastGrid.FontSelectedCell = null;
            this.fastGrid.Location = new System.Drawing.Point(0, 0);
            this.fastGrid.MinimumTableWidth = null;
            this.fastGrid.MultiSelectEnabled = false;
            this.fastGrid.Name = "fastGrid";
            this.fastGrid.SelectEnabled = true;
            this.fastGrid.Size = new System.Drawing.Size(364, 275);
            this.fastGrid.StickFirst = false;
            this.fastGrid.StickLast = false;
            this.fastGrid.TabIndex = 7;
            this.fastGrid.UserHitCell += new FastGrid.UserHitCellDel(this.FastGridUserHitCell);
            this.fastGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FastGridMouseClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Yellow;
            this.imageList.Images.SetKeyName(0, "ico delete.png");
            // 
            // ManageTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 273);
            this.Controls.Add(this.fastGrid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(370, 300);
            this.Name = "ManageTemplate";
            this.Tag = "TitleTemplateManagement";
            this.Text = "Управление шаблонами";
            this.ResumeLayout(false);

        }

        #endregion

        private FastGrid.FastGrid fastGrid;
        private System.Windows.Forms.ImageList imageList;
    }
}