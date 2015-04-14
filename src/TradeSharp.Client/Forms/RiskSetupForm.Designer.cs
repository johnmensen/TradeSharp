namespace TradeSharp.Client.Forms
{
    partial class RiskSetupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RiskSetupForm));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAccept = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageLeverage = new System.Windows.Forms.TabPage();
            this.riskSetupControl = new TradeSharp.Client.Controls.RiskSetupControl();
            this.tabPageColor = new System.Windows.Forms.TabPage();
            this.tbCurLeverage = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbRiskCritical = new System.Windows.Forms.TextBox();
            this.tbRiskWarning = new System.Windows.Forms.TextBox();
            this.cbMessageOnDealOpening = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.picLeverage = new System.Windows.Forms.PictureBox();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.imageListGlypth = new System.Windows.Forms.ImageList(this.components);
            this.timerUpdateRisk = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPageLeverage.SuspendLayout();
            this.tabPageColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLeverage)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(206, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleReset";
            this.btnCancel.Text = "Сбросить";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnAccept
            // 
            this.btnAccept.AutoSize = true;
            this.btnAccept.Enabled = false;
            this.btnAccept.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnAccept.Location = new System.Drawing.Point(125, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 0;
            this.btnAccept.Tag = "TitleOK";
            this.btnAccept.Text = "ОК";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageLeverage);
            this.tabControl.Controls.Add(this.tabPageColor);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(365, 371);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageLeverage
            // 
            this.tabPageLeverage.Controls.Add(this.riskSetupControl);
            this.tabPageLeverage.Location = new System.Drawing.Point(4, 22);
            this.tabPageLeverage.Name = "tabPageLeverage";
            this.tabPageLeverage.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLeverage.Size = new System.Drawing.Size(357, 345);
            this.tabPageLeverage.TabIndex = 0;
            this.tabPageLeverage.Tag = "TitleEnterVolume";
            this.tabPageLeverage.Text = "Объем входа";
            this.tabPageLeverage.UseVisualStyleBackColor = true;
            // 
            // riskSetupControl
            // 
            this.riskSetupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.riskSetupControl.LeveragePerOrder = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.riskSetupControl.Location = new System.Drawing.Point(3, 3);
            this.riskSetupControl.Name = "riskSetupControl";
            this.riskSetupControl.Size = new System.Drawing.Size(351, 339);
            this.riskSetupControl.TabIndex = 0;
            this.riskSetupControl.Ticker = "";
            // 
            // tabPageColor
            // 
            this.tabPageColor.Controls.Add(this.tbCurLeverage);
            this.tabPageColor.Controls.Add(this.label6);
            this.tabPageColor.Controls.Add(this.tbRiskCritical);
            this.tabPageColor.Controls.Add(this.tbRiskWarning);
            this.tabPageColor.Controls.Add(this.cbMessageOnDealOpening);
            this.tabPageColor.Controls.Add(this.label5);
            this.tabPageColor.Controls.Add(this.label4);
            this.tabPageColor.Controls.Add(this.picLeverage);
            this.tabPageColor.Location = new System.Drawing.Point(4, 22);
            this.tabPageColor.Name = "tabPageColor";
            this.tabPageColor.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageColor.Size = new System.Drawing.Size(357, 345);
            this.tabPageColor.TabIndex = 1;
            this.tabPageColor.Tag = "TitleTradeLeverage";
            this.tabPageColor.Text = "Торговое плечо";
            this.tabPageColor.UseVisualStyleBackColor = true;
            this.tabPageColor.Validating += new System.ComponentModel.CancelEventHandler(this.TabPageColorValidating);
            // 
            // tbCurLeverage
            // 
            this.tbCurLeverage.Enabled = false;
            this.tbCurLeverage.Location = new System.Drawing.Point(122, 163);
            this.tbCurLeverage.Name = "tbCurLeverage";
            this.tbCurLeverage.Size = new System.Drawing.Size(58, 20);
            this.tbCurLeverage.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 166);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 6;
            this.label6.Tag = "TitleCurrentLeverageSmall";
            this.label6.Text = "текущее плечо";
            // 
            // tbRiskCritical
            // 
            this.tbRiskCritical.Location = new System.Drawing.Point(122, 111);
            this.tbRiskCritical.Name = "tbRiskCritical";
            this.tbRiskCritical.Size = new System.Drawing.Size(58, 20);
            this.tbRiskCritical.TabIndex = 5;
            this.tbRiskCritical.TextChanged += new System.EventHandler(this.TbRiskWarningTextChanged);
            // 
            // tbRiskWarning
            // 
            this.tbRiskWarning.Location = new System.Drawing.Point(122, 85);
            this.tbRiskWarning.Name = "tbRiskWarning";
            this.tbRiskWarning.Size = new System.Drawing.Size(58, 20);
            this.tbRiskWarning.TabIndex = 4;
            this.tbRiskWarning.TextChanged += new System.EventHandler(this.TbRiskWarningTextChanged);
            // 
            // cbMessageOnDealOpening
            // 
            this.cbMessageOnDealOpening.AutoSize = true;
            this.cbMessageOnDealOpening.Location = new System.Drawing.Point(6, 140);
            this.cbMessageOnDealOpening.Name = "cbMessageOnDealOpening";
            this.cbMessageOnDealOpening.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbMessageOnDealOpening.Size = new System.Drawing.Size(245, 17);
            this.cbMessageOnDealOpening.TabIndex = 3;
            this.cbMessageOnDealOpening.Tag = "TitleMessageOnRiskExcessOnEnterSmall";
            this.cbMessageOnDealOpening.Text = "сообщение о превышении риска при входе";
            this.cbMessageOnDealOpening.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbMessageOnDealOpening.UseVisualStyleBackColor = true;
            this.cbMessageOnDealOpening.CheckedChanged += new System.EventHandler(this.CbMessageOnDealOpeningCheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 13);
            this.label5.TabIndex = 2;
            this.label5.Tag = "TitleUnallowableRiskSmall";
            this.label5.Text = "недопустимый риск";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 1;
            this.label4.Tag = "TitleHighRiskSmall";
            this.label4.Text = "высокий риск";
            // 
            // picLeverage
            // 
            this.picLeverage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picLeverage.Dock = System.Windows.Forms.DockStyle.Top;
            this.picLeverage.Location = new System.Drawing.Point(3, 3);
            this.picLeverage.Name = "picLeverage";
            this.picLeverage.Size = new System.Drawing.Size(351, 77);
            this.picLeverage.TabIndex = 0;
            this.picLeverage.TabStop = false;
            this.picLeverage.SizeChanged += new System.EventHandler(this.PicLeverageSizeChanged);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "image_step_1.png");
            this.imageList.Images.SetKeyName(1, "image_step_2.png");
            this.imageList.Images.SetKeyName(2, "image_step_3.png");
            this.imageList.Images.SetKeyName(3, "image_step_4.png");
            // 
            // imageListGlypth
            // 
            this.imageListGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlypth.ImageStream")));
            this.imageListGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlypth.Images.SetKeyName(0, "icon_refresh (2).png");
            // 
            // timerUpdateRisk
            // 
            this.timerUpdateRisk.Interval = 4000;
            this.timerUpdateRisk.Tick += new System.EventHandler(this.TimerUpdateRiskTick);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnAccept);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 371);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(365, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(287, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Tag = "TitleCancel";
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // RiskSetupForm
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(365, 400);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(330, 330);
            this.Name = "RiskSetupForm";
            this.Tag = "TitleRiskSettings";
            this.Text = "Настройки риска";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RiskSetupFormFormClosing);
            this.Load += new System.EventHandler(this.RiskSetupFormLoad);
            this.ResizeEnd += new System.EventHandler(this.RiskSetupFormResizeEnd);
            this.Move += new System.EventHandler(this.RiskSetupFormMove);
            this.tabControl.ResumeLayout(false);
            this.tabPageLeverage.ResumeLayout(false);
            this.tabPageColor.ResumeLayout(false);
            this.tabPageColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLeverage)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLeverage;
        private System.Windows.Forms.TabPage tabPageColor;
        private System.Windows.Forms.TextBox tbRiskCritical;
        private System.Windows.Forms.TextBox tbRiskWarning;
        private System.Windows.Forms.CheckBox cbMessageOnDealOpening;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox picLeverage;
        private System.Windows.Forms.TextBox tbCurLeverage;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ImageList imageListGlypth;
        private System.Windows.Forms.ImageList imageList;
        private Controls.RiskSetupControl riskSetupControl;
        private System.Windows.Forms.Timer timerUpdateRisk;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cancelButton;
    }
}