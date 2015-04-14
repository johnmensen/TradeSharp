using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [Serializable]
    public class PaidServiceDetail
    {
        public string SignalTitle { get; set; }

        public string UserName { get; set; }

        public decimal DayFee { get; set; }

        public decimal MonthFee { get; set; }

        public string Currency { get; set; }

        public int SubscribersCount { get; set; }

        public string AccountGroup { get; set; }

        public int? AccountId { get; set; }
    }

    [XmlSerializerFormat]
    [ServiceContract]
    public interface IPlatformManager
    {
        /// <summary>
        /// зарегаться и получить токен
        /// </summary>
        [OperationContract(IsOneWay = false)]
        string AuthoriseUser(string userLogin, string password, long localTime);

        [OperationContract(IsOneWay = false)]
        string AuthoriseUserWithAccountDetail(string userLogin, string password, long localTime, out Account[] userAccounts);

        /// <summary>
        /// получить кошелек пользователя, его подписки и платежи
        /// </summary>
        [OperationContract(IsOneWay = false)]
        Wallet GetUserWalletSubscriptionAndLastPayments(
            string hash,
            string userLogin,
            long localTime,
            int maxPaymentsQuery,
            out int paymentsTotalCount,
            out List<Subscription> subscriptions,
            out List<Transfer> transfers,
            out WalletError error);

        /// <summary>
        /// получить счета пользователя
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<Account> GetUserAccounts(string hash, string userLogin, long localTime, out RequestStatus error);

        [OperationContract(IsOneWay = false)]
        Account GetAccountDetail(string hash, string userLogin, long localTime,
            int accountId, bool calculateEquity, out decimal brokerLeverage, out decimal exposure);
        
        /// <summary>
        /// получить подписки пользователя
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<Subscription> GetUserSubscriptions(string hash, string userLogin, long localTime,
                                                              out RequestStatus error);

        /// <summary>
        /// подписаться на торговый сигнал, указать настройки автоматической торговли
        /// </summary>
        [OperationContract(IsOneWay = false)]
        WalletError SubscribeOnTradeSignal(string hash, string userLogin, long localTime, int serviceId,
            bool tradeAuto, bool enableHedgingOrders, int percentLeverage, int maxVolume, int minVolume,
            int volumeStep, double maxLeverage);

        /// <summary>
        /// отписаться от торгового сигнала
        /// </summary>
        [OperationContract(IsOneWay = false)]
        WalletError UnsubscribeFromTradeSignal(string hash, string userLogin, long localTime, int serviceId);

        /// <summary>
        /// получить информацию по платному сервису (торговому сигналу)
        /// </summary>
        [OperationContract(IsOneWay = false)]
        PaidServiceDetail GetPaidServiceDetail(int serviceId);

        /// <summary>
        /// получить email-адреса подписчиков для пользователя
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<string> GetSubscriberEmails(int signallerId);

        /// <summary>
        /// получить email-адреса подписчиков для пользователя (полученному по его счету)
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<string> GetSubscriberEmailsByAccount(int accountId);
        
        /// <summary>
        /// зарегать счет в системе
        /// </summary>
        [OperationContract(IsOneWay = false)]
        AccountRegistrationResponse RegisterAccount(
            string login,
            string password,
            string email,
            string firstName,
            string lastName,
            string patronym,
            string phone,
            string currency, int startBalance);

        /// <summary>
        /// получить информацию о пользователе и его торговых счетах
        /// </summary>
        [OperationContract(IsOneWay = false)]
        PlatformUser GetUserFullInfo(string hash, string userLogin, long localTime, out List<Account> accounts);

        [OperationContract(IsOneWay = false)]
        PlatformVolumesSummary GetPairVolumes(string semicolonSeparatedTickersListString);

        [OperationContract(IsOneWay = true)]
        void SaveUserSettingsString(string hash, string userLogin, long localTime, string settingsString);

        [OperationContract(IsOneWay = false)]
        string LoadUserSettingsString(string hash, string userLogin, long localTime);

        [OperationContract(IsOneWay = false)]
        bool ModifyOrder(MarketOrder order, out string errorString);

        #region Портфели
        /// <summary>
        /// подписать на портфель
        /// отключить остальные подписки
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SubscribeOnPortfolio(string hash, string userLogin, long localTime,
            int portfolioId, AutoTradeSettings tradeAutoSettings);

        /// <summary>
        /// подписать пользователя на его индивидуальный портфель, заданный формулой
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SubscribeOnCustomPortfolio(
            string hash, string userLogin, long localTime,
            string formula, float? marginValue, int topCount,
            AutoTradeSettings tradeAutoSettings);

        /// <summary>
        /// отписаться от портфеля
        /// опционально - отписаться от всех подписок в составе портфеля
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus UnsubscribePortfolio(string hash, string userLogin, long localTime, 
            bool deleteSubscriptions);

        /// <summary>
        /// получить портфель пользователя (портфель, на который подписан пользователь)
        /// и те сигналы, что составляют портфель
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus GetUserPortfolioAndSubscriptions(string hash, string userLogin, long localTime,
            out List<Subscription> signals, out TopPortfolio portfolio);
        #endregion

        #region Ордера
        /// <summary>
        /// получить count последних открытых ордеров системы
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<MarketOrder> GetAllOpenedOrders(int count);

        /// <summary>
        /// закрыть сделку по ее ID
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendCloseRequest(int orderId, PositionExitReason reason);

        /// <summary>
        /// закрыть несколько сделок разом
        /// </summary>
        [OperationContract(IsOneWay = false)]
        int SendCloseRequests(int[] orderIds, PositionExitReason reason);

        [OperationContract(IsOneWay = false)]
        List<string> GetTickersTraded(string hash, string userLogin, int accountId, long localTime);

        [OperationContract(IsOneWay = false)]
        List<MarketOrder> GetClosedOrders(string hash, string userLogin, long localTime,
            int accountId, string optionalSymbolFilter, int startId, int maxCount);

        [OperationContract(IsOneWay = false)]
        List<MarketOrder> GetOpenOrdersByAccount(string hash, string userLogin, long localTime,
            int accountId, string optionalSymbolFilter, int startId, int maxCount);

        [OperationContract(IsOneWay = false)]
        RequestStatus OpenPosition(string hash, string userLogin, long localTime, int accountId,
                                   string symbol, int volume, int side, float stopLoss, float takeProfit, int magic,
                                   string comment);

        [OperationContract(IsOneWay = false)]
        RequestStatus ClosePosition(string hash, string userLogin, long localTime, int accountId,
                                    int positionId);

        [OperationContract(IsOneWay = false)]
        RequestStatus EditPosition(string hash, string userLogin, long localTime, int accountId,
                                   int orderId,
                                   float stopLoss, float takeProfit, int magic, string comment);
        #endregion
    }
}
