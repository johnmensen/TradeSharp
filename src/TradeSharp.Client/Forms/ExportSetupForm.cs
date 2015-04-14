using System;
using System.Text;
using System.Windows.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class ExportSetupForm : Form
    {
        public string DateTimeFormat
        {
            get { return tbTimeFormat.Text; }
        }

        public char ColumnSeparator
        {
            get
            {
                return cbColumnSeparator.Text == ";"
                           ? ';' : cbColumnSeparator.Text == "Пробел"
                                 ? ' ' : cbColumnSeparator.Text == "," ? ',' : (char) 9;
            }
        }

        public char FloatSeparator
        {
            get { return cbFloatPoint.Text == "Точка" ? '.' : ','; }
        }

        public Encoding ExportEncoding
        {
            get { return (Encoding) cbEncoding.SelectedItem; }
        }

        public ExportSetupForm()
        {
            InitializeComponent();
        }

        private void ExportSetupForm_Load(object sender, EventArgs e)
        {
            cbEncoding.DataSource = new [] {Encoding.ASCII, Encoding.Unicode, Encoding.UTF8, Encoding.UTF32};
            cbEncoding.SelectedIndex = 0;
        }
    }
}
