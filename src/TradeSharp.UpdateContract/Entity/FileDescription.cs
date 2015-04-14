using System;
using System.ServiceModel;

namespace TradeSharp.UpdateContract.Entity
{
    [MessageContract]
    public class FileDescription
    {
        [MessageHeader]
        public string Name { get; set; }

        [MessageHeader]
        public string Path { get; set; }

        [MessageHeader]
        public DateTime TimeUpdated { get; set; }

        [MessageHeader]
        public long Length { get; set; }

        [MessageHeader]
        public SystemName TargetSystem { get; set; }

        [MessageHeader]
        public string TargetSystemString { get; set; }
    }

    [MessageContract]
    public class FileProperties : FileDescription
    {
        [MessageHeader]
        public string HashCode { get; set; }
    }
}
