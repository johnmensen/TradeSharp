using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Candlechart.Chart;
using Candlechart.ChartMath;

namespace Candlechart.Core
{
    public class InteractivityToolCollection : CollectionBase
    {
        // Fields
        private readonly PaneFrameTool _paneFrameTool;

        // Methods
        internal InteractivityToolCollection(ChartControl owner)
        {
            Owner = owner;
            _paneFrameTool = new PaneFrameTool(this);
        }

        internal InteractivityTool CaptureTool { get; set; }

        private ChartControl Chart
        {
            get { return Owner; }
        }

        internal Pane CurrentPane { get; set; }

        public InteractivityTool this[string name]
        {
            get
            {
                foreach (InteractivityTool tool in InnerList)
                {
                    if (tool.Name == name)
                    {
                        return tool;
                    }
                }
                return null;
            }
        }

        internal ChartControl Owner { get; set; }

        public PaneFrameTool PaneFrameTool
        {
            get { return _paneFrameTool; }
        }

        public void Add(InteractivityTool tool)
        {
            List.Add(tool);
            tool.Owner = this;
        }

        private MouseEventArgs ConvertMouseEventArgs(MouseEventArgs e, Pane pane)
        {
            if (pane != null)
            {
                Point pt = Chart.PointToScreen(new Point(e.X, e.Y));
                pt = pane.PointToClient(pt);
                return new MouseEventArgs(e.Button, e.Clicks, pt.X, pt.Y, e.Delta);
            }
            return e;
        }

        public void DisableAll()
        {
            foreach (InteractivityTool tool in List)
            {
                tool.Enabled = false;
            }
        }

        public void EnableAll()
        {
            foreach (InteractivityTool tool in List)
            {
                tool.Enabled = true;
            }
        }

        internal Pane FindPane(int x, int y)
        {
            foreach (Pane pane in Chart.Panes.PositionList)
            {
                if (Conversion.ChildToParent(pane.Bounds, Chart.ClientRect).Contains(x, y))
                {
                    return pane;
                }
            }
            return null;
        }

        internal void HandleMouseDown(MouseEventArgs e)
        {
            if (CaptureTool != null)
            {
                MouseEventArgs args = ConvertMouseEventArgs(e, CurrentPane);
                CaptureTool.HandleMouseDown(args);
            }
            else
            {
                CurrentPane = FindPane(e.X, e.Y);
                MouseEventArgs args2 = ConvertMouseEventArgs(e, CurrentPane);
                if (!PaneFrameTool.HandleMouseDown(args2))
                {
                    foreach (InteractivityTool tool in List)
                    {
                        if (tool.Enabled)
                        {
                            tool.HandleMouseDown(args2);
                            if (CaptureTool != null)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal void HandleMouseMove(MouseEventArgs e)
        {
            if (CaptureTool != null)
            {
                MouseEventArgs args = ConvertMouseEventArgs(e, CurrentPane);
                CaptureTool.HandleMouseMove(args);
            }
            else
            {
                CurrentPane = FindPane(e.X, e.Y);
                MouseEventArgs args2 = ConvertMouseEventArgs(e, CurrentPane);
                if (!PaneFrameTool.HandleMouseMove(args2))
                {
                    foreach (InteractivityTool tool in List)
                    {
                        if (tool.Enabled)
                        {
                            tool.HandleMouseMove(args2);
                        }
                    }
                }
            }
        }

        internal void HandleMouseUp(MouseEventArgs e)
        {
            if (CaptureTool != null)
            {
                MouseEventArgs args = ConvertMouseEventArgs(e, CurrentPane);
                CaptureTool.HandleMouseUp(args);
            }
            else
            {
                CurrentPane = FindPane(e.X, e.Y);
                MouseEventArgs args2 = ConvertMouseEventArgs(e, CurrentPane);
                if (!PaneFrameTool.HandleMouseUp(args2))
                {
                    foreach (InteractivityTool tool in List)
                    {
                        if (tool.Enabled)
                        {
                            tool.HandleMouseUp(args2);
                        }
                    }
                }
            }
        }

        internal void HandleMouseWheel(MouseEventArgs e)
        {
            if (CaptureTool != null)
            {
                MouseEventArgs args = ConvertMouseEventArgs(e, CurrentPane);
                CaptureTool.HandleMouseWheel(args);
            }
            else
            {
                CurrentPane = FindPane(e.X, e.Y);
                MouseEventArgs args2 = ConvertMouseEventArgs(e, CurrentPane);
                if (!PaneFrameTool.HandleMouseWheel(args2))
                {
                    foreach (InteractivityTool tool in List)
                    {
                        if (tool.Enabled)
                        {
                            tool.HandleMouseWheel(args2);
                        }
                    }
                }
            }
        }

        public void Remove(InteractivityTool tool)
        {
            List.Remove(tool);
        }

        public void Remove(string name)
        {
            InteractivityTool tool = this[name];
            if (tool != null)
            {
                Remove(tool);
            }
        }
    }
}
