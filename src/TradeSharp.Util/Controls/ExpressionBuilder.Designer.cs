using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Util.Controls
{
    partial class ExpressionBuilder
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionBuilder));
            this.filterPanel = new System.Windows.Forms.Panel();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // filterPanel
            // 
            this.filterPanel.AutoScroll = true;
            this.filterPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterPanel.Location = new System.Drawing.Point(0, 0);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(325, 298);
            this.filterPanel.TabIndex = 3;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Yellow;
            this.imageList.Images.SetKeyName(0, "ico12 close.bmp");
            // 
            // ExpressionBuilder
            // 
            this.Controls.Add(this.filterPanel);
            this.Name = "ExpressionBuilder";
            this.Size = new System.Drawing.Size(325, 298);
            this.Resize += new System.EventHandler(this.ExpressionBuilderResize);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel filterPanel;
        private ImageList imageList;
    }
}
