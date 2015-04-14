namespace TradeSharp.Client.Controls
{
    partial class TickersSelectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TickersSelectForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.txtEditSelecting = new System.Windows.Forms.Label();
            this.cbEditSelecting = new System.Windows.Forms.ComboBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.gridAllTickers = new FastGrid.FastGrid();
            this.imageListIsFavorite = new System.Windows.Forms.ImageList(this.components);
            this.imageListIsSelected = new System.Windows.Forms.ImageList(this.components);
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.txtEditSelecting);
            this.panelTop.Controls.Add(this.cbEditSelecting);
            this.panelTop.Controls.Add(this.btnCancel);
            this.panelTop.Controls.Add(this.btnAccept);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(364, 47);
            this.panelTop.TabIndex = 0;
            // 
            // txtEditSelecting
            // 
            this.txtEditSelecting.AutoSize = true;
            this.txtEditSelecting.Location = new System.Drawing.Point(197, 17);
            this.txtEditSelecting.Name = "txtEditSelecting";
            this.txtEditSelecting.Size = new System.Drawing.Size(51, 13);
            this.txtEditSelecting.TabIndex = 5;
            this.txtEditSelecting.Text = "Выбрать";
            // 
            // cbEditSelecting
            // 
            this.cbEditSelecting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEditSelecting.FormattingEnabled = true;
            this.cbEditSelecting.Items.AddRange(new object[] {
            "Все",
            "Избранные",
            "Очистить"});
            this.cbEditSelecting.Location = new System.Drawing.Point(254, 12);
            this.cbEditSelecting.Name = "cbEditSelecting";
            this.cbEditSelecting.Size = new System.Drawing.Size(90, 21);
            this.cbEditSelecting.TabIndex = 4;
            this.cbEditSelecting.SelectedIndexChanged += new System.EventHandler(this.CbEditSelectingSelectedIndexChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(97, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(79, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(12, 12);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(79, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // gridAllTickers
            // 
            this.gridAllTickers.CaptionHeight = 20;
            this.gridAllTickers.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridAllTickers.CellHeight = 18;
            this.gridAllTickers.CellPadding = 5;
            this.gridAllTickers.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.gridAllTickers.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridAllTickers.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridAllTickers.ColorCellFont = System.Drawing.Color.Black;
            this.gridAllTickers.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridAllTickers.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridAllTickers.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(242)))), ((int)(((byte)(228)))));
            this.gridAllTickers.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridAllTickers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridAllTickers.FitWidth = false;
            this.gridAllTickers.FontAnchoredRow = null;
            this.gridAllTickers.FontCell = null;
            this.gridAllTickers.FontHeader = null;
            this.gridAllTickers.FontSelectedCell = null;
            this.gridAllTickers.Location = new System.Drawing.Point(0, 47);
            this.gridAllTickers.MinimumTableWidth = null;
            this.gridAllTickers.MultiSelectEnabled = false;
            this.gridAllTickers.Name = "gridAllTickers";
            this.gridAllTickers.SelectEnabled = true;
            this.gridAllTickers.Size = new System.Drawing.Size(364, 265);
            this.gridAllTickers.StickFirst = false;
            this.gridAllTickers.StickLast = false;
            this.gridAllTickers.TabIndex = 1;
            // 
            // imageListIsFavorite
            // 
            this.imageListIsFavorite.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIsFavorite.ImageStream")));
            this.imageListIsFavorite.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIsFavorite.Images.SetKeyName(0, "True");
            // 
            // imageListIsSelected
            // 
            this.imageListIsSelected.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIsSelected.ImageStream")));
            this.imageListIsSelected.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIsSelected.Images.SetKeyName(0, "True");
            // 
            // TickersSelectForm
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 312);
            this.Controls.Add(this.gridAllTickers);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(380, 350);
            this.MinimumSize = new System.Drawing.Size(380, 98);
            this.Name = "TickersSelectForm";
            this.Text = "Котировки";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.ComboBox cbEditSelecting;
        private FastGrid.FastGrid gridAllTickers;
        private System.Windows.Forms.ImageList imageListIsFavorite;
        private System.Windows.Forms.ImageList imageListIsSelected;
        private System.Windows.Forms.Label txtEditSelecting;
    }
}