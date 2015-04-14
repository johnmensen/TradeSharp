using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    partial class WalletManager
    {
        public RequestStatus InvestInPAMM(ProtectedOperationContext secCtx, string login, int accountId,
                                          decimal sumInWalletCurrency)
        {
            return new WalletManager4Pamm(secCtx, login, accountId).InvestOrWithdrawFromPamm(sumInWalletCurrency, false, false, null);
        }

        public RequestStatus WithdrawFromPAMM(ProtectedOperationContext secCtx, string login, int accountId,
                                          decimal sumInWalletCurrency, bool withdrawAll)
        {
            return new WalletManager4Pamm(secCtx, login, accountId).InvestOrWithdrawFromPamm(sumInWalletCurrency, true, withdrawAll, null);
        }

        public static decimal CalculateAccountEquityWithShares(TradeSharpConnection ctx,
            ACCOUNT account,
            int accountOwnerId,
            Dictionary<string, QuoteData> quotes,
            out List<AccountShare> accountShares,
            out bool noQuoteError)
        {
            accountShares = new List<AccountShare>();
            var orders = ctx.POSITION.Where(p => p.AccountID == account.ID &&
                p.State == (int)PositionState.Opened).ToList().Select(LinqToEntity.DecorateOrder).ToList();

            var curProfit = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(orders,
                quotes, account.Currency, out noQuoteError);
            if (noQuoteError)
                return account.Balance;

            var equity = (decimal)curProfit + account.Balance;

            // паи владельцев
            accountShares = ctx.ACCOUNT_SHARE.Where(s => s.Account == account.ID).ToList().Select(s =>
            {
                var sh = LinqToEntity.DecorateAccountShare(s);
                sh.ShareMoney = sh.SharePercent * equity / 100;
                return sh;
            }).ToList();

            if (accountShares.Count == 0)
            {
                // прописать долю владельца
                accountShares.Add(new AccountShare
                {
                    ShareMoney = equity,
                    SharePercent = 100,
                    UserId = accountOwnerId
                });
            }

            return equity;
        }
    }

    internal class WalletManager4Pamm
    {
        private const int MinInvestAmountOnPamm = 1;

        private ProtectedOperationContext secCtx;

        private string login;

        private int accountId;

        private decimal sumInWalletCurrency;

        private bool withdrawNotInvest;

        private bool withdrawAll;

        private TradeSharpConnection ctx;

        private PLATFORM_USER owner;

        private SERVICE service;

        public WalletManager4Pamm(ProtectedOperationContext secCtx,
                                  string login, int accountId)
        {
            this.secCtx = secCtx;
            this.login = login;
            this.accountId = accountId;
        }

        private RequestStatus CheckAmount()
        {
            if (sumInWalletCurrency < MinInvestAmountOnPamm && (!withdrawNotInvest || !withdrawAll))
            {
                Logger.ErrorFormat("InvestOrWithdrawFromPamm(" + login + ") - неверно указана сумма инвестирования: " +
                                   sumInWalletCurrency);
                return RequestStatus.IncorrectData;
            }

            if (!UserSessionStorage.Instance.PermitUserOperation(secCtx, false, true))
                return RequestStatus.Unauthorized;

            return RequestStatus.OK;
        }

        public RequestStatus InvestOrWithdrawFromPamm(decimal sumInWalletCurrency, bool withdrawNotInvest,
                                                      bool withdrawAll,
                                                      TradeSharpConnection databaseConnection)
        {
            this.sumInWalletCurrency = sumInWalletCurrency;
            this.withdrawNotInvest = withdrawNotInvest;
            this.withdrawAll = withdrawAll;

            var checkStatus = CheckAmount();
            if (checkStatus != RequestStatus.OK)
                return checkStatus;

            ctx = databaseConnection ?? DatabaseContext.Instance.Make();
            try
            {
                // найти подписанта
                var subscriber = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                if (subscriber == null)
                {
                    Logger.ErrorFormat("InvestOrWithdrawFromPamm(login={0}) - подписант не найден", login);
                    return RequestStatus.IncorrectData;
                }

                // владелец целевого ПАММ-счета
                var stat = FindPAMMServiceWithOwner();
                if (stat != RequestStatus.OK)
                    return stat;

                // посчитать средства по счету
                var account = ctx.ACCOUNT.First(a => a.ID == accountId);
                var quotes = QuoteStorage.Instance.ReceiveAllData();
                bool noQuoteError;

                List<AccountShare> shares;
                var equity = WalletManager.CalculateAccountEquityWithShares(ctx, account, owner.ID, quotes, out shares,
                                                                            out noQuoteError);
                if (noQuoteError)
                {
                    Logger.ErrorFormat(
                        "InvestOrWithdrawFromPamm(acc={0}) - невозможно произвести расчет текущей прибыли - нет котировок",
                        accountId);
                    return RequestStatus.ServerError;
                }
                decimal amountInAccountCurx;
                // пересчитать сумму в валюту ПАММа
                // проверить не выводит ли / не заводит ли слишком много?
                var status = CheckShareAndUpdateSubscriberWallet(shares, subscriber, account, quotes,
                                                                 out amountInAccountCurx);
                if (status != RequestStatus.OK)
                    return status;

                // добавить денег и пересчитать дольки
                var newEquity = equity + (withdrawNotInvest ? -1 : 1)*amountInAccountCurx;
                var sharePercent = amountInAccountCurx*100/newEquity;
                var sharesNew = shares.Select(s => new AccountShare
                    {
                        UserId = s.UserId,
                        ShareMoney = s.ShareMoney,
                        SharePercent = s.ShareMoney*100/newEquity,
                        HWM = s.HWM
                    }).ToList();

                // доля нового совладельца - пополнить или создать запись
                if (!withdrawNotInvest)
                {
                    var newOwnerShare = sharesNew.FirstOrDefault(s => s.UserId == subscriber.ID);
                    if (newOwnerShare != null)
                    {
                        newOwnerShare.SharePercent += sharePercent;
                        newOwnerShare.ShareMoney += amountInAccountCurx;
                    }
                    else
                    {
                        sharesNew.Add(new AccountShare
                            {
                                ShareMoney = amountInAccountCurx,
                                SharePercent = sharePercent,
                                UserId = subscriber.ID,
                                HWM = amountInAccountCurx
                            });
                    }
                }

                // найти существующую подписку и либо добавить денег,
                // либо создать новую подписку
                var subscriptExists = ctx.SUBSCRIPTION.Any(s => s.Service == service.ID && s.User == subscriber.ID);
                if (!subscriptExists && !withdrawNotInvest)
                {
                    // добавить подписку
                    ctx.SUBSCRIPTION.Add(new SUBSCRIPTION
                        {
                            User = subscriber.ID,
                            RenewAuto = false,
                            Service = service.ID,
                            TimeStarted = DateTime.Now,
                            TimeEnd = DateTime.Now
                        });
                }
                else if (subscriptExists && withdrawNotInvest && withdrawAll)
                {
                    // удалить подписку
                    ctx.SUBSCRIPTION.Remove(
                        ctx.SUBSCRIPTION.First(s => s.Service == service.ID && s.User == subscriber.ID));
                }

                // модифицировать записи ACCOUNT_SHARE
                UpdateShares(sharesNew, subscriber.ID);

                // пополнить баланс счета
                account.Balance += amountInAccountCurx;

                // сохранить изменения
                ctx.SaveChanges();
                return RequestStatus.OK;
            } // using ...
            catch (Exception ex)
            {
                Logger.Error("Ошибка в InvestOrWithdrawFromPamm", ex);
                return RequestStatus.ServerError;
            }
            finally
            {
                if (databaseConnection == null)
                    ctx.Dispose();
            }
        }

        private void UpdateShares(List<AccountShare> sharesNew, int subscriberId)
        {
            foreach (var share in sharesNew)
            {
                var shareOwner = share.UserId;
                var existShare =
                    ctx.ACCOUNT_SHARE.FirstOrDefault(s => s.ShareOwner == shareOwner && s.Account == accountId);

                if (existShare != null && existShare.ShareOwner == subscriberId && withdrawNotInvest && withdrawAll)
                {
                    ctx.ACCOUNT_SHARE.Remove(existShare);
                    continue;
                }

                if (existShare != null)
                    existShare.Share = share.SharePercent;
                else
                {
                    var newShare = new ACCOUNT_SHARE
                    {
                        Account = accountId,
                        Share = share.SharePercent,
                        ShareOwner = share.UserId,
                        HWM = share.HWM
                    };
                    ctx.ACCOUNT_SHARE.Add(newShare);
                }
            }
        }

        private RequestStatus FindPAMMServiceWithOwner()
        {
            owner = (from pa in ctx.PLATFORM_USER_ACCOUNT
                         join u in ctx.PLATFORM_USER on pa.PlatformUser equals u.ID
                         where pa.Account == accountId && pa.RightsMask == (int)AccountRights.Управление
                         select u).FirstOrDefault();
            if (owner == null)
            {
                Logger.ErrorFormat("InvestOrWithdrawFromPamm(acc={0}) - владелец счета не найден", accountId);
                return RequestStatus.IncorrectData;
            }

            // найти ПАММ-сервис для целевого акаунта
            service = (from srv in ctx.SERVICE
                           where srv.AccountId == accountId &&
                                 srv.User == owner.ID && srv.ServiceType == (int)PaidServiceType.PAMM
                           select srv).FirstOrDefault();
            if (service == null)
            {
                Logger.ErrorFormat("InvestOrWithdrawFromPamm(acc={0}) - счет не указан как ПАММ", accountId);
                return RequestStatus.BadRequest;
            }

            return RequestStatus.OK;
        }

        private RequestStatus CheckShareAndUpdateSubscriberWallet(List<AccountShare> shares, PLATFORM_USER subscriber,
            ACCOUNT account, Dictionary<string, QuoteData> quotes, out decimal amountInAccountCurx)
        {
            amountInAccountCurx = 0;
            var subscribersShare = shares.FirstOrDefault(s => s.UserId == subscriber.ID);
            if (subscribersShare == null && withdrawNotInvest)
            {
                Logger.ErrorFormat("CheckShareAndUpdateSubscriberWallet(acc={0}, user={1}) - у пользователя нет вклада",
                                   accountId, subscriber.Login);
                return RequestStatus.BadRequest;
            }

            // посчитать сумму вложения в валюте целевого счета
            var ownerWallet = ctx.WALLET.First(w => w.User == subscriber.ID);

            string errorStr;
            var calculatedAmountInAccountCurx = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(account.Currency,
                ownerWallet.Currency, (double) sumInWalletCurrency, quotes, out errorStr);
            if (!calculatedAmountInAccountCurx.HasValue)
            {
                Logger.Error("CheckShareAndUpdateSubscriberWallet - невозможно перевести " + account.Currency + " в " +
                             ownerWallet.Currency +
                             ": " + errorStr);
                return RequestStatus.ServerError;
            }
            amountInAccountCurx = calculatedAmountInAccountCurx.Value;

            // завести денег в ПАММ - списать с кошелька подписчика
            if (!withdrawNotInvest)
            {
                // списать деньги с кошелька подписчика
                if (ownerWallet.Balance < sumInWalletCurrency)
                {
                    Logger.ErrorFormat(
                        "CheckShareAndUpdateSubscriberWallet({0} на счет {1}) - сумма инвестирования {2} больше баланса кошелька ({3})",
                        login, accountId, sumInWalletCurrency, ownerWallet.Balance);
                    return RequestStatus.MarginOrLeverageExceeded;
                }
                ownerWallet.Balance -= sumInWalletCurrency;
            }
            else
            {
                // вывести все?
                if (withdrawAll)
                {
                    amountInAccountCurx = subscribersShare.ShareMoney;

                    string errorDepoAmountStr;
                    var calculatedAmountInWalletCurx = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(
                        ownerWallet.Currency, account.Currency, (double) amountInAccountCurx, quotes, out errorDepoAmountStr);
                    if (!calculatedAmountInWalletCurx.HasValue)
                    {
                        Logger.Error("CheckShareAndUpdateSubscriberWallet - (расчет суммы вывода) невозможно перевести " + account.Currency + " в " +
                                     ownerWallet.Currency + ": " + errorDepoAmountStr);
                        return RequestStatus.ServerError;
                    }
                    sumInWalletCurrency = calculatedAmountInWalletCurx.Value;
                }
                else
                {
                    // не превышает ли выводимая сумма долю вкладчика?
                    if (calculatedAmountInAccountCurx > subscribersShare.ShareMoney)
                    {
                        Logger.ErrorFormat("CheckShareAndUpdateSubscriberWallet({0} на счет {1}) - сумма вывода {2} больше доли пайщика ({3})",
                                           login, accountId, calculatedAmountInAccountCurx, subscribersShare.ShareMoney);
                        return RequestStatus.BadRequest;
                    }

                    subscribersShare.ShareMoney -= calculatedAmountInAccountCurx.Value;
                    // выводится, фактически, вся сумма?
                    if (subscribersShare.ShareMoney < MinInvestAmountOnPamm)
                    {
                        Logger.InfoFormat("CheckShareAndUpdateSubscriberWallet({0} на счет {1}) - сумма вывода {2} примерно равна доле пайщика ({3}), выводится все",
                            login, accountId, calculatedAmountInAccountCurx, subscribersShare.ShareMoney);
                        withdrawAll = true;
                        subscribersShare.ShareMoney = 0;
                    }
                }

                ownerWallet.Balance += sumInWalletCurrency;
            }
            return RequestStatus.OK;
        }
    }
}
