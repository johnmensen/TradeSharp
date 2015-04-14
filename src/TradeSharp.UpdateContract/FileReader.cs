using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TradeSharp.UpdateContract.Entity;

namespace TradeSharp.UpdateContract
{
    public static class FileReader
    {
        public static void LoadFiles(string systemFolder, string folder, string systemName, List<FileProperties> curFiles)
        {
            if (!Directory.Exists(folder))
                return;
            var md5 = MD5.Create();
            foreach (var fileName in Directory.GetFiles(folder))
            {
                var fileInfo = new FileInfo(fileName);
                var fileProperties = new FileProperties
                {
// ReSharper disable PossibleNullReferenceException
                    Name = Path.GetFileName(fileName).ToLower(),
// ReSharper restore PossibleNullReferenceException
                    Path = folder.StartsWith(systemFolder) ? folder.Substring(systemFolder.Length).ToLower() : folder,
                    TargetSystemString = systemName,
                    TimeUpdated = fileInfo.LastWriteTime,
                    Length = fileInfo.Length,
                };
                var systemNameTyped = SystemNameParser.ParseSystemName(systemName);
                if (systemNameTyped.HasValue)
                    fileProperties.TargetSystem = systemNameTyped.Value;

                try
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        var hash = md5.ComputeHash(stream);
                        var sb = new StringBuilder();
                        for (var i = 0; i < hash.Length; i++)
                            sb.Append(hash[i].ToString("x2"));
                        fileProperties.HashCode = sb.ToString();
                    }
                    curFiles.Add(fileProperties);
                }
                catch
                {
                    // различные ошибки чтения файла
                }
            }
            // зайти внутрь
            foreach (var dir in Directory.GetDirectories(folder))
            {
                LoadFiles(systemFolder, dir, systemName, curFiles);
            }
        }
    }
}
