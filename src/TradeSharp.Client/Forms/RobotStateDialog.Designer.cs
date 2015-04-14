namespace TradeSharp.Client.Forms
{
    partial class RobotStateDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RobotStateDialog));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.gridRobot = new FastGrid.FastGrid();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
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
            this.splitContainer.Panel1.Controls.Add(this.gridRobot);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer.Size = new System.Drawing.Size(525, 364);
            this.splitContainer.SplitterDistance = 256;
            this.splitContainer.TabIndex = 0;
            // 
            // gridRobot
            // 
            this.gridRobot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridRobot.CaptionHeight = 20;
            this.gridRobot.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridRobot.CellHeight = 18;
            this.gridRobot.CellPadding = 5;
            this.gridRobot.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridRobot.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridRobot.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridRobot.ColorCellFont = System.Drawing.Color.Black;
            this.gridRobot.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridRobot.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridRobot.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridRobot.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridRobot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRobot.FitWidth = false;
            this.gridRobot.FontAnchoredRow = null;
            this.gridRobot.FontCell = null;
            this.gridRobot.FontHeader = null;
            this.gridRobot.FontSelectedCell = null;
            this.gridRobot.Location = new System.Drawing.Point(0, 0);
            this.gridRobot.MinimumTableWidth = null;
            this.gridRobot.MultiSelectEnabled = false;
            this.gridRobot.Name = "gridRobot";
            this.gridRobot.SelectEnabled = true;
            this.gridRobot.Size = new System.Drawing.Size(256, 364);
            this.gridRobot.StickFirst = false;
            this.gridRobot.StickLast = false;
            this.gridRobot.TabIndex = 0;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(265, 364);
            this.propertyGrid.TabIndex = 0;
            // 
            // RobotStateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 364);
            this.Controls.Add(this.splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RobotStateDialog";
            this.Text = "Торговые роботы";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private FastGrid.FastGrid gridRobot;
        private System.Windows.Forms.PropertyGrid propertyGrid;
    }
}