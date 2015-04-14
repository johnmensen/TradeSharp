namespace TradeSharp.Client.Forms
{
    partial class QuoteBaseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuoteBaseForm));
            this.btnExecute = new System.Windows.Forms.Button();
            this.cbAction = new System.Windows.Forms.ComboBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.imageListFavorite = new System.Windows.Forms.ImageList(this.components);
            this.imageListSelected = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.grid = new FastGrid.FastGrid();
            this.standByControl = new TradeSharp.UI.Util.Control.StandByControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(219, 3);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 1;
            this.btnExecute.Tag = "TitleExecute";
            this.btnExecute.Text = "Выполнить";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.BtnExecuteClick);
            // 
            // cbAction
            // 
            this.cbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAction.FormattingEnabled = true;
            this.cbAction.Location = new System.Drawing.Point(3, 3);
            this.cbAction.Name = "cbAction";
            this.cbAction.Size = new System.Drawing.Size(210, 21);
            this.cbAction.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(460, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 1;
            this.closeButton.Tag = "TitleClose";
            this.closeButton.Text = "Закрыть";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // imageListFavorite
            // 
            this.imageListFavorite.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListFavorite.ImageStream")));
            this.imageListFavorite.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListFavorite.Images.SetKeyName(0, "True");
            // 
            // imageListSelected
            // 
            this.imageListSelected.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSelected.ImageStream")));
            this.imageListSelected.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSelected.Images.SetKeyName(0, "True");
            this.imageListSelected.Images.SetKeyName(1, "False");
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.closeButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnExecute, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbAction, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 374);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(538, 29);
            this.tableLayoutPanel1.TabIndex = 5;
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
            this.grid.FitWidth = true;
            this.grid.FontAnchoredRow = null;
            this.grid.FontCell = null;
            this.grid.FontHeader = null;
            this.grid.FontSelectedCell = null;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.MinimumTableWidth = null;
            this.grid.MultiSelectEnabled = false;
            this.grid.Name = "grid";
            this.grid.SelectEnabled = false;
            this.grid.Size = new System.Drawing.Size(538, 374);
            this.grid.StickFirst = false;
            this.grid.StickLast = false;
            this.grid.TabIndex = 9;
            // 
            // standByControl
            // 
            this.standByControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standByControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.standByControl.IsShown = true;
            this.standByControl.Location = new System.Drawing.Point(0, 0);
            this.standByControl.Name = "standByControl";
            this.standByControl.Size = new System.Drawing.Size(538, 374);
            this.standByControl.TabIndex = 10;
            this.standByControl.Tag = "TitleLoading";
            this.standByControl.Text = "Загрузка...";
            this.standByControl.TransparentForm = null;
            // 
            // QuoteBaseForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 403);
            this.Controls.Add(this.standByControl);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(450, 300);
            this.Name = "QuoteBaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleQuoteArchive";
            this.Text = "Архив котировок";
            this.Load += new System.EventHandler(this.QuoteBaseFormLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageListFavorite;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ImageList imageListSelected;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.ComboBox cbAction;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private FastGrid.FastGrid grid;
        private UI.Util.Control.StandByControl standByControl;
    }
}