using System.Runtime.Serialization;

namespace TradeSharp.Contract.WebContract
{
    /// <summary>
    /// имя службы Windows, ее удобочитаемое название + имя запускаемого файла
    /// 
    /// используется следящим сервисом и администраторским модулем, подключенным к следящему сервису
    /// </summary>
    [DataContract]
    public class TradeSharpServiceProcess : HttpParameter
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string Status { get; set; }

        public override string ToString()
        {
            return Name + ", " + FileName + ", " + Title + ", " + Status;
        }
    }
}
