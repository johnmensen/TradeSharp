namespace TradeSharp.Client.Subscription.Dialog
{
    partial class PerformerCriteriaFunctionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PerformerCriteriaFunctionForm));
            this.btnCheckFormula = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnAccept = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelOrder = new System.Windows.Forms.Panel();
            this.tbMarginValue = new System.Windows.Forms.TextBox();
            this.cbMargin = new System.Windows.Forms.CheckBox();
            this.cbSortOrder = new System.Windows.Forms.ComboBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tbComment = new System.Windows.Forms.RichTextBox();
            this.tbVariables = new System.Windows.Forms.RichTextBox();
            this.btnDeleteCriteria = new System.Windows.Forms.Button();
            this.cbFunction = new System.Windows.Forms.ComboBox();
            this.panelTop.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCheckFormula
            // 
            this.btnCheckFormula.Location = new System.Drawing.Point(3, 3);
            this.btnCheckFormula.Name = "btnCheckFormula";
            this.btnCheckFormula.Size = new System.Drawing.Size(75, 23);
            this.btnCheckFormula.TabIndex = 2;
            this.btnCheckFormula.Text = "Проверить";
            this.btnCheckFormula.UseVisualStyleBackColor = true;
            this.btnCheckFormula.Click += new System.EventHandler(this.BtnCheckFormulaClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(385, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(304, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "ОК";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.cbFunction);
            this.panelTop.Controls.Add(this.btnDeleteCriteria);
            this.panelTop.Controls.Add(this.btnAccept);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(463, 23);
            this.panelTop.TabIndex = 1;
            // 
            // btnAccept
            // 
            this.btnAccept.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnAccept.Image = ((System.Drawing.Image)(resources.GetObject("btnAccept.Image")));
            this.btnAccept.Location = new System.Drawing.Point(440, 0);
            this.btnAccept.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(23, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnCheckFormula, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnOk, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 397);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(463, 29);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panelOrder
            // 
            this.panelOrder.Controls.Add(this.tbMarginValue);
            this.panelOrder.Controls.Add(this.cbMargin);
            this.panelOrder.Controls.Add(this.cbSortOrder);
            this.panelOrder.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelOrder.Location = new System.Drawing.Point(0, 23);
            this.panelOrder.Name = "panelOrder";
            this.panelOrder.Size = new System.Drawing.Size(463, 28);
            this.panelOrder.TabIndex = 4;
            // 
            // tbMarginValue
            // 
            this.tbMarginValue.Location = new System.Drawing.Point(264, 5);
            this.tbMarginValue.Name = "tbMarginValue";
            this.tbMarginValue.Size = new System.Drawing.Size(70, 20);
            this.tbMarginValue.TabIndex = 2;
            this.tbMarginValue.Text = "0.000";
            // 
            // cbMargin
            // 
            this.cbMargin.AutoSize = true;
            this.cbMargin.Checked = true;
            this.cbMargin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMargin.Location = new System.Drawing.Point(130, 7);
            this.cbMargin.Name = "cbMargin";
            this.cbMargin.Size = new System.Drawing.Size(128, 17);
            this.cbMargin.TabIndex = 1;
            this.cbMargin.Text = "граничное значение";
            this.cbMargin.UseVisualStyleBackColor = true;
            this.cbMargin.CheckedChanged += new System.EventHandler(this.CbMarginCheckedChanged);
            // 
            // cbSortOrder
            // 
            this.cbSortOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSortOrder.FormattingEnabled = true;
            this.cbSortOrder.Items.AddRange(new object[] {
            "По убыванию",
            "По возрастанию"});
            this.cbSortOrder.Location = new System.Drawing.Point(3, 3);
            this.cbSortOrder.Name = "cbSortOrder";
            this.cbSortOrder.Size = new System.Drawing.Size(121, 21);
            this.cbSortOrder.TabIndex = 0;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 51);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tbComment);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tbVariables);
            this.splitContainer.Size = new System.Drawing.Size(463, 346);
            this.splitContainer.SplitterDistance = 152;
            this.splitContainer.TabIndex = 5;
            // 
            // tbComment
            // 
            this.tbComment.BackColor = System.Drawing.SystemColors.Window;
            this.tbComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbComment.Location = new System.Drawing.Point(0, 0);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(463, 152);
            this.tbComment.TabIndex = 0;
            this.tbComment.Text = "";
            // 
            // tbVariables
            // 
            this.tbVariables.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tbVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbVariables.Location = new System.Drawing.Point(0, 0);
            this.tbVariables.Name = "tbVariables";
            this.tbVariables.Size = new System.Drawing.Size(463, 190);
            this.tbVariables.TabIndex = 1;
            this.tbVariables.Text = "";
            // 
            // btnDeleteCriteria
            // 
            this.btnDeleteCriteria.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnDeleteCriteria.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteCriteria.Image")));
            this.btnDeleteCriteria.Location = new System.Drawing.Point(417, 0);
            this.btnDeleteCriteria.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.btnDeleteCriteria.Name = "btnDeleteCriteria";
            this.btnDeleteCriteria.Size = new System.Drawing.Size(23, 23);
            this.btnDeleteCriteria.TabIndex = 2;
            this.btnDeleteCriteria.UseVisualStyleBackColor = true;
            this.btnDeleteCriteria.Click += new System.EventHandler(this.BtnDeleteCriteriaClick);
            // 
            // cbFunction
            // 
            this.cbFunction.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbFunction.FormattingEnabled = true;
            this.cbFunction.Location = new System.Drawing.Point(0, 0);
            this.cbFunction.Name = "cbFunction";
            this.cbFunction.Size = new System.Drawing.Size(417, 21);
            this.cbFunction.TabIndex = 3;
            this.cbFunction.SelectedIndexChanged += new System.EventHandler(this.CbFunctionSelectedIndexChanged);
            // 
            // PerformerCriteriaFunctionForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(463, 426);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.panelOrder);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PerformerCriteriaFunctionForm";
            this.Text = "Формула оценки торговли";
            this.Load += new System.EventHandler(this.PerformerCriteriaFunctionFormLoad);
            this.panelTop.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panelOrder.ResumeLayout(false);
            this.panelOrder.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCheckFormula;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelOrder;
        private System.Windows.Forms.TextBox tbMarginValue;
        private System.Windows.Forms.CheckBox cbMargin;
        private System.Windows.Forms.ComboBox cbSortOrder;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.RichTextBox tbComment;
        private System.Windows.Forms.RichTextBox tbVariables;
        private System.Windows.Forms.ComboBox cbFunction;
        private System.Windows.Forms.Button btnDeleteCriteria;
    }
}