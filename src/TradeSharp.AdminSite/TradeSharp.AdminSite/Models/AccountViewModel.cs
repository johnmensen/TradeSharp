using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MvcPaging;
using TradeSharp.Admin.Util;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models.CommonClass;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Модель для представления Accounts и AccountsPartialTable (оснавная таблица счётов)
    /// </summary>
	public class AccountViewModel
	{
        public AccountViewModel()
        {
            var listFilterGroups = Utils.SelectAllValuesFromTable<ACCOUNT_GROUP>(x => new SelectListItem { Text = x.Code, Value = x.Code });
            listFilterGroups.Insert(0, new SelectListItem { Text = "", Value = "" }); // для фильтра добавляем пустой элемент
            var listFilterCurrency = Utils.SelectAllValuesFromTable<COMMODITY>(x => new SelectListItem { Text = x.Title, Value = x.Title });
            listFilterCurrency.Insert(0, new SelectListItem { Text = "", Value = "" }); // для фильтра добавляем пустой элемент

            FilterGroups = listFilterGroups;
            FilterBalanceTickers = listFilterCurrency;
        }


        /// <summary>
        /// Номер текущей страници
        /// </summary>
        public int PageNomber { get; set; }

        /// <summary>
        /// Текущий размер страници
        /// </summary>
        public int CurrentPageSize { get; set; }

        #region фильтры
        /// <summary>
        /// Текущее значение для фильтра столбца "владелец счёта"
        /// </summary>
        public string FilterOwners { get; set; }

        /// <summary>
        /// Текущее значение для фильтра столбца "группа счетов"
        /// </summary>
        public string FilterGroup { get; set; }

        /// <summary>
        /// Текущее значение для фильтра столбца "боланс - верхняя граница"
        /// </summary>
        public int? FilterBalanceUpper { get; set; }

        /// <summary>
        /// Текущее значение для фильтра столбца "баланс - нижняя граница"
        /// </summary>
        public int? FilterBalanceLower { get; set; }

        /// <summary>
        /// Текущее значение для фильтра столбца "валюта"
        /// </summary>
        public string FilterBalanceTicker { get; set; }

        /// <summary>
        /// Текущее значение для фильтра столбца "номер счёта" [Filter("ID")]
        /// </summary>
        public int? FilterId { get; set; }
        #endregion
        

        /// <summary>
        /// Все выбранные столбци для сортировки
        /// </summary>
        public List<KeyValuePair<string, int>> SortColName { get; set; }

        /// <summary>
        /// Поле предназначено, для механизма сохранения информации о выбранных для сортировки солбцах между запросами.
        /// Через это поле передаются с клиента уже выбранные столбци для сортировки в формате "SortGroup:1;SortBalance:-1"
        /// 
        /// По большому счёту, это поле дублирует данные поля "SortColName", только в виде строки. Это сделано потому что нельзя в 
        /// '@Html.HiddenFor' положить Dictionary, что бы вернуть его с клиента на сервер
        /// </summary>
        public string StrSortColName { get; set; }
        
        /// <summary>
        /// возможные значения размеров страници
        /// </summary>
	    public int[] PageSizeItems
	    {
            get { return pageSizeItems; }
	    }

        /// <summary>
        /// Возможные значения фильтра для столбца "группа" (группа счетов). Получаются с сервера.
        /// </summary>
        public IEnumerable<SelectListItem> FilterGroups { get; set; }

        /// <summary>
        /// В фильтре столбца "Баланс" значения выпадающего списка "Валюта". Получаются с сервера.
        /// </summary>
        public IEnumerable<SelectListItem> FilterBalanceTickers { get; set; }

	    /// <summary>
	    /// Записи текущей страници таблици
	    /// </summary>
	    public PagedList<AccountTag> CurrentPageItems { get; set; }

        /// <summary>
        /// Словарь содержит делегаты для сортировки таблици по каждому из столбцов
        /// </summary>
        public static readonly Dictionary<string, Func<AccountTag, object>> sortFieldSelector = new Dictionary
            <string, Func<AccountTag, object>>
            {
                {"SortId", accountTag => accountTag.Id},
                {"SortBalance", accountTag => accountTag.account.Balance},
                {"SortOwners", accountTag => accountTag.OwnersName},
                {"SortGroup", accountTag => accountTag.Group}
            };

	    private readonly int[] pageSizeItems = new[]{10,15,25,50,100};
	}
}