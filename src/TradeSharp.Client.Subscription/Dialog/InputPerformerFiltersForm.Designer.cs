namespace TradeSharp.Client.Subscription.Dialog
{
    partial class InputPerformerFiltersForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputPerformerFiltersForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.countNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.fioCheckBox = new System.Windows.Forms.CheckBox();
            this.emailCheckBox = new System.Windows.Forms.CheckBox();
            this.accountCheckBox = new System.Windows.Forms.CheckBox();
            this.fioTextBox = new System.Windows.Forms.TextBox();
            this.emailTextBox = new System.Windows.Forms.TextBox();
            this.accountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.fioCSCheckBox = new System.Windows.Forms.CheckBox();
            this.emailCSCheckBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countNumericUpDown)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accountNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(7, 117);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(418, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(340, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Tag = "TitleCancel";
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(259, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Tag = "TitleOK";
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.label1);
            this.flowLayoutPanel2.Controls.Add(this.countNumericUpDown);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(7, 91);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(418, 26);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "TitleShowFirst";
            this.label1.Text = "Показать первые";
            // 
            // countNumericUpDown
            // 
            this.countNumericUpDown.Location = new System.Drawing.Point(106, 3);
            this.countNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.countNumericUpDown.Name = "countNumericUpDown";
            this.countNumericUpDown.Size = new System.Drawing.Size(69, 20);
            this.countNumericUpDown.TabIndex = 0;
            this.countNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.fioCheckBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.emailCheckBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.accountCheckBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.fioTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.emailTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.accountNumericUpDown, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.fioCSCheckBox, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.emailCSCheckBox, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(418, 78);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // fioCheckBox
            // 
            this.fioCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.fioCheckBox.AutoSize = true;
            this.fioCheckBox.Location = new System.Drawing.Point(3, 4);
            this.fioCheckBox.Name = "fioCheckBox";
            this.fioCheckBox.Size = new System.Drawing.Size(62, 17);
            this.fioCheckBox.TabIndex = 0;
            this.fioCheckBox.Tag = "TitleInitials";
            this.fioCheckBox.Text = "Ф.И.О.";
            this.fioCheckBox.UseVisualStyleBackColor = true;
            this.fioCheckBox.CheckedChanged += new System.EventHandler(this.FioCheckBoxCheckedChanged);
            // 
            // emailCheckBox
            // 
            this.emailCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.emailCheckBox.AutoSize = true;
            this.emailCheckBox.Location = new System.Drawing.Point(3, 30);
            this.emailCheckBox.Name = "emailCheckBox";
            this.emailCheckBox.Size = new System.Drawing.Size(73, 17);
            this.emailCheckBox.TabIndex = 3;
            this.emailCheckBox.Tag = "TitleEmail";
            this.emailCheckBox.Text = "Эл. почта";
            this.emailCheckBox.UseVisualStyleBackColor = true;
            this.emailCheckBox.CheckedChanged += new System.EventHandler(this.EmailCheckBoxCheckedChanged);
            // 
            // accountCheckBox
            // 
            this.accountCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.accountCheckBox.AutoSize = true;
            this.accountCheckBox.Location = new System.Drawing.Point(3, 56);
            this.accountCheckBox.Name = "accountCheckBox";
            this.accountCheckBox.Size = new System.Drawing.Size(65, 17);
            this.accountCheckBox.TabIndex = 6;
            this.accountCheckBox.Tag = "TitleAccountNumber";
            this.accountCheckBox.Text = "N счета";
            this.accountCheckBox.UseVisualStyleBackColor = true;
            this.accountCheckBox.CheckedChanged += new System.EventHandler(this.AccountCheckBoxCheckedChanged);
            // 
            // fioTextBox
            // 
            this.fioTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fioTextBox.Location = new System.Drawing.Point(82, 3);
            this.fioTextBox.Name = "fioTextBox";
            this.fioTextBox.Size = new System.Drawing.Size(206, 20);
            this.fioTextBox.TabIndex = 1;
            this.fioTextBox.TextChanged += new System.EventHandler(this.CheckByDefault);
            // 
            // emailTextBox
            // 
            this.emailTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.emailTextBox.Location = new System.Drawing.Point(82, 29);
            this.emailTextBox.Name = "emailTextBox";
            this.emailTextBox.Size = new System.Drawing.Size(206, 20);
            this.emailTextBox.TabIndex = 4;
            this.emailTextBox.TextChanged += new System.EventHandler(this.CheckByDefault);
            // 
            // accountNumericUpDown
            // 
            this.accountNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.accountNumericUpDown.Location = new System.Drawing.Point(82, 55);
            this.accountNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.accountNumericUpDown.Name = "accountNumericUpDown";
            this.accountNumericUpDown.Size = new System.Drawing.Size(206, 20);
            this.accountNumericUpDown.TabIndex = 7;
            this.accountNumericUpDown.ValueChanged += new System.EventHandler(this.CheckByDefault);
            // 
            // fioCSCheckBox
            // 
            this.fioCSCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.fioCSCheckBox.AutoSize = true;
            this.fioCSCheckBox.Location = new System.Drawing.Point(294, 4);
            this.fioCSCheckBox.Name = "fioCSCheckBox";
            this.fioCSCheckBox.Size = new System.Drawing.Size(121, 17);
            this.fioCSCheckBox.TabIndex = 2;
            this.fioCSCheckBox.Tag = "TitleCaseSensitiveSmall";
            this.fioCSCheckBox.Text = "учитывать регистр";
            this.fioCSCheckBox.UseVisualStyleBackColor = true;
            // 
            // emailCSCheckBox
            // 
            this.emailCSCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.emailCSCheckBox.AutoSize = true;
            this.emailCSCheckBox.Location = new System.Drawing.Point(294, 30);
            this.emailCSCheckBox.Name = "emailCSCheckBox";
            this.emailCSCheckBox.Size = new System.Drawing.Size(121, 17);
            this.emailCSCheckBox.TabIndex = 5;
            this.emailCSCheckBox.Tag = "TitleCaseSensitiveSmall";
            this.emailCSCheckBox.Text = "учитывать регистр";
            this.emailCSCheckBox.UseVisualStyleBackColor = true;
            // 
            // InputPerformerFiltersForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(432, 153);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 180);
            this.Name = "InputPerformerFiltersForm";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleSearchForManager";
            this.Text = "Поиск управляющего";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countNumericUpDown)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accountNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown countNumericUpDown;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox fioCheckBox;
        private System.Windows.Forms.CheckBox emailCheckBox;
        private System.Windows.Forms.CheckBox accountCheckBox;
        private System.Windows.Forms.TextBox fioTextBox;
        private System.Windows.Forms.TextBox emailTextBox;
        private System.Windows.Forms.NumericUpDown accountNumericUpDown;
        private System.Windows.Forms.CheckBox fioCSCheckBox;
        private System.Windows.Forms.CheckBox emailCSCheckBox;
    }
}