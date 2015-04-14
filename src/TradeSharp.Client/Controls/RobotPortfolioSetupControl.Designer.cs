namespace TradeSharp.Client.Controls
{
    partial class RobotPortfolioSetupControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RobotPortfolioSetupControl));
            this.imageListGlypth = new System.Windows.Forms.ImageList(this.components);
            this.saveRobotsPortfolioDlg = new System.Windows.Forms.SaveFileDialog();
            this.openRobotsPortfolioDlg = new System.Windows.Forms.OpenFileDialog();
            this.menuMagic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BtnUnselectRobot = new System.Windows.Forms.Button();
            this.BtnSelectRobot = new System.Windows.Forms.Button();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.lbRobotsCollection = new System.Windows.Forms.ListBox();
            this.panelSelUnsel = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.gridRobot = new FastGrid.FastGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSaveProperties = new System.Windows.Forms.Button();
            this.btnReadProperties = new System.Windows.Forms.Button();
            this.btnPropertiesRobots = new System.Windows.Forms.Button();
            this.panelLeft.SuspendLayout();
            this.panelSelUnsel.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListGlypth
            // 
            this.imageListGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlypth.ImageStream")));
            this.imageListGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlypth.Images.SetKeyName(0, "ico_arrow_green_left.png");
            this.imageListGlypth.Images.SetKeyName(1, "ico_arrow_green_right.png");
            this.imageListGlypth.Images.SetKeyName(2, "ico_load.png");
            this.imageListGlypth.Images.SetKeyName(3, "ico_save.png");
            // 
            // saveRobotsPortfolioDlg
            // 
            this.saveRobotsPortfolioDlg.DefaultExt = "*.pxml";
            this.saveRobotsPortfolioDlg.Filter = "Robots portfolio|*.pxml|All files|*.*";
            this.saveRobotsPortfolioDlg.Title = "Портфель роботов";
            // 
            // openRobotsPortfolioDlg
            // 
            this.openRobotsPortfolioDlg.DefaultExt = "*.pxml";
            this.openRobotsPortfolioDlg.Filter = "Robots portfolio|*.pxml|All files|*.*";
            this.openRobotsPortfolioDlg.Title = "Портфель роботов";
            // 
            // menuMagic
            // 
            this.menuMagic.Name = "menuMagic";
            this.menuMagic.Size = new System.Drawing.Size(61, 4);
            // 
            // BtnUnselectRobot
            // 
            this.BtnUnselectRobot.ImageIndex = 0;
            this.BtnUnselectRobot.ImageList = this.imageListGlypth;
            this.BtnUnselectRobot.Location = new System.Drawing.Point(3, 42);
            this.BtnUnselectRobot.Name = "BtnUnselectRobot";
            this.BtnUnselectRobot.Size = new System.Drawing.Size(28, 23);
            this.BtnUnselectRobot.TabIndex = 36;
            this.toolTip.SetToolTip(this.BtnUnselectRobot, "Убрать");
            this.BtnUnselectRobot.UseVisualStyleBackColor = true;
            this.BtnUnselectRobot.Click += new System.EventHandler(this.BtnUnselectRobotClick);
            // 
            // BtnSelectRobot
            // 
            this.BtnSelectRobot.ImageIndex = 1;
            this.BtnSelectRobot.ImageList = this.imageListGlypth;
            this.BtnSelectRobot.Location = new System.Drawing.Point(3, 13);
            this.BtnSelectRobot.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.BtnSelectRobot.Name = "BtnSelectRobot";
            this.BtnSelectRobot.Size = new System.Drawing.Size(28, 23);
            this.BtnSelectRobot.TabIndex = 35;
            this.toolTip.SetToolTip(this.BtnSelectRobot, "Добавить");
            this.BtnSelectRobot.UseVisualStyleBackColor = true;
            this.BtnSelectRobot.Click += new System.EventHandler(this.BtnSelectRobotClick);
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.lbRobotsCollection);
            this.panelLeft.Controls.Add(this.panelSelUnsel);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(199, 159);
            this.panelLeft.TabIndex = 35;
            // 
            // lbRobotsCollection
            // 
            this.lbRobotsCollection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRobotsCollection.FormattingEnabled = true;
            this.lbRobotsCollection.Location = new System.Drawing.Point(0, 0);
            this.lbRobotsCollection.Name = "lbRobotsCollection";
            this.lbRobotsCollection.Size = new System.Drawing.Size(165, 159);
            this.lbRobotsCollection.TabIndex = 34;
            this.lbRobotsCollection.DoubleClick += new System.EventHandler(this.LbRobotsCollectionDoubleClick);
            this.lbRobotsCollection.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LbRobotsCollectionMouseDown);
            // 
            // panelSelUnsel
            // 
            this.panelSelUnsel.Controls.Add(this.BtnUnselectRobot);
            this.panelSelUnsel.Controls.Add(this.BtnSelectRobot);
            this.panelSelUnsel.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelSelUnsel.Location = new System.Drawing.Point(165, 0);
            this.panelSelUnsel.Name = "panelSelUnsel";
            this.panelSelUnsel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.panelSelUnsel.Size = new System.Drawing.Size(34, 159);
            this.panelSelUnsel.TabIndex = 33;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.gridRobot);
            this.panelRight.Controls.Add(this.panel1);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(199, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(401, 159);
            this.panelRight.TabIndex = 36;
            // 
            // gridRobot
            // 
            this.gridRobot.AllowDrop = true;
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
            this.gridRobot.FitWidth = true;
            this.gridRobot.FontAnchoredRow = null;
            this.gridRobot.FontCell = null;
            this.gridRobot.FontHeader = null;
            this.gridRobot.FontSelectedCell = null;
            this.gridRobot.Location = new System.Drawing.Point(0, 0);
            this.gridRobot.MinimumTableWidth = null;
            this.gridRobot.MultiSelectEnabled = true;
            this.gridRobot.Name = "gridRobot";
            this.gridRobot.SelectEnabled = true;
            this.gridRobot.Size = new System.Drawing.Size(401, 129);
            this.gridRobot.StickFirst = false;
            this.gridRobot.StickLast = false;
            this.gridRobot.TabIndex = 40;
            this.gridRobot.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridRobotDragDrop);
            this.gridRobot.DragEnter += new System.Windows.Forms.DragEventHandler(this.GridRobotDragEnter);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSaveProperties);
            this.panel1.Controls.Add(this.btnReadProperties);
            this.panel1.Controls.Add(this.btnPropertiesRobots);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 129);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(401, 30);
            this.panel1.TabIndex = 39;
            // 
            // btnSaveProperties
            // 
            this.btnSaveProperties.ImageIndex = 3;
            this.btnSaveProperties.ImageList = this.imageListGlypth;
            this.btnSaveProperties.Location = new System.Drawing.Point(187, 3);
            this.btnSaveProperties.Name = "btnSaveProperties";
            this.btnSaveProperties.Size = new System.Drawing.Size(31, 23);
            this.btnSaveProperties.TabIndex = 39;
            this.btnSaveProperties.UseVisualStyleBackColor = true;
            this.btnSaveProperties.Click += new System.EventHandler(this.BtnSavePropertiesClick);
            // 
            // btnReadProperties
            // 
            this.btnReadProperties.ImageIndex = 2;
            this.btnReadProperties.ImageList = this.imageListGlypth;
            this.btnReadProperties.Location = new System.Drawing.Point(224, 3);
            this.btnReadProperties.Name = "btnReadProperties";
            this.btnReadProperties.Size = new System.Drawing.Size(27, 23);
            this.btnReadProperties.TabIndex = 40;
            this.btnReadProperties.UseVisualStyleBackColor = true;
            this.btnReadProperties.Click += new System.EventHandler(this.BtnReadPropertiesClick);
            // 
            // btnPropertiesRobots
            // 
            this.btnPropertiesRobots.Location = new System.Drawing.Point(6, 3);
            this.btnPropertiesRobots.Name = "btnPropertiesRobots";
            this.btnPropertiesRobots.Size = new System.Drawing.Size(160, 23);
            this.btnPropertiesRobots.TabIndex = 38;
            this.btnPropertiesRobots.Tag = "TitleRobotParameters";
            this.btnPropertiesRobots.Text = "Параметры робота(ов)";
            this.btnPropertiesRobots.UseVisualStyleBackColor = true;
            this.btnPropertiesRobots.Click += new System.EventHandler(this.BtnPropertiesRobotsClick);
            // 
            // RobotPortfolioSetupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Name = "RobotPortfolioSetupControl";
            this.Size = new System.Drawing.Size(600, 159);
            this.Load += new System.EventHandler(this.RobotPortfolioSetupControlLoad);
            this.panelLeft.ResumeLayout(false);
            this.panelSelUnsel.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveRobotsPortfolioDlg;
        private System.Windows.Forms.OpenFileDialog openRobotsPortfolioDlg;
        private System.Windows.Forms.ContextMenuStrip menuMagic;
        private System.Windows.Forms.ImageList imageListGlypth;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.ListBox lbRobotsCollection;
        private System.Windows.Forms.Panel panelSelUnsel;
        private System.Windows.Forms.Button BtnUnselectRobot;
        private System.Windows.Forms.Button BtnSelectRobot;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSaveProperties;
        private System.Windows.Forms.Button btnReadProperties;
        private System.Windows.Forms.Button btnPropertiesRobots;
        private FastGrid.FastGrid gridRobot;
    }
}
