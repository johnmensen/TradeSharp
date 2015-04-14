namespace VisualResxMergeTool
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnLoad = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.labelDeltaNodes = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnMakeResult = new System.Windows.Forms.Button();
            this.cbAddFromTheirs = new System.Windows.Forms.CheckBox();
            this.cbDeleteFromMine = new System.Windows.Forms.CheckBox();
            this.gridModified = new FastGrid.FastGrid();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnLoad);
            this.panelTop.Controls.Add(this.labelDeltaNodes);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(521, 42);
            this.panelTop.TabIndex = 0;
            // 
            // btnLoad
            // 
            this.btnLoad.ImageIndex = 0;
            this.btnLoad.ImageList = this.imageList1;
            this.btnLoad.Location = new System.Drawing.Point(3, 3);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(32, 27);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ico load.png");
            // 
            // labelDeltaNodes
            // 
            this.labelDeltaNodes.AutoSize = true;
            this.labelDeltaNodes.Location = new System.Drawing.Point(167, 9);
            this.labelDeltaNodes.Name = "labelDeltaNodes";
            this.labelDeltaNodes.Size = new System.Drawing.Size(19, 13);
            this.labelDeltaNodes.TabIndex = 1;
            this.labelDeltaNodes.Text = "+0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(81, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "узлов в Theirs:";
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnMakeResult);
            this.panelBottom.Controls.Add(this.cbAddFromTheirs);
            this.panelBottom.Controls.Add(this.cbDeleteFromMine);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 266);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(521, 53);
            this.panelBottom.TabIndex = 1;
            // 
            // btnMakeResult
            // 
            this.btnMakeResult.Location = new System.Drawing.Point(190, 18);
            this.btnMakeResult.Name = "btnMakeResult";
            this.btnMakeResult.Size = new System.Drawing.Size(212, 23);
            this.btnMakeResult.TabIndex = 2;
            this.btnMakeResult.Text = "Сформировать файл результата";
            this.btnMakeResult.UseVisualStyleBackColor = true;
            this.btnMakeResult.Click += new System.EventHandler(this.btnMakeResult_Click);
            // 
            // cbAddFromTheirs
            // 
            this.cbAddFromTheirs.AutoSize = true;
            this.cbAddFromTheirs.Checked = true;
            this.cbAddFromTheirs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAddFromTheirs.Location = new System.Drawing.Point(12, 26);
            this.cbAddFromTheirs.Name = "cbAddFromTheirs";
            this.cbAddFromTheirs.Size = new System.Drawing.Size(131, 17);
            this.cbAddFromTheirs.TabIndex = 1;
            this.cbAddFromTheirs.Text = "добавить из THEIRS";
            this.cbAddFromTheirs.UseVisualStyleBackColor = true;
            // 
            // cbDeleteFromMine
            // 
            this.cbDeleteFromMine.AutoSize = true;
            this.cbDeleteFromMine.Location = new System.Drawing.Point(12, 3);
            this.cbDeleteFromMine.Name = "cbDeleteFromMine";
            this.cbDeleteFromMine.Size = new System.Drawing.Size(111, 17);
            this.cbDeleteFromMine.TabIndex = 0;
            this.cbDeleteFromMine.Text = "удалить из MINE";
            this.cbDeleteFromMine.UseVisualStyleBackColor = true;
            // 
            // gridModified
            // 
            this.gridModified.CaptionHeight = 20;
            this.gridModified.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridModified.CellHeight = 18;
            this.gridModified.CellPadding = 5;
            this.gridModified.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridModified.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridModified.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridModified.ColorCellFont = System.Drawing.Color.Black;
            this.gridModified.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridModified.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridModified.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridModified.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridModified.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridModified.FitWidth = false;
            this.gridModified.FontAnchoredRow = null;
            this.gridModified.FontCell = null;
            this.gridModified.FontHeader = null;
            this.gridModified.FontSelectedCell = null;
            this.gridModified.Location = new System.Drawing.Point(0, 42);
            this.gridModified.MinimumTableWidth = null;
            this.gridModified.MultiSelectEnabled = false;
            this.gridModified.Name = "gridModified";
            this.gridModified.SelectEnabled = true;
            this.gridModified.Size = new System.Drawing.Size(521, 224);
            this.gridModified.StickFirst = false;
            this.gridModified.StickLast = false;
            this.gridModified.TabIndex = 2;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "resx";
            this.openFileDialog.Filter = "Resources|*.mine;*.resx|*.resx|*.resx|*.*|*.*";
            this.openFileDialog.FilterIndex = 0;
            this.openFileDialog.Title = "Открыть файл ресурсов";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 319);
            this.Controls.Add(this.gridModified);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.Text = "Merge RESX";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label labelDeltaNodes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBottom;
        private FastGrid.FastGrid gridModified;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnMakeResult;
        private System.Windows.Forms.CheckBox cbAddFromTheirs;
        private System.Windows.Forms.CheckBox cbDeleteFromMine;
    }
}

