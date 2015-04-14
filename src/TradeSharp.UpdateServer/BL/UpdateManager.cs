using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using TradeSharp.UpdateContract;
using TradeSharp.UpdateContract.Contract;
using TradeSharp.UpdateContract.Entity;

namespace TradeSharp.UpdateServer.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class UpdateManager : IUpdateManager
    {
        private static UpdateManager instance;

        public static UpdateManager Instance
        {
            get { return instance ?? (instance = new UpdateManager()); }
        }

        private readonly bool verboseLogging;

        private UpdateManager()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("Verbose"))
                if ("True".Equals(ConfigurationManager.AppSettings["Verbose"], StringComparison.OrdinalIgnoreCase))
                    verboseLogging = true;

            var sm = System.Reflection.Assembly.GetEntryAssembly();
            ownFolder = Path.GetDirectoryName(sm.Location);
            UpdateFileInfo();

            ReadFolderBySystemDictionary();
        }
        
        #region Данные

        private List<FileProperties> files = new List<FileProperties>();

        private readonly ReaderWriterLockSlim fileLocker = new ReaderWriterLockSlim();

        private const int LockTimeout = 1000;

        private readonly ThreadSafeTimeStamp lastUpdated = new ThreadSafeTimeStamp();

        private const int UpdateIntervalSecs = 60;

        #endregion

        #region Директории

        private readonly string ownFolder;

        private readonly Dictionary<string, string> folderBySystem =
            new Dictionary<string, string>
                {
                    { SystemName.Terminal.ToString(), "\\terminal" },
                    { SystemName.AdminApp.ToString(), "\\admin" },
                    { SystemName.ManagerApp.ToString(), "\\manager" }                    
                };

        #endregion

        #region IUpdateManager

        public FileData LoadServerFile(DownloadingFile fileDescr)
        {
            var systemName =
                string.IsNullOrEmpty(fileDescr.SystemNameString)
                    ? fileDescr.SystemName.ToString()
                    : fileDescr.SystemNameString;

            if (!folderBySystem.ContainsKey(systemName))
            {
                if (verboseLogging)
                    Logger.DebugFormat("Не найдена директория для {0}",
                            systemName);
                return null;
            }


            var sysFolder = ownFolder + folderBySystem[systemName];
            var fullPath = sysFolder + "\\" + fileDescr.FilePath;
            var data = new FileData(fullPath);

            // попытка получить содержимое файла
            try
            {
                if (!File.Exists(fullPath))
                {
                    if (verboseLogging)
                        Logger.DebugFormat("Ошибка получения файла {0}, system name: {1}",
                            fullPath, systemName);
                    return null;
                }

                var fs = new FileStream(fullPath, FileMode.Open);
                data.FileByteStream = fs;            
            }
            catch (Exception ex)
            {
                Logger.Info("LoadServerFile() - file loading error", ex);
            }
            
            return data;
        }

        public List<FileDescription> GetFileVersions(SystemName systemName)
        {
            return GetFileVersionsString(systemName.ToString());
        }

        public List<FileDescription> GetFileVersionsString(string systemName)
        {
            if (verboseLogging)
                Logger.InfoFormat("Запрошены файлы для \"{0}\"", systemName);

            var secsSinceUpdate = (DateTime.Now - lastUpdated.GetLastHit()).TotalSeconds;
            if (secsSinceUpdate > UpdateIntervalSecs)
                UpdateFileInfo();

            if (!fileLocker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("GetFileVersions() - lock timeout");
                return new List<FileDescription>();
            }

            List<FileDescription> result;

            try
            {
                result = files.Where(f => f.TargetSystemString == systemName).Select(f =>
                                      new FileDescription
                                          {
                                              Name = f.Name,
                                              Path = f.Path,
                                              Length = f.Length,
                                              TimeUpdated = f.TimeUpdated,
                                              TargetSystem = f.TargetSystem,
                                              TargetSystemString = f.TargetSystemString
                                          }).ToList();
            }
            finally 
            {
                fileLocker.ExitReadLock();
            }

            if (result.Count == 0)
                Logger.DebugFormat("Запрос файлов \"{0}\" - не найдены", systemName);

            if (verboseLogging)
                Logger.InfoFormat("Запрошены файлы для \"{0}\" - {1} файлов найдено", systemName, result.Count);

            return result;
        }

        public List<FileProperties> GetFileProperties(SystemName systemName)
        {
            return GetFilePropertiesString(systemName.ToString());
        }

        public List<FileProperties> GetFilePropertiesString(string systemName)
        {
            var secsSinceUpdate = (DateTime.Now - lastUpdated.GetLastHit()).TotalSeconds;
            if (secsSinceUpdate > UpdateIntervalSecs)
                UpdateFileInfo();

            if (!fileLocker.TryEnterReadLock(LockTimeout))
            {
                Logger.Error("GetFileProperties() - lock timeout");
                return new List<FileProperties>();
            }

            List<FileProperties> result;

            try
            {
                result = files.Where(f => f.TargetSystemString == systemName).ToList();
            }
            finally
            {
                fileLocker.ExitReadLock();
            }

            return result;
        }

        #endregion

        #region Прочие методы

        /// <summary>
        /// прочитать инф. по файлам из родной директории
        /// </summary>
        private void UpdateFileInfo()
        {
            var curFiles = new List<FileProperties>();

            foreach (var sys in folderBySystem)
            {
                var path = ownFolder + "\\" + sys.Value.Trim('\\');
                FileReader.LoadFiles(path, path, sys.Key, curFiles);
            }

            // обновить список файлов
            if (!fileLocker.TryEnterWriteLock(LockTimeout))
            {
                Logger.Error("UpdateFileInfo() - lock timeout");
                return;
            }
            try
            {
                files = curFiles;
                lastUpdated.Touch();
            }
            finally
            {
                fileLocker.ExitWriteLock();
            }            
        }

        private void ReadFolderBySystemDictionary()
        {
            var str = ConfigurationManager.AppSettings.Get("folders");
            if (string.IsNullOrEmpty(str)) return;
            var foldersAdded = new List<string>();
            foreach (var part in str.Split(new [] {";"}, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = part.Split(new [] {"="}, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2) continue;
                if (!folderBySystem.ContainsKey(keyValue[0]))
                    folderBySystem.Add(keyValue[0], keyValue[1]);
                else
                    throw new Exception("Дублирующийся ключ " + keyValue[0] + " в записи \"folders\"");
                foldersAdded.Add(keyValue[0]);
            }
            if (verboseLogging && foldersAdded.Count > 0)
                Logger.InfoFormat("Добавлены директории: " + string.Join(", ", foldersAdded));
        }

        #endregion
    }
}
