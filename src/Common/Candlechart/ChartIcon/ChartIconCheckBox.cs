using System;
using Candlechart.Chart;

namespace Candlechart.ChartIcon
{
    /// <summary>
    /// имеет два состояния: Вкл и Выкл
    /// меняет состояние после клика
    /// каждому состоянию соответствует своя фигура
    /// </summary>
    public class ChartIconCheckBox : ChartIcon
    {
        public delegate void OnClickDel(ChartIcon sender, out bool newState);

        public OnClickDel clickHandler;

        public bool IconChecked
        {
            get { return imageIndex > 0; }
            set { imageIndex = value ? 1 : 0; }
        }

        public Func<ChartControl, bool> getStateFromChart;

        public ChartIconCheckBox(OnClickDel clickHandler)
            : base((sender, args) => { })
        {
            this.clickHandler = clickHandler;
        }

        public override ChartIcon MakeCopy()
        {
            var cpy = new ChartIconCheckBox(null) { key = key, State = State, imageIndex = imageIndex, Size = Size };
            return cpy;
        }

        public override void OnMouseClick()
        {
            if (clickHandler != null)
            {
                bool newState;
                clickHandler(this, out newState);
                IconChecked = newState;
                return;
            }
            
            IconChecked = !IconChecked;
        }

        public void GetStateFromChartObject(ChartControl chart)
        {
            if (getStateFromChart != null)
                IconChecked = getStateFromChart(chart);
        }
    }
}
