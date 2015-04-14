using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class TradeSharpAccountProxy :  ITradeSharpAccount
    {
        private static ITradeSharpAccount fakeChannel;      
        private ChannelFactory<ITradeSharpAccount> factory;
        private ITradeSharpAccount channel;
        private readonly string endpointName;

        public TradeSharpAccountProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<ITradeSharpAccount>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpAccountProxy: невозможно создать прокси", ex);
                channel = null;
            }
        }
        
        private ITradeSharpAccount Channel
        {
            get
            {
                if (fakeChannel != null)
                    return fakeChannel;
                if (channel == null)
                    RenewFactory();
                return channel;
            }
        }

        #region ITradeSharpAccount
        public Dictionary<int, List<MarketOrder>> GetSignallersOpenTrades(int accountId)
        {
            try
            {
                return Channel.GetSignallersOpenTrades(accountId);
            }
            catch
            {
                try
                {
                    return Channel.GetSignallersOpenTrades(accountId);
                }
                catch (Exception ex)
                {
                    Logger.Error("error in GetSignallersOpenTrades", ex);
                    throw;
                }
            }
        }

        public RequestStatus GetAccountInfo(int accountId, bool needEquityInfo, out Account account)
        {
            return Channel.GetAccountInfo(accountId, needEquityInfo, out account);
        }

        public RequestStatus GetMarketOrders(int accountId, out List<MarketOrder> orders)
        {
            return Channel.GetMarketOrders(accountId, out orders);
        }

        public RequestStatus GetHistoryOrdersCompressed(int? accountId, DateTime? startDate, out byte[] buffer)
        {
            return Channel.GetHistoryOrdersCompressed(accountId, startDate, out buffer);
        }

        public RequestStatus GetHistoryOrders(int? accountId, DateTime? startDate, out List<MarketOrder> orders)
        {
            return Channel.GetHistoryOrders(accountId, startDate, out orders);
        }

        public RequestStatus GetHistoryOrdersByCloseDate(int? accountId, DateTime? startDate, out List<MarketOrder> orders)
        {
            return Channel.GetHistoryOrdersByCloseDate(accountId, startDate, out orders);
        }

        public RequestStatus GetPendingOrders(int accountId, out List<PendingOrder> orders)
        {
            return Channel.GetPendingOrders(accountId, out orders);
        }

        public List<int> GetAccountChannelIDs(int accountId)
        {
            return Channel.GetAccountChannelIDs(accountId);
        }

        public RequestStatus GetBalanceChanges(int accountId, DateTime? startTime, out List<BalanceChange> balanceChanges)
        {
            return Channel.GetBalanceChanges(accountId, startTime, out balanceChanges);
        }

        public RequestStatus ChangeBalance(int accountId, decimal summ, string comment)
        {
            return Channel.ChangeBalance(accountId, summ, comment);
        }

        public AccountRegistrationStatus RegisterAccount(PlatformUser user, string accountCurrency, int startBalance, decimal maxLeverage, string completedPassword, bool autoSignIn)
        {
            return Channel.RegisterAccount(user, accountCurrency, startBalance, maxLeverage, completedPassword,
                                                autoSignIn);
        }

        public bool RemindPassword(string email, string login)
        {
            return Channel.RemindPassword(email, login);
        }

        public List<TradeSignalCategory> GetTradeSignalsSubscribed(string userLogin)
        {
            return Channel.GetTradeSignalsSubscribed(userLogin);
        }

        public List<TradeSignalCategory> GetAllTradeSignals()
        {
            return Channel.GetAllTradeSignals();
        }
        #endregion        
    }
}
