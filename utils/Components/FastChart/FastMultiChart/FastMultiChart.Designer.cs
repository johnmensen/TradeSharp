namespace FastMultiChart
{
    partial class FastMultiChart
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
            this.SuspendLayout();
            // 
            // FastMultiChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "FastMultiChart";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FastMultiChartPaint);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FastMultiChartKeyPress);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FastMultiChartMouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FastMultiChartMouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FastMultiChartMouseUp);
            this.Resize += new System.EventHandler(this.FastMultiChartResize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
