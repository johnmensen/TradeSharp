using System;
using System.ServiceModel;

namespace TradeSharp.UpdateContract.Entity
{
    [MessageContract]
    public class FileData : IDisposable
    {
        public FileData() {}

        public FileData(string filePath)
        {
            FilePath = filePath;
        }

        [MessageHeader]
        public string FilePath { get; set; }

        [MessageBodyMember]
        public System.IO.Stream FileByteStream { get; set; }

        public void Dispose()
        {
            if (FileByteStream != null)
            {
                FileByteStream.Close();
                FileByteStream = null;
            }
        }
    }
}
