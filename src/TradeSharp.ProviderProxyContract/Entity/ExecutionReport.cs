using System;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.ProviderProxyContract.Entity
{
    [Serializable]
    public class ExecutionReport : MessssageQueueItem
    {
        public BrokerResponse brokerResponse;

        public override string Title
        {
            get { return string.Format("ExecutionReport_{0}", brokerResponse.RequestId); }
            protected set { }
        }

        public ExecutionReport()
        {
            brokerResponse = new BrokerResponse();
        }

        public override bool SendToQueue(bool isError)
        {
            try
            {
                SendToQueue(!isError ? MessageQueues.QueueFromProvider 
                    : MessageQueues.QueueFromProviderError, isError);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format("ExecutionReport на {0}, статус {1}, цена {2:f4}, отказ:{3}",
                                 brokerResponse.RequestId, brokerResponse.Status, brokerResponse.Price,
                                 string.IsNullOrEmpty(brokerResponse.RejectReasonString)
                                 ? brokerResponse.RejectReason.ToString() : brokerResponse.RejectReasonString);
        }
    }
}
