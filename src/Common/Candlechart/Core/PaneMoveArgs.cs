using System;
using System.Drawing;

namespace Candlechart.Core
{
    public class PaneMoveEventArgs : EventArgs
    {
        // Fields
        private readonly Pane _pane;

        // Methods
        public PaneMoveEventArgs(Pane pane)
        {
            _pane = pane;
        }

        // Properties
        public Pane Pane
        {
            get { return _pane; }
        }
    }
    public delegate void PaneMoveEventHandler(object sender, PaneMoveEventArgs e);
    public enum PaneMoveStyle
    {
        Outline,
        Blend
    }
    public class PaneResizeEventArgs : EventArgs
    {
        // Fields
        private readonly Pane _adjacentPane;
        private readonly Pane _affectedPane;

        // Methods
        public PaneResizeEventArgs(Pane affectedPane, Pane adjacentPane)
        {
            _affectedPane = affectedPane;
            _adjacentPane = adjacentPane;
        }

        // Properties
        public Pane AdjacentPane
        {
            get { return _adjacentPane; }
        }

        internal int AdjacentPaneInitialHeight { get; set; }

        public Pane AffectedPane
        {
            get { return _affectedPane; }
        }

        internal int AffectedPaneInitialHeight { get; set; }

        internal Point Current { get; set; }

        internal Point Start { get; set; }
    }
    public delegate void PaneResizeEventHandler(object sender, PaneResizeEventArgs e);
    public enum PaneResizeStyle
    {
        Outline,
        Instantaneous
    }    
}
