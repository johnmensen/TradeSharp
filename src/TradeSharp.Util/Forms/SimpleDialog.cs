using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class SimpleDialog : Form
    {
        public string InputValue
        {
            get { return inputRichText ? richTextBox.Text : inputLabel.Text; }
            set
            {
                if (inputRichText)
                    richTextBox.Text = value;
                else
                    inputLabel.Text = value;
            }
        }

        private bool inputRichText;
        public bool InputRichText
        {
            get { return inputRichText; }
            set
            {
                inputRichText = value;
                richTextBox.Visible = inputRichText;
                inputLabel.Visible = !inputRichText;
                if (inputRichText)
                {
                    richTextBox.Text = inputLabel.Text;
                    Height += 100;
                }
                else
                {
                    inputLabel.Text = richTextBox.Text;
                    Height -= 100;
                }
            }
        }

        public SimpleDialog()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }
        
        public SimpleDialog(string caption, string label, bool enableCancel, string inputText = "") : this()
        {
            Text = caption;
            commentLabel.Text = label;
            if (!enableCancel)
                cancelButton.Visible = false;
            InputValue = inputText;
        }
    }
}
