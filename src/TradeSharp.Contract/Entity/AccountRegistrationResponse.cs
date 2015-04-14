using System;
using System.Collections.Generic;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class AccountRegistrationResponse
    {
        public List<string> errors = new List<string>();

        public AccountRegistrationStatus status;

        public string statusName;
    }
}
