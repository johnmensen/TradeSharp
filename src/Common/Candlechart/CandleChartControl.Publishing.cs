using System;
using System.Linq;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using Candlechart.Controls;
using Entity;
using TradeSharp.Util;

namespace Candlechart
{    
    public partial class CandleChartControl
    {
        private static readonly Regex temporaryForecastFilePathTemplate = new Regex(
            @"\D{3}_\D+\d?_\d\d\.\d\d\.\d{4}_\d\d-\d\d(.thumb)?.((png)|(txt))", RegexOptions.IgnoreCase);

        private const string Url = "http://forexinvest.com/forecast/upload_photo";

        private const string HashKey = "10c4d2cee6f49d0e4e03dadd33dd8788";

        private static readonly Encoding requestEncoding = Encoding.UTF8;

        /// <summary>
        /// опубликовать прогноз на сайте
        /// </summary>
        public void PublishForecast()
        {
            var accountId = getAccountId() ?? 0;
            if (accountId == 0) return;            

            var dlg = new PublishForecastWindow
                          {
                              Ticker = Symbol,
                              Signal = BarSettingsStorage.Instance.GetBarSettingsFriendlyName(Timeframe)                                           
                          };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            // - 1 - создать картинку
            var timePublishStr = DateTime.Now.ToString("dd.MM.yyyy_HH-mm");
            var ticker = dlg.Ticker;
            var imageName = string.Format("{0}_{1}_{2}.png", ticker, dlg.Signal, timePublishStr);
            var bmp = MakeImage();
            var hashSrc = accountId.ToString() + imageName + HashKey;
            var token = hashSrc.CreateMD5Hash(requestEncoding);

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                bmp.Save(memoryStream, ImageFormat.Png);
                bytes = memoryStream.ToArray();
            }

            bool shouldRetry;
            var nameValueCollection = new NameValueCollection
                {
                    {"account", accountId.ToString()},
                    {"token", token}
                };
            // отправить картинку по адресу
            var rst = HttpUploadFile(dlg.Commentary,
                Url, imageName, bytes, "filedata", "image/png", nameValueCollection, out shouldRetry);
            //if (shouldRetry) rst = HttpUploadFile(Url, imageName, bytes, "filedata", "image/png", nameValueCollection, out shouldRetry);
            FreeTerminalFolderOfTemporaryForecastImages();

            var msg = rst ? Localizer.GetString("MessageForecastSent") : Localizer.GetString("MessageForecastSendingFailed");
            MessageBox.Show(msg,
                Localizer.GetString("TitleForecast"), 
                MessageBoxButtons.OK,
                rst ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void FreeTerminalFolderOfTemporaryForecastImages()
        {
            foreach (var fileName in Directory.GetFiles(ExecutablePath.ExecPath, "*.*"))
            {
                var nameOnly = Path.GetFileName(fileName);
                // соответствует шаблону?
                if (!temporaryForecastFilePathTemplate.IsMatch(nameOnly))
                    continue;

                try
                {
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка удаления временного файла " + nameOnly, ex);
                }
            }
        }

        public static bool HttpUploadFile(
            string textMessage,
            string url, 
            string file, 
            byte[] fileData,
            string paramName, 
            string contentType, 
            NameValueCollection nvc, 
            out bool shouldRetry)
        {
            // дополнить URL параметром
            var preffix = url.Contains("?") ? "&comment=" : "?comment=";
            url = url + preffix + HttpUtility.UrlEncode(textMessage);

            shouldRetry = false;
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;

            using (var rs = wr.GetRequestStream())
            {
                const string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = requestEncoding.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                const string headerTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, paramName, file, contentType);
                byte[] headerbytes = requestEncoding.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                rs.Write(fileData, 0, fileData.Length);

                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
            }
            
            try
            {
                var response = wr.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    return true;
                    using (var readStream = new StreamReader(responseStream))
                    {
                        var read = new Char[256];
                        var count = readStream.Read(read, 0, 256);
                        var textBuffer = new StringBuilder();
                        while (count > 0)
                        {
                            var str = new String(read, 0, count);
                            textBuffer.Append(str);
                            count = readStream.Read(read, 0, 256);
                        }
                        
                        var defaultRegex = new Regex("(?<=\\\"status_code\\\"\\s*\\:\\s*)\\d+(?=,)");
                        var matches = defaultRegex.Matches(textBuffer.ToString());
                        if (matches.Count == 0 || matches[0].Value != "200") shouldRetry = true;
                    }
                }
               
                Logger.InfoFormat("PublishForecast - статус {0}", (((HttpWebResponse)response).StatusDescription));
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в HttpUploadFile() - GetResponse", ex);
                return false;
            }
        }
    }
}