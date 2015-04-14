using System;
using System.Drawing;
using Candlechart.Chart;

namespace Candlechart.Core
{
    public class ExponentLabel
    {        
        // Methods
        internal ExponentLabel(YAxis owner)
        {
            Owner = owner;
        }

        private ChartControl Chart
        {
            get { return Owner.Chart; }
        }

        private YAxis Owner { get; set; }

        internal void Draw(Graphics g, Rectangle axisRect, YAxisAlignment alignment)
        {
            YAxisLabelInfo labelInfo = Owner.LabelInfo;
            StringFormat stringFormat = YAxis.PrepareStringFormat(alignment);
            using (stringFormat)
            {
                if (((int)Math.Round(labelInfo.Exponent)) != 1)
                {
                    var ef = new RectangleF(0f, 0f, 0f, 0f);
                    var exponent = (int)labelInfo.Exponent;
                    string str = "";
                    if (labelInfo.Exponent > 1000000.0)
                    {
                        exponent = (int)(labelInfo.Exponent / 1000000.0);
                        str = "M";
                    }
                    else if (labelInfo.Exponent > 1000.0)
                    {
                        exponent = (int)(labelInfo.Exponent / 1000.0);
                        str = "K";
                    }
                    if (labelInfo.Exponent < 1.01E-06)
                    {
                        exponent = (int)((labelInfo.Exponent * 1.01) / 1E-06);
                        str = "u";
                    }
                    else if (labelInfo.Exponent < 0.00101)
                    {
                        exponent = (int)((labelInfo.Exponent * 1.01) / 0.001);
                        str = "m";
                    }
                    ef.Size = g.MeasureString("x " + exponent + str, 
                        Chart.visualSettings.ExponentLabelFont, 
                        ef.Location, stringFormat);
                    if (alignment == YAxisAlignment.Left)
                    {
                        ef.X = (axisRect.Right - ef.Width) - 2f;
                    }
                    else if (alignment == YAxisAlignment.Right)
                    {
                        ef.X = axisRect.Left + 2;
                    }
                    ef.Y = (axisRect.Bottom - ef.Size.Height) - 1f;
                    if (axisRect.Contains(Rectangle.Round(ef)))
                    {
                        var pen = new Pen(Chart.visualSettings.ExponentLabelForeColor);
                        var brush = new SolidBrush(Chart.visualSettings.ExponentLabelBackColor);
                        var brush2 = new SolidBrush(Chart.visualSettings.ExponentLabelTextColor);
                        using (pen)
                        {
                            using (brush)
                            {
                                using (brush2)
                                {
                                    g.FillRectangle(brush, ef);
                                    g.DrawRectangle(pen, (int)ef.X, (int)ef.Y, (int)ef.Width, (int)ef.Height);
                                    g.DrawString("x " + exponent + str, 
                                        Chart.visualSettings.ExponentLabelFont, brush2, ef, stringFormat);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
