using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RobotFarmManager : IRobotFarm
    {
        public RobotFarmManager()
        {
        }

        /// <summary>
        /// вернуть данные по счету для фермы роботов
        /// </summary>
        public bool GetAccountData(string login, string pwrd, int accountId,
            out Account account, out List<MarketOrder> openedOrders)
        {
            openedOrders = new List<MarketOrder>();
            account = null;

            try
            {
                // проверить логин - пароль
                using (var db = DatabaseContext.Instance.Make())
                {
                    var user = db.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user == null) return false;
                    if (user.Password != pwrd) return false;                
                    
                    // выдернуть ордер из списка
                    var accountBase = db.ACCOUNT.FirstOrDefault(a => a.ID == accountId);
                    if (accountBase == null) return false;
                    account = LinqToEntity.DecorateAccount(accountBase);

                    // получить сделки
                    var lstPos = db.POSITION.Where(p => p.AccountID == accountId).ToList();
                    foreach (var pos in lstPos)
                        openedOrders.Add(LinqToEntity.DecorateOrder(pos));
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в GetAccountData()", ex);
                return false;
            }
        }
    }
}
