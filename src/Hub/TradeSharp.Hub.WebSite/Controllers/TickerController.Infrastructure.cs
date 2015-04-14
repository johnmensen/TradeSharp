using TradeSharp.Hub.BL.Model;
using TradeSharp.Hub.WebSite.Models;

namespace TradeSharp.Hub.WebSite.Controllers
{
    /// <summary>
    /// В этой части контроллера Ticker реализованы "вспомогательные" методы, которые неудобно помещать куда то в другое место, но и в 
    /// среди основных методов контроллера они не должны быть 
    /// </summary>
    public partial class TickerController
    {
        public GenericGridModel<Ticker> GetModel(string sortBy = "Name", bool ascending = true, int page = 1, int pageSize = 4)
        {
            var model = new GenericGridModel<Ticker>
            {
                PagerModel =
                {
                    SortBy = sortBy,
                    SortAscending = ascending,
                    CurrentPageIndex = page,
                    PageSize = pageSize
                }
            };

            var items = tickerRepository.GetAllTickers(model.PagerModel.SortBy, model.PagerModel.SortAscending,
                            (model.PagerModel.CurrentPageIndex - 1) * model.PagerModel.PageSize,
                            model.PagerModel.PageSize);

            model.PagerModel.TotalRecordCount = items.totalRecordsCount;
            model.Items = items.items;

            return model;
        }
    }
}