using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Contract;
using TradeSharp.Hub.BL.Model;
using TradeSharp.Util;

namespace TradeSharp.Hub.BL.Repository
{
    public class CurrencyRepository : EntityRepository<Currency>, ICurrencyRepository
    {
        public PagedListResult<Currency> GetAllCurrencies(string sortBy, bool ascending, int skip, int take)
        {
            return GetItemsPaged(string.Empty, sortBy, ascending, skip, take);
        }

        /// <summary>
        /// Возвращает все валюты в виде списка
        /// </summary>
        /// <returns></returns>
        public List<Currency> GetAllCurrencies()
        {
            return context.Currency.ToList();
        }

        /// <summary>
        /// Возвращает валюту по коду
        /// </summary>
        /// <param name="code">код искомой валюты</param>
        public Currency GetCurrency(string code)
        {
            return context.Currency.FirstOrDefault(t => t.Code == code);
        }

        public bool DeleteCurrency(string code, out string errorString)
        {
            errorString = string.Empty;
            var currency = context.Currency.FirstOrDefault(t => t.Code == code);
            if (currency == null)
            {
                errorString = "\"" + code + "\" не найден";
                return false;
            }
            try
            {
                context.Currency.Remove(currency);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка в DeleteCurrency " + code, ex);
                errorString = ex.Message;
                return false;
            }           
        }

        public bool AddOrUpdateCurrency(Currency currency, out string errorString)
        {
            errorString = string.Empty;
            if (currency == null || string.IsNullOrEmpty(currency.Code)) throw new ArgumentException("");

            var existOne = context.Currency.FirstOrDefault(t => t.Code == currency.Code);

            try
            {
                if (existOne != null)
                {
                    existOne.Code = currency.Code;
                    existOne.CurrencyIndex = currency.CurrencyIndex;
                }
                else
                {
                    context.Currency.Add(new Currency
                    {
                         Code = currency.Code,
                         CurrencyIndex = currency.CurrencyIndex
                    });
                }
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка добавления / редактирования валюты " + currency.Code, ex);
                errorString = ex.Message;
                return false;
            }
        }
    }
}
