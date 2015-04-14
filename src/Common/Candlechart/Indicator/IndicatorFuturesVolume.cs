using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleFuturesVolumes")]
    [LocalizedCategory("TitleStockIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorFuturesVolume : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var fut = new IndicatorFuturesVolume();
            Copy(fut);
            return fut;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var fut = (IndicatorFuturesVolume) indi;
            CopyBaseSettings(fut);
            fut.volumeData.AddRange(volumeData);
            fut.IndicatorVolumeType = IndicatorVolumeType;
            fut.IndicatorDisplayedLevels = IndicatorDisplayedLevels;
            fut.ChannelId = ChannelId;
            fut.ColorVolume = ColorVolume;
            fut.ColorOI = ColorOI;
            fut.needRebuildLevels = needRebuildLevels;
            fut.seriesVolume = seriesVolume;
            fut.seriesOI = seriesOI;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleFuturesVolumes"); } }

        private List<FuturesVolume> volumeData = new List<FuturesVolume>();        
        
        public enum DisplayedLevels
        {
            Все = 0, ОткрытыйИнтерес, Объем
        }

        public enum VolumeType
        {
            Summary = 0, Globex, RegularTradingHours
        }

        [LocalizedDisplayName("TitleVolumeType")]
        [Description("Тип объема (суммарный, глобекс, яма)")]
        [LocalizedCategory("TitleMain")]
        public VolumeType IndicatorVolumeType { get; set; }

        [LocalizedDisplayName("TitleLevels")]
        [Description("Выбор отображаемых уровней")]
        [LocalizedCategory("TitleMain")]
        public DisplayedLevels IndicatorDisplayedLevels { get; set; }

        private int channelId = 2;
        [LocalizedDisplayName("TitleChannel")]
        [Description("ID канала, содержащего биржевые данные")]
        [LocalizedCategory("TitleMain")]
        public int ChannelId
        {
            get { return channelId; }
            set { channelId = value; }
        }
        
        private Color colorVolume = Color.Green;
        [LocalizedDisplayName("TitleVolumeLineColor")]
        [Description("Цвет линии объема")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorVolume
        {
            get { return colorVolume; }
            set { colorVolume = value;  }
        }

        private Color colorOI = Color.Red;
        [LocalizedDisplayName("TitleOpenInterestLineColorShort")]
        [Description("Цвет линии открытого интереса")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorOI
        {
            get { return colorOI; }
            set { colorOI = value; }
        }

        /// <summary>
        /// взводится при изменении параметров индюка,
        /// обновления списка volumeData...
        /// если всзведен - BuildSeries() обновляет данные (series...)
        /// </summary>
        private bool needRebuildLevels = true;

        private LineSeries seriesVolume = new LineSeries(Localizer.GetString("TitleVolume")) { Transparent = true };
        private LineSeries seriesOI = new LineSeries(Localizer.GetString("TitleOpenInterestShort")) { Transparent = true };
        
        public void BuildSeries(ChartControl chart)
        {
            if (!needRebuildLevels) return;

            seriesVolume.Data.Clear();
            seriesOI.Data.Clear();

            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            
            // добавить опционные уровни
            BuildSeries();
            needRebuildLevels = false;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            SeriesResult = new List<Series.Series> { seriesVolume, seriesOI };
            EntitleIndicator();

            needRebuildLevels = true;
            owner.Owner.OnNewsReceived += OnNewsReceived;
            // показать уже существующие уровни
            List<News> lstNews;
            NewsStorage.Instance.ReadNews(channelId, out lstNews);
            if (lstNews != null)
                if (lstNews.Count > 0)
                    ProcessNews(lstNews.ToArray());
        }

        private void BuildSeries()
        {
            if (DrawPane != owner.StockPane)
                DrawPane.Title = UniqueName;
            if (volumeData.Count == 0) return;
            if (owner.StockSeries.Data.Count == 0) return;

            var lstVolm = new List<Point>();
            var lstOI = new List<Point>();

            foreach (var futuresVolume in volumeData)
            {
                var index = owner.StockSeries.GetIndexByCandleOpen(futuresVolume.Date);                
                if (index <= 0) continue;
                // объем
                if (IndicatorDisplayedLevels == DisplayedLevels.Объем || IndicatorDisplayedLevels == DisplayedLevels.Все)
                {
                    var volume = IndicatorVolumeType == VolumeType.Summary
                                     ? futuresVolume.VolumeGlobex + futuresVolume.VolumeRTH
                                     : IndicatorVolumeType == VolumeType.Globex
                                           ? futuresVolume.VolumeGlobex
                                           : futuresVolume.VolumeRTH;                    
                    lstVolm.Add(new Point(index, volume));                                               
                }
                // О.И.
                if (IndicatorDisplayedLevels == DisplayedLevels.ОткрытыйИнтерес || IndicatorDisplayedLevels == DisplayedLevels.Все)
                {
                    lstOI.Add(new Point(index, futuresVolume.OpenInterest));                    
                }
            }
            // интерполировать
            if (lstVolm.Count > 0)
            {
                var lastIndex = 0;
                foreach (var ptVol in lstVolm)
                {
                    if (ptVol.X >= owner.StockSeries.Data.Count) break;
                    for (var i = lastIndex; i <= ptVol.X; i++)
                        seriesVolume.Data.Add(ptVol.Y);
                    lastIndex = ptVol.X + 1;
                }                
            }
            if (lstOI.Count > 0)
            {
                var lastIndex = 0;
                foreach (var ptOI in lstOI)
                {
                    if (ptOI.X >= owner.StockSeries.Data.Count) break;
                    for (var i = lastIndex; i <= ptOI.X; i++)
                        seriesOI.Data.Add(ptOI.Y);
                    lastIndex = ptOI.X + 1;
                }
            }
        }

        void OnNewsReceived(News[] news)
        {
            ProcessNews(news);
        }

        private void ProcessNews(News[] news)
        {
            if (news.Length == 0) return;
            needRebuildLevels = true;
            // добавить уровни
            foreach (var ns in news)
            {
                if (ns.ChannelId != channelId || string.IsNullOrEmpty(ns.Body)) continue;
                var fut = FuturesVolume.Parse(ns.Body);
                if (fut == null) continue;
                // проверить - совпадает ли базовый актив
                if (fut.Ticker != owner.Symbol) continue;
                volumeData.Add(fut);
            }
            volumeData = volumeData.OrderBy(v => v.Date).ToList();
            BuildSeries(owner);
        }

        public void Remove()
        {
            owner.Owner.OnNewsReceived -= OnNewsReceived;            
        }

        public void AcceptSettings()
        {
            needRebuildLevels = true;
            seriesVolume.LineColor = colorVolume;
            seriesOI.LineColor = colorOI;
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles != null)
                if (newCandles.Count > 0)
                    BuildSeries(owner);
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
