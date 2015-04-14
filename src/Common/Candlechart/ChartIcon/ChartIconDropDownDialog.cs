using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Candlechart.Chart;

namespace Candlechart.ChartIcon
{
    public class ChartIconDropDownDialog : ChartIcon
    {
        public ChartControl Owner { private get; set; }

        private EventHandler beforeDropDown;

        public event EventHandler BeforeDropDown
        {
            add { beforeDropDown += value; }
            remove { beforeDropDown -= value; }
        }

        public ChartIconDropDownDialog()
        {
        }

        public void DataBind(List<object> values)
        {
            //dropControl.Values = values;
        }

        public DropDownList.FormatValueDel Formatter
        {
            set { /*dropControl.Formatter = value;*/ }
        }

        public override void OnMouseClick()
        {
            if (beforeDropDown != null)
                beforeDropDown(this, EventArgs.Empty);
            // показать опции
            ShowOptions();
        }

        public override ChartIcon MakeCopy()
        {
            var cpy = new ChartIconDropDownDialog
                          {
                              Owner = Owner,
                              key = key,
                              Position = Position,
                              Size = Size
                          };
            return cpy;
        }

        private void ShowOptions()
        {
            var popup = new DropDownWindowPopup(Owner);
            popup.Show(Owner, Position.X, Position.Y + Size.Height);
        }
    }
}
