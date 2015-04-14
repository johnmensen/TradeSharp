using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Entity;
using TradeSharp.Robot.BL;

namespace TradeSharp.Robot.Robot
{
    public class DiversByTimeframe
    {
        public BarSettings timeframe = new BarSettings { Intervals = new List<int> { 60 } };
        
        [Category("Дивергенции")]
        [PropertyXMLTag("DiversByTimeframe.Timeframe")]
        [DisplayName("Таймфрейм")]
        [Description("Таймфрейм")]
        [Editor(typeof(TimeframeUITypeEditor), typeof(UITypeEditor))]
        public string Timeframe
        {
            get { return BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe); }
            set { timeframe = BarSettingsStorage.Instance.GetBarSettingsByName(value); }
        }

        private List<IndexDivergencyInfo> indexList = new List<IndexDivergencyInfo>();

        [Category("Дивергенции")]
        [PropertyXMLTag("DiversByTimeframe.Divers")]
        [DisplayName("Дивергенции")]
        [Description("Дивергенции с польз. индексами")]
        public List<IndexDivergencyInfo> IndexList
        {
            get { return indexList; }
            set { indexList = value; }
        }

        public DiversByTimeframe() {}

        public DiversByTimeframe(DiversByTimeframe div)
        {
            timeframe = new BarSettings(div.timeframe);
            foreach (var ind in div.IndexList)
            {
                var cpy = new IndexDivergencyInfo(ind);
                indexList.Add(cpy);
            }
        }
    }    
}
