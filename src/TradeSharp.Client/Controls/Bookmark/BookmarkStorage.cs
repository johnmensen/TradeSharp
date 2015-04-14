using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Client.BL;

namespace TradeSharp.Client.Controls.Bookmark
{
    class BookmarkStorage
    {
        private static BookmarkStorage instance;

        public static BookmarkStorage Instance
        {
            get { return instance ?? (instance = new BookmarkStorage()); }
        }

        public readonly List<TerminalBookmark> bookmarks = new List<TerminalBookmark>();

        public long CurrentBookmarkId { get; set; }

        private Action<TerminalBookmark> bookmarkDeleted;

        public event Action<TerminalBookmark> BookmarkDeleted
        {
            add { bookmarkDeleted += value; }
            remove { bookmarkDeleted -= value; }
        }

        private bool wasModified;
        public bool WasModified
        {
            private set { wasModified = value; }
            get
            {
                var val = wasModified;
                wasModified = false;
                return val;
            }
        }

        private BookmarkStorage()
        {            
        }

        public void AddBookmark(TerminalBookmark bookmark)
        {
            bookmarks.Add(bookmark);
            WasModified = true;
        }

        public void RemoveBookmark(long id)
        {
            if (bookmarks.Count < 2) return;
            var bm = bookmarks.FirstOrDefault(b => b.Id == id);
            if (bm == null) return;
            bookmarks.Remove(bm);
            WasModified = true;
            if (bookmarkDeleted != null)
                bookmarkDeleted(bm);
        }

        public bool SwapBookmarks(TerminalBookmark a, TerminalBookmark b)
        {
            var indexA = bookmarks.IndexOf(a);
            if (indexA < 0) return false;
            var indexB = bookmarks.IndexOf(b);
            if (indexB < 0) return false;
            if (indexA == indexB) return false;
            bookmarks[indexA] = b;
            bookmarks[indexB] = a;
            WasModified = true;
            return true;
        }

        public void ReadFromConfig()
        {
            bookmarks.Clear();
            bookmarks.AddRange(UserSettings.Instance.TabPages.Select(p => new TerminalBookmark(p)));
            WasModified = false;
        }

        public TerminalBookmark this[long id]
        {
            get 
            { 
                var bm = bookmarks.FirstOrDefault(b => b.Id == id);
                return bm;
            }
        }

        public TerminalBookmark FindBookmark(string title)
        {
            return bookmarks.FirstOrDefault(b => b.Title == title);
        }
    }
}
