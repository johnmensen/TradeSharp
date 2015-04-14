using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using TradeSharp.Contract.Contract;

namespace TradeSharp.Server.Contract
{
    //[AttributeUsage(AttributeTargets.Class)]
    //public class ServiceManagerCallbackAttribute : Attribute, IInputSessionShutdown, IContractBehavior, IDispatchMessageInspector
    //{
    //    #region IInputSessionShutdown Members

    //    public void ChannelFaulted(IDuplexContextChannel channel)
    //    {
    //    }

    //    public void DoneReceiving(IDuplexContextChannel channel)
    //    {// исключить из списка зарегистрированных клиентов
    //        ServiceManagerClientManager.RemoveClient(channel.SessionId);
    //    }

    //    #endregion

    //    #region IContractBehavior Members

    //    public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
    //    {

    //    }

    //    public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    //    {

    //    }

    //    public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
    //    {
    //        dispatchRuntime.InputSessionShutdownHandlers.Add(new ServiceManagerCallbackAttribute());
    //        dispatchRuntime.MessageInspectors.Add(new ServiceManagerCallbackAttribute());
    //    }

    //    public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
    //    {

    //    }

    //    #endregion

    //    #region IDispatchMessageInspector Members (Аналог синков в Remoting)

    //    public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel,
    //        InstanceContext instanceContext)
    //    {
    //        var clientCallback = OperationContext.Current.GetCallbackChannel<ITradeSharpServerCallback>();
    //        ServiceManagerClientManager.AddClient(channel.SessionId, clientCallback);
    //        return null;
    //    }

    //    public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
    //    {
    //    }

    //    #endregion
    //}
}
