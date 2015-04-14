namespace DemoPlugin.Dialog
{
    partial class CalculatorForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.tbVolume = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbOptionType = new System.Windows.Forms.ComboBox();
            this.btnCalcPremium = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbStrikePrice = new System.Windows.Forms.TextBox();
            this.tbPriceAtTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dpTimeExpires = new System.Windows.Forms.DateTimePicker();
            this.dpTimeCalc = new System.Windows.Forms.DateTimePicker();
            this.tbResult = new System.Windows.Forms.RichTextBox();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.label6);
            this.panelTop.Controls.Add(this.tbVolume);
            this.panelTop.Controls.Add(this.label5);
            this.panelTop.Controls.Add(this.cbOptionType);
            this.panelTop.Controls.Add(this.btnCalcPremium);
            this.panelTop.Controls.Add(this.label4);
            this.panelTop.Controls.Add(this.label3);
            this.panelTop.Controls.Add(this.tbStrikePrice);
            this.panelTop.Controls.Add(this.tbPriceAtTime);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.dpTimeExpires);
            this.panelTop.Controls.Add(this.dpTimeCalc);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(311, 185);
            this.panelTop.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(182, 99);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "объем (\"ставка\")";
            // 
            // tbVolume
            // 
            this.tbVolume.Location = new System.Drawing.Point(184, 115);
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(100, 20);
            this.tbVolume.TabIndex = 24;
            this.tbVolume.Text = "10 000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 98);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "тип";
            // 
            // cbOptionType
            // 
            this.cbOptionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOptionType.FormattingEnabled = true;
            this.cbOptionType.Items.AddRange(new object[] {
            "CALL",
            "PUT",
            "CALL TOUCH",
            "PUT TOUCH"});
            this.cbOptionType.Location = new System.Drawing.Point(3, 114);
            this.cbOptionType.Name = "cbOptionType";
            this.cbOptionType.Size = new System.Drawing.Size(133, 21);
            this.cbOptionType.TabIndex = 22;
            // 
            // btnCalcPremium
            // 
            this.btnCalcPremium.Location = new System.Drawing.Point(3, 151);
            this.btnCalcPremium.Name = "btnCalcPremium";
            this.btnCalcPremium.Size = new System.Drawing.Size(281, 23);
            this.btnCalcPremium.TabIndex = 21;
            this.btnCalcPremium.Text = "Рассчитать премию";
            this.btnCalcPremium.UseVisualStyleBackColor = true;
            this.btnCalcPremium.Click += new System.EventHandler(this.btnCalcPremium_Click_1);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(182, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "страйк-цена";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(182, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "расчет при цене";
            // 
            // tbStrikePrice
            // 
            this.tbStrikePrice.Location = new System.Drawing.Point(184, 66);
            this.tbStrikePrice.Name = "tbStrikePrice";
            this.tbStrikePrice.Size = new System.Drawing.Size(100, 20);
            this.tbStrikePrice.TabIndex = 18;
            // 
            // tbPriceAtTime
            // 
            this.tbPriceAtTime.Location = new System.Drawing.Point(184, 23);
            this.tbPriceAtTime.Name = "tbPriceAtTime";
            this.tbPriceAtTime.Size = new System.Drawing.Size(100, 20);
            this.tbPriceAtTime.TabIndex = 17;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "экспирация";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "время расчета";
            // 
            // dpTimeExpires
            // 
            this.dpTimeExpires.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpTimeExpires.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpTimeExpires.Location = new System.Drawing.Point(3, 66);
            this.dpTimeExpires.Name = "dpTimeExpires";
            this.dpTimeExpires.Size = new System.Drawing.Size(133, 20);
            this.dpTimeExpires.TabIndex = 14;
            // 
            // dpTimeCalc
            // 
            this.dpTimeCalc.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpTimeCalc.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpTimeCalc.Location = new System.Drawing.Point(3, 23);
            this.dpTimeCalc.Name = "dpTimeCalc";
            this.dpTimeCalc.Size = new System.Drawing.Size(133, 20);
            this.dpTimeCalc.TabIndex = 13;
            // 
            // tbResult
            // 
            this.tbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbResult.Location = new System.Drawing.Point(0, 185);
            this.tbResult.Name = "tbResult";
            this.tbResult.Size = new System.Drawing.Size(311, 151);
            this.tbResult.TabIndex = 1;
            this.tbResult.Text = "";
            // 
            // CalculatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 336);
            this.Controls.Add(this.tbResult);
            this.Controls.Add(this.panelTop);
            this.Name = "CalculatorForm";
            this.Text = "Опционный калькулятор";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbVolume;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbOptionType;
        private System.Windows.Forms.Button btnCalcPremium;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbStrikePrice;
        private System.Windows.Forms.TextBox tbPriceAtTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dpTimeExpires;
        private System.Windows.Forms.DateTimePicker dpTimeCalc;
        private System.Windows.Forms.RichTextBox tbResult;

    }
}