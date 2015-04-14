using System;
using System.ComponentModel.DataAnnotations;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// запись в БД о том, что в какой то платёжной системе, каким то пользователем был пополнен кошелёк кампании.
    /// Это просто для истории и отчётности
    /// </summary>
    public class PaymentSystemTransfer
    {
        [LocalizedDisplayName("TitlePaymentNumber")]
        [Range(1, (double)decimal.MaxValue, ErrorMessage = "Значение должно быть числом не меньшим 1")]
        public int Id { get; set; }

        /// <summary>
        /// ссылка на запись в таблице USER_PAYMET_SYSTEM. Это НЕ указание на платёжную систему
        /// </summary>
        public int? UserPaymentSys { get; set; }

        /// <summary>
        /// название платёжной системы, полученное из таблици USER_PAYMET_SYSTEM
        /// </summary>
        [LocalizedDisplayName("TitlePaymentSystem")]
        public PaymentSystem PaymentSys { get; set; }

        /// <summary>
        /// Пользователь (если он известен), который осуществил платёж
        /// </summary>
        [LocalizedDisplayName("TitlePurseUsers")]
        public Wallet Wallet { get; set; }

        /// <summary>
        /// Размер перевода
        /// </summary>
        [LocalizedDisplayName("TitleAmount")]
        public decimal Ammount { get; set; }

        /// <summary>
        /// Валюта перевода
        /// </summary>
        [LocalizedDisplayName("TitleCurrencyTS")]
        public string Currency { get; set; }

        /// <summary>
        /// Дата регистрации платежа в системе T#
        /// </summary>
        [LocalizedDisplayName("TitleDateProcessedPayment")]
        public DateTime DateProcessed { get; set; }

        /// <summary>
        /// Дата регистрации платежа в платёжной системе
        /// </summary>
        [LocalizedDisplayName("TitleDateValuePayment")]
        public DateTime DateValue { get; set; }

        /// <summary>
        /// Комментарий, в котором может содержаться логин пользователя или номер кошелька T#, на который нужно зачислить средства
        /// </summary>
        [LocalizedDisplayName("TitleUserComment")]
        public string Comment { get; set; }

        /// <summary>
        /// Ссылка на "внутреннюю" 
        /// </summary>
        public int? Transfer { get; set; }

        #region Поля на случай если платёж не будет опознан
        public string SourcePaySysAccount { get; set; }
        public string SourcePaySysPurse { get; set; }
        public string SourseFirstName { get; set; }
        public string SourseLastName { get; set; }
        public string SourseEmail { get; set; }
        #endregion       
    }
}
