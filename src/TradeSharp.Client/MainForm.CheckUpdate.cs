using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using TradeSharp.Client.BL.Sound;
using TradeSharp.Contract.Entity;
using TradeSharp.UI.Util.Update;
using TradeSharp.UpdateContract.Contract;
using TradeSharp.UpdateContract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        private readonly ThreadSafeTimeStamp lastCheckUpdate = new ThreadSafeTimeStamp();

        private const int CheckUpdateIntervalMils = 1000 * 60;

        private volatile bool updateCheckInProgress;

        private long lastUpdateFileDiffHash;
        
        private void CheckUpdates()
        {
            // проверка уже запущена?
            if (updateCheckInProgress) return;

            // проверка запускалась недавно?
            var lastTimeUpdated = lastCheckUpdate.GetLastHitIfHitted();
            if (!lastTimeUpdated.HasValue)
            {
                lastCheckUpdate.Touch();
                return;
            }
            var milsSince = (DateTime.Now - lastTimeUpdated.Value).TotalMilliseconds;
            if (milsSince < CheckUpdateIntervalMils) return;

            // асинхронно запустить проверку
            if (updateCheckInProgress) return;
            updateCheckInProgress = true;
            ThreadPool.QueueUserWorkItem(DoCheckUpdates, null);
        }

        private void DoCheckUpdates(object data)
        {
            try
            {
                // получить версии файлов на сервере
                List<FileVersion> serverFiles;
                try
                {
                    var factory = new ChannelFactory<IUpdateManager>("IUpdateManagerBinding");
                    var channel = factory.CreateChannel();
                    var files = channel.GetFileProperties(SystemName.Terminal);
                    serverFiles = files.Select(f => new FileVersion
                        {
                            FileName = f.Path + "\\" + f.Name,
                            Date = f.TimeUpdated,
                            Length = f.Length,
                            HashCode = f.HashCode
                        }).ToList();
                    if (serverFiles.Count == 0)
                    {
                        Logger.Error("Ошибка получения версий файлов на сервере");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка получения версий файлов на сервере", ex);
                    return;
                }

                // получить версии файлов в родном каталоге
                var ownFiles = ClientFileBrowser.GetFileVersions(Logger.ErrorFormat);
                var diffs = ClientFileBrowser.CompareWithServer(serverFiles, ownFiles);

                if (diffs.Count == 0) return;

                // антиспам
                var difHash = diffs.Sum(d => d.MakeHash());
                if (lastUpdateFileDiffHash == difHash) return;
                lastUpdateFileDiffHash = difHash;

                // предупредить пользователя о том, что некоторые файлы нуждаются в обновлении
                try
                {
                    EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.TradeSignal);
                }
                catch (Exception ex)
                {
                    Logger.Error("DoCheckUpdates() - EventSoundPlayer error", ex);
                }

                AddUrlToStatusPanelSafe(DateTime.Now,
                    string.Format(
                    Localizer.GetString("MessageSomeFilesOutdatedFmt"),
                    diffs.Count), "Restart");

                var listFiles = string.Join(" ", diffs.Select(d => d.FileName));
                if (listFiles.Length > 255)
                    listFiles = listFiles.Substring(0, 252) + "...";

                ShowMsgWindowSafe(new AccountEvent(
                    Localizer.GetString("MessageTerminalUpdateAvailable"),
                    string.Format(Localizer.GetString("MessageNFilesOutdatedFmt"), diffs.Count) + 
                    ": " + listFiles, AccountEventCode.UpdatePending));

            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка процедуры проверки обновлений на сервере", ex);
            }
            finally
            {
                updateCheckInProgress = false;
                lastCheckUpdate.Touch();
            }
        }

        /// <summary>
        /// пользователь кликнул ссылку в окне сообщений - рестарт терминала
        /// </summary>
        private bool ProcessUserClickOnRestartLink(string linkTarget)
        {
            if (linkTarget != "Restart") return false;
            // предложить выйти из терминала
            if (MessageBox.Show("Работа терминала будет завершена. Продолжить?", "Завершить работу",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return true;

            skipFormClosingPrompt = true;
            Close();
            return true;
        }
    }    
}
