using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using FastGrid;
using TradeSharp.RobotFarm.BL.HtmlHelper;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm.BL.Web
{
    partial class WebServer
    {
        private const string ImgRsxAccept = "TradeSharp.RobotFarm.img.ico_accept.png";
        private const string ImgRsxDelete = "TradeSharp.RobotFarm.img.ico_delete.png";
        private const string ImgRsxAdd = "TradeSharp.RobotFarm.img.ico_add.png";

        /// <summary>
        /// htmlHalper
        /// </summary>
        private FastGridTableHelper fastGridTable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb">ссылка на StringBuilder "накапливающий" разметку</param>
        private void RenderAccountEditTable(StringBuilder sb)
        {
            sb.AppendLine("      <h2>счета пользователей</h2>");
            sb.AppendLine("      <div style=\"color:Black;background-color:White;padding:8px;\">");

            fastGridTable = new FastGridTableHelper("AccountEditTable", CreatingTablePrototype(), true);
            fastGridTable.additionalAttribute.Add(HtmlAttribute.Class, "lightTable");

            sb.Append(fastGridTable.GetHtmlMarkup(RobotFarm.Instance.Accounts));
        }

        /// <summary>
        /// Сформировать таблицу FastGrid, по которой будем делать html разметку
        /// </summary>
        /// <returns></returns>
        private FastGrid.FastGrid CreatingTablePrototype()
        {
            var result = new FastGrid.FastGrid();
            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            result.Columns.Add(new FastColumn("AccountId", "№ счета")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var inputHelper = new InputHelper(HtmlInputType.Text);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Id, "txtAccountId" + accountId);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Name, "txtAccountId" + accountId);   
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Style, "width: 70px;");
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Value, accountId);
                            return inputHelper.GetHtmlMarkup();
                        }
                });

            result.Columns.Add(new FastColumn("UserLogin", "Логин")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var inputHelper = new InputHelper(HtmlInputType.Text);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Id, "txtUserLogin" + accountId);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Name, "txtUserLogin" + accountId);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Style, "width: 140px;");
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Value, row.UserLogin);
                            return inputHelper.GetHtmlMarkup();
                        }
                });

            result.Columns.Add(new FastColumn("UserPassword", "Пароль")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var inputHelper = new InputHelper(HtmlInputType.Password);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Id, "txtUserPassword" + accountId);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Name, "txtUserPassword" + accountId);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Style, "width: 140px;");
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Value, row.UserPassword);
                            return inputHelper.GetHtmlMarkup();
                        }
                });

            result.Columns.Add(new FastColumn("TradeEnabled", "Торговать")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var inputHelper = new InputHelper(HtmlInputType.Checkbox);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Id, "txtTradeEnabled" + accountId);
                            inputHelper.additionalAttribute.Add(HtmlAttribute.Name, "txtTradeEnabled" + accountId);
                            if (row.TradeEnabled) inputHelper.additionalAttribute.Add(HtmlAttribute.Checked, "checked");
                            return inputHelper.GetHtmlMarkup();
                        }
                });

            result.Columns.Add(new FastColumn("TradeEnabled", "Проверить")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var linkEdit = new HyperlinkHelper();
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Id, "linkCheckAccount" + accountId);
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Name, "linkCheckAccount" + accountId);
                            linkEdit.additionalAttribute.Add(HtmlAttribute.OnClick, "var paramString = &quot;actionAccount=check&&quot;  + " + fastGridTable.GetUrlEncodedRowValuesFuncName + "(getSender(event).parentNode.parentNode);" + "ajaxFunction(paramString, function (rst) { alert(rst); });");
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Href, "#");
                            linkEdit.childElements.Add(new TextContentHalper("Проверить"));
                            return linkEdit.GetHtmlMarkup();
                        }
                });

            result.Columns.Add(new FastColumn("TradeEnabled", "Редактировать")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var linkEdit = new HyperlinkHelper();
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Id, "linkEditAccount" + accountId);
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Name, "linkEditAccount" + accountId);
                            linkEdit.additionalAttribute.Add(HtmlAttribute.OnClick, "if (!confirm(&quot;Сохранить изменения в счёте " + accountId + "?&quot;)) return; var paramString = &quot;actionAccount=edit&currentAccountId=" + accountId + "&&quot; + " + fastGridTable.GetUrlEncodedRowValuesFuncName + "(getSender(event).parentNode.parentNode);" + " ajaxFunction(paramString, function (rst) { alert(rst); });");
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Href, "#");
                            linkEdit.childElements.Add(new TextContentHalper("Принять"));

                            return linkEdit.GetHtmlMarkup();
                        }
                });

            result.Columns.Add(new FastColumn("TradeEnabled", "Удалить")
                {
                    rowFormatter = rowValue =>
                        {
                            var row = (FarmAccount)rowValue;
                            var accountId = row.AccountId.ToString();

                            var linkEdit = new HyperlinkHelper();
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Id, "linkDelAccount" + accountId);
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Name, "linkDelAccount" + accountId);
                            linkEdit.additionalAttribute.Add(HtmlAttribute.OnClick, "if (!confirm(&quot;Удалить счёт " + accountId + "?&quot;)) return;" +
                                                                                    "var paramString = &quot;?actionAccount=dell&currentAccountId=" + accountId + "&&quot; + " +
                                                                                    fastGridTable.GetUrlEncodedRowValuesFuncName + "(getSender(event).parentNode.parentNode); " +
                                                                                    "ajaxJsonFunction(paramString, null, function (rst) { " + "AccountEditTableItems = eval(rst);  " +
                                                                                    "FillAccountEditTable(); });");
                            linkEdit.additionalAttribute.Add(HtmlAttribute.Href, "#");
                            linkEdit.childElements.Add(new TextContentHalper("Удалить"));
                            return linkEdit.GetHtmlMarkup();
                        }
                });
            // ReSharper restore SpecifyACultureInStringConversionExplicitly
            return result;
        }

        /// <summary>
        /// Добавляет на страницу кнопку добавления нового аккаунта
        /// </summary>
        private static void InsertAddAccountButtonControl(StringBuilder sb)
        {
            sb.Append("<br /><br />");
            var formAddAccount = new FormHelper();
            formAddAccount.additionalAttribute.Add(HtmlAttribute.Id, "formAddAccount");
            formAddAccount.additionalAttribute.Add(HtmlAttribute.Name, "formAddAccount");

            var hidden = new InputHelper(HtmlInputType.Hidden);
            hidden.additionalAttribute.Add(HtmlAttribute.Value, "openNewAccountForm");
            hidden.additionalAttribute.Add(HtmlAttribute.Name, "action");

            var text = new TextContentHalper("Добавить новый счёт ");

            var image = new InputHelper(HtmlInputType.Image);
            image.additionalAttribute.Add(HtmlAttribute.Alt, "Добавить");
            image.additionalAttribute.Add(HtmlAttribute.Src, RenderImgBytesSrc(ImgRsxAdd));

            formAddAccount.childElements.Add(hidden); 
            formAddAccount.childElements.Add(text);
            formAddAccount.childElements.Add(image);
            sb.AppendLine(formAddAccount.GetHtmlMarkup());
            sb.Append("<br /><br />");
        }

        /// <summary>
        /// Добавляет на страницу таблицу добавления нового аккаунта
        /// </summary>
        private static void InsertAddAccountTableControl(StringBuilder sb)
        {
            // тут нет особой сложнасти конкатинации или вложенности и логика более менее очевидна. 
            // поэтому пока механизм HtmlHelper-ов в данном конкретном случае не оправдан - сложность только увеличится
          

            sb.Append("<h2>Новый счёт</h2>");
            sb.Append("<form method=\"POST\" id=\"formNewAccount\" name=\"formNewAccount\">");
            sb.AppendLine(
                "<table>" +
                "<tr><td>Номер счёта</td><td><input type=\"text\" style=\"width: 50px;\" name=\"newAccountId\" id=\"newAccountId\" value=\"\"></td></tr>" +
                "<tr><td>Логин</td><td><input type=\"text\" name=\"newUserLogin\" id=\"newUserLogin\" value=\"\"></td></tr>" +
                "<tr><td>Пароль</td><td><input type=\"password\" name=\"newUserPassword\" id=\"newUserPassword\" value=\"\"></td></tr>" +
                "<tr><td>Разрешить торговать</td><td><input type=\"checkbox\" name=\"newTradeEnabled\" id=\"newTradeEnabled\"></td></tr>" +
                "</table>  <br />");
            sb.Append("<button style=\"width: 100px;\"><img alt=\"Принять\" src=\"" + RenderImgBytesSrc(ImgRsxAccept) + "\"> Принять</button>");
            sb.AppendLine("<input type=\"hidden\" name=\"action\" value=\"addNewAccount\" />");
            sb.Append("</form>");
            sb.Append("<form method=\"POST\" id=\"formNewAccount\" name=\"formNewAccount\">");
            sb.Append("<button style=\"width: 100px;\"><img alt=\"Отмена\" src=\"" + RenderImgBytesSrc(ImgRsxDelete) + "\"> Отмена</button>");
            sb.AppendLine("<input type=\"hidden\" name=\"action\" value=\"cancel\" />");
            sb.Append("</form>");
        }

        /// <summary>
        /// Обработчик отправки формы на сервер
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string AccountSubmitAction(HttpListenerContext context, out string action)
        {
            var actionResult = string.Empty;
            action = string.Empty;

            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                var textData = reader.ReadToEnd();

                //Парсим строку с передаными от клиента параметрами
                Dictionary<string, string> submitVal = null;
                try
                {
                    submitVal = textData.Split('&')
                                        .ToDictionary(variable => variable.Substring(0, variable.IndexOf('=')),
                                                      variable => variable.Substring(variable.IndexOf('=') + 1));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Logger.Error("Строка с параметрами добавляемого/редактируемого счёта пуста или в неверном формате", ex);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка при разборе строки с параметрами добавляемого/редактируемого счёта", ex);
                }


                
                if (submitVal != null && submitVal.Count > 0 && submitVal.ContainsKey("action"))
                {
                    action = submitVal["action"];
                    //Если строка распарсилась нормально, проверям, какое действие выполняет пользователь
                    switch (action)
                    {
                        case "addNewAccount":
                            try
                            {
                                AddNewAccount(new FarmAccountArg
                                    {
                                        AccountId = HttpUtility.UrlDecode(submitVal["newAccountId"]).ToIntSafe() ?? -1,
                                        Login = HttpUtility.UrlDecode(submitVal["newUserLogin"]),
                                        Password = HttpUtility.UrlDecode(submitVal["newUserPassword"]),
                                        TradeEnabled =
                                            submitVal.ContainsKey("newTradeEnabled") &&
                                            submitVal["newTradeEnabled"] == "on"
                                    });
                                actionResult = "Новый счёт добавлен";
                            }
                            catch (ArgumentException ex)
                            {
                                actionResult = ex.Message;
                                Logger.Error("Ошибка добавления нового счёта", ex);
                            }
                            catch (Exception ex)
                            {
                                actionResult = ex.Message;
                                Logger.Error("Ошибка добавления нового счёта", ex);
                            }
                            break;
                    }
                }
            }
            return actionResult;
        }

        /// <summary>
        /// Обработчик асинхронных действий пользователя с помощью объекта XMLHttpRequest и ответа клиенту в виде простого текста
        /// </summary>
        /// <returns>Выполнено ли какое либо асинхронное действие</returns>
        private static bool AccountAjaxAction(HttpListenerContext context)
        {
            switch (context.Request.QueryString["actionAccount"])
            {
                case "check":
                    try
                    {
                        var message = FarmAccount.CheckCredentials(new FarmAccountArg
                        {
                            AccountId = context.Request.QueryString["col0"].ToIntSafe() ?? -1,
                            // ReSharper disable AssignNullToNotNullAttribute
                            Login = Encoding.UTF8.GetString(HttpUtility.UrlDecodeToBytes(context.Request.QueryString["col1"], Encoding.Default)),
                            Password = Encoding.UTF8.GetString(HttpUtility.UrlDecodeToBytes(context.Request.QueryString["col2"], Encoding.Default)),
                            // ReSharper restore AssignNullToNotNullAttribute
                            TradeEnabled = context.Request.QueryString.AllKeys.Contains("col3") && context.Request.QueryString["col3"] == "true"
                        });
                        WriteTextResponse(context, string.Format("Результат : \"{0}\"", message));
                        return true;
                    }
                    catch (ArgumentException ex)
                    {
                        Logger.Error("Ошибка проверки счёта", ex);
                        WriteTextResponse(context, ex.Message);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка проверки счёта", ex);
                        WriteTextResponse(context, ex.Message);
                        return true;
                    }
                case "edit":
                    try
                    {
                        var currentAccountId = context.Request.QueryString["currentAccountId"].ToIntSafe() ?? -1;
                        
                        EditAccount(new FarmAccountArg
                        {
                            CurrentAccountId = currentAccountId,
                            AccountId = context.Request.QueryString["col0"].ToIntSafe() ?? -1,
                            // ReSharper disable AssignNullToNotNullAttribute
                            Login = Encoding.UTF8.GetString(HttpUtility.UrlDecodeToBytes(context.Request.QueryString["col1"], Encoding.Default)),
                            Password = Encoding.UTF8.GetString(HttpUtility.UrlDecodeToBytes(context.Request.QueryString["col2"], Encoding.Default)),
                            // ReSharper restore AssignNullToNotNullAttribute
                            TradeEnabled = context.Request.QueryString.AllKeys.Contains("col3") && context.Request.QueryString["col3"] == "true"
                        });

                        WriteTextResponse(context, string.Format("Результат : аккаунт \"{0}\" изменён", currentAccountId));
                        return true;
                    }
                    catch (ArgumentException ex)
                    {
                        Logger.Error("Ошибка обновления счёта", ex);
                        WriteTextResponse(context, ex.Message);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка обновления счёта", ex);
                        WriteTextResponse(context, ex.Message);
                        return true;
                    }
            }
            return false;
        }

        /// <summary>
        /// Обработчик асинхронных действий пользователя с помощью jquary и ответа клиенту в виде JSON 
        /// </summary>
        /// <returns>Выполнено ли какое либо асинхронное действие</returns>
        private bool AccountAjaxJsonAction(HttpListenerContext context)
        {
            switch (context.Request.QueryString["actionAccount"])
            {
                #region
                case "dell":
                    try
                    {
                        var currentAccountId = context.Request.QueryString["currentAccountId"].ToIntSafe() ?? -1;
                        DeleteAccount(currentAccountId);
                        fastGridTable.GetHtmlMarkup(RobotFarm.Instance.Accounts);
                        if (fastGridTable != null)
                        {
                            var res = fastGridTable.GetFormatItemArray();
                            Logger.Info(res);
                            WriteJsonResponse(context, res);
                            return true;
                        }
                        return false;
                    }
                    catch (ArgumentException ex)
                    {
                        Logger.Error("Ошибка удаления счёта", ex);
                        WriteTextResponse(context, ex.Message);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка удаления счёта", ex);
                        WriteTextResponse(context, ex.Message);
                        return true;
                    }
                    #endregion
            }
            return false;
        }
        
        /// <summary>
        /// редактирование существующего счёта
        /// </summary>
        private static void EditAccount(FarmAccountArg e)
        {
            CheckValidationSubmitData(e, "Edit");

            var account = RobotFarm.Instance.Accounts.FirstOrDefault(x => x.AccountId == e.CurrentAccountId);
            if (account == null) return;
            

            account.AccountId = e.AccountId;
            account.UserLogin = e.Login;
            account.UserPassword = e.Password;
            account.TradeEnabled = e.TradeEnabled;
        }

        /// <summary>
        /// удаление счёта
        /// </summary>
        private static void DeleteAccount(int id)
        {
            CheckValidationSubmitData(new FarmAccountArg { AccountId = id }, "Delete");

            var account = RobotFarm.Instance.Accounts.FirstOrDefault(x => x.AccountId == id);
            if (account == null) return;
            RobotFarm.Instance.Accounts.Remove(account);
        }

        /// <summary>
        /// добавление нового аккаунта
        /// </summary>
        private static void AddNewAccount(FarmAccountArg e)
        {
            CheckValidationSubmitData(e, "Add");

            var newAccount = new FarmAccount
            {
                AccountId = e.AccountId,
                TradeEnabled = e.TradeEnabled,
                UserLogin = e.Login,
                UserPassword = e.Password
            };
            RobotFarm.Instance.Accounts.Add(newAccount);
        }

        /// <summary>
        /// Проверка на валидность, введённых пользователем данных
        /// </summary>
        private static void CheckValidationSubmitData(FarmAccountArg e, string action)
        {
            if (action != "Delete" && (string.IsNullOrEmpty(e.Login.Trim()) || string.IsNullOrEmpty(e.Password.Trim())))
                throw new ArgumentException("Логин и/или пароль не могут быть пустыми значениями");
            if (e.AccountId == -1)
                throw new ArgumentException(string.Format("Неверный формат уникального идентификатора {0}.", e.AccountId));

            if (action == "Add" && RobotFarm.Instance.Accounts.Any(x => x.AccountId == e.AccountId))
                throw new ArgumentException(string.Format("В системе уже зарегистрирован аккаут с уникальным идентификатором {0}. Уникальные идентификаторы не могут повторяться.",e.AccountId));

            if (action == "Edit" && RobotFarm.Instance.Accounts.Where(y => y.AccountId != e.CurrentAccountId).Any(x => x.AccountId == e.AccountId))
                throw new ArgumentException(string.Format("В системе уже зарегистрирован аккаут с уникальным идентификатором {0}. Уникальные идентификаторы не могут повторяться.", e.AccountId));
        }
    }
}