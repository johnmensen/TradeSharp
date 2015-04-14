using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Sound
{
    /// <summary>
    /// потокобезопасный класс - асинхронно проигрывает определенный звук
    /// (помещает в очередь на воспроизведение, один из N потоков)
    /// </summary>
    class EventSoundPlayer
    {
        private static EventSoundPlayer instance;

        public static EventSoundPlayer Instance
        {
            get { return instance ?? (instance = new EventSoundPlayer()); }
        }

        private volatile int soundStreamsCount;

        private readonly int soundStreamsMax = AppConfig.GetIntParam("Sound.MaxStreams", 3);

        private readonly Dictionary<string, byte[]> soundFileContent = 
            new Dictionary<string, byte[]>();

        public volatile bool silent;

        private readonly Dictionary<VocalizedEvent, string> defaultSounds =
            new Dictionary<VocalizedEvent, string>
                {
                    { VocalizedEvent.Started, "started.wav" },
                    { VocalizedEvent.LoggedIn, "logged.wav" },
                    { VocalizedEvent.CommonError, "error.wav" },
                    { VocalizedEvent.TradeResponse, "order.wav" },
                    { VocalizedEvent.TradeSignal, "signal.wav" },
                };

        /// <summary>
        /// вызывается единожды при загрузке, читает звуковые файлы
        /// </summary>
        public void LoadSounds()
        {
            // прочитать содержимое файлов
            var soundPath = ExecutablePath.ExecPath + "\\sounds\\";
            try
            {            
                if (Directory.Exists(soundPath))
                {
                    foreach (var file in Directory.GetFiles(soundPath))
                    {
                        var bytes = File.ReadAllBytes(file);
                        if (bytes.Length > 10)
                            soundFileContent.Add(Path.GetFileName(file), bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки звука", ex);
            }

            ApplySoundScheme();
        }

        /// <summary>
        /// из UserSettings взять схему событие - имя файла
        /// и инициализировать SoundSettings
        /// </summary>
        public void ApplySoundScheme()
        {
            silent = UserSettings.Instance.Mute;

            try
            {
                // добить недостающие события/звуки в настройках
                var setsLacks = false;
                foreach (var pair in defaultSounds)
                {
                    var evt = pair.Key;
                    if (UserSettings.Instance.VocalEvents.Any(e => e.EventName == evt)) continue;
                    setsLacks = true;
                    UserSettings.Instance.VocalEvents.Add(new VocalizedEventFileName
                                                              {
                                                                  EventName = evt,
                                                                  FileName = pair.Value
                                                              });
                }
                if (setsLacks) UserSettings.Instance.SaveSettings();
            
                var dic = new Dictionary<VocalizedEvent, byte[]>();
                foreach (var sets in UserSettings.Instance.VocalEvents)
                {
                    byte[] bytes;
                    soundFileContent.TryGetValue(sets.FileName, out bytes);
                    if (bytes != null)
                        dic.Add(sets.EventName, bytes);
                }
                SoundSettings.UpdateEventSound(dic);
            }
            catch (Exception ex)
            {
                Logger.Error("ApplySoundScheme() error", ex);
            }
        }

        /// <summary>
        /// асинхронно запустить проигрывание соотв. звука
        /// </summary>
        public void PlayEvent(VocalizedEvent evt)
        {
            if (silent) return;
            // проверить количество потоков
            if (soundStreamsCount >= soundStreamsMax) return;
            
            // получить содержимое звукового файла
            var bytes = SoundSettings.GetEventSound(evt);
            if (bytes == null || bytes.Length == 0) return;
            
            // проиграть в отдельном потоке
            ThreadPool.QueueUserWorkItem(PlaySoundSync, bytes);
        }

        public static string MakeSoundFilePath(string fileName)
        {
            return ExecutablePath.ExecPath + "\\sounds\\" + fileName;
        }

        private void PlaySoundSync(object state)
        {
            var bytes = (byte[]) state;
            try
            {
                soundStreamsCount++;
                using (var audioMemory = new MemoryStream(bytes))
                {
                    try
                    {
                        using (var player = new SoundPlayer(audioMemory))
                        {
                            player.PlaySync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка воспроизведения звука: " + ex.Message);
                    }                    
                }
            }
            finally
            {
                soundStreamsCount--;
            }            
        }
    }
}
