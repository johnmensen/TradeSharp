using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace TradeSharp.SiteBridge.QuoteServer.BL
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface ICandleStorage
    {
        [OperationContract(IsOneWay = false)]
        List<KosherCandle> GetCandles(string timeframe,
            string ticker, DateTime timeStart, out string errorCode);

        [OperationContract(IsOneWay = false)]
        List<KosherCandle> GetCandlesSilent(string timeframe,
            string ticker, DateTime timeStart);

        [OperationContract(IsOneWay = false)]
        List<KosherCandle> GetCandlesSilentDateString(string timeframe,
            string ticker, string timeStartStr);

        [OperationContract(IsOneWay = false)]
        string[] GetEnabledTickersAndTimeframes();
    }
}
