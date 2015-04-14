using System;
using System.Collections.Generic;
using Candlechart.Chart;

namespace Candlechart.ChartIcon
{
    public class ChartIconDropDown : ChartIcon
    {
        public DropDownList listControl;

        private ChartControl owner;
        public ChartControl Owner
        {
            set
            {
                owner = value;
                listControl.owner = value;
            }
            private get { return owner; }
        }

        private EventHandler beforeDropDown;

        public event EventHandler BeforeDropDown
        {
            add { beforeDropDown += value; }
            remove { beforeDropDown -= value; }
        }

        public ChartIconDropDown()
        {
            listControl = new DropDownList
                              {
                                  CalcHeightAuto = true,
                                  Width = 150,
                                  MaxLines = 12
                              };
        }

        public void DataBind(List<object> values)
        {
            listControl.Values = values;
        }

        public DropDownList.FormatValueDel Formatter
        {
            set { listControl.Formatter = value; }
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
            var cpy = new ChartIconDropDown
                          {
                              Owner = Owner,
                              listControl = new DropDownList(listControl),
                              key = key,
                              Position = Position,
                              Size = Size
                          };
            return cpy;
        }

        private void ShowOptions()
        {
            var popup = new DropDownListPopup(listControl);
            popup.Show(Owner, Position.X, Position.Y + Size.Height);
        }
    }
}
