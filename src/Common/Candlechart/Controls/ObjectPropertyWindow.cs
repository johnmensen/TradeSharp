using System.Collections.Generic;
using System.Windows.Forms;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class ObjectPropertyWindow : Form
    {
        public ObjectPropertyWindow()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }
     
        public ObjectPropertyWindow(List<object> editedObjects, string title = "") : this()
        {
            if (!string.IsNullOrEmpty(title))
                Text = title;
            if (editedObjects.Count == 1)
                propertyGrid.SelectedObject = editedObjects[0];
            else
                propertyGrid.SelectedObjects = editedObjects;
        }
    }
}