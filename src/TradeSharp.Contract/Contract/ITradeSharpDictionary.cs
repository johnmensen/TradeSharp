using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    /// <summary>
    /// серверный интерфейс
    /// инкапсулирует методы доступа к метаданным
    /// </summary>
    [ServiceContract]
    public interface ITradeSharpDictionary
    {
        /// <returns>Вернуть всех пользователей - только информация, доступная всем</returns>
        [OperationContract(IsOneWay = false)]
        List<PlatformUser> GetAllPlatformUsers();

        [OperationContract(IsOneWay = false)]
        List<TradeTicker> GetTickers(out long lotByGroupHash);

        [OperationContract(IsOneWay = false)]
        LotByGroupDictionary GetLotByGroup();

        /// <returns>код группы счетов - сессия провайдера</returns>
        [OperationContract(IsOneWay = false)]
        List<AccountGroup> GetAccountGroupsWithSessionInfo();

        /// <param name="dealerCode">код дилера (ассоциированого с ProviderService)</param>
        /// <returns>MQ name - session name</returns>
        [OperationContract(IsOneWay = false)]
        ProviderSession[] GetQueueAndSession(string dealerCode);

        [OperationContract(IsOneWay = false)]
        Dictionary<string, object> GetMetadataByCategory(string catName);

        [OperationContract(IsOneWay = false)]
        Dictionary<string, Dictionary<string, object>> GetAllMetadata();

        [OperationContract(IsOneWay = true)]
        void DeleteMetadataItem(string catName, string paramName);

        [OperationContract(IsOneWay = true)]
        void AddOrReplaceMetadataItem(string catName, string paramName, object ptr);
    }
}