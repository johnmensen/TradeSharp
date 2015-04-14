using System.ServiceProcess;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber
{
    public partial class NewsGrabber : ServiceBase
    {
        public NewsGrabber()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Запуск сервиса");
            BL.Scheduler.Instance.Start();
        }

        protected override void OnStop()
        {
            Logger.Info("Останов сервиса");
            BL.Scheduler.Instance.Stop();
        }
    }
}
