using System;
using System.Windows.Forms;
using TestApp.BL;

namespace TestApp
{
    public partial class MsGridForm : Form
    {
        public MsGridForm()
        {
            InitializeComponent();
        }

        private void MsGridFormLoad(object sender, EventArgs e)
        {
            var pers = Person.MakePeople(500);
            grid.DataSource = pers;
        }

        private void BtnSpeedTestClick(object sender, EventArgs e)
        {
            var tmStart = DateTime.Now;
            for (var i = 0; i < 100; i++)
            {
                grid.FirstDisplayedScrollingRowIndex = 100;
                grid.Invalidate();
                grid.FirstDisplayedScrollingRowIndex = 0;
                grid.Invalidate();
            }
            var deltaMils = (int)(DateTime.Now - tmStart).TotalMilliseconds;
            MessageBox.Show("Invalidating lasted " + deltaMils + " mils.");
        }
    }
}
