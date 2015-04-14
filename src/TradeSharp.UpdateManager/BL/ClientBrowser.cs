using System.Collections.Generic;
using System.IO;
using System.Linq;
using TradeSharp.UpdateContract;
using TradeSharp.UpdateContract.Entity;

namespace TradeSharp.UpdateManager.BL
{
    static class ClientBrowser
    {
        public static readonly string ownPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        public static List<FileVersion> GetFileVersions()
        {
            return GetFileVersions(ownPath, ownPath);
        }

        public static List<FileVersion> CompareWithServer(List<FileVersion> serverFiles,
            List<FileVersion> clientFiles)
        {
            var lackFiles = serverFiles.Where(srvFile => !clientFiles.Any(f => f.IsRelevant(srvFile))).ToList();
            return lackFiles;
        }

        private static List<FileVersion> GetFileVersions(string path, string folder)
        {
            var files = new List<FileProperties>();
            FileReader.LoadFiles(path, folder, SystemName.Terminal.ToString(), files);
            return files.Select(f => new FileVersion
                {
                    FileName = f.Path + "\\" + f.Name,
                    Date = f.TimeUpdated,
                    Length = f.Length,
                    HashCode = f.HashCode
                }).ToList();
        }
    }
}
