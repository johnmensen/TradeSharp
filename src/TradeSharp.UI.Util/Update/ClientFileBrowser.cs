using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TradeSharp.UpdateContract;
using TradeSharp.UpdateContract.Entity;

namespace TradeSharp.UI.Util.Update
{
    /// <summary>
    /// получает информацию о версиях файлов терминала в локальном каталоге
    /// </summary>
    public static class ClientFileBrowser
    {
        public static readonly string ownPath = System.Reflection.Assembly.GetEntryAssembly() != null
                                                    ? Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
                                                    : string.Empty;

        static ClientFileBrowser()
        {
            ownPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly() != null ? 
                System.Reflection.Assembly.GetEntryAssembly().Location :
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""));
        }

        public static List<FileVersion> GetFileVersions(VersionInfo.ErrorMessageDel onErrorMessage)
        {
            return GetFileVersions(ownPath, ownPath, onErrorMessage);
        }

        public static List<FileVersion> CompareWithServer(List<FileVersion> serverFiles, List<FileVersion> clientFiles)
        {
            var differentFiles = serverFiles.Where(srvFile => !clientFiles.Any(f => f.IsRelevant(srvFile))).ToList();
            return differentFiles;
        }

        private static List<FileVersion> GetFileVersions(string path, string folder, VersionInfo.ErrorMessageDel onErrorMessage)
        {
            try
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
            catch (Exception ex)
            {
                onErrorMessage("Ошибка при получении FileInfo ({0}{1}): {2}", path, folder, ex);
                return new List<FileVersion>();
            }
        }
    }
}
