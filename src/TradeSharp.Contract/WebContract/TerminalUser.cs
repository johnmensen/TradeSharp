using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TradeSharp.Contract.WebContract
{
    [DataContract]
    public class TerminalUser : HttpParameter
    {
        [DataMember]
        public string IP { get; set; }

        [DataMember]
        public int Account { get; set; }

        [DataMember]
        public string Login { get; set; }
    }
}
