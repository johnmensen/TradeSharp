using System.Collections.Generic;
using System.ServiceModel;
using ServerUnitManager.Contract.Util;

namespace ServerUnitManager.Contract
{
    [ServiceContract]
    public interface IServerUnitManager
    {
        [OperationContract(IsOneWay = false)]
        List<ServiceUnitStatus> GetServicesStatus();

        [OperationContract(IsOneWay = false)]
        StartProcessStatus TryStartService(string serviceName);

        [OperationContract(IsOneWay = false)]
        KillProcessStatus TryStopService(string serviceName);

        [OperationContract(IsOneWay = true)]
        void TryUpdateServices(List<string> serviceNames);

        [OperationContract(IsOneWay = true)]
        void UpdateServiceFilesStates(string serviceFolder, int filesUpdated, int filesLeft, bool updateFinished);
    }

    public enum KillProcessStatus
    {
        OK = 0, FailedToKillTask, TaskNotFound, TaskKilled
    }

    public enum StartProcessStatus
    {
        OK = 0, NotFound, IsPending, FailedToStart
    }
}
