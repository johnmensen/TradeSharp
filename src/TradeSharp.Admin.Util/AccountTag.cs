using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using System.Linq;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Admin.Util
{
    public class AccountTag
    {
        public Account account;

        public int Id { get { return account.ID; } }

        public string Group { get { return account.Group; } }

        public bool IsReal { get; set; }

        /// <summary>
        /// Строка, в которой через запятую перечислены только имена
        /// </summary>
        public string OwnersName { get; set; }

        /// <summary>
        /// Словарь, содержащий соответствие уникальный идентификатор пользователя - имя пользователя
        /// </summary>
        public Dictionary<int, string> OwnersNameId { get; set; }

        public string BalanceString
        {
            get { return account.Balance.ToStringUniformMoneyFormat(false) + " " + account.Currency; }
        }

        public bool Selected { get; set; }

        /// <summary>
        /// Конструктор, в который передаётся GetAllAccounts_Result, не содержащий сведений о соответствии "уникальный идентификатор пользователя - имя пользователя" и
        /// не заполняющий словарь "OwnersNameId"
        /// </summary>
        public AccountTag(GetAllAccounts_Result ac)
        {
            if (ac == null)
            {
                account = new Account();
            }
            else
            {
                account = new Account
                {
                    Balance = ac.Balance,
                    Currency = ac.Currency,
                    Group = ac.AccountGroup,
                    ID = ac.ID,
                };
                OwnersName = ac.UserNames ?? string.Empty;
                OwnersNameId = new Dictionary<int, string>();
            }
        }

        /// <summary>
        /// Конструктор, в который передаётся GetAllAccountsUserDetail_Result.
        /// словарь "OwnersNameId" заполняется соответствиями "уникальный идентификатор пользователя - имя пользователя"
        /// </summary>
        public AccountTag(GetAllAccountsUserDetail_Result ac)
        {
            if (ac == null)
            {
                account = new Account();
            }
            else
            {
                account = new Account
                {
                    Balance = ac.Balance,
                    Currency = ac.Currency,
                    Group = ac.AccountGroup,
                    ID = ac.ID,
                };


                OwnersNameId = new Dictionary<int, string>();
                OwnersName = string.Empty;

                if (ac.UserNames != null)
                {
                    OwnersName = ac.UserNames;

                    int[] idArray = null;
                    try
                    {
                        idArray = ac.UserId.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                    }
                    catch (FormatException ex)
                    {
                        Logger.Info("Ошибка в конструкторе AccountTag. Хранимая процедура GetAllAccountsUserDetail вернула из таблици PLATFORM_USER значение Id не являющееся int", ex);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Logger.Info("Ошибка в конструкторе AccountTag. Строка UserId не корректна или равна null", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("Ошибка в конструкторе AccountTag. Не удалось разбить строку UserId в массив целых чисел", ex);
                    }

                
                    var nameArray = ac.UserNames.Split(',');
                    if (idArray != null && idArray.Count() == nameArray.Count())
                    {
                        //TODO Тут возможна неоднозначноасть. Тут возможный источник ошибки
                        for (var i = 0; i < nameArray.Count(); i++)
                        {
                            OwnersNameId.Add(idArray[i], nameArray[i]);
                        }
                    }
                }
            }
        }

        public AccountTag(Account ac)
        {
            account = ac;
        }


    }
}
