using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FastGrid
{
    public partial class ColumnConfigurator : Form
    {
        private readonly FastGrid grid;

        public ColumnConfigurator(FastGrid grid)
        {
            InitializeComponent();
            this.grid = grid;
            foreach (var column in grid.Columns)
                if (column.Visible)
                    listBoxVisible.Items.Add(column.Title);
                else
                    listBoxAvailable.Items.Add(column.Title);
        }

        private void ListBoxAvailableMouseDoubleClick(object sender, MouseEventArgs e)
        {
            object item = listBoxAvailable.SelectedItem;
            if (item == null)
                return;
            listBoxAvailable.Items.Remove(item);
            listBoxVisible.Items.Add(item);
            buttonApply.Enabled = true;
        }

        private void ListBoxVisibleMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listBoxVisible.SelectedItem;
            if (item == null)
                return;
            listBoxVisible.Items.Remove(item);
            listBoxAvailable.Items.Add(item);
            buttonApply.Enabled = true;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            ButtonApplyClick(sender, e);
        }

        private void ButtonApplyClick(object sender, EventArgs e)
        {
            var columns = new List<FastColumn>();
            foreach (var columnItem in listBoxAvailable.Items)
            {
                var column = grid.Columns.Find(c => c.Title == columnItem as string);
                column.Visible = false;
                columns.Add(column);
            }
            foreach (var columnItem in listBoxVisible.Items)
            {
                var column = grid.Columns.Find(c => c.Title == columnItem as string);
                column.Visible = true;
                columns.Add(column);
            }
            grid.Columns = columns;
            grid.CheckSize(true);
            buttonApply.Enabled = false;
        }

        private void ListBoxVisibleKeyDown(object sender, KeyEventArgs e)
        {
            var index = listBoxVisible.SelectedIndex;
            if (index == -1)
                return;
            if(e.KeyCode == Keys.Up && index > 0)
            {
                var item = listBoxVisible.Items[index - 1];
                listBoxVisible.Items[index - 1] = listBoxVisible.Items[index];
                listBoxVisible.Items[index] = item;
                buttonApply.Enabled = true;
            }
            if (e.KeyCode == Keys.Down && index < listBoxVisible.Items.Count - 1)
            {
                var item = listBoxVisible.Items[index + 1];
                listBoxVisible.Items[index + 1] = listBoxVisible.Items[index];
                listBoxVisible.Items[index] = item;
                buttonApply.Enabled = true;
            }
        }
    }
}
