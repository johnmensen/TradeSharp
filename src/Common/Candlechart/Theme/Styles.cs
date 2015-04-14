using System.Drawing;
using Candlechart.Core;

namespace Candlechart.Theme
{
    public enum FrameStyle
    {
        Flat,
        ThreeD
    }

    public enum GridLineStyle
    {
        Dot,
        Dash,
        Solid
    }

    internal interface IChartItem
    {
        // Methods
        void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect);
        bool GetXExtent(ref double left, ref double right);
        bool GetYExtent(double left, double right, ref double top, ref double bottom);
    }    
    
    public enum LineStyle
    {
        Solid,
        Dot,
        Dash,
        DashDot,
        DashDotDot
    }    
}