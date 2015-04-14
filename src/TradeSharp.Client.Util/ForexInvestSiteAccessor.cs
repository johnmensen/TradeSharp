using System.Windows.Forms;
using TradeSharp.Contract.Util.Proxy;

namespace TradeSharp.Client.Util
{
    public class ForexInvestSiteAccessor
    {
        private string login;
        private string token;
        private long lockTime;

        private const string SiteUrlDefault = "http://forexinvest.com";
        public string siteUrl;

        private readonly WebBrowser webBrowser;

        private bool isLogin;
        public bool IsLogIn
        {
            get { return isLogin; }
        }

        public ForexInvestSiteAccessor(WebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
            siteUrl = SiteUrlDefault;
            var sysMeta = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("TerminalUrl");
            if (sysMeta == null) return;

            object siteUrlObj;
            sysMeta.TryGetValue("BrowserUrl", out siteUrlObj);
            if (siteUrlObj == null) return;

            var urlStr = siteUrlObj as string;
            if (string.IsNullOrEmpty(urlStr)) return;

            siteUrl = urlStr;
        }

        public void LoginOnSite(string currentLogin, string currentToken, long currentLockTime)
        {
            login = currentLogin;
            token = currentToken;
            lockTime = currentLockTime;
            webBrowser.DocumentCompleted += LoginToSiteDocumentCompleted;
            webBrowser.Navigate(siteUrl);
        }

        void LoginToSiteDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser == null || webBrowser.Document == null) return;

            webBrowser.Document.InvokeScript("setAuthStorage", new object[] { login, lockTime, token });
            webBrowser.DocumentCompleted -= LoginToSiteDocumentCompleted;

            isLogin = true;
        }
    }
}
