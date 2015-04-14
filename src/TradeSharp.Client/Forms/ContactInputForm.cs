using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ContactInputForm : Form
    {
        public object SelectedItem
        {
            get { return imagedComboBox1.SelectedItem; }
        }

        public string InputText
        {
            get { return textBox1.Text; }
        }

        public ContactInputForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public void SetItems(List<object> items, string displayMember, string displayImageIndexMember, ImageList imageList)
        {
            imagedComboBox1.DisplayMember = displayMember;
            imagedComboBox1.DisplayImageIndexMember = displayImageIndexMember;
            imagedComboBox1.ImageList = imageList;
            imagedComboBox1.Items.AddRange(items.ToArray());
        }
    }
}
