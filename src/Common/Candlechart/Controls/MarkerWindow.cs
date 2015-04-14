using System.Windows.Forms;

namespace Candlechart.Controls
{
    public partial class MarkerWindow : Form
    {
        public string Text
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        public bool IsDeleteBtnPressed { get; private set; }

        public MarkerWindow()
        {
            IsDeleteBtnPressed = false;
            InitializeComponent();
        }

        private void BtnDeleteClick(object sender, System.EventArgs e)
        {
            IsDeleteBtnPressed = false;
            Close();
        }
    }
}
