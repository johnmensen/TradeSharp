using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TradeSharp.Util
{
    public interface IApplicationMessageTarget
    {
        bool CheckLockMessage(Keys key);
        void ProcessApplicationMessageRegisteredKey(ApplicationMessageBinding objectBinding);
    }

    /// <summary>
    /// Горячая клавиша
    /// </summary>
    public class ApplicationMessageBinding
    {
        public ApplicationMessage Message { get; set; }
        public string Title { get; set; }
        public Keys mainKey;
        public Keys? mod1, mod2;
        public enum WindowMessage { KeyUp = 0, KeyDown }
        public WindowMessage messageType;
        public bool stopFurtherProcessing = true;

        public string ActionDescription { get; set; }

        /// <summary>
        /// Возврящает строку в формате пригодном для привязки к строке таблици или textBox-у
        /// </summary>
        public string Key
        {
            get
            {
                var keys = new StringBuilder(string.Format("{0}", mainKey));
                if (mod1 != Keys.None && mod1 != null) keys.Append(string.Format(" + {0}", mod1));
                if (mod2 != Keys.None && mod2 != null) keys.Append(string.Format(" + {0}", mod2));
                return keys.ToString();
            }
        }

        /// <summary>
        /// Обычный конструктор
        /// </summary>
        public ApplicationMessageBinding(ApplicationMessage message, Keys mainKey, WindowMessage messageType)
        {
            Message = message;
            Title = EnumFriendlyName<ApplicationMessage>.GetString(message);
            this.mainKey = mainKey;
            this.messageType = messageType;
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        public ApplicationMessageBinding(ApplicationMessageBinding k) : this (k.Message, k.mainKey, k.messageType)
        {
            stopFurtherProcessing = k.stopFurtherProcessing;
            mod1 = k.mod1;
            mod2 = k.mod2;
            ActionDescription = k.ActionDescription;
        }

        /// <summary>
        /// Переопределение метода ToString(). Строка возвращается в формате пригодном для сохранения в XML 
        /// </summary>
        public override string ToString()
        {
             var keysStringDiscription = new StringBuilder(string.Format("message:{0}", Message));
             keysStringDiscription.Append(string.Format(",key:{0}", mainKey));
             if (mod1 != Keys.None && mod1 != null) keysStringDiscription.Append(string.Format(",mod1:{0}", mod1));
             if (mod2 != Keys.None && mod2 != null) keysStringDiscription.Append(string.Format(",mod2:{0}", mod2));
             keysStringDiscription.Append(string.Format(",messageType:{0}", messageType));
             keysStringDiscription.Append(string.Format(",stopFurtherProcessing:{0}", stopFurtherProcessing));
             return  keysStringDiscription.ToString();
        }

        /// <summary>
        /// Метод восстановления объекта горячей клавиши из строки. Этот метод обратный методу "ToString()"
        /// </summary>
        public static ApplicationMessageBinding Parse(string str)
        {
            var messageSrt = string.Empty;
            var keySrt = string.Empty;
            var mod1Srt = string.Empty;
            var mod2Srt = string.Empty;
            var messageTypeSrt = string.Empty;
            var stopFurtherProcessingSrt = string.Empty;

            foreach (var s in str.Split(','))
            {
                var param = s.Split(':');
                if (param.Length < 2 || string.IsNullOrEmpty(param[0].Trim()) || string.IsNullOrEmpty(param[1].Trim())) 
                {
                    Logger.Error(String.Format("Неправилиный формат записи пользовательских настроек горячих клавиш в файле UserSettings. Ошибка в  \"{0}\"", s));
                    continue; //Это значить что в паре "ключ / значение" один или оба отсутствуют
                }
 
                switch (param[0])
                {
                    case "message":
                        messageSrt = param[1];
                        break;
                    case "key":
                        keySrt = param[1];
                        break;
                    case "mod1":
                        mod1Srt = param[1];
                        break;
                    case "mod2":
                        mod2Srt = param[1];
                        break;
                    case "messageType":
                        messageTypeSrt = param[1];
                        break;
                    case "stopFurtherProcessing":
                        stopFurtherProcessingSrt = param[1];
                        break;
                }
            }

            ApplicationMessage message;
            Keys key;
            Keys? mod1;
            Keys? mod2;

            WindowMessage messageType;
            bool stopFurtherProcessing;
            
            try
            {
                message = Enum.GetValues(typeof (ApplicationMessage)).Cast<ApplicationMessage>().First(x => x.ToString() == messageSrt);
                key = Enum.GetValues(typeof (Keys)).Cast<Keys>().First(x => x.ToString() == keySrt);
                mod1 = Enum.GetValues(typeof (Keys)).Cast<Keys>().FirstOrDefault(x => x.ToString() == mod1Srt);
                mod2 = Enum.GetValues(typeof (Keys)).Cast<Keys>().FirstOrDefault(x => x.ToString() == mod2Srt);

                messageType = Enum.GetValues(typeof (WindowMessage)).Cast<WindowMessage>().First(x => x.ToString() == messageTypeSrt);
                stopFurtherProcessing = stopFurtherProcessingSrt == "True";
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при попытке распарсить кастомные настройки горячих клавиш из файла UserSettings", ex);
                return null;
            }
            

            return new ApplicationMessageBinding(message, key, messageType) { mod1 = mod1, mod2 = mod2, stopFurtherProcessing = stopFurtherProcessing };
        }

        /// <summary>
        /// Сравнивает два экзампляра горячих клавиш по значениям полей
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool AreEqual(ApplicationMessageBinding b)
        {
            return mainKey == b.mainKey && mod1 == b.mod1 && mod2 == b.mod2 && messageType == b.messageType &&
                stopFurtherProcessing == b.stopFurtherProcessing;
        }
    }

    /// <summary>
    /// Перечисление всех быстрых действий, доступных в системе
    /// </summary>
    public enum ApplicationMessage
    {
        [Description("Настроить Индикаторы")]
        SetupIndicators = 1,
        [Description("Снимок Графика")]
        ChartSnapshot = 2,
        [Description("Новый Ордер")]
        NewOrder = 3,
        Quotes = 4,
        Chat = 5,
        Statistics = 6,
        [Description("Настройка Роботов")]
        RobotSetup = 7
    }

    public class AppMessageFilter : IMessageFilter
    {
        /// <summary>
        /// Список горячих склавиш "по умолчанию"
        /// </summary>
        public static readonly List<ApplicationMessageBinding> defaultHotKeys = new List<ApplicationMessageBinding>
        {
            new ApplicationMessageBinding(ApplicationMessage.SetupIndicators, Keys.F7, ApplicationMessageBinding.WindowMessage.KeyUp) { ActionDescription = "Вызов окна настройки индикаторов графика"},
            new ApplicationMessageBinding(ApplicationMessage.ChartSnapshot, Keys.P, ApplicationMessageBinding.WindowMessage.KeyDown) { mod1 = Keys.Control, ActionDescription = "Создание снимка графика"},
            new ApplicationMessageBinding(ApplicationMessage.NewOrder, Keys.F9, ApplicationMessageBinding.WindowMessage.KeyUp) { ActionDescription = "Создание нового ордера"},
            new ApplicationMessageBinding(ApplicationMessage.Quotes, Keys.F11, ApplicationMessageBinding.WindowMessage.KeyUp) { ActionDescription = "Вызов окна настройки котировок"},
            new ApplicationMessageBinding(ApplicationMessage.Chat, Keys.C, ApplicationMessageBinding.WindowMessage.KeyUp) { mod1 = Keys.Alt, ActionDescription = "Вызов чата терминала"},
            new ApplicationMessageBinding(ApplicationMessage.Statistics, Keys.S, ApplicationMessageBinding.WindowMessage.KeyUp) { mod1 = Keys.Control, ActionDescription = "Вызов окна статистики"}
        };
        
        /// <summary>
        /// Список горячих клавиш с кастомными настройками (при старте заполняется дефолтными значениями)
        /// </summary>
        private static List<ApplicationMessageBinding> hotKeys = defaultHotKeys.Select(k => new ApplicationMessageBinding(k)).ToList();
        
        /// <summary>
        /// Возвращает все зарегистрированные в системе горячие клавиши
        /// </summary>
        public static List<ApplicationMessageBinding> HotKeys
        {
            get { return hotKeys; }
            set { hotKeys = value; }
        }

        private IApplicationMessageTarget ownerForm;
        private const int WmKeyUp = 0x101;
        private const int WmKeyDown = 0x100;
        
        public AppMessageFilter(IApplicationMessageTarget ownerForm)
        {
            this.ownerForm = ownerForm;
        }

        /// <summary>
        /// Возврашяет, в виде строки, те горячие клавиши, хотя бы одно поле которых не равно клавишам из списка "defaultHotKeys".
        /// Метод AreEqual сравнивает по значениям.
        /// </summary>
        /// <returns>возврашает текушее состояние горячих клавиш в виде строки</returns>
        public static string GetDiffString()
        {
            var difValues = hotKeys.Where(k => !defaultHotKeys.Any(d => d.AreEqual(k)));
            return string.Join(";", difValues);
        }

        /// <summary>
        /// применяет кастомные насройки, переданные в строке "diffString", к дефолтному списку горячих клавиш.
        /// </summary>
        /// <param name="diffString">кастомные настройки. Должны быть прочитаны из UserSettings</param>
        public static void ApplyDiffString(string diffString)
        {
            // вернуть дефолтовые
            hotKeys = defaultHotKeys.Select(k => new ApplicationMessageBinding(k)).ToList();

            // прочитать и обновить значения
            if (string.IsNullOrEmpty(diffString.Trim())) return;
            var keyStrings = diffString.Split(';');

            foreach (var keyString in keyStrings)
            {
                var msg = ApplicationMessageBinding.Parse(keyString);
                if (msg == null) continue;
                var msgType = msg.Message;

                var hotKey = hotKeys.First(k => k.Message == msgType);
                if (hotKey.AreEqual(msg)) continue;
                hotKeys.Remove(hotKey);
                hotKeys.Add(msg);
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WmKeyUp || m.Msg == WmKeyDown)
            {
                
                var messageType =
                    m.Msg == WmKeyUp
                        ? ApplicationMessageBinding.WindowMessage.KeyUp
                        : ApplicationMessageBinding.WindowMessage.KeyDown;
                var key = (Keys)(int)m.WParam & Keys.KeyCode;


                if (!ownerForm.CheckLockMessage(key))
                {
                    var pressedKey = hotKeys.FirstOrDefault(x => x.mainKey == key); // Ищем в списке горячих клавиш нажатую сейчас клавишу

                    if (pressedKey != null && messageType == pressedKey.messageType)
                    {
                        var modifierKeys = Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(x => Control.ModifierKeys.ToString().Split(',').Select(y => y.Trim()).Contains(x.ToString())).ToArray();


                        //TODO Переписать эо обязательно
                        //если должен быть нажат модификатор
                        if (pressedKey.mod1.HasValue && pressedKey.mod1.Value != Keys.None)
                        {
                            if ((modifierKeys.Length == 1 && modifierKeys[0] == Keys.None) || !modifierKeys.Contains(pressedKey.mod1.Value)) return false;
                        }
                        if (pressedKey.mod2.HasValue && pressedKey.mod2.Value != Keys.None)
                        {
                            if ((modifierKeys.Length == 1 && modifierKeys[0] == Keys.None) || !modifierKeys.Contains(pressedKey.mod2.Value)) return false;
                        }


                        //Если не должно быть нажато модификатора
                        if (!pressedKey.mod1.HasValue || pressedKey.mod1.Value == Keys.None)
                        {
                            if ((modifierKeys.Length == 1 && modifierKeys[0] != Keys.None) || modifierKeys.Length > 1) return false;
                            
                        }

                        if (!pressedKey.mod2.HasValue || pressedKey.mod2.Value == Keys.None)
                        {
                            if (modifierKeys.Length == 2 && modifierKeys[1] != Keys.None) return false;
                        }
                        
                        ownerForm.ProcessApplicationMessageRegisteredKey(pressedKey);
                        return pressedKey.stopFurtherProcessing;
                    }
                }
            }
            return false;
        }
    }
}
