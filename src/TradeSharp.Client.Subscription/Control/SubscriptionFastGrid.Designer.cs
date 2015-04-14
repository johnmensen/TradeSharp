namespace TradeSharp.Client.Subscription.Control
{
    partial class SubscriptionFastGrid
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubscriptionFastGrid));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.imageListGlyph = new System.Windows.Forms.ImageList(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemUnsubscribe = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemTradeSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.cbActionOnSignal = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.summaryFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.summaryLabel = new System.Windows.Forms.Label();
            this.unsubscribeFromAllButton = new System.Windows.Forms.Button();
            this.topSubscriptionControl = new TradeSharp.Client.Subscription.Control.TopSubscriptionControl();
            this.grid = new FastGrid.FastGrid();
            this.contextMenu.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.summaryFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(84, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отменить";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnAccept
            // 
            this.btnAccept.Enabled = false;
            this.btnAccept.Location = new System.Drawing.Point(3, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Tag = "TitleAccept";
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // imageListGlyph
            // 
            this.imageListGlyph.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlyph.ImageStream")));
            this.imageListGlyph.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlyph.Images.SetKeyName(0, "True");
            this.imageListGlyph.Images.SetKeyName(1, "False");
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemUnsubscribe,
            this.menuitemTradeSettings,
            this.menuitemStatistics});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(191, 70);
            // 
            // menuItemUnsubscribe
            // 
            this.menuItemUnsubscribe.Name = "menuItemUnsubscribe";
            this.menuItemUnsubscribe.Size = new System.Drawing.Size(190, 22);
            this.menuItemUnsubscribe.Text = "Отписаться";
            this.menuItemUnsubscribe.Click += new System.EventHandler(this.MenuItemUnsubscribeClick);
            // 
            // menuitemTradeSettings
            // 
            this.menuitemTradeSettings.Name = "menuitemTradeSettings";
            this.menuitemTradeSettings.Size = new System.Drawing.Size(190, 22);
            this.menuitemTradeSettings.Text = "Настройка торговли...";
            this.menuitemTradeSettings.Click += new System.EventHandler(this.MenuitemTradeSettingsClick);
            // 
            // menuitemStatistics
            // 
            this.menuitemStatistics.Name = "menuitemStatistics";
            this.menuitemStatistics.Size = new System.Drawing.Size(190, 22);
            this.menuitemStatistics.Text = "Статистика...";
            this.menuitemStatistics.Click += new System.EventHandler(this.MenuitemStatisticsClick);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 1;
            this.label1.Tag = "TitleActionOnForecastReceive";
            this.label1.Text = "Получив прогноз";
            // 
            // cbActionOnSignal
            // 
            this.cbActionOnSignal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbActionOnSignal.FormattingEnabled = true;
            this.cbActionOnSignal.Location = new System.Drawing.Point(102, 3);
            this.cbActionOnSignal.Name = "cbActionOnSignal";
            this.cbActionOnSignal.Size = new System.Drawing.Size(218, 21);
            this.cbActionOnSignal.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Controls.Add(this.btnAccept);
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnRefresh);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 340);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(504, 31);
            this.flowLayoutPanel1.TabIndex = 9;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(165, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Tag = "TitleRefresh";
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefreshClick);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel2.Controls.Add(this.label1);
            this.flowLayoutPanel2.Controls.Add(this.cbActionOnSignal);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(504, 29);
            this.flowLayoutPanel2.TabIndex = 11;
            // 
            // summaryFlowLayoutPanel
            // 
            this.summaryFlowLayoutPanel.AutoSize = true;
            this.summaryFlowLayoutPanel.Controls.Add(this.summaryLabel);
            this.summaryFlowLayoutPanel.Controls.Add(this.unsubscribeFromAllButton);
            this.summaryFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.summaryFlowLayoutPanel.Location = new System.Drawing.Point(0, 29);
            this.summaryFlowLayoutPanel.Name = "summaryFlowLayoutPanel";
            this.summaryFlowLayoutPanel.Size = new System.Drawing.Size(504, 29);
            this.summaryFlowLayoutPanel.TabIndex = 12;
            this.summaryFlowLayoutPanel.WrapContents = false;
            // 
            // summaryLabel
            // 
            this.summaryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.summaryLabel.AutoSize = true;
            this.summaryLabel.Location = new System.Drawing.Point(3, 8);
            this.summaryLabel.Name = "summaryLabel";
            this.summaryLabel.Size = new System.Drawing.Size(181, 13);
            this.summaryLabel.TabIndex = 0;
            this.summaryLabel.Text = "(краткая информация о подписке)";
            // 
            // unsubscribeFromAllButton
            // 
            this.unsubscribeFromAllButton.AutoSize = true;
            this.unsubscribeFromAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.unsubscribeFromAllButton.Location = new System.Drawing.Point(190, 3);
            this.unsubscribeFromAllButton.Name = "unsubscribeFromAllButton";
            this.unsubscribeFromAllButton.Size = new System.Drawing.Size(117, 23);
            this.unsubscribeFromAllButton.TabIndex = 1;
            this.unsubscribeFromAllButton.Tag = "TitleUnsubscribeFromAll";
            this.unsubscribeFromAllButton.Text = "Отписаться от всех";
            this.unsubscribeFromAllButton.UseVisualStyleBackColor = true;
            // 
            // topSubscriptionControl
            // 
            this.topSubscriptionControl.AutoSize = true;
            this.topSubscriptionControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topSubscriptionControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.topSubscriptionControl.Location = new System.Drawing.Point(0, 58);
            this.topSubscriptionControl.Name = "topSubscriptionControl";
            this.topSubscriptionControl.Portfolio = null;
            this.topSubscriptionControl.Size = new System.Drawing.Size(504, 31);
            this.topSubscriptionControl.TabIndex = 14;
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
            this.grid.Location = new System.Drawing.Point(0, 89);
            this.grid.MinimumTableWidth = null;
            this.grid.MultiSelectEnabled = false;
            this.grid.Name = "grid";
            this.grid.SelectEnabled = true;
            this.grid.Size = new System.Drawing.Size(504, 251);
            this.grid.StickFirst = false;
            this.grid.StickLast = false;
            this.grid.TabIndex = 15;
            // 
            // SubscriptionFastGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grid);
            this.Controls.Add(this.topSubscriptionControl);
            this.Controls.Add(this.summaryFlowLayoutPanel);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "SubscriptionFastGrid";
            this.Size = new System.Drawing.Size(504, 371);
            this.contextMenu.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.summaryFlowLayoutPanel.ResumeLayout(false);
            this.summaryFlowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.ImageList imageListGlyph;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemUnsubscribe;
        private System.Windows.Forms.ToolStripMenuItem menuitemTradeSettings;
        private System.Windows.Forms.ToolStripMenuItem menuitemStatistics;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbActionOnSignal;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel summaryFlowLayoutPanel;
        private System.Windows.Forms.Label summaryLabel;
        private System.Windows.Forms.Button unsubscribeFromAllButton;
        private TopSubscriptionControl topSubscriptionControl;
        private FastGrid.FastGrid grid;
    }
}
