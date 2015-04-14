using System;
using System.Windows.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class PropertiesDlg : Form
    {
        public object[] objectsList;
        
        public PropertiesDlg()
        {
            InitializeComponent();
        }
        
        public PropertiesDlg(object[] objects, string title)
        {            
            InitializeComponent();
            objectsList = objects;
            Text = title;
        }

        public PropertiesDlg(object obj, string title)
        {
            InitializeComponent();
            objectsList = new [] { obj };
            Text = title;
        }

        private void PropertiesDlgLoad(object sender, EventArgs e)
        {
            grid.SelectedObjects = objectsList;            
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
