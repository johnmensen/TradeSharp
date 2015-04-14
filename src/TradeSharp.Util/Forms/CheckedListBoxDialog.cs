using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class CheckedListBoxDialog : Form
    {
        public readonly Dictionary<object, bool> options;

        public CheckedListBoxDialog()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public CheckedListBoxDialog(Dictionary<object, bool> options) : this()
        {
            this.options = options;
            checkedListBox.DataSource = options.Keys.ToList();
            for (var i = 0; i < checkedListBox.Items.Count; i++)
            {
                if (options[checkedListBox.Items[i]])
                    checkedListBox.SetItemChecked(i, true);
            }
        }

        public static DialogResult Show(Dictionary<object, bool> options, Form owner, string title)
        {
            var dlg = new CheckedListBoxDialog(options)
                {
                    Text = title
                };
            return dlg.ShowDialog(owner);
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            for (var i = 0; i < checkedListBox.Items.Count; i++)
            {
                options[checkedListBox.Items[i]] = checkedListBox.GetItemChecked(i);
            }
            DialogResult = DialogResult.OK;
        }
    }
}
