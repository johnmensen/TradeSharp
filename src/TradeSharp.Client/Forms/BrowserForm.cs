using System;
using System.Windows.Forms;
using System.Linq;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.Client.Forms
{
    public partial class BrowserForm : Form, IMdiNonChartWindow
    {
        private const int MaxUrlsStored = 12;

        public static BrowserForm Instance { get; private set; }

        private ForexInvestSiteAccessor siteAccessor;

        private string lastNavigatedPage;

        public BrowserForm()
        {
            InitializeComponent();
        }

        #region IMdiNonChartWindow
        public NonChartWindowSettings.WindowCode WindowCode { get {return NonChartWindowSettings.WindowCode.WebBrowser;} }
        public int WindowInnerTabPageIndex { get; set; }
        private Action<Form> formMoved;
        public event Action<Form> FormMoved
        {
            add { formMoved += value; }
            remove { formMoved -= value; }
        }

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        private Action<Form> resizeEnded;
        public event Action<Form> ResizeEnded
        {
            add { resizeEnded += value; }
            remove { resizeEnded -= value; }
        }
        
        private void BrowserFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }

        private void BrowserFormMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }

        private void BrowserFormLoad(object sender, EventArgs e)
        {
            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });

            // восстановить посещенные страницы
            var urls = UserSettings.Instance.VisitedUrls;
            cbURL.DataSource = UserSettings.Instance.VisitedUrls;

            siteAccessor = new ForexInvestSiteAccessor(webBrowser);
            NavigateOnStart(urls.Count == 0 ? string.Empty : urls[0]);

            Instance = this;
        }

        private void NavigateOnStart(string url)
        {
            if (string.IsNullOrEmpty(url) || 
                url.Equals(siteAccessor.siteUrl, StringComparison.InvariantCultureIgnoreCase))
                Login();
            else
                webBrowser.Navigate(url);
        }

        private static bool RefreshAutentificationValue(out string login, out string token, out long lockTime)
        {
            token = string.Empty;
            lockTime = default(long);
            login = AccountStatus.Instance.Login;
            if (string.IsNullOrEmpty(login)) return false;
            var context = CurrentProtectedContext.Instance.MakeProtectedContext();
            if (context == null) return false;
            token = context.hash;
            lockTime = context.clientLocalTime;
            return true;
        }

        private void BrowserFormFormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;

            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }
        #endregion

        public void OnUserAuthenticated()
        {
            if (lastNavigatedPage != siteAccessor.siteUrl) return;
            Login();
        }

        private void Login()
        {
            string login;
            string token;
            long lockTime;
            if (!RefreshAutentificationValue(out login, out token, out lockTime)) return;
            siteAccessor.LoginOnSite(login, token, lockTime);
        }

        private void btnNavigate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbURL.Text)) return;
            webBrowser.Navigate(cbURL.Text);
        }

        private void UpdateUrlList(string url)
        {
            url = url.Replace("http://", "").ToLower();

            var urls = cbURL.Items.Cast<string>().ToList();
            var contains = urls.Contains(url);
            if (contains) // упорядочить существующий список, чтобы посещенная страница оказалась наверху
                urls.Remove(url);
            urls.Insert(0, url);
            if (urls.Count > MaxUrlsStored)
                urls.RemoveAt(urls.Count - 1);

            UserSettings.Instance.VisitedUrls = urls;
            UserSettings.Instance.SaveSettings();

            cbURL.DataSource = urls;
        }

        private void cbURL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) 
                btnNavigate_Click(sender, EventArgs.Empty);
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var url = webBrowser.Url.ToString();
            cbURL.Text = url;
            lastNavigatedPage = url;
            UpdateUrlList(url);
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            Login();
        }
    }
}
