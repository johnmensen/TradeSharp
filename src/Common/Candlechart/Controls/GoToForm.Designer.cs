namespace Candlechart.Controls
{
    partial class GoToForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoToForm));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageCenter = new System.Windows.Forms.TabPage();
            this.lblRangeTime = new System.Windows.Forms.Label();
            this.lblRangeCandle = new System.Windows.Forms.Label();
            this.cbCenterDate = new System.Windows.Forms.CheckBox();
            this.dpCenterTime = new System.Windows.Forms.DateTimePicker();
            this.cbCenterCandle = new System.Windows.Forms.CheckBox();
            this.tbCenterCandle = new System.Windows.Forms.TextBox();
            this.pageMargins = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dpTo = new System.Windows.Forms.DateTimePicker();
            this.dpFrom = new System.Windows.Forms.DateTimePicker();
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.pageCenter.SuspendLayout();
            this.pageMargins.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnAccept);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 169);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(273, 31);
            this.panelBottom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(122, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отменить";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(3, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(85, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Tag = "TitleNavigate";
            this.btnAccept.Text = "Перейти";
            this.btnAccept.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageCenter);
            this.tabControl.Controls.Add(this.pageMargins);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(273, 169);
            this.tabControl.TabIndex = 1;
            // 
            // pageCenter
            // 
            this.pageCenter.Controls.Add(this.lblRangeTime);
            this.pageCenter.Controls.Add(this.lblRangeCandle);
            this.pageCenter.Controls.Add(this.cbCenterDate);
            this.pageCenter.Controls.Add(this.dpCenterTime);
            this.pageCenter.Controls.Add(this.cbCenterCandle);
            this.pageCenter.Controls.Add(this.tbCenterCandle);
            this.pageCenter.Location = new System.Drawing.Point(4, 22);
            this.pageCenter.Name = "pageCenter";
            this.pageCenter.Padding = new System.Windows.Forms.Padding(3);
            this.pageCenter.Size = new System.Drawing.Size(265, 143);
            this.pageCenter.TabIndex = 0;
            this.pageCenter.Tag = "TitleNavigateChartCenter";
            this.pageCenter.Text = "Центр графика";
            this.pageCenter.UseVisualStyleBackColor = true;
            // 
            // lblRangeTime
            // 
            this.lblRangeTime.AutoSize = true;
            this.lblRangeTime.Location = new System.Drawing.Point(8, 81);
            this.lblRangeTime.Name = "lblRangeTime";
            this.lblRangeTime.Size = new System.Drawing.Size(35, 13);
            this.lblRangeTime.TabIndex = 5;
            this.lblRangeTime.Text = "label2";
            // 
            // lblRangeCandle
            // 
            this.lblRangeCandle.AutoSize = true;
            this.lblRangeCandle.Location = new System.Drawing.Point(6, 29);
            this.lblRangeCandle.Name = "lblRangeCandle";
            this.lblRangeCandle.Size = new System.Drawing.Size(35, 13);
            this.lblRangeCandle.TabIndex = 4;
            this.lblRangeCandle.Text = "label1";
            // 
            // cbCenterDate
            // 
            this.cbCenterDate.AutoSize = true;
            this.cbCenterDate.Checked = true;
            this.cbCenterDate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCenterDate.Location = new System.Drawing.Point(140, 61);
            this.cbCenterDate.Name = "cbCenterDate";
            this.cbCenterDate.Size = new System.Drawing.Size(58, 17);
            this.cbCenterDate.TabIndex = 3;
            this.cbCenterDate.Tag = "TitleTimeLower";
            this.cbCenterDate.Text = "время";
            this.cbCenterDate.UseVisualStyleBackColor = true;
            this.cbCenterDate.CheckedChanged += new System.EventHandler(this.CbCenterDateCheckedChanged);
            // 
            // dpCenterTime
            // 
            this.dpCenterTime.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpCenterTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpCenterTime.Location = new System.Drawing.Point(8, 58);
            this.dpCenterTime.Name = "dpCenterTime";
            this.dpCenterTime.Size = new System.Drawing.Size(124, 20);
            this.dpCenterTime.TabIndex = 2;
            // 
            // cbCenterCandle
            // 
            this.cbCenterCandle.AutoSize = true;
            this.cbCenterCandle.Location = new System.Drawing.Point(140, 6);
            this.cbCenterCandle.Name = "cbCenterCandle";
            this.cbCenterCandle.Size = new System.Drawing.Size(55, 17);
            this.cbCenterCandle.TabIndex = 1;
            this.cbCenterCandle.Tag = "TitleCandleLower";
            this.cbCenterCandle.Text = "свеча";
            this.cbCenterCandle.UseVisualStyleBackColor = true;
            this.cbCenterCandle.CheckedChanged += new System.EventHandler(this.CbCenterCandleCheckedChanged);
            // 
            // tbCenterCandle
            // 
            this.tbCenterCandle.Enabled = false;
            this.tbCenterCandle.Location = new System.Drawing.Point(8, 6);
            this.tbCenterCandle.Name = "tbCenterCandle";
            this.tbCenterCandle.Size = new System.Drawing.Size(76, 20);
            this.tbCenterCandle.TabIndex = 0;
            // 
            // pageMargins
            // 
            this.pageMargins.Controls.Add(this.label2);
            this.pageMargins.Controls.Add(this.label1);
            this.pageMargins.Controls.Add(this.dpTo);
            this.pageMargins.Controls.Add(this.dpFrom);
            this.pageMargins.Location = new System.Drawing.Point(4, 22);
            this.pageMargins.Name = "pageMargins";
            this.pageMargins.Padding = new System.Windows.Forms.Padding(3);
            this.pageMargins.Size = new System.Drawing.Size(265, 143);
            this.pageMargins.TabIndex = 1;
            this.pageMargins.Tag = "TitleNavigateChartBounds";
            this.pageMargins.Text = "Границы";
            this.pageMargins.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 13);
            this.label2.TabIndex = 6;
            this.label2.Tag = "TitleFromSmall";
            this.label2.Text = "с";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(19, 13);
            this.label1.TabIndex = 5;
            this.label1.Tag = "TitleToSmall";
            this.label1.Text = "по";
            // 
            // dpTo
            // 
            this.dpTo.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpTo.Location = new System.Drawing.Point(54, 31);
            this.dpTo.Name = "dpTo";
            this.dpTo.Size = new System.Drawing.Size(124, 20);
            this.dpTo.TabIndex = 4;
            // 
            // dpFrom
            // 
            this.dpFrom.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpFrom.Location = new System.Drawing.Point(54, 5);
            this.dpFrom.Name = "dpFrom";
            this.dpFrom.Size = new System.Drawing.Size(124, 20);
            this.dpFrom.TabIndex = 3;
            // 
            // GoToForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 200);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GoToForm";
            this.Tag = "TitleNavigateTo";
            this.Text = "Быстрый переход";
            this.panelBottom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.pageCenter.ResumeLayout(false);
            this.pageCenter.PerformLayout();
            this.pageMargins.ResumeLayout(false);
            this.pageMargins.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pageCenter;
        private System.Windows.Forms.CheckBox cbCenterDate;
        private System.Windows.Forms.DateTimePicker dpCenterTime;
        private System.Windows.Forms.CheckBox cbCenterCandle;
        private System.Windows.Forms.TextBox tbCenterCandle;
        private System.Windows.Forms.TabPage pageMargins;
        private System.Windows.Forms.Label lblRangeTime;
        private System.Windows.Forms.Label lblRangeCandle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dpTo;
        private System.Windows.Forms.DateTimePicker dpFrom;
    }
}