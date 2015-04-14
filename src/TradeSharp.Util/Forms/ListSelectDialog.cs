using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class ListSelectDialog : Form
    {
        private object selectedItem;
        public object SelectedItem
        {
            get { return selectedItem; }
        }

        public ListSelectDialog()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public void Initialize(IEnumerable<object> items, string text = "", string caption = "")
        {
            if(!string.IsNullOrEmpty(caption))
                Text = caption;
            if (!string.IsNullOrEmpty(text))
                label.Text = text;
            else
                label.Hide();
            listBox.Items.AddRange(items.ToArray());
        }

        private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            okButton.Enabled = false;
            if (listBox.SelectedIndex == -1)
                return;
            selectedItem = listBox.SelectedItem;
            okButton.Enabled = true;
        }

        private void ListBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox.SelectedIndex == -1)
                return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
