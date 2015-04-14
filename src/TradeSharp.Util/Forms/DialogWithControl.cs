using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class DialogWithControl : Form
    {
        private Control mainControl;
        public Control MainControl
        {
            get { return mainControl; }
            set
            {
                Controls.Remove(mainControl);
                mainControl = value;
                mainControl.Dock = DockStyle.Fill;
                Controls.Add(value);
            }
        }

        public DialogWithControl()
        {
            InitializeComponent();
        }
    }
}
