using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class DropDownDialog : Form
    {
        public DropDownDialog()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public DropDownDialog(string title, List<object> items, bool isListStyle) : this()
        {
            Text = title;
            if (isListStyle)
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (var item in items)            
                comboBox.Items.Add(item);
            if (comboBox.Items.Count > 0) comboBox.SelectedIndex = 0;
        }

        public object SelectedItem
        {
            get
            {
                return comboBox.SelectedIndex >= 0 ? comboBox.SelectedItem : null;
            }
        }

        public string SelectedText
        {
            get
            {
                return comboBox.Text;
            }
            set { comboBox.Text = value; }
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
