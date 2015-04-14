using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Threading;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    class UserSettingsStorage : IUserSettingsStorage
    {
        private readonly ConcurrentDictionary<int, ReaderWriterLockSlim> userFileLocker = 
            new ConcurrentDictionary<int, ReaderWriterLockSlim>();

        private readonly string storageFolder = ExecutablePath.ExecPath + "\\usersettings";

        public UserSettingsStorage()
        {
            if (!Directory.Exists(storageFolder))
            {
                try
                {
                    Directory.CreateDirectory(storageFolder);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка создания директории " + storageFolder, ex);
                    throw;
                }
            }
        }

        public void SaveUserSettings(int userId, string settingsString)
        {
            var locker = userFileLocker.GetOrAdd(userId, new ReaderWriterLockSlim());
            if (!locker.TryEnterWriteLock(2000))
            {
                Logger.ErrorFormat("UserSettingsStorage - таймаут записи для пользователя " + userId);
                return;
            }
            try
            {
                var fileName = GetFileName(userId);
                using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    sw.Write(settingsString);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("UserSettingsStorage - ошибка записи для пользователя " + userId, ex);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public string LoadUserSettings(int userId)
        {
            var locker = userFileLocker.GetOrAdd(userId, new ReaderWriterLockSlim());
            if (!locker.TryEnterReadLock(2000))
            {
                Logger.ErrorFormat("UserSettingsStorage - таймаут чтения для пользователя " + userId);
                return string.Empty;
            }
            try
            {
                var fileName = GetFileName(userId);
                if (!File.Exists(fileName))
                    return string.Empty;
                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("UserSettingsStorage - ошибка чтения для пользователя " + userId, ex);
            }
            finally
            {
                locker.ExitReadLock();
            }
            return string.Empty;
        }

        private string GetFileName(int userId)
        {
            return storageFolder + "\\" + userId + ".txt";
        }
    }
}
