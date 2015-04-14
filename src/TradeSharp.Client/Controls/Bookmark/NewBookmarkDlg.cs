using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls.Bookmark
{
    public partial class NewBookmarkDlg : Form
    {
        private readonly ImageList lstIcons;

        public TerminalBookmark Bookmark { get; set; }

        public NewBookmarkDlg()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public NewBookmarkDlg(ImageList lstIcons, TerminalBookmark bookmark) : this()
        {
            this.lstIcons = lstIcons;
            Bookmark = bookmark;
            if (bookmark != null)
                btnOk.Text = Localizer.GetString("TitleEdit");
            btnImage.ImageList = lstIcons;
            cbDisplayStyle.DataSource = EnumItem<TerminalBookmark.DisplayMode>.items;
            cbDisplayStyle.SelectedIndex = 0;

            // прописать свойства переданной вкладки
            if (bookmark == null) return;
            tbTitle.Text = bookmark.Title;
            btnImage.ImageIndex = bookmark.ImageIndex;
            cbDisplayStyle.SelectedIndex = cbDisplayStyle.Items.Cast<EnumItem<TerminalBookmark.DisplayMode>>().ToList().FindIndex(
                m => m.Value == bookmark.BookmarkDisplayMode);
        }

        private void BtnImageClick(object sender, EventArgs e)
        {
            var dlg = new SelectPictureForm(lstIcons);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            btnImage.ImageIndex = dlg.ImageIndex;
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            var displayStyle = ((EnumItem<TerminalBookmark.DisplayMode>)cbDisplayStyle.SelectedItem).Value;

            // валидация
            var errors = new List<string>();
            if (string.IsNullOrEmpty(tbTitle.Text))
                errors.Add(Localizer.GetString("MessageBookmarkTitleNotProvided"));
            if (displayStyle == TerminalBookmark.DisplayMode.Картинка && btnImage.ImageIndex < 0)
                errors.Add(Localizer.GetString("MessageBookmarkImageNotProvided"));

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(", ", errors), 
                    Localizer.GetString("TitleError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // создать новую или отредактировать существующую
            if (Bookmark == null)
            {
                // проверить текст на совпадение
                if (BookmarkStorage.Instance.bookmarks.Any(b => b.Title == tbTitle.Text))
                {
                    if (MessageBox.Show(
                        string.Format(Localizer.GetString("MessageBookmarkAlreadyExistsFmt"),
                            tbTitle.Text),
                            Localizer.GetString("TitleConfirmation"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return;
                }

                Bookmark = new TerminalBookmark(tbTitle.Text)
                    {
                        ImageIndex = btnImage.ImageIndex,
                        BookmarkDisplayMode = displayStyle
                    };
                DialogResult = DialogResult.OK;
                return;
            }

            // редактировать существующую
            Bookmark.Title = tbTitle.Text;
            Bookmark.ImageIndex = btnImage.ImageIndex;
            Bookmark.BookmarkDisplayMode = displayStyle;
            DialogResult = DialogResult.OK;
        }
    }
}
