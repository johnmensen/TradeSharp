namespace TradeSharp.DbMigration.App
{
    partial class MainForm
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblCurVersion = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbConnectionString = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbDatabase = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnLog = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.lbRevision = new System.Windows.Forms.ListBox();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.lblCurVersion);
            this.panelTop.Controls.Add(this.label4);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Controls.Add(this.tbConnectionString);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Controls.Add(this.cbDatabase);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(4);
            this.panelTop.Size = new System.Drawing.Size(379, 116);
            this.panelTop.TabIndex = 2;
            // 
            // lblCurVersion
            // 
            this.lblCurVersion.AutoSize = true;
            this.lblCurVersion.Location = new System.Drawing.Point(241, 13);
            this.lblCurVersion.Name = "lblCurVersion";
            this.lblCurVersion.Size = new System.Drawing.Size(10, 13);
            this.lblCurVersion.TabIndex = 7;
            this.lblCurVersion.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(143, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "текущая версия:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Строка подключения";
            // 
            // tbConnectionString
            // 
            this.tbConnectionString.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbConnectionString.Location = new System.Drawing.Point(4, 71);
            this.tbConnectionString.Name = "tbConnectionString";
            this.tbConnectionString.Size = new System.Drawing.Size(371, 41);
            this.tbConnectionString.TabIndex = 4;
            this.tbConnectionString.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "База данных";
            // 
            // cbDatabase
            // 
            this.cbDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDatabase.FormattingEnabled = true;
            this.cbDatabase.Location = new System.Drawing.Point(7, 29);
            this.cbDatabase.Name = "cbDatabase";
            this.cbDatabase.Size = new System.Drawing.Size(257, 21);
            this.cbDatabase.TabIndex = 2;
            this.cbDatabase.SelectedIndexChanged += new System.EventHandler(this.cbDatabase_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Location = new System.Drawing.Point(0, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Ревизия";
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnLog);
            this.panelBottom.Controls.Add(this.btnApply);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 336);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(379, 36);
            this.panelBottom.TabIndex = 5;
            // 
            // btnLog
            // 
            this.btnLog.Location = new System.Drawing.Point(189, 6);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(125, 23);
            this.btnLog.TabIndex = 1;
            this.btnLog.Text = "Показать лог";
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(10, 6);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(110, 23);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Миграция";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lbRevision
            // 
            this.lbRevision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRevision.FormattingEnabled = true;
            this.lbRevision.Location = new System.Drawing.Point(0, 129);
            this.lbRevision.Name = "lbRevision";
            this.lbRevision.Size = new System.Drawing.Size(379, 207);
            this.lbRevision.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 372);
            this.Controls.Add(this.lbRevision);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Миграция БД Trade#";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox tbConnectionString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.ListBox lbRevision;
        private System.Windows.Forms.Label lblCurVersion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnLog;

    }
}

