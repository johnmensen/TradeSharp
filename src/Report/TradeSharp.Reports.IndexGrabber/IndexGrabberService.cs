using System;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using TradeSharp.Reports.Lib.IndexGrabber;
using TradeSharp.Util;

namespace TradeSharp.Reports.IndexGrabber
{
    public partial class IndexGrabberService : ServiceBase
    {
        private IndexStorage indexStorage;
        private Thread threadGrabbing;
        private volatile bool isStopping;
        private DateTime? lastTimeGrab;
        private ServiceHost hostGrabber;

        public IndexGrabberService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            indexStorage = new IndexStorage(ExecutablePath.ExecPath + "\\data",
                ExecutablePath.ExecPath + "\\grabbers.xml");
            // запустить процесс опроса индексов
            threadGrabbing = new Thread(DoGrab);
            threadGrabbing.Start();
            // сервис
            BL.IndexGrabber.Instance.indexStorage = indexStorage;
            hostGrabber = new ServiceHost(BL.IndexGrabber.Instance);
            //hostGrabber.AddServiceEndpoint(typeof (IIndexGrabber), new NetTcpBinding("OpenNetTcpBinding"), "net.tcp://localhost:55101/IndexGrabber");
            try
            {
                hostGrabber.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка старта хоста WCF", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            isStopping = true;
            hostGrabber.Close();
            threadGrabbing.Join();
        }

        private void DoGrab()
        {
            while (!isStopping)
            {
                Thread.Sleep(300);
                var daysSinceGrab = lastTimeGrab.HasValue ?
                    (DateTime.Now - lastTimeGrab.Value).TotalDays : int.MaxValue;
                if (daysSinceGrab < 1) continue;

                lastTimeGrab = DateTime.Now;
                var updated = indexStorage.UpdateTickers();
                Logger.InfoFormat("Обновление индексов ({0})", updated);
            }
        }
    }
}
