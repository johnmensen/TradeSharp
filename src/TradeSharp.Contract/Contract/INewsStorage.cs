using System;
using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [ServiceContract]
    public interface INewsStorage
    {
        [OperationContract(IsOneWay = false)]
        List<News> GetNews(int account, DateTime date, int[] newsChannelIds);

        [OperationContract(IsOneWay = false)]
        NewsMap GetNewsMap(int accountId);
    }
}
