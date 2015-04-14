using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public class PopupListBox
    {
        protected int rowIndex;

        protected FastColumn column;
        
        protected Type columnType;

        protected Action<FastColumn, int, object> onValueUpdated;

        protected object[] values;

        protected List<string> stringValues = new List<string>();

        protected Control parent;

        private readonly int selectedIndex;

        public PopupListBox(object originalValue,
            int rowIndex, FastColumn column, Type columnType,
            Action<FastColumn, int, object> onValueUpdated, Control parent)
        {
            this.rowIndex = rowIndex;
            this.onValueUpdated = onValueUpdated;
            this.parent = parent;
            this.column = column;

            // получить допустимые значения
            if (columnType == typeof(bool))
                values = new object[] { false, true };
            else if (columnType.IsSubclassOf(typeof(Enum)))
                values = Enum.GetValues(columnType).Cast<object>().ToArray();
            if (values == null) return;

            // сгенерировать массив строк для указанного типа
            var index = 0;
            foreach (var val in values)
            {
                if (val.Equals(originalValue)) 
                    selectedIndex = index;
                index++;
                string str;
                if (column.formatter != null)
                    str = column.formatter(val);
                else if (columnType.IsSubclassOf(typeof (Enum)))
                {
                    var resourceKey = "Enum" + columnType.Name + val;
                    str = Localizer.HasKey(resourceKey) ? Localizer.GetString(resourceKey) : val.ToString();
                }
                else
                    str = val.ToString();
                stringValues.Add(str);
            }
        }
    
        public void ShowOptions(int x, int y)
        {
            var menu = new ContextMenuStrip();
            var index = 0;
            foreach (var str in stringValues)
            {
                var item = (ToolStripMenuItem) menu.Items.Add(str);
                item.Click += ItemOnClick;
                if (index++ == selectedIndex)
                    item.Checked = true;
            }
            menu.Show(parent, x, y);
        }

        private void ItemOnClick(object sender, EventArgs eventArgs)
        {
            var senderItem = (ToolStripMenuItem) sender;
            if (senderItem.Checked)
                return;
            var index = stringValues.IndexOf(senderItem.Text);
            if (index == -1)
                return;
            onValueUpdated(column, rowIndex, values[index]);
        }
    }
}
