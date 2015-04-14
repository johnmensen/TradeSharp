using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class UserSession
    {
        public string login;
        public string ip;
        public long terminalId;
        public DateTime loginTime;

        /// <summary>
        /// ID счета - разрешение на торговые операции
        /// </summary>
        public List<Cortege2<int, bool>> enabledAccounts = new List<Cortege2<int, bool>>();
        /// <summary>
        /// торговый счет, к которому подключен терминал
        /// </summary>
        public int accountId;
        /// <summary>
        /// метка пользовательской сессии
        /// ее передает клиент в своих запросах
        /// </summary>
        public int sessionTag;
        /// <summary>
        /// последнее время клиентского запроса, обнуляется в
        /// момент аутентификации. каждое переданное значение должно быть
        /// больше предыдущего, от него строится хеш-отпечаток с логином и паролем
        /// проверяется совпадение
        /// </summary>
        public long lastRequestClientTime;
        /// <summary>
        /// уведомления клиенту раздаются на указанный Id
        /// </summary>
        public ITradeSharpServerCallback callback;
        /// <summary>
        /// версия терминала (номер ревизии)
        /// </summary>
        public string terminalVersion;
        /// <summary>
        /// Id пользователя - заполняется сервером при аутентификации
        /// </summary>
        public int userId;

        public UserSession() {}

        public UserSession(UserSession speciman)
        {
            login = speciman.login;
            ip = speciman.ip;
            terminalId = speciman.terminalId;
            loginTime = speciman.loginTime;
            sessionTag = speciman.sessionTag;
            lastRequestClientTime = speciman.lastRequestClientTime;
            callback = speciman.callback;
            accountId = speciman.accountId;
            enabledAccounts = speciman.enabledAccounts.ToList();
            terminalVersion = speciman.terminalVersion;
            userId = speciman.userId;
        }
    }
}
