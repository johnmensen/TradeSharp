using System;

namespace TradeSharp.UI.Util.Update
{
    // copy of UpdateManager.BL.FileVersion
    public class FileVersion
    {
        public string FileName { get; set; }

        public DateTime Date { get; set; }

        public long Length { get; set; }

        public string HashCode { get; set; }

        public FileVersion() { }

        public FileVersion(string fileName)
        {
            FileName = fileName;
        }

        public override string ToString()
        {
            return FileName;
        }

        public bool IsRelevant(FileVersion serverFile)
        {
            return FileName == serverFile.FileName && HashCode == serverFile.HashCode;
        }

        public string MakeUriFormatPath(string uri)
        {
            return uri + FileName.Replace('\\', '/');
        }

        public long MakeHash()
        {
            return Length/2 + Date.Ticks/8 + FileName.GetHashCode();
        }
    }
}
