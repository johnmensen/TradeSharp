using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// Управляет платежами. Например, можно отредактировать неопознанные платежи.
    /// </summary>
    public class PaymentTransferModel
    {
        public List<PaymentSystemTransfer> DefinedPaymentTransfer { get; set; }
        public List<PaymentSystemTransfer> UndefinedPaymentTransfer { get; set; }

        public PaymentTransferModel()
        {
            DefinedPaymentTransfer = new List<PaymentSystemTransfer>();
            UndefinedPaymentTransfer = new List<PaymentSystemTransfer>();
        }
    }
}