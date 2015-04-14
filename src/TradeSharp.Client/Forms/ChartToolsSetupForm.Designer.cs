namespace TradeSharp.Client.Forms
{
    partial class ChartToolsSetupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartToolsSetupForm));
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageButtons = new System.Windows.Forms.TabPage();
            this.chartButtonSettingsPanel = new TradeSharp.Client.Controls.ToolButtonSettingsPanel();
            this.tabPageSystemButtons = new System.Windows.Forms.TabPage();
            this.systemButtonSettingsPanel = new TradeSharp.Client.Controls.ToolButtonSettingsPanel();
            this.tabPageSeriesSettings = new System.Windows.Forms.TabPage();
            this.splitContainerSeries = new System.Windows.Forms.SplitContainer();
            this.treeSeries = new System.Windows.Forms.TreeView();
            this.panelSeriesParams = new System.Windows.Forms.Panel();
            this.panelSeriesTop = new System.Windows.Forms.Panel();
            this.btnUpdateSeries = new System.Windows.Forms.Button();
            this.tabPageScripts = new System.Windows.Forms.TabPage();
            this.gridScripts = new FastGrid.FastGrid();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnUseDefault = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControlMain.SuspendLayout();
            this.tabPageButtons.SuspendLayout();
            this.tabPageSystemButtons.SuspendLayout();
            this.tabPageSeriesSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSeries)).BeginInit();
            this.splitContainerSeries.Panel1.SuspendLayout();
            this.splitContainerSeries.Panel2.SuspendLayout();
            this.splitContainerSeries.SuspendLayout();
            this.panelSeriesTop.SuspendLayout();
            this.tabPageScripts.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageButtons);
            this.tabControlMain.Controls.Add(this.tabPageSystemButtons);
            this.tabControlMain.Controls.Add(this.tabPageSeriesSettings);
            this.tabControlMain.Controls.Add(this.tabPageScripts);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(622, 426);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageButtons
            // 
            this.tabPageButtons.Controls.Add(this.chartButtonSettingsPanel);
            this.tabPageButtons.Location = new System.Drawing.Point(4, 22);
            this.tabPageButtons.Name = "tabPageButtons";
            this.tabPageButtons.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageButtons.Size = new System.Drawing.Size(614, 400);
            this.tabPageButtons.TabIndex = 0;
            this.tabPageButtons.Tag = "TitleButtons";
            this.tabPageButtons.Text = "Кнопки";
            this.tabPageButtons.UseVisualStyleBackColor = true;
            // 
            // chartButtonSettingsPanel
            // 
            this.chartButtonSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartButtonSettingsPanel.Location = new System.Drawing.Point(3, 3);
            this.chartButtonSettingsPanel.Name = "chartButtonSettingsPanel";
            this.chartButtonSettingsPanel.Size = new System.Drawing.Size(608, 394);
            this.chartButtonSettingsPanel.TabIndex = 0;
            // 
            // tabPageSystemButtons
            // 
            this.tabPageSystemButtons.Controls.Add(this.systemButtonSettingsPanel);
            this.tabPageSystemButtons.Location = new System.Drawing.Point(4, 22);
            this.tabPageSystemButtons.Name = "tabPageSystemButtons";
            this.tabPageSystemButtons.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSystemButtons.Size = new System.Drawing.Size(614, 400);
            this.tabPageSystemButtons.TabIndex = 2;
            this.tabPageSystemButtons.Tag = "TitleSystemButtons";
            this.tabPageSystemButtons.Text = "Системные кнопки";
            this.tabPageSystemButtons.UseVisualStyleBackColor = true;
            // 
            // systemButtonSettingsPanel
            // 
            this.systemButtonSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemButtonSettingsPanel.Location = new System.Drawing.Point(3, 3);
            this.systemButtonSettingsPanel.Name = "systemButtonSettingsPanel";
            this.systemButtonSettingsPanel.Size = new System.Drawing.Size(608, 394);
            this.systemButtonSettingsPanel.TabIndex = 0;
            // 
            // tabPageSeriesSettings
            // 
            this.tabPageSeriesSettings.Controls.Add(this.splitContainerSeries);
            this.tabPageSeriesSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageSeriesSettings.Name = "tabPageSeriesSettings";
            this.tabPageSeriesSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSeriesSettings.Size = new System.Drawing.Size(614, 400);
            this.tabPageSeriesSettings.TabIndex = 1;
            this.tabPageSeriesSettings.Tag = "TitleGraphicalObjectsShort";
            this.tabPageSeriesSettings.Text = "Граф. объекты";
            this.tabPageSeriesSettings.UseVisualStyleBackColor = true;
            // 
            // splitContainerSeries
            // 
            this.splitContainerSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerSeries.Location = new System.Drawing.Point(3, 3);
            this.splitContainerSeries.Name = "splitContainerSeries";
            // 
            // splitContainerSeries.Panel1
            // 
            this.splitContainerSeries.Panel1.Controls.Add(this.treeSeries);
            // 
            // splitContainerSeries.Panel2
            // 
            this.splitContainerSeries.Panel2.Controls.Add(this.panelSeriesParams);
            this.splitContainerSeries.Panel2.Controls.Add(this.panelSeriesTop);
            this.splitContainerSeries.Size = new System.Drawing.Size(608, 394);
            this.splitContainerSeries.SplitterDistance = 202;
            this.splitContainerSeries.TabIndex = 0;
            // 
            // treeSeries
            // 
            this.treeSeries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeSeries.Location = new System.Drawing.Point(0, 0);
            this.treeSeries.Name = "treeSeries";
            this.treeSeries.Size = new System.Drawing.Size(202, 394);
            this.treeSeries.TabIndex = 0;
            this.treeSeries.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeSeriesAfterSelect);
            // 
            // panelSeriesParams
            // 
            this.panelSeriesParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSeriesParams.Location = new System.Drawing.Point(0, 28);
            this.panelSeriesParams.Name = "panelSeriesParams";
            this.panelSeriesParams.Size = new System.Drawing.Size(402, 366);
            this.panelSeriesParams.TabIndex = 1;
            // 
            // panelSeriesTop
            // 
            this.panelSeriesTop.Controls.Add(this.btnUpdateSeries);
            this.panelSeriesTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSeriesTop.Location = new System.Drawing.Point(0, 0);
            this.panelSeriesTop.Name = "panelSeriesTop";
            this.panelSeriesTop.Size = new System.Drawing.Size(402, 28);
            this.panelSeriesTop.TabIndex = 0;
            // 
            // btnUpdateSeries
            // 
            this.btnUpdateSeries.Location = new System.Drawing.Point(3, 3);
            this.btnUpdateSeries.Name = "btnUpdateSeries";
            this.btnUpdateSeries.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateSeries.TabIndex = 0;
            this.btnUpdateSeries.Tag = "TitleAccept";
            this.btnUpdateSeries.Text = "Принять";
            this.btnUpdateSeries.UseVisualStyleBackColor = true;
            this.btnUpdateSeries.Click += new System.EventHandler(this.BtnUpdateSeriesClick);
            // 
            // tabPageScripts
            // 
            this.tabPageScripts.Controls.Add(this.gridScripts);
            this.tabPageScripts.Location = new System.Drawing.Point(4, 22);
            this.tabPageScripts.Name = "tabPageScripts";
            this.tabPageScripts.Size = new System.Drawing.Size(614, 400);
            this.tabPageScripts.TabIndex = 3;
            this.tabPageScripts.Tag = "TitleScripts";
            this.tabPageScripts.Text = "Скрипты";
            this.tabPageScripts.UseVisualStyleBackColor = true;
            // 
            // gridScripts
            // 
            this.gridScripts.CaptionHeight = 20;
            this.gridScripts.CellEditMode = FastGrid.FastGrid.CellEditModeTrigger.LeftClick;
            this.gridScripts.CellHeight = 18;
            this.gridScripts.CellPadding = 5;
            this.gridScripts.ColorAltCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gridScripts.ColorAnchorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.gridScripts.ColorCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.gridScripts.ColorCellFont = System.Drawing.Color.Black;
            this.gridScripts.ColorCellOutlineLower = System.Drawing.Color.White;
            this.gridScripts.ColorCellOutlineUpper = System.Drawing.Color.DarkGray;
            this.gridScripts.ColorSelectedCellBackground = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(247)))), ((int)(((byte)(227)))));
            this.gridScripts.ColorSelectedCellFont = System.Drawing.Color.Black;
            this.gridScripts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridScripts.FitWidth = false;
            this.gridScripts.FontAnchoredRow = null;
            this.gridScripts.FontCell = null;
            this.gridScripts.FontHeader = null;
            this.gridScripts.FontSelectedCell = null;
            this.gridScripts.Location = new System.Drawing.Point(0, 0);
            this.gridScripts.MinimumTableWidth = null;
            this.gridScripts.MultiSelectEnabled = false;
            this.gridScripts.Name = "gridScripts";
            this.gridScripts.SelectEnabled = true;
            this.gridScripts.Size = new System.Drawing.Size(614, 400);
            this.gridScripts.StickFirst = false;
            this.gridScripts.StickLast = false;
            this.gridScripts.TabIndex = 0;
            this.gridScripts.UserHitCell += new FastGrid.UserHitCellDel(this.GridScriptsUserHitCell);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnUseDefault, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnAccept, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 426);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(622, 29);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // btnUseDefault
            // 
            this.btnUseDefault.Location = new System.Drawing.Point(3, 3);
            this.btnUseDefault.Name = "btnUseDefault";
            this.btnUseDefault.Size = new System.Drawing.Size(97, 23);
            this.btnUseDefault.TabIndex = 2;
            this.btnUseDefault.Tag = "TitleDefault";
            this.btnUseDefault.Text = "По умолчанию";
            this.btnUseDefault.UseVisualStyleBackColor = true;
            this.btnUseDefault.Click += new System.EventHandler(this.BtnUseDefaultClick);
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(463, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Text = "OK";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(544, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ChartToolsSetupForm
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(622, 455);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChartToolsSetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleToolbarSettings";
            this.Text = "Настройка инструментальной панели";
            this.Load += new System.EventHandler(this.ChartToolsSetupFormLoad);
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.ChartToolsSetupFormHelpRequested);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageButtons.ResumeLayout(false);
            this.tabPageSystemButtons.ResumeLayout(false);
            this.tabPageSeriesSettings.ResumeLayout(false);
            this.splitContainerSeries.Panel1.ResumeLayout(false);
            this.splitContainerSeries.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSeries)).EndInit();
            this.splitContainerSeries.ResumeLayout(false);
            this.panelSeriesTop.ResumeLayout(false);
            this.tabPageScripts.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageButtons;
        private System.Windows.Forms.TabPage tabPageSeriesSettings;
        private System.Windows.Forms.SplitContainer splitContainerSeries;
        private System.Windows.Forms.TreeView treeSeries;
        private System.Windows.Forms.Panel panelSeriesTop;
        private System.Windows.Forms.Button btnUpdateSeries;
        private System.Windows.Forms.Panel panelSeriesParams;
        private System.Windows.Forms.TabPage tabPageSystemButtons;
        private System.Windows.Forms.TabPage tabPageScripts;
        private FastGrid.FastGrid gridScripts;
        private Controls.ToolButtonSettingsPanel chartButtonSettingsPanel;
        private Controls.ToolButtonSettingsPanel systemButtonSettingsPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnUseDefault;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
    }
}