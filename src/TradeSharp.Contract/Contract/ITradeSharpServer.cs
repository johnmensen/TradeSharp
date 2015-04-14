using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    /// <summary>
    /// контракт реализует сервер
    /// контракт доступен только в интрасети для сервисов, не для доступа извне
    /// </summary>
    [XmlSerializerFormat]
    [ServiceContract]
    public interface ITradeSharpServer
    {
        /// <summary>
        /// получить информацию по всем авторам сигналов / ДУшникам и т.д.
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<PerformerStat> GetAllManagers(PaidServiceType? serviceTypeFilter);

        /// <summary>
        /// получить список эталонных счетов для портфелей Компании
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<PerformerStat> GetCompanyTopPortfolioManagedAccounts();

        /// <summary>
        /// Зачисление на TradeSharp кошелёк средств. 
        /// </summary>
        /// <param name="walletId">TradeSharp кошелёк плательщика</param>
        /// <param name="currency">Валюта, в которой был осуществлён перевод в платёжной системе (валюта кошелька кампании)</param>
        /// <param name="amount">Количество зачисляемых средств</param>
        /// <param name="data">Дата проводки</param>
        [OperationContract(IsOneWay = false)]
        bool DepositOnWallet(int walletId, string currency, decimal amount, DateTime data);

        /// <summary>
        /// Обрабатывает платеж, который числиться как "неопознанный". Зачисляет деньги на счёт соответствующего пользователя
        /// </summary>
        /// <param name="paymentTransferId">Уникальный идентификатор редактируемой записи из таблици PAYMENT_SYSTEM_TRANSFER</param>
        /// <param name="paymentSysId">Уникальный идентификатор записи из таблици USER_PAYMENT_SYSTEM. Нужно в тех случаях, когда значение поля UserPaymentSys в PAYMENT_SYSTEM_TRANSFER равно null</param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        bool RegistrationUndefinedTransfer(int paymentTransferId, int? paymentSysId);
    }
}
