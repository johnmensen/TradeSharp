using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Util;

namespace TradeSharp.TradeSignal.BL
{
    public class SignalWebServer : BaseWebServer
    {
        private static SignalWebServer instance;

        public static SignalWebServer Instance
        {
            get { return instance ?? (instance = new SignalWebServer()); }
        }

        private SignalWebServer()
        {
        }

        public override string ServiceName
        {
            get { return "Торговые сигналы"; }
        }

        public override void ProcessHttpRequest(HttpListenerContext context)
        {
            var queryParams = PreProcessQueryString(context.Request.QueryString);

            var sbResp = new StringBuilder();
            // приложение закачивает файл с прогнозом
            // рендерить ничего не надо - ответить OK или не OK
            if (context.Request.HttpMethod == "PUT")
                ProcessPutRequest(queryParams, context, sbResp);
            else
            {
                // приложение запросило времена обновлений сигналов
                // по указанным в параметре категориям
                if (queryParams.ContainsKey(TradeSignalXml.ReqPtrSignalUpdates))
                    MakeSignalUpdateResponseString(queryParams[TradeSignalXml.ReqPtrSignalUpdates], sbResp);
                else
                {
                    if (queryParams.ContainsKey(TradeSignalXml.ReqPtrGetForecast))
                        MakeForecastResponseString(queryParams, sbResp);
                    else
                        // вывести разметку страницы, состояние сервиса и справка по параметрам запроса
                    {
                        RenderHttpHead(sbResp, string.Empty, string.Empty, true);
                        RenderBodyOpenTag(sbResp);

                        // проверить учетку
                        //if (CheckCredentials(context, sb))
                        try
                        {
                            MakeBody(sbResp);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("SignalWebServer - ошибка в MakeBody()", ex);
                            sbResp.AppendLine("<p>Техническая ошибка</p>");
                        }

                        RenderBodyCloseTag(sbResp);
                        RenderHttpCloseTag(sbResp);
                    }
                }
            }

            // отправить текст ответа в поток
            var b = TradeSignalXml.DefaultEncoding.GetBytes(sbResp.ToString());
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);
            context.Response.OutputStream.Close();
        }

        private static void MakeSignalUpdateResponseString(string requestedCategoriesString, StringBuilder response)
        {
            if (string.IsNullOrEmpty(requestedCategoriesString))
            {
                response.Append(TradeSignalXml.ResponseErrorPreffix + "empty parameter");
                return;
            }
            var reqIds = requestedCategoriesString.ToIntArrayUniform();
            if (reqIds.Length == 0)
            {
                response.Append(TradeSignalXml.ResponseErrorPreffix + "no Ids provided");
                return;
            }
            var updateTimes = SignalStorage.Instance.GetLastUpdateTimes(reqIds.ToList());
            if (updateTimes == null || updateTimes.Count == 0)
            {
                response.Append(TradeSignalXml.ResponseForecastIsEmpty);
                return;
            }
            foreach (var up in updateTimes)
            {
                response.AppendFormat("{0}_{1}_{2}_{3}{4}", up.a.categoryId, up.a.ticker, 
                    up.a.timeframe, up.b.ToStringUniform(), Environment.NewLine);
            }
        }

        private static void MakeForecastResponseString(Dictionary<string, string> queryParams, StringBuilder response)
        {
            if (!queryParams.ContainsKey(TradeSignalXml.ReqPtrForecastCategory) || 
                string.IsNullOrEmpty(queryParams[TradeSignalXml.ReqPtrForecastCategory]))
            {
                response.Append("error:param \"" + TradeSignalXml.ReqPtrForecastCategory + "\" is missed");
                return;
            }
            if (!queryParams.ContainsKey(TradeSignalXml.ReqPtrForecastTicker) || 
                string.IsNullOrEmpty(queryParams[TradeSignalXml.ReqPtrForecastTicker]))
            {
                response.Append("error:param \"" + TradeSignalXml.ReqPtrForecastTicker + "\" is missed");
                return;
            }
            if (!queryParams.ContainsKey(TradeSignalXml.ReqPtrForecastTimeframe) ||
                string.IsNullOrEmpty(queryParams[TradeSignalXml.ReqPtrForecastTimeframe]))
            {
                response.Append("error:param \"" + TradeSignalXml.ReqPtrForecastTimeframe + "\" is missed");
                return;
            }
            var catId = queryParams[TradeSignalXml.ReqPtrForecastCategory].ToIntSafe();
            if (!catId.HasValue)
            {
                response.Append("error:param \"" + TradeSignalXml.ReqPtrForecastCategory + "\" should be a number");
                return;
            }
            var ticker = queryParams[TradeSignalXml.ReqPtrForecastTicker];
            if (!DalSpot.Instance.GetTickerNames().Contains(ticker))
            {
                response.Append("error:param \"" + TradeSignalXml.ReqPtrForecastTicker + "\" refers to a wrong symbol");
                return;
            }
            BarSettings timeframeSets;
            try
            {
                timeframeSets = new BarSettings(queryParams[TradeSignalXml.ReqPtrForecastTimeframe], TradeSignalXml.TimeframeSeparator);                
            }
            catch
            {
                response.Append("error:param \"" + TradeSignalXml.ReqPtrForecastTimeframe + "\" was not recognized");
                return;
            }
            var respXml = SignalStorage.Instance.GetTradeSignalXml(catId.Value, ticker, timeframeSets);
            if (string.IsNullOrEmpty(respXml))
            {
                response.Append("error:not found");
                return;
            }
            response.Append(respXml);
        }

        private static Dictionary<string, string> PreProcessQueryString(NameValueCollection query)
        {            
            var dic = new Dictionary<string, string>();            
            for (var i = 0; i < query.Count; i++)
            {
                var queryValues = query.GetValues(i);
                if (queryValues != null && queryValues.Length > 0)
                    dic.Add(query.GetKey(i), queryValues[0]);
            }            
            return dic;
        }

        private void MakeBody(StringBuilder sb)
        {
            sb.AppendLine("    <p>Сервис торговых сигналов.<br/>Состояние: нет ошибок</p>");            
        }

        private static void ProcessPutRequest(Dictionary<string, string> queryParams, HttpListenerContext context, StringBuilder resp)
        {
            string catIdString;
            queryParams.TryGetValue(TradeSignalXml.ReqPtrForecastCategory, out catIdString);
            if (string.IsNullOrEmpty(catIdString))
            {
                Logger.Error("SignalWebServer, PUT request - param \"" + TradeSignalXml.ReqPtrForecastCategory + "\" is not set");
                resp.Append("error");
                return;
            }

            string ticker;
            queryParams.TryGetValue(TradeSignalXml.ReqPtrForecastTicker, out ticker);
            if (string.IsNullOrEmpty(ticker))
            {
                Logger.Error("SignalWebServer, PUT request - param \"" + TradeSignalXml.ReqPtrForecastTicker + "\" is not set");
                resp.Append("error");
                return;
            }

            string timeframe;
            queryParams.TryGetValue(TradeSignalXml.ReqPtrForecastTimeframe, out timeframe);
            if (string.IsNullOrEmpty(timeframe))
            {
                Logger.Error("SignalWebServer, PUT request - param \"" + TradeSignalXml.ReqPtrForecastTimeframe + "\" is not set");
                resp.Append("error");
                return;
            }

            var catId = catIdString.ToIntSafe();
            if (!catId.HasValue)
            {
                Logger.ErrorFormat("SignalWebServer, PUT request - param \"" + TradeSignalXml.ReqPtrForecastCategory + 
                    "\" is terribly wrong (\"{0}\")",
                    catIdString);
                resp.Append("error");
                return;
            }

            BarSettings timeframeSets;
            try
            {
                timeframeSets = new BarSettings(timeframe, TradeSignalXml.TimeframeSeparator);
            }
            catch
            {
                Logger.ErrorFormat("SignalWebServer, PUT request - param \"" + TradeSignalXml.ReqPtrForecastTimeframe + 
                    "\" is terribly wrong (\"{0}\")", timeframe);
                resp.Append("error");
                return;
            }

            // получить данные запроса
            var body = context.Request.InputStream;
            var encoding = TradeSignalXml.DefaultEncoding; // context.Request.ContentEncoding;
            var len = context.Request.ContentLength64;
            if (len == 0)
            {
                Logger.Error("SignalWebServer, PUT request - zero data length");
                resp.Append("error");
                return;
            }
            var data = new byte[len];
            using (var reader = new BinaryReader(body, encoding))
            {
                for (var i = 0; i < len; i++)            
                    data[i] = reader.ReadByte();            
            }
            var forecastXml = encoding.GetString(data);
                        
            // отправить запрос - положить "сигнал" в хранилище
            ThreadPool.QueueUserWorkItem(PutForecastInStorage, new ForecastQueueItem(forecastXml,
                                                                                     catId.Value, ticker, timeframeSets));
            resp.Append("OK");
        }

        private static void PutForecastInStorage(object forecast)
        {
            if (forecast == null || forecast is ForecastQueueItem == false) return;
            var typedForecast = (ForecastQueueItem) forecast;
            SignalStorage.Instance.UpdateSignal(typedForecast.categoryId,
                typedForecast.ticker, typedForecast.timeframe, typedForecast.forecastXml);
        }
    }

    class ForecastQueueItem
    {
        public string forecastXml;
        public int categoryId;
        public string ticker;
        public BarSettings timeframe;

        public ForecastQueueItem(string forecastXml, int categoryId, string ticker, BarSettings timeframe)
        {
            this.forecastXml = forecastXml;
            this.categoryId = categoryId;
            this.ticker = ticker;
            this.timeframe = timeframe;
        }
    }
}
