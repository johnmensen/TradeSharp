using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Entity;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class TickersAndVolumesBarControl : UserControl
    {
        public readonly List<TickerVolumes> Lines = new List<TickerVolumes>();

        private int centerLabelWidth = 50;
        public int CenterLabelWidth
        {
            get { return centerLabelWidth; }
            set { centerLabelWidth = value; }
        }

        private int rowHeight = 20;
        public int RowHeight
        {
            get { return rowHeight; }
            set { rowHeight = value; }
        }

        private Color leftColor = Color.LightCoral;
        public Color LeftColor
        {
            get { return leftColor; }
            set { leftColor = value; }
        }

        private Color rightColor = Color.LightBlue;
        public Color RightColor
        {
            get { return rightColor; }
            set { rightColor = value; }
        }

        public TickersAndVolumesBarControl()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var freeSpaceOnEachSide = (Width - centerLabelWidth) / 2;
            using (var textBrush = new SolidBrush(Color.Black))
                for (var lineIndex = 0; lineIndex < Lines.Count; lineIndex++ )
                {
                    var line = Lines[lineIndex];
                    var leftWidth = (int) (freeSpaceOnEachSide * line.LeftValue);
                    if (leftWidth > 0)
                        e.Graphics.FillRoundedRectangle(new SolidBrush(leftColor),
                                                        new Rectangle(freeSpaceOnEachSide - leftWidth,
                                                                      lineIndex * rowHeight,
                                                                      leftWidth + centerLabelWidth / 2, rowHeight), 5);
                    var rightWidth = (int) (freeSpaceOnEachSide * line.RightValue);
                    if (rightWidth > 0)
                        e.Graphics.FillRoundedRectangle(new SolidBrush(rightColor),
                                                        new Rectangle(freeSpaceOnEachSide + centerLabelWidth / 2,
                                                                      lineIndex * rowHeight, centerLabelWidth / 2 + rightWidth, rowHeight), 5);
                    e.Graphics.FillRoundedRectangle(new SolidBrush(Color.White),
                                                    new Rectangle(freeSpaceOnEachSide, lineIndex * rowHeight,
                                                                  centerLabelWidth, rowHeight), 5);
                    e.Graphics.DrawString(line.LeftLabel, Font, textBrush,
                                          new RectangleF(0, lineIndex * rowHeight, freeSpaceOnEachSide, rowHeight),
                                          new StringFormat
                                          {
                                              Alignment = StringAlignment.Far,
                                              LineAlignment = StringAlignment.Center
                                          });
                    e.Graphics.DrawString(line.RightLabel, Font, textBrush,
                                          new RectangleF(freeSpaceOnEachSide + centerLabelWidth,
                                                         lineIndex * rowHeight,
                                                         freeSpaceOnEachSide, rowHeight),
                                          new StringFormat
                                              {
                                                  Alignment = StringAlignment.Near,
                                                  LineAlignment = StringAlignment.Center
                                              });
                    e.Graphics.DrawString(line.CenterLabel, Font, textBrush,
                                          new RectangleF(freeSpaceOnEachSide, lineIndex * rowHeight, centerLabelWidth, rowHeight),
                                          new StringFormat
                                          {
                                              Alignment = StringAlignment.Center,
                                              LineAlignment = StringAlignment.Center
                                          });
                }
        }
    }

    public class TickerVolumes
    {
        // 0..1
        public double LeftValue;

        // 0..1
        public double RightValue;

        public string CenterLabel;

        public string LeftLabel;

        public string RightLabel;
    }
}
