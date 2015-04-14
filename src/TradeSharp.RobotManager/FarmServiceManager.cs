using System;
using System.ServiceProcess;
using System.Threading;
using TradeSharp.Util;

namespace TradeSharp.RobotManager
{
    /// <summary>
    /// Класс обёртка над ServiceController.
    /// </summary>
    public class FarmServiceManager
    {
        /// <summary>
        /// В параметре передаётся числовое значение из перечисления "ServiceControllerStatus"
        /// </summary>
        public event Action<int> ServiceStateChange;

        /// <summary>
        /// Следует ли постоянно проверять состояние указанной службы
        /// </summary>
        public bool CheckingServiceStatus { get;  set; }

        /// <summary>
        /// Текущий статус службы. Это поле приводится к типу "ServiceControllerStatus"
        /// -1       : произошла ошибка
        /// null     : состояние пока не определено (программа только запустилась)
        /// 0,1,2... : то или иное состояние службы
        /// </summary>
        public int? СurrentStatus { get; private set; }

        /// <summary>
        /// Вспомогательный поток, в котором постоянно отслеживается статус службы (не изменился ли он). 
        /// Если статус изменяется, то генерируется событие ServiceStateChange
        /// </summary>
        private readonly Thread serviceStatusWatcher;

        public void ServiceStart()
        {
            if (СurrentStatus == (int)ServiceControllerStatus.Stopped) ServiceControllerSingleton.Instance.Start();
        }

        public void ServiceStop()
        {
            if (СurrentStatus == (int)ServiceControllerStatus.Running) ServiceControllerSingleton.Instance.Stop();
        }

        public FarmServiceManager(string serviceName)
        {
            ServiceControllerSingleton.ServiceName = serviceName;
            CheckingServiceStatus = true;
            serviceStatusWatcher = new Thread(СheckServiceStatus);
            serviceStatusWatcher.Start();
        }

        /// <summary>
        /// Проверка - изменилось ли состояние (статус) службы
        /// </summary>
        private void СheckServiceStatus()
        {
            while (CheckingServiceStatus)
            {
                Thread.Sleep(500);
                ServiceControllerSingleton.Instance.Refresh();
                
                if (ServiceStateChange != null)  // Если есть подписчики на изменение статуса службы
                {
                    int? status;
                    try
                    {
                        status = (int)ServiceControllerSingleton.Instance.Status;
                    }
                    catch (InvalidOperationException ex)
                    {
                        status = -1;
                        Logger.Error(string.Format("Возможно служба не установлена {0}", ServiceControllerSingleton.ServiceName), ex);
                    }
                    catch (Exception ex)
                    {
                        status = -1;
                        Logger.Error("СheckServiceStatus()", ex);
                    }

                    if (СurrentStatus == status) continue; //Если статус не менялся, тогда ничего не делаем
                    {
                        СurrentStatus = status;
                        ServiceStateChange(status.Value);
                    }
                }
                else
                {
                    // аварийная остановка прсмотра статуса службы - по какой то причине все подписчики отвалились, просмативать состояние не для кого 
                    CheckingServiceStatus = false;
                }
            } 
        }  

        /// <summary>
        /// Возвращает в виде строки текущее состояние службы
        /// </summary>
        public string StatusToString()
        {
            return StatusToString(ServiceControllerSingleton.Instance.Status);
        }

        /// <summary>
        /// Возвращает в виде строки указанное состояние службы
        /// </summary>
        public string StatusToString(ServiceControllerStatus status)
        {
            switch (status)
            {
                // ReSharper disable LocalizableElement
                case ServiceControllerStatus.ContinuePending:
                    return "Ожидается возобновление работы службы.";
                case ServiceControllerStatus.Paused:
                    return "Служба приостановлена.";
                case ServiceControllerStatus.PausePending:
                    return "Ожидается приостановка службы.";
                case ServiceControllerStatus.Running:
                    return "Служба запущена.";
                case ServiceControllerStatus.StartPending:
                    return "Служба запускается...";
                case ServiceControllerStatus.Stopped:
                    return "Служба не запущена.";
                case ServiceControllerStatus.StopPending:
                    return "Служба останавливается...";
                default:
                    return string.Empty;
                // ReSharper restore LocalizableElement
            }
        }
    }
}
