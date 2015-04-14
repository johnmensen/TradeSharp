using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.WebContract;
using TradeSharp.Localisation;
using TradeSharp.Server.Contract;
using TradeSharp.Server.Repository;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    class WebServer : BaseWebServer
    {
        private static WebServer instance;

        public static WebServer Instance
        {
            get { return instance ?? (instance = new WebServer()); }
        }

        private IAccountRepository accountRepository;

        private WebServer()
        {
            accountRepository = AccountRepository.Instance;
        }

        public override string ServiceName
        {
            get { return "TRADE# Server"; }
        }

        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            string resp;

            // параметры запроса переданы в формате JSON
            // вероятно, на выходе требуется также JSON
            if (context.Request.QueryString.AllKeys.Contains("formatquery"))
            {
                resp = ProcessFormattedHttpRequest(context);
            }
            // выполнить действия, определенные в запросе, рендерить страницу не нужно
            else if (context.Request.QueryString.HasKeys() && ProcessQueryString(context.Request.QueryString, out resp))
            {
                if (string.IsNullOrEmpty(resp))
                    resp = "<html></html>";
            }
            // рендерить дефолтную страницу
            else
            {
                var sb = new StringBuilder();
                RenderHttpHead(sb,
                                    "", // стили
                                    "", // скрипты
                                    true);
                sb.AppendLine("<body style=\"background-color:#303030;margin:0px;padding-top:10px\">");
                RenderPopupDiv(sb, 200, 50, "messageDiv");
                sb.AppendLine("  <div style=\"background-color:White;width:800px;margin: auto;text-align: left;padding:10px\">");
                
                // тело:
                if (context.Request.QueryString.AllKeys.Contains("help"))
                {
                    RenderSupportedQueries(sb);
                }
                else
                {
                    // default page:
                    sb.AppendLine("    <a href=\"?help=1\">Справка</a> <br/>");
                    // состояние сервиса
                    RenderServiceState(sb);
                    // подключенные клиенты
                    RenderUserSessions(sb);
                }

                // закрыть страницу
                sb.AppendLine("  </div>");
                RenderBodyCloseTag(sb);
                RenderHttpCloseTag(sb);

                resp = sb.ToString();
            }

            WriteResponse(resp, context);
        }

        private string ProcessFormattedHttpRequest(HttpListenerContext context)
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                var text = reader.ReadToEnd();
                if (string.IsNullOrEmpty(text))
                    return HttpParameter.SerializeInJSon(new List<HttpParameter> { new ExecutionReport { IsOk = false, Comment = "Список параметров (JSON) пуст" } });
                var ptrs = HttpParameter.DeserializeFromJSon(text);
                if (ptrs.Count == 0)
                    return HttpParameter.SerializeInJSon(new List<HttpParameter> { new ExecutionReport { IsOk = false, Comment = "Список параметров (JSON) - ошибка десериализации" } });

                if (context.Request.QueryString.AllKeys.Contains("register"))                
                    return ProcessFormattedRequestRegister(ptrs);
                if (context.Request.QueryString.AllKeys.Contains("balance"))
                    return ProcessFormattedRequestBalance(ptrs);
            }

            return HttpParameter.SerializeInJSon(new List<HttpParameter>
                        {
                            new ExecutionReport { IsOk = false, Comment = "Запрос не поддерживается" }
                        });
        }

        private string ProcessFormattedRequestBalance(List<HttpParameter> ptrs)
        {
            if (ptrs.Count != 1 || ptrs[0] is ChangeAccountBalanceQuery == false)
                return HttpParameter.SerializeInJSon(new List<HttpParameter>
                    {
                        new ExecutionReport
                            {
                                IsOk = false,
                                Comment = "Список параметров (JSON) - ожидается один параметр типа ChangeAccountBalanceQuery"
                            }
                    });
            var queryPtr = (ChangeAccountBalanceQuery)ptrs[0];

            // пополнить счет или произвести списания
            var status = accountRepository.ChangeBalance(queryPtr.AccountId,
                                                        queryPtr.Amount, queryPtr.Description, queryPtr.ValueDate,
                                                        queryPtr.ChangeType);

            return HttpParameter.SerializeInJSon(new List<HttpParameter>
            {
                new ExecutionReport
                    {
                        IsOk = status == RequestStatus.OK,
                        Comment = EnumFriendlyName<RequestStatus>.GetString(status)
                    }
            });            
        }

        private static string ProcessFormattedRequestRegister(List<HttpParameter> ptrs)
        {
            if (ptrs.Count != 1 || ptrs[0] is RegisterAccountQuery == false)
                return HttpParameter.SerializeInJSon(new List<HttpParameter>
                    {
                        new ExecutionReport
                            {
                                IsOk = false,
                                Comment = "Список параметров (JSON) - ожидается один параметр типа RegisterAccountQuery"
                            }
                    });
            var queryPtr = (RegisterAccountQuery) ptrs[0];
            var status = ManagerAccount.Instance.RegisterAccount(
                new PlatformUser
                    {
                        Login = queryPtr.UserLogin,
                        Password = queryPtr.UserPassword,
                        Name = queryPtr.UserName,
                        Surname = queryPtr.UserSurname,
                        Patronym = queryPtr.UserPatronym,
                        Description = queryPtr.UserDescription,
                        Title = queryPtr.UserLogin,
                        Email = queryPtr.UserEmail,
                        Phone1 = queryPtr.UserPhone1,
                        Phone2 = queryPtr.UserPhone2,
                        RegistrationDate = DateTime.Now,
                        RightsMask = queryPtr.UserRightsMask,
                        RoleMask = queryPtr.UserRoleMask
                    },
                queryPtr.Currency,
                (int) queryPtr.Balance,
                (decimal) queryPtr.MaxLeverage,
                queryPtr.UserPassword, // пароль задан заранее, автоматом не сочиняем
                false); // не подписывать автоматом на торговые сигналы лучших в мире трейдеров
            return HttpParameter.SerializeInJSon(new List<HttpParameter>
                {
                    new ExecutionReport
                        {
                            IsOk = status == AccountRegistrationStatus.OK,
                            Comment = EnumFriendlyName<AccountRegistrationStatus>.GetString(status)
                        }
                });
        }

        private static void RenderSupportedQueries(StringBuilder sb)
        {
            sb.AppendLine("    <a href=\"?\">На главную</a>");
            sb.AppendLine("    <br/>");
            sb.AppendLine("    <h2>Сервер обрабатывает следующие запросы:</h2>");

            var samples = new Dictionary<HttpParameter, string>
                {
                    {new RegisterAccountQuery
                        {
                            Balance = 25000,
                            Currency = "USD",
                            Group = "Demo",
                            MaxLeverage = 100.5f,
                            UserDescription = "UserTitle",
                            UserEmail = "email@user.org",
                            UserLogin = "UserLogin",
                            UserName = "User Name",
                            UserPassword = "UserPassword",
                            UserPatronym = "User Patronym",
                            UserPhone1 = "8 800 600 50 60",
                            UserPhone2 = "8 800 600 50 61",
                            UserRightsMask = UserAccountRights.Trade,
                            UserRoleMask = UserRole.Trader,
                            UserSurname = "User Surname"
                        }, "<h3>Регистрация пользователя / счета</h3> http://serverurl:port?formatquery=1&amp;register=1, HTTP POST"},
                        { new ChangeAccountBalanceQuery
                            {
                                AccountId = 3,
                                ChangeType = BalanceChangeType.Deposit,
                                Amount = 0.15M,
                                ValueDate = DateTime.Now
                            }, 
                            "<h3>Пополнение/списание со счета</h3> http://serverurl:port?formatquery=1&amp;balance=1, HTTP POST"}
                };

            foreach (var sample in samples)
            {
                sb.AppendLine("    " + sample.Value);
                sb.AppendLine("    <br/> <b>Содержание запроса - JSON-массив вида</b> </br>");
                sb.AppendLine("    <p style=\"width:90%\">");
                sb.Append(HttpUtility.HtmlEncode(HttpParameter.SerializeInJSon(
                    new List<HttpParameter> {sample.Key})));
                sb.AppendLine("</p>");
                // допустимые значения перечислений
                foreach (var pi in sample.Key.GetType().GetProperties())
                {
                    if (pi.PropertyType.IsEnum)
                        sb.AppendLine("    <span>" + MakeEnabledPropertyValuesString(pi) + "</span> <br/>");
                }
            }
        }

        private static string MakeEnabledPropertyValuesString(PropertyInfo pi)
        {
            var values = string.Join(", ", Enum.GetNames(pi.PropertyType));
            return "<b>" + pi.Name + "</b>: " + values;
        }

        //private static string FormatPropertyValue(PropertyInfo pi, object sample)
        //{
        //    var val = pi.GetValue(sample, null);
        //    if (val == null) return "null";
        //    if (pi.PropertyType == typeof (DateTime) ||
        //        Nullable.GetUnderlyingType(pi.PropertyType) == typeof (DateTime))
        //        return "'\\/Date(" + (long)((DateTime)val - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + "+"
        //        + TimeZoneInfo.Local.BaseUtcOffset.Hours.ToString("D2")
        //        + TimeZoneInfo.Local.BaseUtcOffset.Minutes.ToString("D2") + ")\\/'".Replace("+-", "-");

        //    var omitQuotes = IsNumericType(pi.PropertyType);
            
        //    return omitQuotes 
        //        ? Converter.GetStringFromObject(val)
        //        : "'" + Converter.GetStringFromObject(val) + "'";
        //}

        //private static readonly HashSet<Type> numericTypes = new HashSet<Type>
        //{
        //    typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
        //};

        //internal static bool IsNumericType(Type type)
        //{
        //    return numericTypes.Contains(type) ||
        //           numericTypes.Contains(Nullable.GetUnderlyingType(type));
        //}

        
        /// <summary>
        /// вернуть true если дальнейшая обработка запроса не требуется
        /// </summary>        
        private static bool ProcessQueryString(NameValueCollection query, out string resp)
        {
            resp = string.Empty;

            if (query.AllKeys.Contains("reset"))
            {
                ResetServiceState();
                return false;
            }

            // вернуть сессии пользователей
            if (query.AllKeys.Contains("current_session"))
            {
                var sessions = UserSessionStorage.Instance.GetSessions();
                var userInfos = sessions.Select(s => new TerminalUser
                {
                    IP = s.ip,
                    Login = s.login,
                    Account = s.accountId
                }).Cast<HttpParameter>().ToList();

                resp = HttpParameter.SerializeInJSon(userInfos);
                return true;
            }

            if (query.AllKeys.Contains("sendMessage") &&
                query.AllKeys.Contains("terminal"))
            {
                var msg = query["sendMessage"];
                var terminal = query["terminal"].ToLongSafe() ?? 0;

                Logger.InfoFormat("Command send message (\"{0}\" to {1})",
                    msg, terminal);
                SendMessageToTerminal(msg, terminal);
                return true;
            }

            if (query.AllKeys.Contains("logoutUser"))
            {
                var terminal = query["logoutUser"].ToLongSafe() ?? 0;

                Logger.InfoFormat("Logout user {0}",
                    terminal);
                UserSessionStorage.Instance.ExcludeStaleSessions(new List<long> { terminal });
                return true;
            }

            return false;
        }

        private static void ResetServiceState()
        {
            ModuleStatusController.Instance.ResetStatus();
        }

        private static void RenderServiceState(StringBuilder sb)
        {
            var stateCode = ModuleStatusController.Instance.Status.State;
            var stateCodeTitle = stateCode == ServiceProcessState.OK
                                     ? "исправен"
                                     : stateCode == ServiceProcessState.HasWarnings
                                           ? "есть предупреждения"
                                           : stateCode == ServiceProcessState.HasErrors ? "есть ошибки" : "критическая ошибка";
            sb.Append("<p>Состояние сервиса: ");
            sb.Append(stateCodeTitle);

            var error = ModuleStatusController.Instance.LastError;
            if (!string.IsNullOrEmpty(error))
            {
                sb.Append("<br/>Ошибка: ");
                sb.Append(error);
            }
            sb.Append("</p>");

            sb.Append("<br/><br/><table>");

            var lastLogTime = ModuleStatusController.Instance.lastLoginTime.Value;
            if (lastLogTime.HasValue) AddEventRow("Последнее подключение",
                lastLogTime.Value.ToString("ddd HH:mm:ss"), sb);

            var exString = ModuleStatusController.Instance.GetModuleExtendedStatusString();
            if (!string.IsNullOrEmpty(exString))
            {
                sb.Append(exString.Replace(Environment.NewLine, "<br/>"));
                sb.AppendLine("</br>");
            }

            var lastQuoteTime = ModuleStatusController.Instance.lastQuoteTime.Value;
            AddEventRow("Последняя котировка", lastQuoteTime.HasValue
                            ? ConvertShortTimeSpanToString(DateTime.Now - lastQuoteTime.Value) : " - ", sb);

            var lastMsgTime = ModuleStatusController.Instance.lastProviderMessageTime.Value;
            if (lastMsgTime.HasValue) AddEventRow("Последнее сообщение брокера",
                lastMsgTime.Value.ToString("ddd HH:mm:ss"), sb);
            sb.Append("</table>");
            sb.AppendLine("</br>");
            sb.AppendLine("</br>");
            sb.AppendLine("<a style=\"margin:4px;background: #bfbfb6;font-weight:bolder\" href='?reset=1'>");
            sb.Append("Сбросить ошибки</a>");
        }

        private static void RenderUserSessions(StringBuilder sb)
        {
            try
            {
                sb.AppendLine("      <h3>Текущие сеансы пользователей</h3>");
                sb.AppendLine("    <a href=\"#\" onclick=\"$('table#tabSess').toggle();\">Пользователи</a>");
                BasicServicePageRenderer.RenderTableHeader(sb, "tabSess", "style=\"display:none\"");
                BasicServicePageRenderer.RenderRowOpen(sb, true, string.Empty);
                // заголовок таблицы
                sb.AppendLine("        <td>логин</td> <td>адрес</td> <td>терминал</td> <td>версия</td> <td>счет</td>" +
                              " <td>время входа</td> <td>послед. обращение</td> <td>сообщение</td><td>Отключить</td>");
                BasicServicePageRenderer.RenderRowClose(sb);
                // формировать таблицу
                var sessions = UserSessionStorage.Instance.GetSessions();
                var rowNumber = 0;
                foreach (var session in sessions)
                {
                    rowNumber++;
                    BasicServicePageRenderer.RenderRowOpen(sb, false, string.Empty);
                    sb.AppendLine(string.Format("        <td>{0}</td>", session.login));
                    sb.AppendLine(string.Format("        <td>{0}</td>",
                        session.ip.StartsWith("192.168.") ? session.ip + " (VPN)" : session.ip));
                    sb.AppendLine(string.Format("        <td>{0}</td>", session.terminalId));
                    sb.AppendLine(string.Format("        <td>{0}</td>", session.terminalVersion));
                    sb.AppendLine(string.Format("        <td>{0}</td>", session.accountId));
                    sb.AppendLine(string.Format("        <td>{0:dd.MMM HH:mm:ss}</td>", session.loginTime));
                    sb.AppendLine(string.Format("        <td>{0}</td>", TickTimeToTimeStrSafe(
                        session.lastRequestClientTime, "dd.MMM HH:mm:ss")));
                    // персональное сообщение
                    sb.AppendLine(string.Format(
                        "        <td><input type=\"text\" style=\"width:130px\" id=\"msgField{0}\" />", rowNumber));
                    sb.AppendFormat("<a href=\"#\" onclick=\"var msgText = document.getElementById('msgField{0}').value;", rowNumber);
                    sb.AppendFormat(" ajaxFunction('sendMessage=' + encodeURIComponent(msgText) + '&terminal=' + encodeURIComponent('{0}'));\"",
                        session.terminalId);
                    sb.Append(">отправить</a></td>");
                    // отключить пользователя
                    sb.AppendFormat("<td><a href=\"#\" onclick=\"ajaxFunction('logoutUser=' + encodeURIComponent('{0}'));\">", session.terminalId);
                    sb.Append("отключить</a></td>");

                    BasicServicePageRenderer.RenderRowClose(sb);
                }
                BasicServicePageRenderer.RenderRowClose(sb);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в RenderUserSessions", ex);
            }
        }

        private static string TickTimeToTimeStrSafe(long ticks, string timeFormat)
        {
            if (string.IsNullOrEmpty(timeFormat)) timeFormat = "dd.MM.yyyy HH:mm:ss.fff";
            var str = ticks.ToString();
            try
            {
                var time = new DateTime(ticks);
                str = time.ToString(timeFormat);
            }
            catch { }
            return str;
        }

        private static void AddEventRow(string title, string eventStr, StringBuilder sb)
        {
            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", title, eventStr);
        }

        private static string ConvertShortTimeSpanToString(TimeSpan span)
        {
            if (span.TotalMinutes > 1)
                return string.Format("{0}м {1}c",
                                     (int)span.TotalMinutes, span.Seconds);
            return string.Format("{0}c", (int)span.TotalSeconds);
        }

        private static void SendMessageToTerminal(string msg, long terminalId)
        {
            try
            {
                UserSessionStorage.Instance.SendMessageToUser(terminalId,
                    msg, ServiceMessageCategory.DialogBox);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка отправки сообщения на терминал {0}: {1}",
                    terminalId, ex);
            }
        }
    }
}
