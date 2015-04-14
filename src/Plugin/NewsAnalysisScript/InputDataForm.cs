using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Client;

namespace NewsAnalysisScript
{
    public partial class InputDataForm : Form
    {
        public InputDataForm()
        {
            InitializeComponent();
        }

        public void SetCurrencies(List<string> currencies)
        {
            availableListBox.Items.AddRange(currencies.ToArray());
        }

        public string GetNewsFileName()
        {
            return newsPathTextBox.Text;
        }

        public string[] GetQuotesFileNames()
        {
            return quotesPathTextBox.Text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public List<string> GetSelectedCurrencies()
        {
            var result = new List<string>();
            foreach (var item in selectedListBox.Items)
                result.Add(item.ToString());
            return result;
        }

        public DateTime GetStartTime()
        {
            return startDateTimePicker.Value;
        }

        public DateTime GetEndTime()
        {
            return endDateTimePicker.Value;
        }

        public bool GetOnlyValuableNewsFlag()
        {
            return checkBox1.Checked;
        }

        private void SelectedListBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            object item = selectedListBox.SelectedItem;
            if (item == null)
                return;
            selectedListBox.Items.Remove(item);
            availableListBox.Items.Add(item);
            okButton.Enabled = selectedListBox.Items.Count != 0;
        }

        private void AvailableListBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            object item = availableListBox.SelectedItem;
            if (item == null)
                return;
            availableListBox.Items.Remove(item);
            selectedListBox.Items.Add(item);
            okButton.Enabled = true;
        }

        private void OpenNewsButtonClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовый файл (*.txt)|*.txt";
            if (openFileDialog.ShowDialog(MainForm.Instance) == DialogResult.Cancel)
                return;
            newsPathTextBox.Text = openFileDialog.FileName;
        }

        private void DeleteNewsbuttonClick(object sender, EventArgs e)
        {
            newsPathTextBox.Text = null;
        }

        private void OpenQuotesButtonClick(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Файлы котировок (*.quote)|*.quote";
            if (openFileDialog.ShowDialog(MainForm.Instance) == DialogResult.Cancel)
                return;
            quotesPathTextBox.Text = string.Join(";", openFileDialog.FileNames);
        }

        private void DeleteQuotesButtonClick(object sender, EventArgs e)
        {
            newsPathTextBox.Text = null;
        }
    }
}
