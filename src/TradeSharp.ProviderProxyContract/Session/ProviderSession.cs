namespace TradeSharp.ProviderProxyContract.Session
{
    // ReSharper disable InconsistentNaming
    public class ProviderSession
    {
        public string title;
        public string nameFIX;
        public ProviderSession() {}
        public ProviderSession(string title, string nameFIX)
        {
            this.title = title;
            this.nameFIX = nameFIX;
        }
        public override string ToString()
        {
            return string.Format("{0}({1})", title, nameFIX);
        }
    }
    // ReSharper restore InconsistentNaming
}
