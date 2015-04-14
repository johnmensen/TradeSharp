using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls.Bookmark;
using TradeSharp.Client.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    /*
     * Автосохранение рабочего пространства терминала
    */
    public partial class MainForm
    {
        private const string AutosaveFileFormat = "{0:ddMMHHmmss}_{1:ddMMHHmmss}.zip";
        
        /// <summary>
        /// если пришло время сохранить настройки автоматически - сделать это
        /// </summary>
        private void CheckAutosave()
        {
            if (!terminalIsLoaded) return;
            if (!workspaceIsLoadedOk) return;

            var timeSaved = timeLastAutosave.GetLastHit();
            var deltaMils = (DateTime.Now - timeSaved).TotalMilliseconds;
            if (deltaMils < autosaveIntervalMils) return;
            // если нет изменений
            if (!SettingsAutosaver.AreSettingsUpdated(timeSaved)) return;
            // выполнить автосохранение
            var fileName = string.Format(AutosaveFileFormat, timeStarted, DateTime.Now);
            // почистить старые файлы автосохранения
            if (PerformAutosave(fileName))
                SettingsAutosaver.CleanupAutosaves(autosaveFolder, maxAutosaveSessions);
            // обновить время последнего сохранения
            timeLastAutosave.Touch();
        }

        public bool PerformAutosave(string fileName)
        {
            if (!EnsureAutosavePath(autosaveFolder)) return false;
            var targetFilePath = autosaveFolder + '\\' + fileName;
            // сохранить настройки во временную папку
            SaveCurrentSettings(autosaveFolder, true);
            // упаковать настройки в файл
            var result = SettingsAutosaver.PackAndSaveSettingsFiles(autosaveFolder, targetFilePath, false);
            // удалить временные файлы
            SettingsAutosaver.DeleteTempSettingsFileInAutosaveFolder(autosaveFolder);
            return result;
        }

        private void SaveCurrentSettings(bool autosaving = true)
        {
            SaveCurrentSettings(ExecutablePath.ExecPath, autosaving);
        }

        private void SaveCurrentSettings(string folder, bool autosaving)
        {
            if (!terminalIsLoaded)
            {
                Logger.Debug("SaveCurrentSettings - is not loaded");
                return;
            }
            if (isExiting && autosaving) return;
            try
            {
                if (!loadChartsEvent.WaitOne(timeoutLockSaveMs))
                {
                    Logger.Error("Таймаут на сохранении конфигурации");
                    return;
                }
                
                loadChartsEvent.Reset();
                // настройки всего терминала
                UserSettings.Instance.WindowSize = new Size(Width, Height);
                UserSettings.Instance.WindowPos = new Point(Left, Top);
                UserSettings.Instance.StatusBarHeight = panelStatus.Height;
                UserSettings.Instance.StatusSplitterPositionRel =
                    splitContainerStatus.Width <= 0
                        ? 50
                        : 100*splitContainerStatus.SplitterDistance/splitContainerStatus.Width;

                UserSettings.Instance.ChartSetsList.Clear();
                if (isExiting) return;

                // настройки окон (видимость, размеры, вкладки)
                SaveNonChartWindowsPlacement(null);
                foreach (var child in MdiChildren)
                {
                    if (child is ChartForm)                    
                        UserSettings.Instance.ChartSetsList.Add(GetChildSettings((ChartForm)child));
                }

                // настройки прочих окон (не графиков)
                UserSettings.Instance.WindowSetsList = nonChartWindows;
                // закладки
                UserSettings.Instance.TabPages = BookmarkStorage.Instance.bookmarks.ToList();
                UserSettings.Instance.SaveSettings(folder + TerminalEnvironment.UserSettingsFileName);
                Logger.Debug("UserSettings.Instance.SaveSettings(" + 
                    folder + TerminalEnvironment.UserSettingsFileName + ")");

                // сохранить объекты графика и настройки индюков
                SaveObjectsAndIndicators(folder);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения настроек", ex);
            }
            finally
            {
                loadChartsEvent.Set();
            }
        }

        private void SaveObjectsAndIndicators(string folder)
        {
            var doc = new XmlDocument();
            var docItem = (XmlElement) doc.AppendChild(doc.CreateElement("objects"));
            foreach (var child in Charts)
            {
                child.SaveObjects(docItem);
                // индикаторы
                child.chart.SaveIndicatorSettings(string.Format("{0}\\{1}", folder, FileNameIndiSettings));
            }
            try
            {
                // объекты
                doc.Save(string.Format("{0}\\{1}", folder, FileNameObjectSettings));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка сохранения \"{0}\": {1}", FileNameObjectSettings, ex);
            }
        }

        public static bool EnsureAutosavePath(string autosaveFolder)
        {
            try
            {
                if (!Directory.Exists(autosaveFolder))
                    Directory.CreateDirectory(autosaveFolder);
                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка создания каталога автосохранения ({0}): {1}",
                    autosaveFolder, ex);
                return false;
            }
        }
    }
}