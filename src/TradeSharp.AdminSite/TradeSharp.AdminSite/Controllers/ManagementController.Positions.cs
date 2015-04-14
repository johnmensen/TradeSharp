using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Linq;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Helper;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Controllers
{
    /// <summary>
    /// Обзор совершонных сделок
    /// </summary>
    public partial class ManagementController
    {
        /// <summary>
        /// формирует модель для представления PositionListModel
        /// </summary>
        /// <param name="message"></param>
        /// <param name="accountId"></param>
        [HttpGet]
        public ActionResult PositionList(string message, int accountId = -1)
        {
            FillFiltersToPositionList();
            var positionList = positionRepository.GetPositionList(accountId);

            if (!string.IsNullOrEmpty(message)) ResultMessage = message;

            return View(positionList);
        }

        /// <summary>
        /// Как правило вызывается при фильтрации таблици
        /// </summary>
        /// <param name="positionListModel">класс модели с заполнеными данными фильров</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PositionList(PositionListModel positionListModel)
        {
            var accId = positionListModel.AccountId.HasValue ? positionListModel.AccountId.Value.ToString(CultureInfo.InvariantCulture) : "";
            FillFiltersToPositionList(accId);
            if (!ModelState.IsValid) return View(positionListModel);

            //TODO Формат даты сейчас зашит харкодом!!
            positionListModel.DateTimeFormat = new DateTimeFormatInfo {DateSeparator = ".", ShortDatePattern = "dd.MM.yyyy"};

            var model = positionRepository.GetPositionList(positionListModel);

            if (Request.IsAjaxRequest())
                return PartialView("PositionListPartialTable", model);
            return View(model);
        }

        /// <summary>
        /// Вспомогательный метод заполнения выпадающих списками для фильтров представления PositionListModel
        /// </summary>
        private void FillFiltersToPositionList(string currentId = "")
        {
            ViewData["listStatus"] = PositionListModel.StatusList();
            ViewData["isRealAccountList"] = PositionListModel.IsRealAccountList();
            ViewData["listAccountId"] = PositionListModel.AccountIdList(currentId);
            ViewData["listSymbol"] = PositionListModel.SymbolList();
            ViewData["listDealType"] = PositionListModel.DealTypeList();
        }

        /// <summary>
        /// Асинхронно вызывающийся метод при добавлении нового символа в поле поиска фильтра Счетов в таблице сделок
        /// </summary>
        /// <param name="searchText">текст для фильтрации</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AccountListForPositionTableAjaxUpdate(string searchText)
        {
            return Json(new { AccountIdHTML = PositionListModel.AccountIdAsynchList(searchText) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Запрос представления подробностей о сделке
        /// </summary>
        /// <param name="positionId">Уникальный идентификатор сделки</param>
        [HttpGet]
        public ActionResult PositionDetails(int positionId)
        {
            var positionItem = positionRepository.GetPositionItemDetails(positionId);
            if (positionItem != null) return View(positionItem);
            return RedirectToAction("Http204", "Error", new {description = String.Format("{0} (#{1}).", Resource.ErrorMessageServer , positionId) });
        }

        /// <summary>
        /// формирует представление с формой для редактирования выбранных позиций (но сам процесс редактирования осуществляется в следующем метода)
        /// Тут используется  POST потому что "positionId" может быть очень длинным и запихивать его в URL при GET запросе не стоит
        /// </summary>
        /// <param name="positionId">содержит Id-шники редактируемых позиций в виде строки 1,6,9,7,5...</param>
        [HttpPost]
        public ActionResult GetPositionEditView(string positionId)
        {
            FillFiltersToPositionList();
            var model = GetPositionsEditModel(positionId);
            return View("SafePositionEdit", model);
        }

        /// <summary>
        /// Осуществляет редактирование позиций, которые "безопасны" для редактирования (те, что выделены синим цветом)
        /// </summary>
        [HttpPost]
        public ActionResult SafePositionEdit(FormCollection formCollection)
        {
            var openPositionChangedPropertyList = new List<SystemProperty>();
            var closePositionChangedPropertyList = new List<SystemProperty>();

            var strOpenId = String.Empty; // содержит, перечисленные через запятую, уникальные идентификаторы открытых позиций, которые нужно обновить
            var strCloseId = String.Empty; // -- закрытых позиций, которые нужно обновить

            //Список ошибок валидации. Формат: Поле - коментарий к ошибке 
            var validationErrorList = new List<Tuple<string, string>>();

            // Мапим коллекцию 'FormCollection' в списоки из элементов 'SystemProperty'
            // Элемент item имеет тут знаения типа "Open_Side", "Open_StopLoss" и т.п.
            foreach (var item in formCollection)
            {
                if (String.IsNullOrEmpty(formCollection[item.ToString()])) 
                    continue;  //Если строка оставлена пользователем пустой, тогда НЕ вносим это поле в список на редактирование

                string type; 
                string systemName;

                #region парсим тип свойства и его системное имя
                try
                {
                    type = ((string) item).Split('_')[0];
                    systemName = ((string)item).Split('_')[1];
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("SafePositionEdit() - не {0}", item), ex);
                    continue;
                }
                #endregion
                
                var prop = new SystemProperty
                    {
                        SystemName = systemName,
                        Value = formCollection[item.ToString()]
                    };

                
                if (!prop.Validation())
                {
                    validationErrorList.Add(new Tuple<string, string>(systemName, Resource.ErrorMessageInvalid));
                    continue;
                }

                // Поскольку в следующем switch нет случаев 'OpenDanger_', 'CloseDanger_' и 'OtherDanger_' то в выборку на 
                // редактирование попадут только "безопасные" поля сделок 
                #region в зависимости от состояния сделки (открыта / закрыта), запихиваем её свойство в соответствующий список из 'SystemProperty'
                switch (type)
                {
                    case "Open":
                        openPositionChangedPropertyList.Add(prop);
                        break;
                    case "Close":
                        closePositionChangedPropertyList.Add(prop);
                        break;
                    case "ItemsId":
                        if (systemName == "Open") strOpenId = formCollection[item.ToString()];
                        if (systemName == "Close") strCloseId = formCollection[item.ToString()];
                        break;
                }
                #endregion               
            }

            var mess = string.Empty;

            mess += positionRepository.UpdateSavePositionItem(strOpenId, openPositionChangedPropertyList, PositionState.Opened) ?
                 string.Format("{0}: {1}", Resource.MessageMarketOrderUpdate, strOpenId) : 
                 string.Format("{0}: {1} - {2}.",Resource.ErrorMessage, Resource.ErrorMessageUnableMarketOrderUpdate, strOpenId);
            mess += "   ";
            mess += positionRepository.UpdateSavePositionItem(strCloseId, closePositionChangedPropertyList, PositionState.Closed) ?
                string.Format("{0}: {1}", Resource.MessageMarketOrderUpdate, strCloseId) :
                string.Format("{0}: {1} - {2}.", Resource.ErrorMessage, Resource.ErrorMessageUnableMarketOrderUpdate, strCloseId);


            #region Если были найдены ошибки валидации возвращаем на ту же самую страницу редактирования, но уже со списком ошибок
            if (validationErrorList.Count > 0)
            {
                var idArray = String.Empty;
                if (!String.IsNullOrEmpty(strOpenId)) idArray += strOpenId;
                if (!String.IsNullOrEmpty(strCloseId)) idArray += "," + strCloseId;

                var positions = positionRepository.GetPositionsById(idArray.ToIntArrayUniform());
                var model = new PositionsEditModel(positions) { validationErrorList = validationErrorList };
                return View("SafePositionEdit", model);
            }
            #endregion

            return RedirectToAction("PositionList", new { message = mess, accountId = -1 });
        }

        /// <summary>
        /// Переоткнывает закрытые сделки
        /// </summary>
        /// <param name="id">уникальные идентификаторы тех позиций, которые нужно отредактировать, перечисленные через запятую</param>
        [HttpPost]
        public ActionResult ReOpenDeal(string id)
        {
            positionRepository.ReopenPositions(id);

            var strings = new[] { "success" };
            return Json(strings);
        }

        /// <summary>
        /// При закрытии сделок, этот метод заполнянт данными всплывающее окно с данными для закрытия позиций.
        /// Метод вызывается асинхронно из JQuerry
        /// </summary>
        /// <param name="strId">Уникальные идентификаторы </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FillCloseDealForm(string strId)
        {
            var strings = GetSidePriceListById(strId);
            return Json(strings.ToArray());
        }

        /// <summary>
        /// Закрыть позиции, значениями указанными пользователями
        /// </summary>
        /// <param name="formCollection">данные введённые пользователем, для закрытия позиции (дата закрытия и цены закрытия для каждой валютной пары)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CloseDeal(FormCollection formCollection)
        {
            var timeExit = DateTime.Now;
            var chbxTimeExitValue = false;
            var closeDealIdValue = String.Empty;
            var validationErrorList = new List<string>();
            var warningList = new List<string>();
            var ddlExitReasonValue = PositionExitReason.ClosedFromUI;


            #region Пытаемся получить Id-шники редактируемых сделок
            var closeDealId = formCollection.AllKeys.FirstOrDefault(x => x == "closeDealId");
            if (closeDealId == null)
            {
                Logger.Error(
                    "CloseDeal - попытка принудительно закрыть сделки алвинистратором. Ошибка: в перечислении FormCollection не найдет объект с именем 'closeDealId'");
                validationErrorList.Add("Не удалось найти элемент типа hidden с именем 'closeDealId'");
            }
            else closeDealIdValue = formCollection[closeDealId];
            #endregion

            #region Пытаемся получить причину выхода из сделки

            var ddlExitReason = formCollection.AllKeys.FirstOrDefault(x => x == "ddlExitReason");
            if (ddlExitReason == null)
            {
                Logger.Error(
                    "CloseOrCancelDeal - попытка принудительно закрыть сделки администратором. Ошибка: в перечислении FormCollection не найдет объект с именем 'ddlExitReason'");
                validationErrorList.Add("Не удалось найти элемент типа ddlExitReason с именем 'ddlExitReason'");
            }
            else
            {
// ReSharper disable RedundantAssignment
                int? intExitReasonValue = -1;
// ReSharper restore RedundantAssignment
                intExitReasonValue = formCollection[ddlExitReason].ToIntSafe();
                if (intExitReasonValue != null && intExitReasonValue != -1)
                    ddlExitReasonValue = (PositionExitReason) intExitReasonValue;
            }

            #endregion

            //Проверяем - выставил ли пользователь дату и котировки руками
            var chbxTimeExit = formCollection.AllKeys.FirstOrDefault(x => x == "chbxTimeExit");
            if (chbxTimeExit != null)
                chbxTimeExitValue = formCollection[chbxTimeExit] == "on";


            if (!chbxTimeExitValue)
            {
                return CloseDaelCurrentTimeValues(closeDealIdValue, warningList, validationErrorList, timeExit, ddlExitReasonValue);
            }

            // Этот код выполняется, если пользователь закрывает сделки задним чилом, выставляя дату и котировки руками
            #region Получаем дату
            var txtbxTimeExitId = formCollection.AllKeys.FirstOrDefault(x => x == "txtbxTimeExit");
            if (txtbxTimeExitId == null)
            {
                Logger.Error("CloseOrCancelDeal - попытка принудительно закрыть сделки администратором. Ошибка: в перечислении FormCollection не найдет объект с именем 'txtbxTimeExit'");
                validationErrorList.Add(Resource.ErrorMessageTxtbxTimeExitNotFound);
            }
            else
            {
                var tExit = formCollection[txtbxTimeExitId].ToDateTimeUniformSafe(null);
                if (tExit.HasValue)
                {
                    timeExit = tExit.Value;
                }
                else
                {
                    Logger.Error("CloseOrCancelDeal - попытка принудительно закрыть сделки администратором. Ошибка: не удалось распарсить дату из 'txtbxTimeExit'");
                    validationErrorList.Add(Resource.ErrorMessageUnableParseTimeExit);
                }
            }
            #endregion

            var lstPrice = (from string inp in formCollection 
                            let input = inp.Split('_') 
                            let val = formCollection[inp].Replace(',', '.').ToFloatUniformSafe() 
                            where input.Length == 2 && val.HasValue 
                            select new Tuple<string, int, float>(input[0], Utils.dealSide[input[1]], val.Value)).ToList();


            if (validationErrorList.Count > 0) // Если были обнаружены ошибки
            {
                ViewBag.ErrorList = validationErrorList;
                var model = GetPositionsEditModel(closeDealIdValue);
                return View("SafePositionEdit", model);
            }


            var successCloseDeals = positionRepository.ClosingPositions(closeDealIdValue, timeExit, ddlExitReasonValue, lstPrice);
            var msg = successCloseDeals.Count < 10 ? string.Format("{0}: {1}.", Resource.MessageMarketOrderClosed, string.Join(", ", successCloseDeals)) :
                string.Format("{0}: {1}.", Resource.MessageMarketOrderClosed, string.Join(", ", successCloseDeals.Take(10)));


            return RedirectToAction("PositionList", new { message = msg, accountId = -1 });
        }

        /// <summary>
        /// Вспомогательный метод - закрывает позицию текушими значниями времени и катеровок
        /// </summary>
        /// <param name="closeDealIdValue"></param>
        /// <param name="warningList"></param>
        /// <param name="validationErrorList"></param>
        /// <param name="timeExit"></param>
        /// <param name="ddlExitReasonValue"></param>
        /// <returns></returns>
        private ActionResult CloseDaelCurrentTimeValues(string closeDealIdValue, List<string> warningList, List<string> validationErrorList,
                                                        DateTime timeExit, PositionExitReason ddlExitReasonValue)
        {
            var strings = GetSidePriceListById(closeDealIdValue);
            var adoHalper = new DatabaseQuoteEnquirer();
            var tickers = strings.Select(x => x.Split('_')[0]).Distinct();

            var quotes = tickers.ToDictionary(x => x, x => adoHalper.GetQuoteStoredProc(x));


            var lstStoragePrice = new List<Tuple<string, int, float>>();
            foreach (var inp in strings)
            {
                var input = inp.Split('_');
                var side = Utils.dealSide[input[1]];
                var quote = quotes.Where(x => x.Key == input[0]).ToList();

 
                if (quote.Any())
                {
                    var price = quote.First().Value.bid;
                    if (side == -1) price = quote.First().Value.ask;
                    lstStoragePrice.Add(new Tuple<string, int, float>(input[0], side, price));
                }
                else
                {
                    warningList.Add(String.Format("{0} : {1} {2}. {3}.",
                        Resource.ErrorMessage, Resource.ErrorMessageQuoteNotFound, input[0], Resource.ErrorMessageTransactionsOnPairWillNotClosed));
                    Logger.Error(String.Format("CloseOrCancelDeal - попытка принудительно закрыть сделки администратором. " +
                                               "Ошибка: для валютной пары {0} не найдены катировка. Сделки по этой валютной паре не будут закрыты.",input[0]));
                }
            }

            if (validationErrorList.Count > 0)
            {
                ViewBag.ErrorList = validationErrorList;
                ViewBag.WarningList = warningList;
                var model = GetPositionsEditModel(closeDealIdValue);
                return View("SafePositionEdit", model);
            }


            var successCloseDeals = positionRepository.ClosingPositions(closeDealIdValue, timeExit, ddlExitReasonValue, lstStoragePrice);
            var msg = successCloseDeals.Count < 10 ? 
                string.Format("{0}: {1}. {2}",
                Resource.MessageMarketOrderClosed, string.Join(", ", successCloseDeals), Resource.ErrorMessageClosedNotAllTransactions) :
            string.Format("{0} {1} {2}. {3}",
            Resource.MessageMarketOrderClosed, string.Join(", ", successCloseDeals.Take(10)), Resource.TextEtc, Resource.ErrorMessageClosedNotAllTransactions);
            
            return RedirectToAction("PositionList", new { message = msg, positionId = -1 });
        }

        /// <summary>
        /// Полная отмена сделок
        /// </summary>
        /// <param name="strId">уникальные удентификаторы отменяемых сделок</param>
        /// <param name="dealType">Тип отменяемых сделок (от этого зависит алгоритм отмены)</param>
        [HttpGet]
        public ActionResult CancelDeal(string strId, string dealType)
        {
            var id = new List<int>();
            switch (dealType)
            {
                case "Open":
                    id = positionRepository.CancelingOpenPositions(strId);
                    break;
                case "Close":
                    id = positionRepository.CancelingClosedPositions(strId);
                    break;
            }

            var msg = String.Format("{0} : {1}. {2} - {3}. {4}",
                                                            Resource.MessageMarketOrderCancel, String.Join(", ", id),
                                                            Resource.TextYouPointed, strId, Resource.ErrorMessageCanceledNotAllTransactions);

            FillFiltersToPositionList();
            var positionList = positionRepository.GetPositionList(-1);
            ResultMessage = msg;
            return View("PositionList", positionList);
        }

        /// <summary>
        /// Редактирование 'опасных' полей в выбранных на редактирование сделок
        /// </summary>
        /// <param name="formCollection">коллекция данных формы</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditDangerDeal(FormCollection formCollection)
        {
            FillFiltersToPositionList();
            var validationErrorList = new List<string>();

            PositionState? state = null;
            var strId = String.Empty;
            var newValueTicker = String.Empty;
            int? newValueSide = null;
            int? newValueVolume = null;
            float? newValueEnterPrice = null;
            float? newValueExitPrice = null;

            #region
            if (formCollection.AllKeys.Contains("editDangerDealId")) strId = formCollection["editDangerDealId"].Trim();

            if (formCollection.AllKeys.Contains("editDangerDealType"))
            {
                switch (formCollection["editDangerDealType"])
                {
                    case "Open":
                        state = PositionState.Opened;
                        break;
                    case "Close":
                        state = PositionState.Closed;
                        break;
                }
                if (state == null)
                    validationErrorList.Add(Resource.MessageTypeEditTransactionNotRecognized);
            }

            if (formCollection.AllKeys.Contains("ddlChangeTicker")) newValueTicker = formCollection["ddlChangeTicker"].Trim();

            if (formCollection.AllKeys.Contains("txtbxNewSide") && !String.IsNullOrEmpty(formCollection["txtbxNewSide"].Trim()))
            {
                newValueSide = formCollection["txtbxNewSide"].ToIntSafe();
                if (newValueSide == null || (newValueSide != 1 && newValueSide != -1)) validationErrorList.Add("Side - " + Resource.ErrorMessageVolumeInvalid);
            }

            if (formCollection.AllKeys.Contains("txtbxNewVolume") && !String.IsNullOrEmpty(formCollection["txtbxNewVolume"].Trim()))
            {
                newValueVolume = formCollection["txtbxNewVolume"].ToIntSafe();
                if (newValueVolume == null) validationErrorList.Add("Volum - " + Resource.ErrorMessageVolumeInvalid);
            }

            if (formCollection.AllKeys.Contains("txtbxNewEnterPrice") && !String.IsNullOrEmpty(formCollection["txtbxNewEnterPrice"].Trim()))
            {
                newValueEnterPrice = formCollection["txtbxNewEnterPrice"].ToFloatUniformSafe();
                if (newValueEnterPrice == null) validationErrorList.Add("Enter price - " + Resource.ErrorMessageVolumeInvalid);
            }

            if (formCollection.AllKeys.Contains("txtbxNewExitPrice") && !String.IsNullOrEmpty(formCollection["txtbxNewExitPrice"].Trim()))
            {
                newValueExitPrice = formCollection["txtbxNewExitPrice"].ToFloatUniformSafe();
                if (newValueExitPrice == null) validationErrorList.Add("Exit price - " + Resource.ErrorMessageVolumeInvalid);
            }
            #endregion
            
            if (validationErrorList.Count > 0)
            {
                ViewBag.ErrorList = validationErrorList;
                var model = GetPositionsEditModel(strId);
                return View("SafePositionEdit", model);
            }

            var msg = string.Empty;
            if (state != null)
            {
                msg = positionRepository.EditDangerDeal(strId, state.Value, newValueTicker, newValueSide, newValueVolume, newValueEnterPrice, newValueExitPrice) ?
                    Resource.MessageEditingMade
                    : String.Format("{0}: {1}", Resource.ErrorMessage, Resource.ErrorMessageUnableUpdateRecordDB);
            }

            return RedirectToAction("PositionList", new { message = msg });
        }

        /// <summary>
        /// С представления 'Список сделок' переходим на предстваление 'Добавить сделки'
        /// </summary>
        [HttpGet]
        public ActionResult AddDeal()
        {
            FillFiltersToPositionList();
            var newPosition = new PositionItem
                {
                    State = PositionState.Opened
                };
            return View(newPosition);
        }

        /// <summary>
        /// Добавление новой сделки
        /// </summary>
        /// <param name="positionItem">Ссылка на модель новой сделки</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddDeal(PositionItem positionItem)
        {
            var dateTame = positionItem.StrTimeEnter.ToDateTimeUniformSafe(new DateTimeFormatInfo { DateSeparator = ".", ShortDatePattern = "dd.MM.yyyy" });
            FillFiltersToPositionList();
            if (ModelState.IsValid && dateTame.HasValue)
            {
                string msg;
                positionItem.TimeEnter = dateTame.Value;
                if (positionRepository.NewDeal(positionItem))
                {
                    msg = String.Format("{0}. {1} - {2}", Resource.MessageMarketOrderOpen, Resource.TitleStatus, positionItem.State);
                }
                else
                {
                    msg = String.Format("{0}: {1}", Resource.ErrorMessage, Resource.MessageMarketOrderOpen);
                }

                return RedirectToAction("PositionList", new { message = msg });
            } 
            return View(positionItem);
        }     

        /// <summary>
        /// Вспомогательный метод формирующий список с описание сделки. Элемент массива типа "USDRUR_Sell"
        /// </summary>
        /// <param name="strId">уникальные идентификаторы сделок, для которых нужно сформировать массив</param>
        private List<string> GetSidePriceListById(string strId)
        {
            var id = strId.ToIntArrayUniform();
            var pos = positionRepository.GetPositionsById(id);

            var strings = new List<string>();
            foreach (var positionItem in pos.GroupBy(x => x.Symbol))
            {
                var first = positionItem.FirstOrDefault();
                if (first == null) continue;

                var side = first.Side;
                strings.Add(positionItem.Key + "_" + (side == -1 ? "Sell" : "Buy"));
                if (positionItem.Count(x => x.ID != first.ID && x.Side != side) > 0)
                    strings.Add(positionItem.Key + "_" + (side == -1 ? "Buy" : "Sell"));
            }
            return strings;
        }

        /// <summary>
        /// Формирует модель для представления с формой редактирования выбранных сделок
        /// </summary>
        /// <param name="positionId">Уникальные идентификаторы сделок, перечисленные через запятую</param>
        private PositionsEditModel GetPositionsEditModel(string positionId)
        {
            var idArray = positionId.ToIntArrayUniform();
            var positions = positionRepository.GetPositionsById(idArray);
            var model = new PositionsEditModel(positions);
            ViewBag.ListOfExitReason = new SelectList(from PositionExitReason e in Enum.GetValues(typeof(PositionExitReason))
                                                      select new { Id = (int)e, Name = e.ToString() }, "Id", "Name");
            return model;
        }
    }
}