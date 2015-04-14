using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.TradeSignalExecutor.BL
{
    /// <summary>
    /// класс обновляет портфель по каждой подписке на портфель,
    /// по каждому счету
    /// 
    /// процедуру инициирует SignalExecutor
    /// </summary>
    class PortfolioBalancer
    {
        private static readonly Lazy<PortfolioBalancer> instance = new Lazy<PortfolioBalancer>(() => new PortfolioBalancer());

        public static PortfolioBalancer Instance
        {
            get { return instance.Value; }
        }

        private PortfolioBalancer()
        {
        }

        public volatile bool IsStopping;

        #region Flood Safe Logger
        private static readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(10000);

        private const int PortfolioUpdateError = 1;
        private const int PortfolioUpdateFailed = 2;
        private const int GetPortfoliosError = 3;
        #endregion

        public bool UpdatePortfolios()
        {
            List<Cortege2<string, int>> subscriptions;
            using (var ctx = DatabaseContext.Instance.Make())
            {
                try
                {
                    subscriptions =
                        (from userPort in ctx.USER_TOP_PORTFOLIO
                         join port in ctx.TOP_PORTFOLIO on userPort.Portfolio equals port.Id
                         join usr in ctx.PLATFORM_USER on userPort.User equals usr.ID
                         select new
                         {
                            login = usr.Login,
                            portfolio = port.Id
                         }).ToList().Select(s => new Cortege2<string, int>
                         {
                             a = s.login,
                             b = s.portfolio
                         }).ToList(); 
                }
                catch (Exception ex)
                {
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, GetPortfoliosError, 1000*60*15,
                                                          "Ошибка получения подписок на портфели: {0}", ex);
                    return false;
                }
            }

            foreach (var sub in subscriptions)
            {
                if (IsStopping) break;
                UpdateTop(sub.a, sub.b);
            }

            return true;
        }

        /// <summary>
        /// обновить портфель пользователя
        /// </summary>
        public RequestStatus UpdateTop(string userLogin, int portfolioId)
        {
            try
            {
                var status = Dealer.ProxyTrade.SubscribeOnPortfolio(ProtectedOperationContext.MakeServerSideContext(),
                            userLogin,
                            new TopPortfolio
                                {
                                    Id = portfolioId
                                }, null, null);
                if (status != RequestStatus.OK)
                    logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, PortfolioUpdateFailed, 1000 * 60 * 15,
                        "Обновление портфеля {0} польз. {1}: {2}", portfolioId, userLogin, status);
                return status;
            }
            catch (Exception ex)
            {
                logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error, PortfolioUpdateError, 1000 * 60 * 15,
                    "Ошибка обновления портфеля {0} польз. {1}: {2}", portfolioId, userLogin, ex);                
                return RequestStatus.ServerError;
            }
        }
    }
}
