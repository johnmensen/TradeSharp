namespace ForexSiteDailyQuoteParser
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
            this.txtOutputFolder = new System.Windows.Forms.Label();
            this.OutputFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.btnSelectOutputFolder = new System.Windows.Forms.Button();
            this.txtSelectedOutputFolder = new System.Windows.Forms.Label();
            this.txtFormats = new System.Windows.Forms.Label();
            this.ddlResourceFormat = new System.Windows.Forms.ComboBox();
            this.ddlOutputFormat = new System.Windows.Forms.ComboBox();
            this.btnParse = new System.Windows.Forms.Button();
            this.listResourceFormate = new System.Windows.Forms.ListBox();
            this.listOutputFormate = new System.Windows.Forms.ListBox();
            this.listErrors = new System.Windows.Forms.ListBox();
            this.btnMarge = new System.Windows.Forms.Button();
            this.listResultContent = new System.Windows.Forms.ListBox();
            this.btnClearListResultContent = new System.Windows.Forms.Button();
            this.listPreview = new System.Windows.Forms.ListBox();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabSourseFiles = new System.Windows.Forms.TabPage();
            this.listOutputFile = new System.Windows.Forms.ListBox();
            this.listResourceFile = new System.Windows.Forms.ListBox();
            this.btnSelectOutputFile = new System.Windows.Forms.Button();
            this.btnSelectResourceFile = new System.Windows.Forms.Button();
            this.tabParsing = new System.Windows.Forms.TabPage();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.rbtnFirstParserSaveFormat = new System.Windows.Forms.RadioButton();
            this.rbtnSecondParserSaveFormat = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabSave = new System.Windows.Forms.TabPage();
            this.btnSaveNewFile = new System.Windows.Forms.Button();
            this.txtNewFileName = new System.Windows.Forms.Label();
            this.txtbxNewFileName = new System.Windows.Forms.TextBox();
            this.fileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tabMain.SuspendLayout();
            this.tabSourseFiles.SuspendLayout();
            this.tabParsing.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabSave.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.AutoSize = true;
            this.txtOutputFolder.Location = new System.Drawing.Point(18, 20);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(203, 13);
            this.txtOutputFolder.TabIndex = 0;
            this.txtOutputFolder.Text = "Каталог для сгенерированных файлов";
            // 
            // btnSelectOutputFolder
            // 
            this.btnSelectOutputFolder.Location = new System.Drawing.Point(227, 15);
            this.btnSelectOutputFolder.Name = "btnSelectOutputFolder";
            this.btnSelectOutputFolder.Size = new System.Drawing.Size(75, 23);
            this.btnSelectOutputFolder.TabIndex = 1;
            this.btnSelectOutputFolder.Text = "Выбрать";
            this.btnSelectOutputFolder.UseVisualStyleBackColor = true;
            this.btnSelectOutputFolder.Click += new System.EventHandler(this.BtnSelectOutputFolderClick);
            // 
            // txtSelectedOutputFolder
            // 
            this.txtSelectedOutputFolder.AutoSize = true;
            this.txtSelectedOutputFolder.Location = new System.Drawing.Point(18, 46);
            this.txtSelectedOutputFolder.Name = "txtSelectedOutputFolder";
            this.txtSelectedOutputFolder.Size = new System.Drawing.Size(88, 13);
            this.txtSelectedOutputFolder.TabIndex = 2;
            this.txtSelectedOutputFolder.Text = "Путь к каталогу";
            // 
            // txtFormats
            // 
            this.txtFormats.AutoSize = true;
            this.txtFormats.Location = new System.Drawing.Point(3, 10);
            this.txtFormats.Name = "txtFormats";
            this.txtFormats.Size = new System.Drawing.Size(102, 13);
            this.txtFormats.TabIndex = 3;
            this.txtFormats.Text = "Форматы записей";
            // 
            // ddlResourceFormat
            // 
            this.ddlResourceFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlResourceFormat.FormattingEnabled = true;
            this.ddlResourceFormat.Location = new System.Drawing.Point(6, 27);
            this.ddlResourceFormat.Name = "ddlResourceFormat";
            this.ddlResourceFormat.Size = new System.Drawing.Size(187, 21);
            this.ddlResourceFormat.TabIndex = 4;
            // 
            // ddlOutputFormat
            // 
            this.ddlOutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlOutputFormat.FormattingEnabled = true;
            this.ddlOutputFormat.Location = new System.Drawing.Point(592, 27);
            this.ddlOutputFormat.Name = "ddlOutputFormat";
            this.ddlOutputFormat.Size = new System.Drawing.Size(187, 21);
            this.ddlOutputFormat.TabIndex = 5;
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(8, 399);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(136, 23);
            this.btnParse.TabIndex = 6;
            this.btnParse.Text = "Распарсить файлы";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.BtnParseClick);
            // 
            // listResourceFormate
            // 
            this.listResourceFormate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listResourceFormate.FormattingEnabled = true;
            this.listResourceFormate.Location = new System.Drawing.Point(278, 0);
            this.listResourceFormate.Name = "listResourceFormate";
            this.listResourceFormate.Size = new System.Drawing.Size(898, 143);
            this.listResourceFormate.TabIndex = 7;
            this.listResourceFormate.SelectedIndexChanged += new System.EventHandler(this.ListResourceFormateSelectedIndexChanged);
            // 
            // listOutputFormate
            // 
            this.listOutputFormate.Dock = System.Windows.Forms.DockStyle.Left;
            this.listOutputFormate.FormattingEnabled = true;
            this.listOutputFormate.Location = new System.Drawing.Point(0, 0);
            this.listOutputFormate.Name = "listOutputFormate";
            this.listOutputFormate.Size = new System.Drawing.Size(278, 143);
            this.listOutputFormate.TabIndex = 8;
            this.listOutputFormate.SelectedIndexChanged += new System.EventHandler(this.ListResourceFormateSelectedIndexChanged);
            // 
            // listErrors
            // 
            this.listErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listErrors.FormattingEnabled = true;
            this.listErrors.Location = new System.Drawing.Point(0, 0);
            this.listErrors.Name = "listErrors";
            this.listErrors.Size = new System.Drawing.Size(1176, 84);
            this.listErrors.TabIndex = 9;
            // 
            // btnMarge
            // 
            this.btnMarge.Location = new System.Drawing.Point(93, 6);
            this.btnMarge.Name = "btnMarge";
            this.btnMarge.Size = new System.Drawing.Size(127, 23);
            this.btnMarge.TabIndex = 10;
            this.btnMarge.Text = "Объединение";
            this.btnMarge.UseVisualStyleBackColor = true;
            this.btnMarge.Click += new System.EventHandler(this.BtnMargeClick);
            // 
            // listResultContent
            // 
            this.listResultContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listResultContent.FormattingEnabled = true;
            this.listResultContent.Location = new System.Drawing.Point(0, 0);
            this.listResultContent.Name = "listResultContent";
            this.listResultContent.Size = new System.Drawing.Size(440, 159);
            this.listResultContent.TabIndex = 12;
            // 
            // btnClearListResultContent
            // 
            this.btnClearListResultContent.Location = new System.Drawing.Point(5, 6);
            this.btnClearListResultContent.Name = "btnClearListResultContent";
            this.btnClearListResultContent.Size = new System.Drawing.Size(82, 23);
            this.btnClearListResultContent.TabIndex = 14;
            this.btnClearListResultContent.Text = "очистить";
            this.btnClearListResultContent.UseVisualStyleBackColor = true;
            this.btnClearListResultContent.Click += new System.EventHandler(this.BtnClearListResultContentClick);
            // 
            // listPreview
            // 
            this.listPreview.Dock = System.Windows.Forms.DockStyle.Right;
            this.listPreview.FormattingEnabled = true;
            this.listPreview.Location = new System.Drawing.Point(440, 0);
            this.listPreview.Name = "listPreview";
            this.listPreview.Size = new System.Drawing.Size(736, 159);
            this.listPreview.TabIndex = 15;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabSourseFiles);
            this.tabMain.Controls.Add(this.tabParsing);
            this.tabMain.Controls.Add(this.tabSave);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1190, 478);
            this.tabMain.TabIndex = 17;
            // 
            // tabSourseFiles
            // 
            this.tabSourseFiles.Controls.Add(this.listOutputFile);
            this.tabSourseFiles.Controls.Add(this.listResourceFile);
            this.tabSourseFiles.Controls.Add(this.btnSelectOutputFile);
            this.tabSourseFiles.Controls.Add(this.btnSelectResourceFile);
            this.tabSourseFiles.Controls.Add(this.btnParse);
            this.tabSourseFiles.Controls.Add(this.txtFormats);
            this.tabSourseFiles.Controls.Add(this.ddlResourceFormat);
            this.tabSourseFiles.Controls.Add(this.ddlOutputFormat);
            this.tabSourseFiles.Location = new System.Drawing.Point(4, 22);
            this.tabSourseFiles.Name = "tabSourseFiles";
            this.tabSourseFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabSourseFiles.Size = new System.Drawing.Size(1182, 452);
            this.tabSourseFiles.TabIndex = 0;
            this.tabSourseFiles.Text = "Файлы";
            this.tabSourseFiles.UseVisualStyleBackColor = true;
            // 
            // listOutputFile
            // 
            this.listOutputFile.FormattingEnabled = true;
            this.listOutputFile.HorizontalScrollbar = true;
            this.listOutputFile.Location = new System.Drawing.Point(592, 83);
            this.listOutputFile.Name = "listOutputFile";
            this.listOutputFile.Size = new System.Drawing.Size(578, 277);
            this.listOutputFile.TabIndex = 10;
            // 
            // listResourceFile
            // 
            this.listResourceFile.FormattingEnabled = true;
            this.listResourceFile.HorizontalScrollbar = true;
            this.listResourceFile.Location = new System.Drawing.Point(8, 83);
            this.listResourceFile.Name = "listResourceFile";
            this.listResourceFile.Size = new System.Drawing.Size(578, 277);
            this.listResourceFile.TabIndex = 9;
            // 
            // btnSelectOutputFile
            // 
            this.btnSelectOutputFile.Location = new System.Drawing.Point(592, 54);
            this.btnSelectOutputFile.Name = "btnSelectOutputFile";
            this.btnSelectOutputFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectOutputFile.TabIndex = 8;
            this.btnSelectOutputFile.Text = "Выбрать";
            this.btnSelectOutputFile.UseVisualStyleBackColor = true;
            this.btnSelectOutputFile.Click += new System.EventHandler(this.BtnSelectOutputFileClick);
            // 
            // btnSelectResourceFile
            // 
            this.btnSelectResourceFile.Location = new System.Drawing.Point(6, 54);
            this.btnSelectResourceFile.Name = "btnSelectResourceFile";
            this.btnSelectResourceFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectResourceFile.TabIndex = 7;
            this.btnSelectResourceFile.Text = "Выбрать";
            this.btnSelectResourceFile.UseVisualStyleBackColor = true;
            this.btnSelectResourceFile.Click += new System.EventHandler(this.BtnSelectResourceFileClick);
            // 
            // tabParsing
            // 
            this.tabParsing.Controls.Add(this.panel5);
            this.tabParsing.Controls.Add(this.panel4);
            this.tabParsing.Controls.Add(this.panel2);
            this.tabParsing.Controls.Add(this.panel1);
            this.tabParsing.Location = new System.Drawing.Point(4, 22);
            this.tabParsing.Name = "tabParsing";
            this.tabParsing.Padding = new System.Windows.Forms.Padding(3);
            this.tabParsing.Size = new System.Drawing.Size(1182, 452);
            this.tabParsing.TabIndex = 1;
            this.tabParsing.Text = "Парсинг";
            this.tabParsing.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.listResultContent);
            this.panel5.Controls.Add(this.listPreview);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(3, 290);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1176, 159);
            this.panel5.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.rbtnFirstParserSaveFormat);
            this.panel4.Controls.Add(this.rbtnSecondParserSaveFormat);
            this.panel4.Controls.Add(this.btnClearListResultContent);
            this.panel4.Controls.Add(this.btnMarge);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 230);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1176, 60);
            this.panel4.TabIndex = 0;
            // 
            // rbtnFirstParserSaveFormat
            // 
            this.rbtnFirstParserSaveFormat.AutoSize = true;
            this.rbtnFirstParserSaveFormat.Checked = true;
            this.rbtnFirstParserSaveFormat.Location = new System.Drawing.Point(242, 12);
            this.rbtnFirstParserSaveFormat.Name = "rbtnFirstParserSaveFormat";
            this.rbtnFirstParserSaveFormat.Size = new System.Drawing.Size(76, 17);
            this.rbtnFirstParserSaveFormat.TabIndex = 16;
            this.rbtnFirstParserSaveFormat.TabStop = true;
            this.rbtnFirstParserSaveFormat.Text = "First parser";
            this.rbtnFirstParserSaveFormat.UseVisualStyleBackColor = true;
            // 
            // rbtnSecondParserSaveFormat
            // 
            this.rbtnSecondParserSaveFormat.AutoSize = true;
            this.rbtnSecondParserSaveFormat.Location = new System.Drawing.Point(242, 35);
            this.rbtnSecondParserSaveFormat.Name = "rbtnSecondParserSaveFormat";
            this.rbtnSecondParserSaveFormat.Size = new System.Drawing.Size(94, 17);
            this.rbtnSecondParserSaveFormat.TabIndex = 15;
            this.rbtnSecondParserSaveFormat.Text = "Second parser";
            this.rbtnSecondParserSaveFormat.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.listErrors);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 146);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1176, 84);
            this.panel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listResourceFormate);
            this.panel1.Controls.Add(this.listOutputFormate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1176, 143);
            this.panel1.TabIndex = 0;
            // 
            // tabSave
            // 
            this.tabSave.Controls.Add(this.btnSaveNewFile);
            this.tabSave.Controls.Add(this.txtNewFileName);
            this.tabSave.Controls.Add(this.txtbxNewFileName);
            this.tabSave.Controls.Add(this.txtOutputFolder);
            this.tabSave.Controls.Add(this.txtSelectedOutputFolder);
            this.tabSave.Controls.Add(this.btnSelectOutputFolder);
            this.tabSave.Location = new System.Drawing.Point(4, 22);
            this.tabSave.Name = "tabSave";
            this.tabSave.Size = new System.Drawing.Size(1182, 452);
            this.tabSave.TabIndex = 2;
            this.tabSave.Text = "Сохранение";
            this.tabSave.UseVisualStyleBackColor = true;
            // 
            // btnSaveNewFile
            // 
            this.btnSaveNewFile.Location = new System.Drawing.Point(21, 188);
            this.btnSaveNewFile.Name = "btnSaveNewFile";
            this.btnSaveNewFile.Size = new System.Drawing.Size(75, 23);
            this.btnSaveNewFile.TabIndex = 5;
            this.btnSaveNewFile.Text = "Сохранить";
            this.btnSaveNewFile.UseVisualStyleBackColor = true;
            this.btnSaveNewFile.Click += new System.EventHandler(this.BtnSaveNewFileClick);
            // 
            // txtNewFileName
            // 
            this.txtNewFileName.AutoSize = true;
            this.txtNewFileName.Location = new System.Drawing.Point(21, 142);
            this.txtNewFileName.Name = "txtNewFileName";
            this.txtNewFileName.Size = new System.Drawing.Size(102, 13);
            this.txtNewFileName.TabIndex = 4;
            this.txtNewFileName.Text = "Имя нового файла";
            // 
            // txtbxNewFileName
            // 
            this.txtbxNewFileName.Location = new System.Drawing.Point(21, 162);
            this.txtbxNewFileName.Name = "txtbxNewFileName";
            this.txtbxNewFileName.Size = new System.Drawing.Size(189, 20);
            this.txtbxNewFileName.TabIndex = 3;
            // 
            // fileDialog
            // 
            this.fileDialog.Filter = "Text Files (.txt)|*.txt";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1190, 478);
            this.Controls.Add(this.tabMain);
            this.Name = "MainForm";
            this.tabMain.ResumeLayout(false);
            this.tabSourseFiles.ResumeLayout(false);
            this.tabSourseFiles.PerformLayout();
            this.tabParsing.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabSave.ResumeLayout(false);
            this.tabSave.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label txtOutputFolder;
        private System.Windows.Forms.FolderBrowserDialog OutputFolderBrowserDialog;
        private System.Windows.Forms.Button btnSelectOutputFolder;
        private System.Windows.Forms.Label txtSelectedOutputFolder;
        private System.Windows.Forms.Label txtFormats;
        private System.Windows.Forms.ComboBox ddlResourceFormat;
        private System.Windows.Forms.ComboBox ddlOutputFormat;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.ListBox listResourceFormate;
        private System.Windows.Forms.ListBox listOutputFormate;
        private System.Windows.Forms.ListBox listErrors;
        private System.Windows.Forms.Button btnMarge;
        private System.Windows.Forms.ListBox listResultContent;
        private System.Windows.Forms.Button btnClearListResultContent;
        private System.Windows.Forms.ListBox listPreview;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabSourseFiles;
        private System.Windows.Forms.TabPage tabParsing;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabSave;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnSelectOutputFile;
        private System.Windows.Forms.Button btnSelectResourceFile;
        private System.Windows.Forms.OpenFileDialog fileDialog;
        private System.Windows.Forms.ListBox listOutputFile;
        private System.Windows.Forms.ListBox listResourceFile;
        private System.Windows.Forms.Button btnSaveNewFile;
        private System.Windows.Forms.Label txtNewFileName;
        private System.Windows.Forms.TextBox txtbxNewFileName;
        private System.Windows.Forms.RadioButton rbtnFirstParserSaveFormat;
        private System.Windows.Forms.RadioButton rbtnSecondParserSaveFormat;
    }
}

