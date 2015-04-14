using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class GenericObjectEditor : UserControl
    {
        private readonly Type propType;
        public readonly object propValue;
        public string Title { get; private set; }

        public object PropertyValue
        {
            get
            {
                if (editCheck.Visible) return editCheck.Checked;
                if (editCombo.Visible) return Enum.Parse(propType, (string) editCombo.SelectedItem);                                    
                if (editDateTime.Visible) return editDateTime.Value;
                if (editColorPick.Visible) return editColorPick.BackColor;
                
                return StringFormatter.StringToObject(editText.Text, propType);
            }
        }
        
        public GenericObjectEditor()
        {
            InitializeComponent();
        }

        public GenericObjectEditor(string title, Type propType, object propValue)
        {
            InitializeComponent();
            this.propType = propType;
            Title = title;
            lblComment.Text = title;
            this.propValue = propValue;
        }

        private void GenericObjectEditorLoad(object sender, EventArgs e)
        {
            // настроить внешность
            if (propType == typeof(DateTime))
            {
                editDateTime.Value = (DateTime) propValue;
                editDateTime.Visible = true;
                editDateTime.Dock = DockStyle.Fill;
                return;
            }
            if (propType == typeof(bool))
            {
                editCheck.Checked = (bool)propValue;
                editCheck.Visible = true;
                editCheck.Dock = DockStyle.Fill;
                return;
            }
            if (propType == typeof(Color))
            {
                editColorPick.BackColor = (Color)propValue;
                editColorPick.Visible = true;
                editColorPick.Dock = DockStyle.Fill;
                return;
            }
            if (propType.IsSubclassOf(typeof(Enum)))
            {
                var values = Enum.GetValues(propType);
                var strings = (from object val in values select val.ToString()).ToList();
                editCombo.DataSource = strings;
                var valueStr = propValue.ToString();
                for (var i = 0; i < editCombo.Items.Count; i++)
                {
                    if ((string)editCombo.Items[i] != valueStr) continue;
                    editCombo.SelectedIndex = i;
                    break;
                }
                editCombo.Visible = true;
                editCombo.Dock = DockStyle.Fill;
                return;
            }
            editText.Text = StringFormatter.ObjectToString(propValue);
            editText.Visible = true;
            editText.Dock = DockStyle.Fill;
        }

        private void EditColorPickClick(object sender, EventArgs e)
        {
            colorDialog.Color = editColorPick.BackColor;
            if (colorDialog.ShowDialog() != DialogResult.OK) return;
            editColorPick.BackColor = colorDialog.Color;
        }
    }
}
