using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface ITradeSignalRepository
    {
        string AddSignal(ServiceTradeSignalModel newTradeSignal);
        string EditSignal(ServiceTradeSignalModel oldTradeSignal);
        string DellSignal(int id);
        string Subscribe();
        string UnSubscribe(string userLogin, int serviceId);
    }
}
