using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface ITopPortfolioRepository
    {
        List<TopPortfolio> GetAllTopPortfolio();
        TopPortfolio GetTopPortfolio(int id);
        bool SaveTopPortfolioChanges(TopPortfolioItem topPortfolioItem);
        bool UpdateCriteria(int id, string newCriteria, double? newMarginValue);
        string SubscribeOnPortfolioOnExistAccount(TopPortfolioItem newTopPortfolio);
        string SubscribeOnPortfolioOnNewAccount(TopPortfolioItem newTopPortfolio);
        bool DeletePortfolio(int id);
    }
}