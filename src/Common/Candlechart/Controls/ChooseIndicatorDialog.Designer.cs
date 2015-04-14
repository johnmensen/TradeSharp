namespace Candlechart.Controls
{
    partial class ChooseIndicatorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseIndicatorDialog));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnTable = new System.Windows.Forms.Button();
            this.imageListGlypth = new System.Windows.Forms.ImageList(this.components);
            this.btnTree = new System.Windows.Forms.Button();
            this.treeView = new System.Windows.Forms.TreeView();
            this.gridView = new FastGrid.FastGrid();
            this.imageListGrid = new System.Windows.Forms.ImageList(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(343, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.AutoSize = true;
            this.btnOK.Location = new System.Drawing.Point(262, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Tag = "TitleOK";
            this.btnOK.Text = "Выбрать";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // btnTable
            // 
            this.btnTable.AutoSize = true;
            this.btnTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTable.ImageIndex = 1;
            this.btnTable.ImageList = this.imageListGlypth;
            this.btnTable.Location = new System.Drawing.Point(35, 3);
            this.btnTable.Name = "btnTable";
            this.btnTable.Size = new System.Drawing.Size(24, 24);
            this.btnTable.TabIndex = 1;
            this.btnTable.UseVisualStyleBackColor = true;
            this.btnTable.Click += new System.EventHandler(this.BtnTableClick);
            // 
            // imageListGlypth
            // 
            this.imageListGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlypth.ImageStream")));
            this.imageListGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlypth.Images.SetKeyName(0, "ico_tree_view.png");
            this.imageListGlypth.Images.SetKeyName(1, "ico_table_view.png");
            // 
            // btnTree
            // 
            this.btnTree.AutoSize = true;
            this.btnTree.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnTree.FlatAppearance.BorderSize = 2;
            this.btnTree.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTree.ImageIndex = 0;
            this.btnTree.ImageList = this.imageListGlypth;
            this.btnTree.Location = new System.Drawing.Point(3, 3);
            this.btnTree.Name = "btnTree";
            this.btnTree.Size = new System.Drawing.Size(26, 26);
            this.btnTree.TabIndex = 0;
            this.btnTree.UseVisualStyleBackColor = true;
            this.btnTree.Click += new System.EventHandler(this.BtnTreeClick);
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 32);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(421, 324);
            this.treeView.TabIndex = 2;
            this.treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewNodeMouseDoubleClick);
            // 
            // gridView
            // 
            this.gridView.CaptionHeight = 20;
            this.gridView.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridView.CellHeight = 18;
            this.gridView.CellPadding = 5;
            this.gridView.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridView.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridView.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridView.ColorCellFont = System.Drawing.Color.Black;
            this.gridView.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridView.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridView.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridView.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridView.FitWidth = true;
            this.gridView.FontAnchoredRow = null;
            this.gridView.FontCell = null;
            this.gridView.FontHeader = null;
            this.gridView.FontSelectedCell = null;
            this.gridView.Location = new System.Drawing.Point(0, 32);
            this.gridView.MinimumTableWidth = null;
            this.gridView.MultiSelectEnabled = false;
            this.gridView.Name = "gridView";
            this.gridView.SelectEnabled = true;
            this.gridView.Size = new System.Drawing.Size(421, 324);
            this.gridView.StickFirst = false;
            this.gridView.StickLast = false;
            this.gridView.TabIndex = 3;
            this.gridView.Visible = false;
            this.gridView.UserHitCell += new FastGrid.UserHitCellDel(this.GridViewUserHitCell);
            // 
            // imageListGrid
            // 
            this.imageListGrid.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGrid.ImageStream")));
            this.imageListGrid.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGrid.Images.SetKeyName(0, "True");
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnTree);
            this.flowLayoutPanel1.Controls.Add(this.btnTable);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(421, 32);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.btnCancel);
            this.flowLayoutPanel2.Controls.Add(this.btnOK);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 356);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(421, 29);
            this.flowLayoutPanel2.TabIndex = 5;
            // 
            // ChooseIndicatorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 385);
            this.Controls.Add(this.gridView);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChooseIndicatorDialog";
            this.Tag = "TitleChooseIndicator";
            this.Text = "Выбрать индикатор";
            this.Load += new System.EventHandler(this.ChooseIndicatorDialogLoad);
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.ChooseIndicatorDialogHelpRequested);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageListGlypth;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnTable;
        private System.Windows.Forms.Button btnTree;
        private System.Windows.Forms.TreeView treeView;
        private FastGrid.FastGrid gridView;
        private System.Windows.Forms.ImageList imageListGrid;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}