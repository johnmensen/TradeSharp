using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.BL.Sound;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Subscription.Model;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        /// <summary>
        /// magic ордера, созданного по сигналу, будет равен ID "родительского" ордера
        /// плюс указанное смещение
        /// </summary>
        public const int SignalMagicStart = 0;

        /// <summary>
        /// отобразить сигнал в статусной панели,
        /// отобразить объекты и открыть окно графика,
        /// если выбраны соответствующие опции
        /// </summary>        
        private void InstanceTradeSignalsUpdated(List<TradeSignalUpdate> signals)
        {
            //var accountId = AccountStatus.Instance.accountID;
            // проиграть звук - получен торговый сигнал
            EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.TradeSignal);

            foreach (var signal in signals)
            {
                // добавить в статусную панель
                var signalParamString = signal.ServiceId + "_" + signal.Ticker + "_" + signal.Timeframe;
                var signalCaption = string.Format("сигнал \"{0}\" ({1} {2})",
                    MakeTabName(signal),
                    signal.Ticker,
                    BarSettingsStorage.Instance.GetBarSettingsFriendlyName(signal.Timeframe));
                AddUrlToStatusPanelSafe(signal.TimeUpdated, signalCaption, signalParamString);

                // проверить - включен ли режим авто-открытия торгового сигнала?
                var action = UserSettings.Instance.ActionOnSignal;
                if (action == ActionOnSignal.Сообщение) continue;

                var openAuto = action == ActionOnSignal.ОткрытьГрафик;
                var skipIfNo = action == ActionOnSignal.ПоказатьНаГрафике;
                Invoke(new Action<bool, bool, bool, TradeSignalUpdate, bool>(ShowTradeSignal), openAuto, skipIfNo,
                       !skipIfNo, signal, true);
            }
        }

        private bool ProcessUserClickOnTradeSignalStatusLink(string linkTarget)
        {
            var parts = linkTarget.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) return false;
            var catId = parts[0].ToIntSafe();
            if (!catId.HasValue) return false;
            if (!DalSpot.Instance.GetTickerNames().Contains(parts[1])) return false;
            var timeframe = BarSettings.TryParseString(parts[2]);
            if (timeframe == null) return false;

            if (MessageBox.Show("Показать торговый сигнал?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return true;

            // таки показать сигнал
            ShowTradeSignal(false, false, true, new TradeSignalUpdate(catId.Value, parts[1], timeframe), true);

            return true;
        }

        private static void OnPublishTradeSignal(CandleChartControl sender)
        {
            if (!AccountStatus.Instance.isAuthorized) return;

            // проверить, какие сигналы может рассылать пользователь с указанного акаунта
            var signals = TradeSharpAccount.Instance.proxy.GetUserOwnedPaidServices(AccountStatus.Instance.Login);
            if (signals == null || signals.Count == 0)
            {
                MessageBox.Show(
                    string.Format(Localizer.GetString("MessageUnableToSendSignalsFromAccountFmt"), 
                    AccountStatus.Instance.accountID), 
                    Localizer.GetString("TitleWarning"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // открыть форму публикации сигнала
            object selSignal;
            string selectedText;
            if (!Dialogs.ShowComboDialog(Localizer.GetString("MessageSelectTradeSignalCategory"), 
                signals.Cast<object>().ToList(),
                out selSignal, out selectedText, true))
                return;
            if (selSignal == null) return;

            var srvId = ((PaidService)selSignal).Id;
            var symbol = sender.Symbol;
            var timeframe = sender.Timeframe;

            // получить XML прогноза
            var doc = new XmlDocument();
            var docNode = (XmlElement)doc.AppendChild(doc.CreateElement("forecast"));
            var nodeForecast = (XmlElement)docNode.AppendChild(doc.CreateElement(TradeSignalXml.TagNameForecast));
            var nodeObjects = (XmlElement)docNode.AppendChild(doc.CreateElement(TradeSignalXml.TagNameObjects));
            nodeForecast.Attributes.Append(doc.CreateAttribute(TradeSignalXml.AttributeUpdateTime)).Value = DateTime.Now.ToStringUniform();
            // сохранить объекты
            sender.SaveObjects(nodeObjects);

            // всем объектам прописать Magic, равный категории торговых сигналов
            foreach (XmlElement nodeSeries in nodeObjects.ChildNodes)
                foreach (XmlElement nodeObject in nodeSeries.ChildNodes)
                {
                    //if (nodeObject.Attributes["magic"] != null)
                    nodeObject.Attributes["magic"].Value = srvId.ToString();
                }

            // сохранить XML в строку
            var forecastXml = doc.ToStringUniform(TradeSignalXml.DefaultEncoding);

            // сохранить XML на сервер
            if (TradeSignalReceiver.PutForecast(srvId, symbol, timeframe, forecastXml))
                MessageBox.Show(Localizer.GetString("MessageTradeSignalIsSaved"));
            else
                MessageBox.Show(Localizer.GetString("MessageFailedToSaveTradeSignal"),
                    Localizer.GetString("TitleError"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// открыть окошко отправки "пресигнала"
        /// </summary>        
        private static void OnMakeNewSignalMessage(CandleChartControl sender)
        {
            if (!AccountStatus.Instance.isAuthorized) return;

            // проверить, какие сигналы может рассылать пользователь с указанного акаунта
            var signals = TradeSharpAccount.Instance.proxy.GetUserOwnedPaidServices(AccountStatus.Instance.Login);
            if (signals == null || signals.Count == 0)
            {
                MessageBox.Show("Со счета #" + AccountStatus.Instance.accountID +
                                " рассылка торговых сигналов невозможна", "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // открыть форму "пресигнала"
            var dlg = new NewTextSignalForm(sender, signals);
            dlg.ShowDialog();
        }

        public void ShowTradeSignal(bool autoOpenChart, bool skipIfNoChart, bool showErrorMessages,
            TradeSignalUpdate signalUpdate, bool switchToTab)
        {
            var xmlStr = TradeSignalFileStorage.Instance.LoadTradeSignalXml(signalUpdate.ServiceId,
                                                               signalUpdate.Ticker, signalUpdate.Timeframe.ToString());
            if (string.IsNullOrEmpty(xmlStr))
            {
                if (showErrorMessages)
                    MessageBox.Show(this, string.Format("Не удалось загрузить объекты для {0} {1}",
                                                  signalUpdate.Ticker,
                                                  BarSettingsStorage.Instance.GetBarSettingsFriendlyName(
                                                      signalUpdate.Timeframe)),
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            ShowTradeSignal(autoOpenChart, skipIfNoChart, showErrorMessages, signalUpdate, xmlStr, switchToTab);
        }

        /// <summary>
        /// отобразить на графике торговый прогноз,
        /// предложить открыть окно графика
        /// Thread-Safe
        /// </summary>
        private void ShowTradeSignal(bool autoOpenChart, bool skipIfNoChart, bool showErrorMessages,
            TradeSignalUpdate signalUpdate, string signalXml, bool switchToChart)
        {
            // найти график
            var chartsList = Charts;
            var chart = chartsList.FirstOrDefault(c => c.chart.Symbol == signalUpdate.Ticker &&
                c.chart.Timeframe == signalUpdate.Timeframe);
            if (chart == null && skipIfNoChart) return;

            // загрузить объекты
            XmlElement nodeObjects = null;
            try
            {
                var docObjects = new XmlDocument();
                docObjects.LoadXml(signalXml);
                if (docObjects.DocumentElement == null)
                    throw new Exception("Document element is null");
                nodeObjects = (XmlElement)docObjects.DocumentElement.SelectSingleNode(TradeSignalXml.TagNameObjects);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ShowTradeSignal(#{0} {1} {2}) error: {3}",
                    signalUpdate.ServiceId, signalUpdate.Ticker, signalUpdate.Timeframe, ex);
                return;
            }

            // предложить открыть график либо открыть автоматом            
            if (chart == null)
            {
                var chartName = signalUpdate.Ticker + " " +
                                BarSettingsStorage.Instance.GetBarSettingsFriendlyName(signalUpdate.Timeframe);
                if (!autoOpenChart)
                    if (MessageBox.Show(this, "Открыть график " + chartName + "?",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return;

                // загрузить график с объектами
                var sets = new ChartWindowSettings
                {
                    AutoScroll = false,
                    BarOffset = 5,
                    Symbol = signalUpdate.Ticker,
                    Timeframe = signalUpdate.Timeframe.ToString(),
                    WindowState = FormWindowState.Normal.ToString(),
                    WindowSize = new Size(400, 320),
                    ColorBarDn = Color.DarkGray,
                    ColorBarUp = Color.White,
                    ColorShadowUp = Color.Black,
                    ColorShadowDn = Color.Black,
                    ShowLastQuote = true,
                    UniqueId =
                        string.Format("{0}_{1}", signalUpdate.Ticker,
                                      Guid.NewGuid().ToString().Substring(0, 10))
                };
                var tabName = MakeTabName(signalUpdate);
                var bookmark = bookmarkStrip.EnsureBookmark(tabName);
                sets.TabPageId = bookmark.Id;
                LoadChildChartSafe(sets, nodeObjects, null, true);
                return;
            }

            // удалить на графике объекты с magic, равным текущему торговому сигналу
            chart.chart.DeleteObjectsByMagic(signalUpdate.ServiceId);

            // показать объекты на графике
            chart.chart.LoadObjects(nodeObjects, true, true);

            // открыть вкладку с графиком
            if (switchToChart)
                GoOnBookmark(chart.bookmarkId);

            chart.Focus();
        }

        private static string MakeTabName(TradeSignalUpdate sig)
        {
            return string.Format("Торговые сигналы {0}", sig.CategoryName); //, timeFrameName);
        }

        private static string GetSignalCategoryTitle(int categoryId)
        {
            return string.Format("Торговые сигналы {0}", categoryId);
        }

        /// <summary>
        /// пришел торговый сигнал
        /// в зависимости от настроек - торгануть или забить
        /// </summary>
        public void ProcessTradeSignalAction(TradeSignalAction action)
        {
            // проиграть звук - получен торговый сигнал
            EventSoundPlayer.Instance.PlayEvent(VocalizedEvent.TradeSignal);

            if (action is TradeSignalActionTrade)
            {
                var trade = (TradeSignalActionTrade)action;
                ProcessTradeSignalActionTrade(trade);
                return;
            }

            if (action is TradeSignalActionClose)
            {
                var trade = (TradeSignalActionClose)action;
                ProcessTradeSignalActionClose(trade);
                return;
            }

            if (action is TradeSignalActionMoveStopTake)
            {
                var trail = (TradeSignalActionMoveStopTake)action;
                ProcessTradeSignalActionMoveStopTake(trail);
                //return;
            }
        }

        /// <summary>
        /// торгануть по сигналу
        /// </summary>
        private void ProcessTradeSignalActionTrade(TradeSignalActionTrade trade)
        {
        }

        /// <summary>
        /// закрыть сделку по сигналу
        /// </summary>
        private void ProcessTradeSignalActionClose(TradeSignalActionClose trade)
        {            
        }

        /// <summary>
        /// перетащить стоп по сигналу
        /// </summary>
        private void ProcessTradeSignalActionMoveStopTake(TradeSignalActionMoveStopTake trail)
        {
        }

        public bool PutTextMessage(int signalCatId,
            string message, string ticker, BarSettings timeFrame)
        {
            if (!AccountStatus.Instance.isAuthorized) return false;

            try
            {
                var acEvent = new UserEvent
                {
                    Code = AccountEventCode.TradeSignal,
                    Action = AccountEventAction.DefaultAction,
                    Title = Localizer.GetString("TitleTradeSignal"),
                    Text = string.Join("#-#", ticker, timeFrame, message)
                };

                serverProxyTrade.proxy.SendTradeSignalEvent(CurrentProtectedContext.Instance.MakeProtectedContext(),
                    AccountStatus.Instance.accountID, signalCatId, acEvent);

                // показать окно - пресигнал отправлен
                var signalTitle =
                    string.Format(Localizer.GetString("MessageTradeSignalSentFmt"),
                                  ticker, BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeFrame));
                AddMessageToStatusPanelSafe(DateTime.Now, signalTitle);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в PutTextMessage()", ex);
                return false;
            }
        }

        /// <summary>
        /// обработать полученные сообщения: торговые сигналы, уведомления о состоянии счета и т.п.
        /// </summary>        
        public void ProcessAccountEventsSafe(List<UserEvent> acEvents)
        {
            Invoke(new Action<List<UserEvent>>(ProcessAccountEventsUnsafe), acEvents);
        }

        private void ProcessAccountEventsUnsafe(List<UserEvent> acEvents)
        {
            foreach (var evt in acEvents)
            {
                if (evt.Action == AccountEventAction.DefaultAction)
                {
                    // получить действие для торгового сигнала, настроенное в параметрах
                    var eCode = evt.Code;
                    var evAction = UserSettings.Instance.AccountEventAction.FirstOrDefault(e => e.EventCode == eCode);
                    if (evAction != null)
                        evt.Action = evAction.EventAction;
                }

                switch (evt.Action)
                {
                    case AccountEventAction.DoNothing:
                        break;
                    case AccountEventAction.StatusPanelOnly:
                        ShowEventInStatusPanel(evt);
                        break;
                    default:
                        ShowEventAlert(evt);
                        break;
                }
            }
        }

        private void ShowEventAlert(UserEvent ev)
        {
            var lines = MakeEventString(ev, true);
            // в статусную строку ...
            AddMessageToStatusPanelUnsafe(ev.Time, lines.Length > 1 ? lines[0] + ", " + lines[1] : lines[0]);
            // и в окошко
            var lineBold = lines[0];
            var linesOther = lines.Length > 1 ? lines[1] : string.Empty;
            ShowMsgWindowSafe(new AccountEvent(lineBold, linesOther, AccountEventCode.TradeSignal));
        }

        private void ShowEventInStatusPanel(UserEvent ev)
        {
            AddMessageToStatusPanelUnsafe(DateTime.Now, MakeEventString(ev, false)[0]);
        }

        public static string[] MakeEventString(UserEvent evt, bool lineBreaks)
        {
            if (evt.Code == AccountEventCode.AccountModified)
            {
                var line = string.Format("[{0:dd.MM.yyyy HH:mm}] счет №{1} - {2}",
                                evt.Time, evt.Title);
                if (string.IsNullOrEmpty(evt.Text))
                    return new[] { line };

                if (lineBreaks)
                    return new[] { line, evt.Text };
                line = line + ", " + evt.Text;
                return new[] { line };
            }

            if (evt.Code == AccountEventCode.TradeSignal)
            {
                var line = string.Format("[{0:dd.MM.yyyy HH:mm}]{1} - {2}",
                                evt.Time,
                                evt.AccountId.HasValue ? " счет №" + evt.AccountId.Value : "",
                                evt.Title);
                if (string.IsNullOrEmpty(evt.Text)) return new[] { line };

                // ticker - TF - message
                var textParts = evt.Text.Split(new[] { "#-#" }, StringSplitOptions.None);
                if (textParts.Length < 3) return new[] { line };
                var tickerString = string.IsNullOrEmpty(textParts[0]) ? "" : textParts[0] + ", ";
                var timeframeString = string.IsNullOrEmpty(textParts[1]) ? ""
                    : BarSettingsStorage.Instance.GetBarSettingsFriendlyName(textParts[1]);
                var message = textParts[2];

                var line2 = tickerString + timeframeString;
                if (!string.IsNullOrEmpty(message)) line2 = line2 + " " + message;
                return lineBreaks ? new[] { line, line2 } : new[] { line + ", " + line2 };
            }

            return new[] { String.Empty };
        }
    }
}