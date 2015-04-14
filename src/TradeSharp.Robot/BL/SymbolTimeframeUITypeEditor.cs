using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using TradeSharp.Robot.Robot;

namespace TradeSharp.Robot.BL
{
// ReSharper disable InconsistentNaming
    public class SymbolTimeframesUITypeEditor : UITypeEditor
// ReSharper restore InconsistentNaming
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var timeFramesString = value as string;
            if (string.IsNullOrEmpty(timeFramesString))
                return value;
            var timeFrames = BaseRobot.ParseTickerTimeframeString(timeFramesString);
            var dlg = new RobotTimeframesForm(timeFrames);
            if (dlg.ShowDialog() != DialogResult.OK)
                return value;
            return dlg.UpdatedGraphics.Count == 0 ? string.Empty : 
                BaseRobot.GetStringFromTickerTimeframe(dlg.UpdatedGraphics);
        }
    }
}
