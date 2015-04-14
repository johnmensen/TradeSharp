using System;
using System.Collections.Generic;
using BSEngine.SignalDelivery.Contract.Entity;
using BSEngine.SignalDelivery.Contract.Proxy;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.DealerInterface;
using TradeSharp.Util;

namespace SignalDealer
{
    /// <summary>
    /// автоматическое подтверждение сделок с отправкой управляющего сигнала
    /// в систему BSEngine
    /// </summary>
    public class SignalDealer : SimpleDealer, IDealer
    {
        private string endpointAddress;
        private string bingingConfigName;
        /// <summary>
        /// категория торгового сигнала
        /// </summary>
        private int signalCategoryCode;

        private string fileName;
        
        public string DealerCode { get; private set; }

        public List<string> GroupCodes { get; private set; }

        public SignalDealer(DealerDescription desc, List<string> accountGroupCodes)
        {
            GroupCodes = accountGroupCodes;
            DealerCode = desc.Code;
            fileName = desc.FileName;
        }

        public void Initialize()
        {
            var cfg = new DealerConfigParser(typeof(SignalDealer), fileName);
            // попытаться "вручную" инициализировать привязку к серверу
            // клиента WCF ManagerSignalProxy
            endpointAddress = cfg.GetString("endpointAddress", "net.tcp://localhost:55157/TradeSignalManager");
            bingingConfigName = cfg.GetString("bingingConfigName", "OpenNetTcpBinding");
            signalCategoryCode = cfg.GetInt("tradeSignalCategory", 1);
        }

        public void Deinitialize()
        {
        }

        public string GetErrorString()
        {
            return string.Empty;
        }

        public void ClearError()
        {
        }

        public RequestStatus SendNewOrderRequest(Account account, 
            MarketOrder order,
            OrderType orderType,
            decimal requestedPrice,
            decimal slippagePoints)
        {
            order.TimeEnter = DateTime.Now;
            order.State = PositionState.Opened;
            
            // подставить текущую цену
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote == null)
                return RequestStatus.NoPrice;
            order.PriceEnter = order.Side > 0 ? quote.ask : quote.bid;
            
            // проверить проскальзывание
            if (slippagePoints != 0)
            {
                var slippageAbs = DalSpot.Instance.GetAbsValue(order.Symbol, slippagePoints);
                var delta = Math.Abs(order.PriceEnter - (float)requestedPrice);
                if (delta > (float)slippageAbs) return RequestStatus.Slippage;
            }

            if (account.Balance <= 0)
            {
                Logger.ErrorFormat("SendNewOrderRequest (счет {0}) - баланс счета равен 0", account.ID);
                return RequestStatus.DealerError;
            }
            // получить уникальный Id сигнала, он же - Magic для ордера
            // посчитать плечо, пересчитав объем в валюту депо
            bool areEqual, inverse;
            var baseTicker = DalSpot.Instance.FindSymbol(order.Symbol, true, account.Currency, 
                out inverse, out areEqual);
            if (!areEqual && string.IsNullOrEmpty(baseTicker))
            {
                Logger.ErrorFormat("SendNewOrderRequest - неизвестный сивол пересчета базовой валюты ({0})",
                    order.Symbol);
                return RequestStatus.DealerError;
            }
            var rateDepo = 1f;
            if (!areEqual)
            {
                var baseQuote = QuoteStorage.Instance.ReceiveValue(baseTicker);
                if (baseQuote == null)
                {
                    Logger.ErrorFormat("SendNewOrderRequest - нет котировки {0} для пересчета базовой валюты",
                                       baseQuote);
                    return RequestStatus.DealerError;
                }
                rateDepo = inverse ? 1/quote.ask : quote.bid;
            }
            var volumeDepo = order.Volume * rateDepo;
            var leverage = (decimal) volumeDepo / account.Balance;

            // отправить сигнал
            var signal = new ManagerSignal 
            {
                Id = DateTime.Now.Ticks.ToString(),
                Price = (decimal)order.PriceEnter, 
                Side = order.Side, 
                Symbol = order.Symbol,
                Leverage = (decimal)leverage,
                Category = signalCategoryCode
            };

            // !!!
            // переделал ID сигнала в запись поля expertComment вместо comment
            //order.Comment = signal.Id;
            order.ExpertComment = signal.Id;

            // отправка сигнала успешна?)
            var timeExec = new ThreadSafeTimeStamp();
            timeExec.Touch();
            if (!SendSignal(signal, true)) return RequestStatus.DealerError;
            int posID;
            // сохранить ордер (и уведомить клиента)
            var endTime = DateTime.Now - timeExec.GetLastHit();
            Logger.InfoFormat("Время исполнения SendNewOrderRequest SendSignal: {0} секунд, {1} миллисекунд", endTime.TotalSeconds, endTime.TotalMilliseconds);
            timeExec.Touch();
            var result = ServerInterface.SaveOrderAndNotifyClient(order, out posID);
            endTime = DateTime.Now - timeExec.GetLastHit();
            Logger.InfoFormat("Время исполнения SendNewOrderRequest - SaveOrderAndNotifyClient: {0} секунд, {1} миллисекунд", endTime.TotalSeconds, endTime.TotalMilliseconds);
            return result ? RequestStatus.OK : RequestStatus.SerializationError;
        }

        public RequestStatus SendCloseRequest(MarketOrder order, PositionExitReason reason)
        {
            if (string.IsNullOrEmpty(order.ExpertComment))
            {
                Logger.ErrorFormat("SignalDealer.SendCloseRequest({0}) - поле ExpertComment не заполнено",
                    order.ID);
                //return RequestStatus.DealerError;
            }
            // получить цену
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote == null)
                return RequestStatus.NoPrice;
            var price = order.Side > 0 ? quote.GetPrice(QuoteType.Bid) : quote.GetPrice(QuoteType.Ask);
            // отправить сигнал
            var signal = new ManagerSignal {Id = order.ExpertComment, PriceClose = (decimal)price, Category = signalCategoryCode, 
                Symbol = order.Symbol, Leverage = 0, Price = (decimal)order.PriceEnter, Side = order.Side};
            var timeExec = new ThreadSafeTimeStamp();
            timeExec.Touch();
            if (!string.IsNullOrEmpty(order.ExpertComment) && !SendSignal(signal, false)) 
                return RequestStatus.DealerError;
            var endTime = DateTime.Now - timeExec.GetLastHit();
            Logger.InfoFormat("Время исполнения SendCloseRequest SendSignal: {0} секунд, {1} миллисекунд", endTime.TotalSeconds, endTime.TotalMilliseconds);
            timeExec.Touch();
            // закрыть ордер немедленно
            var result = ServerInterface.CloseOrder(order, (decimal) price, reason);
            endTime = DateTime.Now - timeExec.GetLastHit();
            Logger.InfoFormat("Время исполнения SendCloseRequest - CloseOrder: {0} секунд, {1} миллисекунд", endTime.TotalSeconds, endTime.TotalMilliseconds);
            return result ? RequestStatus.OK : RequestStatus.ServerError;
        }
    
        /// <summary>
        /// отправить сигнал в систему BSEngine
        /// 
        /// </summary>                
        private bool SendSignal(ManagerSignal signal, bool isOpen)
        {
            ManagerSignalProxy proxy;
            try
            {
                proxy = new ManagerSignalProxy(endpointAddress, bingingConfigName);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalDealer (create proxy) - ошибка отправки сигнала №{0} ({1} {2} {3}) - {4}",
                    signal.Id, isOpen ? "откр." : "закр.",
                    signal.Side, signal.Symbol, ex);
                Logger.ErrorFormat("для proxy \"{0}\", binding \"{1}\"", endpointAddress, bingingConfigName);
                return false;
            }
            
            try
            {
                return isOpen
                           ? proxy.ProcessOpenSignal(signal)
                           : proxy.ProcessCloseSignal(signal);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("SignalDealer - ошибка отправки сигнала №{0} ({1} {2} {3}) - {4}",
                    signal.Id, isOpen ? "откр." : "закр.",
                    signal.Side, signal.Symbol, ex);
                Logger.ErrorFormat("для proxy \"{0}\", binding \"{1}\"", endpointAddress, bingingConfigName);
                return false;
            }            
        }
    }
}