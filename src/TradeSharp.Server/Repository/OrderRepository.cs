using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Server.BL;
using TradeSharp.Server.Contract;
using TradeSharp.Util;

namespace TradeSharp.Server.Repository
{
    class OrderRepository : IOrderRepository
    {
        private static readonly Lazy<IOrderRepository> instance = new Lazy<IOrderRepository>(() => new OrderRepository());

        private IBrokerRepository brokerRepository;

        public static IOrderRepository Instance
        {
            get { return instance.Value; }
        }

        private OrderRepository()
        {
            brokerRepository = new BrokerRepository();
        }

        public bool CloseOrder(MarketOrder order, decimal price, PositionExitReason exitReason)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                var account = ctx.ACCOUNT.FirstOrDefault(ac => ac.ID == order.AccountID);
                if (account == null)
                {
                    Logger.ErrorFormat("Закрытие ордера #{0}: невозможно прочитать данные счета ({1})",
                                       order.ID, order.AccountID);
                    return false;
                }

                // провести ордер через биллинг
                ORDER_BILL bill = null;
                if (order.State == PositionState.Opened)
                    bill = BillingManager.ProcessPriceForOrderClosing(order, LinqToEntity.DecorateAccount(account), ctx);

                // посчитать результат
                // и обновить объект
                order.State = PositionState.Closed;
                order.PriceExit = (float?)price;
                var deltaAbs = order.Side * (order.PriceExit.Value - order.PriceEnter);
                order.ResultPoints = DalSpot.Instance.GetPointsValue(order.Symbol, deltaAbs);
                var deltaDepo = deltaAbs * order.Volume;
                var quotes = QuoteStorage.Instance.ReceiveAllData();
                string errorStr;
                var resultedDepo = DalSpot.Instance.ConvertToTargetCurrency(order.Symbol, false, account.Currency,
                                                                            deltaDepo, quotes, out errorStr, false);

                if (!resultedDepo.HasValue)
                {
                    Logger.ErrorFormat("#{0} ({1} {2}{3}, {4:f1} пп): ошибка расчета прибыли в валюте депозита - {5}",
                                       order.ID,
                                       order.Side > 0 ? "B" : "S",
                                       order.Symbol,
                                       order.Volume,
                                       order.ResultPoints,
                                       errorStr);
                    return false;
                }
                order.ResultDepo = (float)resultedDepo.Value;
                //order.Swap = (float)swap;
                order.ExitReason = exitReason;
                order.TimeExit = DateTime.Now;
                var posClosed = LinqToEntity.UndecorateClosedPosition(order);

                POSITION pos = null;
                try
                {
                    // занести ордер в список закрытых позиций (создать новую запись "истории")
                    ctx.POSITION_CLOSED.Add(posClosed);

                    // удалить открытый ордер
                    pos = ctx.POSITION.FirstOrDefault(p => p.ID == order.ID);
                    if (pos == null)
                    {
                        Logger.ErrorFormat("CloseOrder - позиция {0} не найдена", order.ID);
                        ServiceManagerClientManagerProxy.Instance.CloseOrderResponse(null, RequestStatus.ServerError, "crudsav");

                        return false;
                    }
                    ctx.POSITION.Remove(pos);

                    // посчитать профиты
                    if (bill != null)
                        BillingManager.ProcessOrderClosing(order, account, bill, ctx, quotes, brokerRepository.BrokerCurrency);

                    // сохранить изменения
                    ctx.SaveChanges();

                    // обновить баланс
                    var resultAbs = Math.Abs(order.ResultDepo);
                    if (!UpdateAccountBalance(ctx,
                        account, (decimal)resultAbs,
                                              order.ResultDepo >= 0
                                                  ? BalanceChangeType.Profit
                                                  : BalanceChangeType.Loss,
                                              string.Format("результат сделки #{0}", posClosed.ID), DateTime.Now, order.ID))
                        Logger.ErrorFormat("Не удалось применить обновление баланса #{0}", posClosed.ID);
                }
                catch (OptimisticConcurrencyException ex)
                {
                    Logger.Error("CloseOrder - OptimisticConcurrencyException", ex);
                    ctx.Entry(posClosed).State = EntityState.Modified;
                    ((IObjectContextAdapter)ctx).ObjectContext.Refresh(RefreshMode.ClientWins, posClosed);
                    if (pos != null)
                    {
                        ctx.Entry(pos).State = EntityState.Modified;
                        ((IObjectContextAdapter)ctx).ObjectContext.Refresh(RefreshMode.ClientWins, pos);
                    }
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка закрытия позиции {0} (счет #{1}) (фиксация в БД): {2}", 
                        order.ID, order.AccountID, ex);
                    ServiceManagerClientManagerProxy.Instance.CloseOrderResponse(null, RequestStatus.ServerError, "crudsave");
                    return false;
                }
            }

            // уведомить клиента
            ServiceManagerClientManagerProxy.Instance.CloseOrderResponse(order, RequestStatus.OK, "");

            // разослать торговый сигнал
            MakeOrderClosedSignal(order.AccountID, order.ID, (float)price);

            return true;
        }

        public bool UpdateAccountBalance(TradeSharpConnection ctx,
            ACCOUNT account, decimal amount, BalanceChangeType changeType,
            string description, DateTime valueDate, int? positionId)
        {
            var bc = new BALANCE_CHANGE
            {
                AccountID = account.ID,
                Amount = amount,
                ChangeType = (int)changeType,
                Description = description,
                ValueDate = valueDate,
                Position = positionId
            };
            try
            {
                ctx.BALANCE_CHANGE.Add(bc);
                account.Balance += ((changeType == BalanceChangeType.Deposit ||
                    changeType == BalanceChangeType.Profit) ? amount : -amount);
                ctx.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка обновления баланса счета {0} на сумму {1}: {2}",
                    account.ID, amount, ex);
                return false;
            }
        }

        public void MakeOrderClosedSignal(int accountId, int orderId, float priceExit)
        {
            var signalCat = ManagementTraderList.Instance.IsSignaller(accountId);
            if (!signalCat.HasValue) return;
            var action = new TradeSignalActionClose
            {
                OrderId = orderId,
                Price = priceExit,
                ServiceId = signalCat.Value
            };
            ServiceManagerClientManagerProxy.Instance.ProcessTradeSignalAction(action);
        }
    }
}
