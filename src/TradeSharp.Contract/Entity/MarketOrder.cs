using System;
using System.ComponentModel;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    public enum VolumeRoundType { Ближайшее = 0, Вниз = 1, Вверх = 2 }

    [Serializable]
    public class MarketOrder
    {
// ReSharper disable LocalizableElement
        [DisplayName("ID")]
// ReSharper restore LocalizableElement
        public int ID { get; set; }
        [LocalizedDisplayName("TitleAccount")]
        public int AccountID { get; set; }
        [LocalizedDisplayName("TitleVolume")]
        public int Volume { get; set; }
        [LocalizedDisplayName("TitleInstruments")]
        public string Symbol { get; set;}
        [LocalizedDisplayName("TitleOrderType")]
        public int Side { get; set; }
        [LocalizedDisplayName("TitleStopLoss")]
        public float? StopLoss { get; set; }
        [LocalizedDisplayName("TitleTakeProfit")]
        public float? TakeProfit { get; set; }
        [LocalizedDisplayName("TitlePriceEntry")]
        public float PriceEnter { get; set; }
        [LocalizedDisplayName("TitlePriceExit")]
        public float? PriceExit { get; set; }
        [LocalizedDisplayName("TitleTimeEnter")]
        public DateTime TimeEnter { get; set; }
        [LocalizedDisplayName("TitleTimeExit")]
        public DateTime? TimeExit { get; set; }
        
        /// <summary>
        /// рез-т в валюте депозита
        /// </summary>
        [LocalizedDisplayName("TitleResultDepo")]
        public float ResultDepo { get; set; }
        /// <summary>
        /// рез-т в базовой валюте
        /// </summary>
        [LocalizedDisplayName("TitleResultBase")]
        public float ResultBase { get; set; }

        [LocalizedDisplayName("TitleResultPoints")]
        public float ResultPoints { get; set; }
        /// <summary>
        /// своп в контрвалюте
        /// </summary>
        [LocalizedDisplayName("TitleSwap")]
        public float? Swap { get; set; }

        [LocalizedDisplayName("TitleState")]
        public PositionState State { get; set; }

        [LocalizedDisplayName("TitleExitReason")]
        public PositionExitReason ExitReason { get; set; }

        [LocalizedDisplayName("TitleComment")]
        [EntityFilter("Комментарий", CheckAuto = true)]
        public string Comment { get; set; }
        
        [EntityFilter("Комментарий робота", CheckAuto = true)]
        [LocalizedDisplayName("TitleExpertComment")]
        public string ExpertComment { get; set; }

        [LocalizedDisplayName("TitleMagic")]
        public int? Magic { get; set; }

        [LocalizedDisplayName("TitlePendingOrder")]
        public int? PendingOrderID { get; set; }
        
        [EntityFilter("Лучшая цена", CheckAuto = true)]
        [LocalizedDisplayName("TitlePriceBest")]
        public float? PriceBest { get; set; }
        
        [EntityFilter("Худшая цена", CheckAuto = true)]
        [LocalizedDisplayName("TitlePriceWorst")]
        public float? PriceWorst { get; set; }

        [LocalizedDisplayName("TitleMasterOrder")]
        public int? MasterOrder { get; set; }

        #region Trailing
        [LocalizedDisplayName("TitleTrailing")]
        public string Trailing 
        { 
            get
            {
                if (trailingLevels.Length == 0)
                    return "";
                var res = new StringBuilder();
                for (var i = 0; i < trailingLevels.Length; i++)
                    if (trailingLevels[i] != null &&  trailingTargets[i] != null)
                    res.Append(string.Format("[{0}:{1}]", trailingLevels[i], trailingTargets[i]));
                return res.ToString();
            } 

            set 
            { 
                var parts = value.ToDecimalArrayUniform();
                for (var i = 0; i < trailingLevels.Length; i++)
                {
                    trailingLevels[i] = null;
                    trailingTargets[i] = null;
                }

                var endIndex = Math.Min(parts.Length / 2, trailingLevels.Length);
                for (var i = 0; i < endIndex; i++)
                {
                    var level = parts[i << 1];
                    var target = parts[(i << 1) + 1];
                    trailingLevels[i] = (float)level;
                    trailingTargets[i] = (float)target;
                }
            }
        }
        // ордера должны быть упорядочены по возрастанию с 1 по 4
        public readonly float?[] trailingLevels = new float?[4];
        public readonly float?[] trailingTargets = new float?[4];

        public float? TrailLevel1
        {
            get { return trailingLevels[0]; }
            set { trailingLevels[0] = value; }
        }
        public float? TrailLevel2
        {
            get { return trailingLevels[1]; }
            set { trailingLevels[1] = value; }
        }
        public float? TrailLevel3
        {
            get { return trailingLevels[2]; }
            set { trailingLevels[2] = value; }
        }
        public float? TrailLevel4
        {
            get { return trailingLevels[3]; }
            set { trailingLevels[3] = value; }
        }
        public float? TrailTarget1
        {
            get { return trailingTargets[0]; }
            set { trailingTargets[0] = value; }
        }
        public float? TrailTarget2
        {
            get { return trailingTargets[1]; }
            set { trailingTargets[1] = value; }
        }
        public float? TrailTarget3
        {
            get { return trailingTargets[2]; }
            set { trailingTargets[2] = value; }
        }
        public float? TrailTarget4
        {
            get { return trailingTargets[3]; }
            set { trailingTargets[3] = value; }
        }
        #endregion

        // вероятно, вычисляемые поля
        public float VolumeInDepoCurrency { get; set; }
        
        public bool IsOpened
        {
            get { return State == PositionState.Opened; }
        }
        public bool IsClosed
        {
            get { return State == PositionState.Closed; }
        }

        public float CalculateProfit(float priceExit)
        {
            return (priceExit - PriceEnter) * Volume * Side;
        }

        public float CalculateProfit(QuoteData q)
        {
            var priceExit = Side > 0 ? q.bid : q.ask;
            return CalculateProfit(priceExit);
        }

        public bool CompareOrders(MarketOrder order)
        {
            return order.AccountID == AccountID && order.Comment == Comment && order.ExitReason == ExitReason &&
                   order.ExpertComment == ExpertComment && order.ID == ID && order.Magic == Magic &&
                   order.PendingOrderID == PendingOrderID &&
                   order.PriceBest == PriceBest && order.PriceEnter == PriceEnter && order.PriceExit == PriceExit &&
                   order.PriceWorst == PriceWorst &&
                   order.ResultBase == ResultBase && order.ResultDepo == ResultDepo &&
                   order.ResultPoints == ResultPoints &&
                   order.Side == Side && order.State == State && order.StopLoss == StopLoss && order.Swap == Swap &&
                   order.Symbol == Symbol && order.TakeProfit == TakeProfit && order.TimeEnter == TimeEnter &&
                   order.TimeExit == TimeExit &&
                   order.TrailLevel1 == TrailLevel1 && order.TrailLevel2 == TrailLevel2 &&
                   order.TrailLevel3 == TrailLevel3 &&
                   order.TrailLevel4 == TrailLevel4 && order.TrailTarget1 == TrailTarget1 &&
                   order.TrailTarget2 == TrailTarget2 &&
                   order.TrailTarget3 == TrailTarget3 && order.TrailTarget4 == TrailTarget4 && order.Volume == Volume &&
                   order.VolumeInDepoCurrency == VolumeInDepoCurrency && order.MasterOrder == MasterOrder;
        }

        public MarketOrder MakeCopy()
        {
            var order = new MarketOrder
                            {
                                ID = ID,
                                AccountID = AccountID,
                                Volume = Volume,
                                Symbol = Symbol,
                                Side = Side,
                                StopLoss = StopLoss,
                                TakeProfit = TakeProfit,
                                PriceEnter = PriceEnter,
                                PriceExit = PriceExit,
                                TimeEnter = TimeEnter,
                                TimeExit = TimeExit,
                                ResultDepo = ResultDepo,
                                ResultBase = ResultBase,
                                ResultPoints = ResultPoints,
                                Swap = Swap,
                                State = State,
                                ExitReason = ExitReason,
                                Comment = Comment,
                                ExpertComment = ExpertComment,
                                Magic = Magic,
                                PendingOrderID = PendingOrderID,
                                PriceBest = PriceBest,
                                PriceWorst = PriceWorst,
                                VolumeInDepoCurrency = VolumeInDepoCurrency,
                                MasterOrder = MasterOrder
                            };

            for (var i = 0; i < trailingLevels.Length; i++)
                order.trailingLevels[i] = trailingLevels[i];
            for (var i = 0; i < trailingTargets.Length; i++)
                order.trailingTargets[i] = trailingTargets[i];

            return order;
        }

        public static int RoundDealVolume(int srcVolume, VolumeRoundType roundType, int minVolume, int volumeStep)
        {
            if (srcVolume < minVolume)
            {
                var хренТорганешь = true;
                if (roundType != VolumeRoundType.Вниз)
                {
                    var minThreshold =
                        roundType == VolumeRoundType.Ближайшее ? minVolume * 3 / 4 : minVolume / 2;
                    if (srcVolume > minThreshold) хренТорганешь = false;
                }
                if (хренТорганешь) return 0;                
            }
            var surVolume = srcVolume - minVolume;
            var stepsCount = surVolume / (double)minVolume;
            var stepsInt = roundType == VolumeRoundType.Вниз
                               ? (int)stepsCount
                               : roundType == VolumeRoundType.Вверх
                                     ? (int)Math.Ceiling(stepsCount)
                                     : (int)Math.Round(stepsCount);
            surVolume = stepsInt * volumeStep;
            srcVolume = minVolume + surVolume;
            return srcVolume;
        }

        public override string ToString()
        {
            return ToStringShort();
        }

        public string ToStringShort()
        {
            return string.Format("#{0} {1} {2} {3}", ID, Side > 0 ? "BUY" : "SELL", Volume.ToStringUniformMoneyFormat(),
                                 Symbol);
        }

        public string ToString(bool showState, bool showEnterExit)
        {
            return showEnterExit 
                ? string.Format("#{0} {1} {2} {3}{4} ({5:f4}{6})", ID, Side > 0 ? "BUY" : "SELL", Volume.ToStringUniformMoneyFormat(),
                    Symbol, showState ? ", " + State : "",
                    PriceEnter,
                    PriceExit.HasValue ? " - " + PriceExit.Value.ToString("f4", CultureProvider.Common) : "")
                : string.Format("#{0} {1} {2} {3}{4}", ID, Side > 0 ? "BUY" : "SELL", Volume.ToStringUniformMoneyFormat(),
                    Symbol, showState ? ", " + State : "");
        }

        #region Торговые сигналы
        public static string MakeSignalComment(int signalCatId)
        {
            return string.Format("sigCat={0}", signalCatId);
        }

        public static bool GetTradeSignalFromDeal(MarketOrder order, out int signalCatId, out int parentDealId)
        {
            signalCatId = 0;
            parentDealId = 0;

            if (string.IsNullOrEmpty(order.ExpertComment) || !order.MasterOrder.HasValue) return false;
            const string startPreffix = "sigCat=";
            if (!order.ExpertComment.StartsWith(startPreffix)) return false;
            var orderSignalCat = order.ExpertComment.Substring(startPreffix.Length).ToIntSafe();
            if (!orderSignalCat.HasValue) return false;

            signalCatId = orderSignalCat.Value;
            parentDealId = order.MasterOrder.Value;
            return true;
        }
        #endregion        
    }

    public delegate void OrdersUpdatedDel();
}
