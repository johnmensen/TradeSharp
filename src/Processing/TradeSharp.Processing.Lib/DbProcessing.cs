using System;
using System.Linq;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Processing.Lib
{
    public static class DbProcessing 
    {
        /// <summary>
        /// Зачисляем на T# кошелёк пользователя определённую сумму.
        /// </summary>
        /// <param name="wallet">T# кошелёк</param>
        /// <param name="additionalAmount">валюта кошелёка T#</param>
        /// <param name="data"></param>
        /// <param name="userPaymentSysId"></param>
        /// <returns></returns>
        public static bool DepositOnWallet(WALLET wallet, decimal additionalAmount, DateTime data, int userPaymentSysId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var comment = string.Format("Зачисление на TradeSharp кошелёк {0} средств в размере {1} {2}.",
                                                wallet.User, additionalAmount.ToStringUniformMoneyFormat(), wallet.Currency);
                    var formatComment = comment.Length < 250 ? comment : comment.Substring(0, 249);
                    #region

                    var transfer = new TRANSFER
                        {
                            User = wallet.User,
                            Amount = additionalAmount,
                            ValueDate = DateTime.Now,
                            TargetAmount = additionalAmount,
                            Comment = formatComment
                        };

                    ctx.TRANSFER.Add(transfer);

                    ctx.PAYMENT_SYSTEM_TRANSFER.Add(new PAYMENT_SYSTEM_TRANSFER
                        {
                            UserPaymentSys = userPaymentSysId,
                            Ammount = additionalAmount,
                            Currency = wallet.Currency,
                            DateProcessed = DateTime.Now,
                            DateValue = data,
                            Transfer = transfer.ID,
                            Comment = formatComment
                        });
                    #endregion
                    
                    try
                    {
                        var wal = ctx.WALLET.SingleOrDefault(x => x.User == wallet.User);
                        if (wal == null)
                        {
                            Logger.ErrorFormat("DepositOnWallet() - WALLET с id {0} не найден в БД", wallet.User);
                            return false;
                        }
                        wal.Balance += additionalAmount;

                        ctx.SaveChanges();
                        Logger.InfoFormat("Произведено зачисление средств на TradeSharp кошелёк {0} в размере {1} {2}.",
                                          wallet.User, additionalAmount, wallet.Currency);
                        return true;
                    }
                    #region catch
                    catch (Exception ex)
                    {
                        var message = string.Format(
                            "Не удалось сохранить в базу данных запись о зачислении на TradeSharp кошелька {0} средств в размере {1} {2}. " +
                            " Будет произведена попытка провести платёж как 'Неопознанный'. Администратор базы данных должен вручную зачислить " +
                            "средства на этот кошелёк, внеся изменения в таблицы WALLET и PAYMENT_SYSTEM_TRANSFER.",
                            wallet.User, additionalAmount.ToStringUniformMoneyFormat(), wallet.Currency);
                        Logger.Error(message, ex);
                        return false;
                    }
                    #endregion     
                }
            }
            #region catch
            catch (Exception ex)
            {
                Logger.Error("DepositOnWallet()", ex);
                return false;
            }
            #endregion          
        }

        /// <summary>
        /// зарегистриорвать 'Неопознанный' платёж. При этом в таблице PAYMENT_SYSTEM_TRANSFER редактируется поле Transfer.
        /// после этого сумма из поля Ammount зачисляется в кошелёк.
        /// </summary>
        /// <param name="paymentSystemTransferId">Уникальный идентификатор "неопознанного" платежа (запись в таблице PAYMENT_SYSTEM_TRANSFER, которую сейчас будем редактировать)</param>
        /// <param name="userPaymentSysId">Уникальный идентификатор записи о платёжной системе (таблица USER_PAYMENT_SYSTEM), из которой беруться данные о кошельке</param>
        public static bool RegistrationUndefinedTransfer(int paymentSystemTransferId, int? userPaymentSysId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var paymentSystemTransfer = ctx.PAYMENT_SYSTEM_TRANSFER.Single(x => x.Id == paymentSystemTransferId);
                    var userWalletCurrency = paymentSystemTransfer.Currency;
                    var additionalAmount = paymentSystemTransfer.Ammount;

                    var userId = paymentSystemTransfer.USER_PAYMENT_SYSTEM != null ? paymentSystemTransfer.USER_PAYMENT_SYSTEM.UserId :
                        ctx.USER_PAYMENT_SYSTEM.Single(w => w.Id == userPaymentSysId).UserId;
                   
                    var comment = string.Format("Восстановление 'неопознанного' платежа {3}. Зачисление на TradeSharp кошелёк {0} средств в размере {1} {2}.",
                                                userId, additionalAmount.ToStringUniformMoneyFormat(), userWalletCurrency, paymentSystemTransferId);
                    
                    var transfer = new TRANSFER
                    {
                        User = userId,
                        Amount = additionalAmount,
                        ValueDate = DateTime.Now,
                        TargetAmount = additionalAmount,
                        Comment = comment.Length < 250 ? comment : comment.Substring(0, 249)
                    };
                    ctx.TRANSFER.Add(transfer);

                    if (paymentSystemTransfer.USER_PAYMENT_SYSTEM == null) paymentSystemTransfer.UserPaymentSys = userPaymentSysId;
                    paymentSystemTransfer.Transfer = transfer.ID;

                    try
                    {
                        var wal = ctx.WALLET.Single(x => x.User == userId); 
                        wal.Balance += additionalAmount;

                        ctx.SaveChanges();
                        Logger.InfoFormat("RegistrationUndefinedTransfer() - Произведено восстановление 'неопознанного' платежа и зачисление средств на TradeSharp кошелёк {0} в размере {1} {2}.",
                                          userId, additionalAmount, userWalletCurrency);
                        return true;
                    }
                    #region catch
                    catch (Exception ex)
                    {
                        var message = string.Format(
                            "При попытке зарегистриорвать 'Неопознанный' платёж, не удалось сохранить в базу данных запись о зачислении на" +
                            " TradeSharp кошелёк {0} средств в размере {1} {2}. " +
                            " Администратор базы данных должен вручную зачислить " +
                            "средства на этот кошелёк, внеся изменения в таблицы WALLET и PAYMENT_SYSTEM_TRANSFER.",
                            userId, additionalAmount.ToStringUniformMoneyFormat(), userWalletCurrency);
                        Logger.Error(message, ex);
                        return false;
                    }
                    #endregion
                }
            }
            #region catch
            catch (Exception ex)
            {
                Logger.Error("RegistrationUndefinedTransfer()", ex);
                return false;
            }
            #endregion
        }


        /// <summary>
        /// Найти в БД T# кошелёк по его Id (он равен Id T# пользователя - владельца этого кошелька)
        /// </summary>
        /// <param name="id">Id искомого T# кошелёка и T# пользователя - владельца этого кошелька</param>
        public static WALLET GetWalletById(int id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var wallet = ctx.WALLET.SingleOrDefault(x => x.User == id);
                    if (wallet == null)
                    {
                        Logger.ErrorFormat("GetWalletById() - WALLET с id {0} не найден в БД", id);
                        return null;
                    }
                    return wallet;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetWalletById()", ex);
                return null;
            }
        }

        /// <summary>
        /// Предварительная проверка, есть ли для указанного walletId соответствующая запись в USER_PAYMENT_SYSTEM. Если её нет - значит уже что то не так!
        /// </summary>
        /// <param name="id">Id T# кошелёка и T# пользователя - владельца этого кошелька</param>
        public static int? GetUserPaymentSysId(int id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // ВАЖНО
                    // userPaymentSys всегда должен быть только один (SingleOrDefault) иначе один платёж в платёжной системе увеличит средства нескольких пользователей

                    var userPaymentSys = ctx.USER_PAYMENT_SYSTEM.SingleOrDefault(x => x.UserId == id);
                    if (userPaymentSys == null)
                    {
                        Logger.ErrorFormat("DepositOnWallet() - USER_PAYMENT_SYSTEM с UserId {0} не найден в БД", id);
                        return null;
                    }
                    return userPaymentSys.Id;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserPaymentSysId()", ex);
                return null;
            }
        }
    }
}