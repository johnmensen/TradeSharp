using System;
using System.IO;
using System.Net;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.WatchService.BL
{
    public class SMSNotifier
    {
        private static SMSNotifier instance;

        public static SMSNotifier Instance
        {
            get { return instance ?? (instance = new SMSNotifier()); }
        }

        private readonly string url, pwrd, targetPhones;

        private SMSNotifier()
        {
            url = AppConfig.GetStringParam("SMS.Url", "http://sms.ru/sms/send");
            pwrd = AppConfig.GetStringParam("SMS.Password", "3b9ddc99-9672-1084-a5a5-21831424cd02");
            targetPhones = AppConfig.GetStringParam("SMS.Phones", "79040289881");
        }

        public void SendMessage(string text)
        {
            var HttpWReq = (HttpWebRequest)WebRequest.Create(url);
            var encoding = Encoding.GetEncoding(1251);



            var postData = string.Format("api_id={0}&to={2}&text={1}", pwrd, text, targetPhones);


            var data = encoding.GetBytes(postData);

            HttpWReq.Method = "POST";
            HttpWReq.ContentType = "application/x-www-form-urlencoded; charset=windows-1251";
            HttpWReq.ContentLength = data.Length;

            
            try
            {
                var newStream = HttpWReq.GetRequestStream();
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                Logger.InfoFormat("SMS is sent ({0})", text);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Ошибка при отправке SMS"), ex);
            }
            
        }
    }
}