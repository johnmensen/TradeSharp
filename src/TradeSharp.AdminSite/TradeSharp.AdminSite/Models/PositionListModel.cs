using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Entity;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Helper;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Класс-модель
    /// </summary>
    public class PositionListModel
    {
        /// <summary>
        /// Список, из которго формируются строки таблици
        /// </summary>
        public List<PositionItem> Positions { get; set; }

        /// <summary>
        /// Общее количество элементов в выборке для таблици (вместе с неотображёнными в таблице)
        /// </summary>
        public int? TotalCountItems { get; set; }

        #region Фильтры
        public static int countItemShowdefault = 10;
        /// <summary>
        /// Количество отображаемых елементов в таблице
        /// </summary>
        [Range(1, 2500, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ErrorMessageRange1_2500")]
        public int CountItemShow
        {
            get { return сountItemShow; }
            set { сountItemShow = value; }
        }

        /// <summary>
        /// Является ли счёт реальным или виртуальным
        /// </summary>
        public int? IsRealAccount { get; set; }

        /// <summary>
        /// Номер счёта
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Статус сделки
        /// </summary>
        public PositionState? Status { get; set; }

        /// <summary>
        /// Тип сделки (покупка / продажа)
        /// </summary>
        public DealType? Side { get; set; }

        /// <summary>
        /// Начало интервала фильрации по дате открытия сделки
        /// </summary>
        public string TimeOpenFrom { get; set; }

        /// <summary>
        /// Конец интервала фильрации по дате открытия сделки
        /// </summary>
        public string TimeOpenTo { get; set; }

        /// <summary>
        /// Начало интервала фильрации по дате закрытия сделки
        /// </summary>
        public string TimeExitFrom { get; set; }

        /// <summary>
        /// Конец интервала фильрации по дате закрытия сделки
        /// </summary>
        public string TimeExitTo { get; set; }

        /// <summary>
        /// Т.к. свойства 'TimeOpenFrom', 'TimeOpenTo', 'TimeExitFrom' и 'TimeExitTo' передают дату в виде строк, то их нужно парсить
        /// свойство DateTimeFormat содержит форматы записи дат, разделитили и т.п. что бы парсить дату
        /// </summary>
        public DateTimeFormatInfo DateTimeFormat { get; set; }

        /// <summary>
        /// Тоговый инструмент
        /// </summary>
        public string Symbol { get; set; }
        #endregion

        #region Списки для фильтров
        /// <summary>
        /// количество отображаемых элементов
        /// </summary>
        private int сountItemShow = 10;

        /// <summary>
        /// Формирует список статусов для выпадающего списка фильтра
        /// </summary>
        public static List<SelectListItem> StatusList()
        {
            var result = Enum.GetValues(typeof(PositionState)).Cast<PositionState>().Select(x => new SelectListItem
                    {
                        Text = EnumFriendlyName<PositionState>.GetString(x),
                        Value = ((int)x).ToString(CultureInfo.InvariantCulture)
                    }).ToList();
            result.Insert(0, new SelectListItem { Text = Resource.TitleWithoutFilter, Value = "-1", Selected = true });
            return result;
        }

        /// <summary>
        /// Формирует список типов счетов для выпадающего списка фильтра
        /// </summary>
        public static List<SelectListItem> IsRealAccountList()
        {
            return new List<SelectListItem>
                {
                    new SelectListItem{ Text = Resource.TitleWithoutFilter, Value = "-1", Selected = true},
                    new SelectListItem{ Text = Resource.TitleReal, Value = "1", Selected = false},
                    new SelectListItem{ Text = Resource.TitleVirtual, Value = "0", Selected = false},
                };
        }

        /// <summary>
        /// Формирует список валютных пар для выпадающего списка фильтра
        /// </summary>
        public static List<SelectListItem> SymbolList()
        {
            var result = Utils.SelectAllValuesFromTable<SPOT>(x => new SelectListItem { Text = x.Title, Value = x.Title });
            result.Insert(0, new SelectListItem { Text = Resource.TitleWithoutFilter, Value = "null", Selected = true });
            return result;
        }

        /// <summary>
        /// Формирует список типов сделки (покупка / продажа) для выпадающего списка фильтра
        /// </summary>
        public static List<SelectListItem> DealTypeList()
        {
            var result = Enum.GetValues(typeof(DealType)).Cast<DealType>().Select(x => new SelectListItem
            {
                Text = EnumFriendlyName<DealType>.GetString(x),
                Value = ((int)x).ToString(CultureInfo.InvariantCulture)
            }).ToList();
            result.Insert(0, new SelectListItem { Text = Resource.TitleWithoutFilter, Value = null, Selected = true });
            return result;
        }

        #region Фильтры с асинхронным обращением к серверу
        /// <summary>
        /// Формирует список счетов для выпадающего списка фильтра в виде строготипизированного списка из элементов SelectListItem
        /// </summary>
        /// <param name="currentId">Этот параметр связан с тем, что после осуществления фильтрации, в выпадающий список
        ///  асинхронно подгружаются только номера первых счётов и настроеный фильтр сбивается. Этот параметр исправляет ситуацию</param>
        public static List<SelectListItem> AccountIdList(string currentId)
        {
            int countItemInResult;
            var ajaxDropDownList = new AjaxDropDownListHelper(Delegats.dropDownFilterAccountDelegate);

            var result = ajaxDropDownList.GetListItem(currentId, out countItemInResult);
            if (countItemInResult > ajaxDropDownList.CountShowItem)
                result.Add(new SelectListItem
                {
                    Value = "over",
                    Text = string.Format("+{0} " + Resource.TitleNotShownItem, countItemInResult - ajaxDropDownList.CountShowItem)
                });
            return result;
        }

        /// <summary>
        /// Формирует список счетов для выпадающего списка фильтра в виде Html разметки
        /// </summary>
        public static string AccountIdAsynchList(string searchText)
        {
            var ajaxDropDownListHelper = new AjaxDropDownListHelper(Delegats.dropDownFilterAccountDelegate);
            return ajaxDropDownListHelper.GetHtmlMarkup(searchText);
        }
        #endregion
        #endregion
    }
}