using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using Candlechart.Chart;

namespace Candlechart.Core
{
    public class PaneCollection : CollectionBase
    {
        // Fields
        private readonly ArrayList _positionList = new ArrayList();

        // Methods
        internal PaneCollection(ChartControl owner)
        {
            Owner = owner;
            StockPane = new Pane("STOCK", Owner, Chart.visualSettings.BackgroundImageResourceName)
                            {
                                PercentHeight = 100f,
                                TitleBoxVisible = false
                            };
            PositionList.Add(StockPane);
            XAxisPane = new XAxisPane(Owner);
        }

        internal Pane BottomPane
        {
            get { return (Pane)PositionList[PositionList.Count - 1]; }
        }

        private ChartControl Chart
        {
            get { return Owner; }
        }

        public Pane this[string name]
        {
            get
            {
                foreach (Pane pane in InnerList)
                {
                    if (pane.Name == name)
                    {
                        return pane;
                    }
                }
                return null;
            }
        }

        public Pane this[int index]
        {
            get
            {
                return (Pane)InnerList[index];                
            }
        }

        public Pane GetPaneByCoords(int x, int y)
        {            
            foreach (Pane pane in InnerList)
            {                
                var dx = x - pane.Location.X;
                var dy = y - pane.Location.Y;
                if (dx >= 0 && dx < pane.Width && dy >= 0 && dy <= pane.Height)
                {
                    return pane;                    
                }
            }
            return null;
        }

        private ChartControl Owner { get; set; }

        internal ArrayList PositionList
        {
            get { return _positionList; }
        }

        internal Pane StockPane { get; set; }

        internal Pane TopPane
        {
            get { return (Pane)PositionList[0]; }
        }

        internal XAxisPane XAxisPane { get; set; }

        public Pane Add(string name)
        {
            return Add(name, (Pane)PositionList[PositionList.Count - 1],
                       ((Pane)PositionList[PositionList.Count - 1]).PercentHeight);
        }

        public Pane Add(string name, Pane insertAfter)
        {
            float percentHeight = insertAfter != null ?                                                                          
                insertAfter.PercentHeight : 
                ((Pane)PositionList[PositionList.Count - 1]).PercentHeight;
            return Add(name, insertAfter, percentHeight);
        }

        internal Pane Add(string name, Pane insertAfter, float percentHeight)
        {
            var pane = new Pane(name, Owner) {PercentHeight = percentHeight};
            List.Add(pane);
            int index = PositionList.IndexOf(insertAfter);
            PositionList.Insert(index + 1, pane);
            Chart.PerformLayout();
            return pane;
        }

        internal Pane Add(Pane pane, Pane insertAfter, float percentHeight)
        {
            pane.PercentHeight = percentHeight;
            List.Add(pane);
            int index = PositionList.IndexOf(insertAfter);
            PositionList.Insert(index + 1, pane);
            Chart.PerformLayout();
            return pane;
        }

        public Pane Add(Pane pane)
        {
            return Add(pane, (Pane)PositionList[PositionList.Count - 1],
                       ((Pane)PositionList[PositionList.Count - 1]).PercentHeight);
        }

        public Pane Add(Pane pane, float percentHeight)
        {
            return Add(pane, (Pane)PositionList[PositionList.Count - 1], percentHeight);
        }

        public bool ContainsPane(Pane pane)
        {
            return PositionList.Contains(pane);
        }

        public new void Clear()
        {
            List.Clear();
            PositionList.Clear();
            PositionList.Add(StockPane);
            Layout();
        }

        internal void Draw(Graphics g, Rectangle clipRectangle)
        {
            GraphicsContainer container;
            foreach (Pane pane in PositionList)
            {
                if (clipRectangle.IntersectsWith(pane.Bounds))
                {
                    container = g.BeginContainer();
                    g.SetClip(pane.Bounds);
                    g.TranslateTransform(pane.Left, pane.Top);
                    pane.PrepareToDraw(g);
                    g.EndContainer(container);
                }
            }
            XAxisPane xAxisPane = XAxisPane;
            if (Chart.YAxisAlignment == YAxisAlignment.Right)
            {
                xAxisPane.Bounds = new Rectangle(0, xAxisPane.Top, Chart.ClientRect.Width - Chart.YAxisWidth,
                                                 xAxisPane.Height);
            }
            else xAxisPane.Bounds = Chart.YAxisAlignment == YAxisAlignment.Left ? 
                new Rectangle(Chart.YAxisWidth, xAxisPane.Top, Chart.ClientRect.Width - Chart.YAxisWidth, xAxisPane.Height) :
                new Rectangle(Chart.YAxisWidth, xAxisPane.Top, Chart.ClientRect.Width - (Chart.YAxisWidth * 2), xAxisPane.Height);
            if (clipRectangle.IntersectsWith(xAxisPane.Bounds))
            {
                container = g.BeginContainer();
                g.SetClip(xAxisPane.Bounds);
                g.TranslateTransform(xAxisPane.Left, xAxisPane.Top);
                xAxisPane.Draw(g);
                g.EndContainer(container);
            }
            foreach (Pane pane3 in PositionList)
            {
                if (clipRectangle.IntersectsWith(pane3.Bounds))
                {
                    container = g.BeginContainer();
                    g.SetClip(pane3.Bounds);
                    g.TranslateTransform(pane3.Left, pane3.Top);
                    pane3.Draw(g);
                    g.EndContainer(container);
                }
            }
        }

        internal void Layout()
        {
            if (PositionList.Count != 0)
            {
                NormalizePercentHeight();
                Rectangle clientRect = Chart.ClientRect;
                int num = clientRect.Height - (Chart.InterPaneGap * (PositionList.Count - 1));
                num -= Chart.InterPaneGap + XAxisPane.FixedHeight;
                if (num > 0)
                {
                    Pane pane;
                    int minimumPaneHeight;
                    Pane pane2 = null;
                    bool flag = false;
                    for (int i = 0; i < PositionList.Count; i++)
                    {
                        pane = (Pane)PositionList[i];
                        minimumPaneHeight = (int)Math.Round(((pane.PercentHeight * num) / 100f));
                        if (minimumPaneHeight < pane.MinimumPaneHeight)
                        {
                            pane.PercentHeight = ((pane.MinimumPaneHeight) / ((float)num)) * 100f;
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        NormalizePercentHeight();
                    }
                    var list = new ArrayList();
                    int num5 = -2147483648;
                    int num6 = -1;
                    int num3 = 0;
                    for (int j = 0; j < PositionList.Count; j++)
                    {
                        pane = (Pane)PositionList[j];
                        minimumPaneHeight = (int)Math.Round(((pane.PercentHeight * num) / 100f));
                        if (minimumPaneHeight < pane.MinimumPaneHeight)
                        {
                            minimumPaneHeight = pane.MinimumPaneHeight;
                        }
                        list.Add(minimumPaneHeight);
                        num3 += minimumPaneHeight;
                        if (minimumPaneHeight >= num5)
                        {
                            num5 = minimumPaneHeight;
                            num6 = list.Count - 1;
                            pane2 = pane;
                        }
                    }
                    if (num3 > num)
                    {
                        list[num6] = ((int)list[num6]) - (num3 - num);
                        if (((int)list[num6]) < pane2.MinimumPaneHeight)
                        {
                            list[num6] = pane2.MinimumPaneHeight;
                        }
                    }
                    if (num3 < num)
                    {
                        list[list.Count - 1] = ((int)list[list.Count - 1]) + (num - num3);
                    }
                    int y = 0;
                    num3 = 0;
                    for (int k = 0; k < PositionList.Count; k++)
                    {
                        pane = (Pane)PositionList[k];
                        pane.Location = new Point(0, y);
                        pane.Size = new Size(clientRect.Width, (int)list[k]);
                        y += ((int)list[k]) + Chart.InterPaneGap;
                        num3 += (int)list[k];
                    }
                    XAxisPane.Location = new Point(0, y);
                    XAxisPane.Size = new Size(clientRect.Width, XAxisPane.FixedHeight);
                    Chart.Invalidate();
                }
            }
        }

        internal void NormalizePercentHeight()
        {
            if (PositionList.Count != 0)
            {
                float num = 0f;
                foreach (Pane pane in PositionList)
                {
                    num += pane.PercentHeight;
                }
                if (num < float.Epsilon)
                {
                    StockPane.PercentHeight = 100f;
                }
                else
                {
                    foreach (Pane pane2 in PositionList)
                    {
                        pane2.PercentHeight = (pane2.PercentHeight / num) * 100f;
                    }
                }
            }
        }

        internal void RecalculatePercentHeight()
        {
            if (PositionList.Count != 0)
            {
                int num = Chart.ClientRect.Height - (Chart.InterPaneGap * (PositionList.Count - 1));
                num -= Chart.InterPaneGap + XAxisPane.FixedHeight;
                foreach (Pane pane in PositionList)
                {
                    pane.PercentHeight = ((pane.Height) / ((float)num)) * 100f;
                }
            }
        }

        public void Remove(Pane pane)
        {
            List.Remove(pane);
            PositionList.Remove(pane);
            Chart.PerformLayout();
        }

        public void Remove(string name)
        {
            Pane pane = this[name];
            if (pane != null)
            {
                Remove(pane);
            }
        }

        //private void RemoveAt(int index)
        //{
        //}

        // Properties
    }
}
