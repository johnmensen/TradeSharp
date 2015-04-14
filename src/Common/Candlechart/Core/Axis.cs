using System;
using Candlechart.Chart;
using Candlechart.Theme;
using System.Drawing;

namespace Candlechart.Core
{
    public abstract class Axis
    {
        private Font _font;

        protected Axis(Pane owner)
        {
            Owner = owner;
        }

        public abstract Color BackColor { get; }

        internal ChartControl Chart
        {
            get { return Owner.Owner; }
        }

        public abstract Font Font { get; }

        public abstract Color ForeColor { get; }

        public abstract Color TextColor { get; }

        public abstract Color GridBandColor { get; }

        public abstract bool GridBandVisible { get; }

        public abstract Color GridLineColor { get; }

        public abstract GridLineStyle GridLineStyle { get; }

        public abstract bool GridLineVisible { get; }

        internal Pane Owner { get; private set; }

        
        internal abstract void Draw(Graphics g);
    }        
}
