using System;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    partial class ManagerTrade
    {
        /// <summary>
        /// подписать на портфель
        /// отключить остальные подписки
        /// если портфель пользовательский - сохранить его
        /// </summary>
        public RequestStatus SubscribeOnPortfolio(
            ProtectedOperationContext secCtx,
            string subscriberLogin,
            TopPortfolio portfolio,
            decimal? maxFee,
            AutoTradeSettings tradeAutoSettings)
        {
            //Logger.InfoFormat("SubscribeOnPortfolio({0})", subscriberLogin);
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.BindToSignal))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.BindToSignal), false))
                        return RequestStatus.Unauthorized;

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                    return walletRepository.SubscribeUserOnPortfolio(ctx, subscriberLogin, portfolio, maxFee, tradeAutoSettings);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в SubscribeOnPortfolio()", ex);
                return RequestStatus.ServerError;
            }
        }

        public RequestStatus ApplyPortfolioTradeSettings(ProtectedOperationContext secCtx, string login,
                                                         AutoTradeSettings sets)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.BindToSignal))
                if (!UserSessionStorage.Instance.PermitUserOperation(secCtx,
                    UserOperationRightsStorage.IsTradeOperation(UserOperation.BindToSignal), false))
                    return RequestStatus.Unauthorized;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user == null)
                        return RequestStatus.Unauthorized;
                    var subsription = ctx.USER_TOP_PORTFOLIO.FirstOrDefault(u => u.User == user.ID);
                    if (subsription == null)
                        return RequestStatus.CommonError;
                    subsription.AutoTrade = sets.TradeAuto;
                    subsription.MaxLeverage = sets.MaxLeverage;
                    subsription.PercentLeverage = sets.PercentLeverage;
                    subsription.HedgingOrdersEnabled = sets.HedgingOrdersEnabled;
                    subsription.FixedVolume = sets.FixedVolume;
                    subsription.MinVolume = sets.MinVolume;
                    subsription.MaxVolume = sets.MaxVolume;
                    subsription.VolumeRound = (int?) sets.VolumeRound;
                    subsription.StepVolume = sets.StepVolume;
                    subsription.TargetAccount = sets.TargetAccount;
                    ctx.SaveChanges();
                    return RequestStatus.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ApplyPortfolioTradeSettings()", ex);
                return RequestStatus.ServerError;
            }
        }

        public RequestStatus UnsubscribePortfolio(ProtectedOperationContext secCtx,
                                                  string subscriberLogin, bool deletePortfolio, bool deleteSubscriptions)
        {
            if (UserOperationRightsStorage.IsProtectedOperation(UserOperation.GetAccountDetail))
                if (
                    !UserSessionStorage.Instance.PermitUserOperation(secCtx,
                                                                     UserOperationRightsStorage.IsTradeOperation(
                                                                         UserOperation.BindToSignal), false))
                    return RequestStatus.Unauthorized;

            using (var ctx = DatabaseContext.Instance.Make())
                return walletRepository.UnsubscribeUserFromPortfolio(ctx, subscriberLogin, deletePortfolio, deleteSubscriptions);
        }
    }
}
