using System.Collections.Generic;
using System.Drawing;
using Candlechart.Core;
using Candlechart.Series;
using Entity;

namespace TradeSharp.Client.BL
{
    public class ChartWindowSettings : BaseWindowSettings
    {
        #region Chart-specific

        private string symbol;
        [PropertyXMLTag("Chart.Symbol")]
        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }

        private string uniqueId;
        [PropertyXMLTag("Chart.UniqueId")]
        public string UniqueId
        {
            get { return uniqueId; }
            set { uniqueId = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private string timeframe;
        [PropertyXMLTag("Chart.Timeframe")]
        public string Timeframe
        {
            get { return timeframe; }
            set
            {
                UserSettings.Instance.lastTimeModified.Touch();
                timeframe = value;
            }
        }

        private int barOffset;
        [PropertyXMLTag("Chart.BarOffset")]
        public int BarOffset
        {
            get { return barOffset; }
            set { barOffset = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private string theme;
        [PropertyXMLTag("Chart.Theme")]
        public string Theme
        {
            get { return theme; }
            set { theme = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private Color colorBarUp;
        [PropertyXMLTag("Chart.ColorBarUp")]
        public Color ColorBarUp
        {
            get { return colorBarUp; }
            set
            {
                colorBarUp = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }

        private Color colorBarDn;
        [PropertyXMLTag("Chart.ColorBarDn")]
        public Color ColorBarDn
        {
            get { return colorBarDn; }
            set { colorBarDn = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private Color colorShadowUp;
        [PropertyXMLTag("Chart.ColorShadowUp")]
        public Color ColorShadowUp
        {
            get { return colorShadowUp; }
            set
            {
                colorShadowUp = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }

        private Color colorShadowDn;
        [PropertyXMLTag("Chart.ColorShadowDn")]
        public Color ColorShadowDn
        {
            get { return colorShadowDn; }
            set { colorShadowDn = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private Color colorBackground;
        [PropertyXMLTag("Chart.ColorBackground")]
        public Color ColorBackground
        {
            get { return colorBackground; }
            set { colorBackground = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private double firstCandleIndex;
        [PropertyXMLTag("Chart.FirstCandleIndex")]
        public double FirstCandleIndex
        {
            get { return firstCandleIndex; }
            set { firstCandleIndex = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private double lastCandleIndex;
        [PropertyXMLTag("Chart.LastCandleIndex")]
        public double LastCandleIndex
        {
            get { return lastCandleIndex; }
            set { lastCandleIndex = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private bool showLastQuote;
        [PropertyXMLTag("Chart.ShowLastQuote")]
        public bool ShowLastQuote
        {
            get { return showLastQuote; }
            set { showLastQuote = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private bool autoScroll;
        [PropertyXMLTag("Chart.AutoScroll")]
        public bool AutoScroll
        {
            get { return autoScroll; }
            set
            {
                autoScroll = value;
                UserSettings.Instance.lastTimeModified.Touch(); 
            }
        }

        [PropertyXMLTag("Chart.LastTemplateName")]
        public string LastTemplateName { get; set; }

        private YAxisAlignment yalign = YAxisAlignment.Right;
        [PropertyXMLTag("Chart.YAxisAlignment")]
        public YAxisAlignment YAxisAlignment
        {
            get { return yalign; }
            set
            {
                yalign = value;
                UserSettings.Instance.lastTimeModified.Touch(); 
            }
        }

        private long tabPageId;
        [PropertyXMLTag("Chart.TabPage")]
        public long TabPageId
        {
            get { return tabPageId; }
            set { tabPageId = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private StockSeries.CandleDrawMode graphMode;
        [PropertyXMLTag("Chart.GraphMode")]
        public StockSeries.CandleDrawMode GraphMode
        {
            get { return graphMode; }
            set { graphMode = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private List<float> paneLocations = new List<float>();
        
        [PropertyXMLTag("Chart.PaneLocations")]
        public List<float> PaneLocations
        {
            get { return paneLocations; }
            set
            {
                paneLocations = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }

        #endregion

        #region Серии
        private SeriesSettings seriesSettings = new SeriesSettings();
        [PropertyXMLTag("SeriesSettings")]
        public SeriesSettings SeriesSettings
        {
            get { return seriesSettings; }
            set
            {
                seriesSettings = value;
                UserSettings.Instance.lastTimeModified.Touch();
            }
        }
        #endregion
    }
}
