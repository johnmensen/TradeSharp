using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleGlassOptions")]
    [LocalizedCategory("TitleStockIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public partial class IndicatorOptionDom : BaseChartIndicator, IChartIndicator
    {
        #region Настройки

        [LocalizedDisplayName("TitleTicker")]
        [Description("Тикер - берется автоматом для ряда пар (6E, 6B ...)")]
        [LocalizedCategory("TitleMain")]
        public string Ticker { get; set; }

        private Color clCall = Color.Green;
        [LocalizedDisplayName("TitleCallColor")]
        [Description("Цвет CALL-опционных уровней")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClCall
        {
            get { return clCall; }
            set { clCall = value; }
        }

        private Color clPut = Color.Red;
        [LocalizedDisplayName("TitlePutColor")]
        [Description("Цвет PUT-опционных уровней")]
        [LocalizedCategory("TitleVisuals")]
        public Color ClPut
        {
            get { return clPut; }
            set { clPut = value; }
        }

        [LocalizedDisplayName("TitleCreateDrawingPanel")]
        [LocalizedCategory("TitleMain")]
        [Description("Создавать свою панель отрисовки")]
        public override bool CreateOwnPanel { get; set; }

        private string cmeUserName = "AndreySitaev";
        
        [LocalizedDisplayName("TitleLogin")]
        [Description("Имя пользователя в cmegroup")]
        [Category("CME")]
        public string CmeUserName
        {
            get { return cmeUserName; }
            set { cmeUserName = value; }
        }

        private string cmePassword = "Master2005";

        [LocalizedDisplayName("TitlePassword")]
        [Description("Пароль учетной записи в cmegroup")]
        [Category("CME")]
        public string CmePassword
        {
            get { return cmePassword; }
            set { cmePassword = value; }
        }

        #endregion

        #region Данные

        private static readonly Dictionary<string, string> tickerByPair = new Dictionary<string, string>
                                                                              {
                                                                                  { "EURUSD", "XT" }
                                                                              };

        private readonly SeriesSpanWithText seriesSpan = new SeriesSpanWithText(Localizer.GetString("TitleLevelsOrSegments"));

        #endregion

        public override BaseChartIndicator Copy()
        {
            var ma = new IndicatorOptionDom();
            Copy(ma);
            return ma;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ma = (IndicatorOptionDom)indi;
            CopyBaseSettings(ma);
            ma.Ticker = Ticker;
            ma.ClCall = ClCall;
            ma.ClPut = ClPut;
            ma.CmeUserName = CmeUserName;
            ma.CmePassword = CmePassword;
        }

        public override string Name
        {
            get { return Localizer.GetString("TitleGlassOptions"); }
        }

        public void BuildSeries(ChartControl chart)
        {
            var ticker = GetTickerName();
            if (string.IsNullOrEmpty(ticker)) return;
            LoadCMEData(ticker);
        }

        private void LoadCMEData(string ticker)
        {
            // получить разметку
            var url =
                string.Format(
                    "http://datasuite.cmegroup.com/dataSuite.html?template=opt&productCode={0}&exchange=XCME&selected_tab=fx",                    
                    ticker);
            var xml = QueryPage(url, false);
            if (string.IsNullOrEmpty(xml)) return;

            // парсить разметку
        }

        private string GetTickerName()
        {
            if (!string.IsNullOrEmpty(Ticker)) return Ticker;
            var symbol = owner.Symbol;
            string ticker;
            tickerByPair.TryGetValue(symbol, out ticker);
            return ticker;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            SeriesResult = new List<Series.Series> { seriesSpan };
            EntitleIndicator();
        }

        public void Remove()
        {
            if (seriesSpan != null) seriesSpan.data.Clear();
        }

        public void AcceptSettings()
        {
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (updatedCandle == null && (newCandles == null || newCandles.Count == 0)) return;
            // проверить на мин. интервал и обновить график
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            return string.Empty;
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
    }
}
