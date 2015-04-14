namespace TradeSharp.Client.Controls
{
    partial class WebMoneyPursePanelControl
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnAddPurse = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnAddPurse);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(362, 29);
            this.panelTop.TabIndex = 0;
            // 
            // btnAddPurse
            // 
            this.btnAddPurse.Location = new System.Drawing.Point(3, 3);
            this.btnAddPurse.Name = "btnAddPurse";
            this.btnAddPurse.Size = new System.Drawing.Size(130, 23);
            this.btnAddPurse.TabIndex = 0;
            this.btnAddPurse.Tag = "TitleAddWallet";
            this.btnAddPurse.Text = "Добавить кошелек";
            this.btnAddPurse.UseVisualStyleBackColor = true;
            this.btnAddPurse.Click += new System.EventHandler(this.BtnAddPurseClick);
            // 
            // WebMoneyPursePanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelTop);
            this.Name = "WebMoneyPursePanelControl";
            this.Size = new System.Drawing.Size(362, 145);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnAddPurse;

    }
}
