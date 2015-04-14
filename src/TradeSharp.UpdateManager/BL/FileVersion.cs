using System;

namespace TradeSharp.UpdateManager.BL
{
    class FileVersion
    {
        public string FileName { get; set; }

        public DateTime Date { get; set; }

        public long Length { get; set; }

        public string HashCode { get; set; }

        public FileVersion() {}

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
            //return FileName == serverFile.FileName && Date >= serverFile.Date && Length == serverFile.Length;
            return FileName == serverFile.FileName && HashCode == serverFile.HashCode;
        }

        public string MakeUriFormatPath(string uri)
        {
            return uri + FileName.Replace('\\', '/');
        }
    }
}
