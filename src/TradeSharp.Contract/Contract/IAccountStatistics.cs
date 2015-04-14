using System.Collections.Generic;
using System.ServiceModel;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Contract.Contract
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IAccountStatistics
    {
        [OperationContract(IsOneWay = false)]
        List<EquityOnTime> GetAccountProfit1000(int accountId);

        [OperationContract(IsOneWay = false)]
        AccountEfficiency GetAccountEfficiency(int accountId);

        [OperationContract(IsOneWay = false)]
        AccountEfficiency GetAccountEfficiencyShort(int accountId, bool needOpenedDeals, bool needClosedDeals);

        [OperationContract(IsOneWay = false)]
        PerformerStat GetPerformerStatBySignalCatId(int signalCatId);

        [OperationContract(IsOneWay = false)]
        List<PerformerStat> GetAllPerformers(bool managersOnly);

        [OperationContract(IsOneWay = false)]
        PerformerStat GetPerformerByAccountId(int accountId);

        [OperationContract(IsOneWay = false)]
        List<PerformerStat> GetAllPerformersWithCriteria(bool managersOnly, string criteria, int count, bool ascending,
            float? marginScore, int serviceTypeMask);

        [OperationContract(IsOneWay = false)]
        List<MarketOrder> GetAccountDeals(int accountId, bool openedDeals);

        [OperationContract(IsOneWay = false)]
        PerformerFormulasAndCriteriasSet GetFormulasAndCriterias();

        /// <summary>
        /// определение трейдеров, значения свойств которых удовлетворяют фильтрам (поиск)
        /// </summary>
        /// <param name="managersOnly"></param>
        /// <param name="filters">Cortege3.a - название свойства, Cortege3.b - выражение для поиска, Cortege3.c - учет регистра в выражении</param>
        /// <param name="count"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        List<PerformerStat> GetPerformersByFilter(bool managersOnly, List<PerformerSearchCriteria> filters, int count);

        // работа с информацией по пользователям с привязкой к файлам
        /// <summary>
        /// чтение полной информации по пользователю
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        UserInfoEx GetUserInfo(int id);

        /// <summary>
        /// запись полной информации по пользователю
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        UserInfoEx SetUserInfo(UserInfoEx info);

        /// <summary>
        /// чтение краткой информации по пользователям (без аватаров и описания)
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        List<UserInfoEx> GetUsersBriefInfo(List<int> users);

        // работа с файлами
        /// <summary>
        /// чтение хеш-кодов содержимого файлов
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        List<string> GetFilesHashCodes(List<string> names);

        /// <summary>
        /// чтение содержимого файлов
        /// </summary>
        [OperationContract(IsOneWay = false)]
        List<byte[]> ReadFiles(List<string> names);

        /// <summary>
        /// запись содержимого файла
        /// </summary>
        [OperationContract(IsOneWay = false)]
        bool WriteFile(string name, byte[] data);

        // работа с портфелями
        /// <summary>
        /// чтение идентификаторов всех портфелей
        /// </summary>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        List<int> GetCompanyTopPortfolios();

        // чтение идентификатора портфелея, которым владеет пользователь
        //[OperationContract(IsOneWay = false)]
        //int GetOwnTopPortfolio(int platformUserId);

        /// <summary>
        /// чтение идентификатора портфеля, на который пользователь подписан
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        int GetSubscribedTopPortfolioId(string userLogin);

        /// <summary>
        /// чтение портфеля, на который пользователь подписан
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        TopPortfolio GetSubscribedTopPortfolio(string userLogin);

        /// <summary>
        /// чтение всех данных по одному портфелю
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userAccountEfficiency"></param>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        TopPortfolio GetTopPortfolio(int id, out AccountEfficiency userAccountEfficiency);
    }
}
