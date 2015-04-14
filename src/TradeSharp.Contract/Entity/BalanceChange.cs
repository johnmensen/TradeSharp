using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class BalanceChange
    {
        public const int DescriptionMaxLength = 60;

        public int ID { get; set; }
        public int AccountID { get; set; }
        /// <summary>
        /// пополнение - снятие
        /// </summary>
        public BalanceChangeType ChangeType { get; set; }        
        /// <summary>
        /// объем в валюте, например, в евро
        /// в общем случае валюта не совпадает с валютой депозита
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// курс валюты перевода к валюте депо на момент валютирования
        /// </summary>
        public decimal CurrencyToDepoRate { get; set; }
        /// <summary>
        /// валюта перевода
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// сумма в валюте депо
        /// </summary>
        public decimal AmountDepo
        {
            get { return CurrencyToDepoRate * Amount; }
        }
        /// <summary>
        /// дата валютирования
        /// </summary>
        public DateTime ValueDate { get; set; }
        /// <summary>
        /// коментарий
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// ссылка на ордер, результат которого равен AmountDepo
        /// </summary>
        public int? PositionId { get; set; }

        public decimal SignedAmountDepo
        {
            get
            {
                var sign = ChangeType == BalanceChangeType.Deposit ||
                    ChangeType == BalanceChangeType.Profit ||
                    ChangeType == BalanceChangeType.Swap ? 1 : -1;
                return AmountDepo * sign;
            }
        }
    
        public static int GetSign(BalanceChangeType changeType)
        {
            return changeType == BalanceChangeType.Deposit ||
                   changeType == BalanceChangeType.Profit || changeType == BalanceChangeType.Swap
                       ? 1 : -1;
        }

        public static float CorrectSign(float sum, BalanceChangeType changeType)
        {
            if (changeType == BalanceChangeType.Swap) return sum;
            return GetSign(changeType) * Math.Abs(sum);
        }

        public static decimal CorrectSign(decimal sum, BalanceChangeType changeType)
        {
            if (changeType == BalanceChangeType.Swap) return sum;
            return GetSign(changeType) * Math.Abs(sum);
        }
    }

    public enum BalanceChangeType
    {
        Deposit = 1, Withdrawal = 2, Profit = 3, Loss = 4, Swap = 5
    }
}
