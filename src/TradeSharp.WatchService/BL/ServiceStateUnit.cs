using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    public class ServiceStateUnit
    {
        private readonly ModuleStatusProxy  proxy;

        public ServiceProcessState ReportSeverity
        {
            get;
            set;
        }

        private ServiceStateInfo lastServiceState = new ServiceStateInfo(ServiceProcessState.OK);
        public ServiceStateInfo LastServiceState
        {
            get { return lastServiceState; }
            set { lastServiceState = value; }
        }

        /// <summary>
        /// попадает в отчет
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// позволяет различать UDP-пакеты
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// время последнего обновления, инициализируется в конструкторе
        /// </summary>
        public DateTime LastUpdated
        {
            get;
            set;
        }

        private int updateTimeoutSeconds = 60;
        /// <summary>
        /// если ответа нет N секунд - считать юнит отключенным
        /// </summary>
        public int UpdateTimeoutSeconds
        {
            get { return updateTimeoutSeconds; }
            set { updateTimeoutSeconds = value; }
        }

        /// <summary>
        /// состояние, отправленное в последнем отчете (от дублирования  отчетов)
        /// </summary>
        public ServiceStateInfo LastReportedState { get; set; }

        /// <summary>
        /// время последнего отчета (от дублирования  отчетов)
        /// </summary>
        public DateTime LastReportTime { get; set; }

        private readonly bool udpBinding;
        public bool UdpBinding
        {
            get { return udpBinding; }
        }

        private readonly bool siteBinding;
        public bool SiteBinding
        {
            get { return siteBinding; }
        }

        private readonly string siteUrl;
        private readonly string regExp;

        public ServiceStateUnit(string binding, ServiceProcessState _reportSeverity, string _name, Dictionary<string, string> param)
        {
            udpBinding = binding == "UDP";

            if (binding.ToLower() == "site")
            {
                siteBinding = true;
                if (param.ContainsKey("url")) siteUrl = param["url"];
                if (param.ContainsKey("regexp")) regExp = param["regexp"];
            }
            else
            {
                if (!udpBinding)
                    proxy = new ModuleStatusProxy(binding);
                    //wrapper = new ServiceStateWrapper(binding);
            }
            

            ReportSeverity = _reportSeverity;
            Name = _name;
            LastUpdated = DateTime.Now;
        }

        public ServiceStateInfo GetServiceState()
        {
            if (udpBinding) return LastServiceState;

            if (siteBinding)
            {
                LastServiceState.SetState(RequestToSiteState());
                return LastServiceState;
            }

            try
            {
                LastServiceState.SetState(proxy.GetModuleStatus().State);
                //LastServiceState = wrapper.GetServiceProcessState(););

                return LastServiceState;
            }
            catch (Exception ex)
            {
                LastServiceState = new ServiceStateInfo(ServiceProcessState.Offline, ex.Message, DateTime.Now);
                return LastServiceState;
            }
        }

        /// <summary>
        /// Проверяет доступность сайта с url, указанном в siteUrl
        /// </summary>
        private ServiceProcessState RequestToSiteState()
        {
            try
            {
                var textFromSite = (new WebClient()).DownloadString(siteUrl);
                var defaultRegex = new Regex(regExp);
                var matches = defaultRegex.Matches(textFromSite);
                return matches.Count > 0 ? ServiceProcessState.OK : ServiceProcessState.HasErrors;
            }
            catch (WebException ex)
            {
                Logger.Error(string.Format("Ошибка обращения к сайту {0}", siteUrl), ex);
                return ServiceProcessState.Offline;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Ошибка обращения к сайту {0}", siteUrl), ex);
                return ServiceProcessState.HasErrors;
            }
        }

        public bool ShouldReport()
        {
            if (LastServiceState == null) return false;
            if (ReportSeverity == ServiceProcessState.OK) return false;

            foreach (ServiceProcessState sev in Enum.GetValues(typeof(ServiceProcessState)))
            {
                if (sev == ServiceProcessState.OK) continue;
                if ((ReportSeverity & sev) == sev)
                    if (LastServiceState.State == sev) return true;
            }

            return false;
        }

        public void UpdateLastReportedData()
        {
            LastReportedState = LastServiceState;
            LastReportTime = DateTime.Now;
        }

        public bool HasBeenReported()
        {
            if (LastReportedState == null) return false;
            return LastReportedState.IsOfSameSeverity(LastServiceState);
        }

        public void ResetErrorState()
        {
            proxy.ResetStatus();
        }
    }

    //[Flags]
    //public enum ReportOnSeverity
    //{
    //    ReportNever = 0,
    //    ReportOnOffline = 1,
    //    ReportOnCritical = 2,
    //    ReportOnErrors = 4,
    //    ReportOnWarnings = 8
    //}
}
