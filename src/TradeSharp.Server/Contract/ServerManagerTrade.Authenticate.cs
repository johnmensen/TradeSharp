using System.ServiceModel;
using System.ServiceModel.Channels;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Server.BL;

namespace TradeSharp.Server.Contract
{
    public partial class ManagerTrade
    {
        public AuthenticationResponse Authenticate(
            string login, string hash, 
            string terminalVersion,
            long terminalId, long clientTime,
            out int sessionTag)
        {
            string address;
            var clientCallback = GetContextParams(out address);
            return UserSessionStorage.Instance.Authenticate(login, hash, terminalVersion,
                clientTime, terminalId, 
                address, clientCallback, out sessionTag);
        }

        public void Logout(ProtectedOperationContext ctx)
        {
            UserSessionStorage.Instance.Logout(ctx);
        }

        public void Ping()
        {            
        }

        public void ReviveChannel(ProtectedOperationContext ctx, string login, int accountId, string terminalVersion)
        {
            string address;
            var clientCallback = GetContextParams(out address);
            UserSessionStorage.Instance.ReviveChannel(ctx, clientCallback, address, login, accountId, terminalVersion);
        }

        private ITradeSharpServerCallback GetContextParams(out string address)
        {
            var clientCallback = OperationContext.Current.GetCallbackChannel<ITradeSharpServerCallback>();
            var messageProperties = OperationContext.Current.IncomingMessageProperties;
            var endpointProperty =
                (RemoteEndpointMessageProperty)messageProperties[RemoteEndpointMessageProperty.Name];
            address = string.Format("{0}:{1}", endpointProperty.Address, endpointProperty.Port);
            return clientCallback;
        }        
    }
}