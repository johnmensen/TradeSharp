using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using TradeSharp.SiteAdmin.App_GlobalResources;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.BL;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Helper;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.CommonClass;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class PositionRepository : IPositionRepository
    {
        private readonly IAccountRepository accountRepository;

        public PositionRepository()
        {
            accountRepository = DependencyResolver.Current.GetService<IAccountRepository>();    
        }
        
        /// <summary>
        /// формируем модель для представления PositionListModel (выборка из БД хранимой процедурой и null-фильтрами)
        /// Метод обращается к хранимой процедуре "GetPositionList" на сервере. в основном это нужно для из за того, что происходит фильтрация выборки
        /// из двух таблиц (POSITION и POSITION_CLOSED) - лучше это производить на SQL сервере, а не LINQ2Object
        /// </summary>
        /// <param name="accountId">Уникальный идентификатор счёта для котороо выбираются позиции</param>
        /// <remarks>Пока не тестируетя</remarks>
        public PositionListModel GetPositionList(int accountId)
        {
            try
            {
                List<PositionItem> result;
                int totalCountPositeion;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var allPositions = ctx.GetPositionListWrapped(PositionListModel.countItemShowdefault, accountId == -1 ? null : (int?)accountId,
                        null, null, null, null, null, null, null, null, out totalCountPositeion);
                    result = Utils.DecoratPositionItems(allPositions.ToList());
                }
                return new PositionListModel { AccountId = accountId, Positions = result, TotalCountItems = totalCountPositeion };
            }
            catch (Exception ex)
            {
                Logger.Error("GetPositionList(int accountId)", ex);
                return new PositionListModel();
            }
        }

        /// <summary>
        /// формируем модель для представления PositionListModel (выборка с фильтрами из БД хранимой процедурой)
        /// </summary>
        /// <param name="positionListModel">Объект с новыми значениями фильтров</param>
        /// <remarks>Пока не тестируетя</remarks>
        public PositionListModel GetPositionList(PositionListModel positionListModel)
        {
            try
            {
                List<PositionItem> result;
                int totalCountPositeion;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var allPositions = ctx.GetPositionListWrapped(positionListModel.CountItemShow,
                                                           !positionListModel.AccountId.HasValue || positionListModel.AccountId == -1 ? null : positionListModel.AccountId,
                                                           !positionListModel.IsRealAccount.HasValue || positionListModel.IsRealAccount == -1 ? null : positionListModel.IsRealAccount,
                                                           positionListModel.Symbol == null || positionListModel.Symbol.ToLower() == "null" ? null : positionListModel.Symbol,
                                                           !((int?)positionListModel.Status).HasValue || (int?)positionListModel.Status == -1 ? null : (int?)positionListModel.Status,
                                                           positionListModel.Side.HasValue ? (int?)positionListModel.Side : null,
                                                           positionListModel.TimeOpenFrom.ToDateTimeUniformSafe(positionListModel.DateTimeFormat),
                                                           positionListModel.TimeOpenTo.ToDateTimeUniformSafe(positionListModel.DateTimeFormat),
                                                           positionListModel.TimeExitFrom.ToDateTimeUniformSafe(positionListModel.DateTimeFormat),
                                                           positionListModel.TimeExitTo.ToDateTimeUniformSafe(positionListModel.DateTimeFormat),
                                                           out totalCountPositeion);
                    result = Utils.DecoratPositionItems(allPositions.ToList());
                }
                return new PositionListModel
                {
                    CountItemShow = positionListModel.CountItemShow,
                    AccountId = positionListModel.AccountId,
                    Positions = result,
                    TotalCountItems = totalCountPositeion
                };
            }
            catch (Exception ex)
            {
                Logger.Error("GetPositionList(PositionItem decoratPositionItems)", ex);
                return new PositionListModel();
            }
        }

        /// <summary>
        /// формирует модель для представления PositionDetails
        /// </summary>
        /// <param name="positionId">Уникальным идентификатором является сделки</param>
        /// <remarks>Не тестируется</remarks>
        public MarketOrder GetPositionItemDetails(int positionId)
        {
            MarketOrder positionItem = null;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // Проверяем, есть ли указанная сделка среди открытых (потому что их меньше), если нет - ищём её среди закрытых
                    if (ctx.POSITION.Any(x => x.ID == positionId))
                    {
                        POSITION position;
                        try
                        {
                            position = ctx.POSITION.Single(x => x.ID == positionId);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(string.Format("GetPositionItemDetails() - При запросе подробностей о сделке c id {0} не удалось получить конкретный элемент из POSITION", positionId), ex);
                            return null;
                        }
                        positionItem = LinqToEntity.DecorateOrder(position);
                    }
                    else
                    {
                        POSITION_CLOSED position;
                        try
                        {
                            position = ctx.POSITION_CLOSED.Single(x => x.ID == positionId);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(string.Format("GetPositionItemDetails() - При запросе подробностей о сделке c id {0} не удалось получить конкретный элемент из POSITION_CLOSED", positionId), ex);
                            return null;
                        }
                        positionItem = LinqToEntity.DecorateOrder(position);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetPositionItemDetails Exception", ex);
            }
            return positionItem;
        }

        //TODO - Метод не оптимальный. Для каждой вытащенной записи из БД в цикле foreach производится мапинг в "PositionItem" (Для каждой по отдельности).
        //TODO - Метод не оптимальный. Происходи 2 обращения к БД - нуждо использовать join или unit
        /// <summary>
        /// Получить список сделок по их уникальным идентификаторам
        /// </summary>
        /// <param name="idList">Массив уникальных идентификаторов позиций, которые нужно получить</param>
        /// <remarks>Тестируется</remarks>
        public List<PositionItem> GetPositionsById(int[] idList)
        {
            var result = new List<PositionItem>();
            if (idList == null || idList.Length == 0) return result;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var source in ctx.POSITION.Where(x => idList.Contains(x.ID)))
                        // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        var marketOrder = LinqToEntity.DecorateOrder(source);
                        var positionItem = new PositionItem(marketOrder);
                        result.Add(positionItem);
                    }

                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var source in ctx.POSITION_CLOSED.Where(x => idList.Contains(x.ID)))
                        // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        var marketOrder = LinqToEntity.DecorateOrder(source);

                        //var positionItem = marketOrder as PositionItem;

                        var positionItem = new PositionItem(marketOrder);
                        result.Add(positionItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetPositionsById()", ex);
            }
            return result;
        }

        /// <summary>
        /// Обновляет в БД записи о сделках, внося изменения указанные пользователем (например, в представлении 'SafePositionEdit')
        /// Метод применяется для редактирования 'безопасных' свойств.
        /// В цикле перебираются все свойства "POSITION_CLOSED" или "POSITION" и имя каждого свойства в итерации сравнивается с именами в "propertyMetaData"
        /// </summary>
        /// <param name="strId">перечисленные через запятую, уникальные идентификаторы позиций, которые нужно обновить</param>
        /// <param name="propertyMetaData">Список свойств (главное - их имена) и их значений для обновления</param>
        /// <param name="state"></param>
        public bool UpdateSavePositionItem(string strId, List<SystemProperty> propertyMetaData, PositionState state)
        {
            var id = strId.ToIntArrayUniform();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    switch (state)
                    {
                        case PositionState.Closed:
                            var closePositonProps = typeof(POSITION_CLOSED).GetProperties();
                            foreach (var position in ctx.POSITION_CLOSED.Where(x => id.Contains(x.ID)).ToList())
                            {
                                foreach (var fieldMeta in propertyMetaData)
                                {
                                    var prop = closePositonProps.FirstOrDefault(p => p.Name.ToLower() == fieldMeta.SystemName.ToLower());
                                    if (prop != null)
                                    {
                                        var value = Converter.GetNullableObjectFromString(fieldMeta.Value, prop.PropertyType);
                                        prop.SetValue(position, value);
                                    }
                                }
                            }
                            break;
                        case PositionState.Opened:
                            var openPositionProps = typeof(POSITION).GetProperties();
                            foreach (var position in ctx.POSITION.Where(x => id.Contains(x.ID)).ToList())
                            {
                                foreach (var fieldMeta in propertyMetaData)
                                {
                                    var prop = openPositionProps.FirstOrDefault(p => p.Name.ToLower() == fieldMeta.SystemName.ToLower());
                                    if (prop != null)
                                    {
                                        var value = Converter.GetNullableObjectFromString(fieldMeta.Value, prop.PropertyType);
                                        prop.SetValue(position, value);
                                    }
                                }
                            }
                            break;
                    }
                    ctx.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateSavePositionItem()", ex);
                return false;
            }
        }

        /// <summary>
        /// Переоткрытие закрытых сделок
        /// </summary>
        /// <param name="strId">Уникальные идентификаторы сделокЮ которые нужно переоткрыть, перечисленные через запятую</param>
        /// <returns></returns>
        public bool ReopenPositions(string strId)
        {
            var id = strId.ToIntArrayUniform();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                //  Метод GetPositionsById тут использовать не надо т.к. в нём тот же код, но с лишним обращением к таблице POSITION, а так же
                //  тут не нужно лишнее преобразование MarketOrder к PositionItem

                var selOrders = new List<MarketOrder>();
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var order in ctx.POSITION_CLOSED.Where(x => id.Contains(x.ID))) selOrders.Add(LinqToEntity.DecorateOrder(order));
                // ReSharper restore LoopCanBeConvertedToQuery


                // Группируем все сделки по счётам
                var selOrderGroupByAccount = selOrders.GroupBy(x => x.AccountID);

                // Идем по группам сделок в выборке
                foreach (var selOrderGroup in selOrderGroupByAccount)
                {
                    var balanceDelta = -selOrderGroup.Sum(o => o.ResultDepo);
                    try
                    {
                        // "освежить" ордера и запомнить, какие транзакции нужно будет удалить
                        var balanceChangeDescript = new List<Cortege2<int, string>>();
                        foreach (var order in selOrderGroup)
                        {
                            balanceChangeDescript.Add(new Cortege2<int, string>(order.ResultDepo >= 0 ? (int)BalanceChangeType.Profit
                                : (int)BalanceChangeType.Loss,
                                string.Format("{1} #{0}", order.ID, Resource.TitleMarketOrderResult)));

                            order.ResultBase = 0;
                            order.ResultDepo = 0;
                            order.ResultPoints = 0;
                            order.State = PositionState.Opened;
                            order.PriceExit = null;
                            order.TimeExit = null;
                        }
                   
                        // удалить закрытые позиции
                        foreach (var oldOrder in selOrderGroup)
                        {
                            var ord = ctx.POSITION_CLOSED.FirstOrDefault(o => o.ID == oldOrder.ID);
                            if (ord != null) ctx.POSITION_CLOSED.Remove(ord);
                        }

                        // корректировать баланс
                        var account = ctx.ACCOUNT.FirstOrDefault(a => a.ID == selOrderGroup.Key);
                        if (account != null)
                        {
                            account.Balance += (decimal)balanceDelta;
                            //ctx.ACCOUNT.ApplyCurrentValues(account); //TODO убрал ApplyCurrentValues
                        }

                        // удалить трансферы
                        foreach (var transInfo in balanceChangeDescript)
                        {
                            var changeType = transInfo.a;
                            var transTitle = transInfo.b;
                            var trans = ctx.BALANCE_CHANGE.FirstOrDefault(t => t.AccountID == selOrderGroup.Key &&
                                                                               t.ChangeType == changeType &&
                                                                               t.Description == transTitle);
                            if (trans != null)
                                ctx.BALANCE_CHANGE.Remove(trans);
                        }

                        // добавить открытые позиции
                        var ordersDb = selOrderGroup.Select(LinqToEntity.UndecorateOpenedPosition).ToList();
                        foreach (var ord in ordersDb)
                        {
                            ctx.POSITION.Add(ord);
                        }

                        // завершить транзакцию
                        ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ReopenPositions(string strId)", ex);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Закрытие сделок
        /// </summary>
        /// <param name="strId">Уникальные идентификаторы закрываемых сделок, перечисленные через запятую</param>
        /// <param name="timeExit">Время выхода</param>
        /// <param name="exitReason">Symbol, Side (в виде Ask и Bid), Price</param>
        /// <param name="lstPrice">Причина закрытия сделки, указанная пользователем</param>
        public List<string> ClosingPositions(string strId, DateTime timeExit, PositionExitReason exitReason, List<Tuple<string, int, float>> lstPrice)
        {
            Logger.Info("Начинаем закрывать сделки " + strId);
            
            var result = new List<string>();
            var id = strId.ToIntArrayUniform();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // Вытаскиваем все открытые сделки, которые нужно закрыть
                    var selOrders = new List<MarketOrder>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var order in ctx.POSITION.Where(x => id.Contains(x.ID))) selOrders.Add(LinqToEntity.DecorateOrder(order));
                    // ReSharper restore LoopCanBeConvertedToQuery

                    if (selOrders.Count != id.Length)
                        Logger.Error("ClosingPositions() - в таблице 'POSITION' не найдены некоторые или все сделки с идентификаторами " + strId);

                    // Группируем все сделки по счётам
                    var selOrderGroupByAccount = selOrders.GroupBy(x => x.AccountID);

                    // Перебираем все счета
                    foreach (var orderGroup in selOrderGroupByAccount)
                    {
                        // Список удачно закрытых сделок в текущем счёте
                        var successClosedPositions = new List<string>();

                        var acc = accountRepository.GetAccount(orderGroup.Key);
                        if (acc == null)
                        {
                            Logger.Error("ClosingPositions() - не удалось получить счёт " + orderGroup.Key);
                            continue;
                        }

                        Logger.Info("Начинаем закрывать сделки в счёте " + orderGroup.Key);
                        // Перебираем все сделки в текущем счёте
                        foreach (var order in orderGroup)
                        {
                            #region
                            //Ищем цену выхода, указанную пользователем, для сделок с таким тикером и направлением
                            var priceExitTuple = lstPrice.FirstOrDefault(x => x.Item1 == order.Symbol && x.Item2 == order.Side);

                            if (priceExitTuple == null)
                            {
                                Logger.Error(string.Format("ClosingPositions() - не найдена цена выхода, указанная пользователем, для сделок счёта {0} с тикером {1} и направлением {2}", 
                                    order.ID, order.Symbol, order.Side)); continue;
                            }

                            var closedOrder = order.MakeCopy();
                            closedOrder.State = PositionState.Closed;
                            closedOrder.TimeExit = timeExit;
                            closedOrder.PriceExit = priceExitTuple.Item3;
                            closedOrder.ExitReason = exitReason;
                            
                            // посчитать прибыль
                            string errorStr;
                            if (!DealProfitCalculator.CalculateOrderProfit(closedOrder, acc.Currency, priceExitTuple.Item3, out errorStr))
                            {
                                if (!string.IsNullOrEmpty(errorStr))
                                    Logger.Error("Сделка " + closedOrder.ID +  " не будет закрыта - не удалось пересчитать прибыль : " + errorStr);
                                continue;
                            }

                            var balance = new BALANCE_CHANGE
                                {
                                    AccountID = order.AccountID,
                                    ChangeType = closedOrder.ResultBase > 0 ? (int)BalanceChangeType.Profit : (int)BalanceChangeType.Loss,
                                    Description = string.Format("результат сделки #{0}", order.ID),
                                    Amount = (decimal)Math.Abs(closedOrder.ResultDepo),
                                    ValueDate = closedOrder.TimeExit.Value
                                };

                            // убрать сделку из числа открытых, добавить закрытую и добавить проводку по счету
                            try
                            {
                                // убрать
                                var pos = ctx.POSITION.FirstOrDefault(p => p.ID == order.ID);
                                ctx.POSITION.Remove(pos);
                                Logger.Info("запись о сделке " + order.ID + " удалена из таблици POSITION");

                                ctx.POSITION_CLOSED.Add(LinqToEntity.UndecorateClosedPosition(closedOrder));
                                Logger.Info("запись о сделке " + order.ID + " добавленав таблицу POSITION_CLOSED");

                                // добавить проводку по счету
                                ctx.BALANCE_CHANGE.Add(balance);
                                var acBase = ctx.ACCOUNT.FirstOrDefault(ac => ac.ID == order.AccountID);
                                
                                if (acBase == null)
                                {
                                    Logger.Error("ClosingPositions() - не удалось найти счёт " + order.AccountID + " в таблице 'ACCOUNT', что бы добавить проводку");
                                    continue;
                                }
 
                                acBase.Balance += (decimal)closedOrder.ResultDepo;
                                Logger.Info("Баланс счёта " + order.AccountID + " изменён на величину " + (decimal)closedOrder.ResultDepo);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("ClosingPositions() - Ошибка при попытке убрать сделку из числа открытых, добавить закрытую и добавить проводку по счету", ex);
                                continue;
                            }
                            #endregion
                            successClosedPositions.Add(order.ID.ToString());
                            Logger.Error("Сделка " + order.ID + " отредактирована удачно");
                        }
                        ReCalculateAccountBalance(ctx, acc.ID);

                        Logger.Info("Начинаем сохранять в базу данных изменения по счёту " + orderGroup.Key);
                        ctx.SaveChanges();
                        result.AddRange(successClosedPositions);
                        
                    }

                    if (result.Count == 0)
                        Logger.Info("Не удалось закрыть ни одной из указанных сделок");
                    else
                        Logger.Info("Изменения сохранены. Из указанных сделок " + strId + " закрыты следующие: " + string.Join(", ", result));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ClosingPositions() - возникла ошибка при попытке сохранить изменения в базу данных. Не удалось закрыть сделки " + strId, ex);
            }
            return result;
        }

        /// <summary>
        /// Отмена открытых сделок, без возможности восстановления
        /// </summary>
        /// <param name="strId">Уникальные идентификаторы отменяемых открытых сделок, перечисленные через запятую</param>
        /// <returns>Уникальные идентификаторы успешно отменённых открытых сделок</returns>
        public List<int> CancelingOpenPositions(string strId)
        {
            Logger.Info("Начинаем отменять открытые сделки " + strId);

            var result = new List<int>();
            var id = strId.ToIntArrayUniform();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // Вытаскиваем все открытые сделки, которые нужно отменить
                    var selOrders = new List<MarketOrder>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var order in ctx.POSITION.Where(x => id.Contains(x.ID))) selOrders.Add(LinqToEntity.DecorateOrder(order));
                    // ReSharper restore LoopCanBeConvertedToQuery

                    if (selOrders.Count != id.Length)
                        Logger.Error("CancelingOpenPositions() - в таблице 'POSITION' не найдены некоторые или все сделки с идентификаторами " + strId);

                    // Группируем все сделки по счётам
                    var selOrderGroupByAccount = selOrders.GroupBy(x => x.AccountID);

                    // Перебираем все счета
                    foreach (var orderGroup in selOrderGroupByAccount)
                    {
                        // Список удачно отменённых открытых сделок текущего счёта
                        var successCanceledPositions = new List<int>();

                        var acc = accountRepository.GetAccount(orderGroup.Key);
                        if (acc == null)
                        {
                            Logger.Error("CancelingOpenPositions() - не удалось получить счёт " + orderGroup.Key);
                            continue;
                        }

                        Logger.Info("Начинаем отменять открытые сделки в счёте " + orderGroup.Key);
                        // Перебираем все сделки в текущем счёте
                        foreach (var order in orderGroup)
                        {
                            // убрать сделку из числа открытых
                            try
                            {
                                var pos = ctx.POSITION.FirstOrDefault(p => p.ID == order.ID);
                                if (pos == null)
                                {
                                    Logger.Error("CancelingOpenPositions() - запись о сделке " + order.ID + " не найдена в таблице POSITION. Сделка не может быть отменена");
                                    continue;
                                }
                                ctx.POSITION.Remove(pos);
                                Logger.Info("запись о сделке " + order.ID + " удалена из таблици POSITION");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("CancelingOpenPositions() - Ошибка при попытке убрать сделку из числа открытых", ex);
                                continue;
                            }
                            successCanceledPositions.Add(order.ID);
                        }
                        Logger.Info("Начинаем сохранять в базу данных изменения по счёту " + orderGroup.Key);
                        ctx.SaveChanges();
                        result.AddRange(successCanceledPositions);
                    }
                    Logger.Info("Изменения сохранены. Cделки " + string.Join(", ", result) + " отменены");
                    if (id.Length - result.Count != 0)
                        Logger.Error("CancelingOpenPositions() - по каким то причинам, не удалось отменить " + (id.Length - result.Count) + " сделок");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CancelingOpenPositions() - возникла ошибка при попытке сохранить изменения в базу данных. Не удалось отменить открытые сделки " + strId, ex);
            }
            Logger.Info("Сделки " + strId + " отменены");
            return result;
        }

        public List<int> CancelingClosedPositions(string strId)
        {
            Logger.Info("Начинаем отменяем закрытые сделки " + strId);

            var result = new List<int>();
            var id = strId.ToIntArrayUniform();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // Вытаскиваем все закрытые сделки, которые нужно отменить
                    var selOrders = new List<MarketOrder>();
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var order in ctx.POSITION_CLOSED.Where(x => id.Contains(x.ID))) selOrders.Add(LinqToEntity.DecorateOrder(order));
                    // ReSharper restore LoopCanBeConvertedToQuery

                    if (selOrders.Count != id.Length)
                        Logger.Error("CancelingClosedPositions() - в таблице 'POSITION_CLOSED' не найдены некоторые или все сделки с идентификаторами " + strId);

                    // Группируем все сделки по счетам
                    var selOrderGroupByAccount = selOrders.GroupBy(x => x.AccountID);

                    // Перебираем все счета
                    foreach (var orderGroup in selOrderGroupByAccount)
                    {
                        // Список удачно отменённых закрытых сделок текущего счёта
                        var successCanceledPositions = new List<int>();

                        var acc = accountRepository.GetAccount(orderGroup.Key);
                        if (acc == null)
                        {
                            Logger.Error("CancelingClosedPositions() - не удалось получить счёт " + orderGroup.Key);
                            continue;
                        }

                        Logger.Info("Начинаем отменять закрытые сделки в счёте " + orderGroup.Key);
                        Logger.Debug(string.Format("для счёта {0} количество BALANCE_CHANGE до отмены открытых сделок равно {1}", 
                            orderGroup.Key, ctx.BALANCE_CHANGE.Count(c => c.AccountID == orderGroup.Key)));

                        // Перебираем все сделки в текущем счёте
                        foreach (var order in orderGroup)
                        {
                            #region убрать сделку из числа закрытых
                            try
                            {
                                var pos = ctx.POSITION_CLOSED.FirstOrDefault(p => p.ID == order.ID);
                                if (pos == null)
                                {
                                    Logger.Error("CancelingClosedPositions() - запись о сделке " + order.ID + " не найдена в таблице POSITION_CLOSED. Сделка не может быть отменена");
                                    continue;
                                }
                                ctx.POSITION_CLOSED.Remove(pos);
                                Logger.Info("запись о сделке " + order.ID + " удалена из таблици POSITION_CLOSED");
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("CancelingClosedPositions() - Ошибка при попытке убрать сделку из числа закрытых", ex);
                                continue;
                            }
                            #endregion
                            

                            var dealDescr = string.Format("{1} #{0}", order.ID, Resource.TitleMarketOrderResult);
                            var trans = ctx.BALANCE_CHANGE.FirstOrDefault(c => c.AccountID == order.AccountID &&
                                                                               (c.ChangeType ==(int)BalanceChangeType.Profit ||
                                                                                c.ChangeType ==(int)BalanceChangeType.Loss) &&
                                                                                c.Description.Equals(dealDescr,StringComparison.OrdinalIgnoreCase));

                            if (trans == null)
                            {
                                Logger.Error("CancelingClosedPositions() - Ошибка при попытке убрать сделку из числа закрытых, не удалось найти транзакцию");
                                continue;
                            }
                            ctx.BALANCE_CHANGE.Remove(trans);
                            successCanceledPositions.Add(order.ID);
                        }
                        Logger.Info("Начинаем вспомогательное сохранение в базу данных");
                        ctx.SaveChanges();


                        Logger.Debug(string.Format("для счёта {0} количество BALANCE_CHANGE после отмены открытых сделок равно {1}", 
                            orderGroup.Key, ctx.BALANCE_CHANGE.Count(c => c.AccountID == orderGroup.Key)));

                        ReCalculateAccountBalance(ctx, acc.ID);


                        Logger.Info("Начинаем сохранять в базу данных изменения по счёту " + orderGroup.Key);
                        ctx.SaveChanges();
                        result.AddRange(successCanceledPositions);
                    }
                    Logger.Info("Изменения сохранены. Cделки " + string.Join(", ", result) + " отменены");
                    if (id.Length - result.Count != 0)
                        Logger.Error("CancelingClosedPositions() - по каким то причинам, не удалось отменить " + (id.Length - result.Count) + " сделок");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CancelingClosedPositions() - возникла ошибка при попытке сохранить изменения в базу данных. Не удалось отменить открытые сделки " + strId, ex);
            }
            Logger.Info("Сделки " + strId + " отменены");
            return result;
        }       

        /// <summary>
        /// Редактирование 'опасных' полей сделок
        /// </summary>
        /// <param name="strId">Уникальные идентификаторы редактируемых сделок</param>
        /// <param name="currentState">Статус редактируемых сделок</param>
        /// <param name="newTicker">Новое значение инструмента</param>
        /// <param name="newSide">Новое значение типа сделки</param>
        /// <param name="newVolume">Новое значение объёма сделки</param>
        /// <param name="newEnterPrice">Новое значение цены входа</param>
        /// <param name="newExitPrice">Новое значение цены выхода</param>
        public bool EditDangerDeal(string strId, PositionState currentState, string newTicker, int? newSide, int? newVolume, float? newEnterPrice, float? newExitPrice)
        {
            var id = strId.ToIntArrayUniform();
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var selOrders = new List<MarketOrder>();
                // ReSharper disable LoopCanBeConvertedToQuery
                if (currentState == PositionState.Closed)
                {
                    foreach (var order in ctx.POSITION_CLOSED.Where(x => id.Contains(x.ID))) selOrders.Add(LinqToEntity.DecorateOrder(order)); 
                } else
                foreach (var order in ctx.POSITION.Where(x => id.Contains(x.ID))) selOrders.Add(LinqToEntity.DecorateOrder(order));
                // ReSharper restore LoopCanBeConvertedToQuery

                // Группируем все сделки по счётам
                var selOrderGroupByAccount = selOrders.GroupBy(x => x.AccountID);
                // Идём по счетам в группе
                foreach (var selOrderGroup in selOrderGroupByAccount)
                {
                    var acc = accountRepository.GetAccount(selOrderGroup.Key);

                    try
                    {
                        // Идём по сделкам в счёте
                        foreach (var order in selOrderGroup)
                        {
                            decimal? sl = null;
                            if (order.StopLoss.HasValue) sl = Convert.ToDecimal(order.StopLoss);

                            decimal? tp = null;
                            if (order.TakeProfit.HasValue) tp = Convert.ToDecimal(order.TakeProfit);

                            var timeEnter = order.TimeEnter;
                            var timeExit = order.TimeExit;
                            var price = newEnterPrice.HasValue ? newEnterPrice : order.PriceEnter;
                            var priceExit = newExitPrice.HasValue ? newExitPrice : order.PriceExit;
                            var volume = newVolume.HasValue ? newVolume : order.Volume;
                            var symbol = !string.IsNullOrEmpty(newTicker) && newTicker.ToLower() != "null" ? newTicker : order.Symbol;
                            var side = newSide.HasValue ? newSide : order.Side;
                            var state = order.State;
                            #region

                            MarketOrder ordClosed = null;
                            if (state == PositionState.Closed)
                            {
                                // посчитать профит по сделке
                                ordClosed = order.MakeCopy();
                                ordClosed.State = PositionState.Closed;
                                ordClosed.PriceExit = priceExit.Value;
                                ordClosed.TimeExit = timeExit.Value;
                                string errorStr = null;
                                if (!priceExit.HasValue || !DealProfitCalculator.CalculateOrderProfit(ordClosed, acc.Currency, priceExit.Value, out errorStr))
                                {
                                    if (!string.IsNullOrEmpty(errorStr))
                                        Logger.Error("Сделка " + order.ID + " не будет закрыта : " + errorStr);
                                    else
                                        Logger.Error("Сделка " + order.ID + ". не будет закрыта : не указана priceExit.");
                                    continue;
                                }
                            }

                            try
                            {
                                //Если сделка из 'открытых'
                                if (state != PositionState.Closed)
                                {
                                    // поправить открытую позицию
                                    var pos = ctx.POSITION.First(p => p.ID == order.ID);
                                    pos.PriceEnter = Convert.ToDecimal(price.Value);
                                    pos.TimeEnter = timeEnter;
                                    pos.Stoploss = sl;
                                    pos.Takeprofit = tp;
                                    pos.Volume = volume.Value;
                                    pos.Symbol = symbol;
                                    pos.Side = side.Value;
                                    pos.State = (int) state;
                                }
                                else
                                {
                                    #region поправить закрытую позу и скорректировать результат

                                    var pos = ctx.POSITION_CLOSED.First(p => p.ID == order.ID);
                                    pos.PriceEnter = Convert.ToDecimal(price.Value);
                                    pos.TimeEnter = timeEnter;
                                    pos.Stoploss = sl;
                                    pos.Takeprofit = tp;
                                    pos.Volume = volume.Value;
                                    pos.Symbol = symbol;
                                    pos.Side = side.Value;
                                    pos.PriceExit = Convert.ToDecimal(priceExit.Value);
                                    pos.TimeExit = timeExit.Value;
                                    pos.ResultDepo = (decimal) ordClosed.ResultDepo;
                                    pos.ResultBase = (decimal) ordClosed.ResultBase;
                                    pos.ResultPoints = (decimal) ordClosed.ResultPoints;

                                    #endregion

                                    // поправить проводку и баланс
                                    UpdateBalanceChange(ctx, ordClosed, false);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Ошибка при редактировании сделки " + order.ID, ex);
                            }
                            #endregion

                            Logger.Info("Сделка " + order.ID + " отредактирована удачно.");
                        }
                        // коммит
                        ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("EditDangerDeal", ex);
                        return false;
                    }

                    if (currentState == PositionState.Closed) ReCalculateAccountBalance(ctx, acc.ID);
                }
                return true;
            }
        }

        /// <summary>
        /// Открывает новую сделку в ручную
        /// </summary>
        /// <param name="positionItem">экземпляр модели данных</param>
        /// <returns>Удачно ли выполнено добавления сделки</returns>
        public bool NewDeal(PositionItem positionItem)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var acc = ctx.ACCOUNT.FirstOrDefault(a => a.ID == positionItem.AccountID);
                    Logger.Error("NewDeal - ошибка открытия сделки в ручную. Не удалось найти счёт с номером " +
                                 positionItem.AccountID);
                    if (acc == null) return false;

                    var newPosition = new POSITION
                        {
                            AccountID = positionItem.AccountID,
                            Symbol = positionItem.Symbol,
                            TimeEnter = positionItem.TimeEnter,
                            PriceEnter = positionItem.PriceEnter,
                            Volume = positionItem.Volume,
                            Side = positionItem.Side,
                            State = (int) positionItem.State
                        };

                    ctx.POSITION.Add(newPosition);
                    ctx.SaveChanges();
                }
            }
            catch (UpdateException ex)
            {
                Logger.Error("NewDeal - ошибка открытия сделки в ручную. Ошибка добавления в базу данных. Возможно сущьность с таким идентификатором уже существует или указаны некорректные параметры", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("NewDeal - ошибка открытия сделки в ручную", ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Метод пересчитывает баланс во всех операциях, кроме закрытия или отмены сделок
        /// </summary>
        public void UpdateBalanceChange(TradeSharpConnection ctx, MarketOrder closedPos, bool deleteTransferOnly)
        {
            // поправить трансфер по счету и баланс
            var dealDescr = string.Format("{1} #{0}", closedPos.ID, Resource.TitleMarketOrderResult);
            var trans = ctx.BALANCE_CHANGE.FirstOrDefault(c => c.AccountID == closedPos.AccountID &&
                                                               (c.ChangeType ==
                                                                (int)BalanceChangeType.Profit ||
                                                                c.ChangeType ==
                                                                (int)BalanceChangeType.Loss) && c.Description.Equals(dealDescr, StringComparison.OrdinalIgnoreCase));
            if (deleteTransferOnly)
            {
                if (trans == null) return;
                ctx.BALANCE_CHANGE.Remove(trans);
            }
            else
            {
                // изменить или добавить перевод
                if (trans == null)
                {
                    trans = new BALANCE_CHANGE { Description = dealDescr, AccountID = closedPos.AccountID };
                    ctx.BALANCE_CHANGE.Add(trans);
                }
                trans.Amount = (decimal)Math.Abs(closedPos.ResultDepo);
                trans.ChangeType = closedPos.ResultDepo > 0
                                       ? (int)BalanceChangeType.Profit
                                       : (int)BalanceChangeType.Loss;
                if (closedPos.TimeExit != null) 
                    trans.ValueDate = closedPos.TimeExit.Value;
            }

            //ctx.BALANCE_CHANGE.ApplyCurrentValues(trans);
            ctx.SaveChanges();
        }
    
        /// <summary>
        /// Пересчёт баланса для указанного счёта. Применяется, например, после отмены уже закрытых сделок
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="accountId">уникаьлный идентификатор счёта, для которого пересчитываем баланс</param>
        public bool ReCalculateAccountBalance(TradeSharpConnection ctx, int accountId)
        {
            try
            {
                Logger.Info(string.Format("Пытыемся пересчитать баланс для счёта {0}", accountId));
                var bal = ctx.BALANCE_CHANGE.Where(b => b.AccountID == accountId).Sum(c =>
                                                                                      (c.ChangeType ==(int)BalanceChangeType.Deposit ||
                                                                                       c.ChangeType ==(int)BalanceChangeType.Profit ||
                                                                                       c.ChangeType ==(int) BalanceChangeType.Swap ? 1: -1)*c.Amount);

                var acc = ctx.ACCOUNT.Single(a => a.ID == accountId);
                acc.Balance = bal;
                ctx.SaveChanges();

                return true;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(string.Format(
                        "ReCalculateAccountBalance(). Не удалось пересчитать балланс {0}. Возможно нет ни одной проводки по счёту.",accountId), ex);
            }
            catch (Exception ex)
            {

                Logger.Error(string.Format("ReCalculateAccountBalance(). Не удалось пересчитать балланс счёта {0}.", accountId),ex);
            }
            return false;
        }      
    }
}