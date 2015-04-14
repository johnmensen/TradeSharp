using System;
using System.Linq;
using System.Windows.Forms;
using System.ServiceModel;
using TradeSharp.Chat.Server.BL;

namespace TradeSharp.Chat.StandAloneServer
{
    public partial class ChatServerForm : Form
    {
        private readonly ChatReceiver chat = new ChatReceiver();
        private readonly Timer timer = new Timer();
        private DateTime lastUserChangeTime, lastMessageTime;

        public ChatServerForm(bool monitor = false)
        {
            InitializeComponent();
            var serviceHost = new ServiceHost(chat);
            try
            {
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                serviceHost.Abort();
            }
            //chat.Manager.CreateRoomInternal(new Room { Name = "Главная", IsBound = true});
            FormClosed += delegate
                {
                    chat.Stop();
                    serviceHost.Close();
                };
            if (monitor)
            {
                timer.Interval = 1000;
                timer.Tick += OnTimerTick;
                timer.Start();
            }
        }

        private void OnTimerTick(object obj, EventArgs args)
        {
            var userChanges = chat.Manager.GetUserChangesInternal(lastUserChangeTime);
            foreach (var userChange in userChanges)
            {
                logRichTextBox.Text += userChange + "\n";
            }
            if (userChanges.Count != 0)
                lastUserChangeTime = userChanges.Max(uch => uch.TimeStamp);

            var pendingMessages = chat.Manager.GetMessagesInternal(lastMessageTime);
            foreach (var pendingMessage in pendingMessages)
            {
                logRichTextBox.AppendText(pendingMessage + "\n");
            }
            if (pendingMessages.Count != 0)
                lastMessageTime = pendingMessages.Max(m => m.TimeStamp);
        }

        private void ClearButtonClick(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
        }
    }
}
