using System;
using System.Windows.Forms;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ChatForm : Form, IMdiNonChartWindow
    {
        public ChatForm()
        {
            InitializeComponent();
        }

        #region IMdiNonChartWindow
        
        public NonChartWindowSettings.WindowCode WindowCode
        {
            get { return NonChartWindowSettings.WindowCode.Chat; }
        }
        
        public int WindowInnerTabPageIndex { get; set; }

        #endregion       

        private void ChatFormLoad(object sender, EventArgs e)
        {
            chatControl.Engine = MainForm.Instance.ChatEngine;
            chatControl.Start();

            // выполнить вход
            if (ChatSettings.Instance.AutoLogin)
            {
                var user = AllUsers.Instance.GetAllUsers().Find(u => u.Login == AccountStatus.Instance.Login);
                if (user != null)
                    chatControl.Login(user.ID);
            }

            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });
        }

        private void ChatFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        protected override void OnClosed(EventArgs e)
        {
            chatControl.StopControl();
            base.OnClosed(e);
        }

        private Action<Form> formMoved;
        public event Action<Form> FormMoved
        {
            add { formMoved += value; }
            remove { formMoved -= value; }
        }

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        private Action<Form> resizeEnded;
        public event Action<Form> ResizeEnded
        {
            add { resizeEnded += value; }
            remove { resizeEnded -= value; }
        }

        private void ChatFormMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }

        private void ChatFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }
    }
}
