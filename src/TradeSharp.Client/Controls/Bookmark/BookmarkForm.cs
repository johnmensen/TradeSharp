using System;
using System.Drawing;
using System.Windows.Forms;
using TradeSharp.Client.Controls.QuoteTradeControls;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.Bookmark
{
    public partial class BookmarkForm : Form
    {
        public TerminalBookmark SelectedBookmark { get; private set; }
        
        private readonly ImageList imgList;

        public BookmarkForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public BookmarkForm(ImageList imgList) : this()
        {
            this.imgList = imgList;
        }

        private void BookmarkFormLoad(object sender, EventArgs e)
        {
            LoadBookmarks();
        }

        private void LoadBookmarks()
        {
            panelContent.Controls.Clear();
            SelectedBookmark = null;

            foreach (var bm in BookmarkStorage.Instance.bookmarks)
            {
                var ctrl = new BookmarkControl
                    {
                        Bookmark = bm,
                        ImageList = imgList,
                        Parent = panelContent,
                        AllowDrop = true,
                        ContextMenuStrip = contextMenu
                    };
                ctrl.DragEnter += CellDragEnter;
                ctrl.DragDrop += CellDragDrop;
                ctrl.DragOver += CellDragOver;
                ctrl.MouseDown += CellMouseDown;
                ctrl.Clicked += CtrlOnClicked;
                ctrl.DragLeave += CellDragLeave;

                ctrl.CloseClicked += CtrlOnCloseClicked;
                ctrl.CalculateSize();
                panelContent.Controls.Add(ctrl);
            }

            ArrangeBookmarks(false);
            // выделить первую закладку
            SelectedBookmark = ((BookmarkControl)panelContent.Controls[0]).Bookmark;
        }

        private void CtrlOnCloseClicked(object sender, EventArgs e)
        {
            var bookmarksCount = panelContent.Controls.Count;
            if (bookmarksCount == 1) return; // нельзя удалить последнюю вкладку

            // удалить закладку
            var bm = (BookmarkControl) sender;
            BookmarkStorage.Instance.RemoveBookmark(bm.Bookmark.Id);
            LoadBookmarks();
        }

        private void ArrangeBookmarks(bool recalculateSizes)
        {
            var top = 0;
            var left = 0;

            if (recalculateSizes)
                foreach (BookmarkControl ctrl in panelContent.Controls)
                    ctrl.CalculateSize();                

            foreach (Control ctrl in panelContent.Controls)
            {
                var right = left + ctrl.Width;
                if (right > panelContent.Width)
                {
                    left = 0;
                    top += (ctrl.Height + 1);
                }
                ctrl.Location = new Point(left, top);
                left += (ctrl.Width + 1);

                if (recalculateSizes)
                    ctrl.Invalidate();
            }
        }

        /// <summary>
        /// открыть добавления вкладки - прописать текст и выбрать картинку
        /// </summary>        
        private void BtnAddClick(object sender, EventArgs e)
        {
            var dlg = new NewBookmarkDlg(imgList, null);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            var bookmark = dlg.Bookmark;
            
            // добавить закладку в список
            BookmarkStorage.Instance.AddBookmark(bookmark);
            
            // добавить и выстроить закладки
            LoadBookmarks();
        }

        #region MouseHover
        private void CellDragLeave(object sender, EventArgs e)
        {
            var cell = (BookmarkControl)sender;
            if (cell.DragState == DragDropState.InFrame)
                cell.DragState = DragDropState.Normal;
        }

        private void CellDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }

        private void CellDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(BookmarkControl)))
            {
                var ctrlSrc = (BookmarkControl)e.Data.GetData(typeof(BookmarkControl));
                var ctrlTarget = sender as BookmarkControl;
                if (ctrlTarget == null) return;
                ctrlTarget.DragState = DragDropState.Normal;
                
                // если дропа не было - передать обратно контролу событие MouseUp
                if (ctrlTarget == ctrlSrc)
                {
                    var ptMouse = ctrlTarget.PointToClient(new Point(e.X, e.Y));
                    ctrlSrc.FireMouseUp(new MouseEventArgs(MouseButtons.Left, 1,
                        ptMouse.X, ptMouse.Y, 0));
                    return;
                }
                
                // поменять местами ячейки
                if (!BookmarkStorage.Instance.SwapBookmarks(ctrlSrc.Bookmark, ctrlTarget.Bookmark)) return;
                LoadBookmarks();
            }
        }

        private void CellMouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var cell = (BookmarkControl)sender;
            if (cell == null) return;

            if (e.Button == MouseButtons.Left)
            {
                cell.DragState = DragDropState.Washy;
                panelContent.DoDragDrop(sender, DragDropEffects.Link);
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                contextMenu.Tag = cell;
                contextMenu.Show(e.X, e.Y);
                //return;
            }
        }

        private void CellDragOver(object sender, DragEventArgs e)
        {
            var cell = (BookmarkControl)sender;
            if (cell.DragState != DragDropState.Washy)
                cell.DragState = DragDropState.InFrame;
        }

        #endregion

        private void CtrlOnClicked(object sender, EventArgs eventArgs)
        {
            SelectedBookmark = ((BookmarkControl)sender).Bookmark;
            DialogResult = DialogResult.OK;
        }

        private void MenuitemEditClick(object sender, EventArgs e)
        {
            var cell = (BookmarkControl) contextMenu.Tag;
            var dlg = new NewBookmarkDlg(imgList, cell.Bookmark);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            cell.Bookmark = dlg.Bookmark;
            // выстроить ячейки
            ArrangeBookmarks(true);
        }
    }
}
