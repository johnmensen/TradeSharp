using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.TradeSignalExecutor.BL
{
    /// <summary>
    /// хранит информацию об обработанном торговом сигнале для
    /// отображения в Веб-интерфейсе
    /// </summary>
    class ProcessedSignal
    {
        public TradeSignalAction signal;

        public DateTime time = DateTime.Now;

        public readonly Dictionary<int, RequestStatus> subscriberAccountProcessingStatus = new Dictionary<int, RequestStatus>();
    }
}
