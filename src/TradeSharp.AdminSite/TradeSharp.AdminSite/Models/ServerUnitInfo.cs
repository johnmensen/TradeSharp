using TradeSharp.Contract.Entity;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Модель для представления "ServerUnitList.cshtml" состоит из списка экземпляров этого класса
    /// </summary>
    public class ServerUnitInfo
    {
        public string UnitName { get; set; }

        public ServiceProcessState ModuleStatus { get; set; }
        
        public string ExtendedStatusInfo { get; set; }
    }    
}