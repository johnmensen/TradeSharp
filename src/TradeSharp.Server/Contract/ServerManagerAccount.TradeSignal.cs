using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    public partial class ManagerTrade
    {
        /// <summary>
        /// "отправить", точнее - сохранить торговый сигнал
        /// </summary>
        public void SendTradeSignalEvent(ProtectedOperationContext secCtx, 
            int accountId, int serviceId, UserEvent acEvent)
        {
            try
            {
                if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.SendTradeSignal))
                    if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                        UserOperationRightsStorage.IsTradeOperation(UserOperation.SendTradeSignal), true))
                {
                    Logger.InfoFormat("SendTradeSignalEvent (ac={0}) - not permitted", accountId);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserEvents() - SendTradeSignalEvent error", ex);
                return;
            }

            acEvent.Code = AccountEventCode.TradeSignal;
            acEvent.Time = DateTime.Now;  

            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    // проверить авторские права счета accountId на кат. tradeSignalCategory
                    var acCat = ctx.SERVICE.FirstOrDefault(u => u.AccountId == accountId && u.ID == serviceId);
                    if (acCat == null)
                    {
                        Logger.InfoFormat("SendTradeSignalEvent (ac={0}) - no rights record", accountId);
                        return;
                    }
                    
                    // получить подписчиков
                    var subs = from subscriber in ctx.SUBSCRIPTION
                               where subscriber.Service == serviceId
                               select subscriber;
                    var newEvents = new List<UserEvent>();
                    foreach (var sub in subs)
                    {
                        var newEvt = acEvent.Copy();
                        newEvt.User = sub.User;
                        newEvents.Add(newEvt);
                    }
                    //Logger.InfoFormat("Сообщение {0} добавлено в очередь {1} подписчикам", acEvent, newEvents.Count);
                    
                    // создать ивент для самого отправителя
                    var ownerEvt = acEvent.Copy();
                    ownerEvt.User = acCat.User;
                    ownerEvt.Action = AccountEventAction.DoNothing;
                    ownerEvt.Code = AccountEventCode.TradeSignal;
                    ownerEvt.Title = "Торг. сигнал отправлен";
                    ownerEvt.Text = ownerEvt.Text.Length > 35 ? ownerEvt.Text.Substring(20) + "..." : ownerEvt.Text;
                    newEvents.Add(ownerEvt);
                    UserEventStorage.Instance.PushEvents(newEvents);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка в ServerManagerAccount.SendTradeSignalEvent(ac={0}): {1}",
                        accountId, ex);
                }
            }                                 
        }

        public bool SubscribeOnService(ProtectedOperationContext secCtx,
            string login, int serviceId, bool renewAuto, bool unsubscribe,
            AutoTradeSettings tradeSets,
            out WalletError error)
        {
            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, false))
            {
                error = WalletError.InsufficientRights;
                return false;
            }

            // создать или удалить подписку
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // пользователь
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user == null)
                    {
                        Logger.ErrorFormat("SubscribeOnService({0}, srv {1}) - пользователь не найден",
                            login, serviceId);
                        error = WalletError.InvalidData;
                        return false;
                    }

                    return walletRepository.SubscribeOnService(ctx, user.ID, serviceId, renewAuto, unsubscribe, tradeSets, out error);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в SubscribeOnService({0}): {1}", serviceId, ex);
                error = WalletError.ServerError;
                return false;
            }
        }
    }
}