using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Candlechart;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// Реализуется функционал автосохранение рабочего пространства терминала
    /// </summary>
    public static class SettingsAutosaver
    {
        private static readonly Regex regAutosaveFilenamePattern = new Regex("\\d{10}_\\d{10}", RegexOptions.IgnoreCase);

        public static bool AreSettingsUpdated(DateTime timeSaved)
        {
            var setsChanged = UserSettings.Instance.lastTimeModified.GetLastHit();
            // обновились настройки всего терминала?
            if (setsChanged <= timeSaved)
            {
                // обновились настройки индикаторов по графикам?
                var indiUpdated = CandleChartControl.timeUpdateIndicators.GetLastHit();
                if (indiUpdated < timeSaved)
                {
                    var objectsUpdated = CandleChartControl.timeUpdateObjects.GetLastHit();
                    if (objectsUpdated < timeSaved) 
                        return false;
                }
            }
            return true;
        }

        public static void CleanupAutosaves(string autosaveFolder, int maxAutosaveSessions)
        {
            // словарь вида: время старта сеанса сохранения - время сохранения
            var dicSaves = new List<Cortege2<string, DateTime>>();
            // допускается определенное количество сеансов и определенное количество
            // сохранений в сеансах
            foreach (var fileName in Directory.GetFiles(autosaveFolder, "*.zip"))
            {
                // разобрать имя файла типа 2110183200_2110183703.zip
                var fileNameWe = Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrEmpty(fileNameWe)) continue;
                if (!regAutosaveFilenamePattern.IsMatch(fileNameWe)) continue;

                dicSaves.Add(new Cortege2<string, DateTime>(fileName, File.GetCreationTime(fileName)));
            }
            var filesSorted = dicSaves.OrderBy(f => f.b).Select(f => f.a).ToList();

            // если количество сессий-сохранений превышает максимальное...
            var filesToRemove = filesSorted.Count <= maxAutosaveSessions
                                    ? new List<string>()
                                    : filesSorted.Take(filesSorted.Count - maxAutosaveSessions).ToList();

            // удалить все файлы в списке на удаление
            Exception lastEx = null;
            var countSucceeded = 0;
            foreach (var fileName in filesToRemove)
            {
                try
                {
                    File.Delete(fileName);
                    countSucceeded++;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }
            if (lastEx != null)
                Logger.ErrorFormat("Ошибка очистки автосохранений: удалено {0} из {1}, причина: {2}",
                                   countSucceeded, filesToRemove.Count, lastEx);
        }
       
        public static bool PackAndSaveSettingsFiles(string settingsFolder, string targetFileName, bool dialogOnError)
        {
            settingsFolder = settingsFolder.TrimEnd('\\');
            var filesToCompress = new List<string>();
            var path = string.Format("{0}{1}", settingsFolder, TerminalEnvironment.UserSettingsFileName);
            if (File.Exists(path)) filesToCompress.Add(path);
            path = string.Format("{0}\\{1}", settingsFolder, MainForm.FileNameIndiSettings);
            if (File.Exists(path)) filesToCompress.Add(path);
            path = string.Format("{0}\\{1}", settingsFolder, MainForm.FileNameObjectSettings);
            if (File.Exists(path)) filesToCompress.Add(path);

            var bytes = CompressionHelper.CompressFiles(filesToCompress.ToArray());
            try
            {
                using (var fs = new FileStream(targetFileName, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения файла настроек р.п. ({0}): {1}",
                    targetFileName, ex);
                if (dialogOnError)
                    MessageBox.Show(string.Format(
                        Localizer.GetString("MessageErrorSavingWorkspaceFmt"),
                        ex.GetType()), 
                        Localizer.GetString("TitleError"), 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static void DeleteTempSettingsFileInAutosaveFolder(string autosaveFolder)
        {
            var pathsToDelete = new List<string>();
            var path = string.Format("{0}{1}", autosaveFolder, TerminalEnvironment.UserSettingsFileName);
            if (File.Exists(path)) pathsToDelete.Add(path);
            path = string.Format("{0}\\{1}", autosaveFolder, MainForm.FileNameIndiSettings);
            if (File.Exists(path)) pathsToDelete.Add(path);
            path = string.Format("{0}\\{1}", autosaveFolder, MainForm.FileNameObjectSettings);
            if (File.Exists(path)) pathsToDelete.Add(path);
            if (pathsToDelete.Count == 0) return;
            foreach (var filePath in pathsToDelete)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("DeleteTempSettingsFileInAutosaveFolder - ошибка удаления файла \"{0}\": {1}",
                        filePath, ex);
                }
            }
        }
    }
}
