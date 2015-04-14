using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Contract.Proxy
{
    public class WalletManagerProxy : IWalletManager, IDisposable
    {
        private ChannelFactory<IWalletManager> factory;
        private readonly string endpointName;
        private IWalletManager channel;
        public static IWalletManager fakeChannel;
        private IWalletManager Channel
        {
            get
            {
                return fakeChannel ?? channel;
            }
        }

        public WalletManagerProxy(string endpointName)
        {
            this.endpointName = endpointName;
            RenewFactory();
        }

        private void RenewFactory()
        {
            if (fakeChannel != null) return;
            try
            {
                if (factory != null) factory.Abort();
                factory = new ChannelFactory<IWalletManager>(endpointName);
                channel = factory.CreateChannel();
            }
            catch (Exception)
            {
                Logger.Error("WalletManagerProxy: невозможно создать прокси");
                channel = null;
            }
        }

        public void Dispose()
        {
            if (fakeChannel != null) return;
            factory.Close();
        }

        #region IWalletManager
        public Wallet GetUserWallet(ProtectedOperationContext ctx, string userLogin)
        {
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.GetUserWallet(ctx, userLogin);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetUserWallet(ctx, userLogin);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetUserWallet() error: ", ex2);
                    return null;
                }
            }
        }

        public Wallet GetUserWalletSubscriptionAndLastPayments(ProtectedOperationContext ctx, string userLogin, int maxPaymentsQuery,
                                                               out List<Subscription> subscriptions, out List<Transfer> transfers, out WalletError error)
        {
            subscriptions = null;
            transfers = null;
            error = WalletError.OK;

            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.GetUserWalletSubscriptionAndLastPayments(ctx, userLogin, maxPaymentsQuery, out subscriptions, out transfers, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetUserWalletSubscriptionAndLastPayments(ctx, userLogin, maxPaymentsQuery, 
                        out subscriptions, out transfers, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetUserWalletSubscriptionAndLastPayments() error: ", ex2);
                    return null;
                }
            }
        }

        public List<Transfer> GetAllUserPayments(ProtectedOperationContext ctx, PlatformUser user, out WalletError error)
        {
            error = WalletError.OK;
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.GetAllUserPayments(ctx, user, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetAllUserPayments(ctx, user, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetAllUserPayments() error: ", ex2);
                    return null;
                }
            }
        }

        public bool RegisterOrUpdateService(ProtectedOperationContext ctx, PaidService service, out WalletError error)
        {
            error = WalletError.OK;
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.RegisterOrUpdateService(ctx, service, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.RegisterOrUpdateService(ctx, service, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("RegisterOrUpdateService() error: ", ex2);
                    return false;
                }
            }
        }

        public bool DisableService(ProtectedOperationContext ctx, int serviceId, out WalletError error)
        {
            error = WalletError.OK;
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.DisableService(ctx, serviceId, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.DisableService(ctx, serviceId, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("DisableService() error: ", ex2);
                    return false;
                }
            }
        }

        public bool SubscribeOnService(ProtectedOperationContext ctx,
            string login, int serviceId, bool renewAuto, bool unsubscribe, out WalletError error)
        {
            error = WalletError.OK;
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.SubscribeOnService(ctx, login, serviceId, renewAuto, unsubscribe, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.SubscribeOnService(ctx, login, serviceId, renewAuto, unsubscribe, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("SubscribeOnService() error: ", ex2);
                    return false;
                }
            }
        }

        public Wallet TransferToTradingAccount(ProtectedOperationContext ctx, string userLogin,
            int accountId, decimal amountInWalletCurrency, out WalletError error)
        {
            error = WalletError.OK;
            if (Channel == null)
                RenewFactory();
            try
            {
                return Channel.TransferToTradingAccount(ctx, userLogin, accountId, amountInWalletCurrency, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.TransferToTradingAccount(ctx, userLogin, accountId, amountInWalletCurrency, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("TransferToTradingAccount() error: ", ex2);
                    return null;
                }
            }
        }

        public Wallet TransferToWallet(ProtectedOperationContext ctx, string userLogin,
            int accountId, decimal amountInAccountCurrency, out WalletError error)
        {
            error = WalletError.OK;
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.TransferToWallet(ctx, userLogin, accountId, amountInAccountCurrency, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.TransferToWallet(ctx, userLogin, accountId, amountInAccountCurrency, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("TransferToWallet() error: ", ex2);
                    return null;
                }
            }
        }

        public PaidService GetPaidServiceDetail(int serviceId, out PlatformUser serviceOwner)
        {
            serviceOwner = null;
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.GetPaidServiceDetail(serviceId, out serviceOwner);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetPaidServiceDetail(serviceId, out serviceOwner);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetPaidServiceDetail() error: ", ex2);
                    return null;
                }
            }
        }

        public List<PaidService> GetUserOwnerPaidServices(int userId)
        {
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.GetUserOwnerPaidServices(userId);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetUserOwnerPaidServices(userId);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetPaidServiceDetail() error: ", ex2);
                    return null;
                }
            }
        }
        
        public List<UserPaymentSystem> GetUserRegistredPaymentSystemWallets(ProtectedOperationContext ctx,
                                                                            string userLogin,
                                                                            string walletPwrd,
                                                                            out WalletError error)
        {
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.GetUserRegistredPaymentSystemWallets(ctx, userLogin, walletPwrd, out error);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.GetUserRegistredPaymentSystemWallets(ctx, userLogin, walletPwrd, out error);
                }
                catch (Exception ex2)
                {
                    Logger.Error("GetPaidServiceDetail() error: ", ex2);
                    error = WalletError.CommonError;
                    return null;
                }
            }
        }

        public WalletError ChangePaymentSystemWallets(ProtectedOperationContext ctx, UserPaymentSystem paySys,
                                                      bool remove,
                                                      string userLogin, string walletPwrd)
        {
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.ChangePaymentSystemWallets(ctx, paySys, remove, userLogin, walletPwrd);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.ChangePaymentSystemWallets(ctx, paySys, remove, userLogin, walletPwrd);
                }
                catch (Exception ex2)
                {
                    Logger.Error("ChangePaymentSystemWallets() error: ", ex2);
                    return WalletError.CommonError;
                }
            }
        }

        public WalletError SetPaymentWalletsBySystem(ProtectedOperationContext secCtx,
                                                     PaymentSystem syst,
                                                     List<UserPaymentSystem> actualPaySys,
                                                     string userLogin, string walletPwrd)
        {
            if (Channel == null)
                throw new Exception("WalletManagerProxy: связь не установлена");
            try
            {
                return Channel.SetPaymentWalletsBySystem(secCtx, syst, actualPaySys, userLogin, walletPwrd);
            }
            catch
            {
                RenewFactory();
                try
                {
                    return Channel.SetPaymentWalletsBySystem(secCtx, syst, actualPaySys, userLogin, walletPwrd);
                }
                catch (Exception ex2)
                {
                    Logger.Error("SetPaymentWalletsBySystem() error: ", ex2);
                    return WalletError.CommonError;
                }
            }
        }
        #endregion
    }
}
