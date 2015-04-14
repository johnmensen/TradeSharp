using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Indicator;

namespace Candlechart
{
    public partial class CandleChartControl
    {
        public class IndiEventArgs : EventArgs
        {
            public BaseChartIndicator indi;
            public IndiEventArgs(BaseChartIndicator indi)
            {
                this.indi = indi;
            }
        }
        public delegate void IndiEventHandler(object sender, IndiEventArgs pe);
        
        public delegate void ChartRaisedEventDel(CandleChartControl initiator);

        public delegate void ScriptMenuItemActivatedDel(CandleChartControl sender, PointD worldCoords, object script);

        public event IndiEventHandler IndiAddEvent;
        public event IndiEventHandler IndiEditEvent;
        public event IndiEventHandler IndiRemoveEvent;
        
        private ChartRaisedEventDel publishTradeSignal;
        public event ChartRaisedEventDel PublishTradeSignal
        {
            add { publishTradeSignal += value; }
            remove { publishTradeSignal -= value; }
        }

        private ChartRaisedEventDel makeNewTextSignal;
        public event ChartRaisedEventDel MakeNewTextSignal
        {
            add { makeNewTextSignal += value; }
            remove { makeNewTextSignal -= value; }
        }

        private ScriptMenuItemActivatedDel scriptMenuItemActivated;
        public event ScriptMenuItemActivatedDel ScriptMenuItemActivated
        {
            add { scriptMenuItemActivated += value; }
            remove { scriptMenuItemActivated -= value; }
        }

        private void SetupMenuImages()
        {
            contextMenu.ImageList = imageListGlypth;
            menuitemObjectsDlg.ImageKey = "ico_table_view.png";
            menuitemScaleIn.ImageKey = "ico_zoom_in.png";
            menuitemScaleOut.ImageKey = "ico_zoom_out.png";
            menuitemNewOrder.ImageKey = "icon_eur.png";
            menuitemSaveImage.ImageKey = "ico_save.png";
            menuitemScripts.ImageKey = "gear_16.png";
            menucategorySignal.ImageKey = "ico_signal.png";
            menuitemOrdersIndi.ImageKey = "ico_red_down_arrow.png";
            menuitemIndicatorsDlg.ImageKey = "ico_indicators.png";
            menuitemGoTo.ImageKey = "ico_find.png";
            menuitemSyncQuotes.ImageKey = "ico_patch.png";
        }
        
        public void ActivateIndiAddEvent(BaseChartIndicator indi)
        {
            timeUpdateIndicators.Touch();
            if (IndiAddEvent == null) return;
            var eventArg = new IndiEventArgs(indi);
            IndiAddEvent(this, eventArg);
        }

        public void ActivateIndiEditEvent(string oldName, BaseChartIndicator indi)
        {
            timeUpdateIndicators.Touch();
            if (IndiEditEvent == null) return;
            var eventArg = new IndiEventArgs(indi);
            IndiEditEvent(oldName, eventArg);
        }

        public void ActivateIndiRemoveEvent(BaseChartIndicator indi)
        {
            timeUpdateIndicators.Touch();
            if (IndiRemoveEvent == null) return;
            var eventArg = new IndiEventArgs(indi);
            IndiRemoveEvent(this, eventArg);
        }

        private void OnMenuItemObjects(object sender, EventArgs e)
        {
            ShowObjectsDialog();
        }

        private void MenuitemSaveImageClick(object sender, EventArgs e)
        {
            SaveAsImage();
        }

        private void MenuitemScaleInClick(object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void MenuitemScaleOutClick(object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void MenuitemPublishForecastClick(object sender, EventArgs e)
        {
            PublishForecast();
        }

        public void MenuitemIndicatorsDlgClick(object sender, EventArgs e)
        {
            ShowIndicatorsWindow();
            //SaveIndicatorSettings();
        }

        private void ShowIndicatorsWindow()
        {
            var dlg = new IndicatorsGridForm {indicators = indicators, owner = this};
            dlg.onIndicatorAdd += AddNewIndicator;
            dlg.onIndicatorUpdate +=
                delegate(string oldName, IChartIndicator indi)
                    {
                        if (indi is IHistoryQueryIndicator)
                            UpdateCacheForIndicator((IHistoryQueryIndicator) indi);
                        indi.BuildSeries(chart);
                        RedrawChartSafe();
                        ActivateIndiEditEvent(oldName, (BaseChartIndicator) indi);
                    };
            dlg.onIndicatorRemove +=
                delegate(IChartIndicator indi)
                    {
                        indicators.Remove(indi);
                        ActivateIndiRemoveEvent((BaseChartIndicator) indi);
                    };

            dlg.ShowDialog();
        }

        private void AddNewIndicator(IChartIndicator indi)
        {
            indicators.Add(indi);
            indi.Add(chart, null);
            UpdateIndicatorPanesAndSeries();
            EnsureUniqueName(indi);
            indi.AcceptSettings();
                    
            // индикатор требует обновления валютного кэша?
            if (indi is IHistoryQueryIndicator)
                UpdateCacheForIndicator((IHistoryQueryIndicator) indi);
            // индиактор может затребовать другие графики?
            if (indi is IChartQueryIndicator)
                ((IChartQueryIndicator)indi).GetOuterCharts += getOuterCharts;
            indi.BuildSeries(chart);
            ActivateIndiAddEvent((BaseChartIndicator)indi);
        }

        private void MenuitemNewOrderClick(object sender, EventArgs e)
        {
            if (newOrder != null) newOrder(Symbol);
        } 

        private void UpdateCacheForIndicator(IHistoryQueryIndicator indi)
        {
            var tickers = indi.GetRequiredTickersHistory(null);
            if (tickers.Count > 0) updateTickersCacheForRobots(tickers, 5);
        }

        /// <summary>
        /// показать окошко быстрого перехода
        /// </summary>
        private void MenuitemGoToClick(object sender, EventArgs e)
        {
            var totalCount = chart.StockSeries.Data.Count;
            if (totalCount == 0) return;
            // получить границы графика
            var indexOpen = (int) chart.StockPane.WorldRect.Left + 1;
            var timeOpen = chart.StockSeries.GetCandleOpenTimeByIndex(indexOpen);
            var indexClose = (int)chart.StockPane.WorldRect.Right;
            if (indexClose > chart.StockSeries.DataCount) indexClose = chart.StockSeries.DataCount - 1;
            var timeClose = chart.StockSeries.GetCandleOpenTimeByIndex(indexClose);
            var minTime = chart.StockSeries.GetCandleOpenTimeByIndex(0);
            var maxTime = chart.StockSeries.GetCandleOpenTimeByIndex(totalCount - 1);
            // диалог
            var dlg = new GoToForm(indexOpen, indexClose, timeOpen, timeClose, totalCount, minTime, maxTime);
            if (dlg.ShowDialog() == DialogResult.Cancel) return;
            // установить границы в свечах?
            var candleBounds = dlg.CandleBounds;
            if (candleBounds.HasValue)
            {
                chart.SetScrollView(candleBounds.Value.X, candleBounds.Value.Y);
                return;
            }
            // установить границы по времени?
            var timeBounds = dlg.TimeBounds;
            if (!timeBounds.HasValue) return;
            var start = chart.StockSeries.GetIndexByCandleOpen(timeBounds.Value.a);
            var stop = chart.StockSeries.GetIndexByCandleOpen(timeBounds.Value.b);
            chart.SetScrollView(start, stop);
        }

        private void MenuitemMakeTradeSignalClick(object sender, EventArgs e)
        {
            if (publishTradeSignal != null)
                publishTradeSignal(this);
        }

        public void SetupScriptMenu(List<string> menuTitles, List<object> menuTags)
        {
            menuitemScripts.DropDownItems.Clear();
            for (var i = 0; i < menuTitles.Count; i++)
            {
                var title = menuTitles[i];
                var tag = menuTags[i];
                var item = menuitemScripts.DropDownItems.Add(title);
                item.Tag = tag;
                item.Click += MenuItemScriptActivated;
            }
            menuitemScripts.Visible = menuitemScripts.DropDownItems.Count > 0;
        }

        private void MenuItemScriptActivated(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;
            if (scriptMenuItemActivated != null)
                scriptMenuItemActivated(this, (PointD)contextMenu.Tag, item.Tag);
        }

        private void MenuitemSignalTextMessageClick(object sender, EventArgs e)
        {
            if (makeNewTextSignal != null)
                makeNewTextSignal(this);
        }

        /// <summary>
        /// запомнить точку на графике (время)
        /// </summary>
        private void ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var point = chart.StockPane.PointToClient(Cursor.Position);
            var pointD = Conversion.ScreenToWorld(new PointD(point.X, point.Y),
               chart.StockPane.WorldRect, chart.StockPane.CanvasRect);
            contextMenu.Tag = pointD;
        }

        /// <summary>
        /// если на графике уже есть индикатор ордеров - открыть его настройки
        /// иначе - добавить на график индикатор
        /// </summary>
        private void IndicatorOrdersMenuClick(object sender, EventArgs e)
        {
            var ordersIndi = indicators.FirstOrDefault(i => i is IndicatorOrders);
            if (ordersIndi != null)
            {
                var dlg = new IndicatorSettingsWindow { Indi = ordersIndi };
                dlg.ShowDialog();
                return;
            }

            // добавить индикатор и открыть окно его настроек
            var indi = new IndicatorOrders();
            AddNewIndicator(indi);
            new IndicatorSettingsWindow { Indi = indi }.ShowDialog();
        }

        private void SyncQuotes()
        {
            if (syncQuoteHistory == null) return;
            var tickers = new List<string> { Symbol };
            var startTime = chart.StockSeries.DataCount > 0
                                ? chart.StockSeries.Data.Candles[0].timeOpen
                                : DateTime.Now.Date.AddDays(-60);
            // добавить котировки, участвующие в построении валютных индексов
            foreach (var indi in indicators)
            {
                if (indi is IHistoryQueryIndicator == false) continue;
                var indiTickers = ((IHistoryQueryIndicator)indi).GetRequiredTickersHistory(startTime);
                if (indiTickers != null)
                    tickers.AddRange(indiTickers.Keys);
            }
            tickers = tickers.Distinct().ToList();
            syncQuoteHistory(tickers, startTime, true);
        }

        /// <summary>
        /// синхронизировать котировки (заполнить гэпы)
        /// </summary>
        private void MenuitemSyncQuotesClick(object sender, EventArgs e)
        {
            SyncQuotes();
        }
    }
}