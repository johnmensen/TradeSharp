namespace TradeSharp.UI.Util.Forms
{
    partial class PositionForm
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
            this.cbStatus = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbPriceEnter = new System.Windows.Forms.TextBox();
            this.dpEnter = new System.Windows.Forms.DateTimePicker();
            this.dpExit = new System.Windows.Forms.DateTimePicker();
            this.tbPriceExit = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTP = new System.Windows.Forms.TextBox();
            this.tbTrailing = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.cbExitReason = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbMagic = new System.Windows.Forms.TextBox();
            this.tbVolume = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbStatus
            // 
            this.cbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbStatus.FormattingEnabled = true;
            this.cbStatus.Location = new System.Drawing.Point(12, 12);
            this.cbStatus.Name = "cbStatus";
            this.cbStatus.Size = new System.Drawing.Size(121, 21);
            this.cbStatus.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Вход";
            // 
            // tbPriceEnter
            // 
            this.tbPriceEnter.Location = new System.Drawing.Point(15, 52);
            this.tbPriceEnter.Name = "tbPriceEnter";
            this.tbPriceEnter.Size = new System.Drawing.Size(88, 20);
            this.tbPriceEnter.TabIndex = 3;
            // 
            // dpEnter
            // 
            this.dpEnter.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dpEnter.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpEnter.Location = new System.Drawing.Point(109, 52);
            this.dpEnter.Name = "dpEnter";
            this.dpEnter.Size = new System.Drawing.Size(146, 20);
            this.dpEnter.TabIndex = 4;
            // 
            // dpExit
            // 
            this.dpExit.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            this.dpExit.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dpExit.Location = new System.Drawing.Point(109, 126);
            this.dpExit.Name = "dpExit";
            this.dpExit.Size = new System.Drawing.Size(146, 20);
            this.dpExit.TabIndex = 7;
            // 
            // tbPriceExit
            // 
            this.tbPriceExit.Location = new System.Drawing.Point(15, 126);
            this.tbPriceExit.Name = "tbPriceExit";
            this.tbPriceExit.Size = new System.Drawing.Size(88, 20);
            this.tbPriceExit.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Выход";
            // 
            // tbSL
            // 
            this.tbSL.Location = new System.Drawing.Point(15, 165);
            this.tbSL.Name = "tbSL";
            this.tbSL.Size = new System.Drawing.Size(88, 20);
            this.tbSL.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "SL";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(106, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "TP";
            // 
            // tbTP
            // 
            this.tbTP.Location = new System.Drawing.Point(109, 165);
            this.tbTP.Name = "tbTP";
            this.tbTP.Size = new System.Drawing.Size(88, 20);
            this.tbTP.TabIndex = 10;
            // 
            // tbTrailing
            // 
            this.tbTrailing.Location = new System.Drawing.Point(15, 206);
            this.tbTrailing.Name = "tbTrailing";
            this.tbTrailing.Size = new System.Drawing.Size(233, 20);
            this.tbTrailing.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 190);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Трейлинг";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(15, 249);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(233, 20);
            this.tbComment.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 233);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(195, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Комментарий / комментарий робота";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(15, 282);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 16;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // cbExitReason
            // 
            this.cbExitReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbExitReason.FormattingEnabled = true;
            this.cbExitReason.Location = new System.Drawing.Point(139, 12);
            this.cbExitReason.Name = "cbExitReason";
            this.cbExitReason.Size = new System.Drawing.Size(121, 21);
            this.cbExitReason.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(200, 149);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(36, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Magic";
            // 
            // tbMagic
            // 
            this.tbMagic.Location = new System.Drawing.Point(203, 165);
            this.tbMagic.Name = "tbMagic";
            this.tbMagic.Size = new System.Drawing.Size(88, 20);
            this.tbMagic.TabIndex = 18;
            // 
            // tbVolume
            // 
            this.tbVolume.Location = new System.Drawing.Point(15, 78);
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(118, 20);
            this.tbVolume.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(136, 81);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Объем";
            // 
            // PositionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 312);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbVolume);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbMagic);
            this.Controls.Add(this.cbExitReason);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbTrailing);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbTP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbSL);
            this.Controls.Add(this.dpExit);
            this.Controls.Add(this.tbPriceExit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dpEnter);
            this.Controls.Add(this.tbPriceEnter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "PositionForm";
            this.Text = "PositionForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPriceEnter;
        private System.Windows.Forms.DateTimePicker dpEnter;
        private System.Windows.Forms.DateTimePicker dpExit;
        private System.Windows.Forms.TextBox tbPriceExit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTP;
        private System.Windows.Forms.TextBox tbTrailing;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ComboBox cbExitReason;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbMagic;
        private System.Windows.Forms.TextBox tbVolume;
        private System.Windows.Forms.Label label8;
    }
}