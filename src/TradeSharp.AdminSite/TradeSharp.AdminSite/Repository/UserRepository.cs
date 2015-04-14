using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TradeSharp.Admin.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Linq;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Repository
{
    public class UserRepository : IUserRepository
    {
        public List<AccountUserModel> GetAllPlatformUser()
        {
            var result = new List<AccountUserModel>();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var usersInfo = from x in ctx.PLATFORM_USER
                                    select new
                                        {
                                            user = x,
                                            right = x.PLATFORM_USER_ACCOUNT,
                                            walletBalance = x.WALLET.Balance,
                                            walletCurrency = x.WALLET.Currency,
                                            service = x.SERVICE,
                                            CountOwnerSignal = x.SERVICE.Count,
                                            countSubscription = x.SUBSCRIPTION.Count
                                        };
                    foreach (var useInfo in usersInfo)
                    {
                        var us = SiteAdminLinqToEntity.DecoratePlatformUser(useInfo.user);
                        us.UserRightsMask = useInfo.right.ToDictionary(x => x.Account, y => (UserAccountRights)y.RightsMask);
                        us.UserService = useInfo.service;
                        us.CountOwnerSignal = useInfo.CountOwnerSignal;
                        us.CountSubscription = useInfo.countSubscription;
                        us.WalletBalance = useInfo.walletBalance;
                        us.WalletCurrency = useInfo.walletCurrency;
                        result.Add(us);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAllPlatformUser()", ex);
                return null;
            }

            return result;
        }

        public bool DeletePlatformUser(int id, out List<Account> accountsWoOwners)
        {
            try
            {
                return CleenupManager.DeleteUser(id, out accountsWoOwners);
            }
            catch (Exception ex)
            {
                Logger.Error("DeletePlatformUser", ex);
                accountsWoOwners = null;
                return false;
            }
        }

        /// <summary>
        /// Получаем из таблици PLATFORM_USER пользователя по его уникальному идентификатору. 
        /// "преобразуем" полученные данный в объект типа "AccountUserModel"
        /// Из таблици "PLATFORM_USER_ACCOUNT" вытаскиваем все записи (пользователь --- какой-либо счёт этого пользователя --- права на этот счёт этого пользователя) 
        /// преобразуем полученные записи в словарь и записываем в свойство UserRightsMask
        /// 
        /// так же получает данные о кошельке пользователя
        /// </summary>
        /// <param name="id">Уникальный идентификатор пользователя, по которому нужно пудучить детальную информацию</param>
        /// <remarks>Тестируется в методе NuAccountDbEditUserInfo</remarks>
        public AccountUserModel GetUserInfoById(int id)
        {
            AccountUserModel result = null;
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.ID == id);

                    if (user != null)
                    {
                        result = SiteAdminLinqToEntity.DecoratePlatformUser(user);
                        var userRights = user.PLATFORM_USER_ACCOUNT.Where(x => x.PlatformUser == id);
                        result.UserRightsMask = userRights.ToDictionary(x => x.Account, y => (UserAccountRights) y.RightsMask);

                        var wallet = ctx.WALLET.FirstOrDefault(x => x.User == id);
                        if (wallet != null)
                        {
                            result.WalletBalance = wallet.Balance;
                            result.WalletCurrency = wallet.Currency;
                        }
                        else
                        {
                            Logger.InfoFormat("GetWalletDetailsById() - не найдено кошелька для пользователя {0}", id);
                        }
                    }
                }
            }
            #region catch
            catch (EntityException ex)
            {
                Logger.Error(String.Format("Не удалось получить данные о пользователе {0}, из за невозможности обратиться к серверу", id), ex);
            }
            catch (Exception ex)
            {
                Logger.Error("GetUserInfoById", ex);
            }
            #endregion
            return result;
        }

        /// <summary>
        /// Получает пользователя - владельца указанного счёта
        /// </summary>
        /// <param name="id">уникальный идентификатор счёта</param>
        /// <returns>владелец указанного счёта</returns>
        public PlatformUser GetAccountOwner(int id)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var owner =
                        ctx.PLATFORM_USER_ACCOUNT.FirstOrDefault(x => x.Account == id && x.RightsMask == 0);
                    return owner == null ? null : LinqToEntity.DecoratePlatformUser(owner.PLATFORM_USER);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GetAccountOwner", ex);
            }

            return null;
        }

        /// <summary>
        /// Редактирование таблици PLATFORM_USER. 
        /// </summary>
        /// <param name="userModelData">Уникальный идентификатор всегда прежний, вся остальныя информация может меняться</param>
        /// <returns>Результат выполнения опрерации</returns>
        /// <remarks>Тестируется</remarks>
        public bool EditUserInfo(AccountUserModel userModelData)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(x => x.ID == userModelData.UserId);

                    if (user == null) return false;
                    user.Name = userModelData.UserName;
                    user.Surname = userModelData.UserSurname;
                    user.Patronym = userModelData.UserPatronym;
                    user.Description = userModelData.UserDescription;
                    user.Email = userModelData.UserEmail;
                    user.Login = userModelData.UserLogin;
                    user.Password = userModelData.UserPassword;
                    user.Phone1 = userModelData.UserPhone1;
                    user.Phone2 = userModelData.UserPhone2;
                    user.RoleMask = (int)userModelData.UserRoleMask;

                    ctx.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(String.Format("Не удалось сохранить изменения данных пользователе {0}", userModelData.UserId), ex);
            }
            return false;
        }

        /// <summary>
        /// Возвращает всех пользователей-подписчиков на сигналы
        /// </summary>
        /// <param name="signalId">сигналы, подписчиков которых нужно получить</param>
        /// <returns>Логины</returns>
        public List<string> GetLoginSubscriber(int signalId)
        {
            var result = new List<string>();

            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    result = ctx.SUBSCRIPTION.Where(x => x.Service == signalId).Select(x => x.PLATFORM_USER.Login).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Subscriber", ex);
            }

            return result;
        }

        /// <summary>
        /// Получить количество служб (сигналов), которыми управляет пользователь
        /// </summary>
        /// <returns></returns>
        public int? GetSignalCount(int id)
        {
            try
            {
                int count;
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.First(x => x.ID == id);
                    count = user.SERVICE.Count;
                }
                return count;
            }
            catch (Exception ex)
            {
                Logger.Error("GetSignalCount()", ex);
                return null;
            }
        }
    }
}