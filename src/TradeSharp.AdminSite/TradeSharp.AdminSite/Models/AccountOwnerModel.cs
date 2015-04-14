using System;
using System.Linq;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.SiteAdmin.Models
{
    /// <summary>
    /// модель для представления AccountOwner (счёт - пользователь - прова на счёт пользователя)
    /// </summary>
    public class AccountOwnerModel
    {
        /// <summary>
        /// Кортеж 'пользователь - список кортежей "счёт - права этого пользователя на счёт"'
        /// заполняется данными в случае, если модель должна быть сформирована в виде "показать все счета пользователя"
        /// </summary>
        public Tuple<AccountUserModel, List<Tuple<Account, UserAccountRights>>> ForUser { get; set; }

        /// <summary>
        /// Кортеж 'счёт - список кортежей "пользователь - права пользователя на этот счёт"'
        /// заполняется данными в случае, если модель должна быть сформирована в виде "показать всех подписчиков счёта"
        /// </summary>
        public Tuple<Account, List<Tuple<AccountUserModel, UserAccountRights>>> ForAccount { get; set; }

        /// <summary>
        /// Режим предстваления - просматриваются счета пользователя или пользователи, подписанные на счёт
        /// </summary>
        public AccountOwnerPartialViewType AccountOwnerPartialViewType  { get; private set; }


        #region Свойства, используемые для привязки в UI
        /// <summary>
        /// Свойство для DropDownList в шапке таблици
        /// </summary>
        public Account CurrentAccount { get; set; }

        /// <summary>
        /// Свойство для DropDownList в шапке таблици
        /// </summary>
        public AccountUserModel CurrentOwner { get; set; }
        #endregion


        // Accounts и Owners это вспомогательные свойства, которые хранят "в чистом виде" списки соответственно счетов и пользователей
        // число их элементов одинаково. По большому счёту, эти списки - это столбци таблици "PLATFORM_USER_ACCOUNT"  Account и PlatformUser соответственно
        public List<Account> Accounts { get; private set; }
        public List<AccountUserModel> Owners { get; private set; }

        /// <summary>
        /// Список соответствий "уникальный идентификатор (либо пользователя либо чёта в зависимости от типа модели) - права на этот счёт этого пользователя (просмотр, торговля)"
        /// </summary>
        private List<Tuple<int, UserAccountRights>> AccountUserRights { get; set; }
        

        public AccountOwnerModel(List<Account> accounts, List<AccountUserModel> owners, List<Tuple<int, UserAccountRights>> accountUserRights, AccountOwnerPartialViewType accountOwnerPartialViewType)
        {
            Accounts = accounts;
            Owners = owners;
            AccountUserRights = accountUserRights;
            AccountOwnerPartialViewType = accountOwnerPartialViewType;

            ConstructModel();
        }

        private void ConstructModel()
        {
            switch (AccountOwnerPartialViewType)
            {
                case AccountOwnerPartialViewType.Undefined:                    
                    break;
                case AccountOwnerPartialViewType.ForAccount:
                    try
                    {
                        // Если модель должна быть сформирована в виде "показать всех подписчиков счёта", тогда 
                        // в списке Accounts все элементы будут одинаковые - один и тот же новер счёта. 
                        var acc = CurrentAccount = Accounts.GroupBy(x => x.ID).Single().FirstOrDefault(); // "превращаем" все эти одинаковые элементы в один элемент
                        if (acc != null)
                        {
                            // Если всё прошло нормально - формируем список "пользователь счёта acc - права пользователя на этот счёт"
                            var list = (from right in AccountUserRights 
                                        let listItemOwner = Owners.FirstOrDefault(x => x.UserId == right.Item1) 
                                        select new Tuple<AccountUserModel, UserAccountRights>(listItemOwner, right.Item2)).ToList();
                            // Заполняем кортеж "ForAccount", которая будет использоваться в представлении "AccountOwnerPartialForAccount"
                            ForAccount = new Tuple<Account, List<Tuple<AccountUserModel, UserAccountRights>>>(acc, list);
                        }
                    }
                    #region
                    catch (InvalidOperationException ex)
                    {
                        Logger.Error(
                            "ConstructModel() - при формировании модели для представления AccountOwner, при просмотре управляющих счёта, " +
                            "было переданно больше одного счёта или не одного", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ConstructModel() - Не найдено счёта", ex);
                    }
                    #endregion                   
                    break;
                case AccountOwnerPartialViewType.ForOwner:
                    try
                    {
                        // всё аналогично вредыдущему случаю "case"
                        var own = CurrentOwner = Owners.GroupBy(x => x.Title).Single().FirstOrDefault();
                        if (own != null)
                        {
                            var list = (from right in AccountUserRights
                                        let listItemAccount = Accounts.FirstOrDefault(x => x.ID == right.Item1)
                                        select new Tuple<Account, UserAccountRights>(listItemAccount, right.Item2)).ToList();

                            ForUser = new Tuple<AccountUserModel, List<Tuple<Account, UserAccountRights>>>(own, list);
                        }
                    }
                    #region
                    catch (InvalidOperationException ex)
                    {
                        Logger.Error("ConstructModel() - при формировании модели для представления AccountOwner, при просмотре счетов управляющего, " +
                                     "было переданно больше одного управляющего или не одного", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ConstructModel() - Не найдено пользователя", ex);  
                    }

                    #endregion
                    break;
            }
        }
    }

    /// <summary>
    /// перечисление возможных типов модели AccountOwnerModel
    /// </summary>
    public enum AccountOwnerPartialViewType
    {
        /// <summary>
        /// Не определено
        /// </summary>
        Undefined, 
        /// <summary>
        /// тип модели - отобразить всех подписчиков конкретного счёта
        /// </summary>
        ForAccount,
        /// <summary>
        /// тип модели - отобразить все счета конкретного пользователя
        /// </summary>
        ForOwner
    }
}