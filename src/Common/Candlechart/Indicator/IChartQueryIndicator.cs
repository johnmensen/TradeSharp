using System.Collections.Generic;

namespace Candlechart.Indicator
{
    /// <summary>
    /// индюк берет данные других графиков
    /// </summary>
    public interface IChartQueryIndicator
    {
        event GetOuterChartsDel GetOuterCharts;
    }

    public delegate void GetOuterChartsDel(List<string> chartIds, 
        out List<CandleChartControl> charts);
}
