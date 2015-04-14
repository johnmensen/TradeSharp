using System;
using System.Collections.Generic;
using WebMoney.XmlInterfaces.Responses;

namespace TradeSharp.Processing.WebMoney.Server
{
    public interface IPaymentAccessor
    {
        bool CheckInitial();
        List<Transfer> GetTransfers(DateTime startTime, DateTime finishTime);
    }
}
