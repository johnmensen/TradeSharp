using System;
using BrokerService.Contract.Entity;
using BrokerService.Contract.RabbitMQ;
using TradeSharp.DealerInterface;
using TradeSharp.Util;

namespace Mt4Dealer
{
    class RabbitServer
    {
        private readonly RabbitMqHost rabbitHost;

        private readonly RabbitSender rabbitSender;

        private readonly Action<BrokerResponse> processResponse;

        public RabbitServer(DealerConfigParser parser, Action<BrokerResponse> processResponse)
        {
            this.processResponse = processResponse;
            rabbitHost = new RabbitMqHost(
                parser.GetString("MQ.Host", "188.226.214.27"),
                parser.GetString("MQ.ResponseQueueName", "mt4_dealer_response"),
                parser.GetString("MQ.User", "local_office"),
                parser.GetString("MQ.Password", "local_office"),
                ProcessRabbitResponse);

            rabbitSender = new RabbitSender(
                AppConfig.GetStringParam("MQ.Host", "188.226.214.27"),
                AppConfig.GetStringParam("MQ.User", "local_office"),
                AppConfig.GetStringParam("MQ.Password", "local_office"),
                AppConfig.GetStringParam("MQ.RequestQueueName", "mt4_dealer_request"));
        }

        public void SendRequest(TradeTransactionRequest request)
        {
            rabbitSender.SendObject(request);
        }

        public void Start()
        {
            rabbitHost.Start();
        }

        public void Stop()
        {
            rabbitHost.Stop();
        }

        private void ProcessRabbitResponse(object res)
        {
            if (res == null) return;
            var response = res as BrokerResponse;
            processResponse(response);
        }
    }
}
