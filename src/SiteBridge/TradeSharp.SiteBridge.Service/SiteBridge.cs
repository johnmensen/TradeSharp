using System;
using System.ServiceModel;
using System.ServiceProcess;
using TradeSharp.SiteBridge.Lib.Finance;
using TradeSharp.SiteBridge.Lib.Quotes;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Service
{
    public partial class SiteBridge : ServiceBase
    {
        private AccountEfficiencyCache cache;
        private ServiceHost hostServerStat;

        public SiteBridge()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var dailyQuoteStorage = new DailyQuoteStorage();
                var equityCurveCalculator = new EquityCurveCalculator();
                cache = new AccountEfficiencyCache(new EfficiencyCalculator(dailyQuoteStorage, equityCurveCalculator), dailyQuoteStorage);
                cache.Start();
                hostServerStat = new ServiceHost(cache);
                hostServerStat.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStart", ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                hostServerStat.Close();
                cache.Stop();
                Logger.Info("Сервис остановлен");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в OnStop (AccountEfficiencyCache)", ex);
            }
        }
    }
}
