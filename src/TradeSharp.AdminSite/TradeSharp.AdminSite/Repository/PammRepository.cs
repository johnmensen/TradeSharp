using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Models.Items;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class PammRepository : IPammRepository
    {
        public List<PammItem> GetAllPamm(bool anyInvestor)
        {
            var result = new List<PammItem>();          
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var items =
                        ctx.ACCOUNT.Where(x => x.ACCOUNT_SHARE.Any())
                           .Select(x => new { account = x, shareItem = x.ACCOUNT_SHARE }).ToList();

                    if (anyInvestor)
                        items = items.Where(x => x.shareItem.Any()).ToList();

                    
                    foreach (var item in items)
                    {

                        result.Add(new PammItem
                            {
                                Balance = item.account.Balance,
                                ID = item.account.ID,
                                Currency = item.account.Currency,
                                OwnerCount = item.shareItem.Count(),
                                ShareItems = item.shareItem.ToList()
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllAccounts()", ex);
                return null;
            }

            return result;
        }

        public PammItem GetPammById(int id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var account = ctx.ACCOUNT.Where(x => x.ID == id).
                                      Select(x => new { item = x, shareItem = x.ACCOUNT_SHARE }).
                                      Single();
                    var result = new PammItem();
                    LinqToEntity.DecorateAccount(result, account.item);
                    result.ShareItems = account.shareItem.ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetPammById()", ex);
                return null;
            }
        }

        /// <summary>
        /// Получить данные для графиков истории изменения счёта
        /// </summary>
        /// <returns></returns>
        public ACCOUNT_SHARE_HISTORY[] GetAccountHistory(int id)
        {

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                   var  result = ctx.ACCOUNT_SHARE_HISTORY.Where(x => x.Account == id).ToArray();
                   return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAccountHistory()", ex);
                return null;
            }
        }
    }
}