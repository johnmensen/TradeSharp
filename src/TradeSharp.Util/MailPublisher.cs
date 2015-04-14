using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Net;
using System.Text;

namespace TradeSharp.Util
{
    public static class MailPublisher
    {
        private static string url = AppConfig.GetStringParam("Publish.Url", "http://forexinvest.ru:8095");
        private static string username = AppConfig.GetStringParam("Publish.Username", "BrokerSvc@forexinvest.local");
        private static string pass = AppConfig.GetStringParam("Publish.Pass", "Br0ker$201!");

        enum ImageInfoType
        {
            Other = 4
        }
        enum CommandType
        {
            Email = 8
        }

        private static WebClient GetConnection()
        {
            var wc = new WebClient
            {
                Credentials = new NetworkCredential(username, pass)
            };

            var serverRequest = WebRequest.Create(url);
            serverRequest.Credentials = new NetworkCredential(username, pass);
            var serverResponse = serverRequest.GetResponse();
            serverResponse.Close();
            return wc;
        }
        public static void SendEmails(string[] maillist, string subj, string body, string[] attach)
        {
            try
            {
                using (var wc = GetConnection())
                {
                    // ищем картинку, если она есть то отправляем вместе с текстом подписчикам
                    var filelist = new List<string>();
                    foreach (var a in attach)
                    {
                        if (string.IsNullOrEmpty(a) || !File.Exists(a)) continue;
                        // надо отправить файл на сервер
                        var filename = (new FileInfo(a)).Name;
                        filelist.Add(filename);
                        wc.QueryString = new NameValueCollection { { "filename", filename }, { "ImageInfoType", ImageInfoType.Other.ToString() } }; ;
                        wc.UploadFile(url, "PUT", a);
                    }

                    var header = HttpUtility.UrlEncode(subj, Encoding.GetEncoding(1251));
                    var msg = HttpUtility.UrlEncode(body, Encoding.GetEncoding(1251));

                    var emails = HttpUtility.UrlEncode(string.Join(";", maillist), Encoding.GetEncoding(1251));

                    wc.QueryString = new NameValueCollection { { "command", CommandType.Email.ToString() }, { "emails", emails }, {"attach", string.Join(";", filelist.ToArray())}, 
                            { "subj", header } , { "body", msg} };
                    var bret = wc.UploadString(url, "POST", string.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Нет связи с сайтом рассылки публикаций!", ex);
            }
        }
    }
}
