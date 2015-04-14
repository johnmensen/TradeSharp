namespace TradeSharp.UI.Util.Control
{
    partial class Expander
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
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.pnlHader = new System.Windows.Forms.Panel();
            this.txtHader = new System.Windows.Forms.Label();
            this.btnCollaps = new System.Windows.Forms.Button();
            this.pnlHader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlContainer
            // 
            this.pnlContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlContainer.Location = new System.Drawing.Point(3, 41);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(396, 161);
            this.pnlContainer.TabIndex = 0;
            // 
            // pnlHader
            // 
            this.pnlHader.BackColor = System.Drawing.SystemColors.Control;
            this.pnlHader.Controls.Add(this.txtHader);
            this.pnlHader.Controls.Add(this.btnCollaps);
            this.pnlHader.Location = new System.Drawing.Point(4, 4);
            this.pnlHader.Name = "pnlHader";
            this.pnlHader.Size = new System.Drawing.Size(395, 31);
            this.pnlHader.TabIndex = 1;
            // 
            // txtHader
            // 
            this.txtHader.AutoSize = true;
            this.txtHader.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtHader.Location = new System.Drawing.Point(3, 6);
            this.txtHader.Name = "txtHader";
            this.txtHader.Size = new System.Drawing.Size(0, 17);
            this.txtHader.TabIndex = 1;
            // 
            // btnCollaps
            // 
            this.btnCollaps.Location = new System.Drawing.Point(344, 3);
            this.btnCollaps.Name = "btnCollaps";
            this.btnCollaps.Size = new System.Drawing.Size(46, 23);
            this.btnCollaps.TabIndex = 0;
            this.btnCollaps.Text = ">>";
            this.btnCollaps.UseVisualStyleBackColor = true;
            this.btnCollaps.Click += new System.EventHandler(this.BtnCollapsClick);
            // 
            // Expander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlHader);
            this.Controls.Add(this.pnlContainer);
            this.Name = "Expander";
            this.Size = new System.Drawing.Size(408, 214);
            this.Resize += new System.EventHandler(this.ExpanderResize);
            this.pnlHader.ResumeLayout(false);
            this.pnlHader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.Panel pnlHader;
        private System.Windows.Forms.Button btnCollaps;
        private System.Windows.Forms.Label txtHader;
    }
}
