using System;
using System.ComponentModel;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Сделка с фикс. плечом")]
    public class ScriptTradePartDepo : TerminalScript
    {
        #region Параметры

        public enum ScriptOrderType { Рыночный = 0, Отложенный = 1 }
        [DisplayName("Тип ордера")]
        [Category("Основные")]
        [Description("Рыночный / отложенный")]
        [PropertyXMLTag("OrderType")]
        public ScriptOrderType OrderType { get; set; }

        private DealType side = DealType.Buy;
        [DisplayName("Сделка")]
        [Category("Основные")]
        [Description("Покупка / продажа")]
        [PropertyXMLTag("Side")]
        public DealType Side
        {
            get { return side; }
            set { side = value; }
        }

        private bool requestSide = true;
        [DisplayName("Запрашивать операцию")]
        [Category("Основные")]
        [Description("Запрашивать тип сделки")]
        [PropertyXMLTag("RequestSide")]
        public bool RequestSide
        {
            get { return requestSide; }
            set { requestSide = value; }
        }

        private bool confirmTrade = true;
        [DisplayName("Подтверждать сделку")]
        [Category("Основные")]
        [Description("Показывать диалог подтверждения сделки")]
        [PropertyXMLTag("ConfirmTrade")]
        public bool ConfirmTrade
        {
            get { return confirmTrade; }
            set { confirmTrade = value; }
        }

        private decimal leverage = 0.1M;
        [DisplayName("Плечо")]
        [Category("Основные")]
        [Description("Плечо на одну сделку")]
        [PropertyXMLTag("Leverage")]
        public decimal Leverage
        {
            get { return leverage; }
            set { leverage = value; }
        }

        private int dealsMax = 5;
        [DisplayName("№ сделок")]
        [Category("Основные")]
        [Description("Макс. количество сделок")]
        [PropertyXMLTag("DealsMax")]
        public int DealsMax
        {
            get { return dealsMax; }
            set { dealsMax = value; }
        }

        public enum DealCountingType { ОбщийУчет = 0, УчетПоТикеру = 1 }
        [DisplayName("Учет сделок")]
        [Category("Основные")]
        [Description("Учет сделок: общий или по тикеру (инструменту)")]
        [PropertyXMLTag("DealCounting")]
        public DealCountingType DealCounting { get; set; }

        [DisplayName("Округлять объем")]
        [Category("Основные")]
        [Description("Округлять объем сделки")]
        [PropertyXMLTag("VolumeRound")]
        public VolumeRoundType VolumeRound { get; set; }
        #endregion

        public ScriptTradePartDepo()
        {
            ScriptTarget = TerminalScriptTarget.Тикер;
            ScriptName = "Сделка с фикс. плечом";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            return ActivateScript(chart.Symbol);
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"ScriptTradePartDepo\"");
        }        

        public override string ActivateScript(string ticker)
        {
            if (!AccountStatus.Instance.isAuthorized)
            {
                MessageBox.Show("Не авторизован");
                return "Не авторизован";
            }
            var account = AccountStatus.Instance.AccountData;
            if (account == null)
            {
                MessageBox.Show("Не авторизован");
                return "Не авторизован";
            }

            var equity = account.Equity;
            if (equity <= 0)
            {
                MessageBox.Show("На счете отсутствуют денежные средства");
                return "На счете отсутствуют денежные средства";
            }
            
            var dealSide = side;
            decimal? sl = null, tp = null;
            float? trailLevel = null, trailTarget = null;

            // предполагаемая цена входа
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            QuoteData currentQuote;
            quotes.TryGetValue(ticker, out currentQuote);
            var reqPrice = currentQuote == null
                               ? 0
                               : Side == DealType.Buy
                                     ? currentQuote.ask
                                     : currentQuote.bid;

            if (OrderType == ScriptOrderType.Отложенный && currentQuote == null)
            {
                return "Отложенный ордер не создан - нет котировки " + ticker;
            }
            
            // для отложенного ордера обязательно запросим цену
            decimal priceEnter = 0;
            float dealPrice = 0;
            if (RequestSide || OrderType == ScriptOrderType.Отложенный)
            {
                var dlg = new ScriptPartDepoFormDialog(
                    (int) side,
                    currentQuote == null ? (float?)null : currentQuote.bid,
                    OrderType == ScriptOrderType.Отложенный) { TradeTicker = ticker };
                if (dlg.ShowDialog() == DialogResult.Cancel) return "Отменен";
                priceEnter = dlg.Price ?? 0;
                sl = dlg.SL;
                tp = dlg.TP;
                dealSide = (DealType)dlg.Side;

                if (dlg.Trailing != null && dlg.Trailing.Length == 2 && (dlg.Trailing[0] != 0 || dlg.Trailing[1] != 0))
                {
                    if (currentQuote == null)
                    {
                        var msg = string.Format("Нет котировки \"{0}\" для расчета пп трейлинга", ticker);
                        MessageBox.Show(msg);
                        return msg;
                    }
                    
                    var trailPrice = dlg.Trailing[0];
                    dealPrice = 
                        OrderType == ScriptOrderType.Отложенный ? (float)priceEnter : (dealSide == DealType.Buy ? currentQuote.ask : currentQuote.bid);
                    var trailPips = DalSpot.Instance.GetPointsValue(ticker, ((int) dealSide)*(trailPrice - dealPrice));
                    
                    // преобразовать цену в пп от входа
                    trailLevel = trailPips;
                    trailTarget = dlg.Trailing[1];
                }                
            }

            // объем в валюте депозита
            var volumeDepo = equity * leverage;
            var volumeBase = volumeDepo;
            
            // из объема депо пересчитать в объем базовой валюты
            var depoCurx = AccountStatus.Instance.AccountData.Currency;
            
            bool inverse, pairsEqual;
            var tickerTrans = DalSpot.Instance.FindSymbol(ticker, true, depoCurx, 
                out inverse, out pairsEqual);
            if (!pairsEqual)
            {
                QuoteData quote;
                quotes.TryGetValue(tickerTrans, out quote);
                if (quote == null)
                {
                    var msgError = string.Format(
                        "Невозможно рассчитать объем - отсутствует котировка \"{0}\"", tickerTrans);
                    MessageBox.Show(msgError);
                    return msgError;
                }
                var priceTrans = inverse ? 1 / quote.bid : quote.ask;
                volumeBase /= (decimal)priceTrans;
            }

            // округлить объем
            var lotSize = DalSpot.Instance.GetMinStepLot(ticker, account.Group);
            volumeBase = MarketOrder.RoundDealVolume((int)volumeBase, VolumeRound,
                                                   lotSize.a, lotSize.b);
            if (volumeBase == 0)
            {
                var msgError = string.Format(
                    "Рассчетный объем входа ({0}) меньше допустимого ({1})",
                    volumeBase, lotSize.a);
                MessageBox.Show(msgError);
                return msgError;
            }
            
            // проверить количество входов (по тикеру либо вообще)
            var ordersCount =
                DealCounting == DealCountingType.ОбщийУчет
                    ? MarketOrdersStorage.Instance.MarketOrders.Count
                    : MarketOrdersStorage.Instance.MarketOrders.Count(o => o.Symbol == ticker);

            if (ordersCount >= dealsMax)
            {
                var msgError = 
                    DealCounting == DealCountingType.ОбщийУчет 
                    ? string.Format(
                        "Уже открыто {0} сделок из {1} макс.", ordersCount, dealsMax)
                    : string.Format(
                        "По \"{0}\" уже открыто {1} сделок из {2} макс.", 
                            ticker, ordersCount, dealsMax);
                MessageBox.Show(msgError);
                return msgError;
            }

            // подтверждение
            var confirmPrice = dealPrice;
            if (confirmPrice == 0) 
                confirmPrice = (dealSide == DealType.Buy ? currentQuote.ask : currentQuote.bid);
            
            if (confirmTrade)
                if (MessageBox.Show(string.Format("Будет совершена {0} {1} {2} по {3}. Продолжить?",
                    dealSide == DealType.Buy ? "покупка" : "продажа", 
                    volumeBase.ToStringUniformMoneyFormat(), depoCurx,
                    confirmPrice.ToStringUniformPriceFormat()),
                    "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.No)
                    return "отменено пользователем";

            // таки войти в рынок
            if (OrderType == ScriptOrderType.Рыночный)
                MainForm.Instance.SendNewOrderRequestSafe(
                    RequestUniqueId.Next(),
                    AccountStatus.Instance.accountID,
                    new MarketOrder
                        {
                            Volume = (int)volumeBase,
                            Side = (int)dealSide,
                            Symbol = ticker,
                            StopLoss = (float?)sl,
                            TakeProfit = (float?)tp,
                            TrailLevel1 = trailLevel,
                            TrailTarget1 = trailTarget
                        },
                        priceEnter, 0, Contract.Entity.OrderType.Market);
            else
            {
                // создать отложенник
                var orderType = PendingOrderType.Limit;
                if (((int)dealSide == 1 && currentQuote.ask < (float)priceEnter) || 
                    ((int)dealSide == -1 && currentQuote.bid > (float)priceEnter))
                    orderType = PendingOrderType.Stop;
                var pendingOrder = new PendingOrder
                    {
                        AccountID = AccountStatus.Instance.accountID,
                        Symbol = ticker,
                        Volume = (int)volumeBase,
                        Side = (int)dealSide,
                        PriceSide = orderType,
                        PriceFrom = (float)priceEnter,
                        StopLoss = (float?)sl,
                        TakeProfit = (float?)tp,
                        TrailLevel1 = trailLevel,
                        TrailTarget1 = trailTarget
                    };

                MainForm.Instance.SendNewPendingOrderRequestSafe(RequestUniqueId.Next(), pendingOrder);
            }

            return "Сделка с фиксированным плечом (" + dealSide + " " + ticker + ") совершена";
        }        
    }
}
