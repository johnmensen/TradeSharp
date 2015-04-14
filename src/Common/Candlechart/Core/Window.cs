using System;
using Candlechart.Chart;

namespace Candlechart.Core
{
    internal class Window
    {
        private double _left;
        private int _leftBars;
        private int _leftPos;
        private double _right;
        private int _rightBars;
        private int _rightPos;

        public Window(ChartControl owner)
        {
            Owner = owner;
            Reset();
        }

        private ChartControl Chart
        {
            get { return Owner; }
        }

        public static int InitialXExtent
        {
            get { return 15; }
        }

        public static int InitialYExtent
        {
            get { return 2; }
        }

        public double Left
        {
            get { return _left; }
        }

        public int LeftBars
        {
            get { return _leftBars; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("LeftBars", "LeftBars must not be negative.");
                }
                _leftBars = value;
            }
        }

        public int LeftPos
        {
            get { return _leftPos; }
            private set
            {                
                _leftPos = value;
                if (_leftPos > _rightPos)
                {
                    _leftPos = _rightPos;
                }
                if (_leftPos < MinimumPos)
                {
                    _leftPos = MinimumPos;
                }
                _left = Chart.StockSeries.GetBarLeftEdge(LeftPos);                                    
            }
        }

        public int MaximumPos { get; private set; }

        public int MinimumPos { get; private set; }

        private ChartControl Owner { get; set; }

        public double Right
        {
            get { return _right; }
        }

        public int RightBars
        {
            get { return _rightBars; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("RightBars", "RightBars must not be negative.");
                }
                _rightBars = value;
            }
        }

        public int RightPos
        {
            get { return _rightPos; }
            private set
            {
                _rightPos = value;
                if (_rightPos < _leftPos)
                {
                    _rightPos = _leftPos;
                }
                if (_rightPos > MaximumPos)
                {
                    _rightPos = MaximumPos;
                }
                _right = Chart.StockSeries.GetBarRightEdge(RightPos);                
            }
        }

        public double Width
        {
            get { return (_right - _left); }
        }

        public void Reset()
        {
            MinimumPos = -InitialXExtent - LeftBars;
            MaximumPos = InitialXExtent + RightBars;
            LeftPos = MinimumPos;
            RightPos = MaximumPos;
            Chart.OnViewChanged(new EventArgs());
        }

        public void ScrollBy(int amount)
        {
            if (amount == 0) return;
            int leftPos = LeftPos;
            int rightPos = RightPos;
            LeftPos += amount;
            RightPos += amount;
            if ((leftPos == LeftPos) || (rightPos == RightPos))
            {
                LeftPos = leftPos;
                RightPos = rightPos;
            }
            Chart.OnViewChanged(new EventArgs());
        }

        public void ScrollTo(int leftPosValue)
        {
            var frameWd = RightPos - LeftPos;
            _leftPos = leftPosValue;
            _rightPos = _leftPos + frameWd;
            
            if (_leftPos < ChartControl.LeftMargin)
            {
                _leftPos = 0;
                _rightPos = _leftPos + frameWd;
                if (_rightPos > Chart.RightMargin)
                    _rightPos = Chart.RightMargin;
            }
            else
                if (_rightPos > Chart.RightMargin)
                {
                    _rightPos = Chart.RightMargin;
                    _leftPos = _rightPos - frameWd;
                    if (_leftPos < ChartControl.LeftMargin) _leftPos = ChartControl.LeftMargin;
                }
            _left = Chart.StockSeries.GetBarLeftEdge(_leftPos);
            _right = Chart.StockSeries.GetBarRightEdge(_rightPos);

            Chart.OnViewChanged(new EventArgs());
        }

        public void ScrollToEnd()
        {
            if (RightPos >= MaximumPos) return;
            
            bool isEnd = false;
            while (!isEnd)
            {
                LeftPos++;
                RightPos++;
                if (RightPos >= MaximumPos)
                    isEnd = true;
            }
            Chart.OnViewChanged(new EventArgs());
        }

        public void SetScrollView(int leftPos, int rightPos)
        {            
            LeftPos = leftPos;
            RightPos = rightPos;
            Chart.OnViewChanged(new EventArgs());            
        }

        internal void UpdateScrollLimits(double left, double right)
        {
            var flag = false;
            if (right < left) right = left;
            MinimumPos = Chart.StockSeries.FindNearestRightBar(left) - LeftBars;
            MaximumPos = Chart.StockSeries.FindNearestLeftBar(right) + RightBars;
            if (MinimumPos > LeftPos)
            {
                LeftPos = MinimumPos;
                flag = true;
            }
            if (MaximumPos < RightPos)
            {
                RightPos = MaximumPos;
                flag = true;
            }
            if (flag)
            {
                Chart.OnViewChanged(new EventArgs());
            }
        }
    }    
}
