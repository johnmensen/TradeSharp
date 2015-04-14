using System;
using System.Net;
using System.Xml;

namespace TradeSharp.News.NewsReceiverLib
{
    public class ExchangeSession
    {
        public delegate void DownloadFileFinished(byte[] data);

        public DownloadFileFinished FileDownloaded;
        public String OperationResult = String.Empty;
        private WebClient _client;
        private bool _downloading;
        public void ExecuteOptionsSession()
        {
            var doc = new XmlDocument();
            doc.Load("OptionsExchangeReader.xml");
            String my_url = doc.GetElementsByTagName("StartPage")[0].Attributes["URI"].Value;
            LoadDocumentFromURI(my_url); 
        }

        public void ExecuteCurrencyFuturesSession()
        {

        }

        public void ExecuteCRBIndexSession()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UriStr"></param>
        private void LoadDocumentFromURI(String UriStr)
        {
            if (!_downloading)
            {
                _client = new WebClient();
                _client.DownloadProgressChanged += _client_DownloadProgressChanged;
                _client.DownloadDataCompleted += _client_DownloadDataCompleted;
                try
                {
                    // запускаем загрузку
                    _client.DownloadDataAsync(new Uri(UriStr));
                    _downloading = true;
                    //DownloadBtn.Text = "Отменить";
                }
                catch (UriFormatException ex)
                {
                    OperationResult = ex.Message;
                    _client.Dispose();
                }
                catch (WebException ex)
                {
                    OperationResult = ex.Message;
                    _client.Dispose();
                }
            }
            else
            {
                _client.CancelAsync();
            }
        }

        void _client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //progressBar.Value = e.ProgressPercentage;
            //labelStatus.Text = string.Format("{0:N0} / {1:N0} получено байтов", e.BytesReceived, e.TotalBytesToReceive);
            OperationResult = string.Format("{0:N0} / {1:N0} получено байтов", e.BytesReceived, e.TotalBytesToReceive);
        }

        private void _client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                //progressBar.Value = 0;
                OperationResult = "Загрузка отменена";
            }
            else
                OperationResult = e.Error != null ? e.Error.Message : "Загрузка закончена!";
            _client.Dispose();
            _downloading = false;
            //DownloadBtn.Text = "Загрузить";
            // e.Results содержит все данные
            FileDownloaded(e.Result);
            //downloadedData = e.Result;
        }
    }
}
