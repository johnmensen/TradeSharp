using System;
using System.Net;
using System.Threading;

namespace TradeSharp.Util
{
    public delegate void ProcessHttpRequestDel(HttpListenerContext context);

    public class HttpRequestProcessor
    {
        private readonly HttpListener listener = new HttpListener();
        private ProcessHttpRequestDel onRequest;
        private Thread workThread;
        private bool stopFlag;

        public void Start(int port, ProcessHttpRequestDel onRequest, bool needAuthentication)
        {
            this.onRequest = onRequest;
            listener.Prefixes.Add(string.Format("http://*:{0}/", port));
            if (needAuthentication) listener.AuthenticationSchemes = AuthenticationSchemes.Negotiate;
            listener.Start();
            stopFlag = false;
            workThread = new Thread(ThreadLoop);
            workThread.Start();
        }

        public void Stop()
        {
            stopFlag = true;
            workThread.Join();
            listener.Stop();
        }

        private void ThreadLoop()
        {
            const int timeout = 200;
            while (!stopFlag)
            {
                IAsyncResult result = listener.BeginGetContext(ListenerCallback, listener);
                while (!result.AsyncWaitHandle.WaitOne(timeout))
                {
                    if (stopFlag) break;
                }
            }
        }

        public void ListenerCallback(IAsyncResult result)
        {
            var httpListener = (HttpListener)result.AsyncState;
            try
            {
                var context = httpListener.EndGetContext(result);
                ThreadPool.QueueUserWorkItem(OnRequest, context);
            }
            catch (Exception ex)
            {
                Logger.Error("HttpRequestProcessor - end get context error", ex);
            }
        }

        private void OnRequest(object state)
        {
            try
            {
                onRequest((HttpListenerContext)state);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в HttpRequestProcessor", ex);
            }
        }
    }
}
