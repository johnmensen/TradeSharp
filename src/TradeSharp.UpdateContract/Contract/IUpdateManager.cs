using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.UpdateContract.Entity;

namespace TradeSharp.UpdateContract.Contract
{
    [ServiceContract]
    public interface IUpdateManager
    {
        [OperationContract(IsOneWay = false)]
        FileData LoadServerFile(DownloadingFile fileDescr);

        [OperationContract(IsOneWay = false)]
        List<FileDescription> GetFileVersions(SystemName systemName);

        [OperationContract(IsOneWay = false)]
        List<FileDescription> GetFileVersionsString(string systemName);

        [OperationContract(IsOneWay = false)]
        List<FileProperties> GetFileProperties(SystemName systemName);

        [OperationContract(IsOneWay = false)]
        List<FileProperties> GetFilePropertiesString(string systemName);
    }       
}
