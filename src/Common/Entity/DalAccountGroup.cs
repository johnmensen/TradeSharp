using System;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace Entity
{
    public class DalAccountGroup
    {
        private readonly ITradeSharpDictionary proxyDic;

        private AccountGroup[] groups;
        /// <summary>
        /// группы счетов вместе с настройками дилера
        /// </summary>
        public AccountGroup[] Groups
        {
            get
            {
                if (groups != null) return groups;
                try
                {
                    groups = proxyDic.GetAccountGroupsWithSessionInfo().ToArray();
                    return groups;
                }
                catch
                {
                    return new AccountGroup[0];
                }
            }
            private set { groups = value; }
        }

        private static readonly Lazy<DalAccountGroup> instance = new Lazy<DalAccountGroup>(() => new DalAccountGroup(TradeSharpDictionary.Instance.proxy));
        public static DalAccountGroup Instance
        {
            get { return instance.Value; }
        }

        private DalAccountGroup(ITradeSharpDictionary dic)
        {
            proxyDic = dic;
            try
            {
                Groups = dic.GetAccountGroupsWithSessionInfo().ToArray();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка инициализации словаря групп счетов", ex);
            }
            
        }
    }
}
