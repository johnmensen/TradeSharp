using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using TradeSharp.Util;

namespace TradeSharp.Contract.Util.Proxy
{
    public static class HttpMailServerProxy
    {
        private static readonly string url = AppConfig.GetStringParam("MailServer.Url", "http://forexinvest.ru:8095");
        private static readonly string username = AppConfig.GetStringParam("MailServer.Username", "BrokerSvc@forexinvest.local");
        private static readonly string pass = AppConfig.GetStringParam("MailServer.Pass", "Br0ker$201!");

        private static WebClient GetConnection()
        {
            try
            {
                var wc = new WebClient
                {
                    Credentials = new NetworkCredential(username, pass)
                };

                var serverRequest = WebRequest.Create(url);
                serverRequest.Credentials = new NetworkCredential(username, pass);
                var serverResponse = serverRequest.GetResponse();
                if (serverResponse != null)
                {
                    serverResponse.Close();
                    return wc;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в HttpMailServerProxy - GetConnection()", ex);
            }            
            return null;
        }

        public static bool SendEmails(string[] maillist, string subj, string body, string[] attach)
        {            
            var wc = GetConnection();
            if (wc == null) return false;
            try
            {
                // ищем картинку, если она есть то отправляем вместе с текстом подписчикам
                var filelist = new List<string>();
                foreach (var a in attach)
                {
                    if (string.IsNullOrEmpty(a) || !File.Exists(a)) continue;
                    // надо отправить файл на сервер
                    var filename = (new FileInfo(a)).Name;
                    filelist.Add(filename);
                    wc.QueryString = new NameValueCollection { { "filename", filename }, { "ImageInfoType", "Other" } }; ;
                    wc.UploadFile(url, "PUT", a);
                }

                var header = HttpUtility.UrlEncode(subj, Encoding.GetEncoding(1251));
                var msg = HttpUtility.UrlEncode(body, Encoding.GetEncoding(1251));
                var emails = HttpUtility.UrlEncode(string.Join(";", maillist), Encoding.GetEncoding(1251));

                wc.QueryString = new NameValueCollection { { "command", "Email" },
                                                           { "emails", emails }, 
                                                           { "attach", string.Join(";", filelist)},
                                                           { "subj", header }, 
                                                           { "body", msg} };
                wc.UploadString(url, "POST", string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Нет связи с сайтом рассылки публикаций", ex);
                return false;
            }
            finally
            {
                wc.Dispose();
            }
        }        
    }
}
