using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TestStack.White;
using TestStack.White.UIItems.WindowItems;
using TradeSharp.Util;

namespace TradeSharp.Test.WinForms
{
    public class WinFormTest
    {
        private const string TestAppExeName = @"TradeSharp.Test.WinFormContainer.exe";

        /// <summary>
        /// Пустое приложение для тестирование
        /// </summary>
        protected Application application;

        /// <summary>
        /// Тестируемое окно
        /// </summary>
        protected Window window;

        /// <summary>
        /// Способ построение приложения - релиз или отладка
        /// </summary>
        private string BuildConfigName
        {
            get
            {
                #if DEBUG
                return "Debug";
                #else
                return "Release";
                #endif
            }
        }

        /// <summary>
        /// Инициализация и запуск 'пустого' прилжения для тестирования
        /// </summary>
        /// <returns></returns>
        public bool InitEmptyApplication(string formName)
        {
            formName = formName.Replace(" ", "");

            // найти приложение - пустышку
            var fullPath = CheckAllIniFolders(TestAppExeName);
            if (string.IsNullOrEmpty(fullPath))
                return false;
            
            try
            {
                Logger.Debug(string.Format("Поиск каталога {0} и попытка запуска 'пустого' приложения", fullPath));
                //var processStartInfo = new ProcessStartInfo(fullPath, formName);
                //application = Application.Launch(processStartInfo);
                var proc = new Process { StartInfo = { FileName = fullPath, Arguments = formName } };
                proc.Start();
                Logger.InfoFormat("Spawned proc ID: {0}, own proc ID: {1}",
                                  proc.Id, Process.GetCurrentProcess().Id);
                application = Application.Attach(proc.Id);
                
                Logger.Debug("Пустое приложение для тестирования запущено");
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось запустить пустое приложение. ", ex);
                return false;                
            }
            return true;
        }
    
        private string CheckAllIniFolders(string exeName)
        {
            var okPath = string.Empty;
            var folders = new List<string>();

            // подход №1
            try
            {
                var sm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var path = sm.EscapedCodeBase.Replace(@"file:///", "");
                path = path.Substring(0, path.IndexOf("src/Test", StringComparison.OrdinalIgnoreCase) + "src/Test".Length).Replace(@"/", "\\");
                path = Path.Combine(path, @"TradeSharp.Test.WinFormContainer\bin\" + BuildConfigName);
                folders.Add(path);
            }
            catch (Exception ex)
            {
                folders.Add("fail(" + ex.Message + ")");
            }

            // подход №2
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(typeof(WinFormTest).Assembly.Location);
                folders.Add(Path.GetDirectoryName(config.FilePath));
            }
            catch (Exception ex)
            {
                folders.Add("fail(" + ex.Message + ")");
            }

            // подход №3
            try
            {
                folders.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            catch (Exception ex)
            {
                folders.Add("fail(" + ex.Message + ")");
            }

            // подход №4
            try
            {
                folders.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", ""));
            }
            catch (Exception ex)
            {
                folders.Add("fail(" + ex.Message + ")");
            }
            
            for (var i = 0; i < folders.Count; i++)
            {
                if (!Directory.Exists(folders[i]))
                {
                    Logger.Info("CheckAllIniFolders[" + i + "]: folder does not exist: " + folders[i]);
                    continue;
                }

                var fileName = Path.Combine(folders[i], exeName);
                if (!File.Exists(fileName))
                    {
                    Logger.Info("CheckAllIniFolders[" + i + "]: file does not exist: " + fileName);
                    continue;
                }

                okPath = fileName;
            }

            return okPath;
        }
    }
}
