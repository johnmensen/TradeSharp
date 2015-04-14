namespace TradeSharp.Client.Subscription.Control
{
    partial class TradeSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TradeSetting));
            this.txtAutoTrade = new System.Windows.Forms.Label();
            this.txtMaxLeverage = new System.Windows.Forms.Label();
            this.txtPercentageLeverage = new System.Windows.Forms.Label();
            this.txtHedgeOrders = new System.Windows.Forms.Label();
            this.txtFixedVolume = new System.Windows.Forms.Label();
            this.txtRoundingVolume = new System.Windows.Forms.Label();
            this.txtMinVolume = new System.Windows.Forms.Label();
            this.txtStepVolume = new System.Windows.Forms.Label();
            this.tbAutoTrade = new System.Windows.Forms.CheckBox();
            this.cbHedgeOrders = new System.Windows.Forms.CheckBox();
            this.cbRoundingVolume = new System.Windows.Forms.ComboBox();
            this.tbPercentageLeverage = new System.Windows.Forms.TextBox();
            this.tbFixedVolume = new System.Windows.Forms.TextBox();
            this.tbMaxLeverage = new System.Windows.Forms.TextBox();
            this.tbMinVolume = new System.Windows.Forms.TextBox();
            this.tbStepVolume = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tbMaxVolume = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbAccount = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtAutoTrade
            // 
            this.txtAutoTrade.AutoSize = true;
            this.txtAutoTrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtAutoTrade.Location = new System.Drawing.Point(15, 12);
            this.txtAutoTrade.Name = "txtAutoTrade";
            this.txtAutoTrade.Size = new System.Drawing.Size(207, 17);
            this.txtAutoTrade.TabIndex = 0;
            this.txtAutoTrade.Tag = "TitleTradeAutomatically";
            this.txtAutoTrade.Text = "Торговать автоматически:";
            // 
            // txtMaxLeverage
            // 
            this.txtMaxLeverage.AutoSize = true;
            this.txtMaxLeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtMaxLeverage.Location = new System.Drawing.Point(15, 47);
            this.txtMaxLeverage.Margin = new System.Windows.Forms.Padding(5);
            this.txtMaxLeverage.Name = "txtMaxLeverage";
            this.txtMaxLeverage.Size = new System.Drawing.Size(93, 17);
            this.txtMaxLeverage.TabIndex = 1;
            this.txtMaxLeverage.Tag = "TitleMaximumLeverageShort";
            this.txtMaxLeverage.Text = "Макс. плечо:";
            // 
            // txtPercentageLeverage
            // 
            this.txtPercentageLeverage.AutoSize = true;
            this.txtPercentageLeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtPercentageLeverage.Location = new System.Drawing.Point(16, 74);
            this.txtPercentageLeverage.Margin = new System.Windows.Forms.Padding(5);
            this.txtPercentageLeverage.Name = "txtPercentageLeverage";
            this.txtPercentageLeverage.Size = new System.Drawing.Size(113, 17);
            this.txtPercentageLeverage.TabIndex = 2;
            this.txtPercentageLeverage.Tag = "TitleLeveragePercent";
            this.txtPercentageLeverage.Text = "Процент плеча:";
            // 
            // txtHedgeOrders
            // 
            this.txtHedgeOrders.AutoSize = true;
            this.txtHedgeOrders.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtHedgeOrders.Location = new System.Drawing.Point(16, 101);
            this.txtHedgeOrders.Margin = new System.Windows.Forms.Padding(5);
            this.txtHedgeOrders.Name = "txtHedgeOrders";
            this.txtHedgeOrders.Size = new System.Drawing.Size(102, 17);
            this.txtHedgeOrders.TabIndex = 3;
            this.txtHedgeOrders.Tag = "TitleHedgeOrdersShort";
            this.txtHedgeOrders.Text = "Хедж. ордера:";
            // 
            // txtFixedVolume
            // 
            this.txtFixedVolume.AutoSize = true;
            this.txtFixedVolume.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtFixedVolume.Location = new System.Drawing.Point(16, 127);
            this.txtFixedVolume.Margin = new System.Windows.Forms.Padding(5);
            this.txtFixedVolume.Name = "txtFixedVolume";
            this.txtFixedVolume.Size = new System.Drawing.Size(166, 17);
            this.txtFixedVolume.TabIndex = 5;
            this.txtFixedVolume.Tag = "TitleFixedVolume";
            this.txtFixedVolume.Text = "Фиксированный объем:";
            // 
            // txtRoundingVolume
            // 
            this.txtRoundingVolume.AutoSize = true;
            this.txtRoundingVolume.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtRoundingVolume.Location = new System.Drawing.Point(16, 154);
            this.txtRoundingVolume.Margin = new System.Windows.Forms.Padding(5);
            this.txtRoundingVolume.Name = "txtRoundingVolume";
            this.txtRoundingVolume.Size = new System.Drawing.Size(144, 17);
            this.txtRoundingVolume.TabIndex = 6;
            this.txtRoundingVolume.Tag = "TitleVolumeRounding";
            this.txtRoundingVolume.Text = "Округление объема:";
            // 
            // txtMinVolume
            // 
            this.txtMinVolume.AutoSize = true;
            this.txtMinVolume.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtMinVolume.Location = new System.Drawing.Point(16, 181);
            this.txtMinVolume.Margin = new System.Windows.Forms.Padding(5);
            this.txtMinVolume.Name = "txtMinVolume";
            this.txtMinVolume.Size = new System.Drawing.Size(89, 17);
            this.txtMinVolume.TabIndex = 7;
            this.txtMinVolume.Tag = "TitleMinimalVolumeShort";
            this.txtMinVolume.Text = "Мин. объем:";
            // 
            // txtStepVolume
            // 
            this.txtStepVolume.AutoSize = true;
            this.txtStepVolume.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtStepVolume.Location = new System.Drawing.Point(15, 208);
            this.txtStepVolume.Margin = new System.Windows.Forms.Padding(5);
            this.txtStepVolume.Name = "txtStepVolume";
            this.txtStepVolume.Size = new System.Drawing.Size(90, 17);
            this.txtStepVolume.TabIndex = 8;
            this.txtStepVolume.Tag = "TitleVolumeStep";
            this.txtStepVolume.Text = "Шаг объема:";
            // 
            // tbAutoTrade
            // 
            this.tbAutoTrade.AutoSize = true;
            this.tbAutoTrade.Checked = true;
            this.tbAutoTrade.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tbAutoTrade.Location = new System.Drawing.Point(229, 15);
            this.tbAutoTrade.Name = "tbAutoTrade";
            this.tbAutoTrade.Size = new System.Drawing.Size(15, 14);
            this.tbAutoTrade.TabIndex = 9;
            this.tbAutoTrade.UseVisualStyleBackColor = true;
            // 
            // cbHedgeOrders
            // 
            this.cbHedgeOrders.AutoSize = true;
            this.cbHedgeOrders.Location = new System.Drawing.Point(229, 102);
            this.cbHedgeOrders.Name = "cbHedgeOrders";
            this.cbHedgeOrders.Size = new System.Drawing.Size(15, 14);
            this.cbHedgeOrders.TabIndex = 10;
            this.cbHedgeOrders.UseVisualStyleBackColor = true;
            // 
            // cbRoundingVolume
            // 
            this.cbRoundingVolume.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRoundingVolume.FormattingEnabled = true;
            this.cbRoundingVolume.Location = new System.Drawing.Point(198, 154);
            this.cbRoundingVolume.Name = "cbRoundingVolume";
            this.cbRoundingVolume.Size = new System.Drawing.Size(121, 21);
            this.cbRoundingVolume.TabIndex = 11;
            // 
            // tbPercentageLeverage
            // 
            this.tbPercentageLeverage.Location = new System.Drawing.Point(229, 74);
            this.tbPercentageLeverage.Name = "tbPercentageLeverage";
            this.tbPercentageLeverage.Size = new System.Drawing.Size(73, 20);
            this.tbPercentageLeverage.TabIndex = 12;
            this.tbPercentageLeverage.Text = "100";
            // 
            // tbFixedVolume
            // 
            this.tbFixedVolume.Location = new System.Drawing.Point(229, 124);
            this.tbFixedVolume.Name = "tbFixedVolume";
            this.tbFixedVolume.Size = new System.Drawing.Size(90, 20);
            this.tbFixedVolume.TabIndex = 13;
            // 
            // tbMaxLeverage
            // 
            this.tbMaxLeverage.Location = new System.Drawing.Point(229, 47);
            this.tbMaxLeverage.Name = "tbMaxLeverage";
            this.tbMaxLeverage.Size = new System.Drawing.Size(90, 20);
            this.tbMaxLeverage.TabIndex = 14;
            this.tbMaxLeverage.Text = "100";
            // 
            // tbMinVolume
            // 
            this.tbMinVolume.Location = new System.Drawing.Point(229, 181);
            this.tbMinVolume.Name = "tbMinVolume";
            this.tbMinVolume.Size = new System.Drawing.Size(90, 20);
            this.tbMinVolume.TabIndex = 16;
            this.tbMinVolume.Text = "10 000";
            // 
            // tbStepVolume
            // 
            this.tbStepVolume.Location = new System.Drawing.Point(229, 208);
            this.tbStepVolume.Name = "tbStepVolume";
            this.tbStepVolume.Size = new System.Drawing.Size(90, 20);
            this.tbStepVolume.TabIndex = 17;
            this.tbStepVolume.Text = "10 000";
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOk.Location = new System.Drawing.Point(12, 293);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(117, 27);
            this.btnOk.TabIndex = 18;
            this.btnOk.Tag = "TitleOK";
            this.btnOk.Text = "ОК";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCancel.Location = new System.Drawing.Point(156, 293);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(117, 27);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(304, 76);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 17);
            this.label1.TabIndex = 20;
            this.label1.Text = "%";
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "ico_wizzard.png");
            // 
            // tbMaxVolume
            // 
            this.tbMaxVolume.Location = new System.Drawing.Point(229, 234);
            this.tbMaxVolume.Name = "tbMaxVolume";
            this.tbMaxVolume.Size = new System.Drawing.Size(90, 20);
            this.tbMaxVolume.TabIndex = 22;
            this.tbMaxVolume.Text = "100 000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(16, 234);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 17);
            this.label2.TabIndex = 21;
            this.label2.Tag = "TitleMaximumVolumeShort";
            this.label2.Text = "Макс. объем:";
            // 
            // cbAccount
            // 
            this.cbAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAccount.FormattingEnabled = true;
            this.cbAccount.Location = new System.Drawing.Point(73, 260);
            this.cbAccount.Name = "cbAccount";
            this.cbAccount.Size = new System.Drawing.Size(246, 21);
            this.cbAccount.TabIndex = 24;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(16, 260);
            this.label3.Margin = new System.Windows.Forms.Padding(5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 17);
            this.label3.TabIndex = 23;
            this.label3.Tag = "TitleAccount";
            this.label3.Text = "Счет:";
            // 
            // TradeSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbAccount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbMaxVolume);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tbStepVolume);
            this.Controls.Add(this.tbMinVolume);
            this.Controls.Add(this.tbMaxLeverage);
            this.Controls.Add(this.tbFixedVolume);
            this.Controls.Add(this.tbPercentageLeverage);
            this.Controls.Add(this.cbRoundingVolume);
            this.Controls.Add(this.cbHedgeOrders);
            this.Controls.Add(this.tbAutoTrade);
            this.Controls.Add(this.txtStepVolume);
            this.Controls.Add(this.txtMinVolume);
            this.Controls.Add(this.txtRoundingVolume);
            this.Controls.Add(this.txtFixedVolume);
            this.Controls.Add(this.txtHedgeOrders);
            this.Controls.Add(this.txtPercentageLeverage);
            this.Controls.Add(this.txtMaxLeverage);
            this.Controls.Add(this.txtAutoTrade);
            this.Name = "TradeSetting";
            this.Size = new System.Drawing.Size(330, 328);
            this.Load += new System.EventHandler(this.TradeSetting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label txtAutoTrade;
        private System.Windows.Forms.Label txtMaxLeverage;
        private System.Windows.Forms.Label txtPercentageLeverage;
        private System.Windows.Forms.Label txtHedgeOrders;
        private System.Windows.Forms.Label txtFixedVolume;
        private System.Windows.Forms.Label txtRoundingVolume;
        private System.Windows.Forms.Label txtMinVolume;
        private System.Windows.Forms.Label txtStepVolume;
        private System.Windows.Forms.CheckBox tbAutoTrade;
        private System.Windows.Forms.CheckBox cbHedgeOrders;
        private System.Windows.Forms.ComboBox cbRoundingVolume;
        private System.Windows.Forms.TextBox tbPercentageLeverage;
        private System.Windows.Forms.TextBox tbFixedVolume;
        private System.Windows.Forms.TextBox tbMaxLeverage;
        private System.Windows.Forms.TextBox tbMinVolume;
        private System.Windows.Forms.TextBox tbStepVolume;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TextBox tbMaxVolume;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbAccount;
        private System.Windows.Forms.Label label3;
    }
}
