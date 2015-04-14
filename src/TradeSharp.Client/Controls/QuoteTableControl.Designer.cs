using System;
using System.Windows.Forms;

namespace TradeSharp.Client.Controls
{
    partial class QuoteTableControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuoteTableControl));
            this.panelTop = new System.Windows.Forms.Panel();
            this.trackSize = new System.Windows.Forms.TrackBar();
            this.btnChooseTickers = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.btnClose = new System.Windows.Forms.Button();
            this.btnPinDown = new System.Windows.Forms.Button();
            this.panelTable = new System.Windows.Forms.Panel();
            this.imageListCloseButton = new System.Windows.Forms.ImageList(this.components);
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackSize)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.trackSize);
            this.panelTop.Controls.Add(this.btnChooseTickers);
            this.panelTop.Controls.Add(this.btnClose);
            this.panelTop.Controls.Add(this.btnPinDown);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(242, 31);
            this.panelTop.TabIndex = 0;
            // 
            // trackSize
            // 
            this.trackSize.LargeChange = 15;
            this.trackSize.Location = new System.Drawing.Point(83, 2);
            this.trackSize.Maximum = 200;
            this.trackSize.Name = "trackSize";
            this.trackSize.Size = new System.Drawing.Size(141, 45);
            this.trackSize.TabIndex = 3;
            this.trackSize.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackSize.Scroll += new System.EventHandler(this.TrackSizeScroll);
            // 
            // btnChooseTickers
            // 
            this.btnChooseTickers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChooseTickers.ImageIndex = 3;
            this.btnChooseTickers.ImageList = this.imageList;
            this.btnChooseTickers.Location = new System.Drawing.Point(56, 6);
            this.btnChooseTickers.Name = "btnChooseTickers";
            this.btnChooseTickers.Size = new System.Drawing.Size(16, 16);
            this.btnChooseTickers.TabIndex = 2;
            this.btnChooseTickers.UseVisualStyleBackColor = true;
            this.btnChooseTickers.Click += new System.EventHandler(this.BtnChooseTickersClick);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Yellow;
            this.imageList.Images.SetKeyName(0, "ico12 close.bmp");
            this.imageList.Images.SetKeyName(1, "ico12 pindown.bmp");
            this.imageList.Images.SetKeyName(2, "ico12 pinup.bmp");
            this.imageList.Images.SetKeyName(3, "11.bmp");
            // 
            // btnClose
            // 
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ImageIndex = 0;
            this.btnClose.ImageList = this.imageList;
            this.btnClose.Location = new System.Drawing.Point(25, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(16, 16);
            this.btnClose.TabIndex = 1;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // btnPinDown
            // 
            this.btnPinDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPinDown.ImageIndex = 2;
            this.btnPinDown.ImageList = this.imageList;
            this.btnPinDown.Location = new System.Drawing.Point(3, 6);
            this.btnPinDown.Name = "btnPinDown";
            this.btnPinDown.Size = new System.Drawing.Size(16, 16);
            this.btnPinDown.TabIndex = 0;
            this.btnPinDown.UseVisualStyleBackColor = true;
            this.btnPinDown.Click += new System.EventHandler(this.BtnPinUpClick);
            // 
            // panelTable
            // 
            this.panelTable.AllowDrop = true;
            this.panelTable.AutoScroll = true;
            this.panelTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTable.Location = new System.Drawing.Point(0, 31);
            this.panelTable.Name = "panelTable";
            this.panelTable.Size = new System.Drawing.Size(242, 191);
            this.panelTable.TabIndex = 1;
            this.panelTable.Resize += new System.EventHandler(this.PanelTableResize);
            // 
            // imageListCloseButton
            // 
            this.imageListCloseButton.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListCloseButton.ImageStream")));
            this.imageListCloseButton.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // QuoteTableControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelTable);
            this.Controls.Add(this.panelTop);
            this.Name = "QuoteTableControl";
            this.Size = new System.Drawing.Size(242, 222);
            this.Load += new System.EventHandler(this.QuoteTableControlLoad);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelTable;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btnPinDown;
        private System.Windows.Forms.Button btnChooseTickers;
        private System.Windows.Forms.TrackBar trackSize;
        private ImageList imageListCloseButton;


    }
}
