using System;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Proxy
{
    
    public class NewsReceiverProxy : INewsReceiver
    {
        private readonly ChannelFactory<INewsReceiver> factory;
        private readonly INewsReceiver channel;

        public NewsReceiverProxy(string endpointName)
        {
            factory = new ChannelFactory<INewsReceiver>(endpointName);
            channel = factory.CreateChannel();
        }
        public void  PutNews(News[] news)
        {
 	        if (channel == null) throw new Exception("NewsReceiverProxy: связь не установлена");
                    channel.PutNews(news); 
        }
    }
}
