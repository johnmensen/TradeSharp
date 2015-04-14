using TradeSharp.Hub.BL.Model;
using TradeSharp.Hub.WebSite.Models;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public partial class CustomerController
    {
        public GenericGridModel<ServerInstance> GetModel(string sortBy = "Title", bool ascending = true, int page = 1, int pageSize = 15)
        {
            var model = new GenericGridModel<ServerInstance>
            {
                PagerModel =
                {
                    SortBy = sortBy,
                    SortAscending = ascending,
                    CurrentPageIndex = page,
                    PageSize = pageSize
                }
            };

            var items = serverInstanceRepository.GetAllServers(model.PagerModel.SortBy, model.PagerModel.SortAscending,
                                                               (model.PagerModel.CurrentPageIndex - 1) * model.PagerModel.PageSize,
                                                               model.PagerModel.PageSize);

            model.PagerModel.TotalRecordCount = items.totalRecordsCount;
            model.Items = items.items;

            return model;
        }
    }
}