using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Server.BL
{
    /// <summary>
    /// потокобезопасное хранилище событий по пользователям
    /// синхронизируется с БД
    /// </summary>
    class UserEventStorage : Scheduler
    {
        private static UserEventStorage instance;

        public static UserEventStorage Instance
        {
            get { return instance ?? (instance = new UserEventStorage()); }
        }

        private readonly int databaseSynchInterval = AppConfig.GetIntParam("UserEvent.DbSynch.IntervalMils", 1000 * 30);

        private readonly bool storeEventsInDb = AppConfig.GetBooleanParam("UserEvent.UseDb", false);

        private readonly ThreadSafeStorage<int, List<UserEvent>> userEvent = new ThreadSafeStorage<int, List<UserEvent>>();

        private readonly ConcurrentDictionary<string, int?> userLogin = new ConcurrentDictionary<string, int?>();

        private UserEventStorage()
        {
            if (!storeEventsInDb) return;

            LoadEventsFromDb();
            if (storeEventsInDb)
                schedules = new []
                    {
                        new Schedule(StoreEventsInDb, databaseSynchInterval, false)
                    };
        }

        public void PushEvents(List<UserEvent> events)
        {
            try
            {
                if (events.Count == 0) return;
                var eventsSorted = events.GroupBy(x => x.User).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
                foreach (var pair in eventsSorted)
                {
                    var oldEvents = userEvent.ReceiveValue(pair.Key);
                    oldEvents = oldEvents != null ? oldEvents.ToList() : new List<UserEvent>();
                    oldEvents.AddRange(pair.Value);
                    userEvent.UpdateValues(pair.Key, oldEvents);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in UserEventStorage.PushEvents()", ex);
            }
        }

        public List<UserEvent> GetUserEvents(string login)
        {
            try
            {
                var id = GetUserId(login);
                if (!id.HasValue)
                    return new List<UserEvent>();

                var usrEvents = userEvent.ExtractData(id.Value);
                return usrEvents ?? new List<UserEvent>();
            }
            catch (Exception ex)
            {
                Logger.Error("UserEventStorage.GetUserEvents()", ex);
                return new List<UserEvent>();
            }
        }
    
        private int? GetUserId(string login)
        {
            int? id;
            if (userLogin.TryGetValue(login, out id))
                return id;

            // запросить id из БД
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    var user = ctx.PLATFORM_USER.FirstOrDefault(u => u.Login == login);
                    if (user != null)
                    {
                        userLogin.TryAdd(login, user.ID);
                        return user.ID;
                    }
                    userLogin.TryAdd(login, null);
                    Logger.Error("UserEventStorage.GetUserId(" + login + ") - не найден");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("UserEventStorage.GetUserId()", ex);
                return null;
            }
        }
    
        private void StoreEventsInDb(object logMsg)
        {
            try
            {
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    // удалить старые сообщения
                    ctx.ClearOldUserEvents(new DateTime(1900, 1, 1));
                    // сохранить новые
                    var countTotal = 0;
                    var events = userEvent.ReceiveAllData();
                    foreach (var evtList in events)
                    {
                        var evtToSave = evtList.Value.Take(100).Reverse().ToList();
                        countTotal += evtToSave.Count;
                        foreach (var evt in evtToSave)
                        {
                            var usEvt = LinqToEntity.UndecorateUserEvent(evt);
                            ctx.USER_EVENT.Add(usEvt);
                        }
                    }

                    ctx.SaveChanges();
                    if ((bool)logMsg)
                        Logger.InfoFormat("Сохранено {0} записей USER_EVENT", countTotal);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в UserEventStorage.StoreEventsInDb()", ex);
            }
        }

        private void LoadEventsFromDb()
        {
            try
            {
                var plainList = new List<UserEvent>();
                using (var ctx = DatabaseContext.Instance.Make())
                {
                    foreach (var evt in ctx.USER_EVENT)
                        plainList.Add(LinqToEntity.DecorateUserEvent(evt));

                    // удалить старые сообщения
                    ctx.ClearOldUserEvents(new DateTime(1900, 1, 1));
                }

                // заполнить список
                var dic = plainList.GroupBy(x => x.User).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
                userEvent.Clear();
                foreach (var evt in dic)
                    userEvent.UpdateValues(evt.Key, evt.Value);
                Logger.InfoFormat("Прочитано {0} записей USER_EVENT", plainList.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в UserEventStorage.LoadEventsFromDb()", ex);
            }
        }

        public override void Start()
        {
            if (!storeEventsInDb) return;
            base.Start();
        }

        public override void Stop()
        {
            if (!storeEventsInDb) return;
            base.Stop();
            StoreEventsInDb(true);
        }
    }
}
