using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Util;
using Message = TradeSharp.Chat.Contract.Message;

namespace TradeSharp.Chat.Client.Control
{
    public partial class ChatMessagingControl : UserControl
    {
        public enum MessageStyle
        {
            None, // do not apply style
            Mine,
            Owner,
            Others,
            Error,
            Notify
        }

        public delegate void MessageEnteredDel(string message);
        public delegate void RoomLeftDel();

        public MessageEnteredDel MessageEnteted;
        public RoomLeftDel ConversationLeft;

        private readonly List<Cortege2<Message, MessageStyle>> messageHistory = new List<Cortege2<Message, MessageStyle>>();

        public ChatMessagingControl()
        {
            InitializeComponent();
        }

        public void AddMessage(Message message, ChatControlBackEnd engine, MessageStyle style = MessageStyle.None)
        {
            messageHistory.Add(new Cortege2<Message, MessageStyle>(message, style));
            PostMessage(message, style, engine);
        }

        public void Enable(bool enabled)
        {
            sendButton.Enabled = enabled;
            //leaveRoomButton.Enabled = enabled;
        }

        public void Reset(ChatControlBackEnd engine)
        {
            allMessagesRichTextBox.Clear();
            foreach (var message in messageHistory)
                PostMessage(message.a, message.b, engine);
        }

        // UI handlers
        private void MyMessageTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == (char)Keys.Return) && !ModifierKeys.HasFlag(Keys.Control) &&
                !ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift))
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void MyMessageTextBoxTextChanged(object sender, EventArgs e)
        {
            sendButton.Enabled = !string.IsNullOrEmpty(myMessageTextBox.Text);
        }

        private void SendButtonClick(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void LeaveRoomButtonClick(object sender, EventArgs e)
        {
            if (ConversationLeft != null)
                ConversationLeft();
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(myMessageTextBox.Text))
                return;
            if (MessageEnteted != null)
                MessageEnteted(myMessageTextBox.Text);
            myMessageTextBox.Clear();
        }

        private void PostMessage(Message message, MessageStyle style, ChatControlBackEnd engine)
        {
            if (style == MessageStyle.Notify && !ChatSettings.Instance.ShowNotifications)
                return;
            var nickName = "";
            var user = AllUsers.Instance.GetUser(message.Sender);
            if (user != null)
            {
                nickName = user.NickName + ": ";
                style = MessageStyle.Others;
            }
            if (message.Sender == ChatSettings.Instance.Id) // detect own
                style = MessageStyle.Mine;
            else // detect owner
            {
                var room = engine.GetActiveRooms().Find(r => r.Name == message.Room);
                if (room != null && message.Sender == room.Owner)
                    style = MessageStyle.Owner;
            }
            var text = "[" + message.TimeStamp.ToString(ChatSettings.Instance.DateTimeFormat) + "] " + nickName + message.Text;
            var length = allMessagesRichTextBox.TextLength;
            allMessagesRichTextBox.AppendText(text + Environment.NewLine);
            if (style == MessageStyle.None)
                return;
            allMessagesRichTextBox.SelectionStart = length;
            allMessagesRichTextBox.SelectionLength = text.Length;
            switch (style)
            {
                case MessageStyle.Mine:
                    allMessagesRichTextBox.SelectionColor = ChatSettings.Instance.OwnColor;
                    allMessagesRichTextBox.SelectionFont = ChatSettings.Instance.OwnFont;
                    break;
                case MessageStyle.Owner:
                    allMessagesRichTextBox.SelectionColor = ChatSettings.Instance.OwnerColor;
                    allMessagesRichTextBox.SelectionFont = ChatSettings.Instance.OwnerFont;
                    break;
                case MessageStyle.Others:
                    allMessagesRichTextBox.SelectionColor = ChatSettings.Instance.OthersColor;
                    allMessagesRichTextBox.SelectionFont = ChatSettings.Instance.OthersFont;
                    break;
                case MessageStyle.Error:
                    allMessagesRichTextBox.SelectionColor = Color.DarkRed;
                    allMessagesRichTextBox.SelectionFont = Font;
                    break;
                case MessageStyle.Notify:
                    allMessagesRichTextBox.SelectionColor = Color.Gray;
                    allMessagesRichTextBox.SelectionFont = Font;
                    break;
            }
            allMessagesRichTextBox.SelectionStart = allMessagesRichTextBox.TextLength;
            allMessagesRichTextBox.ScrollToCaret();
        }
    }
}
