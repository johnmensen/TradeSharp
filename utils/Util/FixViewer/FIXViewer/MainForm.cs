using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace FIXViewer
{
    public partial class MainForm : Form
    {
        private Encoding fileEncoding = Encoding.UTF8;

        public MainForm()
        {
            InitializeComponent();
            gridLog.AutoGenerateColumns = false;
            tbSeparator.Text = "\x1";
            gridMessage.AutoGenerateColumns = false;
        }

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void BtnUpdateBaseClick(object sender, EventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
            var dlg = new UpdateBaseForm();
            dlg.ShowDialog();
        }

        private void BtnParseClick(object sender, EventArgs e)
        {            
            // прочитать словари
            var doc = new XmlDocument();
            doc.Load(Dictionary.DicPath);
            var dicTag = FixTag.ReadTags(doc.DocumentElement.GetElementsByTagName(FixTag.PARENT_TAG)[0]);
            var dicMsg = FixTag.ReadTags(doc.DocumentElement.GetElementsByTagName(FixTag.PARENT_MSG)[0]);

            // разбить сообщение по парам тэг-значение
            var fields = new List<FIXField>();
            var msg = tbMsg.Text;
            var optSeparatChar = !string.IsNullOrEmpty(tbSeparator.Text) ? tbSeparator.Text[0] : (char) 1;
            var pairs = msg.Split(new[] { (char)0, optSeparatChar });

            var msgType = "";
            foreach (var pair in pairs)
            {
                var keyval = pair.Split('=');
                if (keyval.Length != 2) continue;

                var field = new FIXField {Tag = keyval[0], Value = keyval[1]};
                if (dicTag.ContainsKey(keyval[0]))
                {
                    var tag = dicTag[keyval[0]];
                    field.Title = tag.Title;
                    field.Description = tag.Description;
                    field.URL = tag.URL;
                }
                fields.Add(field);

                if (keyval[0] == "35") msgType = keyval[1];
            }

            if (dicMsg.ContainsKey(msgType))
            {
                var msgInfo = dicMsg[msgType];
                var field = new FIXField
                                {
                                    Tag = "ТИП",
                                    Title = msgInfo.Title,
                                    URL = msgInfo.URL,
                                    Description = msgInfo.Description,
                                    Value = msgType
                                };
                fields.Add(field);
            }

            gridMessage.DataSource = fields;
        }

        /// <summary>
        /// распарсить лог-файл 
        /// </summary>
        private void BtnLoadClick(object sender, EventArgs e)
        {
            // прочитать словари
            var doc = new XmlDocument();
            doc.Load(Dictionary.DicPath);
            var dicMsg = FixTag.ReadTags(doc.DocumentElement.GetElementsByTagName(FixTag.PARENT_MSG)[0]);

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            var messages = new List<FixMessage>();
            
            using (var fs = new StreamReader(openFileDialog.FileName, fileEncoding))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;
                    var msg = FixMessage.ParseMessage(line, dicMsg);
                    if (msg != null) messages.Add(msg);
                }
            }

            gridLog.DataSource = messages;
        }

        private void GridLogCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // вызвать диалог подробностей
            if (e.RowIndex < 0) return;
            var msg = (FixMessage)gridLog.Rows[e.RowIndex].DataBoundItem;
            tbMsg.Text = msg.sourceMessage;
            tabControl.SelectedIndex = 1;
        }

        private void GridLogCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var msg = (FixMessage)gridLog.Rows[e.RowIndex].DataBoundItem;
            if (msg.Direction == "toApp") e.CellStyle.ForeColor = Color.Green;
            else
                if (msg.Direction == "fromApp") e.CellStyle.ForeColor = Color.DarkRed;
            if (!string.IsNullOrEmpty(msg.Result) && msg.Result != "2")
                e.CellStyle.BackColor = Color.LemonChiffon;
        }

        /// <summary>
        /// показать суммарный подтвержденный объем
        /// </summary>
        private void GridLogSelectionChanged(object sender, EventArgs e)
        {
            var dicVolmBySmb = new Dictionary<string, int>();
            var usedIndicies = new List<int>();

            foreach (DataGridViewCell cell in gridLog.SelectedCells)
            {
                var rowIndex = cell.RowIndex;
                if (usedIndicies.Contains(rowIndex)) continue;
                usedIndicies.Add(rowIndex);
                var row = gridLog.Rows[rowIndex];
                var msg = (FixMessage) row.DataBoundItem;
                if (msg.ResultVolume == 0) continue;
                if (dicVolmBySmb.ContainsKey(msg.ResultSymbol))
                    dicVolmBySmb[msg.ResultSymbol] = dicVolmBySmb[msg.ResultSymbol] + msg.ResultVolume;
                else
                    dicVolmBySmb.Add(msg.ResultSymbol, msg.ResultVolume);
            }

            var sb = new StringBuilder();
            foreach (var pair in dicVolmBySmb)
                sb.AppendFormat("{0}: {1}  ", pair.Key, pair.Value);
            lbVolumeBySmb.Text = sb.ToString();
        }
    }

    public class FIXField
    {
        public string Tag { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
    }

    public class FixMessage
    {
        private static readonly Regex regTime = new Regex(@"\d{4}\-\d{2}\-\d{2} \d{2}\:\d{2}\:\d{2}",
            RegexOptions.IgnoreCase);

        public string sourceMessage;

        /// <summary>
        /// время записи в логе
        /// </summary>
        [DisplayName("Время")]
        public DateTime Time { get; set; }

        /// <summary>
        /// toAdmin, fromApp
        /// </summary>
        [DisplayName("Направление")]
        public string Direction { get; set; }

        /// <summary>
        /// Берется из тега 35 (35=A ...)
        /// </summary>
        [DisplayName("Тип")]
        public string Type { get; set; }

        /// <summary>
        /// Берется из тега 39 (39=2 ...), м.б. пустым
        /// </summary>
        [DisplayName("Результат")]
        public string Result { get; set; }

        public string ResultSymbol { get; set; }

        public int ResultVolume { get; set; }

        public static FixMessage ParseMessage(string logEntry, Dictionary<string, FixTag> msgDic)
        {
            // 2012-04-06 10:58:30,601 [3] INFO  MTS.Live [(null)] - toAdmin(8=FIX.4.49=12935=A34=149=FOREXINVEST
            // 52=20120406-06:58:30.60156=CfhDemoPrices57=6877565098=0108=30141=Y553=FOREXINVEST554=FOREXINVEST
            // 10=123, FIX.4.4:FOREXINVEST->CfhDemoPrices:CHFDN)
            var match = regTime.Match(logEntry);
            if (!match.Success) return null;
            var timeStr = match.Value;
            DateTime time;
            try
            {
                time = DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }

            const string fixPreffix = "(8=FIX";
            var start = logEntry.IndexOf(fixPreffix);
            if (start < 0) return null;

            // получить поле Direction
            var sbDir = new StringBuilder();
            for (var i = start - 1; i > 0; i--)
            {
                if (logEntry[i] == ' ') break;
                sbDir.Insert(0, logEntry[i]);
            }

            // создать объект сообщения
            var msg = new FixMessage {Time = time, Direction = sbDir.ToString()};

            // получить тип
            var typeString = GetMessageFromString(logEntry, start, msgDic, "35=", true);
            if (string.IsNullOrEmpty(typeString)) return null;
            msg.Type = typeString;

            // результат исполнения ордера
            msg.Result = GetMessageFromString(logEntry, start, msgDic, "39=", false);
            if (msg.Result == "2" && msg.Type.Contains("Execution Report"))
            {
                // показать проданный/купленный объем и инструмент
                var smb = GetMessageFromString(logEntry, start, msgDic, "55=", false);
                var sideStr = GetMessageFromString(logEntry, start, msgDic, "54=", false);
                var volumeStr = GetMessageFromString(logEntry, start, msgDic, "38=", false);
                var side = sideStr == "1" ? "BUY" : sideStr == "2" ? "SELL" : sideStr;
                if (!string.IsNullOrEmpty(side) && !string.IsNullOrEmpty(smb) && !string.IsNullOrEmpty(volumeStr))
                {
                    msg.Result = string.Format("{0} {1} {2}: OK", side, volumeStr, smb);
                    msg.ResultSymbol = smb;
                    int volm;
                    int.TryParse(volumeStr, out volm);
                    var sign = sideStr == "1" ? 1 : sideStr == "2" ? -1 : 0;
                    msg.ResultVolume = volm * sign;
                }
            }


            // строка сообщения
            start++;
            const string msgEnd = ", FIX.";
            var end = logEntry.IndexOf(msgEnd, start);
            if (end > 0)
                msg.sourceMessage = logEntry.Substring(start, end - start - 1);            

            return msg;
        }

        private static string GetMessageFromString(string logEntry, int start,
            Dictionary<string, FixTag> msgDic, string tagNameWithEqualsSign, bool useDic)
        {
            string typePreffix = (char)1 + tagNameWithEqualsSign;
            var typeStart = logEntry.IndexOf(typePreffix, start);
            if (typeStart < 0) return null;
            var endIndex = logEntry.IndexOf((char)1, typeStart + typePreffix.Length);
            if (endIndex < 0) return null;
            var typeOfMsg = logEntry.Substring(typeStart + typePreffix.Length,
                endIndex - typeStart - typePreffix.Length);
            if (!useDic) return typeOfMsg;
            FixTag tag;
            msgDic.TryGetValue(typeOfMsg, out tag);
            return tag == null ? typeOfMsg : tag.Title;
        }
    }
}
