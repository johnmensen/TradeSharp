using TradeSharp.UI.Util.Control;

namespace TradeSharp.Client.Forms
{
    partial class NewChartForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewChartForm));
            this.cbTimeframe = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnInterval = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panelTimeBounds = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.cbTemplates = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbDaysInRequest = new System.Windows.Forms.NumericUpDown();
            this.cbTicker = new TradeSharp.UI.Util.Control.TickerComboBox();
            this.panelBottom.SuspendLayout();
            this.panelTimeBounds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbDaysInRequest)).BeginInit();
            this.SuspendLayout();
            // 
            // cbTimeframe
            // 
            this.cbTimeframe.FormattingEnabled = true;
            this.cbTimeframe.Location = new System.Drawing.Point(12, 66);
            this.cbTimeframe.Name = "cbTimeframe";
            this.cbTimeframe.Size = new System.Drawing.Size(100, 21);
            this.cbTimeframe.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 5;
            this.label1.Tag = "TitleAllTickersSmall";
            this.label1.Text = "все тикеры";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "таймфрейм";
            this.label3.Tag = "TitleTimeframeLower";
            // 
            // btnInterval
            // 
            this.btnInterval.Location = new System.Drawing.Point(193, 64);
            this.btnInterval.Name = "btnInterval";
            this.btnInterval.Size = new System.Drawing.Size(75, 23);
            this.btnInterval.TabIndex = 8;
            this.btnInterval.Text = "еще...";
            this.btnInterval.Tag = "TitleMoreLower";
            this.btnInterval.UseVisualStyleBackColor = true;
            this.btnInterval.Click += new System.EventHandler(this.BtnIntervalClick);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnOK);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 96);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(292, 36);
            this.panelBottom.TabIndex = 9;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(111, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOK.Location = new System.Drawing.Point(3, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Tag = "TitleOK";
            this.btnOK.Text = "ОК";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // panelTimeBounds
            // 
            this.panelTimeBounds.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTimeBounds.Controls.Add(this.label5);
            this.panelTimeBounds.Controls.Add(this.cbTemplates);
            this.panelTimeBounds.Controls.Add(this.label4);
            this.panelTimeBounds.Controls.Add(this.tbDaysInRequest);
            this.panelTimeBounds.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTimeBounds.Location = new System.Drawing.Point(0, 54);
            this.panelTimeBounds.Name = "panelTimeBounds";
            this.panelTimeBounds.Size = new System.Drawing.Size(292, 42);
            this.panelTimeBounds.TabIndex = 10;
            this.panelTimeBounds.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(96, 1);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 10;
            this.label5.Tag = "TitleTemplateSmall";
            this.label5.Text = "шаблон";
            // 
            // cbTemplates
            // 
            this.cbTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTemplates.FormattingEnabled = true;
            this.cbTemplates.Location = new System.Drawing.Point(96, 14);
            this.cbTemplates.Name = "cbTemplates";
            this.cbTemplates.Size = new System.Drawing.Size(171, 21);
            this.cbTemplates.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, -1);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 8;
            this.label4.Tag = "TitleIntervalInDaysSmall";
            this.label4.Text = "интервал, дней";
            // 
            // tbDaysInRequest
            // 
            this.tbDaysInRequest.Location = new System.Drawing.Point(5, 14);
            this.tbDaysInRequest.Name = "tbDaysInRequest";
            this.tbDaysInRequest.Size = new System.Drawing.Size(75, 20);
            this.tbDaysInRequest.TabIndex = 0;
            this.tbDaysInRequest.ValueChanged += new System.EventHandler(this.TbDaysInRequestValueChanged);
            // 
            // cbTicker
            // 
            this.cbTicker.FormattingEnabled = true;
            this.cbTicker.Location = new System.Drawing.Point(12, 12);
            this.cbTicker.Name = "cbTicker";
            this.cbTicker.Size = new System.Drawing.Size(100, 21);
            this.cbTicker.TabIndex = 11;
            this.cbTicker.SelectedIndexChanged += new System.EventHandler(this.CbTickerSelectedIndexChanged);
            // 
            // NewChartForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 132);
            this.Controls.Add(this.cbTicker);
            this.Controls.Add(this.panelTimeBounds);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.btnInterval);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbTimeframe);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewChartForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "TitleNewChart";
            this.Text = "Новый график";
            this.Load += new System.EventHandler(this.NewChartFormLoad);
            this.panelBottom.ResumeLayout(false);
            this.panelTimeBounds.ResumeLayout(false);
            this.panelTimeBounds.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbDaysInRequest)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbTimeframe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnInterval;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panelTimeBounds;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown tbDaysInRequest;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbTemplates;
        private TickerComboBox cbTicker;
    }
}