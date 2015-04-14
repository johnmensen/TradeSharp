using System;
using System.ComponentModel.Design;

namespace TradeSharp.Util.Controls
{
    public class CollapsiblePanelDesigner : System.Windows.Forms.Design.ParentControlDesigner
    {
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                var collection = new DesignerActionListCollection();
                var panel = Control as CollapsiblePanel;
                if (panel != null)
                {
                    if (!String.IsNullOrEmpty(panel.Name))
                    {
                        if (String.IsNullOrEmpty(panel.Text))
                            panel.Text = panel.Name;
                    }
                }
                collection.Add(new CollapsiblePanelActionList(Control));
                return collection;
            }
        }
    }
}
