using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Contract;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.Form
{
    public partial class UserInfoForm : System.Windows.Forms.Form
    {
        public UserInfoForm()
        {
            InitializeComponent();
            userInfoFastGrid.CaptionHeight = 0;
            userInfoFastGrid.SelectEnabled = false;
            userInfoFastGrid.Columns.Add(new FastColumn("a", "Название") {ColumnWidth = 80});
            userInfoFastGrid.Columns.Add(new FastColumn("b", "Значение"));
        }

        public void SetUser(User user)
        {
            var values = new List<Cortege2<string, string>>();
            values.Add(new Cortege2<string, string>("ID", user.ID.ToString()));
            values.Add(new Cortege2<string, string>("Логин", user.Login));
            values.Add(new Cortege2<string, string>("Фамилия", user.Surname));
            values.Add(new Cortege2<string, string>("Имя", user.Name));
            values.Add(new Cortege2<string, string>("Отчество", user.Patronym));
            values.Add(new Cortege2<string, string>("Title", user.Title));
            values.Add(new Cortege2<string, string>("О себе", user.Description));
            userInfoFastGrid.DataBind(values);
        }

        public void SetUserInfoEx(UserInfoEx info)
        {
            bigAvatarPanel.BackgroundImage = info.AvatarBig;
            smallAvatarPanel.BackgroundImage = info.AvatarSmall;
            aboutRichTextBox.Text = info.About;
            ContactListUtils.UnpackContacts(info.Contacts, contactsListView);
        }

        public UserInfoEx GetUserInfo()
        {
            var result = new UserInfoEx();
            var rows = userInfoFastGrid.GetRowValues<Cortege2<string, string>>(false).ToList();
            result.Id = rows.FirstOrDefault(pair => pair.a == "ID").b.ToInt(0);
            result.AvatarBig = bigAvatarPanel.BackgroundImage as Bitmap;
            result.AvatarSmall = smallAvatarPanel.BackgroundImage as Bitmap;
            result.About = aboutRichTextBox.Text;
            result.Contacts = ContactListUtils.PackContacts(contactsListView);
            return result;
        }

        public void SetReadOnly(bool readOnly)
        {
            aboutRichTextBox.ReadOnly = readOnly;
            //okButton.Enabled = !readOnly;
            okButton.Visible = !readOnly;
            if (readOnly)
                cancelButton.Text = "Закрыть";
        }

        private Bitmap SelectBitmapFromFile()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return null;
            try
            {
                var stream = dialog.OpenFile();
                using (stream)
                {
                    return new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка чтения " + dialog.FileName + "\n" + ex, "Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return null;
            }
        }

        /*private void BigAvatarPictureBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (!okButton.Enabled)
                return;
            if (e.Button == MouseButtons.Right)
            {
                bigAvatarPictureBox.Image = null;
                return;
            }
            var bmp = SelectBitmapFromFile();
            if(bmp != null)
            bigAvatarPictureBox.Image = bmp;
        }

        private void SmallAvatarPictureBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (!okButton.Enabled)
                return;
            if (e.Button == MouseButtons.Right)
            {
                smallAvatarPictureBox.Image = null;
                return;
            }
            var bmp = SelectBitmapFromFile();
            if (bmp != null)
                smallAvatarPictureBox.Image = bmp;
        }*/

        private void UserInfoFormLoad(object sender, EventArgs e)
        {
            try
            {
                var metadataSettings = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("UserInfoEx");
                if (metadataSettings != null)
                {
                    var bigSize = (int) metadataSettings["BigAvatarMaxSize"];
                    bigAvatarPanel.Width = bigSize;
                    bigAvatarPanel.Height = bigSize;
                    var smallSize = (int) metadataSettings["SmallAvatarMaxSize"];
                    smallAvatarPanel.Width = smallSize;
                    smallAvatarPanel.Height = smallSize;
                }
            }
            catch (Exception ex)
            {
                Logger.Info("UserInfoForm.UserInfoFormLoad: server error", ex);
            }
        }
    }
}
