namespace QuoteManager
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageTotal = new System.Windows.Forms.TabPage();
            this.splitContainerInfo = new System.Windows.Forms.SplitContainer();
            this.boxInfo = new System.Windows.Forms.RichTextBox();
            this.gridInfo = new FastGrid.FastGrid();
            this.contextMenuInfo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuitemTrimHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.panelInfoTop = new System.Windows.Forms.Panel();
            this.btnBrowseQuoteFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbQuoteFolder = new System.Windows.Forms.TextBox();
            this.pageGaps = new System.Windows.Forms.TabPage();
            this.menuitemMakeIndexes = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl.SuspendLayout();
            this.pageTotal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInfo)).BeginInit();
            this.splitContainerInfo.Panel1.SuspendLayout();
            this.splitContainerInfo.Panel2.SuspendLayout();
            this.splitContainerInfo.SuspendLayout();
            this.contextMenuInfo.SuspendLayout();
            this.panelInfoTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageTotal);
            this.tabControl.Controls.Add(this.pageGaps);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(598, 447);
            this.tabControl.TabIndex = 0;
            // 
            // pageTotal
            // 
            this.pageTotal.Controls.Add(this.splitContainerInfo);
            this.pageTotal.Controls.Add(this.panelInfoTop);
            this.pageTotal.Location = new System.Drawing.Point(4, 22);
            this.pageTotal.Name = "pageTotal";
            this.pageTotal.Padding = new System.Windows.Forms.Padding(3);
            this.pageTotal.Size = new System.Drawing.Size(590, 421);
            this.pageTotal.TabIndex = 0;
            this.pageTotal.Text = "Информация";
            this.pageTotal.UseVisualStyleBackColor = true;
            // 
            // splitContainerInfo
            // 
            this.splitContainerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInfo.Location = new System.Drawing.Point(3, 54);
            this.splitContainerInfo.Name = "splitContainerInfo";
            // 
            // splitContainerInfo.Panel1
            // 
            this.splitContainerInfo.Panel1.Controls.Add(this.boxInfo);
            // 
            // splitContainerInfo.Panel2
            // 
            this.splitContainerInfo.Panel2.Controls.Add(this.gridInfo);
            this.splitContainerInfo.Size = new System.Drawing.Size(584, 364);
            this.splitContainerInfo.SplitterDistance = 194;
            this.splitContainerInfo.TabIndex = 1;
            // 
            // boxInfo
            // 
            this.boxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.boxInfo.Location = new System.Drawing.Point(0, 0);
            this.boxInfo.Name = "boxInfo";
            this.boxInfo.Size = new System.Drawing.Size(194, 364);
            this.boxInfo.TabIndex = 2;
            this.boxInfo.Text = "";
            // 
            // gridInfo
            // 
            this.gridInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridInfo.CaptionHeight = 20;
            this.gridInfo.CellHeight = 18;
            this.gridInfo.CellPadding = 5;
            this.gridInfo.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridInfo.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridInfo.ColorCellFont = System.Drawing.Color.Black;
            this.gridInfo.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridInfo.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridInfo.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.gridInfo.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridInfo.ContextMenuStrip = this.contextMenuInfo;
            this.gridInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInfo.FontCell = null;
            this.gridInfo.FontHeader = null;
            this.gridInfo.FontSelectedCell = null;
            this.gridInfo.Location = new System.Drawing.Point(0, 0);
            this.gridInfo.MinimumTableWidth = null;
            this.gridInfo.MultiSelectEnabled = false;
            this.gridInfo.Name = "gridInfo";
            this.gridInfo.SelectEnabled = true;
            this.gridInfo.Size = new System.Drawing.Size(386, 364);
            this.gridInfo.TabIndex = 0;
            // 
            // contextMenuInfo
            // 
            this.contextMenuInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemTrimHistory,
            this.menuitemMakeIndexes});
            this.contextMenuInfo.Name = "contextMenuInfo";
            this.contextMenuInfo.Size = new System.Drawing.Size(198, 70);
            // 
            // menuitemTrimHistory
            // 
            this.menuitemTrimHistory.Name = "menuitemTrimHistory";
            this.menuitemTrimHistory.Size = new System.Drawing.Size(197, 22);
            this.menuitemTrimHistory.Text = "Вырезать историю...";
            this.menuitemTrimHistory.Click += new System.EventHandler(this.MenuitemTrimHistoryClick);
            // 
            // panelInfoTop
            // 
            this.panelInfoTop.Controls.Add(this.btnBrowseQuoteFolder);
            this.panelInfoTop.Controls.Add(this.label1);
            this.panelInfoTop.Controls.Add(this.tbQuoteFolder);
            this.panelInfoTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInfoTop.Location = new System.Drawing.Point(3, 3);
            this.panelInfoTop.Name = "panelInfoTop";
            this.panelInfoTop.Size = new System.Drawing.Size(584, 51);
            this.panelInfoTop.TabIndex = 0;
            // 
            // btnBrowseQuoteFolder
            // 
            this.btnBrowseQuoteFolder.Location = new System.Drawing.Point(413, 15);
            this.btnBrowseQuoteFolder.Name = "btnBrowseQuoteFolder";
            this.btnBrowseQuoteFolder.Size = new System.Drawing.Size(27, 23);
            this.btnBrowseQuoteFolder.TabIndex = 2;
            this.btnBrowseQuoteFolder.Text = "...";
            this.btnBrowseQuoteFolder.UseVisualStyleBackColor = true;
            this.btnBrowseQuoteFolder.Click += new System.EventHandler(this.BtnBrowseQuoteFolderClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "каталог котировок";
            // 
            // tbQuoteFolder
            // 
            this.tbQuoteFolder.Location = new System.Drawing.Point(5, 17);
            this.tbQuoteFolder.Name = "tbQuoteFolder";
            this.tbQuoteFolder.Size = new System.Drawing.Size(402, 20);
            this.tbQuoteFolder.TabIndex = 0;
            // 
            // pageGaps
            // 
            this.pageGaps.Location = new System.Drawing.Point(4, 22);
            this.pageGaps.Name = "pageGaps";
            this.pageGaps.Padding = new System.Windows.Forms.Padding(3);
            this.pageGaps.Size = new System.Drawing.Size(590, 421);
            this.pageGaps.TabIndex = 1;
            this.pageGaps.Text = "Гэпы";
            this.pageGaps.UseVisualStyleBackColor = true;
            // 
            // menuitemMakeIndexes
            // 
            this.menuitemMakeIndexes.Name = "menuitemMakeIndexes";
            this.menuitemMakeIndexes.Size = new System.Drawing.Size(197, 22);
            this.menuitemMakeIndexes.Text = "Валютные индексы...";
            this.menuitemMakeIndexes.Click += new System.EventHandler(this.MenuitemMakeIndexesClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 447);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.Text = "Котировки";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.tabControl.ResumeLayout(false);
            this.pageTotal.ResumeLayout(false);
            this.splitContainerInfo.Panel1.ResumeLayout(false);
            this.splitContainerInfo.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInfo)).EndInit();
            this.splitContainerInfo.ResumeLayout(false);
            this.contextMenuInfo.ResumeLayout(false);
            this.panelInfoTop.ResumeLayout(false);
            this.panelInfoTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pageGaps;
        private System.Windows.Forms.TabPage pageTotal;
        private System.Windows.Forms.Panel panelInfoTop;
        private System.Windows.Forms.Button btnBrowseQuoteFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbQuoteFolder;
        private System.Windows.Forms.SplitContainer splitContainerInfo;
        private System.Windows.Forms.RichTextBox boxInfo;
        private FastGrid.FastGrid gridInfo;
        private System.Windows.Forms.ContextMenuStrip contextMenuInfo;
        private System.Windows.Forms.ToolStripMenuItem menuitemTrimHistory;
        private System.Windows.Forms.ToolStripMenuItem menuitemMakeIndexes;
    }
}

