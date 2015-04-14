using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace TestMTTradeServer
{
    public partial class SettingsForm : XtraForm
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        public int PortListen
        {
            get
            {
                return int.Parse(tbPort.Text);
            }
            set
            {
                tbPort.Text = value.ToString();
            }
        }

        public int PortSend
        {
            get
            {
                return int.Parse(tbPortSend.Text);
            }
            set
            {
                tbPortSend.Text = value.ToString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}