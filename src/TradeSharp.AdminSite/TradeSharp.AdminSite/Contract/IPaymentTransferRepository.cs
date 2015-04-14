using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IPaymentTransferRepository
    {
        PaymentTransferModel GetTransfers();
        PaymentSystemTransfer GetTransferById(int id);
    }
}