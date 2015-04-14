using System;
using System.Net;
using System.IO;
using System.Threading;

namespace TradeSharp.Util
{
    /// <summary>
    /// класс для создания асинхронных web-запросов, которые можно прервать
    /// за N секунд, не ожидая ~20 секунд, пока запрос не завершится неудачей
    /// </summary>
    public class AsynchWebRequest
    {
        private WebRequest request;

        private volatile bool requestInProcess;

        public delegate void ResponseDel(Stream stream);

        private ResponseDel onResponse;
        
        public event ResponseDel OnResponce
        {
            add { onResponse += value; }
            remove { onResponse -= value; }
        }

        public bool StartRequest(string requestUri, int timeout)
        {
            return StartRequest(requestUri, timeout, null);
        }

        public bool StartRequest(string requestUri, int timeout, ICredentials credentials)
        {
            if (onResponse == null) return false;
            
            request = WebRequest.Create(requestUri);
            if (credentials != null)
                request.Credentials = credentials;
            
            try
            {
                var rst = request.BeginGetResponse(RequestCompleted, null);
                ThreadPool.RegisterWaitForSingleObject(rst.AsyncWaitHandle, 
                    TimeoutCallback, request, timeout, true);
                requestInProcess = true;
            }
            catch (Exception ex)
            {
                Logger.Error("AsynchWebRequest.StartRequest", ex);
                return false;
            }
            return true;
        }

        public void RequestCompleted(IAsyncResult rst)
        {
            WebResponse response;
            try
            {
                response = request.EndGetResponse(rst);
            }
            catch (WebException ex)
            { // connection could be aborted
                requestInProcess = false;
                return;
            }
            
            using (var stream = response.GetResponseStream())
            {
                try
                {
                    onResponse(stream);
                }
                finally
                {
                    response.Close();
                }                
            }
            requestInProcess = false;
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                var request = state as WebRequest;
                if (request != null) request.Abort();                
            }
        }

        public bool RequestInProcess
        {
            get { return requestInProcess; }
        }
    }
}
