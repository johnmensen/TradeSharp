using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.Bookmark
{
    public partial class BookmarkStripControl : UserControl
    {
        public ImageList ImgList { set; private get; }

        private const int CellPadding = 2;

        private Action<TerminalBookmark, TerminalBookmark> selectedTabChanged;

        public event Action<TerminalBookmark, TerminalBookmark> SelectedTabChanged
        {
            add { selectedTabChanged += value; }
            remove { selectedTabChanged -= value; }
        }

        public TerminalBookmark SelectedBookmark
        {
            get
            {
                if (Controls.Count < 2) return null;

                for (var i = 1; i < Controls.Count; i++)
                {
                    var btn = (BookmarkControl) Controls[i];
                    if (btn.Selected) return btn.Bookmark;                    
                }
                ((BookmarkControl) Controls[1]).Selected = true;
                Controls[1].Invalidate();
                return ((BookmarkControl) Controls[1]).Bookmark;
            }
            set
            {
                for (var i = 1; i < Controls.Count; i++)
                {
                    var btn = (BookmarkControl) Controls[i];
                    var btnSelected = btn.Bookmark.Id == value.Id;
                    if (btnSelected == btn.Selected) continue;
                    btn.Selected = btnSelected;
                    btn.Invalidate();
                }
            }
        }

        public BookmarkStripControl()
        {
            InitializeComponent();
        }

        public void LoadBookmarks()
        {
            var bookmarks = BookmarkStorage.Instance.bookmarks;
            if (bookmarks.Count == 0)
            {
                // создать закладку по-умолчанию
                BookmarkStorage.Instance.AddBookmark(new TerminalBookmark(
                    Localizer.GetString("TitleNewChart")));
                bookmarks = BookmarkStorage.Instance.bookmarks;
            }
            var left = btnMore.Right + CellPadding;

            long selectedId = 0;
            while (Controls.Count > 1)
            {
                if (((BookmarkControl) Controls[1]).Selected)
                    selectedId = ((BookmarkControl) Controls[1]).Bookmark.Id;
                Controls.RemoveAt(Controls.Count - 1);
            }
                
            foreach (var bm in bookmarks)
            {
                var btn = AddBookmarkControl(bm, left);
                left = btn.Right + CellPadding;
                if (selectedId == bm.Id)
                    btn.Selected = true;
            }
        }

        public int BookmarksCount
        {
            get { return Controls.Count - 1; }
        }

        public void DeleteBookmark(TerminalBookmark bm)
        {
            BookmarkStorage.Instance.RemoveBookmark(bm.Id);
            LoadBookmarks();
            GoOnBookmark(BookmarkStorage.Instance.bookmarks[0].Id);
        }

        private BookmarkControl AddBookmarkControl(TerminalBookmark bm, int left)
        {
            var btn = new BookmarkControl
                {
                    Bookmark = bm,
                    ImageList = ImgList,
                    Parent = this,
                    Left = left,
                    ModeEdit = false
                };
            btn.Click += BtnOnClick;
            btn.CalculateSize();
            Controls.Add(btn);
            return btn;
        }

        public TerminalBookmark GetFirstBookmark()
        {
            return ((BookmarkControl) Controls[1]).Bookmark;
        }

        public TerminalBookmark EnsureBookmark(string bookmarkTitle)
        {
            var bookmark = BookmarkStorage.Instance.bookmarks.FirstOrDefault(b => b.Title == bookmarkTitle);
            if (bookmark != null)
                return bookmark;
            bookmark = new TerminalBookmark(bookmarkTitle);
            AddBookmarkControl(bookmark, Controls.Cast<Control>().Max(c => c.Right + CellPadding));
            return bookmark;
        }

        private void BtnOnClick(object sender, EventArgs eventArgs)
        {
            var entering = (BookmarkControl)sender;
            GoOnBookmark(entering);
        }

        private void GoOnBookmark(BookmarkControl entering)
        {
            var leaving = (BookmarkControl)Controls[1];
            
            // перерисовать контролы
            for (var i = 1; i < Controls.Count; i++)
            {
                var btn = (BookmarkControl)Controls[i];
                var isSelected = btn == entering;
                if (btn.Selected != isSelected)
                {
                    btn.Selected = isSelected;
                    if (!btn.Selected)
                        leaving = btn;
                    btn.Invalidate();
                }
            }

            // перейти на "вкладку"
            selectedTabChanged(leaving.Bookmark, entering.Bookmark);
        }

        public void GoOnBookmark(long bookmarkId)
        {
            for (var i = 1; i < Controls.Count; i++)
            {
                if (((BookmarkControl) Controls[i]).Bookmark.Id == bookmarkId)
                {
                    GoOnBookmark((BookmarkControl) Controls[i]);
                    return;
                }
            }            
        }

        private void BtnMoreClick(object sender, EventArgs e)
        {
            var ownTop = PointToScreen(new Point(0, 0));
            var dlg = new BookmarkForm(ImgList) { StartPosition = FormStartPosition.Manual };
            dlg.Location = new Point(ownTop.X, ownTop.Y - dlg.Height);
            dlg.ShowDialog();

            // перезагрузить вкладки
            if (BookmarkStorage.Instance.WasModified)
                LoadBookmarks();
            else
                // заново упорядочить вкладки (возможно изменились настройки их внешности)
                ArrangeBookmarks();
            
            if (dlg.SelectedBookmark == null) return;

            // выделить вкладку
            var leaving = (BookmarkControl) Controls[1];
            for (var i = 1; i < Controls.Count; i++)
            {
                var btn = (BookmarkControl) Controls[i];
                var selected = btn.Bookmark.Id == dlg.SelectedBookmark.Id;
                if (btn.Selected == selected) continue;
                btn.Selected = selected;
                if (!selected)
                    leaving = btn;
                btn.Invalidate();
            }

            // вызвать событие - вкладка изменилась
            selectedTabChanged(leaving.Bookmark, dlg.SelectedBookmark);

            
        }

        private void ArrangeBookmarks()
        {
            var left = btnMore.Right + CellPadding;

            for (var i = 1; i < Controls.Count; i++)
            {
                var bookmark = (BookmarkControl) Controls[i];
                bookmark.Left = left;
                bookmark.Bookmark = BookmarkStorage.Instance[bookmark.Bookmark.Id];
                bookmark.CalculateSize();
                left = bookmark.Right + CellPadding;
                bookmark.Invalidate();
            }
        }

        /// <summary>
        /// моргать, посвистывать и попукивать вкладками
        /// </summary>
        private void TimerInterfaceTick(object sender, EventArgs e)
        {
            foreach (var ctrl in Controls)
            {
                var btn = ctrl as BookmarkControl;
                if (btn == null) continue;
                btn.Blink();
            }
        }           
    
        public void BlinkBookmark(long bookId)
        {
            foreach (var ctrl in Controls)
            {
                var btn = ctrl as BookmarkControl;
                if (btn == null) continue;
                if (btn.Bookmark.Id == bookId)
                {
                    if (!btn.Selected)
                        btn.IsBlinking = true;
                    break;
                }
            }
        }
    }
}
