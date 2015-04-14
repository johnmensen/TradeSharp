using System;
using System.ComponentModel;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// Описывает "кошелоек" - расчетный счет пользователя,
    /// с которого тот может пополнить торг. счет или оплатить сервисы
    /// типа платных сигналов или же отдать часть кошелька в ДУ по дурости
    /// </summary>
    [Serializable]
    public class Wallet
    {
        /// <summary>
        /// ссылка на PlatformUser - владельца, первичный (и внешний) ключ
        /// </summary>
        [DisplayName("Владелец кошелька")]
        public int User { get; set; }

        /// <summary>
        /// валюта кошелька
        /// </summary>
        [DisplayName("Валюта кошелька")]
        public string Currency { get; set; }

        /// <summary>
        /// текущий баланс кошелька
        /// </summary>
        [DisplayName("Текущий баланс кошелька")]
        public decimal Balance { get; set; }

        private string password;
        /// <summary>
        /// отдельный пароль для доступа к кошельку
        /// </summary>
        public string Password
        {
            get { return password; }
            set
            {
                if (value != null && value.Length > 25)
                    value = value.Substring(0, 25);
                password = value;
            }
        }
    }
}
