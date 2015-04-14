using System;
using System.Collections.Generic;
using System.Web.Mvc;
using TradeSharp.Hub.BL.Contract;
using TradeSharp.Hub.BL.Model;
using TradeSharp.Hub.BL.Repository;
using TradeSharp.Hub.WebSite.Helper;
using TradeSharp.Hub.WebSite.Models;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Hub.WebSite.Controllers
{
    public partial class TickerController : Controller
    {
        private readonly ITickerRepository tickerRepository;
        private readonly ICurrencyRepository currencyRepository;
        private readonly IServerInstanceRepository serverInstanceRepository;

        public TickerController()
        {
            tickerRepository = new TickerRepository();
            currencyRepository = new CurrencyRepository();
            serverInstanceRepository = new ServerInstanceRepository();
        }

        [HttpGet]
        public ActionResult Tickers()
        {
            var model = GetModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult SortAndPage(PagerModel pager)
        {
            var model = GetModel(pager.SortBy, pager.SortAscending, pager.CurrentPageIndex, pager.PageSize);
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult FillTickerEditForm(string name)
        {
            var ticker = tickerRepository.GetTicker(name);
            var model = ticker == null ? new TickerEditModel() : new TickerEditModel(ticker);
            model.currencies = currencyRepository.GetAllCurrencies();
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveTicker(TickerEditModel ticker)
        {
            string errorString;
            var status = ModelState.IsValid;

            if (!ModelState.IsValid)
            {
                errorString = "Ошибки заполнения данных: " + string.Join(", ",
                                          ViewData.ModelState.Values.Where(x => x.Errors.Count > 0).Select(v => string.Join(", ",
                                              v.Errors.Select(e => e.ErrorMessage))));
            }
            else
            {
                status = tickerRepository.AddOrUpdateTicker(ticker, out errorString);
            }

            return Json(new { status, errorString });
        }

        [HttpPost]
        public ActionResult DeleteTicker(string tickerName)
        {
            string errorString;
            var status = tickerRepository.DeleteTicker(tickerName, out errorString);

            return Json(new { status, errorString });
        }

        /// <summary>
        /// Редактирование псевдонима тикера
        /// </summary>
        [HttpGet]
        public ActionResult TickerAliasEdit(string brokerCode)
        {
            ViewBag.ServerCodeList = serverInstanceRepository.GetServerCodes();
            ViewBag.BrokerCode = brokerCode;
            var model = tickerRepository.GetAllTickerAlias().Where(x => x.Server == brokerCode).ToList();
            if (Request.IsAjaxRequest())
                return PartialView("TickerAliasEditPartialTable", model);
            return View(model);
        }
        

        
        
        /// <summary>
        /// POST с кнопки "Сохранить" в "TickerAliasEditPartialTable.cshtml"
        /// </summary>
        /// <param name="postArray">список строк в формате "Server-Ticker-Alias"</param>
        [HttpPost]
        public ActionResult TickerAliasEdit(List<string> postArray)
        {
            var result = true;
            if (postArray != null && postArray.Count > 0)
            {
                var dataToUpdate = new List<TickerAlias>();
                foreach (var args in postArray.Select(x => x.Split(HelpViewCharacter.Divider.ToCharArray())))
                {
                    try
                    {
                        dataToUpdate.Add(new TickerAlias
                            {
                                Server = args[0],
                                Ticker = args[1],
                                Alias = args[2]
                            });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("TickerAliasEdit", ex);
                        result = false;
                    }
                }
                tickerRepository.UpdateListTickerAlias(dataToUpdate);
            }

            return Json(new { updateSuccess = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Добавляем / редактируем псевдоним
        /// </summary>
        [HttpGet]
        public ActionResult AddTickerAlias(string brokerCodeArg, string tickerNameArg)
        {
            if (string.IsNullOrWhiteSpace(brokerCodeArg))
            {
                Logger.Error(string.Format("DeleteTickerAlias - невалидный параметр brokerCodeArg: '{0}'", brokerCodeArg));
                return RedirectToAction("TickerAliasEdit", new { brokerCode = brokerCodeArg });
            }

            ViewBag.ServerCodeList = serverInstanceRepository.GetServerCodes();
            ViewBag.TickerNameList = tickerRepository.GetTickerNames();

            var model = new TickerAlias { Server = brokerCodeArg };

            if (!string.IsNullOrEmpty(tickerNameArg))
            {
                var tick = tickerRepository.GetTickerAlias(brokerCodeArg, tickerNameArg);

                model.Ticker = tickerNameArg;
                model.Alias = tick == null ? string.Format("{0}_{1}_alias", brokerCodeArg, tickerNameArg) : tick.Alias;
            }

            return View(model);
        }

        /// <summary>
        /// Добавляем псевдоним
        /// </summary>
        [HttpPost]
        public ActionResult AddTickerAlias(TickerAlias model)
        {
            tickerRepository.AddOrUpdateTickerAlias(model);
            return RedirectToAction("TickerAliasEdit", new { brokerCode = model.Server });
        }

        [HttpGet]
        public ActionResult DeleteTickerAlias(string brokerCodeArg, string tickerNameArg)
        {
            if (string.IsNullOrWhiteSpace(brokerCodeArg) && string.IsNullOrWhiteSpace(tickerNameArg))
            {
                Logger.Error(string.Format("DeleteTickerAlias - невалидные параметры brokerCodeArg: '{0}', tickerNameArg: '{1}'", brokerCodeArg, tickerNameArg));
                return RedirectToAction("TickerAliasEdit");
            }

            tickerRepository.RemoveTickerAlias(brokerCodeArg, tickerNameArg);
            return RedirectToAction("TickerAliasEdit", new {brokerCode = brokerCodeArg});
        }
    }
}