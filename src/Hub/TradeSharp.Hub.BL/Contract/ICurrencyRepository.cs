using System.Collections.Generic;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Model;

namespace TradeSharp.Hub.BL.Contract
{
    public interface ICurrencyRepository
    {
        PagedListResult<Currency> GetAllCurrencies(string sortBy, bool ascending, int skip, int take);
        List<Currency> GetAllCurrencies();

        Currency GetCurrency(string code);
        bool DeleteCurrency(string code, out string errorString);
        bool AddOrUpdateCurrency(Currency currency, out string errorString);
    }
}
