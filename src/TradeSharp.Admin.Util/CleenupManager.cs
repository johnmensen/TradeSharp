using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Admin.Util
{
    /// <summary>
    /// удаляет счета, пользователей и т.п., вычищая все связи
    /// </summary>
    public static class CleenupManager
    {
        public static bool DeleteAccount(int accountId, out List<PlatformUser> newUsersWoAccount)
        {
            // пользователи, лишившиеся акаунта
            var emptyUserIds = new List<int>();
            newUsersWoAccount = new List<PlatformUser>();

            // удалить счет
            using (var ctx = DatabaseContext.Instance.Make())
            {
                // Проверка - является ли счёт управляющим для какого-нибудь портфеля кампании
                var portfolioSyst = ctx.TOP_PORTFOLIO.FirstOrDefault(p => p.ManagedAccount == accountId);
                if (portfolioSyst != null)
                {
                    throw new InvalidOperationException("Удаление счета " + accountId + " невозможно - счет выбран как управляющий для портфеля " +
                        portfolioSyst.Id + " (" + portfolioSyst.Name + " / " + portfolioSyst.ParticipantCount + ")");
                }

                // удалить все трансферы
                try
                {
                    var balanceChanges = ctx.BALANCE_CHANGE.Where(b => b.AccountID == accountId);
                    foreach (var bc in balanceChanges)
                    {                        
                        ctx.BALANCE_CHANGE.Remove(bc);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - delete BALANCE_CHANGE", ex);
                    throw;
                }

          

                // удалить привязки пользователей
                try
                {
                    var acUsers = ctx.PLATFORM_USER_ACCOUNT.Where(b => b.Account == accountId);
                    foreach (var ac in acUsers)
                    {
                        emptyUserIds.Add(ac.PlatformUser);
                        ctx.PLATFORM_USER_ACCOUNT.Remove(ac);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - delete PLATFORM_USER_ACCOUNT", ex);
                    throw;
                }

                // удалить открытые позы
                try
                {
                    var acPos = ctx.POSITION.Where(b => b.AccountID == accountId);
                    foreach (var ac in acPos)
                    {
                        ctx.POSITION.Remove(ac);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - delete POSITION", ex);
                    throw;
                }

                // удалить отложенные ордера
                try
                {
                    var acPos = ctx.PENDING_ORDER.Where(b => b.AccountID == accountId);
                    foreach (var ac in acPos)
                    {
                        ctx.PENDING_ORDER.Remove(ac);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - delete PENDING_ORDER", ex);
                    throw;
                }

                // удалить закрытые позы
                try
                {
                    var acPos = ctx.POSITION_CLOSED.Where(b => b.AccountID == accountId);
                    foreach (var ac in acPos)
                    {
                        ctx.POSITION_CLOSED.Remove(ac);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - delete POSITION_CLOSED", ex);
                    throw;
                }

                // удалить сам счет
                try
                {
                    var account = ctx.ACCOUNT.First(a => a.ID == accountId);
                    ctx.ACCOUNT.Remove(account);
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - delete ACCOUNT", ex);
                    throw;
                }

                // сохранить изменения
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteAccount() - saving changes", ex);
                    throw;
                }

                // найти пользователей, лишившихся счетов
                if (emptyUserIds.Count > 0)
                {
                    // проверить, есть ли еще счета?
                    for (var i = 0; i < emptyUserIds.Count; i++)                        
                    {
                        var id = emptyUserIds[i];
                        var hasAccount = ctx.PLATFORM_USER_ACCOUNT.Any(a => a.PlatformUser == id);
                        if (!hasAccount) continue;
                        emptyUserIds.RemoveAt(i);
                        i--;
                    }
                    foreach (var usId in emptyUserIds)
                    {
                        var userId = usId;
                        var user = ctx.PLATFORM_USER.First(u => u.ID == userId);
                        newUsersWoAccount.Add(LinqToEntity.DecoratePlatformUser(user));
                    }
                }
            }
            
            return true;
        }
    
        public static bool DeleteUser(int userId, out List<Account> accountsWoOwners)
        {
            // акаунты без пользователя
            accountsWoOwners = new List<Account>();
            var emptyAccIds = new List<int>();
            
            // удалить счет
            using (var ctx = DatabaseContext.Instance.Make())
            {
                // Проверка - является ли пользователь управляющим для какого-нибудь сервиса
                try
                {
                    var usingServise = ctx.SERVICE.Where(x => x.User == userId).ToList();
                    if(usingServise.Count > 0)
                    {
                        throw new InvalidOperationException("Удаление пользователя " + userId + " невозможно - пользователь является управляющий для сервисов " +
                        string.Join(Environment.NewLine, usingServise.Select(x => x.ID)));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete SUBSCRIPTION", ex);
                    throw;
                }

                try
                {
                    var userEvents = ctx.SUBSCRIPTION.Where(t => t.User == userId);
                    foreach (var ue in userEvents)
                    {
                        ctx.SUBSCRIPTION.Remove(ue);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete SUBSCRIPTION", ex);
                    throw;
                }

                try
                {
                    var userEvents = ctx.USER_EVENT.Where(t => t.User == userId);
                    foreach (var ue in userEvents)
                    {
                        ctx.USER_EVENT.Remove(ue);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete USER_EVENT", ex);
                    throw;
                }

                try
                {
                    var userTopPortfolio = ctx.USER_TOP_PORTFOLIO.Where(t => t.User == userId);
                    foreach (var utp in userTopPortfolio)
                    {
                        ctx.USER_TOP_PORTFOLIO.Remove(utp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete USER_TOP_PORTFOLIO", ex);
                    throw;
                }

                try
                {
                    var userTradeSignal = ctx.SUBSCRIPTION.Where(t => t.User == userId);
                    foreach (var uts in userTradeSignal)
                    {
                        ctx.SUBSCRIPTION.Remove(uts);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete SUBSCRIPTION", ex);
                    throw;
                }

                try
                {
                    var topPortfolio = ctx.TOP_PORTFOLIO.Where(x => x.OwnerUser.HasValue && x.OwnerUser == userId);
                    foreach (var tp in topPortfolio)
                    {
                        ctx.TOP_PORTFOLIO.Remove(tp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete TOP_PORTFOLIO", ex);
                    throw;
                }

                try
                {
                    var transfers = ctx.TRANSFER.Where(x => x.User == userId);
                    foreach (var t in transfers)
                    {
                        ctx.TRANSFER.Remove(t);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete TRANSFER", ex);
                    throw;
                }

                try
                {
                    var wallets = ctx.WALLET.Where(x => x.User == userId);
                    foreach (var w in wallets)
                    {
                        ctx.WALLET.Remove(w);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete WALLET", ex);
                    throw;
                }


                // удалить привязки пользователей
                try
                {
                    var acUsers = ctx.PLATFORM_USER_ACCOUNT.Where(b => b.PlatformUser == userId);
                    foreach (var ac in acUsers)
                    {
                        emptyAccIds.Add(ac.Account);
                        ctx.PLATFORM_USER_ACCOUNT.Remove(ac);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete PLATFORM_USER_ACCOUNT", ex);
                    throw;
                }
                
                // удалить самого пользователя
                try
                {
                    var usr = ctx.PLATFORM_USER.First(c => c.ID == userId);
                    ctx.PLATFORM_USER.Remove(usr);                    
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - delete PLATFORM_USER", ex);
                    throw;
                }

                // сохранить изменения
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("DeleteUser() - saving changes", ex);
                    throw;
                }

                // найти акаунты, лишившиеся пользователей
                if (emptyAccIds.Count > 0)
                {
                    // проверить, есть ли еще счета?
                    for (var i = 0; i < emptyAccIds.Count; i++)
                    {
                        var id = emptyAccIds[i];
                        var hasUser = ctx.PLATFORM_USER_ACCOUNT.Any(a => a.Account == id);
                        if (!hasUser) continue;
                        emptyAccIds.RemoveAt(i);
                        i--;
                    }
                    foreach (var acId in emptyAccIds)
                    {
                        var accountId = acId;
                        var account = ctx.ACCOUNT.First(a => a.ID == accountId);
                        accountsWoOwners.Add(LinqToEntity.DecorateAccount(account));
                    }
                }
            }

            return true;
        }
    }
}
