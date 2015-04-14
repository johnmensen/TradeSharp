namespace TradeSharp.Robot.BL
{
    partial class RobotTimeframesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RobotTimeframesForm));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.gridTickers = new FastGrid.FastGrid();
            this.lstIcon = new System.Windows.Forms.ImageList(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(199, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(118, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Tag = "TitleOK";
            this.btnAccept.Text = "ОК";
            this.btnAccept.UseVisualStyleBackColor = true;
            // 
            // gridTickers
            // 
            this.gridTickers.CaptionHeight = 20;
            this.gridTickers.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridTickers.CellHeight = 18;
            this.gridTickers.CellPadding = 5;
            this.gridTickers.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridTickers.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridTickers.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridTickers.ColorCellFont = System.Drawing.Color.Black;
            this.gridTickers.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridTickers.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridTickers.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridTickers.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridTickers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTickers.FitWidth = false;
            this.gridTickers.FontAnchoredRow = null;
            this.gridTickers.FontCell = null;
            this.gridTickers.FontHeader = null;
            this.gridTickers.FontSelectedCell = null;
            this.gridTickers.Location = new System.Drawing.Point(0, 0);
            this.gridTickers.MinimumTableWidth = null;
            this.gridTickers.MultiSelectEnabled = false;
            this.gridTickers.Name = "gridTickers";
            this.gridTickers.SelectEnabled = false;
            this.gridTickers.Size = new System.Drawing.Size(277, 273);
            this.gridTickers.StickFirst = false;
            this.gridTickers.StickLast = false;
            this.gridTickers.TabIndex = 1;
            // 
            // lstIcon
            // 
            this.lstIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lstIcon.ImageStream")));
            this.lstIcon.TransparentColor = System.Drawing.Color.Transparent;
            this.lstIcon.Images.SetKeyName(0, "ico delete.png");
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnAccept);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 273);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(277, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // RobotTimeframesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 302);
            this.Controls.Add(this.gridTickers);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RobotTimeframesForm";
            this.Tag = "TitleTimeframes";
            this.Text = "Таймфреймы";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private FastGrid.FastGrid gridTickers;
        private System.Windows.Forms.ImageList lstIcon;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}