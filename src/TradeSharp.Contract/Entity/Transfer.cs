using System;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// перевод средств с кошелька / в кошелек пользователя
    /// </summary>
    [Serializable]
    public class Transfer
    {
        /// <summary>
        /// часть первичного ключа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ссылка на PlatformUser - владельца кошелька, первичный (и внешний) ключ
        /// </summary>
        public int User { get; set; }

        public int? RefWallet { get; set; }

        /// <summary>
        /// сумма перевода в исходной валюте
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// сумма перевода в валюте кошелька
        /// </summary>
        public decimal TargetAmount { get; set; }

        /// <summary>
        /// дата-время перевода, так же часть первичного ключа
        /// </summary>
        public DateTime ValueDate { get; set; }

        /// <summary>
        /// комментарий, опциональный
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// ссылка на платную (оплачиваемую) услугу
        /// </summary>
        public int? Subscription { get; set; }

        /// <summary>
        /// ссылка на перевод на / с торгового счета
        /// </summary>
        public int? BalanceChange { get; set; }
    }
}
