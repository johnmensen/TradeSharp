using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Entity;
using Newtonsoft.Json;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.RobotFarm.Request;
using TradeSharp.Util;
using Position = TradeSharp.RobotFarm.Request.Position;

namespace TradeSharp.RobotFarm.BL.Web
{
    partial class WebServer
    {
        private void ProcessJSONPostRequest(HttpListenerContext context)
        {
            var responseString = JsonConvert.SerializeObject(ProcessJSONPostRequestBody(context));
            var resp = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = resp.Length;
            context.Response.OutputStream.Write(resp, 0, resp.Length);
        }

        private JsonResponse ProcessJSONPostRequestBody(HttpListenerContext context)
        {
            if (!context.Request.HasEntityBody)
                return new JsonResponse(false, "body was not provided");

            string line;
            using (var sr = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                line = sr.ReadToEnd();
                if (string.IsNullOrEmpty(line))
                    return new JsonResponse(false, "body was empty");
            }

            // распарсить объект запроса
            var req = DeserializeRequest(line);
            if (req == null)
                return new JsonResponse(false, "body was not recognized as a correct request");

            if (req is RequestAccounts)
                return ProcessRequestAccounts((RequestAccounts) req);

            if (req is RequestLastOrders)
                return ProcessRequestLastOrders((RequestLastOrders)req);

            if (req is RequestPositionsClosing)
                return ProcessRequestPositionsClosing((RequestPositionsClosing)req);

            Logger.Info("not implemented: " + (line.Length > 512 ? line.Substring(0, 512) : line));
            return new JsonResponse(false, "not implemented")
            {
                RequestId = req.RequestId
            };
        }

        private JsonResponse ProcessRequestAccounts(RequestAccounts req)
        {
            var response = new JsonResponseAccounts
            {
                Success = true,
                RequestId = req.RequestId,
                Accounts = RobotFarm.Instance.Accounts.Select(a =>
                    {
                        var ac = new JsonResponseAccounts.Account
                        {
                            Id = a.AccountId,
                            Login = a.UserLogin,
                            Password = a.UserPassword
                        };
                        var ctx = a.GetContext();
                        if (ctx != null)
                        {
                            var acInfo = ctx.AccountInfo;
                            if (acInfo != null)
                            {
                                ac.Balance = acInfo.Balance;
                                ac.Equity = acInfo.Equity;
                            }
                        }
                        ac.Robots = a.Robots.Select(r =>
                            new JsonResponseAccounts.AccountRobot
                            {
                                Name = r.TypeName,
                                TickerTimeframe = string.Join(", ", r.Graphics.Select(g => string.Format("{0} {1}",
                                    g.a, BarSettingsStorage.Instance.GetBarSettingsFriendlyName(g.b))))
                            }).ToList();
                        return ac;
                    }).ToList(),
                FarmState = RobotFarm.Instance.State
            };

            return response;
        }

        private JsonResponse ProcessRequestLastOrders(RequestLastOrders req)
        {
            var response = new JsonResponseLastOrders
            {
                Success = true,
                RequestId = req.RequestId,
                AccountPositions = RobotFarm.Instance.Accounts.ToDictionary(a => a.AccountId,
                    a => a.GetAccountOrders().Select(o => new Position
                    {
                        Id = o.ID,
                        AccountId = o.AccountID,
                        Side = o.Side,
                        PriceEnter = (decimal)o.PriceEnter,
                        PriceExit = (decimal)(o.PriceExit ?? 0),
                        Profit = (decimal)o.ResultDepo,
                        TimeEnter = o.TimeEnter,
                        TimeExit = o.TimeExit ?? default(DateTime),
                        Sl = (decimal)(o.StopLoss ?? 0),
                        Tp = (decimal)(o.TakeProfit ?? 0),
                        Volume = o.Volume,
                        VolumeDepo = (decimal)o.VolumeInDepoCurrency,
                        Symbol = o.Symbol,
                        Mt4Order = o.MasterOrder ?? 0
                    }).ToList())
            };

            return response;
        }

        private JsonResponse ProcessRequestPositionsClosing(RequestPositionsClosing req)
        {
            var resp = new JsonResponsePositionsClosing
            {
                RequestId = req.RequestId
            };
            var errorStrings = new Dictionary<string, string>();
            int countOk = 0, countFail = 0;
            try
            {
                foreach (var pos in req.positions)
                {
                    string errorString;
                    var order = new MarketOrder
                    {
                        AccountID = pos.AccountId,
                        ID = pos.Id,
                        Side = pos.Side,
                        Symbol = pos.Symbol,
                        StopLoss = (float)pos.Sl,
                        TakeProfit = (float)pos.Tp,
                        TimeEnter = pos.TimeEnter,
                        TimeExit = pos.TimeExit,
                        Volume = pos.Volume,
                        PriceEnter = (float)pos.PriceEnter,
                        PriceExit = pos.PriceExit == 0 ? (float?)null : (float)pos.PriceExit,
                        ResultDepo = (float)pos.Profit,
                        MasterOrder = pos.Mt4Order
                    };
                    if (PlatformManager.Instance.proxy.ModifyOrder(order, out errorString))
                        countOk++;
                    else countFail++;
                    if (!string.IsNullOrEmpty(errorString) && !errorStrings.ContainsKey(errorString))
                        errorStrings.Add(errorString, string.Empty);
                }
                resp.CountClosed = countOk;
                resp.CountFail = countFail;
                resp.Success = countFail == 0;
                resp.ErrorString = string.Join(". ", errorStrings.Keys);
            }
            catch (Exception ex)
            {
                resp.ErrorString = ex.GetType().Name + ": " + ex.Message;
                resp.Success = false;
            }
            
            return resp;
        }

        private JsonRequest DeserializeRequest(string requestJson)
        {
            return JsonRequest.ParseCommand(requestJson);
        }
    }
}
