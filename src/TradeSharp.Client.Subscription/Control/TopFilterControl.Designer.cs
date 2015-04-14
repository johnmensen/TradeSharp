namespace TradeSharp.Client.Subscription.Control
{
    partial class TopFilterControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TopFilterControl));
            this.sortTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.expressionEditorButton = new System.Windows.Forms.Button();
            this.showExpressionLabelButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.collapseButton = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.refreshButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.createPortfolioButton = new System.Windows.Forms.Button();
            this.gridImageList = new System.Windows.Forms.ImageList(this.components);
            this.operatorsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.greaterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.equalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expressionLabel = new System.Windows.Forms.TextBox();
            this.fastGrid = new FastGrid.FastGrid();
            this.timerWhistlerFarter = new System.Windows.Forms.Timer(this.components);
            this.sortTableLayoutPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.operatorsContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // sortTableLayoutPanel
            // 
            this.sortTableLayoutPanel.AutoSize = true;
            this.sortTableLayoutPanel.ColumnCount = 3;
            this.sortTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.sortTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.sortTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.sortTableLayoutPanel.Controls.Add(this.expressionEditorButton, 2, 0);
            this.sortTableLayoutPanel.Controls.Add(this.showExpressionLabelButton, 1, 0);
            this.sortTableLayoutPanel.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.sortTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sortTableLayoutPanel.Location = new System.Drawing.Point(0, 296);
            this.sortTableLayoutPanel.Name = "sortTableLayoutPanel";
            this.sortTableLayoutPanel.RowCount = 1;
            this.sortTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.sortTableLayoutPanel.Size = new System.Drawing.Size(695, 29);
            this.sortTableLayoutPanel.TabIndex = 5;
            // 
            // expressionEditorButton
            // 
            this.expressionEditorButton.AutoSize = true;
            this.expressionEditorButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.expressionEditorButton.Image = ((System.Drawing.Image)(resources.GetObject("expressionEditorButton.Image")));
            this.expressionEditorButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.expressionEditorButton.Location = new System.Drawing.Point(560, 3);
            this.expressionEditorButton.Name = "expressionEditorButton";
            this.expressionEditorButton.Size = new System.Drawing.Size(132, 23);
            this.expressionEditorButton.TabIndex = 2;
            this.expressionEditorButton.Tag = "TitleExpressionEditorMenu";
            this.expressionEditorButton.Text = "Редактор формул...";
            this.expressionEditorButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.expressionEditorButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.expressionEditorButton.UseVisualStyleBackColor = true;
            this.expressionEditorButton.Click += new System.EventHandler(this.ExpressionEditButtonClick);
            // 
            // showExpressionLabelButton
            // 
            this.showExpressionLabelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.showExpressionLabelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showExpressionLabelButton.Location = new System.Drawing.Point(531, 3);
            this.showExpressionLabelButton.Name = "showExpressionLabelButton";
            this.showExpressionLabelButton.Size = new System.Drawing.Size(23, 23);
            this.showExpressionLabelButton.TabIndex = 4;
            this.showExpressionLabelButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.collapseButton);
            this.flowLayoutPanel1.Controls.Add(this.refreshButton);
            this.flowLayoutPanel1.Controls.Add(this.saveButton);
            this.flowLayoutPanel1.Controls.Add(this.createPortfolioButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(470, 29);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // collapseButton
            // 
            this.collapseButton.AutoSize = true;
            this.collapseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.collapseButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.collapseButton.ImageIndex = 1;
            this.collapseButton.ImageList = this.imageList;
            this.collapseButton.Location = new System.Drawing.Point(3, 3);
            this.collapseButton.Name = "collapseButton";
            this.collapseButton.Size = new System.Drawing.Size(80, 23);
            this.collapseButton.TabIndex = 3;
            this.collapseButton.Tag = "TitleCollapse";
            this.collapseButton.Text = "Свернуть";
            this.collapseButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.collapseButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.collapseButton.UseVisualStyleBackColor = true;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Yellow;
            this.imageList.Images.SetKeyName(0, "ico16 movedown.bmp");
            this.imageList.Images.SetKeyName(1, "ico16 moveup.bmp");
            // 
            // refreshButton
            // 
            this.refreshButton.AutoSize = true;
            this.refreshButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.refreshButton.Enabled = false;
            this.refreshButton.Image = ((System.Drawing.Image)(resources.GetObject("refreshButton.Image")));
            this.refreshButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.refreshButton.Location = new System.Drawing.Point(89, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(144, 23);
            this.refreshButton.TabIndex = 4;
            this.refreshButton.Tag = "TitleShowResults";
            this.refreshButton.Text = "Показать результаты";
            this.refreshButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.refreshButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            this.saveButton.AutoSize = true;
            this.saveButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
            this.saveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.saveButton.Location = new System.Drawing.Point(239, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(95, 23);
            this.saveButton.TabIndex = 1;
            this.saveButton.Tag = "TitleSaveMenu";
            this.saveButton.Text = "Сохранить...";
            this.saveButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.saveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButtonClick);
            // 
            // createPortfolioButton
            // 
            this.createPortfolioButton.AutoSize = true;
            this.createPortfolioButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.createPortfolioButton.Image = ((System.Drawing.Image)(resources.GetObject("createPortfolioButton.Image")));
            this.createPortfolioButton.Location = new System.Drawing.Point(340, 3);
            this.createPortfolioButton.Name = "createPortfolioButton";
            this.createPortfolioButton.Size = new System.Drawing.Size(127, 23);
            this.createPortfolioButton.TabIndex = 5;
            this.createPortfolioButton.Tag = "TitleCreatePortfolio";
            this.createPortfolioButton.Text = "Создать портфель";
            this.createPortfolioButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.createPortfolioButton.UseVisualStyleBackColor = true;
            // 
            // gridImageList
            // 
            this.gridImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("gridImageList.ImageStream")));
            this.gridImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.gridImageList.Images.SetKeyName(0, "True");
            // 
            // operatorsContextMenuStrip
            // 
            this.operatorsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.greaterToolStripMenuItem,
            this.LessToolStripMenuItem,
            this.equalToolStripMenuItem});
            this.operatorsContextMenuStrip.Name = "operatorsContextMenuStrip";
            this.operatorsContextMenuStrip.Size = new System.Drawing.Size(83, 70);
            this.operatorsContextMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.OperatorsContextMenuStripItemClicked);
            // 
            // greaterToolStripMenuItem
            // 
            this.greaterToolStripMenuItem.Name = "greaterToolStripMenuItem";
            this.greaterToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.greaterToolStripMenuItem.Text = ">";
            // 
            // LessToolStripMenuItem
            // 
            this.LessToolStripMenuItem.Name = "LessToolStripMenuItem";
            this.LessToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.LessToolStripMenuItem.Text = "<";
            // 
            // equalToolStripMenuItem
            // 
            this.equalToolStripMenuItem.Name = "equalToolStripMenuItem";
            this.equalToolStripMenuItem.Size = new System.Drawing.Size(82, 22);
            this.equalToolStripMenuItem.Text = "=";
            // 
            // expressionLabel
            // 
            this.expressionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.expressionLabel.Location = new System.Drawing.Point(0, 0);
            this.expressionLabel.Name = "expressionLabel";
            this.expressionLabel.ReadOnly = true;
            this.expressionLabel.Size = new System.Drawing.Size(695, 20);
            this.expressionLabel.TabIndex = 7;
            this.expressionLabel.Visible = false;
            this.expressionLabel.WordWrap = false;
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
            this.fastGrid.Location = new System.Drawing.Point(0, 20);
            this.fastGrid.MinimumTableWidth = null;
            this.fastGrid.MultiSelectEnabled = false;
            this.fastGrid.Name = "fastGrid";
            this.fastGrid.SelectEnabled = false;
            this.fastGrid.Size = new System.Drawing.Size(695, 276);
            this.fastGrid.StickFirst = false;
            this.fastGrid.StickLast = false;
            this.fastGrid.TabIndex = 8;
            this.fastGrid.UserHitCell += new FastGrid.UserHitCellDel(this.FastGridUserHitCell);
            // 
            // timerWhistlerFarter
            // 
            this.timerWhistlerFarter.Enabled = true;
            this.timerWhistlerFarter.Interval = 200;
            this.timerWhistlerFarter.Tick += new System.EventHandler(this.TimerWhistlerFarterTick);
            // 
            // TopFilterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fastGrid);
            this.Controls.Add(this.expressionLabel);
            this.Controls.Add(this.sortTableLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(600, 0);
            this.Name = "TopFilterControl";
            this.Size = new System.Drawing.Size(695, 325);
            this.sortTableLayoutPanel.ResumeLayout(false);
            this.sortTableLayoutPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.operatorsContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel sortTableLayoutPanel;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button expressionEditorButton;
        private System.Windows.Forms.ImageList gridImageList;
        private System.Windows.Forms.ContextMenuStrip operatorsContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem greaterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem LessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem equalToolStripMenuItem;
        private System.Windows.Forms.TextBox expressionLabel;
        private FastGrid.FastGrid fastGrid;
        private System.Windows.Forms.Button showExpressionLabelButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button collapseButton;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button createPortfolioButton;
        private System.Windows.Forms.Timer timerWhistlerFarter;

    }
}