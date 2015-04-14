using TradeSharp.Util.Controls;

namespace Candlechart.Controls
{
    partial class ChartCommentEditDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartCommentEditDialog));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabText = new System.Windows.Forms.TabPage();
            this.tbText = new System.Windows.Forms.RichTextBox();
            this.panelTextDecor = new System.Windows.Forms.Panel();
            this.btnClearColor = new System.Windows.Forms.Button();
            this.imageListGlyph = new System.Windows.Forms.ImageList(this.components);
            this.cbStyleUnderline = new System.Windows.Forms.CheckBox();
            this.cbStyleItalic = new System.Windows.Forms.CheckBox();
            this.cbStyleBold = new System.Windows.Forms.CheckBox();
            this.btnColorPicker = new System.Windows.Forms.Button();
            this.cbTextSize = new System.Windows.Forms.ComboBox();
            this.panelTemplates = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.cbTemplates = new System.Windows.Forms.ComboBox();
            this.tabVisual = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.transparencyControl = new TradeSharp.Util.Controls.TransparencyControl();
            this.tbMagic = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnColorFill = new System.Windows.Forms.Button();
            this.btnColorLine = new System.Windows.Forms.Button();
            this.cbShowFrame = new System.Windows.Forms.CheckBox();
            this.cbShowArrow = new System.Windows.Forms.CheckBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.colorDialogLineFill = new System.Windows.Forms.ColorDialog();
            this.panelBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabText.SuspendLayout();
            this.panelTextDecor.SuspendLayout();
            this.panelTemplates.SuspendLayout();
            this.tabVisual.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Controls.Add(this.btnOk);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 269);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(415, 33);
            this.panelBottom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(106, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "TitleCancel";
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(3, 5);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Tag = "TitleOK";
            this.btnOk.Text = "ОК";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabText);
            this.tabControl.Controls.Add(this.tabVisual);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(415, 269);
            this.tabControl.TabIndex = 1;
            // 
            // tabText
            // 
            this.tabText.Controls.Add(this.tbText);
            this.tabText.Controls.Add(this.panelTextDecor);
            this.tabText.Controls.Add(this.panelTemplates);
            this.tabText.Location = new System.Drawing.Point(4, 22);
            this.tabText.Name = "tabText";
            this.tabText.Padding = new System.Windows.Forms.Padding(3);
            this.tabText.Size = new System.Drawing.Size(407, 243);
            this.tabText.TabIndex = 0;
            this.tabText.Tag = "TitleText";
            this.tabText.Text = "Текст";
            this.tabText.UseVisualStyleBackColor = true;
            // 
            // tbText
            // 
            this.tbText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbText.Location = new System.Drawing.Point(3, 35);
            this.tbText.Name = "tbText";
            this.tbText.Size = new System.Drawing.Size(401, 164);
            this.tbText.TabIndex = 2;
            this.tbText.Text = "";
            this.tbText.SelectionChanged += new System.EventHandler(this.TbTextSelectionChanged);
            // 
            // panelTextDecor
            // 
            this.panelTextDecor.Controls.Add(this.btnClearColor);
            this.panelTextDecor.Controls.Add(this.cbStyleUnderline);
            this.panelTextDecor.Controls.Add(this.cbStyleItalic);
            this.panelTextDecor.Controls.Add(this.cbStyleBold);
            this.panelTextDecor.Controls.Add(this.btnColorPicker);
            this.panelTextDecor.Controls.Add(this.cbTextSize);
            this.panelTextDecor.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTextDecor.Location = new System.Drawing.Point(3, 3);
            this.panelTextDecor.Name = "panelTextDecor";
            this.panelTextDecor.Size = new System.Drawing.Size(401, 32);
            this.panelTextDecor.TabIndex = 1;
            // 
            // btnClearColor
            // 
            this.btnClearColor.ImageIndex = 0;
            this.btnClearColor.ImageList = this.imageListGlyph;
            this.btnClearColor.Location = new System.Drawing.Point(33, 4);
            this.btnClearColor.Name = "btnClearColor";
            this.btnClearColor.Size = new System.Drawing.Size(17, 23);
            this.btnClearColor.TabIndex = 6;
            this.btnClearColor.UseVisualStyleBackColor = true;
            this.btnClearColor.Click += new System.EventHandler(this.BtnClearColorClick);
            // 
            // imageListGlyph
            // 
            this.imageListGlyph.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListGlyph.ImageStream")));
            this.imageListGlyph.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListGlyph.Images.SetKeyName(0, "cross_mini.png");
            // 
            // cbStyleUnderline
            // 
            this.cbStyleUnderline.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbStyleUnderline.AutoSize = true;
            this.cbStyleUnderline.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbStyleUnderline.Location = new System.Drawing.Point(117, 4);
            this.cbStyleUnderline.Name = "cbStyleUnderline";
            this.cbStyleUnderline.Size = new System.Drawing.Size(25, 23);
            this.cbStyleUnderline.TabIndex = 5;
            this.cbStyleUnderline.Text = "U";
            this.cbStyleUnderline.UseVisualStyleBackColor = true;
            this.cbStyleUnderline.CheckedChanged += new System.EventHandler(this.CbStyleBoldCheckedChanged);
            // 
            // cbStyleItalic
            // 
            this.cbStyleItalic.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbStyleItalic.AutoSize = true;
            this.cbStyleItalic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbStyleItalic.Location = new System.Drawing.Point(98, 4);
            this.cbStyleItalic.Name = "cbStyleItalic";
            this.cbStyleItalic.Size = new System.Drawing.Size(19, 23);
            this.cbStyleItalic.TabIndex = 4;
            this.cbStyleItalic.Text = "i";
            this.cbStyleItalic.UseVisualStyleBackColor = true;
            this.cbStyleItalic.CheckedChanged += new System.EventHandler(this.CbStyleBoldCheckedChanged);
            // 
            // cbStyleBold
            // 
            this.cbStyleBold.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbStyleBold.AutoSize = true;
            this.cbStyleBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbStyleBold.Location = new System.Drawing.Point(73, 4);
            this.cbStyleBold.Name = "cbStyleBold";
            this.cbStyleBold.Size = new System.Drawing.Size(25, 23);
            this.cbStyleBold.TabIndex = 3;
            this.cbStyleBold.Text = "B";
            this.cbStyleBold.UseVisualStyleBackColor = true;
            this.cbStyleBold.CheckedChanged += new System.EventHandler(this.CbStyleBoldCheckedChanged);
            // 
            // btnColorPicker
            // 
            this.btnColorPicker.Location = new System.Drawing.Point(4, 4);
            this.btnColorPicker.Name = "btnColorPicker";
            this.btnColorPicker.Size = new System.Drawing.Size(30, 23);
            this.btnColorPicker.TabIndex = 2;
            this.btnColorPicker.UseVisualStyleBackColor = true;
            this.btnColorPicker.Click += new System.EventHandler(this.BtnColorPickerClick);
            // 
            // cbTextSize
            // 
            this.cbTextSize.FormattingEnabled = true;
            this.cbTextSize.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "18",
            "22",
            "24",
            "36",
            "48"});
            this.cbTextSize.Location = new System.Drawing.Point(199, 5);
            this.cbTextSize.Name = "cbTextSize";
            this.cbTextSize.Size = new System.Drawing.Size(46, 21);
            this.cbTextSize.TabIndex = 0;
            this.cbTextSize.Text = "9";
            this.cbTextSize.Visible = false;
            // 
            // panelTemplates
            // 
            this.panelTemplates.Controls.Add(this.label1);
            this.panelTemplates.Controls.Add(this.cbTemplates);
            this.panelTemplates.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTemplates.Location = new System.Drawing.Point(3, 199);
            this.panelTemplates.Name = "panelTemplates";
            this.panelTemplates.Size = new System.Drawing.Size(401, 41);
            this.panelTemplates.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Tag = "TitleTemplate";
            this.label1.Text = "Шаблон";
            // 
            // cbTemplates
            // 
            this.cbTemplates.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cbTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTemplates.FormattingEnabled = true;
            this.cbTemplates.Location = new System.Drawing.Point(0, 20);
            this.cbTemplates.Name = "cbTemplates";
            this.cbTemplates.Size = new System.Drawing.Size(401, 21);
            this.cbTemplates.TabIndex = 0;
            this.cbTemplates.SelectedIndexChanged += new System.EventHandler(this.CbTemplatesSelectedIndexChanged);
            // 
            // tabVisual
            // 
            this.tabVisual.Controls.Add(this.label3);
            this.tabVisual.Controls.Add(this.transparencyControl);
            this.tabVisual.Controls.Add(this.tbMagic);
            this.tabVisual.Controls.Add(this.label2);
            this.tabVisual.Controls.Add(this.btnColorFill);
            this.tabVisual.Controls.Add(this.btnColorLine);
            this.tabVisual.Controls.Add(this.cbShowFrame);
            this.tabVisual.Controls.Add(this.cbShowArrow);
            this.tabVisual.Location = new System.Drawing.Point(4, 22);
            this.tabVisual.Name = "tabVisual";
            this.tabVisual.Padding = new System.Windows.Forms.Padding(3);
            this.tabVisual.Size = new System.Drawing.Size(407, 243);
            this.tabVisual.TabIndex = 1;
            this.tabVisual.Tag = "TitleOthers";
            this.tabVisual.Text = "Прочие";
            this.tabVisual.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(142, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 7;
            this.label3.Tag = "TitleOpaquenessSmall";
            this.label3.Text = "непрозрачность";
            // 
            // transparencyControl
            // 
            this.transparencyControl.Location = new System.Drawing.Point(139, 52);
            this.transparencyControl.Name = "transparencyControl";
            this.transparencyControl.Size = new System.Drawing.Size(94, 52);
            this.transparencyControl.TabIndex = 6;
            this.transparencyControl.Transparency = 255;
            // 
            // tbMagic
            // 
            this.tbMagic.Location = new System.Drawing.Point(6, 110);
            this.tbMagic.Name = "tbMagic";
            this.tbMagic.Size = new System.Drawing.Size(68, 20);
            this.tbMagic.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(80, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "magic";
            // 
            // btnColorFill
            // 
            this.btnColorFill.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnColorFill.Location = new System.Drawing.Point(8, 81);
            this.btnColorFill.Name = "btnColorFill";
            this.btnColorFill.Size = new System.Drawing.Size(116, 23);
            this.btnColorFill.TabIndex = 3;
            this.btnColorFill.Tag = "TitleFillingColor";
            this.btnColorFill.Text = "цвет заливки";
            this.btnColorFill.UseVisualStyleBackColor = true;
            this.btnColorFill.Click += new System.EventHandler(this.BtnColorLineClick);
            // 
            // btnColorLine
            // 
            this.btnColorLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnColorLine.Location = new System.Drawing.Point(8, 52);
            this.btnColorLine.Name = "btnColorLine";
            this.btnColorLine.Size = new System.Drawing.Size(116, 23);
            this.btnColorLine.TabIndex = 2;
            this.btnColorLine.Tag = "TitleStrokeColor";
            this.btnColorLine.Text = "цвет обводки";
            this.btnColorLine.UseVisualStyleBackColor = true;
            this.btnColorLine.Click += new System.EventHandler(this.BtnColorLineClick);
            // 
            // cbShowFrame
            // 
            this.cbShowFrame.AutoSize = true;
            this.cbShowFrame.Location = new System.Drawing.Point(8, 29);
            this.cbShowFrame.Name = "cbShowFrame";
            this.cbShowFrame.Size = new System.Drawing.Size(107, 17);
            this.cbShowFrame.TabIndex = 1;
            this.cbShowFrame.Tag = "TitleDrawFrameSmall";
            this.cbShowFrame.Text = "рисовать рамку";
            this.cbShowFrame.UseVisualStyleBackColor = true;
            // 
            // cbShowArrow
            // 
            this.cbShowArrow.AutoSize = true;
            this.cbShowArrow.Location = new System.Drawing.Point(8, 6);
            this.cbShowArrow.Name = "cbShowArrow";
            this.cbShowArrow.Size = new System.Drawing.Size(116, 17);
            this.cbShowArrow.TabIndex = 0;
            this.cbShowArrow.Tag = "TitleDrawArrowSmall";
            this.cbShowArrow.Text = "рисовать стрелку";
            this.cbShowArrow.UseVisualStyleBackColor = true;
            // 
            // ChartCommentEditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 302);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelBottom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChartCommentEditDialog";
            this.Tag = "TitleComment";
            this.Text = "Комментарий";
            this.panelBottom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabText.ResumeLayout(false);
            this.panelTextDecor.ResumeLayout(false);
            this.panelTextDecor.PerformLayout();
            this.panelTemplates.ResumeLayout(false);
            this.panelTemplates.PerformLayout();
            this.tabVisual.ResumeLayout(false);
            this.tabVisual.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabText;
        private System.Windows.Forms.RichTextBox tbText;
        private System.Windows.Forms.Panel panelTextDecor;
        private System.Windows.Forms.ComboBox cbTextSize;
        private System.Windows.Forms.Panel panelTemplates;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbTemplates;
        private System.Windows.Forms.TabPage tabVisual;
        private System.Windows.Forms.Button btnColorPicker;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button btnColorLine;
        private System.Windows.Forms.CheckBox cbShowFrame;
        private System.Windows.Forms.CheckBox cbShowArrow;
        private System.Windows.Forms.Button btnColorFill;
        private System.Windows.Forms.ColorDialog colorDialogLineFill;
        private System.Windows.Forms.TextBox tbMagic;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbStyleUnderline;
        private System.Windows.Forms.CheckBox cbStyleItalic;
        private System.Windows.Forms.CheckBox cbStyleBold;
        private System.Windows.Forms.Button btnClearColor;
        private System.Windows.Forms.ImageList imageListGlyph;
        private System.Windows.Forms.Label label3;
        private TransparencyControl transparencyControl;
    }
}