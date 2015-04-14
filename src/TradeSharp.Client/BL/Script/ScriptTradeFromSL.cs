using System;
using System.ComponentModel;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;
using System.Linq;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Сделка от SL")]
    public class ScriptTradeFromSL : TerminalScript
    {
        #region Параметры

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

        private decimal lossPercent = 5;
        [DisplayName("Процент потерь")]
        [Category("Основные")]
        [Description("Процент потерь при наступлении SL")]
        [PropertyXMLTag("LossPercent")]
        public decimal LossPercent
        {
            get { return lossPercent; }
            set { lossPercent = value; }
        }

        private decimal leverageMax = 0.1M;
        [DisplayName("Макс. плечо")]
        [Category("Основные")]
        [Description("Макс. плечо одной сделки")]
        [PropertyXMLTag("LeverageMax")]
        public decimal LeverageMax
        {
            get { return leverageMax; }
            set { leverageMax = value; }
        }

        private int[] volumePercent = new [] { 50, 50, 50, 100, 100, 75, 50, 50, 50 };
        [DisplayName("Объемы сделок, %")]
        [Category("Основные")]
        [Description("Объем каждой сделки от MAX (SL)")]
        [PropertyXMLTag("VolumePercentStr")]
        public string VolumePercentStr
        {
            get { return string.Join(", ", volumePercent); }
            set
            {
                var ar = value.ToIntArrayUniform();
                if (ar.Length > 0) volumePercent = ar;
            }
        }

        [DisplayName("Округлять объем")]
        [Category("Основные")]
        [Description("Округлять объем сделки")]
        [PropertyXMLTag("VolumeRound")]
        public VolumeRoundType VolumeRound { get; set; }

        private int volumeMin = 10000;
        [DisplayName("Объем мин.")]
        [Category("Основные")]
        [Description("Минимальный объем сделки")]
        [PropertyXMLTag("VolumeMin")]
        public int VolumeMin
        {
            get { return volumeMin; }
            set { volumeMin = value; }
        }

        private int volumeStep = 10000;
        [DisplayName("Объем шаг")]
        [Category("Основные")]
        [Description("Шаг объема сделки")]
        [PropertyXMLTag("VolumeStep")]
        public int VolumeStep
        {
            get { return volumeStep; }
            set { volumeStep = value; }
        }
        #endregion

        public ScriptTradeFromSL()
        {
            ScriptTarget = TerminalScriptTarget.Тикер;
            ScriptName = "Сделка от SL";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            return ActivateScript(chart.Symbol);
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"ScriptTradeFromSL\"");
        }        

        public override string ActivateScript(string ticker)
        {
            if (!AccountStatus.Instance.isAuthorized)
            {
                MessageBox.Show("Не авторизован");
                return "Не авторизован";
            }
            var equity = AccountStatus.Instance.AccountData.Equity;
            if (equity <= 0)
            {
                MessageBox.Show("На счете отсутствуют денежные средства");
                return "На счете отсутствуют денежные средства";
            }
            
            var orders = MarketOrdersStorage.Instance.MarketOrders.Where(o => o.Symbol == ticker).ToList();
            var side = orders.Count == 0 ? DealType.Buy : (DealType)orders[0].Side;
            float? sl = orders.Count == 0 ? null : orders[0].StopLoss;

            // открыть окно выбора направления входа и SL
            var dlg = new TradeFromSLDlg(orders.Count, side, sl);
            if (dlg.ShowDialog() != DialogResult.OK) return "Отменено пользователем";
            side = dlg.Side;
            var targetSl = dlg.StopLoss;

            // выставить стоп остальным ордерам
            foreach (var order in orders)
            {
                var delta = Math.Abs((order.StopLoss ?? 0) - targetSl);
                var deltaPoints = DalSpot.Instance.GetPointsValue(ticker, delta);
                if (deltaPoints < 2) continue;

                // редактировать ордер
                order.StopLoss = targetSl;
                MainForm.Instance.SendEditMarketRequestSafe(order);
            }

            // получить текущую цену
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            QuoteData quote;
            quotes.TryGetValue(ticker, out quote);
            if (quote == null)
                return "Нет котировки " + ticker;

            // коэфф пересчета из контрвалюты в валюту депо
            var kCounterDepo = 1f;
            if (!ticker.EndsWith("USD"))
                kCounterDepo = 1 / ((quote.ask + quote.bid)*0.5f);
            // посчитать макс. допустимый объем входа исходя из просадки
            var ordersResult = orders.Sum(o => o.Volume * o.Side * (targetSl - o.PriceEnter) * kCounterDepo); // убыток (прибыль) по сделкам
            
            var priceEnter = side == DealType.Buy ? quote.ask : quote.bid;
            var orderLossAbs = ((int)side) * (decimal)(targetSl - priceEnter);
            decimal volumeMax = decimal.MaxValue;
            
            // от текущего баланса считать сумму потерь
            var loss = equity * lossPercent / 100;
            if (orderLossAbs < 0)
            {
                volumeMax = (decimal)kCounterDepo * (loss + (decimal)ordersResult) / (-orderLossAbs);
            }

            // вход невозможен
            if (volumeMax < 0)
            {
                var err = string.Format(
                        "Потери по уже открытым позициям для SL {0:f4} ({1:f0}) превысят допустимый процент " +
                        "потерь ({2:f1}%, {3:f0} USD)", targetSl, ordersResult, lossPercent, loss);
                MessageBox.Show(err);
                return err;
            }

            // множитель для объема
            var indexK = orders.Count;
            if (indexK >= volumePercent.Length) indexK = volumePercent.Length - 1;
            var koeffVolume = volumePercent[indexK];
            if (volumeMax < decimal.MaxValue)
                volumeMax *= (koeffVolume/100M);

            // сравнить объем с предельно допустимым
            var maxVolumeDepo = equity * LeverageMax;
            var maxVolumeBase = maxVolumeDepo / (decimal)((quote.bid + quote.ask)*0.5f);

            var volumeEnter = Math.Min(volumeMax, maxVolumeBase);
            
            // округлить объем
            var volume = MarketOrder.RoundDealVolume((int) volumeEnter, VolumeRound,
                                        volumeMin, volumeStep);
            if (volume == 0)
            {
                var err = string.Format("Объем входа ({0:f0}) после округления равен 0",
                                        volumeEnter);
                MessageBox.Show(err);
                return err;
            }

            // подтвердить вход)
            if (confirmTrade)
            {
                var orderLoss = volume*((int) side)*(targetSl - priceEnter);
                var lossStr = ordersResult != 0
                                  ? string.Format("{0}{1:f0} по уже открытым ордерам и {2:f0} по новому ордеру",
                                  ordersResult > 0 ? "прибыль" : "", Math.Abs(ordersResult), (-orderLoss))
                                  : (-orderLoss).ToString("f0");
                var prompt = string.Format("Будет открыта позиция: {0} {1} {2}, SL={3:f4}. Потери при SL: {4}. Продолжить?",
                                           side, volume, ticker, targetSl, lossStr);
                if (MessageBox.Show(prompt, "Подтвердить сделку", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return "Отменено пользователем";
            }

            // войти в рынок
            MainForm.Instance.SendNewOrderRequestSafe(
                RequestUniqueId.Next(),
                AccountStatus.Instance.accountID,
                new MarketOrder
                    {
                        Volume = volume,
                        Side = (int)side,
                        Symbol = ticker,
                        StopLoss = targetSl
                    },
                    (decimal)priceEnter, 0, OrderType.Market);

            return "Операция успешна";
        }        
    }
}

