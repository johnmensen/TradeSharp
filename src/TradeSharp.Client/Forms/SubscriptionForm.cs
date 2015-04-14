using System;
using System.ComponentModel;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Client.Util;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class SubscriptionForm : Form, IMdiNonChartWindow
    {
        public NonChartWindowSettings.WindowCode WindowCode { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int WindowInnerTabPageIndex
        {
            get
            {
                return (int)Invoke(new Func<object>(() => (int) subscriptionControl.Page));
            }
            set
            {
                SubscriptionControl.ActivePage page;
                try
                {
                    page = (SubscriptionControl.ActivePage) value;
                }
                catch
                {
                    return;
                }
                subscriptionControl.Page = page;
            }
        }

        public SubscriptionForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
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

        public static void EnterRoom(string name, string password)
        {
            MainForm.Instance.EnsureChatWindow();
            MainForm.Instance.ChatEngine.EnterRoom(name, password);
        }

        private void SubscriptionFormLoad(object sender, EventArgs e)
        {
            WindowCode = NonChartWindowSettings.WindowCode.Subscription;
            if (DesignMode) return;
            subscriptionControl.Initialize(Close, MainForm.serverProxyTrade,
                                           s => MainForm.Instance.ShowTradeSignal(false, false, true, s, true),
                                           () => AccountStatus.Instance.AccountData,
                                           () => AccountStatus.Instance.Login,
                                           stat => MainForm.Instance.OpenInvestInPAMMDialog(stat, true),
                                           names =>
                                           {
                                               UserSettings.Instance.PerformersGridColumns = names;
                                               UserSettings.Instance.SaveSettings();
                                           },
                                           () => UserSettings.Instance.PerformersGridColumns,
                                           () => UserSettings.Instance.ActionOnSignal,
                                           signal => { UserSettings.Instance.ActionOnSignal = signal; },
                                           MainForm.Instance.ChatEngine);
            subscriptionControl.EnterRoomRequested += EnterRoom;

            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });
        }

        private void SubscriptionFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        private void SubscriptionFormMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }

        private void SubscriptionFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }
    }
}
