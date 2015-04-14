namespace TradeSharp.Client.Controls.Bookmark
{
    partial class BookmarkStripControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookmarkStripControl));
            this.btnMore = new System.Windows.Forms.Button();
            this.lstGlyph = new System.Windows.Forms.ImageList(this.components);
            this.timerInterface = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnMore
            // 
            this.btnMore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMore.ImageIndex = 0;
            this.btnMore.ImageList = this.lstGlyph;
            this.btnMore.Location = new System.Drawing.Point(0, 0);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(28, 21);
            this.btnMore.TabIndex = 0;
            this.btnMore.UseVisualStyleBackColor = true;
            this.btnMore.Click += new System.EventHandler(this.BtnMoreClick);
            // 
            // lstGlyph
            // 
            this.lstGlyph.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lstGlyph.ImageStream")));
            this.lstGlyph.TransparentColor = System.Drawing.Color.Transparent;
            this.lstGlyph.Images.SetKeyName(0, "ico_up_lite.png");
            // 
            // timerInterface
            // 
            this.timerInterface.Enabled = true;
            this.timerInterface.Interval = 500;
            this.timerInterface.Tick += new System.EventHandler(this.TimerInterfaceTick);
            // 
            // BookmarkStripControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnMore);
            this.Name = "BookmarkStripControl";
            this.Size = new System.Drawing.Size(979, 22);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMore;
        private System.Windows.Forms.ImageList lstGlyph;
        private System.Windows.Forms.Timer timerInterface;
    }
}
