using System;
using System.ServiceProcess;

namespace TradeSharp.RobotManager
{
    /// <summary>
    /// Класс для взаимодействия со службой
    /// </summary>
    public static class ServiceControllerSingleton
    {
        /// <summary>
        /// Имя текущей службы
        /// </summary>
        public static string ServiceName { get; set; }

        private static readonly Lazy<ServiceController> instance =
            new Lazy<ServiceController>(() => new ServiceController(ServiceName));

        public static ServiceController Instance
        {
            get { return instance.Value; }
        }
    }
}
