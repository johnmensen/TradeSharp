using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [XmlSerializerFormat]
    [ServiceContract]
    public interface IWalletManager
    {
        /// <summary>
        /// получить данные кошелька - валюта, баланс,
        /// поле Пароль не заполнено
        /// </summary>
        [OperationContract(IsOneWay = false)]
        Wallet GetUserWallet(ProtectedOperationContext ctx, string userLogin);

        [OperationContract(IsOneWay = false)]
        Wallet GetUserWalletSubscriptionAndLastPayments(ProtectedOperationContext ctx, string userLogin,
            int maxPaymentsQuery, 
            out int paymentsTotalCount,
            out List<Subscription> subscriptions, 
            out List<Transfer> transfers, out WalletError error);

        /// <summary>
        /// получить все проводки по счетам пользователя
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<Transfer> GetAllUserPayments(ProtectedOperationContext ctx, PlatformUser user, out WalletError error);

        /// <summary>
        /// зарегистрировать платную услугу или обновить описание (параметры)
        /// платной услуги
        /// 
        /// опционально указывается прогрессивная шкала комиссии
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool RegisterOrUpdateService(ProtectedOperationContext ctx, PaidService service, out WalletError error);

        /// <summary>
        /// отключить платную услугу
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool DisableService(ProtectedOperationContext ctx, int serviceId, out WalletError error);

        /// <summary>
        /// перевести деньги с кошелька на торговый счет
        /// </summary>
        [OperationContract(IsOneWay = false)]
        Wallet TransferToTradingAccount(ProtectedOperationContext ctx, string userLogin,
            int accountId, decimal amountInWalletCurrency, out WalletError error);

        /// <summary>
        /// перевести деньги с торгового счета на кошелек
        /// </summary>
        [OperationContract(IsOneWay = false)]
        Wallet TransferToWallet(ProtectedOperationContext ctx,
            string userLogin, int accountId, decimal amountInAccountCurrency, out WalletError error);

        /// <summary>
        /// получить описание услуги (сигналы, ПАММ) вместе с краткой информацией о владельце
        /// </summary>
        [OperationContract(IsOneWay = false)]
        PaidService GetPaidServiceDetail(int serviceId, out PlatformUser serviceOwner);

        /// <summary>
        /// получить описание услуг, предоставляемых пользователем
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<PaidService> GetUserOwnerPaidServices(int userId);

        /// <summary>
        /// Редактирует балланс указанного кошелька
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="volume"></param>
        /// <param name="deposit">если true, то происходит зачисление. Иначе - списание</param>
        /// <param name="error"></param>
        [OperationContract(IsOneWay = false)]
        Wallet UpdateBalance(int walletId, decimal volume, bool deposit, out WalletError error);

        /// <summary>
        /// Изменение валюты кошелька
        /// </summary>
        [OperationContract(IsOneWay = false)]
        Wallet ChangeCurrency(int walletId, string walletCurrency, bool recalculationBalance, out WalletError error);

        #region Payment Systems

        [OperationContract(IsOneWay = false)]
        List<UserPaymentSystem> GetUserRegistredPaymentSystemWallets(ProtectedOperationContext ctx, string userLogin,
            string walletPwrd, out WalletError error);

        [OperationContract(IsOneWay = false)]
        WalletError ChangePaymentSystemWallets(ProtectedOperationContext ctx, UserPaymentSystem paySys, bool remove,
            string userLogin, string walletPwrd);

        [OperationContract(IsOneWay = false)]
        WalletError SetPaymentWalletsBySystem(ProtectedOperationContext ctx, 
            PaymentSystem syst,
            List<UserPaymentSystem> actualPaySys, 
            string userLogin, string walletPwrd);

        [OperationContract(IsOneWay = false)]
        WalletError GetTransferExtendedInfo(ProtectedOperationContext ctx, 
            int transferId, out BalanceChange balanceChange, out PlatformUser user);
        #endregion        
    
        #region PAMM

        [OperationContract(IsOneWay = false)]
        RequestStatus InvestInPAMM(ProtectedOperationContext secCtx, string login, int accountId,
                                   decimal sumInWalletCurrency);

        [OperationContract(IsOneWay = false)]
        RequestStatus WithdrawFromPAMM(ProtectedOperationContext secCtx, string login, int accountId,
                                       decimal sumInWalletCurrency, bool withdrawAll);

        #endregion
    }

    public enum WalletError
    {
        OK = 0,
        ServerError = 1,            // сервер сфейлил
        CommonError = 2,            // например, сфейлил WCF,
        InsufficientFunds = 3,      // недостаточно средств,
        MarginExceeded = 4,         // недостаточно средств под марж. залог счета
        CurrencyExchangeFailed = 5, // невозможно конвертировать валюту
        WalletPasswordWrong = 6,    // неверный пароль кошелька
        InsufficientRights = 7,     // недостаточно прав пользователя
        AuthenticationError = 8,    // пользователь не найден / пароль неверный
        InvalidData = 9,            // некорректные данные
        ServerSqlError = 10         // не удалось отредактировать базу данных
    }
}
