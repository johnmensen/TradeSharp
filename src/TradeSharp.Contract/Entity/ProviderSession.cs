using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class ProviderSession
    {
        public string MessageQueue { get; set; }
        public string SessionName { get; set; }
        public string HedgingAccount { get; set; }
        public string SenderCompId { get; set; }

        public ProviderSession() {}
        public ProviderSession(string messageQueue, string sessionName, string hedgingAccount, string senderCompId)
        {
            MessageQueue = messageQueue;
            SessionName = sessionName;
            HedgingAccount = hedgingAccount;
            SenderCompId = senderCompId;
        }
    }
}
