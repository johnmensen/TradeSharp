namespace FIXViewer
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.gridLog = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDir = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.btnLoad = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tabPageDetail = new System.Windows.Forms.TabPage();
            this.gridMessage = new System.Windows.Forms.DataGridView();
            this.colMsgTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colURL = new System.Windows.Forms.DataGridViewLinkColumn();
            this.colDescr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.btnParse = new System.Windows.Forms.Button();
            this.tbSeparator = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.обновитьБазуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lbVolumeBySmb = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLog)).BeginInit();
            this.panelLeft.SuspendLayout();
            this.tabPageDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMessage)).BeginInit();
            this.panelTop.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageLog);
            this.tabControl.Controls.Add(this.tabPageDetail);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(907, 469);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.gridLog);
            this.tabPageLog.Controls.Add(this.panelLeft);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLog.Size = new System.Drawing.Size(899, 443);
            this.tabPageLog.TabIndex = 0;
            this.tabPageLog.Text = "Лог";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // gridLog
            // 
            this.gridLog.AllowUserToAddRows = false;
            this.gridLog.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridLog.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.gridLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colDir,
            this.colType,
            this.colResult});
            this.gridLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLog.Location = new System.Drawing.Point(3, 38);
            this.gridLog.Name = "gridLog";
            this.gridLog.Size = new System.Drawing.Size(893, 402);
            this.gridLog.TabIndex = 1;
            this.gridLog.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridLogCellDoubleClick);
            this.gridLog.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridLogCellFormatting);
            this.gridLog.SelectionChanged += new System.EventHandler(this.GridLogSelectionChanged);
            // 
            // colTime
            // 
            this.colTime.DataPropertyName = "Time";
            this.colTime.HeaderText = "Время";
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            // 
            // colDir
            // 
            this.colDir.DataPropertyName = "Direction";
            this.colDir.HeaderText = "Направление";
            this.colDir.Name = "colDir";
            this.colDir.ReadOnly = true;
            // 
            // colType
            // 
            this.colType.DataPropertyName = "Type";
            this.colType.HeaderText = "Тип";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            // 
            // colResult
            // 
            this.colResult.DataPropertyName = "Result";
            this.colResult.HeaderText = "Результат";
            this.colResult.Name = "colResult";
            this.colResult.ReadOnly = true;
            this.colResult.Width = 160;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.lbVolumeBySmb);
            this.panelLeft.Controls.Add(this.btnLoad);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLeft.Location = new System.Drawing.Point(3, 3);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(893, 35);
            this.panelLeft.TabIndex = 0;
            // 
            // btnLoad
            // 
            this.btnLoad.ImageIndex = 0;
            this.btnLoad.ImageList = this.imageList;
            this.btnLoad.Location = new System.Drawing.Point(3, 3);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(33, 29);
            this.btnLoad.TabIndex = 0;
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoadClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico load.png");
            // 
            // tabPageDetail
            // 
            this.tabPageDetail.Controls.Add(this.gridMessage);
            this.tabPageDetail.Controls.Add(this.panelTop);
            this.tabPageDetail.Controls.Add(this.menuStrip);
            this.tabPageDetail.Location = new System.Drawing.Point(4, 22);
            this.tabPageDetail.Name = "tabPageDetail";
            this.tabPageDetail.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDetail.Size = new System.Drawing.Size(899, 443);
            this.tabPageDetail.TabIndex = 1;
            this.tabPageDetail.Text = "Подробно";
            this.tabPageDetail.UseVisualStyleBackColor = true;
            // 
            // gridMessage
            // 
            this.gridMessage.AllowUserToAddRows = false;
            this.gridMessage.AllowUserToDeleteRows = false;
            this.gridMessage.AllowUserToOrderColumns = true;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridMessage.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            this.gridMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMsgTag,
            this.colTitle,
            this.colURL,
            this.colDescr,
            this.colValue});
            this.gridMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMessage.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gridMessage.Location = new System.Drawing.Point(3, 52);
            this.gridMessage.Name = "gridMessage";
            this.gridMessage.ReadOnly = true;
            this.gridMessage.Size = new System.Drawing.Size(893, 388);
            this.gridMessage.TabIndex = 5;
            // 
            // colMsgTag
            // 
            this.colMsgTag.DataPropertyName = "Tag";
            this.colMsgTag.HeaderText = "Тег";
            this.colMsgTag.Name = "colMsgTag";
            this.colMsgTag.ReadOnly = true;
            // 
            // colTitle
            // 
            this.colTitle.DataPropertyName = "Title";
            this.colTitle.HeaderText = "Заголовок";
            this.colTitle.Name = "colTitle";
            this.colTitle.ReadOnly = true;
            // 
            // colURL
            // 
            this.colURL.DataPropertyName = "URL";
            this.colURL.HeaderText = "URL";
            this.colURL.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.colURL.Name = "colURL";
            this.colURL.ReadOnly = true;
            this.colURL.TrackVisitedState = false;
            // 
            // colDescr
            // 
            this.colDescr.DataPropertyName = "Description";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.colDescr.DefaultCellStyle = dataGridViewCellStyle3;
            this.colDescr.HeaderText = "Описание";
            this.colDescr.Name = "colDescr";
            this.colDescr.ReadOnly = true;
            this.colDescr.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDescr.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colValue
            // 
            this.colValue.DataPropertyName = "Value";
            this.colValue.HeaderText = "Значение";
            this.colValue.Name = "colValue";
            this.colValue.ReadOnly = true;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.tbMsg);
            this.panelTop.Controls.Add(this.btnParse);
            this.panelTop.Controls.Add(this.tbSeparator);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(3, 3);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(893, 49);
            this.panelTop.TabIndex = 4;
            // 
            // tbMsg
            // 
            this.tbMsg.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbMsg.Location = new System.Drawing.Point(0, 0);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(893, 20);
            this.tbMsg.TabIndex = 3;
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(140, 22);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(75, 23);
            this.btnParse.TabIndex = 2;
            this.btnParse.Text = "Разбор";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.BtnParseClick);
            // 
            // tbSeparator
            // 
            this.tbSeparator.Location = new System.Drawing.Point(75, 23);
            this.tbSeparator.Name = "tbSeparator";
            this.tbSeparator.Size = new System.Drawing.Size(59, 20);
            this.tbSeparator.TabIndex = 1;
            this.tbSeparator.Text = "#";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "разделитель";
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(3, 3);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(893, 24);
            this.menuStrip.TabIndex = 3;
            this.menuStrip.Text = "menuStrip1";
            this.menuStrip.Visible = false;
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.обновитьБазуToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // обновитьБазуToolStripMenuItem
            // 
            this.обновитьБазуToolStripMenuItem.Name = "обновитьБазуToolStripMenuItem";
            this.обновитьБазуToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.обновитьБазуToolStripMenuItem.Text = "Обновить базу...";
            this.обновитьБазуToolStripMenuItem.Click += new System.EventHandler(this.BtnUpdateBaseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(184, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "опциональный разделитель";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "txt";
            this.openFileDialog.FileName = "log-file.txt";
            this.openFileDialog.Filter = "Txt Files (*.txt)|*.txt|All Files|*.*";
            this.openFileDialog.FilterIndex = 0;
            // 
            // lbVolumeBySmb
            // 
            this.lbVolumeBySmb.AutoSize = true;
            this.lbVolumeBySmb.Location = new System.Drawing.Point(42, 11);
            this.lbVolumeBySmb.Name = "lbVolumeBySmb";
            this.lbVolumeBySmb.Size = new System.Drawing.Size(10, 13);
            this.lbVolumeBySmb.TabIndex = 1;
            this.lbVolumeBySmb.Text = "-";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(907, 469);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.Text = "FIXViewer";
            this.tabControl.ResumeLayout(false);
            this.tabPageLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridLog)).EndInit();
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.tabPageDetail.ResumeLayout(false);
            this.tabPageDetail.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridMessage)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.DataGridView gridLog;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.TabPage tabPageDetail;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem обновитьБазуToolStripMenuItem;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox tbSeparator;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.DataGridView gridMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMsgTag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewLinkColumn colURL;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescr;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDir;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResult;
        private System.Windows.Forms.Label lbVolumeBySmb;
    }
}

