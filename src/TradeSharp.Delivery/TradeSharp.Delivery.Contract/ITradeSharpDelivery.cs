using System.ServiceModel;

namespace TradeSharp.Delivery.Contract
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface ITradeSharpDelivery
    {
        [OperationContract(IsOneWay = true)]
        void DeliverEmail(string[] addresses, string body, string title, bool htmlFormat);
    }
}
