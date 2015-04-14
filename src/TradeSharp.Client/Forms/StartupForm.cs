using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    /// <summary>
    /// Splash-screen
    /// открывает себя в отдельном потоке,
    /// отображает статус
    /// </summary>
    public partial class StartupForm : Form
    {
        private static StartupForm instance;
        public static StartupForm Instance
        {
            get { return instance ?? (instance = new StartupForm()); }
        }

        public static void Reset()
        {
            instance = null;
        }

        /// <summary>
        /// количество этапов загрузки
        /// </summary>
        private int stepsRemained;
        private readonly int totalSteps = Enum.GetValues(typeof(StartupStage)).Length;
        
        public StartupForm()
        {
            InitializeComponent();
            stepsRemained = totalSteps;
            Localizer.LocalizeControl(this);
            // прозрачность меток и background - runtime
            labelTitle.BackColor = Color.Transparent;
            lblLoadStatus.BackColor = Color.Transparent;
            /*using (var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "TradeSharp.Client.splash.png"))
            {
                if (imageStream != null) BackgroundImage = new Bitmap(imageStream);
            }*/
        }

        public void ShowSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new Action(ShowSplashScreen));
                return;
            }
            Show();
            Application.Run(this);
        }

        /// <summary>
        /// Closes the SplashScreen
        /// </summary>
        public void CloseSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new Action(CloseSplashScreen));
                return;
            }
            Close();
        }

        public void SetStatus(StartupStage stage)
        {
            var text = EnumFriendlyName<StartupStage>.GetString(stage);
            SetStatusSafe(text, true);
        }

        public void SetStatus(string text)
        {
            SetStatusSafe(text, false);
        }

        private void SetStatusUnsafe(string text, bool updateProgress)
        {
            lblLoadStatus.Text = text;
            // обновить прогресс
            if (!updateProgress) return;
            stepsRemained--;
            var progress = 100 * (totalSteps - stepsRemained) / totalSteps;
            progressBar.Value = progress;
        }

        private void SetStatusSafe(string text, bool updateProgress)
        {
            try
            {
                Invoke(new Action<string, bool>(SetStatusUnsafe), text, updateProgress);
            }
            catch (InvalidOperationException)
            {
                Thread.Sleep(200);
                try
                {
                    Invoke(new Action<string, bool>(SetStatusUnsafe), text, updateProgress);
                }
                catch
                {
                    return;
                }
            }
        }
    }

    static class SplashScreen
    {
        private static Thread thread;

        public static void ShowSplash(StartupStage stage)
        {
            thread = new Thread(StartupForm.Instance.ShowSplashScreen)
                         {
                             IsBackground = true, 
                             ApartmentState = ApartmentState.STA
                         };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            UpdateState(stage);
        }

        public static void UpdateState(StartupStage stage)
        {
            StartupForm.Instance.SetStatus(stage);
        }

        public static void UpdateState(string text)
        {
            StartupForm.Instance.SetStatus(text);
        }

        public static void CloseSplashScreen()
        {
            StartupForm.Instance.CloseSplashScreen();
            thread.Join();
        }
    }

    public enum StartupStage
    {
        Started = 0,
        MakeInstrumentalPanels = 1,
        MakeQuoteStream = 2,
        MakeServerConnection = 3,
        FillDictionaries = 4,
        LoadingWorkspace = 5,
        PreloadQuotes = 6,
        Finalizing = 7
    }
}
