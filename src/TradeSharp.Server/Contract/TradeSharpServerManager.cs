using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Entity;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Processing.Lib;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TradeSharpServerManager : ITradeSharpServer
    {
        public TradeSharpServerManager()
        {
        }

        #region ITradeSharpServer
        public List<PerformerStat> GetAllManagers(PaidServiceType? serviceTypeFilter)
        {
            var performers = new List<PerformerStat>();

            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var p in ctx.GetPerformers())
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        performers.Add(new PerformerStat
                        {
                            Account = p.AccountId.HasValue ? p.AccountId.Value : -1,
                            Login = p.UserNames,
                            Email = p.Email,
                            Group = p.AccountGroup,
                            SubscriberCount = p.SubscriberCount ?? 0,
                            DepoCurrency = p.Currency,
                            Service = p.ID,
                            ServiceType = p.ServiceType,
                            TradeSignalTitle = p.Comment,
                            UserId = p.User,
                            FeeUSD = p.FixedPrice,
                            IsRealAccount = p.IsReal,
                            FullName = p.FullName
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в ServerManagerAccount.GetAllManagers()", ex);
                }

                // пересчитать Fee в USD
                var quotes = QuoteStorage.Instance.ReceiveAllData();
                foreach (var performer in performers)
                {
                    if (performer.DepoCurrency == PerformerStat.FeeCurrency || performer.FeeUSD == 0)
                        continue;
                    // произвести пересчет
                    string errorString;
                    var fee = DalSpot.Instance.ConvertSourceCurrencyToTargetCurrency(PerformerStat.FeeCurrency, performer.DepoCurrency,
                                                                                     (double) performer.FeeUSD, quotes,
                                                                                     out errorString);
                    if (!fee.HasValue)
                        Logger.Error("GetAllManagers() - cannot convert " + performer.DepoCurrency + " to " + PerformerStat.FeeCurrency);
                    performer.FeeUSD = fee ?? 0;
                }
                
            }
            return performers;
        }
        
        public List<PerformerStat> GetCompanyTopPortfolioManagedAccounts()
        {
            var performers = new List<PerformerStat>();

            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    var query = from usr in ctx.PLATFORM_USER
                                join pac in ctx.PLATFORM_USER_ACCOUNT on usr.ID equals pac.PlatformUser
                                join ac in ctx.ACCOUNT on pac.Account equals ac.ID
                                join pf in ctx.TOP_PORTFOLIO on ac.ID equals pf.ManagedAccount
                                where pf != null
                                select new
                                    {
                                        ac.AccountGroup,
                                        usr.ID,
                                        pac.Account,
                                        usr.Login,
                                        usr.Email,
                                        ac.Currency,
                                        AccountId = ac.ID,
                                        usr.Name
                                    };

                    foreach (var pf in query)
                        performers.Add(new PerformerStat
                            {
                                Account = pf.Account,
                                Login = string.IsNullOrEmpty(pf.Name) ? pf.Login : pf.Name,
                                DepoCurrency = pf.Currency,
                                UserId = pf.ID,
                                Group = pf.AccountGroup,
                                Email = pf.Email
                            });                                                                                                                    
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка в ServerManagerAccount.GetCompanyTopPortfolioManagedAccounts()", ex);
                }
            }

            return performers;
        }

        public bool RegistrationUndefinedTransfer(int paymentTransferId, int? paymentSysId)
        {
            try
            {
                return DbProcessing.RegistrationUndefinedTransfer(paymentTransferId, paymentSysId);
            }
            catch (Exception ex)
            {
                Logger.Error("RegistrationUndefinedTransfer() - ошибка при зачислении средств на кошелёк", ex);
                return false;
            }
        }

        /// <summary>
        ///  Зачисление средств в T# кошелёк пользователя
        /// </summary>
        /// <param name="walletId">всегда должен быть только один, иначе один платёж в платёжной системе увеличит средства нескольких пользователей</param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="data"></param>
        public bool DepositOnWallet(int walletId, string currency, decimal amount, DateTime data)
        {
            try
            {
                var userPaymentSysId = DbProcessing.GetUserPaymentSysId(walletId);
                if (!userPaymentSysId.HasValue) return false;

                var wallet = DbProcessing.GetWalletById(walletId);
                if (wallet == null) return false;

                var additionalAmount = amount;
                if (wallet.Currency != currency) // Нужно конвертировать средства в валюту T# кошелька
                {
                    var convertAmount = PaymentProcessor.ConvertPaySysCurrencyToWalletCurrency(wallet.Currency, currency, (double)amount);
                    if (!convertAmount.HasValue)
                    {
                        var message = string.Format(
                            "Не удалось произвести зачисление на TradeSharp кошелёк {0} средств в размере {1} {2}. \r\n" +
                            "Причина: не удалось сконвертировать валюту перевода {2} в валюту целевого TradeSharp кошелька {3}. \r\n" +
                            "Будет произведена попытка провести платёж как 'Неопознанный'. Администратор базы данных должен вручную зачислить \r\n" +
                            "средства на этот кошелёк, внеся изменения в таблицы WALLET и PAYMENT_SYSTEM_TRANSFER.",
                            wallet.User, amount.ToStringUniformMoneyFormat(), currency, wallet.Currency);

                        Logger.Error(message);
                        return false;
                    }
                    additionalAmount = convertAmount.Value;
                }
                
                // Сохраняем всё в базу данных
                return DbProcessing.DepositOnWallet(wallet, additionalAmount, data, userPaymentSysId.Value);
            }
            catch (Exception ex)
            {
                Logger.Error("DepositOnWallet - ошибка при зачислении средств на кошелёк", ex);
                return false;
            }
        }
        #endregion
    }
}
