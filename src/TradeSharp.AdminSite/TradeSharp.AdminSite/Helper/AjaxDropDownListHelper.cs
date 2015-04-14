using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Helper
{
    /// <summary>
    /// класс серверной логики для асинхронно заполняемого выпадающего списка
    /// </summary>
    public class AjaxDropDownListHelper
    {
        /// <summary>
        /// Количество отображаемых елементов в выпадающем списке
        /// </summary>
        public int CountShowItem { get; set; }

        /// <summary>
        /// Этот делегат содержит ссылку на метод, который выполняется в "GetListItem"
        /// </summary>
        private readonly Delegats.DropDownFilterFillDelegate fillLogic;

        public AjaxDropDownListHelper(Delegats.DropDownFilterFillDelegate fillLogic, int countShowItem = 9)
        {
            this.fillLogic = fillLogic;
            CountShowItem = countShowItem;
        }

        /// <summary>
        /// Получает с сервера список каких либо элементов для фильтра. Логика "получения" приходит "из вне" в делегате fillLogic (чаще всего из класса).
        /// Этот метод вызывается при первом формировании выпадающего списка (поэтому данный метод и возвращает список из SelectListItem)
        /// </summary>
        public List<SelectListItem> GetListItem(string filterParameter, out int countItemsInResult)
        {
            try
            {
                var result = fillLogic(filterParameter, out countItemsInResult);
                result.Insert(0, new SelectListItem { Text = Resource.TitleWithoutFilter, Value = "-1", Selected = true });
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("AjaxDropDownListHelper GetListItem() Не удалось заполнить AjaxDropDownList список. Параметр поиска {0}", filterParameter), ex);
            }
            countItemsInResult = 0;
            return new List<SelectListItem>();
        }

        /// <summary>
        /// Метод вызывается при асинхронном запросе к серверу, при изменении символов в окне фильтра.
        /// Этот метод генерирет уже не список из SelectListItem, а сразу HTML разметку выпадающего списка, которая вставляется в JavaScript-е в select
        /// </summary>
        public string GetHtmlMarkup(string searchText)
        {
            //Получаем с сервера список первых CountShowItem элементов и общее количество элементов в выборке
            int countItemsInResult;
            var idList = GetListItem(searchText, out countItemsInResult);

            var markup = new StringBuilder();
            idList.ForEach(i => markup.Append(string.Format("<option value='{0}' >{0}</option>", i.Value)));

            // Если количесво элементов в выборке больше, чем мы будем отображать в выпадающем списке, тогда формируем "подсказку" для пользователя, что отображены не все элементы 
            if (countItemsInResult - CountShowItem > 0)
                markup.Append(string.Format("<option value='over' disabled>+{0} {1}</option>", countItemsInResult - CountShowItem, Resource.TitleNotShownItem));

            return markup.ToString();
        }
    }
}