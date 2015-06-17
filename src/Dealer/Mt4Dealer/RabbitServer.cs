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
                parser.GetString("MQ.Host", "mq1.amarkets.org"),
                parser.GetString("MQ.ResponseQueueName", "mt4_dealer_response"),
                parser.GetString("MQ.User", "deploy"),
                parser.GetString("MQ.Password", "a6e92c1FEA"),
                ProcessRabbitResponse);

            rabbitSender = new RabbitSender(
                AppConfig.GetStringParam("MQ.Host", "mq1.amarkets.org"),
                AppConfig.GetStringParam("MQ.User", "deploy"),
                AppConfig.GetStringParam("MQ.Password", "a6e92c1FEA"),
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
