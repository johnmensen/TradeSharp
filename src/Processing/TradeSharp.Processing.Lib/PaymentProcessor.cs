using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Processing.Lib
{
    public abstract class PaymentProcessor : IPaymentProcessing
    {
        /// <summary>
        /// Перечисление для строгой типизации ключей словаря в параметре метода 'ParsRequisites'
        /// </summary>
        protected enum RequisitesDictionaryKey { SourcePaySysAccount, SourcePaySysPurse, Comment };

        /// <summary>
        /// Метод парсит строку реквизитов, передаваемую в метод оформления потерянного платежа 'ReportOnFailPayment'.
        /// для разных реализаций этог класса, формирование строки реквизитов разное (например, в webMoney это строка XML формата).
        /// Следовательно и метод, который парсит эту строку, должен быть реализован не тут, а в классе наследнике (но обязательно вызван в меотде 'ReportOnFailPayment').
        /// </summary>
        /// <param name="requisites">Строка реквизитов, сформированныя каким либо способом</param>
        /// <returns>Словать, со строго определённым набором элементов, который заполняется внутри 'ParsRequisites'</returns>
        protected abstract Dictionary<RequisitesDictionaryKey, string> ParsRequisites(string requisites);

       
        /// <summary>
        /// Уведомление о потереном платеже. 
        /// </summary>
        /// <param name="currency">Валюта кошелька в платёжной системе</param>
        /// <param name="amount"></param>
        /// <param name="data"></param>
        /// <param name="paymentSystem"></param>
        /// <param name="requisites"></param>
        protected void ReportOnFailPayment(string currency, decimal amount, DateTime data, PaymentSystem paymentSystem, string requisites)
        {
            #region messageFail
            var messageFail = string.Format(
                            "ReportOnFailPayment() - Не удалось внести в базу данных запись о потерянном платеже на кошелёк платёжной системы {0}. \r\n" +
                            "Админисратор базы данных должен обязательно внести эту информацию врчную. Реквизиты платежа: валюта (currency) - {1}, \r\n" +
                            "размер внесённых средств (amount) {2}, дата (data) {3}. Дополнительные сведения: {4}",
                            paymentSystem, currency, amount,
                            data, requisites);
            #endregion
            
            var dictionary = ParsRequisites(requisites);
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    ctx.PAYMENT_SYSTEM_TRANSFER.Add(new PAYMENT_SYSTEM_TRANSFER
                        {
                            Ammount = amount,
                            Currency = currency,
                            DateProcessed = DateTime.Now,
                            DateValue = data,
                            UserPaymentSys = (int)paymentSystem,
                            SourcePaySysAccount = dictionary.ContainsKey(RequisitesDictionaryKey.SourcePaySysAccount)
                                                      ? dictionary[RequisitesDictionaryKey.SourcePaySysAccount]
                                                      : string.Empty,
                            SourcePaySysPurse = dictionary.ContainsKey(RequisitesDictionaryKey.SourcePaySysPurse)
                                                    ? dictionary[RequisitesDictionaryKey.SourcePaySysPurse]
                                                    : string.Empty,
                            Comment = dictionary.ContainsKey(RequisitesDictionaryKey.Comment)
                                          ? dictionary[RequisitesDictionaryKey.Comment]
                                          : string.Empty
                        });

                    try
                    {
                        ctx.SaveChanges();
                        Logger.Info("Платёж оформлен как неопознанный.");
                    }
                    catch (Exception ex)
                    {

                        Logger.Error(messageFail, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(messageFail, ex);
            }
        }

        #region IPaymentProcessing
        /// <summary>
        /// вывод средств в кошелек (скажем, ВебМани) пользователя
        /// </summary>        
        public abstract bool MakePayment(Wallet wallet, decimal amount, string targetPurse);
        #endregion

        /// <summary>
        /// Получение универсального идентификатора кошелька T# по реквизитам, полученным из платёжной системы
        /// </summary>
        protected int? GetTradeSharpeWalletIdByPaySysRequisite(PaymentSystem systemPayment, string rootId, string purseId)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var userPaySys =
                        ctx.USER_PAYMENT_SYSTEM.SingleOrDefault(
                            x => x.SystemPayment == (byte) systemPayment && x.RootId == rootId && x.PurseId == purseId);
                    if (userPaySys == null) return null;
                    return userPaySys.UserId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetTradeSharpeWalletIdByPaySysRequisite()", ex);
                return null;
            }
            
        }

        /// <summary>
        /// Получение универсального идентификатора кошелька T# по T# логину пользователя
        /// </summary>
        protected static int? GetTradeSharpeWalletIdByLogin(string loginTradeSharp)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var undecUser = ctx.PLATFORM_USER.SingleOrDefault(x => x.Login == loginTradeSharp);
                    if (undecUser == null) return null;
                    return undecUser.ID;
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(string.Format("В таблице PLATFORM_USER логин {0} встречается более одного раза", loginTradeSharp), ex);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("GetTradeSharpeWalletIdByLogin()", ex); 
                return null;
            }
        }

        /// <summary>
        /// Пересчёт зачисляемых средств из валюты перевода в валюту T# кошелька 
        /// </summary>
        public static decimal? ConvertPaySysCurrencyToWalletCurrency(string walletCurrency, string srcCurrency, double amount)
        {
            if (walletCurrency == srcCurrency)
                return (decimal) amount;

            bool inverse;
            var targetCurrency = DalSpot.Instance.FindSymbol(walletCurrency, srcCurrency, out inverse);
            if (string.IsNullOrEmpty(targetCurrency))
            {
                Logger.InfoFormat("Нет данных в словаре валют: {0} / {1}", walletCurrency, srcCurrency);
                return null;
            }
            // targetCurrency = "USDRUB", bool inverse = true
            var quote = QuoteStorage.Instance.ReceiveValue(targetCurrency);
            if (quote == null)
            {
                quote = YahooQuoteProvider.GetQuoteByKey(targetCurrency);
                if (quote == null)
                {
                    Logger.InfoFormat("Котировка {0} не найдена в Yahoo", targetCurrency);
                    return null;
                }
            }
            var price = inverse ? quote.bid : 1 / quote.ask;
            return (decimal) (amount * price);
        }
    }
}