using System.ServiceModel;

namespace TradeSharp.UpdateContract.Entity
{
    [MessageContract]
    public class DownloadingFile
    {
        [MessageHeader]
        public string FilePath;

        private SystemName systemName;
        [MessageHeader]
        public SystemName SystemName
        {
            get { return systemName; }
            set
            {
                systemName = value;
                if (string.IsNullOrEmpty(SystemNameString))
                    systemName = value;
            }
        }
        
        [MessageHeader]
        public string SystemNameString { get; set; }
    }
}
