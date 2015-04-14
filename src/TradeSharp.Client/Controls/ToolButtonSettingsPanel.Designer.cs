namespace TradeSharp.Client.Controls
{
    partial class ToolButtonSettingsPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.treeButtons = new System.Windows.Forms.TreeView();
            this.menuTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemAddButton = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAddGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRemoveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.panelParams = new System.Windows.Forms.Panel();
            this.isVisibleDisplayNameCheckBox = new System.Windows.Forms.CheckBox();
            this.panelToolTop = new System.Windows.Forms.TableLayoutPanel();
            this.btnUpdateToolButton = new System.Windows.Forms.Button();
            this.tbButtonDisplayName = new System.Windows.Forms.TextBox();
            this.btnPicture = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.menuTree.SuspendLayout();
            this.panelToolTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeButtons);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panelParams);
            this.splitContainer.Panel2.Controls.Add(this.isVisibleDisplayNameCheckBox);
            this.splitContainer.Panel2.Controls.Add(this.panelToolTop);
            this.splitContainer.Panel2.Controls.Add(this.label1);
            this.splitContainer.Size = new System.Drawing.Size(482, 325);
            this.splitContainer.SplitterDistance = 210;
            this.splitContainer.TabIndex = 3;
            // 
            // treeButtons
            // 
            this.treeButtons.AllowDrop = true;
            this.treeButtons.ContextMenuStrip = this.menuTree;
            this.treeButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeButtons.Location = new System.Drawing.Point(0, 0);
            this.treeButtons.Name = "treeButtons";
            this.treeButtons.Size = new System.Drawing.Size(210, 325);
            this.treeButtons.TabIndex = 1;
            this.treeButtons.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.TreeButtonsItemDrag);
            this.treeButtons.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeButtonsAfterSelect);
            this.treeButtons.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeButtonsDragDrop);
            this.treeButtons.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeButtonsDragEnter);
            this.treeButtons.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeButtonsDragOver);
            this.treeButtons.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.TreeButtonsGiveFeedback);
            // 
            // menuTree
            // 
            this.menuTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAddButton,
            this.menuItemAddGroup,
            this.menuItemRemoveButton});
            this.menuTree.Name = "menuTree";
            this.menuTree.Size = new System.Drawing.Size(176, 70);
            // 
            // menuItemAddButton
            // 
            this.menuItemAddButton.Name = "menuItemAddButton";
            this.menuItemAddButton.Size = new System.Drawing.Size(175, 22);
            this.menuItemAddButton.Text = "Добавить кнопку...";
            this.menuItemAddButton.Click += new System.EventHandler(this.MenuItemAddButtonClick);
            // 
            // menuItemAddGroup
            // 
            this.menuItemAddGroup.Name = "menuItemAddGroup";
            this.menuItemAddGroup.Size = new System.Drawing.Size(175, 22);
            this.menuItemAddGroup.Text = "Добавить группу...";
            this.menuItemAddGroup.Click += new System.EventHandler(this.MenuItemAddGroupClick);
            // 
            // menuItemRemoveButton
            // 
            this.menuItemRemoveButton.Name = "menuItemRemoveButton";
            this.menuItemRemoveButton.Size = new System.Drawing.Size(175, 22);
            this.menuItemRemoveButton.Text = "Удалить";
            this.menuItemRemoveButton.Click += new System.EventHandler(this.MenuItemRemoveButtonClick);
            // 
            // panelParams
            // 
            this.panelParams.AutoScroll = true;
            this.panelParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelParams.Location = new System.Drawing.Point(0, 60);
            this.panelParams.Name = "panelParams";
            this.panelParams.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.panelParams.Size = new System.Drawing.Size(268, 265);
            this.panelParams.TabIndex = 10;
            // 
            // isVisibleDisplayNameCheckBox
            // 
            this.isVisibleDisplayNameCheckBox.AutoSize = true;
            this.isVisibleDisplayNameCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.isVisibleDisplayNameCheckBox.Location = new System.Drawing.Point(0, 43);
            this.isVisibleDisplayNameCheckBox.Name = "isVisibleDisplayNameCheckBox";
            this.isVisibleDisplayNameCheckBox.Size = new System.Drawing.Size(268, 17);
            this.isVisibleDisplayNameCheckBox.TabIndex = 9;
            this.isVisibleDisplayNameCheckBox.Tag = "TitleShowTextOnButton";
            this.isVisibleDisplayNameCheckBox.Text = "Отображать текст на кнопке";
            this.isVisibleDisplayNameCheckBox.UseVisualStyleBackColor = true;
            this.isVisibleDisplayNameCheckBox.CheckedChanged += new System.EventHandler(this.IsVisibleDisplayNameCheckBoxCheckedChanged);
            // 
            // panelToolTop
            // 
            this.panelToolTop.ColumnCount = 3;
            this.panelToolTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelToolTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelToolTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelToolTop.Controls.Add(this.btnUpdateToolButton, 2, 0);
            this.panelToolTop.Controls.Add(this.tbButtonDisplayName, 1, 0);
            this.panelToolTop.Controls.Add(this.btnPicture, 0, 0);
            this.panelToolTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolTop.Location = new System.Drawing.Point(0, 13);
            this.panelToolTop.Name = "panelToolTop";
            this.panelToolTop.RowCount = 1;
            this.panelToolTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelToolTop.Size = new System.Drawing.Size(268, 30);
            this.panelToolTop.TabIndex = 7;
            // 
            // btnUpdateToolButton
            // 
            this.btnUpdateToolButton.Location = new System.Drawing.Point(190, 3);
            this.btnUpdateToolButton.Name = "btnUpdateToolButton";
            this.btnUpdateToolButton.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateToolButton.TabIndex = 7;
            this.btnUpdateToolButton.Tag = "TitleAccept";
            this.btnUpdateToolButton.Text = "Принять";
            this.btnUpdateToolButton.UseVisualStyleBackColor = true;
            this.btnUpdateToolButton.Click += new System.EventHandler(this.BtnUpdateToolButtonClick);
            // 
            // tbButtonDisplayName
            // 
            this.tbButtonDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbButtonDisplayName.Location = new System.Drawing.Point(32, 3);
            this.tbButtonDisplayName.Name = "tbButtonDisplayName";
            this.tbButtonDisplayName.Size = new System.Drawing.Size(152, 20);
            this.tbButtonDisplayName.TabIndex = 8;
            // 
            // btnPicture
            // 
            this.btnPicture.Location = new System.Drawing.Point(3, 3);
            this.btnPicture.Name = "btnPicture";
            this.btnPicture.Size = new System.Drawing.Size(23, 23);
            this.btnPicture.TabIndex = 6;
            this.btnPicture.UseVisualStyleBackColor = true;
            this.btnPicture.Click += new System.EventHandler(this.BtnPictureClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 4;
            this.label1.Tag = "TitleInstrument";
            this.label1.Text = "Инструмент";
            // 
            // ToolButtonSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "ToolButtonSettingsPanel";
            this.Size = new System.Drawing.Size(482, 325);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.menuTree.ResumeLayout(false);
            this.panelToolTop.ResumeLayout(false);
            this.panelToolTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView treeButtons;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel panelToolTop;
        private System.Windows.Forms.Button btnUpdateToolButton;
        private System.Windows.Forms.TextBox tbButtonDisplayName;
        private System.Windows.Forms.Button btnPicture;
        private System.Windows.Forms.ContextMenuStrip menuTree;
        private System.Windows.Forms.ToolStripMenuItem menuItemAddButton;
        private System.Windows.Forms.ToolStripMenuItem menuItemAddGroup;
        private System.Windows.Forms.ToolStripMenuItem menuItemRemoveButton;
        private System.Windows.Forms.CheckBox isVisibleDisplayNameCheckBox;
        private System.Windows.Forms.Panel panelParams;
    }
}
