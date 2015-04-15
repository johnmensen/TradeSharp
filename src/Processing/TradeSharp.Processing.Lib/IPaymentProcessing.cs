using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Processing.Lib
{
    [ServiceContract]
    [XmlSerializerFormat]
    interface IPaymentProcessing
    {
        [OperationContract(IsOneWay = false)]
        bool MakePayment(Wallet wallet, decimal amount, string targetPurse);
    }
}
