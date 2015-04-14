using System;
using System.Windows.Forms;

namespace TestApp
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new MainForm().Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new MsGridForm().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new ManualSetUpGridForm().Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new TestForm1().Show();
        }
    }
}
