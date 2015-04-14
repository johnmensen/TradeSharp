using TradeSharp.UI.Util.Control;

namespace TradeSharp.Client.Controls
{
    partial class RiskSetupControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RiskSetupControl));
            this.panelTop = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.cbRoundVolume = new System.Windows.Forms.ComboBox();
            this.tbLeverage = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbDealByTickerCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTickerCount = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.imageListGlypth = new System.Windows.Forms.ImageList(this.components);
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.cbUseLeverageAsDefault = new System.Windows.Forms.CheckBox();
            this.panelLeverage = new System.Windows.Forms.Panel();
            this.tbOrderLeverage = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCalcLeverage = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbTicker = new TradeSharp.UI.Util.Control.TickerComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbResultRounded = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbResultedVolume = new System.Windows.Forms.TextBox();
            this.lblCurrency = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbBalance = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.balanceUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.panelTop.SuspendLayout();
            this.panelMiddle.SuspendLayout();
            this.panelLeverage.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.label9);
            this.panelTop.Controls.Add(this.cbRoundVolume);
            this.panelTop.Controls.Add(this.tbLeverage);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.tbDealByTickerCount);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.tbTickerCount);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(327, 150);
            this.panelTop.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(155, 125);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(132, 13);
            this.label9.TabIndex = 13;
            this.label9.Tag = "TitleRoundEnterVolumeSmall";
            this.label9.Text = "(округлять объем входа)";
            // 
            // cbRoundVolume
            // 
            this.cbRoundVolume.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRoundVolume.FormattingEnabled = true;
            this.cbRoundVolume.Location = new System.Drawing.Point(6, 121);
            this.cbRoundVolume.Name = "cbRoundVolume";
            this.cbRoundVolume.Size = new System.Drawing.Size(143, 21);
            this.cbRoundVolume.TabIndex = 12;
            this.cbRoundVolume.SelectedIndexChanged += new System.EventHandler(this.CbTickerSelectedIndexChanged);
            // 
            // tbLeverage
            // 
            this.tbLeverage.Location = new System.Drawing.Point(6, 97);
            this.tbLeverage.Name = "tbLeverage";
            this.tbLeverage.Size = new System.Drawing.Size(56, 20);
            this.tbLeverage.TabIndex = 5;
            this.tbLeverage.Text = "10";
            this.tbLeverage.TextChanged += new System.EventHandler(this.TbTickerCountTextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(251, 13);
            this.label3.TabIndex = 4;
            this.label3.Tag = "MessageWhichSummaryLeverageIPreferAllowable";
            this.label3.Text = "Какое суммарное плечо я считаю допустимым?";
            // 
            // tbDealByTickerCount
            // 
            this.tbDealByTickerCount.Location = new System.Drawing.Point(6, 58);
            this.tbDealByTickerCount.Name = "tbDealByTickerCount";
            this.tbDealByTickerCount.Size = new System.Drawing.Size(56, 20);
            this.tbDealByTickerCount.TabIndex = 3;
            this.tbDealByTickerCount.Text = "5";
            this.tbDealByTickerCount.TextChanged += new System.EventHandler(this.TbTickerCountTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(221, 13);
            this.label2.TabIndex = 2;
            this.label2.Tag = "MessageHowMuchDealsOnEachIOpen";
            this.label2.Text = "Сколько сделок по каждому я открываю?";
            // 
            // tbTickerCount
            // 
            this.tbTickerCount.Location = new System.Drawing.Point(6, 19);
            this.tbTickerCount.Name = "tbTickerCount";
            this.tbTickerCount.Size = new System.Drawing.Size(56, 20);
            this.tbTickerCount.TabIndex = 1;
            this.tbTickerCount.Text = "4";
            this.tbTickerCount.TextChanged += new System.EventHandler(this.TbTickerCountTextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(199, 13);
            this.label1.TabIndex = 0;
            this.label1.Tag = "MessageHowMuchInstrumentsITrade";
            this.label1.Text = "Сколькими инструментами я торгую?";
            // 
            // imageListGlypth
            // 
            this.imageListGlypth.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlypth.ImageStream")));
            this.imageListGlypth.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlypth.Images.SetKeyName(0, "glypth_big_arrow_down.png");
            // 
            // panelMiddle
            // 
            this.panelMiddle.Controls.Add(this.cbUseLeverageAsDefault);
            this.panelMiddle.Controls.Add(this.panelLeverage);
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMiddle.Location = new System.Drawing.Point(0, 150);
            this.panelMiddle.Name = "panelMiddle";
            this.panelMiddle.Size = new System.Drawing.Size(327, 91);
            this.panelMiddle.TabIndex = 5;
            // 
            // cbUseLeverageAsDefault
            // 
            this.cbUseLeverageAsDefault.AutoSize = true;
            this.cbUseLeverageAsDefault.Location = new System.Drawing.Point(3, 72);
            this.cbUseLeverageAsDefault.Name = "cbUseLeverageAsDefault";
            this.cbUseLeverageAsDefault.Size = new System.Drawing.Size(133, 17);
            this.cbUseLeverageAsDefault.TabIndex = 6;
            this.cbUseLeverageAsDefault.Tag = "TitleDefaultVolumeSmall";
            this.cbUseLeverageAsDefault.Text = "объем по-умолчанию";
            this.cbUseLeverageAsDefault.UseVisualStyleBackColor = true;
            this.cbUseLeverageAsDefault.CheckedChanged += new System.EventHandler(this.CbUseLeverageAsDefaultCheckedChanged);
            // 
            // panelLeverage
            // 
            this.panelLeverage.Controls.Add(this.tbOrderLeverage);
            this.panelLeverage.Controls.Add(this.label4);
            this.panelLeverage.Controls.Add(this.btnCalcLeverage);
            this.panelLeverage.Location = new System.Drawing.Point(88, 4);
            this.panelLeverage.Name = "panelLeverage";
            this.panelLeverage.Size = new System.Drawing.Size(151, 66);
            this.panelLeverage.TabIndex = 5;
            // 
            // tbOrderLeverage
            // 
            this.tbOrderLeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbOrderLeverage.Location = new System.Drawing.Point(100, 41);
            this.tbOrderLeverage.Name = "tbOrderLeverage";
            this.tbOrderLeverage.Size = new System.Drawing.Size(47, 20);
            this.tbOrderLeverage.TabIndex = 6;
            this.tbOrderLeverage.Text = "0.5";
            this.tbOrderLeverage.TextChanged += new System.EventHandler(this.TbOrderLeverageTextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(2, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 5;
            this.label4.Tag = "TitleDealLeverage";
            this.label4.Text = "Плечо сделки";
            // 
            // btnCalcLeverage
            // 
            this.btnCalcLeverage.ImageIndex = 0;
            this.btnCalcLeverage.ImageList = this.imageListGlypth;
            this.btnCalcLeverage.Location = new System.Drawing.Point(48, 3);
            this.btnCalcLeverage.Name = "btnCalcLeverage";
            this.btnCalcLeverage.Size = new System.Drawing.Size(46, 32);
            this.btnCalcLeverage.TabIndex = 4;
            this.btnCalcLeverage.UseVisualStyleBackColor = true;
            this.btnCalcLeverage.Click += new System.EventHandler(this.BtnCalcLeverageClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbTicker);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.tbResultRounded);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.tbResultedVolume);
            this.groupBox1.Controls.Add(this.lblCurrency);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.tbBalance);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 241);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(327, 103);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Tag = "TitleCalculationExample";
            this.groupBox1.Text = "Пример расчета";
            // 
            // cbTicker
            // 
            this.cbTicker.FormattingEnabled = true;
            this.cbTicker.Location = new System.Drawing.Point(131, 46);
            this.cbTicker.Name = "cbTicker";
            this.cbTicker.Size = new System.Drawing.Size(121, 21);
            this.cbTicker.TabIndex = 17;
            this.cbTicker.SelectedIndexChanged += new System.EventHandler(this.CbTickerSelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(162, 78);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 16;
            this.label8.Tag = "TitleRoundedSmall";
            this.label8.Text = "округлен";
            // 
            // tbResultRounded
            // 
            this.tbResultRounded.Location = new System.Drawing.Point(221, 75);
            this.tbResultRounded.Name = "tbResultRounded";
            this.tbResultRounded.Size = new System.Drawing.Size(100, 20);
            this.tbResultRounded.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 78);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 13);
            this.label7.TabIndex = 14;
            this.label7.Tag = "TitleVolume";
            this.label7.Text = "Объем";
            // 
            // tbResultedVolume
            // 
            this.tbResultedVolume.Location = new System.Drawing.Point(56, 75);
            this.tbResultedVolume.Name = "tbResultedVolume";
            this.tbResultedVolume.Size = new System.Drawing.Size(100, 20);
            this.tbResultedVolume.TabIndex = 13;
            // 
            // lblCurrency
            // 
            this.lblCurrency.AutoSize = true;
            this.lblCurrency.Location = new System.Drawing.Point(213, 22);
            this.lblCurrency.Name = "lblCurrency";
            this.lblCurrency.Size = new System.Drawing.Size(10, 13);
            this.lblCurrency.TabIndex = 10;
            this.lblCurrency.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 13);
            this.label6.TabIndex = 9;
            this.label6.Tag = "MessageIBuyOrSell";
            this.label6.Text = "Я покупаю / продаю";
            // 
            // tbBalance
            // 
            this.tbBalance.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.tbBalance.Location = new System.Drawing.Point(131, 19);
            this.tbBalance.Name = "tbBalance";
            this.tbBalance.Size = new System.Drawing.Size(76, 20);
            this.tbBalance.TabIndex = 8;
            this.tbBalance.Text = "10 000";
            this.tbBalance.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TbBalanceKeyDown);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 7;
            this.label5.Tag = "TitleAccountBalance";
            this.label5.Text = "Баланс счета";
            // 
            // balanceUpdateTimer
            // 
            this.balanceUpdateTimer.Interval = 3000;
            this.balanceUpdateTimer.Tick += new System.EventHandler(this.BalanceUpdateTimerTick);
            // 
            // RiskSetupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panelMiddle);
            this.Controls.Add(this.panelTop);
            this.Name = "RiskSetupControl";
            this.Size = new System.Drawing.Size(327, 344);
            this.Load += new System.EventHandler(this.RiskSetupControlLoad);
            this.SizeChanged += new System.EventHandler(this.RiskSetupControlSizeChanged);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelMiddle.ResumeLayout(false);
            this.panelMiddle.PerformLayout();
            this.panelLeverage.ResumeLayout(false);
            this.panelLeverage.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDealByTickerCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbTickerCount;
        private System.Windows.Forms.TextBox tbLeverage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ImageList imageListGlypth;
        private System.Windows.Forms.Panel panelMiddle;
        private System.Windows.Forms.Panel panelLeverage;
        private System.Windows.Forms.TextBox tbOrderLeverage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCalcLeverage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbResultedVolume;
        private System.Windows.Forms.Label lblCurrency;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbBalance;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbResultRounded;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cbRoundVolume;
        private System.Windows.Forms.CheckBox cbUseLeverageAsDefault;
        private TickerComboBox cbTicker;
        private System.Windows.Forms.Timer balanceUpdateTimer;
    }
}
