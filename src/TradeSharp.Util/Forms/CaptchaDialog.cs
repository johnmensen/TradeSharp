using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class CaptchaDialog : Form
    {
        private readonly Random random = new Random();

        public bool TestPassed { get; private set; }

        private readonly bool ignoreCase;
        
        private readonly int textLen;

        private readonly bool digitsOnly;

        private string codeText;
        
        public CaptchaDialog()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public CaptchaDialog(string prompt, bool ignoreCase, bool digitsOnly, int textLen) : this()
        {
            Text = prompt;
            this.ignoreCase = ignoreCase;
            this.textLen = textLen;
            this.digitsOnly = digitsOnly;
            
            MakeCode();
        }

        private void MakeCode()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < textLen; i++)
            {
                if (digitsOnly) sb.Append((char) ('0' + random.Next(10)));
                else
                {
                    var isDigit = random.Next(10) > 6;
                    if (isDigit) sb.Append((char)('0' + random.Next(10)));
                    else
                    {
                        var letter = ((char) ('a' + random.Next('z' - 'a')));
                        if (!ignoreCase)
                            if (random.Next(10) > 4) letter = char.ToUpper(letter);
                        sb.Append(letter);
                    }
                }
            }
            codeText = sb.ToString();
        }

        private void CaptchaDialogLoad(object sender, EventArgs e)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            GenerateImage(pictureBox.Width, pictureBox.Height, codeText, Font.FontFamily.Name,
                          (Bitmap)pictureBox.Image);
        }

        private void GenerateImage(int width, int height,
            string text, string fontFamily, Bitmap bitmap)
        {
            // Create a new 32-bit bitmap image.
            //Bitmap bitmap = new Bitmap(
            //  width,
            //  height,
            //  PixelFormat.Format32bppArgb);

            // Create a graphics object for drawing.
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, width, height);

                // Fill in the background.
                var hatchBrush = new HatchBrush(
                    HatchStyle.SmallConfetti,
                    Color.LightGray,
                    Color.White);
                g.FillRectangle(hatchBrush, rect);

                // Set up the text font.
                SizeF size;
                float fontSize = rect.Height + 1;
                Font font;
                // Adjust the font size until the text fits within the image.
                do
                {
                    fontSize--;
                    font = new Font(
                        fontFamily,
                        fontSize,
                        FontStyle.Bold);
                    size = g.MeasureString(text, font);
                } while (size.Width > rect.Width);

                // Set up the text format.
                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                // Create a path using the text and warp it randomly.
                var path = new GraphicsPath();
                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, rect, format);
                const float v = 3.2F;// 4F;
                PointF[] points =
                    {
                        new PointF(random.Next(rect.Width)/v, random.Next(rect.Height)/v),
                        new PointF(rect.Width - random.Next(rect.Width)/v, random.Next(rect.Height)/v),
                        new PointF(random.Next(rect.Width)/v, rect.Height - random.Next(rect.Height)/v),
                        new PointF(rect.Width - random.Next(rect.Width)/v, rect.Height - random.Next(rect.Height)/v)
                    };
                var matrix = new Matrix();
                matrix.Translate(0F, 0F);
                path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

                // Draw the text
                hatchBrush.Dispose();
                hatchBrush = new HatchBrush(
                    HatchStyle.LargeConfetti,
                    Color.LightGray,
                    Color.DarkGray);
                g.FillPath(hatchBrush, path);

                // Add some random noise.
                int m = Math.Max(rect.Width, rect.Height);
                for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
                {
                    int x = random.Next(rect.Width);
                    int y = random.Next(rect.Height);
                    int w = random.Next(m / 50);
                    int h = random.Next(m / 50);
                    g.FillEllipse(hatchBrush, x, y, w, h);
                }

                // Clean up
                font.Dispose();
                hatchBrush.Dispose();
            }
            //return bitmap;
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            var compType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (tbCode.Text.Equals(codeText, compType))
            {
                TestPassed = true;
                DialogResult = DialogResult.OK;
                return;
            }
            tbCode.Text = "";
        }

        private void BtnUpdateCodeClick(object sender, EventArgs e)
        {
            MakeCode();
            var oldImg = pictureBox.Image;
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            GenerateImage(pictureBox.Width, pictureBox.Height, codeText, Font.FontFamily.Name,
                          (Bitmap)pictureBox.Image);
            if (oldImg != null)
                oldImg.Dispose();
        }
    }
}
