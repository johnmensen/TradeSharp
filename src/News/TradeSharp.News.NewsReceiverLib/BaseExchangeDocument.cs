namespace TradeSharp.News.NewsReceiverLib
{
    public abstract class BaseExchangeDocument
    {
        public abstract void ReceiveDoc();
        public abstract void ParseDoc();
        public abstract void UpdateDB();
    }
}
