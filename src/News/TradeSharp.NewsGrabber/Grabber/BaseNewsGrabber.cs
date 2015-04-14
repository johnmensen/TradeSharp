using System;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.NewsGrabber.Grabber
{
    abstract class BaseNewsGrabber
    {
        /// <summary>
        /// не рекомендуется складывать в PutNewsOnServer больше отведенного
        /// количества записей за раз
        /// </summary>
        protected const int MaxNewsInBlock = 100;
        
        private readonly FloodSafeLogger loggerNoFlood = new FloodSafeLogger(1000);

        private const int LogMsgErrorPutNews = 1;

        private static NewsReceiverProxy newsProxy;
        private static NewsReceiverProxy NewsProxy
        {
            get { return newsProxy ?? (newsProxy = new NewsReceiverProxy("INewsReceiverBinding")); }
        }

        public abstract void GetNews();

        protected bool PutNewsOnServer(News[] news)
        {
            try
            {
                NewsProxy.PutNews(news);
                return true;
            }
            catch (System.ServiceModel.CommunicationException)
            {
                // попробовать обновить соединение
                try
                {
                    newsProxy = new NewsReceiverProxy("INewsReceiverBinding");
                    NewsProxy.PutNews(news);
                    return true;
                }
                catch (Exception ex)
                {
                    loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgErrorPutNews, 1000 * 60, "Ошибка доставки новости провайдеру: {0}", ex);
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                loggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                    LogMsgErrorPutNews, 1000 * 60, "Ошибка доставки новости провайдеру: {0}", ex);
                return false;
            }          
        }
    }
}
