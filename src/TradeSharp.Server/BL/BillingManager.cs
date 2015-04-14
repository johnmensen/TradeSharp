using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    static class BillingManager
    {
        /// <summary>
        /// добавить маркапы и прочее шкурилово
        /// 
        /// вызывается на открытии рыночного ордера, срабатывании отложенного
        /// (для всех - единая точка входа - SaveOrder & Notify Client)
        /// </summary>
        public static OrderBill ProcessOrderOpening(MarketOrder order, Account account)
        {
            AccountGroup.MarkupType markType;
            var spreadDelta = AcccountMarkupDictionary.GetMarkupAbs(account.Group, order.Symbol, out markType);
            var bill = new OrderBill(0, markType, 0);
            if (spreadDelta > 0 && markType != AccountGroup.MarkupType.NoMarkup)
            {
                if (markType == AccountGroup.MarkupType.Markup)
                    order.PriceEnter += spreadDelta * order.Side;
                bill.MarkupEnter = spreadDelta;
            }
            
            return bill;
        }

        /// <summary>
        /// вызывается для нового, только что созданного ордера
        /// создает запись по ордеру
        /// </summary>
        public static void SaveNewOrderBill(OrderBill bill, int positionId, TradeSharpConnection ctx)
        {
            bill.Position = positionId;
            ctx.ORDER_BILL.Add(LinqToEntity.UndecorateOrderBill(bill));
        }

        public static ORDER_BILL ProcessPriceForOrderClosing(MarketOrder order, Account account,
            TradeSharpConnection ctx)
        {
            AccountGroup.MarkupType markType;
            var spreadDelta = AcccountMarkupDictionary.GetMarkupAbs(account.Group, order.Symbol, out markType);
            var markupExit = 0f;
            if (spreadDelta > 0 && markType != AccountGroup.MarkupType.NoMarkup)
            {
                markupExit = spreadDelta;
                if (markType == AccountGroup.MarkupType.Markup)
                    order.PriceExit -= spreadDelta * order.Side;
            }

            var bill = ctx.ORDER_BILL.FirstOrDefault(b => b.Position == order.ID);
            if (bill != null)
                bill.MarkupExit = markupExit;
            //else
            //    Logger.Debug("Markup: ProcessPriceForOrderClosing - can not find the bill for pos " + order.ID);
            
            return bill;
        }

        /// <summary>
        /// добавить маркапы и прочее шкурилово
        /// </summary>
        public static void ProcessOrderClosing(MarketOrder order,
            ACCOUNT account,
            ORDER_BILL bill, 
            TradeSharpConnection ctx,
            Dictionary<string, QuoteData> quotes, string brokerCurrency)
        {
            //Logger.Info("BillingManager.ProcessOrderClosing()");
            try
            {
                // перевести маркапы в валюту брокера
                var markupSum = bill.MarkupExit + bill.MarkupEnter;
                // в контрвалюте...
                if (markupSum != 0)
                    markupSum = order.Volume * markupSum;
                Logger.Info("BillingManager: markupSum is " + markupSum);
                    
                // перевести в валюту брокера
                var resultCounter = order.Side * order.Volume * (order.PriceExit.Value - order.PriceEnter);
                var rateCounterDepo = resultCounter == 0 ? 1 : order.ResultDepo / resultCounter;
                // валюта брокера к контрвалюте 
                var rateCounterBroker = rateCounterDepo;
                Logger.Info("BillingManager: rateCounterDepo is " + rateCounterDepo);

                if (account.Currency != brokerCurrency)
                {
                    // пример: позиция EURCAD, валюта брокера USD
                    // надо получить курс CADUSD
                    var markupBroker = (float) markupSum;
                    string errorStr;
                    var resultedMarkup = DalSpot.Instance.ConvertToTargetCurrency(order.Symbol, false, brokerCurrency,
                                                                            markupBroker, quotes, out errorStr, true);

                    if (!resultedMarkup.HasValue)
                    {
                        Logger.ErrorFormat("BillingManager.ProcessOrderClosing - невозможно перевести профит по {0} в {1}: {2}",
                            order.Symbol, brokerCurrency, errorStr);
                        return;
                    }
                    markupBroker = (float)resultedMarkup.Value;
                    rateCounterBroker = (float)(markupBroker / markupSum);
                    markupSum = markupBroker;
                }
                else
                    markupSum *= rateCounterBroker;
                bill.MarkupBroker = markupSum;
                bill.ProfitBroker = resultCounter * rateCounterBroker;
                Logger.InfoFormat("BillingManager: MarkupBroker: {0}, ProfitBroker: {1}",
                    bill.MarkupBroker, bill.ProfitBroker);
              
                // сохранить билль
                ctx.Entry(bill).State = EntityState.Modified;
                Logger.InfoFormat("BillingManager:OK");
            }
            catch (Exception ex)
            {
                Logger.Error("BillingManager.ProcessOrderClosing() - ошибка редактирования счета", ex);
            }
        }
    }
}
