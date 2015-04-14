using System;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.Bookmark
{
    /// <summary>
    /// "закладка" главного окна - для упорядочивания окон MDI
    /// </summary>
    public class TerminalBookmark
    {
        #region Свойства
        public enum DisplayMode { Текст = 0, Картинка, TeкстПлюсКартинка }

        [PropertyXMLTag("Id")]
        public long Id { get; set; }

        [PropertyXMLTag("TabName")]
        public string Title { get; set; }

        [PropertyXMLTag("ImageIndex")]
        public int ImageIndex { get; set; }

        [PropertyXMLTag("DisplayMode")]
        public DisplayMode BookmarkDisplayMode { get; set; }

        public bool ShouldDrawImage
        {
            get
            {
                return BookmarkDisplayMode == DisplayMode.Картинка ||
                       BookmarkDisplayMode == DisplayMode.TeкстПлюсКартинка;
            }
        }

        public bool ShouldDrawText
        {
            get
            {
                return BookmarkDisplayMode == DisplayMode.Текст ||
                       BookmarkDisplayMode == DisplayMode.TeкстПлюсКартинка;
            }
        }
        #endregion

        public TerminalBookmark()
        {
            ImageIndex = -1;
        }

        public TerminalBookmark(string title)
        {
            ImageIndex = -1;
            Title = title;
            Id = new Random().RandomLong();
        }

        public TerminalBookmark(TerminalBookmark bm)
        {
            Id = bm.Id;
            Title = bm.Title;
            ImageIndex = bm.ImageIndex;
            BookmarkDisplayMode = bm.BookmarkDisplayMode;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
