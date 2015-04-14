using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using ServerUnitManager.Contract;
using TradeSharp.UpdateContract.Contract;
using TradeSharp.UpdateContract.Entity;
using TradeSharp.UpdateManager.BL;

namespace TradeSharp.UpdateManager
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// серверу поддержки обновлений будем докладывать о процессе
        /// </summary>
        private IServerUnitManager serverUnitManager;

        private readonly Lazy<string> execFileFolder = new Lazy<string>(() =>
            {
                var sm = Assembly.GetEntryAssembly() ??
                             Assembly.GetExecutingAssembly();
                return Path.GetDirectoryName(sm.Location);
            });

        protected string ExecFileFolder
        {
            get { return execFileFolder.Value; }
        }

        private BackgroundWorker worker;

        private readonly string targetSystem;

        private int errorsCount;

        private static readonly Color[] errorColors = 
            new [] { Color.Green, Color.Olive, Color.Coral, Color.Brown, Color.Red };
        
        public MainForm()
        {
            InitializeComponent();  
            targetSystem = SystemName.Terminal.ToString();
            if (ConfigurationManager.AppSettings.AllKeys.Contains("targetSystem"))
                targetSystem = ConfigurationManager.AppSettings["targetSystem"];
        }

        private void RunClient()
        {
            var clientAppPath = string.Format("{0}\\{1}",
                                              ClientBrowser.ownPath, 
                                              ConfigurationManager.AppSettings.Get("client.app"));
            LogHelper.InfoFormat("Own path is [{0}]", ClientBrowser.ownPath);
            try
            {
                var pi = new ProcessStartInfo(clientAppPath)
                             {
                                 LoadUserProfile = true,
                                 WorkingDirectory = ClientBrowser.ownPath,
                                 UseShellExecute = false
                             };
                Process.Start(pi);                
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat("Error launching application{0}: {1}", clientAppPath, ex);
                return;
            }            
        }

        private void UploadNewFiles(bool newOnly)
        {
            worker = new BackgroundWorker();
            worker.DoWork += WorkerDoWork;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.ProgressChanged += WorkerProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(newOnly);
        }
        
        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunClient();
            Close();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            CreateWcfClient();

            AddMessageLineSafe("Сравнение версий");
            var newOnly = (bool) e.Argument;
            var clientFiles = newOnly ? ClientBrowser.GetFileVersions() : new List<FileVersion>();

            var serverFiles = ServerBrowser.GetFileVersions(targetSystem);
            if (serverFiles.Count == 0)
            {
                LogHelper.InfoFormat("{0} файлов на клиенте, файлов на сервере нет", clientFiles.Count);
                return;
            }

            var lackFiles = ClientBrowser.CompareWithServer(serverFiles, clientFiles);
            if (lackFiles.Count == 0)
            {
                LogHelper.InfoFormat("{0} файлов на клиенте, {1} файлов на сервере - нет файлов для обновления", 
                    clientFiles.Count,
                    serverFiles.Count);
                return;
            }

            AddMessageLineSafe(string.Format("{0} файлов нуждаются в обновлении" + Environment.NewLine, lackFiles.Count));
            var msg = "Загрузка следующих обновлений:" + Environment.NewLine +
                      string.Join(Environment.NewLine, lackFiles);
            AddMessageLineSafe(msg);
            LogHelper.Info(msg);

            var percent = 0;
            IUpdateManager channel = null;
            
            ReportProgressForLocalServerUnitManager(0, lackFiles.Count, false);

            var updatedCount = 0;
            foreach (var file in lackFiles)
            {
                if (worker.CancellationPending) break;
                var fileClientPath = string.Format("{0}{1}", ClientBrowser.ownPath, file); // абсолютное имя файла на клиенте
                var targetDir = Path.GetDirectoryName(fileClientPath);                    
                try
                {                    
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);                    
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorFormat("Error creating directory {0}: {1}", targetDir, ex);
                    continue;
                }

                if (!TryDownloadFile(file, fileClientPath, ref channel))
                    DisplayError();
                else
                {
                    AddMessageLineSafe("Обновлен " + file.FileName);
                    LogHelper.Info("Обновлен " + file.FileName);
                }
                worker.ReportProgress(100 * (++percent) / lackFiles.Count);

                updatedCount++;
                ReportProgressForLocalServerUnitManager(updatedCount, lackFiles.Count - updatedCount, false);
            }

            ReportProgressForLocalServerUnitManager(lackFiles.Count, 0, true);
        }

        private void DisplayError()
        {
            errorsCount++;
            var colorIndex = errorsCount < errorColors.Length ? errorsCount : errorColors.Length - 1;
            var color = errorColors[colorIndex];
            Invoke(new Action<Color>(c =>
                                         {
                                             richTextBox.SelectionStart = 0;
                                             richTextBox.SelectionLength = richTextBox.Text.Length;
                                             richTextBox.SelectionColor = c;
                                             richTextBox.SelectionLength = 0;
                                         }), color);
        }

        private bool TryDownloadFile(FileVersion vers, string clientPath, ref IUpdateManager channel)
        {
            FileData fileData = null;
            try
            {
                if (channel == null)
                    channel = new ChannelFactory<IUpdateManager>("IUpdateManagerBinding").CreateChannel();

                fileData = channel.LoadServerFile(new DownloadingFile { FilePath = vers.FileName, SystemNameString = targetSystem });
            }
            catch
            {
                try
                {
                    if (channel == null)
                        channel = new ChannelFactory<IUpdateManager>("IUpdateManagerBinding").CreateChannel();
                    fileData = channel.LoadServerFile(new DownloadingFile { FilePath = vers.FileName, SystemNameString = targetSystem });
                }
                catch (Exception ex2)
                {
                    LogHelper.ErrorFormat("Unable to download file {0}: {1}",
                        vers.FileName, ex2);
                    return false;
                }
            }

            if (fileData == null) // || fileData.FileByteStream.Length == 0)
            {
                LogHelper.ErrorFormat("Data is empty: name = {0}", vers.FileName);
                //LogHelper.ErrorFormat("Data is empty: name = {0}, length = {1}", vers.FileName,
                //                      fileData != null ? fileData.FileByteStream.Length.ToString() : "null");
                return false;
            }

            // сохранить файл
            try
            {
                using (var fs = new FileStream(clientPath, FileMode.Create))
                {
                    fileData.FileByteStream.CopyTo(fs);
                }
                return true;
            }
            catch (Exception exception)
            {
                LogHelper.InfoFormat("Error saving file {0} by name {1}: {2}. Trying to save as {1}.new", vers.FileName,
                                     clientPath, exception);
                clientPath += ".new";
                try
                {
                    using (var fs = new FileStream(clientPath, FileMode.Create))
                    {
                        fileData.FileByteStream.CopyTo(fs);
                    }
                    LogHelper.InfoFormat("Skipped saving file {0} by name {1}: {2}", vers.FileName,
                                         clientPath, exception);
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorFormat("Error saving file {0} by name {1}: {2}", vers.FileName, clientPath, ex);
                    return false;
                }
            }            
        }

        private void AddMessageLineSafe(string msg)
        {
            Invoke(new Action<string>(s => richTextBox.AppendText(msg + Environment.NewLine)), msg);
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs() ?? new string[0];
            args = args.Select(a => a.Replace("-", "").Replace("/", "").ToLower()).ToArray();
            var newOnly = !args.Any(p => p == "all");

            UploadNewFiles(newOnly);
        }

        private void BtnStopLoadingClick(object sender, EventArgs e)
        {
            if (worker.IsBusy) worker.CancelAsync();
        }
    
        private void ReportProgressForLocalServerUnitManager(int filesUpdated, int filesLeft, bool finished)
        {
            if (serverUnitManager == null) return;
            try
            {
                serverUnitManager.UpdateServiceFilesStates(ExecFileFolder,
                    filesUpdated, filesLeft, finished);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Ошибка в UpdateServiceFilesStates", ex);
                serverUnitManager = null;
            }
        }

        private void CreateWcfClient()
        {
            const string configKeyEndpoint = "ServiceUpdateHost";
            var endpointAddress = "http://localhost:55050/ServerUnitManager";
            if (ConfigurationManager.AppSettings.AllKeys.Contains(configKeyEndpoint))
                endpointAddress = ConfigurationManager.AppSettings[endpointAddress];

            if (string.IsNullOrEmpty(endpointAddress))
                return;

            try
            {
                var myBinding = new BasicHttpBinding();
                var myEndpoint = new EndpointAddress(endpointAddress);
                var myChannelFactory = new ChannelFactory<IServerUnitManager>(myBinding, myEndpoint);
                serverUnitManager = myChannelFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                LogHelper.InfoFormat("Ошибка в CreateWcfClient: " + ex);
                serverUnitManager = null;
            }
        }
    }    
}
