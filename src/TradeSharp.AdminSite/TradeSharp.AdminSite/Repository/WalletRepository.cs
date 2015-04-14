using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    /// <summary>
    /// Класс доступа к данным кошелька
    /// </summary>
    public class WalletRepository : IWalletRepository
    {
        /// <summary>
        /// Редактирует балланс указанного кошелька
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="transferVolume"></param>
        /// <param name="deposit">если true, то происходит зачисление. Иначе - списание</param>
        public bool UpdateBalance(int walletId, decimal transferVolume, bool deposit)
        {
            try
            {
                WalletError error;
                var res = TradeSharpWalletManager.Instance.proxy.UpdateBalance(walletId, transferVolume, deposit, out error);
                return (error == WalletError.OK && res != null);
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateBalance()", ex);
                return false;
            }
        }

        /// <summary>
        /// Перевод средств на счёт / со счёта
        /// </summary>
        /// <returns></returns>
        public bool WalletTransfer(int walletId, decimal transferVolue, 
            string userLogin, int accountId, bool deposit)
        {
            try
            {
                Wallet res;
                WalletError error;
                if (deposit)
                {
                    res = TradeSharpWalletManager.Instance.proxy.TransferToTradingAccount(
                        ProtectedOperationContext.MakeServerSideContext(), userLogin,
                        accountId, transferVolue, out error);
                }
                else
                {
                    res = TradeSharpWalletManager.Instance.proxy.TransferToWallet(
                        ProtectedOperationContext.MakeServerSideContext(), userLogin,
                        accountId, transferVolue, out error);
                }

                Logger.Error("WalletTransfer() " + error);
                 return (error == WalletError.OK && res != null);
            }
            catch (Exception ex)
            {
                Logger.Error("WalletTransfer()", ex);
            }

            return false;
        }

        /// <summary>
        /// Изменение валюты кошелька
        /// </summary>
        public bool ChangeCurrency(int walletId, string walletCurrency, bool recalculationBalance)
        {
            try
            {
                WalletError error;
                var res = TradeSharpWalletManager.Instance.proxy.ChangeCurrency(walletId, walletCurrency, recalculationBalance, out error);
                return (error == WalletError.OK && res != null);
            }
            catch (Exception ex)
            {
                Logger.Error("ChangeCurrency()", ex);
                return false;
            }
        }
    }
}