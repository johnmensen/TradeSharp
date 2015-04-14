using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Web.Mvc;
using System.Linq;
using TradeSharp.Linq;

namespace TradeSharp.SiteAdmin.Helper
{
    /// <summary>
    /// Это вспомогательный класс. В нём храняться сигнатуры делегатов и их различные реализации (т.е. просто методы)
    /// </summary>
    public class Delegats
    {
        #region фильтры выпадающих списков
        /// <summary>
        /// Используется в фильтрах типа выпадающих списков. Эти фильтры осуществляют асинхронные запросы на сервер
        /// </summary>
        /// <param name="filterParameter"></param>
        /// <param name="countItemsInResult"></param>
        /// <returns></returns>
        public delegate List<SelectListItem> DropDownFilterFillDelegate(string filterParameter, out int countItemsInResult);



        public static DropDownFilterFillDelegate dropDownFilterAccountDelegate = delegate(string filterParameter, out int countItemsInResult)
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    countItemsInResult =
                        ctx.ACCOUNT.Count(
                            x => SqlFunctions.StringConvert((double) x.ID).Trim().Contains(filterParameter));
                                                                     var result = ctx.ACCOUNT.Where(x => SqlFunctions.StringConvert((double) x.ID).Trim().
                                                                     Contains(filterParameter)).OrderBy(x => x.ID).
                                                                     Take(9).
                                                                     Select(x => new SelectListItem
                                                                         {
                                                                             Text = SqlFunctions.StringConvert((double) x.ID).Trim(),
                                                                             Value = SqlFunctions.StringConvert((double) x.ID).Trim()
                                                                         }).
                                                                     ToList();
                    return result;
                }
            };        
        #endregion
    }
}