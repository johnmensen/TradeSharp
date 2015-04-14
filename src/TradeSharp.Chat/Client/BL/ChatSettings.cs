using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.BL
{
    public class ChatSettings
    {
        #region singleton

        private static readonly Lazy<ChatSettings> instance = new Lazy<ChatSettings>(() => new ChatSettings());

        public static ChatSettings Instance
        {
            get { return instance.Value; }
        }

        private ChatSettings()
        {
            LoadSettings();
        }

        #endregion

        private bool autoLogin = true;

        // флаг автоматического входа в чат при авторизации в терминале
        [PropertyXMLTag("AutoLogin")]
        public bool AutoLogin
        {
            get { return autoLogin; }
            set { autoLogin = value; }
        }

        // Id пользователя, под которым автоматически выполняется выход
        // (вход под этим Id не обязательно должен быть успешно выполнен)
        [PropertyXMLTag("Id")]
        public int Id { get; set; }

        // комнаты, в которые автоматически выполняется вход
        // (вход не обязательно должен быть выполнен успешно)
        private List<string> rooms = new List<string>();

        public List<string> Rooms
        {
            get { return rooms; }
            set { rooms = value; }
        }

        [PropertyXMLTag("Rooms")]
        public string RoomsString
        {
            get { return string.Join(",", rooms); }
            set
            {
                rooms = value.Split(new[] {','}).ToList();
                if (passwords.Count != rooms.Count)
                    passwords = new List<string>(rooms.Count);
            }
        }

        // пароли на комнаты, в которые автоматически выполняется вход
        private List<string> passwords = new List<string>();

        public List<string> Passwords
        {
            get
            {
                FixPasswords();
                return passwords;
            }
            set
            {
                passwords = value;
                FixPasswords();
            }
        }

        [PropertyXMLTag("Passwords")]
        public string PasswordsString
        {
            get { return string.Join(",", Passwords); }
            set { Passwords = value.Split(new[] {','}).ToList(); }
        }

        private Font ownFont = SystemFonts.DefaultFont;
        public Font OwnFont
        {
            get { return ownFont; }
            set { ownFont = value; }
        }

        [PropertyXMLTag("OwnFont")]
        public string OwnFontString
        {
            get { return FontToString(OwnFont); }
            set { OwnFont = StringToFont(value, OwnFont); }
        }

        public Color OwnColor = Color.DarkGreen;

        [PropertyXMLTag("OwnColor")]
        public string OwnColorString
        {
            get { return ColorToString(OwnColor); }
            set { OwnColor = StringToColor(value, OwnColor); }
        }

        private Font ownerFont = SystemFonts.DefaultFont;
        public Font OwnerFont
        {
            get { return ownerFont; }
            set { ownerFont = value; }
        }

        [PropertyXMLTag("OwnerFont")]
        public string OwnerFontString
        {
            get { return FontToString(OwnerFont); }
            set { OwnerFont = StringToFont(value, OwnerFont); }
        }

        public Color OwnerColor = Color.DarkBlue;

        [PropertyXMLTag("OwnerColor")]
        public string OwnerColorString
        {
            get { return ColorToString(OwnerColor); }
            set { OwnerColor = StringToColor(value, OwnerColor); }
        }

        private Font othersFont = SystemFonts.DefaultFont;
        public Font OthersFont
        {
            get { return othersFont; }
            set { othersFont = value; }
        }

        [PropertyXMLTag("OthersFont")]
        public string OthersFontString
        {
            get { return FontToString(OthersFont); }
            set { OthersFont = StringToFont(value, OthersFont); }
        }

        public Color OthersColor = Color.Black;

        [PropertyXMLTag("OthersColor")]
        public string OthersColorString
        {
            get { return ColorToString(OthersColor); }
            set { OthersColor = StringToColor(value, OthersColor); }
        }

        private string dateTimeFormat = "dd.MM.yyyy HH:mm:ss";

        [PropertyXMLTag("DateTimeFormat")]
        public string DateTimeFormat
        {
            get { return dateTimeFormat; }
            set { dateTimeFormat = value; }
        }

        [PropertyXMLTag("ShowNotifications")]
        public bool ShowNotifications { get; set; }

        [PropertyXMLTag("ShowLog")]
        public bool ShowLog { get; set; }

        public void AddRoom(string room, string password = "")
        {
            if (rooms.Contains(room))
                return;
            rooms.Add(room);
            passwords.Add(password);
        }

        public void RemoveRoom(string room)
        {
            var roomIndex = rooms.IndexOf(room);
            if (roomIndex == -1)
                return;
            rooms.RemoveAt(roomIndex);
            passwords.RemoveAt(roomIndex);
        }

        public void UpdatePassword(string room, string password)
        {
            var roomIndex = rooms.IndexOf(room);
            if (roomIndex == -1)
                return;
            passwords.RemoveAt(roomIndex);
            passwords.Insert(roomIndex, password);
        }

        private void FixPasswords()
        {
            if (passwords.Count != rooms.Count)
                passwords = Enumerable.Repeat("", rooms.Count).ToList();
        }

        private static string FontToString(Font font)
        {
            return font == null ? "" : string.Format("{0},{1},{2}", font.FontFamily.Name, font.Size, (int)font.Style);
        }

        private static Font StringToFont(string str, Font defaultFont)
        {
            var words = str.Split(new[] { ',' });
            if (words.Length != 3)
                return defaultFont;
            return new Font(new FontFamily(words[0]), words[1].ToFloatUniform(), (FontStyle) words[2].ToInt());
        }

        private static string ColorToString(Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", color.B, color.G, color.R);
        }

        private static Color StringToColor(string str, Color defaultColor)
        {
            if (str.Length != 6)
                return defaultColor;
            return Color.FromArgb(Convert.ToInt32(str.Substring(4, 2), 16), Convert.ToInt32(str.Substring(2, 2), 16),
                                  Convert.ToInt32(str.Substring(0, 2), 16));
        }

        #region Загрузка - сохранение
        private void LoadSettings()
        {
            var fileName = ExecutablePath.ExecPath + "\\ChatSettings.xml";
            if (!File.Exists(fileName)) return;
            var doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
                if (doc.DocumentElement == null)
                    return;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка загрузки документа настроек \"{0}\": {1}", fileName, ex);
                return;
            }
            try
            {
                PropertyXMLTagAttribute.InitObjectProperties(this, doc.DocumentElement);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка применения документа настроек \"{0}\": {1}", fileName, ex);
            }
        }

        public void SaveSettings()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("settings"));
            PropertyXMLTagAttribute.SaveObjectProperties(this, doc.DocumentElement);
            var fileName = ExecutablePath.ExecPath + "\\ChatSettings.xml";

            try
            {
                using (var sw = new StreamWriter(fileName, false, Encoding.Unicode))
                {
                    using (var xw = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
                    {
                        doc.Save(xw);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения документа настроек \"{0}\": {1}", fileName, ex);
            }
        }
        #endregion
    }
}
