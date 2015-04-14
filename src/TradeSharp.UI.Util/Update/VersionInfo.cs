using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using Entity;
using TradeSharp.UpdateContract.Contract;
using TradeSharp.UpdateContract.Entity;

namespace TradeSharp.UI.Util.Update
{
    public class VersionInfo
    {
        public delegate void ErrorMessageDel(string format, params object[] ptrs);

        private readonly ErrorMessageDel onErrorMessage;

        private readonly string serverUrl;

        public VersionInfo(ErrorMessageDel onErrorMessage)
        {
            this.onErrorMessage = onErrorMessage;
            serverUrl = ConfigurationManager.AppSettings.Get("server.path");
            if (string.IsNullOrEmpty(serverUrl))
            {
                if (LocalizedResourceManager.Instance != null)
                    serverUrl = LocalizedResourceManager.Instance.GetString("UpdateServerUrl");
            }
        }

        public Dictionary<string, FileVersion> GetOwnVersionInfo(string path)
        {
            var files = ClientFileBrowser.GetFileVersions(onErrorMessage);
            return files.ToDictionary(f => f.FileName, f => f);
        }

        public Dictionary<string, FileVersion> GetVersionInfoFromUrl()
        {
            try
            {
                var factory = string.IsNullOrEmpty(serverUrl)
                                  ? new ChannelFactory<IUpdateManager>("IUpdateManagerBinding")
                                  : new ChannelFactory<IUpdateManager>("IUpdateManagerBinding",
                                                                       new EndpointAddress(serverUrl));
                var channel = factory.CreateChannel();
                var files = channel.GetFileVersions(SystemName.Terminal);
                return files.ToDictionary(f => f.Path + "\\" + f.Name,
                                          f =>
                                          new FileVersion
                                              {
                                                  FileName = f.Path + "\\" + f.Name,
                                                  Length = f.Length,
                                                  Date = f.TimeUpdated
                                              });
            }
            catch (Exception ex)
            {
                onErrorMessage("Ошибка при получении FileInfo: {0}", ex);
                return null;
            }
        }
    }
}
