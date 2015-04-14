using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Server.Contract;
using TradeSharp.Server.Repository;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// опрашивает UserSessionStorage на предмет отвала пользователя
    /// сохраняет в БД информацию о сеансах
    /// </summary>
    class UserEventManager
    {
        private static UserEventManager instance;
        public static UserEventManager Instance
        {
            get { return instance ?? (instance = new UserEventManager()); }
        }
        private UserEventManager()
        {
            accountRepository = AccountRepository.Instance;
        }

        private IAccountRepository accountRepository;

        private volatile bool isStopping;
        private Thread workThread;
        private readonly int threadInterval = AppConfig.GetIntParam("UserEventManager.IntervalMils", 1600);
        private readonly FloodSafeLogger logNoFlood = new FloodSafeLogger(1000 * 10);
        private const int LogMsgErrorGetAccount = 1;

        public void Start()
        {
            workThread = new Thread(ThreadLoop);
            workThread.Start();
        }
        
        public void Stop()
        {
            isStopping = true;
            workThread.Join();
            // напоследок - сохранить в БД записи журнала сессий
            SaveLogMessages();
        }

        private void ThreadLoop()
        {
            const int sleepInterval = 200;
            var iterations = threadInterval/sleepInterval;
            var counter = iterations;

            while (!isStopping)
            {
                Thread.Sleep(sleepInterval);
                counter--;
                if (counter > 0) continue;
                counter = iterations;
                // выполнить основные процедуры
                CheckStaleClients();
                SaveLogMessages();
            }
        }

        /// <summary>
        /// проверить отвалившихся клиентов (польз. сессии)
        /// 
        /// если клиент подписался на счет - отправить ему информацию по счету,
        /// либо просто - пинг
        /// </summary>
        private void CheckStaleClients()
        {
            var sessions = UserSessionStorage.Instance.GetSessions();            
            var staleList = new List<long>();
            foreach (var session in sessions)
            {
                try
                {
                    Account account = null;
                    if (session.accountId > 0)
                    {
                        try
                        {
                            account = accountRepository.GetAccount(session.accountId);
                        }
                        catch (Exception ex)
                        {
                            logNoFlood.LogMessageFormatCheckFlood(LogEntryType.Error,
                                LogMsgErrorGetAccount, 1000 * 60 * 5, "CheckStaleClients() - ошибка получения счета {0}: {1}",
                                session.accountId, ex);
                        }
                    }
                    if (account != null)
                        session.callback.AccountDataUpdated(account);                        
                    else
                        session.callback.Ping();
                }
                catch (Exception ex)
                {
                    staleList.Add(session.terminalId);
                }                
            }
            // удалить подвисшие
            if (staleList.Count == 0) return;
            staleList = staleList.Distinct().ToList();
            try
            {
                UserSessionStorage.Instance.ExcludeStaleSessions(staleList);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("CheckStaleClients - exception while deleting {0} sessions: {1}",
                    staleList.Count, ex);
            }
        }

        /// <summary>
        /// сохранить записи о сеансах в лог
        /// </summary>
        private void SaveLogMessages()
        {
            
        }
    }
}
