namespace TradeSharp.Client.Forms
{
    partial class ChartSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartSettingsForm));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageVisual = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.cbYAxis = new System.Windows.Forms.ComboBox();
            this.cbAutoScroll = new System.Windows.Forms.CheckBox();
            this.cbUseMarkerPrice = new System.Windows.Forms.CheckBox();
            this.pictureBoxCandle = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbTheme = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbShiftBars = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbChartType = new System.Windows.Forms.ComboBox();
            this.tabPageSettings = new System.Windows.Forms.TabPage();
            this.cbTurnBarsDontSumDegrees = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbTurnBarFilter = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbTurnBarFiboMarks = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTurnBarFiboSequence = new System.Windows.Forms.TextBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.btnApplyToAll = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabControl.SuspendLayout();
            this.tabPageVisual.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCandle)).BeginInit();
            this.tabPageSettings.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageVisual);
            this.tabControl.Controls.Add(this.tabPageSettings);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(394, 346);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageVisual
            // 
            this.tabPageVisual.Controls.Add(this.label8);
            this.tabPageVisual.Controls.Add(this.cbYAxis);
            this.tabPageVisual.Controls.Add(this.cbAutoScroll);
            this.tabPageVisual.Controls.Add(this.cbUseMarkerPrice);
            this.tabPageVisual.Controls.Add(this.pictureBoxCandle);
            this.tabPageVisual.Controls.Add(this.label3);
            this.tabPageVisual.Controls.Add(this.cbTheme);
            this.tabPageVisual.Controls.Add(this.label2);
            this.tabPageVisual.Controls.Add(this.tbShiftBars);
            this.tabPageVisual.Controls.Add(this.label1);
            this.tabPageVisual.Controls.Add(this.cbChartType);
            this.tabPageVisual.Location = new System.Drawing.Point(4, 22);
            this.tabPageVisual.Name = "tabPageVisual";
            this.tabPageVisual.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageVisual.Size = new System.Drawing.Size(386, 320);
            this.tabPageVisual.TabIndex = 0;
            this.tabPageVisual.Tag = "TitleDecor";
            this.tabPageVisual.Text = "Оформление";
            this.tabPageVisual.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(122, 177);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 10;
            this.label8.Tag = "TitlePriceScaleSmall";
            this.label8.Text = "шкала цены";
            // 
            // cbYAxis
            // 
            this.cbYAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbYAxis.FormattingEnabled = true;
            this.cbYAxis.Location = new System.Drawing.Point(8, 174);
            this.cbYAxis.Name = "cbYAxis";
            this.cbYAxis.Size = new System.Drawing.Size(100, 21);
            this.cbYAxis.TabIndex = 9;
            // 
            // cbAutoScroll
            // 
            this.cbAutoScroll.AutoSize = true;
            this.cbAutoScroll.Checked = true;
            this.cbAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoScroll.Location = new System.Drawing.Point(226, 36);
            this.cbAutoScroll.Name = "cbAutoScroll";
            this.cbAutoScroll.Size = new System.Drawing.Size(147, 17);
            this.cbAutoScroll.TabIndex = 8;
            this.cbAutoScroll.Tag = "TitleChartAutoScrollSmall";
            this.cbAutoScroll.Text = "автопрокрутка графика";
            this.cbAutoScroll.UseVisualStyleBackColor = true;
            // 
            // cbUseMarkerPrice
            // 
            this.cbUseMarkerPrice.AutoSize = true;
            this.cbUseMarkerPrice.Checked = true;
            this.cbUseMarkerPrice.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseMarkerPrice.Location = new System.Drawing.Point(226, 10);
            this.cbUseMarkerPrice.Name = "cbUseMarkerPrice";
            this.cbUseMarkerPrice.Size = new System.Drawing.Size(93, 17);
            this.cbUseMarkerPrice.TabIndex = 7;
            this.cbUseMarkerPrice.Tag = "TitlePriceMarker";
            this.cbUseMarkerPrice.Text = "маркер цены";
            this.cbUseMarkerPrice.UseVisualStyleBackColor = true;
            // 
            // pictureBoxCandle
            // 
            this.pictureBoxCandle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCandle.Location = new System.Drawing.Point(8, 86);
            this.pictureBoxCandle.Name = "pictureBoxCandle";
            this.pictureBoxCandle.Size = new System.Drawing.Size(100, 82);
            this.pictureBoxCandle.TabIndex = 6;
            this.pictureBoxCandle.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(122, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 5;
            this.label3.Tag = "TitleColorSchemeSmall";
            this.label3.Text = "цветовая схема";
            // 
            // cbTheme
            // 
            this.cbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTheme.FormattingEnabled = true;
            this.cbTheme.Location = new System.Drawing.Point(8, 59);
            this.cbTheme.Name = "cbTheme";
            this.cbTheme.Size = new System.Drawing.Size(100, 21);
            this.cbTheme.TabIndex = 4;
            this.cbTheme.SelectedIndexChanged += new System.EventHandler(this.CbThemeSelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(122, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 3;
            this.label2.Tag = "TitleRightOffsetSmall";
            this.label2.Text = "отступ справа";
            // 
            // tbShiftBars
            // 
            this.tbShiftBars.Location = new System.Drawing.Point(8, 33);
            this.tbShiftBars.Name = "tbShiftBars";
            this.tbShiftBars.Size = new System.Drawing.Size(100, 20);
            this.tbShiftBars.TabIndex = 2;
            this.tbShiftBars.Text = "5";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(122, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 1;
            this.label1.Tag = "TitleChartTypeSmall";
            this.label1.Text = "вид графика";
            // 
            // cbChartType
            // 
            this.cbChartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChartType.FormattingEnabled = true;
            this.cbChartType.Location = new System.Drawing.Point(8, 6);
            this.cbChartType.Name = "cbChartType";
            this.cbChartType.Size = new System.Drawing.Size(100, 21);
            this.cbChartType.TabIndex = 0;
            // 
            // tabPageSettings
            // 
            this.tabPageSettings.Controls.Add(this.cbTurnBarsDontSumDegrees);
            this.tabPageSettings.Controls.Add(this.label7);
            this.tabPageSettings.Controls.Add(this.tbTurnBarFilter);
            this.tabPageSettings.Controls.Add(this.label6);
            this.tabPageSettings.Controls.Add(this.label5);
            this.tabPageSettings.Controls.Add(this.tbTurnBarFiboMarks);
            this.tabPageSettings.Controls.Add(this.label4);
            this.tabPageSettings.Controls.Add(this.tbTurnBarFiboSequence);
            this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageSettings.Name = "tabPageSettings";
            this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSettings.Size = new System.Drawing.Size(386, 349);
            this.tabPageSettings.TabIndex = 1;
            this.tabPageSettings.Tag = "TitlePersonalization";
            this.tabPageSettings.Text = "Персонализация";
            this.tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // cbTurnBarsDontSumDegrees
            // 
            this.cbTurnBarsDontSumDegrees.AutoSize = true;
            this.cbTurnBarsDontSumDegrees.Location = new System.Drawing.Point(6, 97);
            this.cbTurnBarsDontSumDegrees.Name = "cbTurnBarsDontSumDegrees";
            this.cbTurnBarsDontSumDegrees.Size = new System.Drawing.Size(153, 17);
            this.cbTurnBarsDontSumDegrees.TabIndex = 7;
            this.cbTurnBarsDontSumDegrees.Tag = "TitleDoNotSumDegreesSmall";
            this.cbTurnBarsDontSumDegrees.Text = "не суммировать степени";
            this.cbTurnBarsDontSumDegrees.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(112, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 13);
            this.label7.TabIndex = 6;
            this.label7.Tag = "TitleMarkFilterSmall";
            this.label7.Text = "фильтр отметки";
            // 
            // tbTurnBarFilter
            // 
            this.tbTurnBarFilter.Location = new System.Drawing.Point(6, 71);
            this.tbTurnBarFilter.Name = "tbTurnBarFilter";
            this.tbTurnBarFilter.Size = new System.Drawing.Size(100, 20);
            this.tbTurnBarFilter.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 13);
            this.label6.TabIndex = 4;
            this.label6.Tag = "TitleTurnBars";
            this.label6.Text = "Бары разворота";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 3;
            this.label5.Tag = "TitleProjectionMarksSmall";
            this.label5.Text = "отметки проекций";
            // 
            // tbTurnBarFiboMarks
            // 
            this.tbTurnBarFiboMarks.Location = new System.Drawing.Point(6, 45);
            this.tbTurnBarFiboMarks.Name = "tbTurnBarFiboMarks";
            this.tbTurnBarFiboMarks.Size = new System.Drawing.Size(100, 20);
            this.tbTurnBarFiboMarks.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(112, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 1;
            this.label4.Tag = "TitleProjectionSeriesSmall";
            this.label4.Text = "ряд проекций";
            // 
            // tbTurnBarFiboSequence
            // 
            this.tbTurnBarFiboSequence.Location = new System.Drawing.Point(6, 19);
            this.tbTurnBarFiboSequence.Name = "tbTurnBarFiboSequence";
            this.tbTurnBarFiboSequence.Size = new System.Drawing.Size(100, 20);
            this.tbTurnBarFiboSequence.TabIndex = 0;
            // 
            // btnApplyToAll
            // 
            this.btnApplyToAll.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnApplyToAll.Location = new System.Drawing.Point(96, 3);
            this.btnApplyToAll.Name = "btnApplyToAll";
            this.btnApplyToAll.Size = new System.Drawing.Size(107, 23);
            this.btnApplyToAll.TabIndex = 2;
            this.btnApplyToAll.Tag = "TitleApplyToAll";
            this.btnApplyToAll.Text = "Применить всем";
            this.btnApplyToAll.UseVisualStyleBackColor = true;
            this.btnApplyToAll.Click += new System.EventHandler(this.BtnApplyToAllClick);
            // 
            // btnAccept
            // 
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(209, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(88, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Tag = "TitleOK";
            this.btnAccept.Text = "OK";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(303, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnAccept);
            this.flowLayoutPanel1.Controls.Add(this.btnApplyToAll);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 346);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(394, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // ChartSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(394, 375);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "ChartSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleChartSettings";
            this.Text = "Настройки графика";
            this.Load += new System.EventHandler(this.ChartSettingsFormLoad);
            this.tabControl.ResumeLayout(false);
            this.tabPageVisual.ResumeLayout(false);
            this.tabPageVisual.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCandle)).EndInit();
            this.tabPageSettings.ResumeLayout(false);
            this.tabPageSettings.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageVisual;
        private System.Windows.Forms.TabPage tabPageSettings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbShiftBars;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbChartType;
        private System.Windows.Forms.ComboBox cbTheme;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBoxCandle;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.CheckBox cbUseMarkerPrice;
        private System.Windows.Forms.CheckBox cbAutoScroll;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbTurnBarFilter;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbTurnBarFiboMarks;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTurnBarFiboSequence;
        private System.Windows.Forms.CheckBox cbTurnBarsDontSumDegrees;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbYAxis;
        private System.Windows.Forms.Button btnApplyToAll;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}