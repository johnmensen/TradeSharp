using System;
using System.ServiceProcess;

namespace ServerUnitManager.Contract.Util
{
    [Serializable]
    public class ServiceUnitStatus
    {
        /// <summary>
        /// код службы - ее короткое название на английском
        /// типа Server, SideBridge ...
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// название службы - как оно отображается в списке служб
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// имя файла в списке процессов
        /// </summary>
        public string ServiceProcessName { get; set; }

        /// <summary>
        /// запускается - запущена - останавливается - остановлена
        /// </summary>
        public ServiceControllerStatus Status { get; set; }

        /// <summary>
        /// обновление службы в процессе
        /// </summary>
        public bool IsInUpdateList { get; set; }

        /// <summary>
        /// количество обновленных файлов
        /// </summary>
        public int FilesUpdated { get; set; }

        /// <summary>
        /// количество файлов, которые еще нужно обновить
        /// </summary>
        public int FilesLeft { get; set; }

        /// <summary>
        /// сервисы, которые должны быть запущены перед запуском самой службы
        /// </summary>
        public string[] dependsOn;
    }
}
