namespace TradeSharp.UI.Util.Forms
{
    partial class ExportPositionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportPositionsForm));
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerTop = new System.Windows.Forms.SplitContainer();
            this.cbTimeFormat = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbDigitSeparator = new System.Windows.Forms.ComboBox();
            this.cbGroupSeparator = new System.Windows.Forms.ComboBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnSaveLoad = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.treeView = new System.Windows.Forms.TreeView();
            this.grid = new FastGrid.FastGrid();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
            this.splitContainerTop.Panel1.SuspendLayout();
            this.splitContainerTop.Panel2.SuspendLayout();
            this.splitContainerTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerTop);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.grid);
            this.splitContainerMain.Size = new System.Drawing.Size(533, 395);
            this.splitContainerMain.SplitterDistance = 177;
            this.splitContainerMain.TabIndex = 0;
            // 
            // splitContainerTop
            // 
            this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTop.Location = new System.Drawing.Point(0, 0);
            this.splitContainerTop.Name = "splitContainerTop";
            // 
            // splitContainerTop.Panel1
            // 
            this.splitContainerTop.Panel1.Controls.Add(this.cbTimeFormat);
            this.splitContainerTop.Panel1.Controls.Add(this.label3);
            this.splitContainerTop.Panel1.Controls.Add(this.label2);
            this.splitContainerTop.Panel1.Controls.Add(this.label1);
            this.splitContainerTop.Panel1.Controls.Add(this.cbDigitSeparator);
            this.splitContainerTop.Panel1.Controls.Add(this.cbGroupSeparator);
            this.splitContainerTop.Panel1.Controls.Add(this.btnPreview);
            this.splitContainerTop.Panel1.Controls.Add(this.btnSaveLoad);
            // 
            // splitContainerTop.Panel2
            // 
            this.splitContainerTop.Panel2.Controls.Add(this.treeView);
            this.splitContainerTop.Size = new System.Drawing.Size(533, 177);
            this.splitContainerTop.SplitterDistance = 266;
            this.splitContainerTop.TabIndex = 0;
            // 
            // cbTimeFormat
            // 
            this.cbTimeFormat.FormattingEnabled = true;
            this.cbTimeFormat.Location = new System.Drawing.Point(3, 86);
            this.cbTimeFormat.Name = "cbTimeFormat";
            this.cbTimeFormat.Size = new System.Drawing.Size(133, 21);
            this.cbTimeFormat.TabIndex = 8;
            this.cbTimeFormat.Text = "dd.MM.yyyy HH:mm:ss";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(142, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "формат времени";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(142, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "десятичная точка";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(142, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "разделитель групп";
            // 
            // cbDigitSeparator
            // 
            this.cbDigitSeparator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDigitSeparator.FormattingEnabled = true;
            this.cbDigitSeparator.Items.AddRange(new object[] {
            "Точка",
            "Запятая"});
            this.cbDigitSeparator.Location = new System.Drawing.Point(3, 59);
            this.cbDigitSeparator.Name = "cbDigitSeparator";
            this.cbDigitSeparator.Size = new System.Drawing.Size(133, 21);
            this.cbDigitSeparator.TabIndex = 3;
            // 
            // cbGroupSeparator
            // 
            this.cbGroupSeparator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGroupSeparator.FormattingEnabled = true;
            this.cbGroupSeparator.Items.AddRange(new object[] {
            "Табуляция",
            "Точка с запятой",
            "Запятая",
            "Решетка"});
            this.cbGroupSeparator.Location = new System.Drawing.Point(3, 32);
            this.cbGroupSeparator.Name = "cbGroupSeparator";
            this.cbGroupSeparator.Size = new System.Drawing.Size(133, 21);
            this.cbGroupSeparator.TabIndex = 2;
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(0, 112);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(136, 23);
            this.btnPreview.TabIndex = 1;
            this.btnPreview.Text = "Просмотреть";
            this.btnPreview.UseVisualStyleBackColor = true;
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ImageIndex = 0;
            this.btnSaveLoad.ImageList = this.imageList;
            this.btnSaveLoad.Location = new System.Drawing.Point(3, 3);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.Size = new System.Drawing.Size(28, 23);
            this.btnSaveLoad.TabIndex = 0;
            this.btnSaveLoad.UseVisualStyleBackColor = true;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico load.png");
            this.imageList.Images.SetKeyName(1, "ico save.png");
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(263, 177);
            this.treeView.TabIndex = 0;
            // 
            // grid
            // 
            this.grid.CaptionHeight = 20;
            this.grid.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.grid.CellHeight = 18;
            this.grid.CellPadding = 5;
            this.grid.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.grid.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.grid.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.grid.ColorCellFont = System.Drawing.Color.Black;
            this.grid.ColorCellOutlineLower = System.Drawing.Color.White;
            this.grid.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.grid.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.grid.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.FitWidth = false;
            this.grid.FontAnchoredRow = null;
            this.grid.FontCell = null;
            this.grid.FontHeader = null;
            this.grid.FontSelectedCell = null;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.MinimumTableWidth = null;
            this.grid.MultiSelectEnabled = false;
            this.grid.Name = "grid";
            this.grid.SelectEnabled = true;
            this.grid.Size = new System.Drawing.Size(533, 214);
            this.grid.StickFirst = false;
            this.grid.StickLast = false;
            this.grid.TabIndex = 0;
            // 
            // ExportPositionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 395);
            this.Controls.Add(this.splitContainerMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportPositionsForm";
            this.Text = "Экспорт сделок";
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerTop.Panel1.ResumeLayout(false);
            this.splitContainerTop.Panel1.PerformLayout();
            this.splitContainerTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
            this.splitContainerTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerTop;
        private System.Windows.Forms.ComboBox cbTimeFormat;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbDigitSeparator;
        private System.Windows.Forms.ComboBox cbGroupSeparator;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnSaveLoad;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TreeView treeView;
        private FastGrid.FastGrid grid;
    }
}