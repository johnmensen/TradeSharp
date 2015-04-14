using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Candlechart.Chart;
using Candlechart.Core;
using Candlechart.Series;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart.Indicator
{
    [LocalizedDisplayName("TitleTradeSharpVolumes")]
    [LocalizedCategory("TitleTechnicalAnalysisShort")]
    public class IndicatorTradeSharpVolumes : BaseChartIndicator, IChartIndicator
    {
        static class TradeSharpVolumeParser
        {
            private static readonly char[] tickerSeparator = new [] { ';' };
            private static readonly char[] tickerPartSeparator = new[] { ':' };

            /// <summary>
            /// Формат строки "[#fmt]#&newstype=TradeSharpVolumes#&AUDJPY:30000:0;AUDUSD:7620000:0;EURCHF:0:4610000"
            /// </summary>        
            public static Dictionary<string, Cortege2<long, long>> Parse(string str)
            {
                if (string.IsNullOrEmpty(str)) return null;
                if (!str.StartsWith("[#fmt]#&newstype=TradeSharpVolumes#&")) return null;
                str = str.Substring("[#fmt]#&newstype=TradeSharpVolumes#&".Length);

                var parts = str.Split(tickerSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return null;

                var dic = new Dictionary<string, Cortege2<long, long>>();

                foreach (var part in parts)
                {
                    var keyValue = part.Split(
                        tickerPartSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length != 3) continue;

                    var ticker = keyValue[0];
                    var volmBuy = keyValue[1].ToLongSafe();
                    var volmSell = keyValue[2].ToLongSafe();
                    if (volmBuy.HasValue && volmSell.HasValue && !dic.ContainsKey(ticker))
                        dic.Add(ticker, new Cortege2<long, long>(volmBuy.Value, volmSell.Value));
                }
                return dic;
            }
        }

        public override BaseChartIndicator Copy()
        {
            var fut = new IndicatorTradeSharpVolumes();
            Copy(fut);
            return fut;
        }

        public override void Copy(BaseChartIndicator indi)
        {
            var fut = (IndicatorTradeSharpVolumes)indi;
            CopyBaseSettings(fut);
            fut.volumeData = volumeData.ToList();
            fut.ChannelId = ChannelId;
            fut.ColorBuy = ColorBuy;
            fut.ColorSell = ColorSell;
            fut.needRebuildLevels = needRebuildLevels;
            fut.seriesBuy = seriesBuy;
            fut.seriesSell = seriesSell;
        }

        [Browsable(false)]
        public override string Name { get { return Localizer.GetString("TitleTradeSharpVolumes"); } }

        private List<Cortege3<DateTime, long, long>> volumeData = new List<Cortege3<DateTime, long, long>>();

        private int channelId = 4;
        [LocalizedDisplayName("TitleChannel")]
        [Description("ID канала, содержащего данные об объемах")]
        [LocalizedCategory("TitleMain")]
        public int ChannelId
        {
            get { return channelId; }
            set { channelId = value; }
        }

        private Color colorBuy = Color.Green;
        [LocalizedDisplayName("TitleBuyLineColor")]
        [Description("Цвет линии BUY")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorBuy
        {
            get { return colorBuy; }
            set { colorBuy = value; }
        }

        private Color colorSell = Color.Red;
        [LocalizedDisplayName("TitleSellLineColor")]
        [Description("Цвет линии SELL")]
        [LocalizedCategory("TitleVisuals")]
        public Color ColorSell
        {
            get { return colorSell; }
            set { colorSell = value; }
        }

        /// <summary>
        /// взводится при изменении параметров индюка,
        /// обновления списка volumeData...
        /// если всзведен - BuildSeries() обновляет данные (series...)
        /// </summary>
        private bool needRebuildLevels = true;

        private BarSettings previousTimeframe;

        private LineSeries seriesBuy = new LineSeries("BUY") { Transparent = true };
        private LineSeries seriesSell = new LineSeries("SELL") { Transparent = true };

        public void BuildSeries(ChartControl chart)
        {
            if (chart.Timeframe == previousTimeframe && !needRebuildLevels) return;
            previousTimeframe = chart.Timeframe;

            var candles = chart.StockSeries.Data.Candles;
            if (candles.Count == 0) return;

            // добавить данные об объемах
            BuildSeries();
            needRebuildLevels = false;
        }

        public void Add(ChartControl chart, Pane ownerPane)
        {
            owner = chart;
            // инициализируем индикатор
            SeriesResult = new List<Series.Series> { seriesBuy, seriesSell };
            EntitleIndicator();

            needRebuildLevels = true;
            owner.Owner.OnNewsReceived += OnNewsReceived;
            
            // показать уже существующие объемы
            List<News> lstNews = null;
            if (owner.Owner.getAllNewsByChannel != null)
                lstNews = owner.Owner.getAllNewsByChannel(channelId);
            
            if (lstNews != null)
                if (lstNews.Count > 0)
                    ProcessNews(lstNews.ToArray());
        }

        private void BuildSeries()
        {
            if (DrawPane != null && DrawPane != owner.StockPane)
                DrawPane.Title = UniqueName;
            if (volumeData.Count == 0) return;
            if (owner.StockSeries.Data.Count == 0) return;
            previousTimeframe = owner.Timeframe;

            seriesBuy.Data.Clear();
            seriesSell.Data.Clear();

            var lstVolm = new List<Cortege3<int, long, long>>();
            
            foreach (var volData in volumeData)
            {
                var index = owner.StockSeries.GetIndexByCandleOpen(volData.a);
                if (index <= 0) continue;

                lstVolm.Add(new Cortege3<int, long, long>(index, volData.b, volData.c));
            }

            // интерполировать
            if (lstVolm.Count > 0)
            {
                var lastIndex = 0;
                foreach (var ptVol in lstVolm)
                {
                    if (ptVol.a >= owner.StockSeries.Data.Count) break;
                    for (var i = lastIndex; i <= ptVol.a; i++)
                    {
                        seriesBuy.Data.Add(ptVol.b);
                        seriesSell.Data.Add(ptVol.c);
                    }
                    lastIndex = ptVol.a + 1;
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

            var lastProcessedNewsDate = volumeData.Count == 0 ? default(DateTime) : volumeData[volumeData.Count - 1].a;
            // добавить уровни
            foreach (var ns in news)
            {
                if (ns.ChannelId != channelId || string.IsNullOrEmpty(ns.Body) || ns.Time <= lastProcessedNewsDate) continue;
                var volData = TradeSharpVolumeParser.Parse(ns.Body);
                if (volData == null) continue;
                
                // проверить - совпадает ли базовый актив
                Cortege2<long, long> volBuySell;
                if (!volData.TryGetValue(owner.Symbol, out volBuySell))
                    continue;
                volumeData.Add(new Cortege3<DateTime, long, long>(ns.Time, volBuySell.a,
                    volBuySell.b));
            }
            volumeData = volumeData.OrderBy(v => v.a).ToList();
            BuildSeries(owner);
        }

        public void Remove()
        {
            owner.Owner.OnNewsReceived -= OnNewsReceived;
        }

        public void AcceptSettings()
        {
            needRebuildLevels = true;
            seriesBuy.LineColor = colorBuy;
            seriesSell.LineColor = colorSell;
        }

        public void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles)
        {
            if (newCandles != null)
                if (newCandles.Count > 0)
                    BuildSeries(owner);
        }

        public string GetHint(int x, int y, double index, double price, int tolerance)
        {
            if (volumeData.Count == 0) return string.Empty;
            var date = owner.StockSeries.GetCandleOpenTimeByIndex((int) index);
            var volm = volumeData.FirstOrDefault(d => d.a >= date);
            if (volm.a == default(DateTime)) return string.Empty;

            return "BUY: " + volm.b.ToStringUniformMoneyFormat() +
                ", SELL: " + volm.c.ToStringUniformMoneyFormat();
        }

        public List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            return new List<IChartInteractiveObject>();
        }
    }
}
