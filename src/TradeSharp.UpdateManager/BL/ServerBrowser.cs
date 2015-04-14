using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using TradeSharp.UpdateContract.Contract;

namespace TradeSharp.UpdateManager.BL
{
    static class ServerBrowser
    {
        public static List<FileVersion> GetFileVersions(string systemName)
        {
            try
            {
                var factory = new ChannelFactory<IUpdateManager>("IUpdateManagerBinding");
                var channel = factory.CreateChannel();
                var files = channel.GetFilePropertiesString(systemName);
                var versions = files.Select(f => new FileVersion
                                                     {
                                                         FileName = f.Path + "\\" + f.Name,
                                                         Date = f.TimeUpdated,
                                                         Length = f.Length,
                                                         HashCode = f.HashCode
                                                     }).ToList();
                return versions;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Ошибка получения версий файлов на сервере", ex);
                return new List<FileVersion>();
            }
        }
    }
}
