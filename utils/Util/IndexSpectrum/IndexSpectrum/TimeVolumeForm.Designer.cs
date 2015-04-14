namespace IndexSpectrum
{
    partial class TimeVolumeForm
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
            this.dpStart = new System.Windows.Forms.DateTimePicker();
            this.btnCalc = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.cbUseStart = new System.Windows.Forms.CheckBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // dpStart
            // 
            this.dpStart.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpStart.Location = new System.Drawing.Point(115, 26);
            this.dpStart.Name = "dpStart";
            this.dpStart.Size = new System.Drawing.Size(125, 20);
            this.dpStart.TabIndex = 2;
            // 
            // btnCalc
            // 
            this.btnCalc.Location = new System.Drawing.Point(4, 64);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(115, 23);
            this.btnCalc.TabIndex = 4;
            this.btnCalc.Text = "считать в буфер";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.tbFileName);
            this.panelTop.Controls.Add(this.btnBrowse);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(365, 22);
            this.panelTop.TabIndex = 5;
            // 
            // tbFileName
            // 
            this.tbFileName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFileName.Location = new System.Drawing.Point(0, 0);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(340, 20);
            this.tbFileName.TabIndex = 7;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBrowse.Location = new System.Drawing.Point(340, 0);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(25, 22);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // cbUseStart
            // 
            this.cbUseStart.AutoSize = true;
            this.cbUseStart.Location = new System.Drawing.Point(4, 28);
            this.cbUseStart.Name = "cbUseStart";
            this.cbUseStart.Size = new System.Drawing.Size(96, 17);
            this.cbUseStart.TabIndex = 6;
            this.cbUseStart.Text = "старт отсчета";
            this.cbUseStart.UseVisualStyleBackColor = true;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "txt";
            this.openFileDialog.Filter = "csv|*.txt;*.csv|*.*|*.*";
            this.openFileDialog.Title = "Файл экспорта МТ4";
            // 
            // TimeVolumeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 96);
            this.Controls.Add(this.cbUseStart);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.dpStart);
            this.Name = "TimeVolumeForm";
            this.Text = "Объем по времени";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dpStart;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.CheckBox cbUseStart;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}