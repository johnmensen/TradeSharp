using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace TradeSharp.Util
{
    public class CheckBoxPropertyGridEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }
 
        public override void PaintValue(PaintValueEventArgs e)
        {
            var val = (bool?) e.Value;

            ControlPaint.DrawCheckBox(e.Graphics, e.Bounds, 
                (val ?? false) ? ButtonState.Checked : ButtonState.Normal);
        }
    }
}
