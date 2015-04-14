using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TradeSharp.UI.Util.Control
{
    public partial class StandByControl : UserControl
    {
        [DisplayName("IsShown")]
        [Category("StandBy")]
        public bool IsShown
        {
            get { return pictureBox.Enabled; }
            set 
            { 
                pictureBox.Enabled = value;
                if (transparentForm == null)
                    return;
                if(value)
                    transparentForm.TransparencyKey = value ? pictureBox.BackColor : Color.Empty;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        private Form transparentForm;
        [Browsable(false)]
        public Form TransparentForm
        {
            get { return transparentForm; }
            set
            {
                if (transparentForm != null)
                    transparentForm.TransparencyKey = Color.Empty;
                transparentForm = value;
                pictureBox.BackColor = value != null ? Color.Magenta : SystemColors.Control;
                if (IsShown && value != null)
                    transparentForm.TransparencyKey = pictureBox.BackColor;
            }
        }

        public StandByControl()
        {
            InitializeComponent();
            Font = new Font(Font, FontStyle.Bold);
        }

        private void PictureBoxPaint(object sender, PaintEventArgs e)
        {
            // нарисовать метку
            using (var brush = new SolidBrush(ForeColor))
            {
                var textSize = e.Graphics.MeasureString(Text, Font).ToSize();
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                e.Graphics.DrawString(Text, Font, brush, pictureBox.Size.Width / 2,
                                      pictureBox.Size.Height / 2 + pictureBox.Image.Size.Height / 2 +
                                      textSize.Height / 2,
                                      new StringFormat
                                      {
                                          Alignment = StringAlignment.Center,
                                          LineAlignment = StringAlignment.Center
                                      });
            }
        }

    }
}
