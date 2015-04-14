using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.Client.Forms
{
    public partial class OrderDlg : Form
    {
        #region Данные и свойства

        private int orderId;

        private int orderSide;

        private PendingOrder pendingOrder;
        
        private OrderDialogMode state;

        public OrderDialogMode State
        {
            get { return state; }
            set { state = value; }
        }

        public bool IsEditMode
        {
            get { return state == OrderDialogMode.OrderEditMarket || state == OrderDialogMode.OrderEditPending; }
        }

        public bool IsPending
        {
            get { return state == OrderDialogMode.OrderEditPending || state == OrderDialogMode.OrderNewPending; }
        }

        public enum OrderDialogMode
        {
            OrderNewMarket = 0,
            OrderEditMarket,
            OrderNewPending,
            OrderEditPending
        };

        private readonly QuotePoller quotePoller = new QuotePoller(1000);

        private QuoteData lastQuote;
        public QuoteData LastQuote
        {
            get { return lastQuote; }
            set
            {
                lastQuote = value;
                UpdateQuoteSafe();
            }
        }

        private string ticker;
        public string Ticker
        {
            get { return ticker; }
            set
            {
                if (ticker == value) return;
                ticker = value;
                btnBuyMarket.SetPrice(0, ticker);
                btnSellMarket.SetPrice(0, ticker);
            }
        }

        #endregion

        #region Конструкторы

        public OrderDlg()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            gridTrailing.Columns[0].HeaderText = Localizer.GetString("TitleLevel");
            gridTrailing.Columns[1].HeaderText = Localizer.GetString("TitleLevelInPoints");
            gridTrailing.Columns[2].HeaderText = Localizer.GetString("TitleSLInPoints");
        }

        public OrderDlg(string ticker) : this()
        {
            this.ticker = ticker;
        }

        public OrderDlg(OrderDialogMode state) : this()
        {
            this.state = state;            
        }

        public OrderDlg(OrderDialogMode state, int orderId) : this()
        {
            this.state = state;
            this.orderId = orderId;
        }

        #endregion

        private void SetupInterface()
        {
            gridTrailing.AutoGenerateColumns = false;
            gridTrailing.DataSource = trailLevels;

            var fontCaption = new Font(Font, FontStyle.Bold);
            var fontLeft = new Font(FontFamily.GenericSansSerif, fontCaption.Size + 1);
            var fontRight = new Font(FontFamily.GenericSansSerif, fontCaption.Size + 3);

            btnBuyMarket.buttonCaption = "BUY";
            btnBuyMarket.captionColor = Color.Blue;
            btnBuyMarket.captionFont = fontCaption;
            btnBuyMarket.textLeftColor = Color.Black;
            btnBuyMarket.textLeftFont = fontLeft;
            btnBuyMarket.textRightColor = Color.Blue;
            btnBuyMarket.textRightFont = fontRight;

            btnSellMarket.buttonCaption = "SELL";
            btnSellMarket.captionColor = Color.Red;
            btnSellMarket.captionFont = fontCaption;
            btnSellMarket.textLeftColor = Color.Black;
            btnSellMarket.textLeftFont = fontLeft;
            btnSellMarket.textRightColor = Color.Red;
            btnSellMarket.textRightFont = fontRight;

            // список режимов
            var targetType = (state == OrderDialogMode.OrderNewMarket || state == OrderDialogMode.OrderEditMarket)
                                 ? OrderType.Market
                                 : OrderType.Pending;
            cbMode.Items.Add(EnumFriendlyName<OrderType>.GetString(OrderType.Market));
            cbMode.Items.Add(EnumFriendlyName<OrderType>.GetString(OrderType.Pending));
            cbMode.SelectedIndex = cbMode.Items.IndexOf(EnumFriendlyName<OrderType>.GetString(targetType));
            
            // заполнить список валют
            var userTicker = ticker;

            cbCurx.Initialize();
            
            // выбрать валютную пару
            if (!string.IsNullOrEmpty(userTicker))
            {
                var index = cbCurx.Items.IndexOf(userTicker);
                if (index < 0)
                    index = 0;
                cbCurx.SelectedIndex = index;
            }

            if (state != OrderDialogMode.OrderNewMarket)
                ApplyUIMode();

            // загрузить данные ордера, если передан Id
            if (state == OrderDialogMode.OrderEditMarket)
                LoadMarketOrderData();
            else if (state == OrderDialogMode.OrderEditPending)
                    LoadPendingOrderData();

            quotePoller.OnQuoteHashUpdated += QuotePollerOnQuoteHashUpdated;
            quotePoller.StartPolling();
        }

        private void LoadMarketOrderData()
        {
            RequestStatus status;
            List<MarketOrder> actualPos;
            try
            {
                status = TradeSharpAccount.Instance.proxy.GetMarketOrders(AccountStatus.Instance.accountID, out actualPos);
            }
            catch (Exception ex)
            {
                Logger.Error("OrderDlg::LoadMarketOrderData proxy error", ex);
                return;
            }
            if (status != RequestStatus.OK) return;
            var pos = actualPos.FirstOrDefault(p => p.ID == orderId);
            if (pos == null)
            {
                Logger.ErrorFormat("OrderDlg::LoadMarketOrderData - not found ({0})", orderId);
                return;
            }

            Ticker = pos.Symbol;
            orderSide = pos.Side;
            cbVolume.Text = pos.Volume.ToStringUniformMoneyFormat();
            tbSL.Text = pos.StopLoss.ToStringUniformPriceFormat("", true);
            tbTP.Text = pos.TakeProfit.ToStringUniformPriceFormat("", true);
            tbComment.Text = pos.Comment;
            tbExpertComment.Text = pos.ExpertComment;
            Text = string.Format("Ордер №{0} {1} {2}, вход {3}",
                                    pos.ID, pos.Side > 0 ? "BUY" : "SELL", pos.Symbol, 
                                    pos.PriceEnter.ToStringUniformPriceFormat());
            cbCurx.SelectedIndex = cbCurx.Items.IndexOf(pos.Symbol);
            tbMagic.Text = pos.Magic.HasValue ? pos.Magic.Value.ToString() : "";
            tbPrice.Text = pos.PriceEnter.ToStringUniformPriceFormat(true);
            
            // трейлинг
            LoadTrailingLevels(pos.trailingLevels, pos.trailingTargets);
            
            CustomTextButton inactBtn = pos.Side > 0 ? btnSellMarket : btnBuyMarket;
            btnBuyMarket.Enabled = false;
            btnSellMarket.Enabled = false;
            inactBtn.Enabled = true;
            inactBtn.buttonCaption = "Закрыть";
            inactBtn.textLeftColor = SystemColors.InactiveCaptionText;
            inactBtn.textRightColor = SystemColors.InactiveCaptionText;
            inactBtn.captionColor = SystemColors.ControlText;
        }

        private void LoadPendingOrderData()
        {
            RequestStatus status;
            List<PendingOrder> actualPos;
            try
            {
                status = TradeSharpAccount.Instance.proxy.GetPendingOrders(AccountStatus.Instance.accountID, out actualPos);
            }
            catch (Exception ex)
            {
                Logger.Error("OrderDlg::GetPendingOrders proxy error", ex);
                return;
            }
            if (status != RequestStatus.OK) return;
            var pos = actualPos.FirstOrDefault(p => p.ID == orderId);
            if (pos == null)
            {
                Logger.ErrorFormat("OrderDlg::GetPendingOrders - not found ({0})", orderId);
                return;
            }
            pendingOrder = pos;

            Ticker = pos.Symbol;
            orderSide = pos.Side;
            cbVolume.Text = pos.Volume.ToStringUniformMoneyFormat();
            tbSL.Text = pos.StopLoss.ToStringUniformPriceFormat("", true);
            tbTP.Text = pos.TakeProfit.ToStringUniformPriceFormat("", true);
            tbComment.Text = pos.Comment;
            tbExpertComment.Text = pos.ExpertComment;
            Text = string.Format("Отл. ордер №{0} {1} {2} price={3}",
                                    pos.ID, pos.Side > 0 ? "BUY" : "SELL", pos.Symbol, pos.PriceFrom);
            cbCurx.SelectedIndex = cbCurx.Items.IndexOf(pos.Symbol);
            tbMagic.Text = pos.Magic.HasValue ? pos.Magic.Value.ToString() : "";
            tbPrice.Text = pos.PriceFrom.ToStringUniformPriceFormat(true);

            // трейлинг
            LoadTrailingLevels(pos.trailingLevels, pos.trailingTargets);

            // доп параметры
            if (pos.TimeFrom.HasValue)
            {
                cbStartFrom.Checked = true;
                dpTimeFrom.Value = pos.TimeFrom.Value;
            }
            else
                cbStartFrom.Checked = false;
            if (pos.TimeTo.HasValue)
            {
                cbEndTime.Checked = true;
                dpTimeTo.Value = pos.TimeTo.Value;
            }
            else
                cbEndTime.Checked = false;
            if (pos.PriceTo.HasValue)
            {
                cbPriceTo.Checked = true;
                tbPriceTo.Text = pos.PriceTo.Value.ToStringUniformPriceFormat(true);
            }

            // стоп-лимит
            var orderCaption = string.Format("{0} {1}",
                                             pos.Side > 0 ? "BUY" : "SELL",
                                             pos.PriceSide == PendingOrderType.Stop ? "STOP" : "LIMIT");

            // кнопки
            var inactBtn = pos.Side > 0 ? btnSellMarket : btnBuyMarket;
            btnBuyMarket.Enabled = false;
            btnSellMarket.Enabled = false;
            inactBtn.Enabled = true;
            if (pos.Side > 0) btnBuyMarket.buttonCaption = orderCaption;
            else btnSellMarket.buttonCaption = orderCaption;
            inactBtn.textLeftColor = SystemColors.InactiveCaptionText;
            inactBtn.textRightColor = SystemColors.InactiveCaptionText;
            inactBtn.captionColor = SystemColors.ControlText;
            inactBtn.buttonCaption = "Отменить";
        }

        private void QuotePollerOnQuoteHashUpdated(List<string> names, List<QuoteData> quotes)
        {
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i] == Ticker)
                {
                    LastQuote = quotes[i];
                    break;
                }
            }            
        }

        private void OrderDialogLoad(object sender, EventArgs e)
        {
            SetupInterface();
            if (UserSettings.Instance.UseLeverageByDefault)
                AutoCalcVolume();
        }

        private void UpdateQuoteUnsafe()
        {
            var ask = lastQuote == null ? 0 : lastQuote.ask;
            var bid = lastQuote == null ? 0 : lastQuote.bid;
            if (lastQuote == null) return;
            try
            {
                btnBuyMarket.SetPrice(ask, Ticker);
                btnSellMarket.SetPrice(bid, Ticker);
                btnBuyMarket.Invalidate();
                btnSellMarket.Invalidate();
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateQuoteUnsafe: ", ex);
            }
        }

        private void UpdateQuoteSafe()
        {
            try
            {
                BeginInvoke(new Action(UpdateQuoteUnsafe));
            }
            catch (Exception ex)
            {
                Logger.Error("OrderDlg.UpdateQuoteSafe: " + ex.Message);
            }
        }

        private void OrderDialogFormClosing(object sender, FormClosingEventArgs e)
        {
            quotePoller.OnQuoteHashUpdated -= QuotePollerOnQuoteHashUpdated;
            quotePoller.StopPolling();
        }

        private void CbCurxSelectedIndexChanged(object sender, EventArgs e)
        {
            Ticker = (string)cbCurx.SelectedItem;
            LastQuote = QuoteStorage.Instance.ReceiveValue(Ticker);
        }
    
        private void ApplyUIMode()
        {
            cbMode.SelectedIndex = state == OrderDialogMode.OrderEditMarket ||
                                   state == OrderDialogMode.OrderNewMarket
                                       ? 0 : 1;
            cbMode.Enabled = state == OrderDialogMode.OrderNewMarket || state == OrderDialogMode.OrderNewPending;

            tbPrice.Enabled = (state != OrderDialogMode.OrderEditMarket && state != OrderDialogMode.OrderNewMarket);
            cbVolume.Enabled = state != OrderDialogMode.OrderEditMarket;

            cbSlippage.Enabled = state == OrderDialogMode.OrderNewMarket;
            udSlippagePoints.Enabled = cbSlippage.Enabled && cbSlippage.Checked;

            cbOrderOCO.Enabled = IsPending;
            //gridTrailing.Enabled = !IsPending; // отныне для отложенных можно указать трейлинг

            maxTrailLevels = state == OrderDialogMode.OrderNewMarket ? 1 : 4;

            if (state == OrderDialogMode.OrderEditMarket || state == OrderDialogMode.OrderEditPending)
            {
                panelBottom.Height = 0;
                Height += panelBottom.Height;
            }
            //else
            //{
            //}

            cbPriceTo.Enabled = IsPending;
            cbStartFrom.Enabled = IsPending;
            cbEndTime.Enabled = IsPending;
        }

        private void MakeTrade(int side)
        {
            if (state == OrderDialogMode.OrderNewMarket)
            {
                OpenMarket(side, cbSlippage.Checked ? OrderType.Instant : OrderType.Market);
                return;
            }
            if (state == OrderDialogMode.OrderNewPending)
            {
                MakePending(side);
                return;
            }
            if (state == OrderDialogMode.OrderEditMarket)
            {
                CloseMarketOrder();
                return;
            }
            if (state == OrderDialogMode.OrderEditPending)
            {
                CancelPendingOrder();
                return;
            }
        }

        private void CloseMarketOrder()
        {
            try
            {
                var result = MainForm.Instance.SendCloseRequestSafe(AccountStatus.Instance.accountID, orderId, PositionExitReason.ClosedFromUI);
                if (result != RequestStatus.OK)
                    Logger.ErrorFormat("Ошибка закрытия ордера #{0}: {1}", orderId, result);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка закрытия ордера #{0}: {1}", orderId, ex);
            }
            Close();
        }

        private void CancelPendingOrder()
        {
            if (pendingOrder == null) return;
            try
            {
                var result = MainForm.Instance.SendDeletePendingOrderRequestSafe(pendingOrder,
                    PendingOrderStatus.Отменен, null, "Отменен пользователем");
                if (result != RequestStatus.OK)
                    Logger.ErrorFormat("Ошибка закрытия отложенного ордера #{0}: {1}", orderId, result);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка закрытия отложенного ордера #{0}: {1}", orderId, ex);
            }
            Close();
        }

        private void OpenMarket(int side, OrderType ordType)
        {
            Account account = null;
            if (AccountStatus.Instance.connectionStatus == AccountConnectionStatus.Connected)
                account = AccountStatus.Instance.AccountData;
            if (account == null)
            {
                MessageBox.Show(EnumFriendlyName<AccountConnectionStatus>.GetString(AccountConnectionStatus.NotConnected),
                    Localizer.GetString("TitleWarning"));
                return;
            }

            var sl = tbSL.Text.Replace(",", ".").ToDecimalUniformSafe();
            var tp = tbTP.Text.Replace(",", ".").ToDecimalUniformSafe();

            float?[] trailingLevels;
            float?[] trailTargets;
            GetTrailingLevels(side, out trailingLevels, out trailTargets);
            
            var symbol = cbCurx.Text;

            // для инстант-ордера получить текущую цену
            var slipPoints = 0f;
            var requestedPrice = 0f;
            var quote = QuoteStorage.Instance.ReceiveValue(symbol);
            if (cbSlippage.Checked)
            {
                slipPoints = udSlippagePoints.Text.ToFloatUniform();

                if (quote == null)
                {
                    MessageBox.Show(string.Format(Localizer.GetString("MessageNoPriceForInstantOrderFmt"), symbol),
                                    Localizer.GetString("TitleWarning"), 
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                requestedPrice = side > 0 ? quote.ask : quote.bid;
            }

            var volm = cbVolume.Text.Replace(" ", "").ToInt();
            // предупредить трейдера?
            if (!CheckMargin(symbol, side, volm)) return;

            RequestStatus status;
            try
            {
                status = MainForm.Instance.SendNewOrderRequestSafe(
                    RequestUniqueId.Next(),
                    account.ID,
                    new MarketOrder
                    {
                        Volume = volm,
                        Side = side,
                        Symbol = symbol,
                        StopLoss = (float?)(sl != 0 ? sl : null),
                        TakeProfit = (float?)(tp != 0 ? tp : null),
                        Magic = tbMagic.Text.ToIntSafe(),
                        Comment = tbComment.Text,
                        TrailLevel1 = trailingLevels[0],
                        TrailTarget1 = trailTargets[0]
                    },
                    (decimal)requestedPrice, (decimal)slipPoints, ordType);
            }
            catch (Exception ex)
            {
                Logger.Error("Server proxy: SendNewOrderRequest error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageUnableToDeliverRequest"),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (status != RequestStatus.OK)
            {
                MessageBox.Show(
                    EnumFriendlyName<RequestStatus>.GetString(status),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void MakePending(int side)
        {
            var priceVal = tbPrice.Text.Replace(",", ".").ToFloatUniformSafe();
            if (!priceVal.HasValue)
            {
                MessageBox.Show(Localizer.GetString("MessagePriceNotProvided"));
                tbPrice.Focus();
                return;
            }
            var price = priceVal.Value;
            if (price <= 0)
            {
                MessageBox.Show(Localizer.GetString("MessagePriceMustBePositive"));
                tbPrice.Text = "";
                tbPrice.Focus();
                return;
            }

            if (lastQuote == null)
            {
                MessageBox.Show(Localizer.GetString("MessageNoQuote"));
                return;
            }

            // создать новый отложенный ордер
            var volm = cbVolume.Text.Replace(" ", "").ToInt();
            var orderType = PendingOrderType.Limit;
            if ((side == 1 && lastQuote.ask < price) || (side == -1 && lastQuote.bid > price))
                orderType = PendingOrderType.Stop;
            var orderOCO = (PendingOrder)cbOrderOCO.SelectedItem;
            var orderOCOId = orderOCO == null ? (int?)null : orderOCO.ID;

            float?[] trailingLevels;
            float?[] trailTargets;
            GetTrailingLevels(side, out trailingLevels, out trailTargets);

            var order = new PendingOrder
                            {
                                AccountID = AccountStatus.Instance.accountID,
                                Symbol = Ticker,
                                Magic = tbMagic.Text.ToIntSafe(),
                                Volume = volm,
                                Side = side,
                                PriceSide = orderType,
                                PriceFrom = price,
                                PriceTo = tbPriceTo.Text.Replace(",", ".").ToFloatUniformSafe(),
                                TimeFrom = cbStartFrom.Checked ? dpTimeFrom.Value : (DateTime?) null,
                                TimeTo = cbEndTime.Checked ? dpTimeTo.Value : (DateTime?) null,
                                StopLoss = tbSL.Text.Replace(",", ".").ToFloatUniformSafe(),
                                TakeProfit = tbTP.Text.Replace(",", ".").ToFloatUniformSafe(),
                                PairOCO = orderOCOId > 0 ? orderOCO.ID : (int?)null,
                                Comment = tbComment.Text,
                                ExpertComment = tbExpertComment.Text,
                                // трейлинг
                                TrailLevel1 = trailingLevels[0],
                                TrailLevel2 = trailingLevels[1],
                                TrailLevel3 = trailingLevels[2],
                                TrailLevel4 = trailingLevels[3],
                                TrailTarget1 = trailTargets[0],
                                TrailTarget2 = trailTargets[1],
                                TrailTarget3 = trailTargets[2],
                                TrailTarget4 = trailTargets[3],
                            };

            MainForm.Instance.SendNewPendingOrderRequestSafe(RequestUniqueId.Next(), order);
        }

        private bool CheckMargin(string symbol, int side, int volume)
        {
            var minLeverageToWarn = UserSettings.Instance.MessageOnEnterExceedLeverage
                                        ? UserSettings.Instance.RiskLeverCritical
                                        : 10000;
            var equity = AccountStatus.Instance.AccountData.Equity;
            if (equity == 0) return false;
            if (minLeverageToWarn == 0) return true;
            var orders = MarketOrdersStorage.Instance.MarketOrders;
            if (orders == null || orders.Count == 0) return true;
            var depoCurx = AccountStatus.Instance.AccountData.Currency;

            // подсчет плеча
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            var posSum = PositionSummary.GetPositionSummary(quotes, orders, depoCurx);
            var leverage = posSum.Sum(p => !string.IsNullOrEmpty(p.Symbol) ? p.Leverage : 0);
            var volumeBySymbol = posSum.Sum(p => p.Symbol == symbol ? p.Volume : 0);
            var newVolumeBySymbol = volumeBySymbol + (side * volume);
            var newVolumeDepo = ConvertBaseVolumeToDepo(newVolumeBySymbol, symbol, depoCurx, quotes);
            var newSymbolLeverage = Math.Abs(newVolumeDepo / (float)equity);
            var oldSymbolLeverage = posSum.Sum(p => p.Symbol == symbol ? p.Leverage : 0);

            if (newSymbolLeverage <= oldSymbolLeverage) return true; // экспозиция не увеличивается
            var newLeverage = leverage - oldSymbolLeverage + newSymbolLeverage;

            if (newLeverage >= minLeverageToWarn)
            {
                var msg =
                    string.Format(Localizer.GetString("MessageWarningExceedLeverageFmt"), 
                    leverage, newLeverage, minLeverageToWarn);
                if (MessageBox.Show(msg, 
                    Localizer.GetString("TitleWarning"), 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No) 
                    return false;
            }
            return true;
        }

        private static float ConvertBaseVolumeToDepo(int baseVolume, string dealSymbol,
            string depoCurrency, Dictionary<string, QuoteData> curPrices)
        {
            bool inverse, pairsEqual;
            var pair = DalSpot.Instance.FindSymbol(dealSymbol, true, depoCurrency, out inverse, out pairsEqual);
            if (pairsEqual) return baseVolume;
            if (string.IsNullOrEmpty(pair)) return baseVolume;
            if (!curPrices.ContainsKey(pair)) return baseVolume;
            var price = curPrices[pair].GetPrice(QuoteType.Ask);
            if (price == 0) return baseVolume;
            var volume = inverse ? baseVolume / price : baseVolume * price;
            return volume;
        }

        private void BtnSellMarketClick(object sender, EventArgs e)
        {
            MakeTrade(-1);
        }

        private void BtnBuyMarketClick(object sender, EventArgs e)
        {
            MakeTrade(1);
        }

        private void CbModeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (state == OrderDialogMode.OrderEditMarket || state == OrderDialogMode.OrderEditPending)
                return;

            var isMarket = cbMode.SelectedIndex == 0;
            if ((state == OrderDialogMode.OrderNewMarket && isMarket) ||
                (state == OrderDialogMode.OrderNewPending && !isMarket)) return;
            state = isMarket ? OrderDialogMode.OrderNewMarket : OrderDialogMode.OrderNewPending;

            ApplyUIMode();
        }

        private void CbSlippageCheckedChanged(object sender, EventArgs e)
        {
            udSlippagePoints.Enabled = cbSlippage.Checked;
        }

        private void CbExtraCheckedChanged(object sender, EventArgs e)
        {
            if (cbExtra.Checked)
            {
                Height += 200;
                tabControlExtra.Visible = true;
            }
            else
            {
                Height -= 200;
                tabControlExtra.Visible = false;
            }
        }

        private void BtnEditClick(object sender, EventArgs e)
        {
            if (state == OrderDialogMode.OrderEditMarket)
            {
                EditMarketOrder();                
            }

            if (state == OrderDialogMode.OrderEditPending)
            {
                EditPendingOrder();                
            }
            DialogResult = DialogResult.OK;
        }

        private void EditMarketOrder()
        {
            if (orderId < 0) return;
            RequestStatus status;
            List<MarketOrder> orders;
            try
            {
                status = TradeSharpAccount.Instance.proxy.GetMarketOrders(AccountStatus.Instance.accountID, out orders);
            }
            catch (Exception ex)
            {
                Logger.Error("Server proxy: EditMarketOrder error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageUnableToGetOrder"), 
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (status != RequestStatus.OK) return;
            var ord = orders.FirstOrDefault(o => o.ID == orderId);
            if (ord == null) return;

            float?[] trailLevels;
            float?[] trailTargets;
            GetTrailingLevels(orderSide, out trailLevels, out trailTargets);

            // проверяем изменения в ордере и отправляем на сервер
            ord.StopLoss = tbSL.Text.Replace(",", ".").ToFloatUniformSafe();
            ord.TakeProfit = tbTP.Text.Replace(",", ".").ToFloatUniformSafe();
            ord.Comment = tbComment.Text;

            for (var i = 0; i < ord.trailingLevels.Length; i++)
            {
                ord.trailingLevels[i] = trailLevels[i];
                ord.trailingTargets[i] = trailTargets[i];
            }
            
            ord.Magic = tbMagic.Text.ToIntSafe();
            ord.ExpertComment = tbExpertComment.Text;

            // проверяем правильность установки тейка и стопа
            var currQuote = QuoteStorage.Instance.ReceiveValue(ord.Symbol);
            if (currQuote == null)
            {
                MessageBox.Show(
                    Localizer.GetString("MessageCannotEditOrderNoQuote"),
                    Localizer.GetString("TitleError"));
                return;
            }
            if ((ord.Side == 1 && ord.StopLoss >= currQuote.bid) || (ord.Side == -1 && ord.StopLoss <= currQuote.ask))
            {
                MessageBox.Show(Localizer.GetString("MessageStoplossIsIncorrect"),
                    Localizer.GetString("TitleError"));
                return;
            }

            if ((ord.Side == 1 && ord.TakeProfit <= currQuote.bid) || (ord.Side == -1 && ord.TakeProfit >= currQuote.ask))
            {
                MessageBox.Show(Localizer.GetString("MessageTakeprofitIsIncorrect"),
                    Localizer.GetString("TitleError"));
                return;
            }

            try
            {
                MainForm.Instance.SendEditMarketRequestSafe(ord);
            }
            catch (Exception ex)
            {
                Logger.Error("Server proxy: SendEditMarketRequest error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageUnableToDeliverRequest"),
                                Localizer.GetString("TitleError"), 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditPendingOrder()
        {
            if (orderId < 0) return;
            RequestStatus status;
            List<PendingOrder> orders;
            try
            {
                status = TradeSharpAccount.Instance.proxy.GetPendingOrders(AccountStatus.Instance.accountID, out orders);
            }
            catch (Exception ex)
            {
                Logger.Error("Server proxy: EditPendingOrder error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageUnableToGetOrder"),
                    Localizer.GetString("TitleError"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (status != RequestStatus.OK) return;
            var ord = orders.FirstOrDefault(o => o.ID == orderId);
            if (ord == null) return;

            // всякие разные параметры
            ord.Comment = tbComment.Text;
            ord.ExpertComment = tbExpertComment.Text;
            ord.TimeTo = cbEndTime.Checked ? dpTimeTo.Value : (DateTime?) null;
            ord.TimeFrom = cbStartFrom.Checked ? dpTimeFrom.Value : (DateTime?)null;
            if (ord.TimeFrom.HasValue && ord.TimeTo.HasValue)
                if (ord.TimeFrom.Value > ord.TimeTo.Value)
                {
                    var from = ord.TimeFrom;
                    ord.TimeFrom = ord.TimeTo;
                    ord.TimeTo = from;
                }

            var orderOCO = cbOrderOCO.SelectedItem == null ? (int?)null : ((PendingOrder) cbOrderOCO.SelectedItem).ID;
            ord.PairOCO = orderOCO.HasValue ? (orderOCO > 0 ? orderOCO : null) : null;

            // объем входа
            var volm = cbVolume.Text.Replace(" ", "").ToIntSafe();
            if (!volm.HasValue)
            {
                MessageBox.Show(Localizer.GetString("MessageVolumeNotProvided"));
                cbVolume.Focus();
                return;
            }
            if (volm.Value <= 0)
            {
                MessageBox.Show("MessageVolumeMustBePositive");
                cbVolume.Focus();
                return;
            }
            ord.Volume = volm.Value;
            ord.Magic = tbMagic.Text.ToIntSafe();
            ord.StopLoss = tbSL.Text.Replace(",", ".").ToFloatUniformSafe();
            ord.TakeProfit = tbTP.Text.Replace(",", ".").ToFloatUniformSafe();

            // трейлинг
            float?[] trailingLevels;
            float?[] trailTargets;
            GetTrailingLevels(ord.Side, out trailingLevels, out trailTargets);
            for (var i = 0; i < ord.trailingLevels.Length; i++)
            {
                ord.trailingLevels[i] = trailingLevels[i];
                ord.trailingTargets[i] = trailTargets[i];
            }
            
            // цена - и - соотв. - PriceSide
            var priceVal = tbPrice.Text.Replace(",", ".").ToFloatUniformSafe();
            if (!priceVal.HasValue)
            {
                MessageBox.Show(Localizer.GetString("MessagePriceNotProvided"));
                tbPrice.Focus();
                return;
            }
            var price = priceVal.Value;
            if (price <= 0)
            {
                MessageBox.Show(Localizer.GetString("MessagePriceMustBePositive"));
                tbPrice.Text = "";
                tbPrice.Focus();
                return;
            }

            if (lastQuote == null)
            {
                MessageBox.Show(Localizer.GetString("MessageNoQuote"));
                return;
            }
            ord.PriceFrom = price;

            // создать новый отложенный ордер
            ord.PriceSide = PendingOrderType.Limit;
            if ((ord.Side == 1 && lastQuote.ask < price) || (ord.Side == -1 && lastQuote.bid > price))
            ord.PriceSide = PendingOrderType.Stop;

            try
            {
                MainForm.Instance.SendEditPendingRequestSafe(ord);
            }
            catch (Exception ex)
            {
                Logger.Error("Server proxy: SendEditPendingRequestSafe error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageUnableToDeliverRequest"),
                                Localizer.GetString("TitleError"), 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CbPriceToCheckedChanged(object sender, EventArgs e)
        {
            tbPriceTo.Enabled = cbPriceTo.Checked;
        }

        private void CbStartFromCheckedChanged(object sender, EventArgs e)
        {
            dpTimeFrom.Enabled = cbStartFrom.Checked;
        }

        private void CbEndTimeCheckedChanged(object sender, EventArgs e)
        {
            dpTimeTo.Enabled = cbEndTime.Checked;
        }

        /// <summary>
        /// подставить объем из настроек "риска"
        /// </summary>
        private void BtnCalcVolumeClick(object sender, EventArgs e)
        {
            AutoCalcVolume();
        }   
        
        private void AutoCalcVolume()
        {
            var account = AccountStatus.Instance.AccountData;
            if (account == null) return;

            var symbol = cbCurx.Text;
            var leverage = UserSettings.Instance.DealLeverage ?? 0;
            if (leverage == 0) return;

            // из плеча получить объем базовой валюты
            var volmDepo = leverage * account.Equity;

            string errorString;
            var volmBase = DalSpot.Instance.ConvertToTargetCurrency(symbol, true, account.Currency, (double)volmDepo,
                                                     QuoteStorage.Instance.ReceiveAllData(), out errorString);
            if (!string.IsNullOrEmpty(errorString)) return;
            if (!volmBase.HasValue) return;

            // оркуглить
            if (UserSettings.Instance.RoundCalculatedDealVolume)
            {
                var lotSize = DalSpot.Instance.GetMinStepLot(symbol, account.Group);
                volmBase = MarketOrder.RoundDealVolume((int)volmBase.Value, UserSettings.Instance.VolumeRoundType,
                                                       lotSize.a, lotSize.b);
            }

            cbVolume.Text = ((int)volmBase.Value).ToStringUniformMoneyFormat();
        }
    }
}
