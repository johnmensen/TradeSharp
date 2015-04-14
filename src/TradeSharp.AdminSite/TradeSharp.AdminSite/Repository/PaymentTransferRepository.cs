using System;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class PaymentTransferRepository : IPaymentTransferRepository
    {
        public PaymentTransferModel GetTransfers()
        {
            var result = new PaymentTransferModel();
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var ps = (from x in ctx.PAYMENT_SYSTEM_TRANSFER
                             where x.Transfer == null
                             select new { transfer = x, 
                                 paySys = x.USER_PAYMENT_SYSTEM != null ? (PaymentSystem)x.USER_PAYMENT_SYSTEM.SystemPayment : PaymentSystem.Unknown,
                                          wallet = x.USER_PAYMENT_SYSTEM != null ? ctx.WALLET.FirstOrDefault(w => w.User == x.USER_PAYMENT_SYSTEM.UserId) : null
                             }).ToList();

                    foreach (var p in ps)
                    {
                        var decorP = LinqToEntity.DecoratePaymentSystemTransfer(p.transfer);
                        decorP.PaymentSys = p.paySys;
                        if (p.wallet != null)
                            decorP.Wallet = LinqToEntity.DecorateWallet(p.wallet);

                        result.UndefinedPaymentTransfer.Add(decorP);
                    }    
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetUnknownTransfers()", ex);
                return null;
            }
            return result;
        }

        public PaymentSystemTransfer GetTransferById(int id)
        {
            PaymentSystemTransfer result;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var ps = (from x in ctx.PAYMENT_SYSTEM_TRANSFER
                             where x.Id == id
                             select new { transfer = x, 
                                 paySys = x.USER_PAYMENT_SYSTEM != null ? (PaymentSystem)x.USER_PAYMENT_SYSTEM.SystemPayment : PaymentSystem.Unknown,
                                 wallet = x.USER_PAYMENT_SYSTEM != null ? ctx.WALLET.FirstOrDefault(w => w.User == x.USER_PAYMENT_SYSTEM.UserId) : null
                             }).Single();


                    result = LinqToEntity.DecoratePaymentSystemTransfer(ps.transfer);
                    result.Wallet = ps.wallet != null ? LinqToEntity.DecorateWallet(ps.wallet) : new Wallet();
                    result.PaymentSys = ps.paySys;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetUnknownTransfers()", ex);
                return null;
            }
            return result;
        }
    }
}