using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Distribution
{
    public class UserInfoExCache
    {
        private readonly IAccountStatistics dataSource;
        private readonly string path;

        public UserInfoExCache(IAccountStatistics source, string cachePath = "\\files")
        {
            dataSource = source;
            path = ExecutablePath.ExecPath + cachePath;
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("UserInfoExCache: error creating directory {0}", path), ex);
                throw;
            }
        }

        // запрос информации о пользователе
        public UserInfoEx GetUserInfo(int userId)
        {
            var usersInfo = GetUsersInfo(new List<int> {userId});
            if (usersInfo == null || usersInfo.Count == 0)
                return null;
            return usersInfo[0];
        }

        // массовый запрос на информацию о пользователях
        public List<UserInfoEx> GetUsersInfo(List<int> userIds)
        {
            var result = new List<UserInfoEx>();

            if (dataSource == null || userIds == null)
            {
                Logger.Info("UserInfoExCache.GetUsersInfo: server error");
                return null;
            }
            List<UserInfoEx> usersInfo;
            try
            {
                usersInfo = dataSource.GetUsersBriefInfo(userIds);
            }
            catch (Exception ex)
            {
                Logger.Info("UserInfoExCache.GetUsersInfo: {0}", ex);
                return null;
            }
            if (usersInfo == null || usersInfo.Count != userIds.Count)
            {
                Logger.Info("UserInfoExCache.GetUsersInfo: server error");
                return null;
            }
            var requestNames = new List<string>();
            var requestHashCodes = new List<string>();
            foreach (var userInfo in usersInfo)
            {
                result.Add(userInfo);
                if (userInfo == null)
                    continue;
                if (!string.IsNullOrEmpty(userInfo.AvatarBigFileName))
                {
                    requestNames.Add(userInfo.AvatarBigFileName);
                    requestHashCodes.Add(userInfo.AvatarBigHashCode);
                }
                if (!string.IsNullOrEmpty(userInfo.AvatarSmallFileName))
                {
                    requestNames.Add(userInfo.AvatarSmallFileName);
                    requestHashCodes.Add(userInfo.AvatarSmallHashCode);
                }
                if (!string.IsNullOrEmpty(userInfo.AboutFileName))
                {
                    requestNames.Add(userInfo.AboutFileName);
                    requestHashCodes.Add(userInfo.AboutHashCode);
                }
            }
            var files = GetFiles(requestNames, requestHashCodes);
            if (files == null)
                return null;
            for (var i = 0; i < requestNames.Count; i++)
            {
                var name = requestNames[i];
                var data = files[i];
                foreach (var info in usersInfo.Where(ui => ui != null && !string.IsNullOrEmpty(ui.AvatarBigFileName) && ui.AvatarBigFileName == name))
                    info.AvatarBigData = data;
                foreach (var info in usersInfo.Where(ui => ui != null && !string.IsNullOrEmpty(ui.AvatarSmallFileName) && ui.AvatarSmallFileName == name))
                    info.AvatarSmallData = data;
                foreach (var info in usersInfo.Where(ui => ui != null && !string.IsNullOrEmpty(ui.AboutFileName) && ui.AboutFileName == name))
                    info.AboutData = data;
            }
            return result;
        }

        // изменение информации о пользователе
        public UserInfoEx SetUserInfo(UserInfoEx info)
        {
            UserInfoEx dbInfo;
            // делаем запрос на имена файлов, если файловые данные изменились, но не удалились
            if (info.AvatarBig != null && info.AvatarSmall != null && info.About != null)
            {
                var usersInfo = dataSource.GetUsersBriefInfo(new List<int> {info.Id});
                if (usersInfo == null || usersInfo.Count == 0)
                {
                    Logger.Info("UserInfoExCache.SetUserInfo: server error on GetUsersBriefInfo");
                    return null;
                }
                dbInfo = usersInfo[0] ?? info;
            }
            else
                dbInfo = info;

            // обновляем кэш для AvatarBig
            if (!string.IsNullOrEmpty(dbInfo.AvatarBigFileName) && info.AvatarBigData != null)
            {
                info.AvatarBigFileName = dbInfo.AvatarBigFileName;
                if (SetFile(info.AvatarBigFileName, info.AvatarBigData))
                {
                    // исключаем AvatarBig: будем передавать null и действительную хеш-сумму
                    var hashCode = info.AvatarBigHashCode;
                    info.AvatarBig = null;
                    info.AvatarBigHashCode = hashCode;
                }
            }

            // обновляем кэш для AvatarSmall
            if (!string.IsNullOrEmpty(dbInfo.AvatarSmallFileName) && info.AvatarSmallData != null)
            {
                info.AvatarSmallFileName = dbInfo.AvatarSmallFileName;
                if (SetFile(info.AvatarSmallFileName, info.AvatarSmallData))
                {
                        // исключаем AvatarSmall: будем передавать null и действительную хеш-сумму
                        var hashCode = info.AvatarSmallHashCode;
                        info.AvatarSmall = null;
                        info.AvatarSmallHashCode = hashCode;
                }
            }

            // обновляем кэш для About
            if (!string.IsNullOrEmpty(dbInfo.AboutFileName) && info.AboutData != null)
            {
                info.AboutFileName = dbInfo.AboutFileName;
                if (SetFile(info.AboutFileName, info.AboutData))
                {
                    // исключаем About: будем передавать null и действительную хеш-сумму
                    var hashCode = info.AboutHashCode;
                    info.About = null;
                    info.AboutHashCode = hashCode;
                }
            }

            var result = dataSource.SetUserInfo(info);
            // сервер на передал имена файлов
            if (info.AvatarBigData != null && string.IsNullOrEmpty(result.AvatarBigFileName) ||
                info.AvatarSmallData != null && string.IsNullOrEmpty(result.AvatarSmallFileName) ||
                info.AboutData != null && string.IsNullOrEmpty(result.AboutFileName))
            {
                Logger.Info("UserInfoExCache.SetUserInfo: server error on SetUserInfo");
                return result;
            }
            // сервер передал файловые данные
            if(result.AvatarBig != null || result.AvatarSmall != null || result.About != null)
            {
                Logger.Info("UserInfoExCache.SetUserInfo: server returned extra data on SetUserInfo");
            }

            // записываем файлы, которых раньше не было, и которым теперь сервер задал имена
            try
            {
                if (string.IsNullOrEmpty(dbInfo.AvatarBigFileName) && info.AvatarBigData != null)
                    File.WriteAllBytes(path + "\\" + result.AvatarBigFileName, info.AvatarBigData);
                if (string.IsNullOrEmpty(dbInfo.AvatarSmallFileName) && info.AvatarSmallData != null)
                    File.WriteAllBytes(path + "\\" + result.AvatarSmallFileName, info.AvatarSmallData);
                if (string.IsNullOrEmpty(dbInfo.AboutFileName) && info.About != null)
                    File.WriteAllBytes(path + "\\" + result.AboutFileName, info.AboutData);
            }
            catch (Exception ex)
            {
                Logger.Info("UserInfoExCache.SetUserInfo: error caching files", ex);
            }
            return result;
        }

        // чтение файла из БД с обновлением кэша в случае необходимости
        private List<byte[]> GetFiles(List<string> names, List<string> hashCodes)
        {
            var fileDict = new Dictionary<string, byte[]>();

            // forming request
            var requestNames = new List<string>();
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var hashCode = hashCodes[i];
                if (name == null || hashCode == null) // при вызове из GetUsersInfo: на сервере файла не существует
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            File.Delete(path + "\\" + name);
                            fileDict.Add(name, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("UserInfoExCache.GetFiles: error deleting file " + path + "\\" + name, ex);
                    }
                    continue;
                }
                if (File.Exists(path + "\\" + name))
                {
                    var data = File.ReadAllBytes(path + "\\" + name);
                    if (UserInfoEx.ComputeHash(data) == hashCode)
                    {
                        fileDict.Add(name, data);
                        continue;
                    }
                }
                requestNames.Add(name);
            }

            // requesting server
            var files = dataSource.ReadFiles(requestNames);
            if (files == null || files.Count != requestNames.Count)
            {
                Logger.Info("UserInfoExCache.GetFiles: server error");
                return null;
            }

            // saving to cache
            for (var i = 0; i < requestNames.Count; i++ )
            {
                var name = requestNames[i];
                var data = files[i];
                try
                {
                    File.WriteAllBytes(path + "\\" + name, data);
                }
                catch (Exception ex)
                {
                    Logger.Info("UserInfoExCache.GetFiles: error writing cache " + path + "\\" + name, ex);
                }
                fileDict.Add(name, data);
            }

            return names.Select(name => name == null ? null : fileDict[name]).ToList();
        }

        // запись файла в БД с обновлением кэша в случае необходимости
        private bool SetFile(string name, byte[] data)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            try
            {
                var cacheFileName = path + "\\" + name;
                if (File.Exists(cacheFileName) && UserInfoEx.ComputeHash(File.ReadAllBytes(cacheFileName)) == UserInfoEx.ComputeHash(data))
                    return false;
                dataSource.WriteFile(name, data);
                File.WriteAllBytes(cacheFileName, data);
                return true;
            }
            catch(Exception ex)
            {
                Logger.Info(string.Format("UserInfoExCache.SetFile: error caching file {0}", name), ex);
                return false;
            }
        }
    }
}
