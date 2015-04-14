using TradeSharp.Hub.WebSite.Models;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public partial class CurrencyController
    {
        public GenericGridModel<BL.Model.Currency> GetModel(string sortBy = "Code", bool ascending = true, int page = 1, int pageSize = 4)
        {
            var model = new GenericGridModel<BL.Model.Currency>
            {
                PagerModel =
                {
                    SortBy = sortBy,
                    SortAscending = ascending,
                    CurrentPageIndex = page,
                    PageSize = pageSize
                }
            };

            var items = currencyRepository.GetAllCurrencies(model.PagerModel.SortBy, model.PagerModel.SortAscending,
                            (model.PagerModel.CurrentPageIndex - 1) * model.PagerModel.PageSize,
                            model.PagerModel.PageSize);

            model.PagerModel.TotalRecordCount = items.totalRecordsCount;
            model.Items = items.items;

            return model;
        }
    }
}