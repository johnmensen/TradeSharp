namespace ChartTest
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.fastChart1 = new FastChart.Chart.FastChart();
            this.SuspendLayout();
            // 
            // fastChart1
            //             
            this.fastChart1.BackgroundImageCoords = "5;5";            
            this.fastChart1.ClGradEnd = System.Drawing.Color.Snow;
            this.fastChart1.ClGradStart = System.Drawing.Color.Silver;
            this.fastChart1.CurrentTheme = FastChart.Chart.FastThemeName.Нет;
            this.fastChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fastChart1.DrawLegendMargin = false;
            this.fastChart1.DrawMargin = true;
            this.fastChart1.DrawShadow = false;
            this.fastChart1.GradAngle = 90F;
            this.fastChart1.LegendBackground = System.Drawing.Color.White;
            this.fastChart1.LegendInnerPadding = 10;
            this.fastChart1.LegendMarginColor = System.Drawing.Color.Black;
            this.fastChart1.LegendMarginWidth = 1F;
            this.fastChart1.LegendPadding = 15;
            this.fastChart1.LegendPlacement = FastChart.Chart.FastChart.ChartLegendPlacement.Вверху;
            this.fastChart1.LegendShadow = System.Drawing.Color.DimGray;
            this.fastChart1.LengthMinWidth = 100;
            this.fastChart1.Location = new System.Drawing.Point(0, 0);
            this.fastChart1.Name = "fastChart1";
            this.fastChart1.PenMarginColor = System.Drawing.Color.Black;
            this.fastChart1.PenMarginWidth = 1F;
            this.fastChart1.ShowLegend = false;
            this.fastChart1.Size = new System.Drawing.Size(484, 455);
            this.fastChart1.TabIndex = 0;
            this.fastChart1.UseGradient = false;            
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 455);
            this.Controls.Add(this.fastChart1);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.fastChart1.DoubleClick += new System.EventHandler(this.fastChart1_DoubleClick);
            this.ResumeLayout(false);

        }

        #endregion

        private FastChart.Chart.FastChart fastChart1;

        //private FastChart.Chart.FastChart fastChart1;
    }
}