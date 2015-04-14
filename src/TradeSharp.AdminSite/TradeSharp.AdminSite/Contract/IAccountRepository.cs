using System.Collections.Generic;
using TradeSharp.Admin.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IAccountRepository
    {
        /// <summary>
        /// Получить все счета с сервера, отфильтровать их и отсортировать в соответствии с фильтрами модели представления, переданной в параметре
        /// </summary>
        /// <param name="model">модель, содержит значения фильтров и указания к сортировке</param>
        /// <returns>Страница тиблици со  счетами, отсортированная и отфильтрованная</returns>
        List<AccountTag> GetAllAccounts(AccountViewModel model);

        List<Account> GetAllAccounts();

        /// <summary>
        /// Получение счёта с сервера по его уникальному идентификатору
        /// </summary>
        /// <param name="id">уникальный идетификатор</param>
        /// <returns>счёт</returns>
        AccountItem GetAccount(int id);

        /// <summary>
        /// Получить счёта пользователя
        /// </summary>
        List<Account> GetAccountForUser(int userId);

        /// <summary>
        /// Сохранение внесённых пользователем изменений в счёт
        /// </summary>
        /// <param name="account">Ссылка на редактируемый счёт</param>
        /// <returns>Результат выполнения опрерации</returns>
        /// <remarks>Тестируется</remarks>
        bool SaveAccountChanges(Account account);

        int? GetAccountId(string login);

        /// <summary>
        /// Сформировать модель для представления AccountOwner
        /// </summary>
        /// <param name="accountId">Уникольный идентификатор владельца счёта. Если он не null, значит запрос на все счета этого пользователя</param>
        /// <param name="ownerId">Уникальный идентифкатор счёта. Если он не null, значит запрос на всех управляющих этого счета</param>
        /// <remarks>Тестируется</remarks>
        AccountOwnerModel GetAccountOwnerModel(int? accountId = null, int? ownerId = null);

        /// <summary>
        /// Добавление нового счёта. 
        /// </summary>
        /// <param name="newAccount">ссылка на объект, содержащий дабавляемый счёт + учётные данные нового пользователя</param>
        /// <returns>Результат выполнения опрерации</returns>
        bool AddAccount(AddAccountModel newAccount);

        /// <summary>
        /// Редактирование таблици "PLATFORM_USER_ACCOUNT" из представления "AccountOwner"
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="ownerId"></param>
        /// <param name="right">Указывает права пользователя на счёт. Если запись в таблицу добавляется, то этот параметр 0 или 1 (чтение или торговля).
        /// Если запись удаяется - то этот параметр равен -1</param>
        /// <param name="action">Указываает действие, которе следует произмести - добавить или удалить. Этот параметр следовало сделать перечеслением</param>
        /// <remarks>Тестируется</remarks>
        void EditAccountOwnerModel(int accountId, int ownerId, int right, string action);

        /// <summary>
        /// В методе генерируется запрос к БД с учётом фильтров полученных из модели
        /// </summary>
        /// <param name="model">модель, содержит значения фильтров и указания к сортировке</param>
        /// <returns></returns>
        /// <remarks>Не тестируется</remarks>
        List<AccountTag> GetFilterAccountFromServer(AccountViewModel model);

        /// <summary>
        /// Деактивация счёта
        /// </summary>
        /// <param name="accountId">Уникальный идентификатор</param>
        /// <returns>Результат выполнения операции</returns>
        bool Deactivate(int accountId);

        /// <summary>
        /// Получить число служб (сигналов), автором которых является текущий счёт
        /// </summary>
        int? GetSignalCount(int id);
    }
}