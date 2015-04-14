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
    [LocalizedDisplayName("TitleIndexPublication")]
    [LocalizedCategory("TitleStockIndicatorsShort")]
    [TypeConverter(typeof(PropertySorter))]
    public class IndicatorIndexVector : BaseChartIndicator, IChartIndicator
    {
        public override BaseChartIndicator Copy()
        {
            var ind = new IndicatorIndexVector();
            Copy(ind);
            return ind;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var ind = (IndicatorIndexVector)indi;
            CopyBaseSettings(ind);
            ind.indexData.AddRange(indexData);
            ind.ChannelId = ChannelId;
            ind.Color1 = Color1;
            ind.Color2 = Color2;
            ind.Color3 = Color3;
            ind.Color4 = Color4;
            ind.Color5 = Color5;
            ind.Index1 = Index1;
            ind.Index2 = Index2;
            ind.Index3 = Index3;
            ind.Index4 = Index4;
            ind.Index5 = Index5;
            ind.needRebuildLevels = needRebuildLevels;
            ind.NewsTitleFilter = NewsTitleFilter;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleIndexPublication"); } }

        private List<IndexVectorNewsRecord> indexData = new List<IndexVectorNewsRecord>();

        private int channelId = 3;
        [LocalizedDisplayName("TitleChannel")]
        [Description("ID канала, содержащего данные по индексам")]
        [LocalizedCategory("TitleMain")]
        public int ChannelId
        {
            get { return channelId; }
            set { channelId = value; }
        }

        private string newsTitleFilter = "Yahoo Index";
        [LocalizedDisplayName("TitleFilterByTitle")]
        [Description("Фильтр индексов по заголовку новости")]
        [LocalizedCategory("TitleMain")]
        public string NewsTitleFilter
        {
            get { return newsTitleFilter; }
            set { newsTitleFilter = value; }
        }

        #region Индексы
        private readonly string[] indiciesToDisplay = new [] { "", "", "", "", "" };
        [LocalizedDisplayName("TitleIndex1")]
        [Description("Отображаемый индекс 1")]
        [LocalizedCategory("TitleMain")]
        public string Index1 { get { return indiciesToDisplay[0]; } set { indiciesToDisplay[0] = value; } }

        [LocalizedDisplayName("TitleIndex2")]
        [Description("Отображаемый индекс 2")]
        [LocalizedCategory("TitleMain")]
        public string Index2 { get { return indiciesToDisplay[1]; } set { indiciesToDisplay[1] = value; } }

        [LocalizedDisplayName("TitleIndex3")]
        [Description("Отображаемый индекс 3")]
        [LocalizedCategory("TitleMain")]
        public string Index3 { get { return indiciesToDisplay[2]; } set { indiciesToDisplay[2] = value; } }

        [LocalizedDisplayName("TitleIndex4")]
        [Description("Отображаемый индекс 4")]
        [LocalizedCategory("TitleMain")]
        public string Index4 { get { return indiciesToDisplay[3]; } set { indiciesToDisplay[3] = value; } }

        [LocalizedDisplayName("TitleIndex5")]
        [Description("Отображаемый индекс 5")]
        [LocalizedCategory("TitleMain")]
        public string Index5 { get { return indiciesToDisplay[4]; } set { indiciesToDisplay[4] = value; } }
        #endregion

        #region Визуальные
        private readonly Color[] colorsLine = new [] { Color.Maroon, Color.Blue, Color.Green, Color.Black, Color.Moccasin };

        [LocalizedDisplayName("TitleLineColor1")]
        [Description("Цвет линии 1")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color1
        {
            get { return colorsLine[0]; }
            set { colorsLine[0] = value; }
        }

        [LocalizedDisplayName("TitleLineColor2")]
        [Description("Цвет линии 2")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color2
        {
            get { return colorsLine[1]; }
            set { colorsLine[1] = value; }
        }

        [LocalizedDisplayName("TitleLineColor3")]
        [Description("Цвет линии 3")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color3
        {
            get { return colorsLine[2]; }
            set { colorsLine[2] = value; }
        }

        [LocalizedDisplayName("TitleLineColor4")]
        [Description("Цвет линии 4")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color4
        {
            get { return colorsLine[3]; }
            set { colorsLine[3] = value; }
        }

        [LocalizedDisplayName("TitleLineColor5")]
        [Description("Цвет линии 5")]
        [LocalizedCategory("TitleVisuals")]
        public Color Color5
        {
            get { return colorsLine[4]; }
            set { colorsLine[4] = value; }
        }

        #endregion

        /// <summary>
        /// взводится при изменении параметров индюка,
        /// если взведен - BuildSeries() обновляет данные (series...)
        /// </summary>
        private bool needRebuildLevels = true;

        private readonly LineSeries[] seriesIndicies = new [] 
            { 
                new LineSeries(Localizer.GetString("TitleIndex1")) { ShiftX = 1 }, 
                new LineSeries(Localizer.GetString("TitleIndex2")) { ShiftX = 1 }, 
                new LineSeries(Localizer.GetString("TitleIndex3")) { ShiftX = 1 }, 
                new LineSeries(Localizer.GetString("TitleIndex4")) { ShiftX = 1 }, 
                new LineSeries(Localizer.GetString("TitleIndex5")) { ShiftX = 1 }
            };

        public void BuildSeries(ChartControl chart)
        {
            if (!needRebuildLevels) return;

            foreach (var series in seriesIndicies)
                series.Data.Clear();

            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;
            
            BuildSeries();
            needRebuildLevels = false;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            SeriesResult = seriesIndicies.Cast<Series.Series>().ToList();
            EntitleIndicator();

            needRebuildLevels = true;
            owner.Owner.OnNewsReceived += OnNewsReceived;
            // показать уже существующие индексы
            List<News> lstNews;
            NewsStorage.Instance.ReadNews(channelId, out lstNews);
            if (lstNews != null)
                if (lstNews.Count > 0)
                    ProcessNews(lstNews.ToArray());
        }

        private void BuildSeries()
        {
            if (DrawPane != null && DrawPane != owner.StockPane) DrawPane.Title = UniqueName;
            if (indexData.Count == 0) return;
            if (owner.StockSeries.Data.Count == 0) return;

            var lstIndex = indiciesToDisplay.Where(ind => !string.IsNullOrEmpty(ind)).ToDictionary(
                ind => ind, ind => new List<Cortege2<int, decimal>>());

            foreach (var indVector in indexData)
            {
                var index = owner.StockSeries.GetIndexByCandleOpen(indVector.date);
                if (index <= 0) continue;
                for (var i = 0; i < indVector.indexNames.Length; i++)
                {
                    List<Cortege2<int, decimal>> list;
                    lstIndex.TryGetValue(indVector.indexNames[i], out list);
                    if (list != null) list.Add(new Cortege2<int, decimal>(index, indVector.indexValues[i]));
                }
                    
            }
            // интерполировать
            var seriesIndex = -1;
            foreach (var list in lstIndex)
            {
                seriesIndex++;
                if (list.Value.Count == 0) continue;                
                var prevIndex = -1;
                foreach (var ptVol in list.Value)
                {                    
                    if (ptVol.a >= owner.StockSeries.Data.Count) break;
                    if (ptVol.a == prevIndex) continue;

                    var range = ptVol.a - prevIndex;
                    for (var i = 0; i < range; i++)
                        seriesIndicies[seriesIndex].Data.Add((double)ptVol.b);
                    prevIndex = ptVol.a;                    
                }                
            }            
        }

        private void OnNewsReceived(News[] news)
        {
            ProcessNews(news);
        }

        private void ProcessNews(News[] news)
        {
            if (news.Length == 0) return;
            needRebuildLevels = true;

            // insert into NEWS (Channel, DateNews, Title, Body) 
            // values(3, '13.10.2010 17:00', 'Yahoo Index', 
            // '[#fmt]#&newstype=yahoo_index#&publishdate=13.10.2010 17:00:00#&CRB=300.55#&SP500A=
            // 1179.37#&DJIA30=11120.11')
            foreach (var ns in news)
            {
                if (ns.ChannelId != channelId || string.IsNullOrEmpty(ns.Body)) continue;
                if (!string.IsNullOrEmpty(NewsTitleFilter))
                    if (ns.Title != NewsTitleFilter) continue;
                var indVector = IndexVectorNewsRecord.ParseNews(ns.Body, ns.Time);
                if (indVector != null) indexData.Add(indVector);
            }
            indexData = indexData.OrderBy(v => v.date).ToList();
            BuildSeries(owner);
        }

        public void Remove()
        {
            owner.Owner.OnNewsReceived -= OnNewsReceived;
        }

        public void AcceptSettings()
        {
            needRebuildLevels = true;
            for (var i = 0; i < seriesIndicies.Length; i++)
                seriesIndicies[i].LineColor = colorsLine[i];                
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
