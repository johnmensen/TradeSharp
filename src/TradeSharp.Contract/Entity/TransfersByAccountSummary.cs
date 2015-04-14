using System;
using System.Collections.Generic;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// суммарные пополнения / списания / комиссии / начисления по счету за период времени
    /// </summary>
    [Serializable]
    public class TransfersByAccountSummary
    {
        public enum AccountTransferType
        {
            SignalFeeReceived = 0,
            SignalFeePaid = 1,
            TakenFromAccount = 2,
            PutOnAccount = 3,
            DirectTransfer = 4
        }

        public string Login { get; set; }

        private Dictionary<AccountTransferType, Cortege2<int, decimal>> transfersByType = 
            new Dictionary<AccountTransferType, Cortege2<int, decimal>>();
        
        public Dictionary<AccountTransferType, Cortege2<int, decimal>> TransfersByType
        {
            get { return transfersByType; }
            set { transfersByType = value; }
        }

        public TransfersByAccountSummary()
        {
        }

        public TransfersByAccountSummary(string login)
        {
            Login = login;
        }
    }
}
