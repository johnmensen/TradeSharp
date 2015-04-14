using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace FastGrid
{
    public class PopupTextBox : ToolStripDropDown
    {
        protected Control Content;

        protected int RowIndex;

        protected FastColumn Column;
        
        protected Type PropertyType;

        // before conversion event (for unhandled types)
        //public Action<FastColumn, int, string> OnStringUpdated;

        // after conversion event
        public Action<FastColumn, int, object> OnValueUpdated;

        public PopupTextBox()
        {
        }

        public PopupTextBox(object obj, int width, int height,
            int rowIndex, FastColumn column, PropertyInfo property,
            Action<FastColumn, int, object> onValueUpdated)
            : this()
        {
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            AutoSize = false;
            Width = width;
            Height = height;

            RowIndex = rowIndex;
            Column = column;
            PropertyType = property.PropertyType;
            OnValueUpdated = onValueUpdated;

            var valueAttr =
                property.GetCustomAttributes(true).FirstOrDefault(a => a is ValueListAttribute) as
                ValueListAttribute;
            ComboBox comboBox = null;
            if (valueAttr != null)
            {
                comboBox = new ComboBox();
                foreach (var value in valueAttr.Values)
                {
                    comboBox.Items.Add(value);
                    if (value.Equals(obj))
                        comboBox.SelectedItem = value;
                }
                comboBox.KeyUp += OnControlKeyUp;
            }
            if (PropertyType == typeof(int) || PropertyType == typeof(decimal) ||
                PropertyType == typeof (float) || PropertyType == typeof (double) ||
                PropertyType == typeof (long) || PropertyType == typeof (short) ||
                PropertyType == typeof (int?) || PropertyType == typeof (decimal?))
            {
                var numericControl = new NumericUpDown();
                if (PropertyType == typeof (decimal) || PropertyType == typeof (float) ||
                    PropertyType == typeof (double) || PropertyType == typeof (decimal?))
                {
                    decimal value = 0;
                    if (PropertyType == typeof (decimal))
                        value = (decimal) obj;
                    else if (PropertyType == typeof (float))
                        value = (decimal) (float) obj;
                    else if (PropertyType == typeof (double))
                        value = (decimal) (double) obj;
                    else if (PropertyType == typeof (decimal?))
                        value = ((decimal?) obj).HasValue ? ((decimal?) obj).Value : 0;
                    var decimalString = value.ToString().Replace(',', '.');
                    if (decimalString.Contains("."))
                        numericControl.DecimalPlaces = decimalString.Substring(decimalString.IndexOf(".") + 1).Length;
                    numericControl.Minimum = decimal.MinValue;
                    numericControl.Maximum = decimal.MaxValue;
                    numericControl.Value = value;
                }
                else if (PropertyType == typeof (int))
                {
                    numericControl.Minimum = int.MinValue;
                    numericControl.Maximum = int.MaxValue;
                    numericControl.Value = (int) obj;
                }
                else if (PropertyType == typeof (long))
                {
                    numericControl.Minimum = long.MinValue;
                    numericControl.Maximum = long.MaxValue;
                    numericControl.Value = (long) obj;
                }
                else if (PropertyType == typeof (short))
                {
                    numericControl.Minimum = short.MinValue;
                    numericControl.Maximum = short.MaxValue;
                    numericControl.Value = (short) obj;
                }
                else if (PropertyType == typeof (int?))
                {
                    numericControl.Minimum = int.MinValue;
                    numericControl.Maximum = int.MaxValue;
                    numericControl.Value = ((int?) obj).HasValue ? ((int?) obj).Value : 0;
                }
                numericControl.ValueChanged += OnValueChanged;
                numericControl.KeyUp += OnControlKeyUp;

                if (valueAttr != null)
                {
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    if (valueAttr.IsEditable)
                    {
                        comboBox.Controls.Add(numericControl);
                        comboBox.SelectedIndexChanged += (sender, args) =>
                            {
                                var thisComboBox = sender as ComboBox;
                                if (thisComboBox == null)
                                    return;
                                if (thisComboBox.SelectedIndex == -1)
                                    return;
                                decimal result;
                                if (!decimal.TryParse(thisComboBox.SelectedItem.ToString(), out result))
                                    return;
                                numericControl.Value = result;
                            };
                        comboBox.Resize += (sender, args) =>
                            {
                                var thisComboBox = sender as ComboBox;
                                if (thisComboBox == null)
                                    return;
                                numericControl.Width = thisComboBox.Width - SystemInformation.VerticalScrollBarWidth - 2;
                            };
                    }
                    else
                    {
                        comboBox.SelectedIndexChanged += OnValueChanged;
                    }

                    Content = comboBox;
                }
                else
                    Content = numericControl;
            }
            else if (PropertyType == typeof (DateTime))
            {
                var control = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "dd.MM.yyyy HH:mm:ss",
                        ShowUpDown = true
                    };
                try
                {
                    control.Value = (DateTime) obj;
                }
                catch
                {
                }
                control.ValueChanged += OnValueChanged;

                Content = control;
            }
            else
            {
                if (valueAttr != null)
                {
                    comboBox.DropDownStyle = valueAttr.IsEditable ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
                    if (valueAttr.IsEditable)
                    {
                        if (string.IsNullOrEmpty(comboBox.Text))
                            comboBox.Text = obj.ToString();
                        comboBox.TextChanged += OnValueChanged;
                    }
                    comboBox.SelectedIndexChanged += OnValueChanged;

                    Content = comboBox;
                }
                else
                {
                    var textControl = new TextBox {Text = obj as string};
                    textControl.KeyUp += OnControlKeyUp;

                    Content = textControl;
                }
            }

            Content.Dock = DockStyle.Fill;

            var host = new ToolStripControlHost(Content)
                           {
                               Margin = Padding.Empty, 
                               Padding = Padding.Empty,
                               AutoSize = false,
                               Width = width,
                               Height = height
                           };
            Items.Add(host);
            Opened += (sender, e) => Content.Focus();
        }

        private void OnControlKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return && e.KeyCode != Keys.Tab)
                return;
            OnValueChanged(sender, e);
            // закрыть контрол
            Close();
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            object resultObject = null;
            if (PropertyType == typeof (int) || PropertyType == typeof (decimal) ||
                PropertyType == typeof (float) || PropertyType == typeof (double) ||
                PropertyType == typeof (long) || PropertyType == typeof (short) ||
                PropertyType == typeof (int?) || PropertyType == typeof (decimal?))
            {
                var numericControl = Content as NumericUpDown;
                if (numericControl == null)
                {
                    var comboBox = Content as ComboBox;
                    if (comboBox == null)
                        return;
                    foreach (Control ctrl in comboBox.Controls)
                    {
                        numericControl = ctrl as NumericUpDown;
                        if (numericControl != null)
                            break;
                    }
                    if (numericControl == null)
                    {
                        if (OnValueUpdated != null)
                            OnValueUpdated(Column, RowIndex, comboBox.SelectedItem);
                        return;
                    }
                }
                var value = numericControl.Value;
                if (PropertyType == typeof (decimal))
                    resultObject = value;
                else if (PropertyType == typeof (float))
                    resultObject = (float) value;
                else if (PropertyType == typeof (double))
                    resultObject = (double) value;
                else if (PropertyType == typeof (int))
                    resultObject = (int) value;
                else if (PropertyType == typeof (long))
                    resultObject = (long) value;
                else if (PropertyType == typeof (short))
                    resultObject = (short) value;
                else if (PropertyType == typeof (int?))
                    resultObject = (int?) value;
                else if (PropertyType == typeof (decimal?))
                    resultObject = (decimal?) value;
            }
            else if (PropertyType == typeof (DateTime))
            {
                var control = (DateTimePicker) Content;
                resultObject = control.Value;
            }
            else
            {
                var comboBox = Content as ComboBox;
                if (comboBox != null)
                    resultObject = comboBox.DropDownStyle == ComboBoxStyle.DropDownList
                                       ? comboBox.SelectedItem
                                       : comboBox.Text;
                else
                    resultObject = Content.Text;
            }
            if (OnValueUpdated != null)
                OnValueUpdated(Column, RowIndex, resultObject);
        }
    }
}
