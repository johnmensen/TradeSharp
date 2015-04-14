using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    /// <summary>
    /// Интерфейс сервера МТС.Live (незащищённые методы)
    /// </summary>
    [ServiceContract]
    public interface ITradeSharpAccount
    {
        /// <summary>
        /// получить каналы новостей, на которые подписан счет
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<int> GetAccountChannelIDs(int accountId);

        /// <summary>
        /// зарегить клиента и создать ему демо-счет
        /// </summary>
        [OperationContract(IsOneWay = false)]
        AccountRegistrationStatus RegisterAccount(PlatformUser user, string accountCurrency,
            int startBalance, decimal maxLeverage, string completedPassword, bool autoSignIn);

        /// <summary>
        /// получить службы, работающие на счет
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<PaidService> GetPaidServices(string userLogin);

        [OperationContract(IsOneWay = false)]
        List<PaidService> GetUserOwnedPaidServices(string userLogin);

        [OperationContract(IsOneWay = false)]
        List<Subscription> GetSubscriptions(string userLogin);

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
        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetOrdersByFilter(
            int accountId,
            bool getClosedOrders,
            OrderFilterAndSortOrder filter,
            out List<MarketOrder> orders);

        /// <summary>
        /// получить список отложенных ордеров
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        RequestStatus GetPendingOrders(int accountId,
                                      out List<PendingOrder> orders);

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
        /// напомнить пароль - выслать его на почту
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool RemindPassword(string email, string login);

        /// <summary>
        /// получить все открытые трейды по сигналам, на которые подписан данный счет
        /// 
        /// Id категории - список ордеров
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        Dictionary<int, List<MarketOrder>> GetSignallersOpenTrades(int accountId);

        [OperationContract(IsOneWay = false)]
        List<string> GetTickersTraded(int accountId);

        [OperationContract(IsOneWay = false)]
        List<AccountShareOnDate> GetAccountShareHistory(int accountId, string userLogin);

        [OperationContract(IsOneWay = false)]
        List<MarketOrder> GetClosedOrders(int accountId, string optionalSymbolFilter, int startId, int maxCount);

        [OperationContract(IsOneWay = false)]
        List<Transfer> GetAccountTransfersPartByPart(ProtectedOperationContext ctx, string login, int startId,
                                                     int countMax);

        [OperationContract(IsOneWay = false)]
        List<AccountShare> GetAccountShares(ProtectedOperationContext ctx, int accountId, bool needMoneyShares);

        [OperationContract(IsOneWay = false)]
        RequestStatus GetUserOwnAndSharedAccounts(string login,
            ProtectedOperationContext secCtx, out List<AccountShared> accounts);

        [OperationContract(IsOneWay = false)]
        TransfersByAccountSummary GetTransfersSummary(ProtectedOperationContext ctx, string login);

        [OperationContract(IsOneWay = false)]
        List<int> GetFreeMagicsPool(int accountId, int poolSize);
    }
}
