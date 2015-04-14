using System.Runtime.Serialization;

namespace TradeSharp.Contract.WebContract
{
    /// <summary>
    /// остановить или запустить сервис T#
    /// </summary>
    [DataContract]
    public class TradeSharpServiceStartStop : HttpParameter
    {
        /// <summary>
        /// TradeSharpServiceProcess.Name
        /// </summary>
        [DataMember]
        public string SrvName { get; set; }

        /// <summary>
        /// TradeSharpServiceProcess.Name
        /// </summary>
        [DataMember]
        public bool ShouldStart { get; set; }

        public override string ToString()
        {
            return (ShouldStart ? "start " : "stop ") + SrvName;
        }
    }
}
