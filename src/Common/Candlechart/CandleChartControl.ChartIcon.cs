using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Candlechart.ChartIcon;
using Candlechart.Controls;
using Candlechart.Core;
using Candlechart.Indicator;
using Entity;
using FastGrid;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace Candlechart
{
    partial class CandleChartControl
    {
        public static readonly List<ChartIcon.ChartIcon> allChartIcons = new List<ChartIcon.ChartIcon>();

        private static void SetupChartIcons()
        {
            // забубенить кнопкам картинки
            var imageKeyByName = new Dictionary<string, string[]>
                {
                    { ChartIcon.ChartIcon.chartButtonStatByTicker, new [] { "ico_calculator.png" } },
                    { ChartIcon.ChartIcon.chartButtonDealByTicker, new [] { "ico_black_orders.png" } },    
                    { ChartIcon.ChartIcon.chartButtonFastBuy, new [] { "ico_black_up.png" } },    
                    { ChartIcon.ChartIcon.chartButtonFastSell, new [] { "ico_black_down.png" } },    
                    { ChartIcon.ChartIcon.chartButtonFavIndicators, new [] { "ico_black_favourite.png" } },    
                    { ChartIcon.ChartIcon.chartButtonIndexAutoScroll, new [] { "ico_black_forward.png", "ico_black_pause.png" } },
                    { ChartIcon.ChartIcon.chartButtonIndicators, new [] { "ico_black_indicator.png" } },    
                    { ChartIcon.ChartIcon.chartButtonNewOrder, new [] { "ico_black_dollar.png" } },    
                    { ChartIcon.ChartIcon.chartButtonPatchQuotes, new [] { "ico_black_synch.png" } },    
                    { ChartIcon.ChartIcon.chartButtonQuoteArchive, new [] { "ico_black_archive.png" } },
                    { ChartIcon.ChartIcon.chartVisualSettings, new [] { "ico_makeup.png" } },
                    { ChartIcon.ChartIcon.chartProfitByTicker, new [] { "ico_profit_ticker.png" } },
                    { ChartIcon.ChartIcon.chartRobot, new [] { "ico_robot.png" } },
                };
            
            foreach (var img in imageKeyByName)
            {
                var imgCollection = new ChartIcon.ChartIcon.ButtonImageCollection
                    {
                        images = new Bitmap[2,img.Value.Length]
                    };

                for (var i = 0; i < img.Value.Length; i++)
                {
                    // загрузить картинку и создать ее инверсную копию
                    var fullName = "Candlechart.images." + img.Value[i];
                    Bitmap imgNormal, imgInverse;
                    LoadImageFromResxAndMakeInverseCopy(fullName, out imgNormal, out imgInverse);
                    imgCollection.images[0, i] = imgNormal;
                    imgCollection.images[1, i] = imgInverse;
                }
                ChartIcon.ChartIcon.AddButtonImages(img.Key, imgCollection);
            }

            var iconStatByTicker = new ChartIconDropDownDialog
            {
                key = ChartIcon.ChartIcon.chartButtonStatByTicker
            };
            var iconAscroll = new ChartIconCheckBox(null)
            {
                key = ChartIcon.ChartIcon.chartButtonIndexAutoScroll
            };
            var iconIndi = new ChartIcon.ChartIcon((obj, e) => { })
            {
                key = ChartIcon.ChartIcon.chartButtonIndicators
            };
            var iconOrder = new ChartIcon.ChartIcon((obj, e) => { })
            {
                key = ChartIcon.ChartIcon.chartButtonNewOrder
            };
            var iconArchive = new ChartIcon.ChartIcon((obj, e) => { })
            {
                key = ChartIcon.ChartIcon.chartButtonQuoteArchive
            };
            var iconPatchQuotes = new ChartIcon.ChartIcon((obj, e) => { })
            {
                key = ChartIcon.ChartIcon.chartButtonPatchQuotes
            };
            var iconFavIndi = new ChartIconDropDown
            {
                key = ChartIcon.ChartIcon.chartButtonFavIndicators
            };
            var iconFastSell = new ChartIconDropDown
            {
                key = ChartIcon.ChartIcon.chartButtonFastSell
            };
            var iconFastBuy = new ChartIconDropDown
            {
                key = ChartIcon.ChartIcon.chartButtonFastBuy
            };
            var iconFastOrders = new ChartIconDropDown
            {
                key = ChartIcon.ChartIcon.chartButtonDealByTicker
            };
            var iconVisual = new ChartIcon.ChartIcon((obj, e) => { })
            {
                key = ChartIcon.ChartIcon.chartVisualSettings
            };
            var iconProfitByTicker = new ChartIcon.ChartIcon((obj, e) => { })
            {
                key = ChartIcon.ChartIcon.chartProfitByTicker
            };
            var iconRobots = new ChartIconDropDown
            {
                key = ChartIcon.ChartIcon.chartRobot
            };
            iconFastOrders.listControl.columns = new List<FastColumn>
                {
                    new FastColumn("") {ColumnWidth = 38},
                    new FastColumn("") {ColumnWidth = 72},
                    new FastColumn("") {ColumnWidth = 48},
                    new FastColumn("") {ColumnWidth = 73},
                    new FastColumn("") {ColumnWidth = 73}
                };
            iconFastOrders.listControl.MinWidth = iconFastOrders.listControl.columns.Sum(c => c.ColumnWidth);
            iconFastOrders.listControl.MaxWidth = iconFastOrders.listControl.MinWidth;
            iconFastOrders.listControl.Width = iconFastOrders.listControl.MinWidth;
            iconFastOrders.listControl.CalcWidthAuto = false;
            
            // добавить все в массив
            allChartIcons.Add(iconStatByTicker);
            allChartIcons.Add(iconFastOrders);
            allChartIcons.Add(iconAscroll);
            allChartIcons.Add(iconIndi);
            allChartIcons.Add(iconOrder);
            allChartIcons.Add(iconArchive);
            allChartIcons.Add(iconFavIndi);
            allChartIcons.Add(iconFastSell);
            allChartIcons.Add(iconFastBuy);
            allChartIcons.Add(iconPatchQuotes);
            allChartIcons.Add(iconVisual);
            allChartIcons.Add(iconProfitByTicker);
            allChartIcons.Add(iconRobots);

            allChartIcons.ForEach(i =>
                {
                    i.Size = new Size(22, 22);
                });
        }

        private static void LoadImageFromResxAndMakeInverseCopy(string resxImgName, 
            out Bitmap imgNormal, out Bitmap imgInverse)
        {
            using (var imageStream = Assembly.GetCallingAssembly().GetManifestResourceStream(resxImgName))
            {
                imgNormal = new Bitmap(imageStream);
            }
            
            // копия
            // получить байты картинки
            var rect = new Rectangle(0, 0, imgNormal.Width, imgNormal.Height);
            var bmpData =
                imgNormal.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                imgNormal.PixelFormat);
            var ptr = bmpData.Scan0;
            var bytes  = Math.Abs(bmpData.Stride) * imgNormal.Height;
            var rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            imgNormal.UnlockBits(bmpData);

            // обработать байты картинки
            for (var counter = 0; counter < rgbValues.Length; counter += 4)
            {
                var r = rgbValues[counter];
                var g = rgbValues[counter + 1];
                var b = rgbValues[counter + 2];
                GraphicsExtensions.InvertColorLightness(ref r, ref g, ref b);
                rgbValues[counter] = r;
                rgbValues[counter + 1] = g;
                rgbValues[counter + 2] = b;
            }

            // открыть новую картинку на запись
            imgInverse = new Bitmap(imgNormal);
            bmpData =
                imgInverse.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                imgInverse.PixelFormat);
            ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            
            // закрыть картинку
            imgInverse.UnlockBits(bmpData);
        }

        /// <summary>
        /// список записей - ключи в словаре allChartIcons
        /// (пример - chartButtonIndexAutoScroll)
        /// </summary>        
        public void ChooseChartIconsToShow(string[] iconKeys)
        {
            var customButtons = (iconKeys == null || iconKeys.Length == 0)
                                                ? null
                                                : allChartIcons.Where(i => 
                                                    iconKeys.Contains(i.key)).Select(i => i.MakeCopy()).ToArray();
            
            // выстроить кнопки
            if (customButtons != null)
            {
                var left = 95;
                const int top = 4, padding = 4;
                // цвета кнопок
                foreach (var btn in customButtons)
                {
                    btn.Position = new Point(left, top);
                    left += (btn.Size.Width + padding);
                    btn.SetThemeByBackColor(chart.visualSettings.ChartBackColor);
                }
            }
            chart.StockPane.customButtons = customButtons;
            SetupChartIconsHandlers(customButtons);
            UpdateChartIconsState();
        }

        /// <summary>
        /// обновить состояние кнопок графика - например, после применения настроек
        /// касается кнопок - переключателей
        /// </summary>
        public void UpdateChartIconsState()
        {
            foreach (var btn in chart.StockPane.customButtons)
            {
                if (btn is ChartIconCheckBox == false) continue;
                var boxBtn = (ChartIconCheckBox) btn;
                boxBtn.GetStateFromChartObject(chart);
            }
        }

        /// <summary>
        /// назначить кнопкам действия
        /// указать текущее состояние кнопок
        /// </summary>        
        private void SetupChartIconsHandlers(ChartIcon.ChartIcon[] iconButtons)
        {
            foreach (var btn in iconButtons)
            {
                if (btn is ChartIconDropDown)
                {
                    var dropList = (ChartIconDropDown) btn;
                    dropList.Owner = chart;
                    dropList.BeforeDropDown += (sender, args) =>
                        {
                            var maxHeight = Height - dropList.Position.Y - 60;
                            if (maxHeight < 100)
                                maxHeight = 100;
                            dropList.listControl.MaxLines =
                                (int) Math.Ceiling(maxHeight/(double) dropList.listControl.CellHeight);
                        };
                }

                if (btn.key == ChartIcon.ChartIcon.chartButtonIndicators)
                    // показать индикаторы
                    btn.click += (sender, args) => ShowIndicatorsWindow();
                else if (btn.key == ChartIcon.ChartIcon.chartButtonIndexAutoScroll)
                {
                    // автопрокрутка включена - выключена
                    ((ChartIconCheckBox)btn).getStateFromChart = c => c.StockSeries.AutoScroll;
                    ((ChartIconCheckBox)btn).clickHandler += (ChartIcon.ChartIcon sender, out bool state) =>
                                                                  {
                                                                      chart.StockSeries.AutoScroll = !chart.StockSeries.AutoScroll;
                                                                      state = chart.StockSeries.AutoScroll;
                                                                  };
                }
                else if (btn.key == ChartIcon.ChartIcon.chartButtonNewOrder)
                    // диалог ордера
                    btn.click += (sender, args) =>
                                     {
                                         if (newOrder != null) newOrder(Symbol);
                                     };
                else if (btn.key == ChartIcon.ChartIcon.chartButtonQuoteArchive)
                    // архив котировок
                    btn.click += (sender, args) =>
                                    {
                                        if (showQuoteArchive != null) showQuoteArchive(Symbol);
                                    };
                else if (btn.key == ChartIcon.ChartIcon.chartButtonFavIndicators)
                    // список избранных индикаторов
                {
                    var favIndiCtrl = ((ChartIconDropDown) btn);
                    favIndiCtrl.BeforeDropDown += (sender, args) => SetupFavIndicatorsList(favIndiCtrl);
                    favIndiCtrl.listControl.cellClicked += (obj, text) =>
                                                               {
                                                                   var indiDesc = (IndicatorDescription) obj;
                                                                   var ind = (IChartIndicator)Activator.CreateInstance(indiDesc.indicatorType);
                                                                   AddNewIndicator(ind);
                                                                   // открыть окно настройки индикатора
                                                                   new IndicatorSettingsWindow { Indi = ind }.ShowDialog();
                                                               };
                }
                else if (btn.key == ChartIcon.ChartIcon.chartButtonFastBuy || btn.key == ChartIcon.ChartIcon.chartButtonFastSell)
                    // быстро продать или купить
                {
                    var buySell = ((ChartIconDropDown)btn);
                    SetupFastTradeButton(buySell);
                }
                else if (btn.key == ChartIcon.ChartIcon.chartButtonPatchQuotes)
                    // предложить актуализировать график
                {
                    btn.click += (sender, args) => SyncQuotes();
                }
                else if (btn.key == ChartIcon.ChartIcon.chartButtonDealByTicker)
                {
                    // сделки по валютной паре
                    var dealsBtn = ((ChartIconDropDown)btn);
                    dealsBtn.BeforeDropDown += FillButtonDealByTickerList;
                    dealsBtn.listControl.cellClicked += (obj, text) =>
                        {
                            var order = (ChartIconDealTableRow) obj;
                            if (order.Id == 0) return;
                            CallShowWindowEditMarketOrder(new MarketOrder { ID = order.Id });
                        };
                }
                else if (btn.key == ChartIcon.ChartIcon.chartVisualSettings)
                {
                    btn.click += (sender, args) =>
                        {
                            if (visualSettingsSetupCalled != null)
                                visualSettingsSetupCalled();
                        };
                }
                else if (btn.key == ChartIcon.ChartIcon.chartButtonStatByTicker)
                {
                    // статистика по валютной паре
                    var statBtn = ((ChartIconDropDownDialog)btn);
                    statBtn.Owner = chart;
                }
                else if (btn.key == ChartIcon.ChartIcon.chartProfitByTicker)
                {
                    btn.click += (sender, args) =>
                    {
                        if (profitByTickerRequested != null)
                            profitByTickerRequested(Symbol);
                    };
                }
                else if (btn.key == ChartIcon.ChartIcon.chartRobot)
                {   // нажата кнопка Роботы - открыть диалог выбора робота из портфеля,
                    // привязанного к текущему тикеру
                    // по клику на роботе открыть его его настройки или окно состояния робота
                    var btnRobot = (ChartIconDropDown) btn;
                    btnRobot.listControl.CalcWidthAuto = true;
                    btnRobot.BeforeDropDown += (sender, args) =>
                        {
                            btnRobot.listControl.Values = getRobotsByTicker(Symbol).Cast<object>().ToList();
                        };
                    btnRobot.listControl.cellClicked += (obj, text) => onRobotSelected(text);
                }
            }
        }

        private void FillButtonDealByTickerList(object sender, EventArgs e)
        {
            if (receiveMarketOrders == null) return;
            
            // получить ордера
            var curPrice = chart.StockPane.YAxis.CurrentPrice;
            var marketOrders = receiveMarketOrders().Where(o => o.Symbol == Symbol).ToList();
            var orders = marketOrders.OrderBy(order => order.Side * 100000f + order.ResultDepo).Select(o => 
                (object)new ChartIconDealTableRow
                {
                    Id = o.ID,
                    Price = o.PriceEnter,
                    Profit = o.ResultDepo,
                    Side = o.Side,
                    Volume = o.Volume,
                    ProfitPoints = curPrice.HasValue ?
                        (int)Math.Round(DalSpot.Instance.GetPointsValue(Symbol,
                            o.Side * (curPrice.Value - o.PriceEnter))) : 0
                }).ToList();
            
            // сформировать суммарный средневзвешенный ордер
            if (orders.Count > 1)
            {
                float sumBuys = 0, sumSells = 0;
                var sumVolume = 0;
                var totalProfit = 0f;
                foreach (var order in marketOrders)
                {
                    sumVolume += order.Side*order.Volume;
                    if (order.Side > 0)
                        sumBuys += order.Volume * order.PriceEnter;
                    else
                        sumSells += order.Volume * order.PriceEnter;
                    totalProfit += order.ResultDepo;
                }

                if (sumVolume != 0)
                {
                    var averagePrice = (sumBuys - sumSells) / sumVolume;
                    var avgOrder = new ChartIconDealTableRow
                        {
                            Price = averagePrice,
                            Profit = totalProfit,
                            Side = Math.Sign(sumVolume),
                            Volume = Math.Abs(sumVolume)                            
                        };
                    avgOrder.ProfitPoints = curPrice.HasValue
                        ? (int)Math.Round(DalSpot.Instance.GetPointsValue(Symbol, avgOrder.Side * (curPrice.Value -
                        avgOrder.Price))) : 0;
                    orders.Insert(0, avgOrder);
                }
            }
            // прибайндить к списку
            var dealsBtn = ((ChartIconDropDown)sender);
            dealsBtn.DataBind(orders);
        }

        private void SetupFavIndicatorsList(ChartIconDropDown btn)
        {
            var descriptions = new List<object>();
            var favIndis = getFavoriteIndicators();
            foreach (var t in PluginManager.Instance.typeIndicators)
            {
                if (!favIndis.Contains(t.Name)) continue;
                var attrName = (DisplayNameAttribute)Attribute.GetCustomAttribute(t,
                    typeof(DisplayNameAttribute));
                if (attrName == null) continue;
                descriptions.Add(new IndicatorDescription(attrName.DisplayName, "", false)
                    {
                        indicatorType = t
                    });
            }

            btn.listControl.Values = descriptions;
        }            
    
        private void SetFastBuySellOptions(ChartIconDropDown buySell, bool sideBuy)
        {
            const string strMoreOptions = "другое значение...";
            var lstValues = new List<object> {strMoreOptions};
            lstValues.AddRange(getDefaultFastDealVolumes().Select(vlm => 
                string.Format("{0} - {1} - {2}", 
                    sideBuy ? "BUY" : "SELL", vlm.ToStringUniformMoneyFormat(), Symbol)));

            buySell.listControl.Values = lstValues;
            buySell.listControl.Formatter = val =>
                                                {
                                                    return val is string ? (string) val 
                                                        : ((int) val).ToStringUniformMoneyFormat();
                                                };
        }

        private void SetupFastTradeButton(ChartIconDropDown buySell)
        {
            var side = buySell.key == ChartIcon.ChartIcon.chartButtonFastBuy;
            buySell.Owner = chart;
            buySell.listControl.Formatter = val =>
                                                {
                                                    return val is string ? (string) val 
                                                        : ((int) val).ToStringUniformMoneyFormat();
                                                };
            SetFastBuySellOptions(buySell, side);

            
            buySell.listControl.cellClicked += (obj, text) =>
                                                   {
                                                       var strVolume = (string) obj;
                                                       var parts = strVolume.Split(new [] { " - "}, StringSplitOptions.RemoveEmptyEntries);

                                                       if (parts.Length != 3)
                                                       {// показать диалог настройки объемов
                                                            var volumesString = string.Join(", ", getDefaultFastDealVolumes());
                                                            DialogResult rst;
                                                            var str = Dialogs.ShowInputDialog("Укажите объемы входа",
                                                                                        "объемы", true,
                                                                                        volumesString, out rst);
                                                            if (rst != DialogResult.OK) return;
                                                            var volumes = str.ToIntArrayUniform();
                                                            if (volumes.Length == 0) return;

                                                            // сохранить объемы
                                                            updateFastVolumes(volumes);

                                                            // обновить список
                                                            foreach (var btn in chart.StockPane.customButtons.Where(
                                                                b => b.key == ChartIcon.ChartIcon.chartButtonFastBuy || 
                                                                b.key == ChartIcon.ChartIcon.chartButtonFastSell))
                                                            {
                                                                SetFastBuySellOptions((ChartIconDropDown) btn,
                                                                                        btn.key == ChartIcon.ChartIcon.chartButtonFastBuy);
                                                            }

                                                            return;
                                                       }

                                                       // торгануть
                                                       var volm = parts[1].Replace(" ", "").ToIntSafe() ?? 0;

                                                        var sign = side ? DealType.Buy : DealType.Sell;
                                                        if (makeTrade != null)
                                                            makeTrade(sign, Symbol, volm);
                                                    };
        }
    }
}
