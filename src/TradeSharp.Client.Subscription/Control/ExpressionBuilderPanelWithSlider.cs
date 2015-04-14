using System.ComponentModel;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class ExpressionBuilderPanelWithSlider : UserControl
    {
        public delegate void DataChangedDel(System.Windows.Forms.Control sender);

        public DataChangedDel DataChanged;

        public string LabelText
        {
            get { return label.Text; }
            set { label.Text = value; }
        }

        public double Minimum
        {
            get { return (double) numericUpDown.Minimum; }
            set { numericUpDown.Minimum = (decimal) value; }
        }

        public double Maximum
        {
            get { return (double) numericUpDown.Maximum; }
            set { numericUpDown.Maximum = (decimal) value; }
        }

        public double Value
        {
            get { return (double) numericUpDown.Value; }
            set { numericUpDown.Value = (decimal) value; }
        }

        public ExpressionOperator Operator
        {
            get
            {
                if (comboBox.SelectedIndex == 0)
                    return ExpressionOperator.Greater;
                if (comboBox.SelectedIndex == 1)
                    return ExpressionOperator.Lower;
                return ExpressionOperator.Equal;
            }
            set
            {
                switch (value)
                {
                    case ExpressionOperator.Greater:
                        comboBox.SelectedIndex = 0;
                        break;
                    case ExpressionOperator.Lower:
                        comboBox.SelectedIndex = 1;
                        break;
                    case ExpressionOperator.Equal:
                        comboBox.SelectedIndex = 2;
                        break;
                }
            }
        }

        public string OperatorString
        {
            get
            {
                if (comboBox.SelectedIndex == 0)
                    return ">";
                if (comboBox.SelectedIndex == 1)
                    return "<";
                return "=";
            }
        }

        private PerformerStatField field;
        [Browsable(false)]
        public PerformerStatField Field
        {
            get { return field; }
            set
            {
                field = value;
                label.Text = field.ExpressionParamTitle;
                if (field.DefaultOperator.HasValue)
                    Operator = field.DefaultOperator.Value;
                if (field.DefaultValue.HasValue)
                    Value = field.DefaultValue.Value;
            }
        }

        public ExpressionBuilderPanelWithSlider()
        {
            InitializeComponent();

            comboBox.SelectedIndex = 0;
            numericUpDown.Minimum = decimal.MinValue;
            numericUpDown.Maximum = decimal.MaxValue;
        }

        public string UpdateExpression(string expression)
        {
            if (field == null)
                return "";
            return expression + (!string.IsNullOrEmpty(expression) ? "&" : "") + "(" + field.ExpressionParamName + OperatorString + Value + ")";
        }

        private void ComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (DataChanged != null)
                DataChanged(this);
        }

        private void NumericUpDownValueChanged(object sender, System.EventArgs e)
        {
            if (DataChanged != null)
                DataChanged(this);
        }
    }
}
