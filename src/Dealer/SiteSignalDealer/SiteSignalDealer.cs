using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.DealerInterface;
using TradeSharp.Util;

namespace SiteSignalDealer
{
    public class SiteSignalDealer : SimpleDealer, IDealer
    {
        private readonly string fileName;

        private string queryString;

        private string errorString;

        public string DealerCode { get; private set; }

        public List<string> GroupCodes { get; private set; }
        
        public SiteSignalDealer(DealerDescription desc, List<string> accountGroupCodes)
        {
            GroupCodes = accountGroupCodes;
            DealerCode = desc.Code;
            fileName = desc.FileName;
        }

        public void Initialize()
        {
            var cfg = new DealerConfigParser(typeof(SiteSignalDealer), fileName);
            queryString = cfg.GetString("queryString", "http://forexinvest.com/api/put_open_deals?msg=");
        }

        public void Deinitialize()
        {
        }

        public string GetErrorString()
        {
            return errorString;
        }

        public void ClearError()
        {
            errorString = string.Empty;
        }

        public RequestStatus SendNewOrderRequest(Account account, MarketOrder order, 
            OrderType ordType, decimal requestedPrice, decimal slippagePoints)
        {
            order.State = PositionState.Opened;
            order.TimeEnter = DateTime.Now;

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

            int posID;
            // сохранить ордер (и уведомить клиента)
            var result = ServerInterface.SaveOrderAndNotifyClient(order, out posID);
            var status = result ? RequestStatus.OK : RequestStatus.SerializationError;

            if (status == RequestStatus.OK)
                SendRequest(order, "open");
            return status;
        }

        public RequestStatus SendCloseRequest(MarketOrder order, PositionExitReason reason)
        {
            // получить цену
            var quote = QuoteStorage.Instance.ReceiveValue(order.Symbol);
            if (quote == null)
                return RequestStatus.NoPrice;
            var price = order.Side > 0 ? quote.GetPrice(QuoteType.Bid) : quote.GetPrice(QuoteType.Ask);

            // закрыть ордер немедленно
            var status = ServerInterface.CloseOrder(order, (decimal)price, reason)
                ? RequestStatus.OK : RequestStatus.ServerError;

            if (status != RequestStatus.OK) return status;
            order.PriceExit = order.Side > 0 ? quote.GetPrice(QuoteType.Bid) : quote.GetPrice(QuoteType.Ask);
            SendRequest(order, "close");
            return status;
        }

        public override RequestStatus ModifyMarketOrderRequest(MarketOrder order)
        {
            var status = ServerInterface.ModifyMarketOrder(order)
                ? RequestStatus.OK : RequestStatus.ServerError;
            if (status == RequestStatus.OK)
                SendRequest(order, "modify");
            return status;
        }

        private void SendRequest(MarketOrder order, string cmdType)
        {
            var text = MakeOrderJSON(order, cmdType);

            Logger.Info(text);
            try
            {
                var url = queryString + text;
                var req = System.Net.WebRequest.Create(url);
                req.Method = "GET";

                Logger.Info("Пресигнал отправлен на сайт");

                req.GetResponse();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка публикации события (сделки) на сайт", ex);
            }
        }

        private static string MakeOrderJSON(MarketOrder order, string cmdType)
        {
            var orderData = new Dictionary<string, string>
                {
                    {"account", order.AccountID.ToString()},
                    {"order", cmdType},
                    {"id", order.ID.ToString()},
                    {"side", (order.Side > 0 ? "BUY" : "SELL")},
                    {"volume", order.Volume.ToString()},
                    {"symbol", order.Symbol},
                    {"priceEnter", order.PriceEnter.ToStringUniformPriceFormat()},
                    {"timeEnter", order.TimeEnter.ToStringUniform()}
                };
            orderData.Add("timeExit", order.TimeExit.HasValue ? order.TimeExit.Value.ToStringUniform() : "");
            orderData.Add("priceExit", order.PriceExit.HasValue ? order.PriceExit.Value.ToStringUniformPriceFormat(true) : "");
            orderData.Add("profit", order.ResultDepo.ToStringUniform(2));
            orderData.Add("SL", order.StopLoss.HasValue ? order.StopLoss.Value.ToStringUniformPriceFormat() : "");
            orderData.Add("TP", order.TakeProfit.HasValue ? order.TakeProfit.Value.ToStringUniformPriceFormat() : "");
            if (order.TrailLevel1.HasValue && order.TrailLevel1 > 0)
            {
                var trailPrice =
                    order.PriceEnter +
                    order.Side * DalSpot.Instance.GetAbsValue(order.Symbol, order.TrailLevel1.Value);
                var trailTarget = order.PriceEnter +
                    order.Side * DalSpot.Instance.GetAbsValue(order.Symbol, order.TrailTarget1 ?? 0);

                orderData.Add("trailingLevel", trailPrice.ToStringUniformPriceFormat());
                orderData.Add("trailingTarget", trailTarget.ToStringUniformPriceFormat());
            }
            var profitPoints = 0f;
            if (order.PriceExit.HasValue)
                profitPoints = DalSpot.Instance.GetPointsValue(order.Symbol, order.Side * (order.PriceExit.Value - order.PriceEnter));
            orderData.Add("profitPoints", profitPoints.ToStringUniform(3));

            return "{" + string.Join(",", orderData.Select(p => string.Format("\"{0}\": \"{1}\"", p.Key, p.Value))) + "}";
        }    
    }
}
