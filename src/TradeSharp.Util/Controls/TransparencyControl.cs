using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TradeSharp.Util.Controls
{
    public partial class TransparencyControl : UserControl
    {
        private static readonly PointF[] points
            = new []
                {
                    new PointF(1, 1), new PointF(13, 1), 
                    new PointF(27, 15), new PointF(31, 16), 
                    new PointF(17, 16), new PointF(3, 2)
                };

        public TransparencyControl()
        {
            InitializeComponent();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (pictureBox != null)
                if (pictureBox.Image != null)
                    pictureBox.Image.Dispose();
        }

        [Browsable(true)]
        [DisplayName("Transparency")]
        [Category("Main")]
        public int Transparency
        {
            get { return trackBar.Value; }
            set
            {
                if (value < 0 || value > 255)
                    return;
                tbValue.Text = value.ToString();
                trackBar.Value = value;
                UpdatePicture();
            }
        }

        private void TrackBarScroll(object sender, EventArgs e)
        {
            tbValue.Text = trackBar.Value.ToString();
            UpdatePicture();
        }

        private void UpdatePicture()
        {
            var w = pictureBox.Width;
            var h = pictureBox.Height;
            var w2 = w/2;
            var h2 = h/2;

            using (var g = Graphics.FromImage(pictureBox.Image))
            using (var p = new Pen(SystemColors.ControlText))
            using (var brClear = new SolidBrush(SystemColors.ButtonFace))
            using (var brWhite = new SolidBrush(Color.White))
            using (var brDark = new SolidBrush(Color.Gray))
            using (var br = new SolidBrush(Color.FromArgb(trackBar.Value, SystemColors.ControlText)))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRectangle(brClear, 0, 0, w2 - 1, h2 - 1);
                g.FillRectangle(brWhite, w2, 0, w2, h2 - 1);
                g.FillRectangle(brWhite, 0, h2, w2 - 1, h2);
                g.FillRectangle(brDark, w2, h2, w2, h2);
                
                g.FillPolygon(br, points);
                g.DrawPolygon(p, points);
            }
            pictureBox.Invalidate();
        }

        private void TbValueTextChanged(object sender, EventArgs e)
        {
            var val = tbValue.Text.ToIntSafe();
            if (val == null || val < 0 || val > 255) return;

            trackBar.Scroll -= TrackBarScroll;
            trackBar.Value = val.Value;
            trackBar.Scroll += TrackBarScroll;
            UpdatePicture();
        }
    }
}
