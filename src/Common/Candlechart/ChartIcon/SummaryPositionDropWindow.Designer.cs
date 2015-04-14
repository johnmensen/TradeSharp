namespace Candlechart.ChartIcon
{
    partial class SummaryPositionDropWindow
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SummaryPositionDropWindow));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnLinkOrders = new System.Windows.Forms.Button();
            this.btnCloseAll = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.gridDeals = new FastGrid.FastGrid();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico_close_gray.png");
            this.imageList.Images.SetKeyName(1, "ico_pin_16.png");
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnLinkOrders);
            this.panelTop.Controls.Add(this.btnCloseAll);
            this.panelTop.Controls.Add(this.btnClose);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(2);
            this.panelTop.Size = new System.Drawing.Size(370, 28);
            this.panelTop.TabIndex = 2;
            // 
            // btnLinkOrders
            // 
            this.btnLinkOrders.Enabled = false;
            this.btnLinkOrders.Location = new System.Drawing.Point(110, 3);
            this.btnLinkOrders.Name = "btnLinkOrders";
            this.btnLinkOrders.Size = new System.Drawing.Size(90, 23);
            this.btnLinkOrders.TabIndex = 4;
            this.btnLinkOrders.Text = "\"Связать\"";
            this.btnLinkOrders.UseVisualStyleBackColor = true;
            this.btnLinkOrders.Click += new System.EventHandler(this.BtnLinkOrdersClick);
            // 
            // btnCloseAll
            // 
            this.btnCloseAll.Location = new System.Drawing.Point(3, 3);
            this.btnCloseAll.Name = "btnCloseAll";
            this.btnCloseAll.Size = new System.Drawing.Size(90, 23);
            this.btnCloseAll.TabIndex = 3;
            this.btnCloseAll.Text = "Закрыть все";
            this.btnCloseAll.UseVisualStyleBackColor = true;
            this.btnCloseAll.Click += new System.EventHandler(this.BtnCloseAllClick);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ImageIndex = 1;
            this.btnClose.ImageList = this.imageList;
            this.btnClose.Location = new System.Drawing.Point(346, 2);
            this.btnClose.Margin = new System.Windows.Forms.Padding(3, 3, 5, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(22, 24);
            this.btnClose.TabIndex = 2;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // splitContainer
            // 
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 28);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.gridDeals);
            this.splitContainer.Size = new System.Drawing.Size(370, 252);
            this.splitContainer.SplitterDistance = 222;
            this.splitContainer.TabIndex = 3;
            // 
            // gridDeals
            // 
            this.gridDeals.CaptionHeight = 20;
            this.gridDeals.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridDeals.CellHeight = 18;
            this.gridDeals.CellPadding = 5;
            this.gridDeals.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridDeals.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridDeals.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridDeals.ColorCellFont = System.Drawing.Color.Black;
            this.gridDeals.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridDeals.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridDeals.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridDeals.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridDeals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridDeals.FontAnchoredRow = null;
            this.gridDeals.FontCell = null;
            this.gridDeals.FontHeader = null;
            this.gridDeals.FontSelectedCell = null;
            this.gridDeals.Location = new System.Drawing.Point(0, 0);
            this.gridDeals.MinimumTableWidth = null;
            this.gridDeals.MultiSelectEnabled = false;
            this.gridDeals.Name = "gridDeals";
            this.gridDeals.SelectEnabled = true;
            this.gridDeals.Size = new System.Drawing.Size(368, 220);
            this.gridDeals.StickFirst = false;
            this.gridDeals.StickLast = false;
            this.gridDeals.TabIndex = 0;
            // 
            // SummaryPositionDropWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelTop);
            this.Name = "SummaryPositionDropWindow";
            this.Size = new System.Drawing.Size(370, 280);
            this.Load += new System.EventHandler(this.SummaryPositionDropWindowLoad);
            this.panelTop.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.SplitContainer splitContainer;
        private FastGrid.FastGrid gridDeals;
        private System.Windows.Forms.Button btnLinkOrders;
        private System.Windows.Forms.Button btnCloseAll;
    }
}
