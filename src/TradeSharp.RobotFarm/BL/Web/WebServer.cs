using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.RobotFarm.BL.HtmlHelper;
using TradeSharp.RobotFarm.Request;
using TradeSharp.Util;
using QuoteStorage = TradeSharp.Contract.Util.BL.QuoteStorage;

namespace TradeSharp.RobotFarm.BL.Web
{
    partial class WebServer : BaseWebServer
    {
        class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding encoding;

            public StringWriterWithEncoding(StringBuilder sb, Encoding newEncoding) : base(sb) { encoding = newEncoding; }

            public override Encoding Encoding { get { return encoding ?? base.Encoding; } }
        }

        private const string ImgRsxHome = "TradeSharp.RobotFarm.img.ico_home.png";
        private const string ImgRsxPlay = "TradeSharp.RobotFarm.img.ico_play.png";
        private const string ImgRsxPause = "TradeSharp.RobotFarm.img.ico_pause.png";
        private const string ImgRsxStarting = "TradeSharp.RobotFarm.img.ico_starting.png";
        private const string ImgRsxStopping = "TradeSharp.RobotFarm.img.ico_stopping.png";
        private const string ImgRsxActivityTable = "TradeSharp.RobotFarm.img.ico_table.png";
        private const string ImgRsxSave = "TradeSharp.RobotFarm.img.ico_save.png";
        private const string ImgRsxSaveAs = "TradeSharp.RobotFarm.img.ico_save_as.png";
        private const string ImgRsxLog = "TradeSharp.RobotFarm.img.ico_log.png";
        private const string ImgRsxBackground = "TradeSharp.RobotFarm.img.carbon.png";

        private const string RequestCloseDealsByAccount = "closeAccountOrders";

        private static WebServer instance;

        public static WebServer Instance
        {
            get { return instance ?? (instance = new WebServer()); }
        }

        public override string ServiceName
        {
            get { return "Ферма роботов"; }
        }

        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            // запрос возвращает разметку или JSON?
            if (context.Request.QueryString.AllKeys.Contains("json") && context.Request.HttpMethod == "GET")
            {
                Logger.Info("HTTP GET (JSON) request");
                ProcessJsonRequest(context);
                return;
            }

            var messageToPopup = string.Empty;
            var accountAction = string.Empty; // Текущее действие пользователя, при работе с таблицей счетов
            if (context.Request.HttpMethod == "POST")
            {
                if (context.Request.QueryString.AllKeys.Contains("json"))
                {
                    Logger.Info("HTTP POST (JSON) request");
                    ProcessJSONPostRequest(context);
                    return;
                }
                Logger.Info("HTTP POST request");
                if (context.Request.QueryString.AllKeys.Contains("actionAccount"))
                    if (AccountAjaxJsonAction(context)) return;
                if (context.Request.QueryString.AllKeys.Contains("editAccountsTable"))
                    messageToPopup = AccountSubmitAction(context, out accountAction);
                else if (context.Request.QueryString.AllKeys.Contains("updatePortfolio"))
                    messageToPopup = UpdatePortfolio(context, out accountAction);
                else
                    messageToPopup = ProcessFileUpload(context);
            }

            Logger.Info("HTTP GET (regular) request");

            #region Сохранить настройки фермы
            if (context.Request.QueryString.AllKeys.Contains("saveas"))
            {
                // выдать результат - настройки RobotFarm в виде XML
                var doc = new XmlDocument();
                if (RobotFarm.Instance.SaveSettings(doc))
                {
                    var sbXml = new StringBuilder();
                    using (var tw = new StringWriterWithEncoding(sbXml, Encoding.UTF8))
                    using (var xw = XmlWriter.Create(tw))
                    {
                        doc.Save(xw);
                    }
                    // выдать текст (XML)
                    var resp = Encoding.UTF8.GetBytes(sbXml.ToString());
                    context.Response.ContentType = "text/xml";
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.ContentLength64 = resp.Length;
                    context.Response.OutputStream.Write(resp, 0, resp.Length);
                    return;
                }
                else
                    messageToPopup = "Ошибка сохранения";
            }

            if (context.Request.QueryString.AllKeys.Contains("actionAccount"))
            {
                if (AccountAjaxAction(context)) return;
            }

            if (context.Request.QueryString.AllKeys.Contains("actionPortfolio"))
            {
                if (PortfolioAjaxAction(context)) return;
            }

            if (context.Request.QueryString.AllKeys.Contains("requestTradeSignalCat"))
            {
                if (GetTradeSignalCategory(context)) return;
            }

            if (context.Request.QueryString.AllKeys.Contains(RequestCloseDealsByAccount))
            {
                if (CloseDealsByAccount(context)) return;
            }

            if (context.Request.QueryString.AllKeys.Contains("switchAccountEnabled"))
            {
                var accId = context.Request.QueryString["switchAccountEnabled"].ToIntSafe() ?? 0;
                if (accId == 0) return;
                // включить / отключить счет
                var acc = RobotFarm.Instance.Accounts.FirstOrDefault(a => a.AccountId == accId);
                if (acc == null) return;
                acc.TradeEnabled = !acc.TradeEnabled;
                // сообщить - все ОК
                var resp = string.Format("{{\"enabled\":{0}, \"countEnabled\":\"{1}\"}}",
                    acc.TradeEnabled.ToString().ToLower(), GetTradableAccountsCount());
                WriteTextResponse(context, resp);
                return;
            }

            if (context.Request.QueryString.AllKeys.Contains("removeRobot"))
            {
                // ферма остановлена?
                if (RobotFarm.Instance.State != FarmState.Stopped) return;

                var robotIndex = context.Request.QueryString["removeRobot"].ToIntSafe() ?? -1;
                if (robotIndex < 0) return;

                var accId = context.Request.QueryString["forAccount"].ToIntSafe() ?? 0;
                if (accId == 0) return;
                var acc = RobotFarm.Instance.GetAccountById(accId);
                if (acc == null) return;

                if (robotIndex < acc.Robots.Count)
                    acc.Robots.RemoveAt(robotIndex);

                const string resp = "{\"status\": \"OK\"}";
                WriteTextResponse(context, resp);
                return;
            }

            // запрос на сохранение настроек
            if (context.Request.QueryString.AllKeys.Contains("saveSettings"))
            {
                RobotFarm.Instance.SaveSettings();
                // ответить - все гуд, сообщить состояние фермы
                WriteTextResponse(context, "{\"saved\":\"true\"}");
                return;
            }

            // запрос на запуск - останов фермы...
            if (context.Request.QueryString.AllKeys.Contains("startStopFarm"))
            {
                RobotFarm.Instance.StartOrStopFarm();
                // ответить - все гуд, сообщить состояние фермы
                WriteTextResponse(context, string.Format("{{\"state\":\"{0}\"}}", RobotFarm.Instance.State));
                return;
            }
            #endregion

            // если обычный запрос...
            var sb = new StringBuilder();
            var sbBackImg = new StringBuilder();
            RenderImgBytesSrc(sbBackImg, ImgRsxBackground);

            RenderHttpHead(sb,
                "",//"body { background-image:url(" + sbBackImg + "); background-repeat: repeat; }",
                GetScripts(), true);
            // добавить шапку
            sb.AppendLine("<body style=\"background-color:#303030;margin:0px;padding-top:10px\">");
            RenderPopupDiv(sb, 200, 50, "messageDiv");
            sb.AppendLine("  <div style=\"background-color:White;width:800px;margin: auto;text-align: left;padding:10px\">");

            // рендерить меню
            RenderFarmPageMenu(sb, context);
            // рендерить страницу 
            if (context.Request.QueryString.AllKeys.Contains("log"))
                RenderServerLog(sb);
            else if (context.Request.QueryString.AllKeys.Contains("openedPositionsByAccount"))
                RenderAccountPositions(sb, context.Request.QueryString["openedPositionsByAccount"].ToIntSafe() ?? 0);
            else if (context.Request.QueryString.AllKeys.Contains("subscriptionsByAccount"))
                RenderAccountSubscriptions(sb, context.Request.QueryString["subscriptionsByAccount"].ToIntSafe() ?? 0);
            else if (context.Request.QueryString.AllKeys.Contains("portfolioTradeSettings"))
                RenderPortfolioSettings(sb);
            else if (context.Request.QueryString.AllKeys.Contains("trades"))
                RenderTrades(sb, context.Request.QueryString["trades"].ToIntSafe() ?? 0);
            

            else if (context.Request.QueryString.AllKeys.Contains("editAccountsTable"))
            {
                switch (accountAction)
                {
                    case "openNewAccountForm":
                        InsertAddAccountTableControl(sb);
                        break;
                    default:
                        RenderAccountEditTable(sb);
                        InsertAddAccountButtonControl(sb);
                        break;
                }
            }
            else
                RenderFarmPage(sb, context);

            // закрыть тег
            sb.AppendLine("  </div>");


            // всплывающее сообщение?
            if (!string.IsNullOrEmpty(messageToPopup))
                sb.AppendLine("<script>showMessage('messageDiv', '" + messageToPopup + "', 1500);</script>");

            sb.AppendLine("</body>\r\n</html>");

            // записать ответ
            var r = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentLength64 = r.Length;

            context.Response.OutputStream.Write(r, 0, r.Length);
        }

        /// <summary>
        /// на сервер (на ферму) заливаетсяpro файл pxml, содержащий описание портфеля роботов
        /// вернуть сообщение, которое нужно отобразить в Popup-e
        /// </summary>
        private string ProcessFileUpload(HttpListenerContext context)
        {
            var xmlDocs = new Dictionary<string, XmlDocument>();
            var hiddenFields = new Dictionary<string, string>();

            try
            {
                string dataText;
                using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    dataText = reader.ReadToEnd();
                }
                var parser = new HttpRequestParser(dataText);
                foreach (var ctx in parser.records)
                {
                    try
                    {
                        var doc = ctx.GetXml();
                        xmlDocs.Add(ctx.FileName, doc);
                    }
                    catch
                    {
                        hiddenFields.Add(ctx.Name, ctx.FileData);
                    }
                }
            }
            catch (Exception ex)
            {
                var strError = "Ошибка в ProcessFileUpload(): " + ex;
                Logger.Error(strError);
                RobotFarm.Instance.AppendLogMessage(strError);
                return "Ошибка обработки документа";
            }

            // прочитать номер счета из hidden-поля accountId...
            // если номер заполнен - это - портфель роботов
            if (hiddenFields.ContainsKey("accountId") && xmlDocs.Count == 1)
            {
                return UploadRobotPortfolio(hiddenFields["accountId"], xmlDocs.First().Value);
            }
            return string.Empty;
        }

        /// <summary>
        /// загрузить из XML портфель роботов
        /// </summary>        
        private string UploadRobotPortfolio(string accIdStr, XmlDocument doc)
        {
            var accId = accIdStr.ToIntSafe() ?? 0;
            if (accId == 0)
            {
                RobotFarm.Instance.AppendLogMessage("Загрузка портфеля - не прочитан номер счета (" + accIdStr + ")");
                return "Не прочитан номер счета";
            }
            var count = RobotFarm.Instance.LoadRobotsFromXml(doc, accId);
            return "Прочитано " + count + " роботов";
        }

        private void RenderFarmPage(StringBuilder sb, HttpListenerContext context)
        {
            sb.AppendLine();
            // отображаемый (в правой части) счет
            var selectedAccount = 0;
            if (context.Request.QueryString.AllKeys.Contains("account"))
                selectedAccount = context.Request.QueryString["account"].ToIntSafe() ?? 0;
            var accounts = RobotFarm.Instance.Accounts;
            if (selectedAccount == 0 && accounts.Count > 0)
                selectedAccount = accounts[0].AccountId;

            // в левой части окна вывести счета
            RenderTableOpenTag(sb);
            RenderTableRowTag(sb, false);
            RenderAjaxCloseOrdersQuery(sb);
            sb.AppendLine("    <td>");
            sb.AppendLine("      <ul>");
            foreach (var ac in RobotFarm.Instance.Accounts)
            {
                if (ac.AccountId == selectedAccount)
                    sb.AppendLine("      <li style=\"color:White;font-weight:bold;background-color:Silver\">");
                else
                    sb.AppendLine("      <li><a href=\"?account=" + ac.AccountId + "\">");

                sb.Append("Счет #" + ac.AccountId + " /" + ac.UserLogin);
                if (ac.AccountId != selectedAccount) sb.Append("</a>");
                sb.Append("</li>");
            }
            sb.AppendLine("      </ul>");
            sb.AppendLine("      <br/><br/>");

            // кнопка Добавить счет
            sb.AppendLine("    <a href=\"?editAccountsTable=brows\">Счета...</a>");
            sb.AppendLine("    </td>");

            // в правой части показать роботов, нацеленных на счет
            // а также средства по счету
            sb.AppendLine("    <td style=\"min-width:250px; vertical-align: top;\">");

            var account = accounts.FirstOrDefault(a => a.AccountId == selectedAccount);
            if (account != null)
            {
                // состояние счета
                List<MarketOrder> orders;
                var accountInfo = GetAccountInfoFromServer(account.AccountId, out orders);
                var positionsOpened = orders == null ? 0 : orders.Count;
                sb.AppendLine("      <div style=\"color:White;background-color:Silver;padding:8px;\">");
                sb.AppendLine("        <p>");
                sb.AppendLine("          <b>Торговля по счету <a href=\"#\" id=\"linkSwitchAccountEnabled\" onclick=\"switchAccountEnabled(" +
                    account.AccountId + ")\">" +
                    (account.TradeEnabled ? "разрешена" : "запрещена")
                    + "</a></b></br>");
                sb.AppendLine("          Счет #" + account.AccountId + ", группа " + accountInfo.Group + "</br>");
                sb.AppendLine("          Текущий баланс: " + accountInfo.Equity.ToStringUniformMoneyFormat() + " "
                    + accountInfo.Currency + "</br>");
                sb.AppendLine("          Текущая прибыль/убыток: " +
                    (accountInfo.Equity - accountInfo.Balance).ToStringUniformMoneyFormat() + " "
                    + accountInfo.Currency + "</br>");
                sb.AppendLine("          <a href=\"?openedPositionsByAccount=" + account.AccountId +
                    "\">Позиций открыто: " + positionsOpened + "</a>" +
                    "&nbsp;&nbsp;<a href=\"#\" onclick=\"" + RequestCloseDealsByAccount + "(" + account.AccountId + ");\"\">Закрыть все</a>" +
                    "<br/><br/> " +
                    "<a href=\"?subscriptionsByAccount=" + account.AccountId + "\">Портфель пользователя</a><br/>" +
                    "<a href=\"?updatePortfolio=" + account.AccountId + "\">Обновить портфель</a>");
                sb.AppendLine("        </p>");
                sb.AppendLine("      </div>");

                // запущенные роботы
                sb.AppendLine("      <h2>Торговые роботы</h2>");
                sb.AppendLine("      <div style=\"color:Black;background-color:White;padding:8px;\">");
                RenderTableOpenTag(sb);
                RenderTableRowTag(sb, true);
                sb.AppendLine("          <td>Название</td><td>Графики</td><td>Magic</td><td>Удалить</td>");
                sb.AppendLine("        </tr>");
                var robotIndex = 0;
                foreach (var robot in account.Robots)
                {
                    RenderTableRowTag(sb, false);
                    sb.AppendLine(
                        // тип робота
                            "          <td>" + robot.TypeName +
                            "</td><td>" +
                        // таймфрейм
                            string.Join(", ", robot.Graphics.Select(g => string.Format("{0}: {1}", g.a,
                                BarSettingsStorage.Instance.GetBarSettingsFriendlyName(g.b)))) +
                        // удалить робота
                            "</td><td>" + robot.Magic + "</td> <td><a href=\"#\" onclick=\"" +
                            "if (!confirm('Удалить робота " + robot.GetUniqueName() +
                            "?')) return;" +
                            " ajaxFunction('removeRobot=" + (robotIndex++) +
                            "&forAccount=" + account.AccountId +
                            "', function (rst) {" +
                            " showMessage('messageDiv', 'Робот " + robot.GetUniqueName() +
                            " удален', 1000); }" +
                            ");\">Удалить</a></td>");

                    sb.AppendLine("       </tr>");
                }
                sb.AppendLine("      </table>");

                sb.AppendLine("      <br/>");
                sb.AppendLine("    <form method=\"POST\" enctype=\"multipart/form-data\">");
                sb.AppendLine("      <p> Загрузить портфель роботов</p>");
                sb.AppendLine("      <input name=\"uploadedFile\" type=\"file\" /> <br/>");
                sb.AppendLine("      <input type=\"submit\" value=\"Загрузить\" />");
                sb.AppendLine("      <input type=\"hidden\" name=\"accountId\" value=\"" + account.AccountId + "\" />");
                sb.AppendLine("    </form>");

                sb.AppendLine("      </div>");
            }

            sb.AppendLine("    </td>");
        }

        private void RenderAccountPositions(StringBuilder sb, int accountId)
        {
            List<MarketOrder> orders;
            var accountInfo = GetAccountInfoFromServer(accountId, out orders);
            if (accountInfo == null) return;

            var tradeSignalRequestScript = RenderAjaxSignalDetailQuery(sb);

            // краткая сводка по счету
            sb.AppendLine("      <h3>Счет " + accountId + "</h3>");
            sb.AppendLine("      <p>");
            var ptrVal = new Dictionary<string, string>
                {
                    {"Баланс", 
                        accountInfo.Equity.ToStringUniformMoneyFormat() + " " + accountInfo.Currency },
                    {"Текущая прибыль/убыток", 
                        (accountInfo.Equity - accountInfo.Balance).ToStringUniformMoneyFormat() + " " + accountInfo.Currency },
                    {"Группа", accountInfo.Group}                    
                };
            foreach (var pair in ptrVal)
            {
                sb.AppendLine("        " + pair.Key + ": " + pair.Value + "<br/>");
            }
            sb.AppendLine("      </p>");

            // рендерить таблицу открытых позиций
            RenderTableOpenTag(sb);
            RenderTableRowTag(sb, true);
            sb.AppendLine("        <td>№</td><td>Тип</td><td>Объем</td><td>Инстр.</td>" +
                "<td>Цена</td><td>Время</td><td>Выход</td><td>Результат</td><td>SL</td><td>TP</td><td>Сиг.</td></tr>");

            var evenRow = false;
            var colorSchemeText = new Dictionary<Cortege2<bool, bool>, string>
                {
                    {new Cortege2<bool, bool>(true, true), "Black"}, // even - profit
                    {new Cortege2<bool, bool>(false, true), "Black"}, // odd - profit
                    {new Cortege2<bool, bool>(false, false), "Black"}, // odd - loss
                    {new Cortege2<bool, bool>(true, false), "#860000"} // even - loss                    
                };
            var colorSchemeBack = new Dictionary<Cortege2<bool, bool>, string>
                {
                    {new Cortege2<bool, bool>(true, true), "White"}, // even - profit
                    {new Cortege2<bool, bool>(false, true), "#E0E0E0"}, // odd - profit
                    {new Cortege2<bool, bool>(false, false), "FFD6D6"}, // odd - loss
                    {new Cortege2<bool, bool>(true, false), "#White"} // even - loss                    
                };

            foreach (var order in orders)
            {
                evenRow = !evenRow;
                var colorText = colorSchemeText[new Cortege2<bool, bool>(evenRow, order.ResultDepo >= 0)];
                var colorBack = colorSchemeBack[new Cortege2<bool, bool>(evenRow, order.ResultDepo >= 0)];

                sb.AppendLine(string.Format("        <tr style=\"color:{0}; background-color:{1}\">",
                        colorText, colorBack));

                int signalCat, parentDeal;
                var signalString = "";
                if (MarketOrder.GetTradeSignalFromDeal(order, out signalCat, out parentDeal))
                    signalString = string.Format("<a href=\"#\" onclick=\"{0}({1})\">[{1}]</a>",
                        tradeSignalRequestScript, signalCat);

                sb.AppendLine(string.Format("        <td>{0}</td><td>{1}</td><td>{2}</td>" +
                    "<td>{3}</td><td>{4}</td><td>{5:dd.MM.yyyy HH:mm:ss}</td><td>{6}</td><td>{7}</td><td>{8}</td><td>{9}</td><td>{10}</td></tr>",
                    order.ID, order.Side > 0 ? "BUY" : "SELL", order.Volume.ToStringUniformMoneyFormat(),
                    order.Symbol, order.PriceEnter.ToStringUniformMoneyFormat(),
                    order.TimeEnter, (order.PriceExit ?? 0).ToStringUniformPriceFormat(),
                    order.ResultDepo.ToStringUniformMoneyFormat(),
                    order.StopLoss.HasValue ? order.StopLoss.Value.ToStringUniformPriceFormat() : " ",
                    order.TakeProfit.HasValue ? order.TakeProfit.Value.ToStringUniformPriceFormat() : " ",
                    signalString));
            }
        }

        private bool GetTradeSignalCategory(HttpListenerContext context)
        {
            var catId = context.Request.QueryString["requestTradeSignalCat"].ToIntSafe();
            if (!catId.HasValue)
            {
                const string answer = "{\"status\": \"Id не указан\"}";
                WriteJsonResponse(context, answer);
                return true;
            }

            string statusStr;
            try
            {
                var stat = TradeSharpAccountStatistics.Instance.proxy.GetPerformerStatBySignalCatId(catId.Value);
                statusStr = string.Format("Счет №{0}, {1} {2}, владелец: {3}, прибыль: {4:f1}%",
                                          stat.Account, stat.Equity.ToStringUniformMoneyFormat(false), stat.DepoCurrency,
                                          stat.Login, stat.Profit);
            }
            catch (Exception ex)
            {
                statusStr = ex.Message;
            }

            WriteJsonResponse(context, "{\"status\": \"" + statusStr + "\"}");
            return true;
        }

        private bool CloseDealsByAccount(HttpListenerContext context)
        {
            var accountIdString = context.Request.QueryString[RequestCloseDealsByAccount];
            if (string.IsNullOrEmpty(accountIdString))
            {
                const string answer = "{\"status\": \"Id счета не указан\"}";
                WriteJsonResponse(context, answer);
                return true;
            }

            var accountId = context.Request.QueryString[RequestCloseDealsByAccount].ToIntSafe();
            if (!accountId.HasValue)
            {
                const string answer = "{\"status\": \"Id счета не распознан\"}";
                WriteJsonResponse(context, answer);
                return true;
            }
            Logger.InfoFormat("CloseDealsByAccount({0})", accountIdString);

            string statusStr;
            List<MarketOrder> orders;
            try
            {
                GetAccountInfoFromServer(accountId.Value, out orders);
                if (orders == null)
                    throw new Exception("список ордеров = null");
            }
            catch (Exception ex)
            {
                WriteJsonResponse(context, "{\"status\": \"" + "ошибка получения сделок: " + ex.Message + "\"}");
                return true;
            }

            FarmAccount account;
            try
            {
                account = RobotFarm.Instance.GetAccountById(accountId.Value);
                if (account == null)
                    throw new Exception("ошибка получения счета");
            }
            catch (Exception ex)
            {
                WriteJsonResponse(context, "{\"status\": \"" + "ошибка получения счета: " + ex.Message + "\"}");
                return true;
            }

            try
            {
                var total = orders.Count;
                var countOk = 0;

                if (account.proxyTrade == null)
                    throw new Exception("account.ProxyTrade = null");
                if (account.proxyTrade == null)
                    throw new Exception("account.ProxyTrade.proxy = null");
                
                foreach (var order in orders)
                {
                    if (account.proxyTrade.proxy.SendCloseRequest(account.protectedContext.MakeProtectedContext(),
                                                                  account.AccountId, order.ID,
                                                                  PositionExitReason.ClosedFromUI) == RequestStatus.OK)
                        countOk++;
                }
                statusStr = "Закрыто " + countOk + " из " + total;
            }
            catch (Exception ex)
            {
                statusStr = "ошибка закрытия ордеров: " + ex.Message;
            }

            WriteJsonResponse(context, "{\"status\": \"" + statusStr + "\"}");
            return true;
        }

        private string RenderAjaxSignalDetailQuery(StringBuilder sb)
        {
            sb.AppendLine("    <script type=\"text/javascript\">");
            sb.AppendLine("       function requestTradeSignalDetail(signalCat) {");
            sb.AppendLine("         var ptrStr = '?requestTradeSignalCat=' + signalCat;");
            sb.AppendLine("         ajaxJsonFunction(ptrStr, null, function (rst) { ");
            sb.AppendLine("             alert(rst.status);");
            sb.AppendLine("         });");
            sb.AppendLine("       }");
            sb.AppendLine("    </script>");
            return "requestTradeSignalDetail";
        }

        private void RenderAjaxCloseOrdersQuery(StringBuilder sb)
        {
            sb.AppendLine("    <script type=\"text/javascript\">");
            sb.AppendLine("       function " + RequestCloseDealsByAccount + "(accountId) {");
            sb.AppendLine("         var ptrStr = '?" + RequestCloseDealsByAccount + "=' + accountId;");
            sb.AppendLine("         ajaxJsonFunction(ptrStr, null, function (rst) { ");
            sb.AppendLine("             alert(rst.status);");
            sb.AppendLine("         });");
            sb.AppendLine("       }");
            sb.AppendLine("    </script>");
        }

        private void RenderAccountSubscriptions(StringBuilder sb, int accountId)
        {
            var farmAccount = RobotFarm.Instance.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (farmAccount == null)
            {
                sb.AppendLine("    <h3>Счет не найден</h3>");
                return;
            }

            sb.AppendLine("    <p>Настройки авто-торговли по сигналам: <a href=\"?portfolioTradeSettings=1\">редактировать</a></p>");

            // получить все подписки пользователя: на торговые сигналы (SUBSCRIPTION)
            // и на портфели
            List<Subscription> subscriptions = null;
            try
            {
                subscriptions = TradeSharpAccount.Instance.proxy.GetSubscriptions(farmAccount.UserLogin);
            }
            catch (Exception ex)
            {
                sb.AppendLine("     <h3>Подписка на торговые сигналы не прочитана: " + ex.Message + "</h3>");
                Logger.Error("GetTradeSignalsSubscribed()", ex);
            }
            if (subscriptions != null && subscriptions.Count > 0)
            {
                RenderTableOpenTag(sb);
                RenderTableRowTag(sb, true);
                sb.AppendLine("        <td>№</td><td>Сигнал</td><td>Торговать</td><td>Объем</td>" +
                    "<td>Макс.плечо</td><td>Мин/шаг</td></tr>");

                foreach (var sig in subscriptions)
                {
                    var tradeSets = sig.AutoTradeSettings ?? new AutoTradeSettings();
                    sb.AppendLine(string.Format(
                        "        <tr><td>{0}</td> <td>{1}</td> <td>{2}</td> <td>{3}</td> <td>{4:f1}</td> <td>{5}</td></tr>",
                        sig.Service,
                        sig.PaidService.Comment,
                        tradeSets.TradeAuto ? "да" : "нет",
                        tradeSets.FixedVolume.HasValue ? tradeSets.FixedVolume.Value.ToStringUniformMoneyFormat() : ((int)tradeSets.PercentLeverage).ToString() + "%",
                        tradeSets.MaxLeverage,
                        (tradeSets.MinVolume.HasValue ? tradeSets.MinVolume.Value.ToStringUniformMoneyFormat() : "-") + "/" +
                        (tradeSets.StepVolume.HasValue ? tradeSets.StepVolume.Value.ToStringUniformMoneyFormat() : "-")));
                }
                sb.AppendLine("      </table>");
            }

            // подписка пользователя на портфель
            TopPortfolio portfolio = null;
            AccountEfficiency efficiency;// = null;
            try
            {
                var portfolioId = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolioId(farmAccount.UserLogin);
                if (portfolioId > 0)
                    portfolio = TradeSharpAccountStatistics.Instance.proxy.GetTopPortfolio(portfolioId, out efficiency);
            }
            catch (Exception ex)
            {
                sb.AppendLine("     <h3>Подписка на портфель не прочитана: " + ex.Message + "</h3>");
                Logger.Error("GetSubscribedTopPortfolio()", ex);
            }

            if (portfolio != null)
            {
                sb.AppendLine("    <br/> <p>");
                sb.AppendLine(string.Format("      Подписан на портфель - ТОП {0} {1} <br/>",
                    portfolio.ParticipantCount, portfolio.Name));
                sb.AppendLine(string.Format("      {0} управляющих в портфеле<br/>", portfolio.Managers.Count));
                sb.AppendLine("    </p>");
            }
        }

        private static string UpdatePortfolio(HttpListenerContext context, out string action)
        {
            action = string.Empty;

            var accountId = context.Request.QueryString["updatePortfolio"].ToIntSafe() ?? 0;
            var farmAccount = RobotFarm.Instance.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (farmAccount == null)
                return "Счет не найден";

            // принудительно обновить счет
            farmAccount.UpdatePortfolio();
            return "Портфель обновлен";
        }

        private void RenderFarmPageMenu(StringBuilder sb, HttpListenerContext context)
        {
            // домой
            sb.AppendLine("<table border=\"0\" cellspacing=\"2\"> <tr> <td>");
            sb.Append("<a href=\"?\"><img src=\"");
            RenderImgBytesSrc(sb, ImgRsxHome);
            sb.Append("\" title=\"Главная\" /></a> </td>");

            // количество счетов, с непустым портфелем роботов, разрешенных к торговле
            sb.AppendFormat("<td>&nbsp;&nbsp;</td><td id=\"activeAccountsCountCell\" style=\"border-width:1px;\">{0}</td>",
                GetTradableAccountsCount());

            // текущее состояние формы
            var farmState = RobotFarm.Instance.State;
            var imgStateRsx = farmState == FarmState.Started ? ImgRsxPause
                : farmState == FarmState.Starting ? ImgRsxStarting
                : farmState == FarmState.Stopping ? ImgRsxStopping
                : ImgRsxPlay;

            sb.Append(" <td><a href=\"#\" onclick=\"ajaxFunction('startStopFarm=1', processStartStopResult);\"><img src=\"");
            RenderImgBytesSrc(sb, imgStateRsx);
            sb.Append("\" title=\"Старт / стоп\" /></a></td><td>&nbsp;&nbsp;</td>");

            //sb.Append(" <td>");
            //AjaxHyperlinkHelper.AjaxHyperlink(sb, builder => ImgHelper.Img(sb, imgStateRsx), "startStopFarm=1", "processStartStopResult");
            //sb.Append("</td><td>&nbsp;&nbsp;</td>");

            // активность роботов
            sb.Append("<td><a href=\"?trades=0\"><img src=\"");
            RenderImgBytesSrc(sb, ImgRsxActivityTable);
            sb.Append("\" title=\"Активность роботов\" /></a> </td>");
            
            // лог
            sb.Append("<td><a href=\"?log=1\"><img src=\"");
            RenderImgBytesSrc(sb, ImgRsxLog);
            sb.Append("\" title=\"Журнал\" /></a> </td>");

            // сохранение настроек
            sb.Append(" <td><a href=\"#\" title=\"Со\" onclick=\"if (!confirm('Сохранить настройки?')) return;"
                + " ajaxFunction('saveSettings=1', function (rst) {" +
                " showMessage('messageDiv', 'Настройки сохранены', 1000); }" +
                ");\"><img src=\"");
            RenderImgBytesSrc(sb, ImgRsxSave);
            sb.Append("\" /></a></td>");

            // сохранить как
            sb.Append("<td><a href=\"?saveas=1\" target=\"_blank\"><img src=\"");
            RenderImgBytesSrc(sb, ImgRsxSaveAs);
            sb.Append("\" /></a> </td> ");

            sb.AppendLine("</tr></table>");
        }

        private int GetTradableAccountsCount()
        {
            var accounts = RobotFarm.Instance.Accounts;
            return accounts.Count(a => a.TradeEnabled && a.Robots.Count > 0);
        }

        private void RenderServerLog(StringBuilder sb)
        {
            sb.AppendLine("  <p>");

            bool timeout;
            var messages = RobotFarm.Instance.logMessages.GetAll(1000, out timeout);
            if (timeout)
                sb.AppendLine("  <b>Ошибка получения сообщений: таймаут</b>");
            else
                foreach (var msg in messages)
                    sb.AppendLine("     " + msg + "<br/>\r\n");
            sb.AppendLine("  </p>");
        }

        private void RenderTrades(StringBuilder sb, int daysCount)
        {
            new TradeReportBuilder(daysCount).BuildReport(sb);            
        }

        /// <summary>
        /// в ответе будут содержаться форматированные в JSON данные
        /// </summary>
        private void ProcessJsonRequest(HttpListenerContext context)
        {

        }

        private Account GetAccountInfoFromServer(int accountId, out List<MarketOrder> orders)
        {
            orders = new List<MarketOrder>();
            var farmAccount = RobotFarm.Instance.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (farmAccount == null) return new Account();

            RobotFarmProxy proxy;
            try
            {
                proxy = new RobotFarmProxy(TerminalBindings.RobotFarm);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения информации по счету от сервера (создание proxy):", ex);
                return new Account();
            }

            // получить данные от сервера
            Account account;
            try
            {
                proxy.GetAccountData(farmAccount.UserLogin, farmAccount.UserPassword,
                                     accountId, out account, out orders);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения информации по счету от сервера (обращение к proxy):", ex);
                return new Account();
            }
            if (account == null) return new Account();

            // посчитать профит по позициям
            CalculateAccountEquity(account, orders);

            return account;
        }

        private static void CalculateAccountEquity(Account account, List<MarketOrder> orders)
        {
            var quotes = QuoteStorage.Instance.ReceiveAllData();
            if (quotes.Count == 0) return;

            var profit = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(orders, quotes, account.Currency);
            account.Equity = account.Balance + (decimal)profit;
        }

        private static string GetScripts()
        {
            return "   function switchAccountEnabled(accountId) {\r\n" +
                   "      ajaxFunction('switchAccountEnabled=' + accountId);\r\n" +
                   "   }\r\n\r\n" +

                   "   processAjaxResult = function(rst){\r\n" +
                   "      var jsonObj = JSON.parse(rst);\r\n" +
                   "      var link = document.getElementById('linkSwitchAccountEnabled');\r\n" +
                   "      if (jsonObj.enabled) link.innerHTML = 'разрешена';\r\n" +
                   "      else link.innerHTML = 'запрещена';\r\n" +
                   "      var cellObj = document.getElementById('activeAccountsCountCell');\r\n" +
                   "      cellObj.innerHTML = jsonObj.countEnabled; }\r\n\r\n" +

                   "   function processStartStopResult(rst){\r\n" +
                   "      var jsonObj = JSON.parse(rst);\r\n" +
                   "      showMessage('messageDiv', 'Статус: ' + jsonObj.state, 1500); } \r\n" +


            "function ajaxJsonFunction(requestParams, jsonData2send, callBack) { \r\n" +
            "    $.ajax({ \r\n" +
            "       type: 'POST', \r\n" +
            "       contentType: 'application/json; charset=utf-8;', \r\n" +
            "       url: requestParams, \r\n" +
            "       data: jsonData2send, \r\n" +
            "       dataType: 'json', \r\n" +
            "       success: function (data, textStatus, jqXHR) { \r\n" +
            "         callBack(data); \r\n" +
            "       }, \r\n" +
            "         error: function (jqXHR, textStatus, errorThrown) { \r\n" +
            "            alert(textStatus); \r\n alert(errorThrown); \r\n " +
            "       } \r\n" +
            "   });" +
            "}";
        }

        private static void RenderImgBytesSrc(StringBuilder sb, string imgRsxPath)
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(imgRsxPath))
                using (var bmp = new Bitmap(stream))
                    sb.Append(MakeEmbeddedPictureString(bmp));
            }
            catch (Exception ex)
            {
                Logger.Error("RenderImgBytesSrc", ex);
                return;
            }
        }

        private static string RenderImgBytesSrc(string imgRsxPath)
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(imgRsxPath))
                {
                    using (var bmp = new Bitmap(stream))
                        return MakeEmbeddedPictureString(bmp);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RenderImgBytesSrc", ex);
                return string.Empty;
            }
        }

        private static void WriteTextResponse(HttpListenerContext context, string text)
        {
            var resp = Encoding.UTF8.GetBytes(text);
            context.Response.ContentLength64 = resp.Length;
            context.Response.OutputStream.Write(resp, 0, resp.Length);
        }

        private static void WriteJsonResponse(HttpListenerContext context, string text)
        {
            var resp = Encoding.UTF8.GetBytes(text);
            context.Response.ContentLength64 = resp.Length;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Content-type", "text/json");
            context.Response.Headers.Add("Content-type", "application/json");
            context.Response.OutputStream.Write(resp, 0, resp.Length);
        }
    }
}