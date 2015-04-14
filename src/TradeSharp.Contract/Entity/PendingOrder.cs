using System;
using System.Linq;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PendingOrder
    {
        public int ID { get; set; }        
        public PendingOrderType PriceSide { get; set; }
        public int AccountID { get; set; }
        public float PriceFrom { get; set; }
        public float? PriceTo { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string Symbol { get; set; }
        public int Volume { get; set; }
        public int Side { get; set; }
        public int? Magic { get; set; }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    if (value.Length > 64) value = value.Substring(0, 64);
                comment = value;
            }
        }

        private string expertComment;
        public string ExpertComment
        {
            get { return expertComment; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    if (value.Length > 64) value = value.Substring(0, 64);
                expertComment = value;
            }
        }
        
        public float? StopLoss { get; set; }
        public float? TakeProfit { get; set; }
        
        /// <summary>
        /// OCO - one cancels the other
        /// ID отложенного ордера, срабатывание данного ордера отменит указанный
        /// </summary>
        public int? PairOCO { get; set; }
        // поля закрытого ордера        
        public PendingOrderStatus Status { get; set; }
        public string CloseReason { get; set; }
        public DateTime? TimeClosed { get; set; }
        public float? PriceClosed { get; set; }

        #region Trailing
        public string Trailing
        {
            get
            {
                if (trailingLevels.Length == 0)
                    return "";
                var res = new StringBuilder();
                for (var i = 0; i < trailingLevels.Length; i++)
                    if (trailingLevels[i] != null && trailingTargets[i] != null)
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

        public bool OrdersAreSame(PendingOrder ord)
        {
            if (ID != ord.ID) return false;
            if (PriceSide != ord.PriceSide) return false;
            if (AccountID != ord.AccountID) return false;
            if (PriceTo != ord.PriceTo) return false;
            if (TimeFrom != ord.TimeFrom) return false;
            if (TimeTo != ord.TimeTo) return false;
            if (Symbol != ord.Symbol) return false;
            if (Side != ord.Side) return false;
            if (Magic != ord.Magic) return false;
            if (Comment != ord.Comment) return false;
            if (ExpertComment != ord.ExpertComment) return false;
            if (StopLoss != ord.StopLoss) return false;
            if (TakeProfit != ord.TakeProfit) return false;
            if (PairOCO != ord.PairOCO) return false;
            if (Status != ord.Status) return false;
            if (CloseReason != ord.CloseReason) return false;
            if (TimeClosed != ord.TimeClosed) return false;
            if (PriceClosed != ord.PriceClosed) return false;
            if (ord.TrailLevel1 != TrailLevel1 || ord.TrailLevel2 != TrailLevel2 ||
                ord.TrailLevel3 != TrailLevel3 ||
                ord.TrailLevel4 != TrailLevel4 || ord.TrailTarget1 != TrailTarget1 ||
                ord.TrailTarget2 != TrailTarget2 ||
                ord.TrailTarget3 != TrailTarget3 || ord.TrailTarget4 != TrailTarget4) return false;

            return true;
        }

        public PendingOrder() {}

        public PendingOrder(PendingOrder o)
        {
            ID = o.ID;
            PriceSide = o.PriceSide;
            AccountID = o.AccountID;
            PriceFrom = o.PriceFrom;
            PriceTo = o.PriceTo;
            TimeFrom = o.TimeFrom;
            TimeTo = o.TimeTo;
            Symbol = o.Symbol;
            Volume = o.Volume;
            Side = o.Side;
            Magic = o.Magic;
            comment = o.comment;
            expertComment = o.expertComment;
            StopLoss = o.StopLoss;
            TakeProfit = o.TakeProfit;
            PairOCO = o.PairOCO;
            CloseReason = o.CloseReason;
            TimeClosed = o.TimeClosed;
            PriceClosed = o.PriceClosed;
            trailingLevels = o.trailingLevels.ToArray();
            trailingTargets = o.trailingTargets.ToArray();
        }

        public override string ToString()
        {
            if (Side == 0) return "-";
            return (Side > 0 ? "BUY " : "SELL ") + PriceSide + " " + Volume.ToStringUniformMoneyFormat() + " " + Symbol;
        }
    }

    public enum PendingOrderStatus
    {
        Создан = 0, Исполнен = 1, Отменен = 2, ОшибкаИсполнения = 3
    }

    /// <summary>
    /// Пример: ордер Stop (BUY) сработает, когда цена поднимется выше PriceFrom
    /// Ордер Limit (BUY) - ... ниже PriceFrom
    /// </summary>
    public enum PendingOrderPriceSide
    {
        Stop = 1, Limit = 2
    }
}