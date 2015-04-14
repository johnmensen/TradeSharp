using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.Server.Repository
{
    interface IWalletRepository
    {
        WalletError ChargeFeeOnSubscription(TradeSharpConnection ctx, int serviceId, int usr, bool renewSubscription);
        
        bool UnsubscribeSubscriber(TradeSharpConnection ctx, SUBSCRIPTION sub);

        Wallet GetUserWalletSubscriptionAndLastPaymentsInner(
            string userLogin, int maxPaymentsQuery,
            out int paymentsTotalCount,
            out List<Subscription> subscriptions,
            out List<Transfer> transfers, out WalletError error);

        bool SubscribeOnService(TradeSharpConnection ctx,
                                int userId, int serviceId, bool renewAuto, bool unsubscribe,
                                AutoTradeSettings tradeSets, out WalletError error);

        RequestStatus SubscribeUserOnPortfolio(
            TradeSharpConnection ctx,
            string subscriberLogin,
            TopPortfolio portfolio,
            decimal? maxFee,
            AutoTradeSettings tradeAutoSettings);

        RequestStatus UnsubscribeUserFromPortfolio(TradeSharpConnection ctx, string subscriberLogin,
                                                   bool deletePortfolio, bool deleteSubscriptions);
    }
}
