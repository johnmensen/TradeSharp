using System;
using System.Windows.Forms;

namespace Candlechart.Chart
{
    public partial class WaitForm : Form
    {
        public WaitForm()
        {
            InitializeComponent();
        }

        public string OperationTitle
        {
            set
            {
                lblOperation.Text = value;
            }
        }

        private void WaitForm_Load(object sender, EventArgs e)
        {
            progressBar.MarqueeAnimationSpeed = 500;            
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            if (abortPressed != null)
                abortPressed(sender, e);
            Close();
        }

        private event EventHandler abortPressed;
        public event EventHandler AbortPressed
        {
            add { abortPressed += value; }
            remove { abortPressed -= value; }
        }
    }
}