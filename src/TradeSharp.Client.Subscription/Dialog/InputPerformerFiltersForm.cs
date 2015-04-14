using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class InputPerformerFiltersForm : Form
    {
        public InputPerformerFiltersForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public List<PerformerSearchCriteria> GetFilters()
        {
            var stat = new PerformerStat();
            var result = new List<PerformerSearchCriteria>();
            if (fioCheckBox.Checked)
                result.Add(new PerformerSearchCriteria
                    {
                        propertyName = stat.Property(t => t.FullName),
                        compradant = fioTextBox.Text,
                        ignoreCase = !fioCSCheckBox.Checked
                    });
            if (emailCheckBox.Checked)
                result.Add(new PerformerSearchCriteria
                {
                    propertyName = stat.Property(t => t.Email),
                    compradant = emailTextBox.Text,
                    ignoreCase = !emailCSCheckBox.Checked
                });
            if (accountCheckBox.Checked)
                result.Add(new PerformerSearchCriteria
                {
                    propertyName = stat.Property(t => t.Account),
                    compradant = accountNumericUpDown.Value.ToString(),
                    ignoreCase = true,
                    checkWholeWord = true
                });
            return result;
        }

        public int GetCount()
        {
            return (int) countNumericUpDown.Value;
        }

        private void FioCheckBoxCheckedChanged(object sender, System.EventArgs e)
        {
            if (!fioCheckBox.Checked)
                return;
            if (accountCheckBox.Checked)
                accountCheckBox.Checked = false;
        }

        private void EmailCheckBoxCheckedChanged(object sender, System.EventArgs e)
        {
            if (!emailCheckBox.Checked)
                return;
            if (accountCheckBox.Checked)
                accountCheckBox.Checked = false;
        }

        private void AccountCheckBoxCheckedChanged(object sender, System.EventArgs e)
        {
            if (!accountCheckBox.Checked)
                return;
            if (fioCheckBox.Checked)
                fioCheckBox.Checked = false;
            if (emailCheckBox.Checked)
                emailCheckBox.Checked = false;
        }

        private void CheckByDefault(object sender, System.EventArgs e)
        {
            // если ни одного флажка нет, то устанавливаем флажок напротив элемента, в который производится ввод
            if (fioCheckBox.Checked || emailCheckBox.Checked || accountCheckBox.Checked)
                return;
            CheckBox control = null;
            if (sender == fioTextBox)
                control = fioCheckBox;
            if (sender == emailTextBox)
                control = emailCheckBox;
            if (sender == accountNumericUpDown)
                control = accountCheckBox;
            if (control == null)
                return;
            control.Checked = true;
        }
    }
}
