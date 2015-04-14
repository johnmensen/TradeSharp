using System;
using System.Net.Sockets;
using System.Text;
using DevExpress.XtraEditors;

namespace TestMTTradeServer
{
    public partial class UdpTestForm : XtraForm
    {
        public UdpTestForm()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var parts = tbAddress.Text.Split(':');
            using (var client = new UdpClient())
            {
                var bytes = Encoding.ASCII.GetBytes(tbMessage.Text);
                client.Send(bytes, bytes.Length, parts[0], int.Parse(parts[1]));
            }
        }
    }
}