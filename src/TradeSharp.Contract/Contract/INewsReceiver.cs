using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [ServiceContract]
    public interface INewsReceiver
    {
        [OperationContract(IsOneWay = false)]
        void PutNews(News[] news);
    }
}
