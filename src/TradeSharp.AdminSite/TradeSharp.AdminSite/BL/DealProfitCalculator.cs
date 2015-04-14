using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.SiteAdmin.BL
{
    // ReSharper disable LocalizableElement
    static class DealProfitCalculator
    {
        //класс используется при "закрытии" сделок "вручную"

        /// <summary>
        /// для ордера указана цена выхода, но не посчитана прибыль
        /// посчитать прибыль
        /// </summary>
        /// <param name="orderClosed"></param>
        /// <param name="depoCurrency">Валюта депозита</param>
        /// <param name="exitPrice"></param>
        /// <param name="errorStr"></param>
        /// <returns></returns>
        public static bool CalculateOrderProfit(MarketOrder orderClosed, string depoCurrency, float exitPrice, out string errorStr)
        {
            errorStr = string.Empty;
            
            if (exitPrice <= 0)
            {
                errorStr = "Цена закрытия не может быть меньше либо равной 0";
                return false;
            }

            orderClosed.PriceExit = exitPrice;
            // ReSharper disable PossibleInvalidOperationException
            var resultAbs = orderClosed.Side * (orderClosed.PriceExit.Value - orderClosed.PriceEnter);
            // ReSharper restore PossibleInvalidOperationException
            orderClosed.ResultPoints = DalSpot.Instance.GetPointsValue(orderClosed.Symbol, resultAbs);
            orderClosed.ResultBase = resultAbs * orderClosed.Volume;
            // прибыль в валюте депозита
            bool inverse, areSame;
            var smb = DalSpot.Instance.FindSymbol(orderClosed.Symbol, false, depoCurrency, out inverse, out areSame);
            if (string.IsNullOrEmpty(smb) && !areSame)
            {
                errorStr = "Котировки " + orderClosed + " -> " + depoCurrency + " нет в системе";
                return false;
            }

            var rate = 1f;
            if (!areSame)
            {
                if (smb == orderClosed.Symbol)                                    
                    rate = inverse ? 1 / orderClosed.PriceExit.Value : orderClosed.PriceExit.Value;
                else
                {
                    var quote = QuoteStorage.Instance.ReceiveValue(smb);
                    if (quote == null)
                    {
                        errorStr = "Нет котировки по паре " + smb;
                        return false;
                    }
                    rate = inverse ? 1 / quote.ask : quote.bid;
                }
            }
            orderClosed.ResultDepo = rate * orderClosed.ResultBase;
            return true;
        }
    }
    // ReSharper restore LocalizableElement
}
