using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public class PreValuedTextBox : TextBox
    {
        public delegate object ValueFormatterDel(string text);

        private bool hasDefaultValue = true;

        public bool HasDefaultValue
        {
            get { return hasDefaultValue; }
        }

        private Color storedForeColor;

        public string defaultValue;
        [DisplayName("Значение по умолчанию")]
        [Category("Pre Valued")]
        public string DefaultValue
        {
            get { return defaultValue; }
            set 
            { 
                defaultValue = value;
                hasDefaultValue = true;
                Text = defaultValue;
                //storedForeColor = ForeColor;
                ForeColor = DefaultColor;
            }
        }

        private Color defaultColor = Color.Gray;
        [DisplayName("Цвет текста (ум)")]
        [Category("Pre Valued")]
        public Color DefaultColor
        {
            get { return defaultColor; }
            set { defaultColor = value; }
        }

        public string BoundPropertyName { get; set; }

        public string BoundPropertyTitle { get; set; }

        public ValueFormatterDel formatter;

        public PreValuedTextBox()
        {
            storedForeColor = ForeColor;
            ForeColor = DefaultColor;
            Click += (sender, args) => ResetDefault();
            KeyDown += (sender, args) => ResetDefault();
        }

        private void ResetDefault()
        {
            if (!hasDefaultValue) return;
            ForeColor = storedForeColor;
            Text = "";
            hasDefaultValue = false;
        }

        public void UpdateDefaultValue(string val)
        {
            if (!hasDefaultValue) return;
            defaultValue = val;
            Text = defaultValue;
        }

        public object GetFormattedValue()
        {
            if (formatter != null)
                return formatter(Text);
            return Text.Replace(",", ".").ToFloatUniformSafe();
        }
    }
}
