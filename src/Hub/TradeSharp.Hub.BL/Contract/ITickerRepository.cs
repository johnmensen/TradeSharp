using System.Collections.Generic;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Model;

namespace TradeSharp.Hub.BL.Contract
{
    public interface ITickerRepository
    {
        PagedListResult<Ticker> GetAllTickers(string sortBy, bool ascending, int skip, int take);
        Ticker GetTicker(string name);
        IEnumerable<string> GetTickerNames();
        bool DeleteTicker(string name, out string errorString);
        bool AddOrUpdateTicker(Ticker ticker, out string errorString);
        bool AddOrUpdateTickerAlias(TickerAlias newTickerAlias);
        void UpdateListTickerAlias(List<TickerAlias> newTickerAlias);
        TickerAlias GetTickerAlias(string server, string ticker);
        bool RemoveTickerAlias(string server, string ticker);
        IEnumerable<TickerAlias> GetAllTickerAlias();
    }
}
