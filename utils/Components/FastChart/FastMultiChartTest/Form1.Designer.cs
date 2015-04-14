namespace FastMultiChartTest
{
    partial class Form1
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
            this.fastMultiChart1 = new FastMultiChart.FastMultiChart();
            this.SuspendLayout();
            // 
            // fastMultiChart1
            // 
            this.fastMultiChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastMultiChart1.Location = new System.Drawing.Point(0, 0);
            this.fastMultiChart1.MarginXLeft = 10;
            this.fastMultiChart1.MarginXRight = 30;
            this.fastMultiChart1.MarginYBottom = 10;
            this.fastMultiChart1.MarginYTop = 10;
            this.fastMultiChart1.Name = "fastMultiChart1";
            this.fastMultiChart1.RenderPolygons = true;
            this.fastMultiChart1.ScaleDivisionXMaxPixel = -1;
            this.fastMultiChart1.ScaleDivisionXMinPixel = 70;
            this.fastMultiChart1.ScaleDivisionYMaxPixel = 60;
            this.fastMultiChart1.ScaleDivisionYMinPixel = 20;
            this.fastMultiChart1.ScrollBarHeight = 80;
            this.fastMultiChart1.ShowHints = true;
            this.fastMultiChart1.ShowScaleDivisionXLabel = true;
            this.fastMultiChart1.ShowScaleDivisionYLabel = true;
            this.fastMultiChart1.Size = new System.Drawing.Size(721, 273);
            this.fastMultiChart1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 273);
            this.Controls.Add(this.fastMultiChart1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private FastMultiChart.FastMultiChart fastMultiChart1;
    }
}

