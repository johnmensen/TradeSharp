using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class SplashMessagesForm : Form
    {
        public AccountEventCode AccountEventCode { get; set; }

        private static Font boldFont;
        
        private static Font regularFont;

        private readonly ThreadSafeQueue<AccountEvent> msgQueue = new ThreadSafeQueue<AccountEvent>();

        private readonly Regex regexTitle = new Regex(@"\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}", RegexOptions.IgnoreCase);

        private int currentLine;

        private const int MaxMessagesCount = 40;


        /// <summary>
        /// Коды тех сообщений, которые были на данный момент в очереди
        /// </summary>
        private readonly List<AccountEventCode> currentMessageCodes = new List<AccountEventCode>();
        
        public SplashMessagesForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
            CreateFonts();
            msgQueue.MaxLength = MaxMessagesCount;

            if (LocalizedResourceManager.Instance != null)
            {
                var icon = (Icon)LocalizedResourceManager.Instance.GetObject("terminal_icon");
                if (icon != null)
                {
                    FormBorderStyle = FormBorderStyle.FixedDialog;
                    Icon = icon;
                }
            }
        }

        public SplashMessagesForm(AccountEvent message)
            : this()
        {
            AppendMessage(message);
        }

        public SplashMessagesForm(List<AccountEvent> messages)
            : this()
        {
            EnqueueMessages(messages);
        }

        void CreateFonts()
        {
            boldFont = new Font(rbMessages.Font, FontStyle.Bold);
            regularFont = new Font(rbMessages.Font, FontStyle.Regular);
        }
        
        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        public void EnqueueMessages(List<AccountEvent> messages)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            foreach (var msg in messages)
                AppendMessage(msg);
        }

        public void AppendMessage(AccountEvent accountEvent)
        {
            msgQueue.InQueue(new AccountEvent(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + ": " + accountEvent.Title, 
                                              accountEvent.Body, 
                                              accountEvent.AccountEventCode), 10000);
        }

        private void SetBoldMessages(string str)
        {
            var currLen = rbMessages.TextLength;
            var startCount = rbMessages.Lines.Length;
            if (rbMessages.TextLength > 0)
                rbMessages.AppendText(Environment.NewLine);
            rbMessages.AppendText(str);
            for (var i = startCount; i < rbMessages.Lines.Length; i++ )
            {
                var line = rbMessages.Lines[i];
                var titleMatches = regexTitle.Matches(line);
                if (titleMatches.Count == 1 && titleMatches[0].Index == 0)
                {
                    rbMessages.Select(currLen, line.Length + 1);
                    rbMessages.SelectionFont = boldFont;
                }
                else
                {
                    rbMessages.Select(currLen, line.Length + 1);
                    rbMessages.SelectionFont = regularFont;
                }
                currLen += line.Length + 1;
            }
            rbMessages.SelectionStart = rbMessages.Text.Length;
            rbMessages.ScrollToCaret();
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            bool timeout;
            var msgList = msgQueue.ExtractAll(1000, out timeout);
            if (timeout || msgList.Count == 0) return;

            currentMessageCodes.AddRange(msgList.Select(m => m.AccountEventCode).Distinct().ToList());

            var str = new StringBuilder();
            foreach (var msg in msgList)
            {
                if (str.Length != 0)
                    str.Append(Environment.NewLine);
                str.AppendLine(msg.Title);
                str.AppendLine(msg.Body);
            }
            SetBoldMessages(str.ToString());
        }

        private void BtnSaveClick(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                using (var sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                {
                    sw.Write(rbMessages.Text.Replace("\n", Environment.NewLine));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localizer.GetString("MessageErrorSavingFile") + ": " + ex.Message,
                                Localizer.GetString("TitleError"), 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrintClick(object sender, EventArgs e)
        {
            var printDoc = new PrintDocument();
            currentLine = 0;
            printDoc.PrintPage += PrintDocOnPrintPage;

            var printDialog = new PrintDialog { Document = printDoc };
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        private void PrintDocOnPrintPage(object sender, PrintPageEventArgs ppeArgs)
        {
            var font = new Font(FontFamily.GenericSansSerif, 12);
            
            var g = ppeArgs.Graphics;
            
            // read margins from PrintPageEventArgs
            float leftMargin = ppeArgs.MarginBounds.Left;
            float topMargin = ppeArgs.MarginBounds.Top;            
            
            // calculate the lines per page on the basis of the height of the page and the height of the font
            var linesPerPage = ppeArgs.MarginBounds.Height / font.GetHeight(g);
            
            // read lines one by one, using StreamReader
            var count = 0;
            for (; currentLine < rbMessages.Lines.Length; currentLine++)
            {
                var line = rbMessages.Lines[currentLine];
                if (count++ >= linesPerPage) break;

                var yPos = topMargin + (count * font.GetHeight(g));                
                g.DrawString(line, font, Brushes.Black, leftMargin, yPos, new StringFormat());                
                count++;
            }            
            ppeArgs.HasMorePages = currentLine < (rbMessages.Lines.Length - 1);
        }

        private void ConfiguringNotificationsClick(object sender, EventArgs e)
        {
            // в строках ниже из AccountEventCode формируется Dictionary из локализованных строк,
            // а затем эти строки обратно преобразуются в AccountEventCode
            var pageState = currentMessageCodes.Distinct().ToDictionary(x => (object) EnumFriendlyName<AccountEventCode>.GetString(x), x =>
                {
                    AccountEventSettings accountEventSettings;
                    try
                    {
                        accountEventSettings = UserSettings.Instance.AccountEventAction.Single(y => y.EventCode == x);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ConfiguringNotificationsClick()", ex);
                        return true;
                    }
                    return accountEventSettings.EventAction != AccountEventAction.DoNothing;
                });

            if (CheckedListBoxDialog.Show(pageState, this, 
                Localizer.GetString("TitleCustomizeNotifications")) != DialogResult.OK) return;

            foreach (var state in pageState)
            {
                AccountEventCode accountEventCode;
                try
                {
                    accountEventCode = EnumItem<AccountEventCode>.items.Single(x => EnumFriendlyName<AccountEventCode>.GetString(x.Value) == (string) state.Key).Value;
                }
                catch (Exception ex)
                {
                    Logger.Error("ConfiguringNotificationsClick()", ex);
                    continue;
                }
                
                var updateAction = UserSettings.Instance.AccountEventAction.FirstOrDefault(a => a.EventCode == accountEventCode);
                if (updateAction == null) continue;
                updateAction.EventAction = state.Value ? AccountEventAction.DefaultAction : AccountEventAction.DoNothing;                
            }
            UserSettings.Instance.SaveSettings();
        }
    }
}