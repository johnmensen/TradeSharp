using System;
using System.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class TradeSignalRepository : ITradeSignalRepository
    {
        public string AddSignal(ServiceTradeSignalModel newTradeSignal)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var acc = ctx.ACCOUNT.Single(x => x.ID == newTradeSignal.AccountId);

                    ctx.SERVICE.Add(new SERVICE
                        {
                            User = newTradeSignal.User,
                            FixedPrice = newTradeSignal.FixedPrice,
                            Comment = newTradeSignal.Comment,
                            ServiceType = (short)newTradeSignal.ServiceType,
                            Currency = acc.Currency,
                            AccountId = acc.ID
                        });

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddSignal", ex);
                return Resource.ErrorMessageTradeSignalUnableAdd;
            }

            return string.Empty;
        }

        public string EditSignal(ServiceTradeSignalModel oldTradeSignal)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var acc = ctx.ACCOUNT.Single(x => x.ID == oldTradeSignal.AccountId);
                    var signal = ctx.SERVICE.Single(x => x.ID == oldTradeSignal.Id);

                    signal.User = oldTradeSignal.User;
                    signal.FixedPrice = oldTradeSignal.FixedPrice;
                    signal.Comment = oldTradeSignal.Comment;
                    signal.ServiceType = (short) oldTradeSignal.ServiceType;
                    signal.Currency = acc.Currency;
                    signal.AccountId = acc.ID;
                    
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddSignal", ex);
                return Resource.ErrorMessageTradeSignalUnableEdit;
            }

            return string.Empty;
        }

        public string DellSignal(int id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var signal = ctx.SERVICE.Single(x => x.ID == id);
                    ctx.SERVICE.Remove(signal);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddSignal", ex);
                return string.Format("{1} {0}", id, Resource.ErrorMessageTradeSignalUnableDell);
            }

            return string.Empty;
        }

        public string Subscribe()
        {
            return string.Empty;
        }

        /// <summary>
        /// Отписать пользователя от сигналов
        /// </summary>
        /// <param name="userLogin">Логин потзователя, который больше не хочет быть подписчиком</param>
        /// <param name="serviceId">Сигналы, которые пользователь больше не хочет получать</param>
        /// <returns></returns>
        public string UnSubscribe(string userLogin, int serviceId)
        {
            string errorMessage;
            try
            {
                var proxy = new TradeSharpServerTrade(EmptyTradeSharpCallbackReceiver.Instance).proxy;
                WalletError walletError;
                var status = proxy.SubscribeOnService(ProtectedOperationContext.MakeServerSideContext(),
                                                        userLogin, serviceId, false, true, null, out walletError);
                if (!status || walletError != WalletError.OK)
                {
                    errorMessage = Resource.ErrorMessageCampaignAccount;
                    Logger.Error(errorMessage);
                    return errorMessage;
                }
                return string.Empty;                
            }
            catch (Exception ex)
            {
                errorMessage = Resource.ErrorMessageUnableSignAccount + ": " + ex.Message;
                Logger.Error("AddTopPortfolio", ex);
            }
            return errorMessage;
        }
    }
}