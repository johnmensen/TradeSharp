namespace TradeSharp.Chat.Client.Form
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.dateTimeFormatTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ownLabel = new System.Windows.Forms.Label();
            this.ownerLabel = new System.Windows.Forms.Label();
            this.othersLabel = new System.Windows.Forms.Label();
            this.ownFontButton = new System.Windows.Forms.Button();
            this.ownColorButton = new System.Windows.Forms.Button();
            this.ownerFontButton = new System.Windows.Forms.Button();
            this.ownerColorButton = new System.Windows.Forms.Button();
            this.othersFontButton = new System.Windows.Forms.Button();
            this.othersColorButton = new System.Windows.Forms.Button();
            this.showLogCheckBox = new System.Windows.Forms.CheckBox();
            this.showNotificationsCheckBox = new System.Windows.Forms.CheckBox();
            this.autoLoginCheckBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 200);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(439, 29);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(361, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(280, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.dateTimeFormatTextBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.autoLoginCheckBox, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.ownLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.ownerLabel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.othersLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.ownFontButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.ownColorButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.ownerFontButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.ownerColorButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.othersFontButton, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.othersColorButton, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.showLogCheckBox, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.showNotificationsCheckBox, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(439, 182);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(175, 26);
            this.label4.TabIndex = 0;
            this.label4.Text = "Формат даты/времени";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dateTimeFormatTextBox
            // 
            this.dateTimeFormatTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimeFormatTextBox.Location = new System.Drawing.Point(184, 90);
            this.dateTimeFormatTextBox.Name = "dateTimeFormatTextBox";
            this.dateTimeFormatTextBox.Size = new System.Drawing.Size(194, 20);
            this.dateTimeFormatTextBox.TabIndex = 1;
            this.dateTimeFormatTextBox.Text = "dd.MM.yyyy HH:ss:mm";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Собственные сообщения";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(175, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "Сообщения владельца команты";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(175, 29);
            this.label3.TabIndex = 2;
            this.label3.Text = "Прочие сообщения";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ownLabel
            // 
            this.ownLabel.AutoSize = true;
            this.ownLabel.BackColor = System.Drawing.SystemColors.Window;
            this.ownLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ownLabel.Location = new System.Drawing.Point(184, 0);
            this.ownLabel.Name = "ownLabel";
            this.ownLabel.Size = new System.Drawing.Size(194, 29);
            this.ownLabel.TabIndex = 3;
            this.ownLabel.Text = "Пример";
            this.ownLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ownerLabel
            // 
            this.ownerLabel.AutoSize = true;
            this.ownerLabel.BackColor = System.Drawing.SystemColors.Window;
            this.ownerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ownerLabel.Location = new System.Drawing.Point(184, 29);
            this.ownerLabel.Name = "ownerLabel";
            this.ownerLabel.Size = new System.Drawing.Size(194, 29);
            this.ownerLabel.TabIndex = 4;
            this.ownerLabel.Text = "Пример";
            this.ownerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // othersLabel
            // 
            this.othersLabel.AutoSize = true;
            this.othersLabel.BackColor = System.Drawing.SystemColors.Window;
            this.othersLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.othersLabel.Location = new System.Drawing.Point(184, 58);
            this.othersLabel.Name = "othersLabel";
            this.othersLabel.Size = new System.Drawing.Size(194, 29);
            this.othersLabel.TabIndex = 5;
            this.othersLabel.Text = "Пример";
            this.othersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ownFontButton
            // 
            this.ownFontButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ownFontButton.Image = ((System.Drawing.Image)(resources.GetObject("ownFontButton.Image")));
            this.ownFontButton.Location = new System.Drawing.Point(384, 3);
            this.ownFontButton.Name = "ownFontButton";
            this.ownFontButton.Size = new System.Drawing.Size(23, 23);
            this.ownFontButton.TabIndex = 6;
            this.ownFontButton.UseVisualStyleBackColor = true;
            this.ownFontButton.Click += new System.EventHandler(this.OwnFontButtonClick);
            // 
            // ownColorButton
            // 
            this.ownColorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ownColorButton.Image = ((System.Drawing.Image)(resources.GetObject("ownColorButton.Image")));
            this.ownColorButton.Location = new System.Drawing.Point(413, 3);
            this.ownColorButton.Name = "ownColorButton";
            this.ownColorButton.Size = new System.Drawing.Size(23, 23);
            this.ownColorButton.TabIndex = 7;
            this.ownColorButton.UseVisualStyleBackColor = true;
            this.ownColorButton.Click += new System.EventHandler(this.OwnColorButtonClick);
            // 
            // ownerFontButton
            // 
            this.ownerFontButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ownerFontButton.Image = ((System.Drawing.Image)(resources.GetObject("ownerFontButton.Image")));
            this.ownerFontButton.Location = new System.Drawing.Point(384, 32);
            this.ownerFontButton.Name = "ownerFontButton";
            this.ownerFontButton.Size = new System.Drawing.Size(23, 23);
            this.ownerFontButton.TabIndex = 8;
            this.ownerFontButton.UseVisualStyleBackColor = true;
            this.ownerFontButton.Click += new System.EventHandler(this.OwnerFontButtonClick);
            // 
            // ownerColorButton
            // 
            this.ownerColorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ownerColorButton.Image = ((System.Drawing.Image)(resources.GetObject("ownerColorButton.Image")));
            this.ownerColorButton.Location = new System.Drawing.Point(413, 32);
            this.ownerColorButton.Name = "ownerColorButton";
            this.ownerColorButton.Size = new System.Drawing.Size(23, 23);
            this.ownerColorButton.TabIndex = 9;
            this.ownerColorButton.UseVisualStyleBackColor = true;
            this.ownerColorButton.Click += new System.EventHandler(this.OwnerColorButtonClick);
            // 
            // othersFontButton
            // 
            this.othersFontButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.othersFontButton.Image = ((System.Drawing.Image)(resources.GetObject("othersFontButton.Image")));
            this.othersFontButton.Location = new System.Drawing.Point(384, 61);
            this.othersFontButton.Name = "othersFontButton";
            this.othersFontButton.Size = new System.Drawing.Size(23, 23);
            this.othersFontButton.TabIndex = 10;
            this.othersFontButton.UseVisualStyleBackColor = true;
            this.othersFontButton.Click += new System.EventHandler(this.OthersFontButtonClick);
            // 
            // othersColorButton
            // 
            this.othersColorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.othersColorButton.Image = ((System.Drawing.Image)(resources.GetObject("othersColorButton.Image")));
            this.othersColorButton.Location = new System.Drawing.Point(413, 61);
            this.othersColorButton.Name = "othersColorButton";
            this.othersColorButton.Size = new System.Drawing.Size(23, 23);
            this.othersColorButton.TabIndex = 11;
            this.othersColorButton.UseVisualStyleBackColor = true;
            this.othersColorButton.Click += new System.EventHandler(this.OthersColorButtonClick);
            // 
            // showLogCheckBox
            // 
            this.showLogCheckBox.AutoSize = true;
            this.showLogCheckBox.Location = new System.Drawing.Point(3, 139);
            this.showLogCheckBox.Name = "showLogCheckBox";
            this.showLogCheckBox.Size = new System.Drawing.Size(109, 17);
            this.showLogCheckBox.TabIndex = 2;
            this.showLogCheckBox.Text = "Показывать лог";
            this.showLogCheckBox.UseVisualStyleBackColor = true;
            // 
            // showNotificationsCheckBox
            // 
            this.showNotificationsCheckBox.AutoSize = true;
            this.showNotificationsCheckBox.Location = new System.Drawing.Point(3, 116);
            this.showNotificationsCheckBox.Name = "showNotificationsCheckBox";
            this.showNotificationsCheckBox.Size = new System.Drawing.Size(159, 17);
            this.showNotificationsCheckBox.TabIndex = 12;
            this.showNotificationsCheckBox.Text = "Показывать уведомления";
            this.showNotificationsCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoLoginCheckBox
            // 
            this.autoLoginCheckBox.AutoSize = true;
            this.autoLoginCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.autoLoginCheckBox.Location = new System.Drawing.Point(3, 162);
            this.autoLoginCheckBox.Name = "autoLoginCheckBox";
            this.autoLoginCheckBox.Size = new System.Drawing.Size(175, 17);
            this.autoLoginCheckBox.TabIndex = 13;
            this.autoLoginCheckBox.Text = "Автоматически входить в чат";
            this.autoLoginCheckBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(439, 229);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 230);
            this.Name = "SettingsForm";
            this.Text = "Настройка чата";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox showLogCheckBox;
        private System.Windows.Forms.TextBox dateTimeFormatTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label ownLabel;
        private System.Windows.Forms.Label ownerLabel;
        private System.Windows.Forms.Label othersLabel;
        private System.Windows.Forms.Button ownFontButton;
        private System.Windows.Forms.Button ownColorButton;
        private System.Windows.Forms.Button ownerFontButton;
        private System.Windows.Forms.Button ownerColorButton;
        private System.Windows.Forms.Button othersFontButton;
        private System.Windows.Forms.Button othersColorButton;
        private System.Windows.Forms.CheckBox showNotificationsCheckBox;
        private System.Windows.Forms.CheckBox autoLoginCheckBox;
    }
}