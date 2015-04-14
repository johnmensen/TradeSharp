using System;
using System.Linq;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.TradeSignalExecutor.BL
{
    /// <summary>
    /// обновляет HWM PAMM-ов и начисляет комиссию управляющим
    /// </summary>
    internal class PAMMFeeManager : Scheduler
    {
        #region Singletone

        private static readonly Lazy<PAMMFeeManager> instance = new Lazy<PAMMFeeManager>(() => new PAMMFeeManager());

        public static PAMMFeeManager Instance
        {
            get { return instance.Value; }
        }

        private PAMMFeeManager()
        {
            schedules = new Schedule[]
                {
                    new DailySchedule(passedValue => CheckPAMMCommission(),
                                      1000*1, null, 0, 0, 0, 1, string.Empty)
                };
        }

        #endregion

        public void CheckPAMMCommission()
        {
            try
            {
                var feeWasTaken = 0;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var services = (from srv in ctx.SERVICE
                                    join acc in ctx.ACCOUNT on srv.AccountId equals acc.ID
                                    where acc.ACCOUNT_SHARE.Count > 1
                                    select srv).ToList();
                    Logger.Info("CheckPAMMCommission(" + services.Count + ")");
                    foreach (var srv in services)
                    {
                        if (isStopped) break;
                        feeWasTaken += ReCalculateAccountShares(ctx, srv);
                    }

                    Logger.InfoFormat("CheckPAMMCommission - комиссия списана " + feeWasTaken + " раз");
                    // обновить дольки / HWM
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в CheckPAMMCommission", ex);
            }
        }

        /// <summary>
        /// возвращает - соклько раз была списана комиссия
        /// </summary>
        public int ReCalculateAccountShares(TradeSharpConnection ctx, SERVICE srv)
        {
            var feeTakenCount = 0;
            try
            {
                PaidService serviceWithFeeScale = null;

                // дольки
                var shares =
                    ctx.ACCOUNT_SHARE.Where(s => s.Account == srv.AccountId && s.ShareOwner != srv.User).ToList();
                if (shares.Count == 0) return feeTakenCount;

                var ownerShare =
                    ctx.ACCOUNT_SHARE.FirstOrDefault(s => s.Account == srv.AccountId && s.ShareOwner == srv.User);
                if (ownerShare == null)
                {
                    Logger.ErrorFormat(
                        "ReCalculateAccountShares(service={0}, account={1}, owner={2}) - доля владельца не найдена",
                        srv.ID, srv.AccountId, srv.User);
                    return feeTakenCount;
                }

                var account = ctx.ACCOUNT.First(a => a.ID == srv.AccountId);

                // получить актуальный баланс (средства) счета
                var positions =
                    ctx.POSITION.Where(p => p.AccountID == srv.AccountId && p.State == (int) PositionState.Opened)
                       .ToList().Select(LinqToEntity.DecorateOrder).ToList();

                var quotes = QuoteStorage.Instance.ReceiveAllData();

                bool noQuoteError;
                var profit = DalSpot.Instance.CalculateOpenedPositionsCurrentResult(positions,
                                                                                    quotes, account.Currency,
                                                                                    out noQuoteError);
                var equity = account.Balance + (decimal) profit;
                if (noQuoteError)
                {
                    Logger.Error("Ошибка в ReCalculateAccountShares - нет котировки для пересчета одного из тикеров (" +
                                 string.Join(", ", positions.Select(p => p.Symbol).Distinct()));
                    return feeTakenCount;
                }

                // пересчитать долю каждого пайщика
                var ownersMoney = equity * ownerShare.Share / 100M;                
                serviceWithFeeScale = GetPaidServiceProgressiveFeeScaleDetail(ctx, srv);

                foreach (var share in shares)
                {
                    bool feeWasTaken;
                    var record = CalculateAccountShare(ctx, share, ownerShare,
                                                       ref ownersMoney, equity, serviceWithFeeScale, out feeWasTaken);
                    ctx.ACCOUNT_SHARE_HISTORY.Add(record);
                    if (feeWasTaken)
                        feeTakenCount++;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в ReCalculateAccountShares", ex);
            }

            return feeTakenCount;
        }

        public ACCOUNT_SHARE_HISTORY CalculateAccountShare(TradeSharpConnection ctx,
                                                           ACCOUNT_SHARE share,
                                                           ACCOUNT_SHARE ownerShare,
                                                           ref decimal ownersMoney,
                                                           decimal equity, 
                                                           PaidService serviceWithFeeScale,
                                                           out bool feeWasTaken)
        {
            feeWasTaken = false;
            var shareMoney = equity*share.Share/100M;
            var hist = new ACCOUNT_SHARE_HISTORY
                {
                    Account = share.Account,
                    Date = DateTime.Now,
                    OldShare = share.Share,
                    OldHWM = share.HWM,
                    NewShare = share.Share,
                    NewHWM = Math.Max(share.HWM ?? 0, shareMoney),
                    ShareOwner = share.ShareOwner,
                    ShareAmount = shareMoney
                };

            share.HWM = hist.NewHWM;
            if ((hist.OldHWM ?? 0) == 0)
                return hist;
            if (shareMoney <= hist.OldHWM)
                return hist;
            if (serviceWithFeeScale == null)
                return hist;

            // был установлен новый HWM?
            var aboveHwm = shareMoney - hist.OldHWM.Value;
            // посчитать сумму комиссии
            var fee = serviceWithFeeScale.CalculateFee(shareMoney, aboveHwm);
            if (fee < 0.01M) return hist;

            feeWasTaken = true;

            // этот самый кусочек приписать владельцу и списать его с пайщика
            ownersMoney += fee;
            shareMoney -= fee;
            ownerShare.Share = ownersMoney*100M/equity;
            share.Share = shareMoney*100M/equity;

            var newHwm = shareMoney;
            if (newHwm > share.HWM)
            {
                share.HWM = newHwm;
                hist.NewHWM = newHwm;
            }
            return hist;
        }

        private PaidService GetPaidServiceProgressiveFeeScaleDetail(TradeSharpConnection ctx, SERVICE srv)
            {
                var fees = ctx.SERVICE_RATE.Where(r => r.Service == srv.ID).OrderBy(r => r.UserBalance).ToList();
                if (fees.Count == 0) return null;
                var service = LinqToEntity.DecoratePaidService(srv);
                service.serviceRates = fees.Select(f => new PaidServiceRate
                    {
                        Amount = f.Amount,
                        RateType = PaidServiceRate.ServiceRateType.Percent,
                        UserBalance = f.UserBalance
                    }).ToList().ToList();
                return service;
            }
    }
}
