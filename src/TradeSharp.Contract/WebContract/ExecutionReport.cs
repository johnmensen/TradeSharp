using System.Runtime.Serialization;

namespace TradeSharp.Contract.WebContract
{
    [DataContract]
    public class ExecutionReport : HttpParameter
    {
        [DataMember]
        public string Comment { get; set; }

        /// <summary>
        /// TradeSharpServiceProcess.Name
        /// </summary>
        [DataMember]
        public bool IsOk { get; set; }

        public override string ToString()
        {
            return (IsOk ? "OK " : "FAILED ") + Comment;
        }
    }
}
