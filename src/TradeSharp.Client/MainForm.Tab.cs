using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls.Bookmark;
using TradeSharp.Client.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        private List<NonChartWindowSettings> nonChartWindows = new List<NonChartWindowSettings>();

        private void BookmarkSelected(TerminalBookmark leaving, TerminalBookmark entering)
        {
            BookmarkStorage.Instance.CurrentBookmarkId = entering.Id;
            // сохранить положиение и размеры окошек той вкладки, с которой уходим
            SaveNonChartWindowsPlacement(leaving);
            // показать окошки для вкладки
            ShowBookmarkChildren(entering);            
        }

        private void ShowFirstBookmarkChildren()
        {
            var bookmark = bookmarkStrip.GetFirstBookmark();
            BookmarkStorage.Instance.CurrentBookmarkId = bookmark.Id;
            Charts.ForEach(c =>
            {
                if (c.bookmarkId == bookmark.Id) c.SavePlacement();
            });
            ShowBookmarkChildren(bookmark);
        }

        private void ShowBookmarkChildren(TerminalBookmark bookmark)
        {
            bookmarkStrip.SelectedBookmark = bookmark;

            var tabTag = bookmark.Id;
            // сохранить все окна графиков
            foreach (var child in Charts.Where(child => child.bookmarkId != tabTag))
            {
                child.SavePlacement();
                child.Hide();
            }
            
            // показать графики вкладки
            Charts.ForEach(c =>
                {                    
                    try
                    {
                        if (c.bookmarkId == tabTag)
                            c.Show();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Переключение на вкладку {0}: {1}",
                            tabTag, ex);
                        Logger.ErrorFormat("Window title is {0}", c.Text);
                        Logger.ErrorFormat("Ticker is {0}", c.chart.Symbol);
                        throw;
                    }
                });

            Charts.ForEach(c =>
                {
                    if (c.bookmarkId == tabTag) c.RestorePlacement();
                });

            // убрать окна Счет, Доходность, Чат и прочие, если их нет на свежеоткрытой вкладке
            foreach (var child in MdiChildren.Where(c => c is IMdiNonChartWindow))
            {
                var windowCode = ((IMdiNonChartWindow)child).WindowCode;
                if (!nonChartWindows.Any(w => w.Window == windowCode && w.ChartTab == tabTag))
                    child.Close();
            }
            
            // показать окна (Счет, Доходность, Чат ...) текущей вкладки и установить им размеры
            foreach (var wind in nonChartWindows.Where(w => w.ChartTab == tabTag))
            {
                EnsureShowAndPlaceNonChartWindow(wind);
            }

            MenuItemVerticalClick(this, EventArgs.Empty);
        }

        /// <summary>
        /// сохранить размеры окошка, координаты и его текущую вкладку
        /// </summary>
        private void SaveNonChartWindowsPlacement(TerminalBookmark leaving)
        {
            foreach (var child in MdiChildren.Where(c => c is IMdiNonChartWindow))
            {
                var windowCode = ((IMdiNonChartWindow)child).WindowCode;
                var wnd = nonChartWindows.FirstOrDefault(w => w.Window == windowCode
                                                                && ((leaving == null) || (w.ChartTab == leaving.Id)));
                if (wnd == null) continue;
                // размеры и координаты
                try
                {
                    wnd.WindowPos = child.Location;
                    wnd.WindowSize = child.Size;
                    wnd.WindowState = child.WindowState.ToString();   
                    wnd.CurrentTabIndex = ((IMdiNonChartWindow)child).WindowInnerTabPageIndex;
                }
                catch (ObjectDisposedException)
                {
                    continue;
                }
            }
        }

        private void EnsureShowAndPlaceNonChartWindow(NonChartWindowSettings wndSets)
        {
            var wnd = MdiChildren.FirstOrDefault(c => c is IMdiNonChartWindow &&
                                                      ((IMdiNonChartWindow)c).WindowCode == wndSets.Window);
            if (wnd == null)
            {
                // создать окно
                if (wndSets.Window == NonChartWindowSettings.WindowCode.Account)
                    wnd = new AccountMonitorForm { MdiParent = this };

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.RobotTest)
                {
                    wnd = new RoboTesterForm();
                    ((RoboTesterForm)wnd).OnRobotResultsBoundToCharts += OnRobotResultsBoundToCharts;
                }

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.Profit)
                    wnd = new AccountTradeResultsForm { InstantCalculation = false };

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.Chat)
                    wnd = new ChatForm();

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.Quotes)
                    wnd = new QuoteTableForm();

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.Subscription)
                    wnd = new SubscriptionForm();

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.WebBrowser)
                    wnd = new BrowserForm();

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.RiskForm)
                    wnd = new RiskSetupForm();

                else if (wndSets.Window == NonChartWindowSettings.WindowCode.WalletForm)
                    wnd = new WalletForm();
                
                SetupNonMdiForm((IMdiNonChartWindow)wnd);
                wnd.Show();
            }
            else
            {
                wnd.Focus();
            }
            
            // установить окну положенные размеры
            wnd.Location = wndSets.WindowPos;
            wnd.Size = wndSets.WindowSize;
            wnd.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), wndSets.WindowState);
            ((IMdiNonChartWindow) wnd).WindowInnerTabPageIndex = wndSets.CurrentTabIndex;
        }

        /// <summary>
        /// вызывается окном через MainForm.Instance
        /// при открытии окна (счет, профит...) для текущей вкладки добавляется запись этого окошка
        /// </summary>
        public void AddNonChartWindowSets(NonChartWindowSettings wndSets)
        {
            wndSets.ChartTab = bookmarkStrip.SelectedBookmark.Id;
            if (!nonChartWindows.Any(wnd => wnd.ChartTab == wndSets.ChartTab && wnd.Window == wndSets.Window))
                nonChartWindows.Add(wndSets);
        }

        /// <summary>
        /// вызывается окном через MainForm.Instance при закрытии окна
        /// </summary>
        public void RemoveNonChartWindowSets(NonChartWindowSettings.WindowCode wndCode)
        {
            var curTabId = bookmarkStrip.SelectedBookmark.Id;
            nonChartWindows.RemoveAll(wnd => wnd.ChartTab == curTabId
                && wnd.Window == wndCode);
            
            // закрыть опустевшую вкладку?
            CheckEmptyTabOnChartClosing(curTabId, false);
        }

        /// <summary>
        /// заранее, при загрузке рабочего пространства, создать все вкладки (TabPage)
        /// </summary>
        private void EnsureTabPages()
        {
            BookmarkStorage.Instance.ReadFromConfig();
            bookmarkStrip.LoadBookmarks();
        }
    
        /// <summary>
        /// перейти на определенную вкладку
        /// </summary>
        private void GoOnBookmark(long bookmarkId)
        {
            bookmarkStrip.GoOnBookmark(bookmarkId);
        }

        /// <summary>
        /// удалить графики, оставшиеся без вкладки
        /// </summary>
        private void InstanceOnBookmarkDeleted(TerminalBookmark terminalBookmark)
        {
            // закрыть графики - остальные окна не надо уничтожать, и так закроются
            foreach (var chart in Charts)
            {
                if (chart.bookmarkId != terminalBookmark.Id) continue;
                chart.Close();
            }
        }
    
        public void CheckEmptyTabOnChartClosing(long bookmarkId, bool calledFromChartClosingHandler)
        {
            var bookmark = BookmarkStorage.Instance[bookmarkId];
            if (bookmark == null) return;

            // если осталась одна вкладка - даже не пытаемся закрыть
            if (bookmarkStrip.BookmarksCount == 1) return;
            var hasCharts =
                calledFromChartClosingHandler
                    ? Charts.Count(c => c.bookmarkId == bookmarkId) > 1
                    : Charts.Count(c => c.bookmarkId == bookmarkId) > 0;
            if (hasCharts) return;

            // мб остались прочие окна (не графики)?
            if (nonChartWindows.Count > 0) return;

            // спросить пользователя
            if (MessageBox.Show("Удалить вкладку \"" + bookmark.Title + "\"?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            // таки удалить вкладку
            bookmarkStrip.DeleteBookmark(bookmark);
        }
    
        /// <summary>
        /// предложить перенести окно в другую вкладку
        /// </summary>
        private void MoveChildToAnotherTab(Form child)
        {
            if (BookmarkStorage.Instance.bookmarks.Count < 2) return;

            var windowBookmarkId = BookmarkStorage.Instance.CurrentBookmarkId;

            TerminalBookmark bookmark;
            var options = BookmarkStorage.Instance.bookmarks.Where(b => b.Id != windowBookmarkId).Cast<object>().ToList();
            if (options.Count > 1)
            {

                object selected;
                string inputText;

                if (!TradeSharp.Util.Dialogs.ShowComboDialog("Перенести окно", options, out selected, out inputText, true))
                    return;
                bookmark = (TerminalBookmark) selected;
            }
            else
                bookmark = (TerminalBookmark) options[0];
            
            // переместить окно на указанную закладку
            if (child is ChartForm)
                ((ChartForm)child).bookmarkId = bookmark.Id;
            else
            {
                // убрать с текущей вкладки...
                var nonChartWnd = (IMdiNonChartWindow) child;
                var oldTabSets = nonChartWindows.FirstOrDefault(w => w.Window == nonChartWnd.WindowCode
                    && w.ChartTab == BookmarkStorage.Instance.CurrentBookmarkId);
                if (oldTabSets != null)
                    nonChartWindows.Remove(oldTabSets);
                // переместить на новую вкладку
                var newTabSets = nonChartWindows.FirstOrDefault(w => w.Window == nonChartWnd.WindowCode
                    && w.ChartTab == bookmark.Id);
                if (newTabSets == null)
                    nonChartWindows.Add(new NonChartWindowSettings
                        {
                            ChartTab = bookmark.Id,
                            Window = nonChartWnd.WindowCode,
                            CurrentTabIndex = nonChartWnd.WindowInnerTabPageIndex,
                            WindowSize = child.Size
                        });
            }
            
            // перейти на закладку
            GoOnBookmark(bookmark.Id);

            // выровнять окошко
            child.Location = new Point(5, 5);
            child.BringToFront();
        }
    }
}