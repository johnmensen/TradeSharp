using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    /// <summary>
    /// Интерфейс сервера МТС.Live
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ITradeSharpServerCallback))]
    public interface ITradeSharpServerTrade
    {
        /// <param name="login">логин</param>
        /// <param name="hash">хэш из параметров, включающих пароль</param>
        /// <param name="terminalId">Id терминала</param>
        /// <param name="clientTime">клиентское локальное время</param>
        /// <param name="sessionTag">тег пользовательской сессии</param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        AuthenticationResponse Authenticate(
            string login, string hash,
            string terminalVersion,
            long terminalId, long clientTime,
            out int sessionTag);

        [OperationContract(IsOneWay = true)]
        void Logout(ProtectedOperationContext ctx);
        
        [OperationContract(IsOneWay = true)]
        void ReviveChannel(ProtectedOperationContext ctx, string login, int accountId, string terminalVersion);

        /// <summary>
        /// запрос - создать новый ордер - обычный или инстантный
        /// сервер принимает заявку, пытается ее исполнить и отправляет ответ
        /// </summary>
        /// <param name="ctx">инф. об отправителе, проверяется</param>
        /// <param name="requestUniqueId">уникальный Id запроса - во избежание дублей</param>
        /// <param name="requestedPrice">для вычисления проскальзывания, Instant-ордера</param>
        /// <param name="slippagePoints">max slippage for instant orders</param>
        /// <param name="order">все параметры ордера - здесь</param>
        /// <param name="orderType">обычный - инстант</param>
        /// <returns>заявка принята - либо ошибка</returns>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendNewOrderRequest(ProtectedOperationContext ctx,
            int requestUniqueId,
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints);
        
        /// <summary>
        /// запрос - создать новый отложенный ордер
        /// сервер принимает заявку, пытается ее исполнить и отправляет ответ
        /// </summary>
        /// <param name="ctx">инф. об отправителе, проверяется</param>
        /// <param name="requestUniqueId">уникальный Id запроса (во избежание дублей)</param>
        /// <param name="order">параметры запрашиваемого ордера</param>
        /// <returns>заявка принята - либо ошибка</returns>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendNewPendingOrderRequest(ProtectedOperationContext ctx,
            int requestUniqueId, PendingOrder order);

        /// <summary>
        /// запрос - закрыть позицию
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendCloseRequest(ProtectedOperationContext ctx, int accountId, int orderId,
            PositionExitReason reason);

        /// <summary>
        /// закрыть все ордера по тикеру
        /// чтобы не было проблемы на манер: закрыть пару 
        /// SELL 100 USDCAD / BUY 100 USDCAD на границе маржинальных требований
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendCloseByTickerRequest(ProtectedOperationContext ctx, int accountId, string ticker,
            PositionExitReason reason);

        /// <summary>
        /// запрос - снять отложенный ордер
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendDeletePendingOrderRequest(ProtectedOperationContext ctx, PendingOrder order,
            PendingOrderStatus status, int? positionID, string closeReason);

        /// <summary>
        /// запрос - редактирование tp, sl, comment в открытой позиции
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SendEditMarketRequest(ProtectedOperationContext secCtx, MarketOrder ord);

        [OperationContract(IsOneWay = false)]
        RequestStatus SendEditPendingRequest(ProtectedOperationContext secCtx, PendingOrder ord);

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
        /// изменить пароль
        /// </summary>
        [OperationContract(IsOneWay = false)]
        AuthenticationResponse ChangePassword(ProtectedOperationContext ctx, string login, string newPassword);

        /// <summary>
        /// получить события, связанные со счетом (торговые сигналы, лог операций и др)
        /// </summary>
        /// <param name="ctx">контекст операции</param>
        /// <param name="userLogin">номер счета</param>
        /// <param name="deleteReceivedEvents">удалить все события на стороне сервера</param>
        /// <returns>список событий по счету</returns>
        [OperationContract(IsOneWay = false)]
        List<UserEvent> GetUserEvents(ProtectedOperationContext ctx, string userLogin,
            bool deleteReceivedEvents);

        /// <summary>
        /// отправить получателям торгового сигнала событие - текстовое сообщение
        /// </summary>        
        [OperationContract(IsOneWay = true)]
        void SendTradeSignalEvent(ProtectedOperationContext ctx, int accountId, int tradeSignalCategory,
            UserEvent acEvent);

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

        /// <summary>
        /// подписаться на платную услугу / отписаться от услуги
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool SubscribeOnService(ProtectedOperationContext secCtx,
            string login, int serviceId, bool renewAuto, bool unsubscribe,
            AutoTradeSettings tradeSets,
            out WalletError error);

        /// <summary>
        /// подписать на портфель
        /// отключить остальные подписки
        /// если портфель пользовательский - сохранить его
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus SubscribeOnPortfolio(ProtectedOperationContext secCtx, string subscriberLogin,
            TopPortfolio portfolio, decimal? maxFee,
            AutoTradeSettings tradeSets);

        /// <summary>
        /// изменить настройки автоторговли для портфеля, на который подписан пользователь
        /// </summary>
        /// <param name="secCtx"></param>
        /// <param name="login"></param>
        /// <param name="sets"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        RequestStatus ApplyPortfolioTradeSettings(ProtectedOperationContext secCtx, string login, AutoTradeSettings sets);

        /// <summary>
        /// отписаться от портфеля
        /// опционально - отписаться от всех подписок в составе портфеля
        /// </summary>
        [OperationContract(IsOneWay = false)]
        RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx,
            string subscriberLogin, bool deletePortfolio, bool deleteSubscriptions);
    }

    public enum AuthenticationResponse
    {
        OK = 0,
        InvalidAccount,
        WrongPassword,
        AccountInactive,
        ServerError,
        NotAuthorized
    }
}