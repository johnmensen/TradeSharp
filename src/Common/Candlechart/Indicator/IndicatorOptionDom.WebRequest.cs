using System;
using System.IO;
using System.Net;
using System.Text;
using TradeSharp.Util;

namespace Candlechart.Indicator
{    
    public partial class IndicatorOptionDom
    {
        private string QueryPage(string queryString, bool provideCredentials)
        {
            return string.Empty;
            var req = WebRequest.Create(queryString);
            req.Method = "GET";
            if (provideCredentials)
            {
                req.Method = "POST";
                var encoding = Encoding.ASCII;
                var postData = string.Format("username={0}&password={1}", cmeUserName, cmePassword);
                byte[] data = encoding.GetBytes(postData);
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;
                using (var newStream = req.GetRequestStream())
                {
                    newStream.Write(data, 0, data.Length);
                }
            }

            WebResponse responce;
            try
            {
                responce = req.GetResponse();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка запроса к URL({0}): {1}", req.RequestUri, ex);
                return string.Empty;
            }

            if (responce == null)
            {
                Logger.ErrorFormat("Нет ответа от {0}", queryString);
                return string.Empty;
            }
            
            var stream = responce.GetResponseStream();
            if (stream == null)
            {
                Logger.ErrorFormat("Нет ответа от {0}", queryString);
                return string.Empty;
            }
            var sr = new StreamReader(stream);
            var respString = sr.ReadToEnd();

            // https://login.cmegroup.com/cas/login?service=http://datasuite.cmegroup.com/dataSuite.html?template=opt&productCode=XT&exchange=XCME&selected_tab=fx
            const string url = "https://login.cmegroup.com/cas/login";
            if (respString.Contains("Login to cmegroup.com") && !provideCredentials)
                //return QueryPage(url, true);
                PerformDynamicLogin();

            return respString;
        }

        private void PerformDynamicLogin()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) =>
            {
                return true;
            };
            
            const string loginUrl = "https://login.cmegroup.com/cas/login";

            var req = WebRequest.Create(loginUrl) as HttpWebRequest;

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Referer = loginUrl;
            req.CookieContainer = new CookieContainer();

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                var postData = string.Format(
                    @"username={0}&password={1}&keep_me_login=1&save_page=&action=login",
                    cmeUserName, cmePassword);
                sw.Write(postData);
            }

            var rep = req.GetResponse() as HttpWebResponse;
            Console.WriteLine("Response Status: {0}", rep.StatusCode);
            using (var sr = new StreamReader(rep.GetResponseStream()))
            {
                var content = sr.ReadToEnd();
                using (var sw1 = File.CreateText("d:\\output.htm"))
                {
                    sw1.Write(content);
                }
                //Console.WriteLine(content);
            }


            // After sucessfully login (finish the request above), you can
            // continue to issue new WebRequest based requests to other pages
            // But remember to attach the CookieContainer you created
            // in the login request, e.g.

            //var pageUrl = "http://www.oppapers.com/essays/All-About-Dota/618778";
            //var newReq = WebRequest.Create(pageUrl) as HttpWebRequest;
            //newReq.Method = "GET";
            //newReq.CookieContainer = req.CookieContainer;

            //......

        }
    }
}