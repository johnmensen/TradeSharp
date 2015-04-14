using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    public partial interface ITradeSharpServerTrade
    {
        [OperationContract(IsOneWay = true)]
        void Ping();
        
        /// <summary>
        /// Относится к торговому модулю, т.к. подписывает на колбеки
        /// </summary>                
        [OperationContract(IsOneWay = false)]
        AuthenticationResponse GetUserAccounts(string login, ProtectedOperationContext ctx,
            out List<int> accounts, out List<AccountRights> roles);

        [OperationContract(IsOneWay = false)]
        AuthenticationResponse GetUserAccountsWithDetail(string login, ProtectedOperationContext secCtx,
                                                         out List<Account> accounts);

        [OperationContract(IsOneWay = false)]
        AuthenticationResponse GetUserOwnedAccountsWithActualBalance(string login, ProtectedOperationContext secCtx, bool realOnly,
                                                         out List<Account> accounts);

        [OperationContract(IsOneWay = false)]
        bool SelectAccount(ProtectedOperationContext ctx, int accountId);

        [OperationContract(IsOneWay = false)]
        AuthenticationResponse GetUserDetail(string login, string password, out PlatformUser user);

        [OperationContract(IsOneWay = false)]
        AuthenticationResponse ModifyUserAndAccount(string oldLogin, string oldPassword,
            PlatformUser user, int? accountId, float accountMaxLeverage, out bool loginIsBusy);

        /// <summary>
        /// получить данные по счету
        /// </summary>
        /// <param name="accountId">account</param>
        /// <param name="needEquityInfo">расчитать текущие средства (затратная операция)</param>
        /// <param name="account">информация по счету</param>        
        /// <returns>успешно завершилась</returns>
        [OperationContract(IsOneWay = false)]
        RequestStatus GetAccountInfo(int accountId, bool needEquityInfo,
                                     out Account account);

        /// <summary>
        /// получить список открытых позиций
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetMarketOrders(int accountId,
                                      out List<MarketOrder> orders);

        /// <summary>
        /// получить список закрытых позиций в упакованном виде (MarketOrderSerializer)
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetHistoryOrdersCompressed(int? accountId, DateTime? startDate,
                                      out byte[] buffer);

        /// <summary>
        /// получить список закрытых позиций
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetHistoryOrders(int? accountId, DateTime? startDate,
                                      out List<MarketOrder> orders);

        /// <summary>
        /// получить список закрытых позиций
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetHistoryOrdersByCloseDate(int? accountId, DateTime? startDate,
                                      out List<MarketOrder> orders);


        /// <summary>
        /// получить список отложенных ордеров
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetPendingOrders(int accountId,
                                      out List<PendingOrder> orders);

        /// <summary>
        /// получить каналы новостей, на которые подписан счет
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        List<int> GetAccountChannelIDs(int accountId);

        /// <summary>
        /// получить все движения по счету
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus GetBalanceChanges(int accountId, DateTime? startTime, out List<BalanceChange> balanceChanges);

        /// <summary>
        /// изменить баланс
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus ChangeBalance(int accountId, decimal summ, string comment);

        /// <summary>
        /// зарегить клиента и создать ему демо-счет
        /// </summary>
        [OperationContract(IsOneWay = false)]
        AccountRegistrationStatus RegisterAccount(PlatformUser user, string accountCurrency,
            int startBalance, decimal maxLeverage, string completedPassword, bool autoSignIn);

        /// <summary>
        /// напомнить пароль - выслать его на почту
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool RemindPassword(string email, string login);

        /// <summary>
        /// изменить пароль
        /// </summary>
        [OperationContract(IsOneWay = false)]
        AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword);

        /// <summary>
        /// получить категории ТС, на которые подписан либо которыми управляет счет
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<TradeSignalCategory> GetTradeSignalsSubscribed(int accountId);

        [OperationContract(IsOneWay = false)]
        List<TradeSignalCategory> GetAllTradeSignals();

        /// <summary>
        /// подписаться либо отписаться от торговых сигналов
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool BindToTradeSignal(ProtectedOperationContext secCtx,
            string userLogin, int accountId, TradeSignalCategory cat, out WalletError error);

        /// <summary>
        /// получить события, связанные со счетом (торговые сигналы, лог операций и др)
        /// </summary>
        /// <param name="ctx">контекст операции</param>
        /// <param name="accountId">номер счета</param>
        /// <param name="deleteReceivedEvents">удалить все события на стороне сервера</param>
        /// <returns>список событий по счету</returns>
        [OperationContract(IsOneWay = false)]
        List<AccountEvent> GetAccountEvents(ProtectedOperationContext ctx, int accountId,
            bool deleteReceivedEvents);

        /// <summary>
        /// отправить получателям торгового сигнала событие - текстовое сообщение
        /// </summary>        
        [OperationContract(IsOneWay = true)]
        void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId, int tradeSignalCategory,
            AccountEvent acEvent);

        /// <summary>
        /// создать или удалить пользователя с правами на просмотр счета
        /// </summary>
        [OperationContract(IsOneWay = false)]
        CreateReadonlyUserRequestStatus MakeOrDeleteReadonlyUser(ProtectedOperationContext secCtx,
                                                                int accountId, bool makeNew, string pwrd,
                                                                out PlatformUser user);

        /// <summary>
        /// запросить существующего Readonly-пользователя для счета
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus QueryReadonlyUserForAccount(ProtectedOperationContext secCtx, int accountId, out PlatformUser user);
    }
}
