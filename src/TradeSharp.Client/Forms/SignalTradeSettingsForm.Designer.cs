namespace TradeSharp.Client.Forms
{
    partial class SignalTradeSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SignalTradeSettingsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.labelMaxLev = new System.Windows.Forms.Label();
            this.labelPercLev = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelHedgeOrd = new System.Windows.Forms.Label();
            this.labelMagic = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelFixVol = new System.Windows.Forms.Label();
            this.cbLockOrders = new System.Windows.Forms.CheckBox();
            this.tbMagic = new System.Windows.Forms.TextBox();
            this.tbMaxLeverage = new System.Windows.Forms.TextBox();
            this.tbLeveragePercent = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbFixVolume = new System.Windows.Forms.CheckBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbFixVolume = new System.Windows.Forms.TextBox();
            this.cbRoundType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelRoundVol = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbMinVolume = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbStepVolume = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cbTradeAuto = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "макс. плечо";
            // 
            // labelMaxLev
            // 
            this.labelMaxLev.AutoSize = true;
            this.labelMaxLev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMaxLev.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelMaxLev.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelMaxLev.Location = new System.Drawing.Point(120, 29);
            this.labelMaxLev.Name = "labelMaxLev";
            this.labelMaxLev.Size = new System.Drawing.Size(14, 13);
            this.labelMaxLev.TabIndex = 1;
            this.labelMaxLev.Text = "?";
            this.labelMaxLev.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // labelPercLev
            // 
            this.labelPercLev.AutoSize = true;
            this.labelPercLev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelPercLev.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelPercLev.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelPercLev.Location = new System.Drawing.Point(120, 53);
            this.labelPercLev.Name = "labelPercLev";
            this.labelPercLev.Size = new System.Drawing.Size(14, 13);
            this.labelPercLev.TabIndex = 3;
            this.labelPercLev.Text = "?";
            this.labelPercLev.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "процент плеча";
            // 
            // labelHedgeOrd
            // 
            this.labelHedgeOrd.AutoSize = true;
            this.labelHedgeOrd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelHedgeOrd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelHedgeOrd.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelHedgeOrd.Location = new System.Drawing.Point(120, 78);
            this.labelHedgeOrd.Name = "labelHedgeOrd";
            this.labelHedgeOrd.Size = new System.Drawing.Size(14, 13);
            this.labelHedgeOrd.TabIndex = 5;
            this.labelHedgeOrd.Text = "?";
            this.labelHedgeOrd.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // labelMagic
            // 
            this.labelMagic.AutoSize = true;
            this.labelMagic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMagic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelMagic.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelMagic.Location = new System.Drawing.Point(120, 102);
            this.labelMagic.Name = "labelMagic";
            this.labelMagic.Size = new System.Drawing.Size(14, 13);
            this.labelMagic.TabIndex = 7;
            this.labelMagic.Text = "?";
            this.labelMagic.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(88, 102);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "magic";
            // 
            // labelFixVol
            // 
            this.labelFixVol.AutoSize = true;
            this.labelFixVol.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelFixVol.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelFixVol.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelFixVol.Location = new System.Drawing.Point(119, 203);
            this.labelFixVol.Name = "labelFixVol";
            this.labelFixVol.Size = new System.Drawing.Size(14, 13);
            this.labelFixVol.TabIndex = 9;
            this.labelFixVol.Text = "?";
            this.labelFixVol.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // cbLockOrders
            // 
            this.cbLockOrders.AutoSize = true;
            this.cbLockOrders.Location = new System.Drawing.Point(7, 77);
            this.cbLockOrders.Name = "cbLockOrders";
            this.cbLockOrders.Size = new System.Drawing.Size(119, 17);
            this.cbLockOrders.TabIndex = 10;
            this.cbLockOrders.Text = "встречные ордера";
            this.cbLockOrders.UseVisualStyleBackColor = true;
            // 
            // tbMagic
            // 
            this.tbMagic.Location = new System.Drawing.Point(140, 99);
            this.tbMagic.Name = "tbMagic";
            this.tbMagic.Size = new System.Drawing.Size(56, 20);
            this.tbMagic.TabIndex = 11;
            // 
            // tbMaxLeverage
            // 
            this.tbMaxLeverage.Location = new System.Drawing.Point(140, 26);
            this.tbMaxLeverage.Name = "tbMaxLeverage";
            this.tbMaxLeverage.Size = new System.Drawing.Size(56, 20);
            this.tbMaxLeverage.TabIndex = 12;
            this.tbMaxLeverage.Text = "5";
            // 
            // tbLeveragePercent
            // 
            this.tbLeveragePercent.Location = new System.Drawing.Point(140, 50);
            this.tbLeveragePercent.Name = "tbLeveragePercent";
            this.tbLeveragePercent.Size = new System.Drawing.Size(56, 20);
            this.tbLeveragePercent.TabIndex = 13;
            this.tbLeveragePercent.Text = "100";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(197, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "%";
            // 
            // cbFixVolume
            // 
            this.cbFixVolume.AutoSize = true;
            this.cbFixVolume.Location = new System.Drawing.Point(35, 202);
            this.cbFixVolume.Name = "cbFixVolume";
            this.cbFixVolume.Size = new System.Drawing.Size(91, 17);
            this.cbFixVolume.TabIndex = 15;
            this.cbFixVolume.Text = "фикс. объем";
            this.cbFixVolume.UseVisualStyleBackColor = true;
            this.cbFixVolume.CheckedChanged += new System.EventHandler(this.CbFixVolumeCheckedChanged);
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(7, 237);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 16;
            this.btnAccept.Text = "Принять";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAcceptClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(123, 237);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tbFixVolume
            // 
            this.tbFixVolume.Enabled = false;
            this.tbFixVolume.Location = new System.Drawing.Point(139, 200);
            this.tbFixVolume.Name = "tbFixVolume";
            this.tbFixVolume.Size = new System.Drawing.Size(56, 20);
            this.tbFixVolume.TabIndex = 18;
            this.tbFixVolume.Text = "10 000";
            // 
            // cbRoundType
            // 
            this.cbRoundType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRoundType.FormattingEnabled = true;
            this.cbRoundType.Items.AddRange(new object[] {
            "Ближайшее",
            "Вниз",
            "Вверх"});
            this.cbRoundType.Location = new System.Drawing.Point(140, 123);
            this.cbRoundType.Name = "cbRoundType";
            this.cbRoundType.Size = new System.Drawing.Size(97, 21);
            this.cbRoundType.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "округлять объем";
            // 
            // labelRoundVol
            // 
            this.labelRoundVol.AutoSize = true;
            this.labelRoundVol.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelRoundVol.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelRoundVol.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelRoundVol.Location = new System.Drawing.Point(120, 126);
            this.labelRoundVol.Name = "labelRoundVol";
            this.labelRoundVol.Size = new System.Drawing.Size(14, 13);
            this.labelRoundVol.TabIndex = 22;
            this.labelRoundVol.Text = "?";
            this.labelRoundVol.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 9000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label3.Location = new System.Drawing.Point(119, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 24;
            this.label3.Text = "?";
            this.toolTip.SetToolTip(this.label3, "Минимальный объем сделки (округление производится до минимального объема, сверх т" +
                    "ого - с учетом шага объема)");
            this.label3.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label7.Location = new System.Drawing.Point(119, 177);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(14, 13);
            this.label7.TabIndex = 27;
            this.label7.Text = "?";
            this.toolTip.SetToolTip(this.label7, "Объем сделки, больше минимального, округляется на шаг плеча");
            this.label7.Click += new System.EventHandler(this.LabelMaxLevClick);
            // 
            // tbMinVolume
            // 
            this.tbMinVolume.Location = new System.Drawing.Point(139, 149);
            this.tbMinVolume.Name = "tbMinVolume";
            this.tbMinVolume.Size = new System.Drawing.Size(56, 20);
            this.tbMinVolume.TabIndex = 25;
            this.tbMinVolume.Text = "10 000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(58, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "мин. объем";
            // 
            // tbStepVolume
            // 
            this.tbStepVolume.Location = new System.Drawing.Point(139, 174);
            this.tbStepVolume.Name = "tbStepVolume";
            this.tbStepVolume.Size = new System.Drawing.Size(56, 20);
            this.tbStepVolume.TabIndex = 28;
            this.tbStepVolume.Text = "10 000";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(55, 177);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "шаг объема";
            // 
            // cbTradeAuto
            // 
            this.cbTradeAuto.AutoSize = true;
            this.cbTradeAuto.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbTradeAuto.Location = new System.Drawing.Point(7, 6);
            this.cbTradeAuto.Name = "cbTradeAuto";
            this.cbTradeAuto.Size = new System.Drawing.Size(155, 17);
            this.cbTradeAuto.TabIndex = 29;
            this.cbTradeAuto.Text = "торговать по сигналу";
            this.cbTradeAuto.UseVisualStyleBackColor = true;
            // 
            // SignalTradeSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 266);
            this.Controls.Add(this.cbTradeAuto);
            this.Controls.Add(this.tbStepVolume);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbMinVolume);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labelRoundVol);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbRoundType);
            this.Controls.Add(this.tbFixVolume);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.labelFixVol);
            this.Controls.Add(this.cbFixVolume);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbLeveragePercent);
            this.Controls.Add(this.tbMaxLeverage);
            this.Controls.Add(this.tbMagic);
            this.Controls.Add(this.labelHedgeOrd);
            this.Controls.Add(this.cbLockOrders);
            this.Controls.Add(this.labelMagic);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.labelPercLev);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelMaxLev);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SignalTradeSettingsForm";
            this.Text = "Торговля по сигналу";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelMaxLev;
        private System.Windows.Forms.Label labelPercLev;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelHedgeOrd;
        private System.Windows.Forms.Label labelMagic;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelFixVol;
        private System.Windows.Forms.CheckBox cbLockOrders;
        private System.Windows.Forms.TextBox tbMagic;
        private System.Windows.Forms.TextBox tbMaxLeverage;
        private System.Windows.Forms.TextBox tbLeveragePercent;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbFixVolume;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbFixVolume;
        private System.Windows.Forms.ComboBox cbRoundType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelRoundVol;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TextBox tbMinVolume;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbStepVolume;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox cbTradeAuto;
    }
}