using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace TradeSharp.Util.Forms
{
    public partial class CheckedOrderedListBoxDialog : Form
    {
        private List<object> options;
        private List<bool> checkState;

        public CheckedOrderedListBoxDialog()
        {
            InitializeComponent();
            listBox.AllowDrop = true;
        }

        public CheckedOrderedListBoxDialog(List<object> options, List<bool> checkState)
            : this()
        {
            this.checkState = checkState;
            this.options = options;

            foreach (var t in options)
                listBox.Items.Add(t);
            
            for (var i = 0; i < options.Count; i++)
            {
                listBox[i] = checkState[i];
            }

            //var percentChecked = checkState.Count < 1 ? 0 : checkState.Count(s => s)*100/checkState.Count;
            var allChecked = checkState.Count(s => s);
            cbSelection.Checked = allChecked == checkState.Count;
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            var updatedCheckState = new List<bool>();
            var updatedOptions = new List<object>();

            for (var i = 0; i < options.Count; i++)
            {
                updatedCheckState.Add(listBox[i]);
                updatedOptions.Add(listBox.Items[i]);
            }
            checkState = updatedCheckState;
            options = updatedOptions;
            DialogResult = DialogResult.OK;
        }

        public static bool ShowDialog(ref List<object> options, ref List<bool> checkState, string caption)
        {
            var dlg = new CheckedOrderedListBoxDialog(options, checkState)
                {
                    Text = caption
                };
            if (dlg.ShowDialog() != DialogResult.OK) 
                return false;
            checkState = dlg.checkState;
            options = dlg.options;
            return true;
        }

        private void CbSelectionCheckStateChanged(object sender, EventArgs e)
        {
            if (cbSelection.CheckState == CheckState.Indeterminate)
                return;
            var targetState = cbSelection.Checked;
            
            for (var i = 0; i < listBox.Items.Count; i++)
                listBox[i] = targetState;
        }
    }
}
