using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Chat.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Chat.Client.BL
{
    public delegate List<PlatformUser> GetAllPlatformUsersDel();

    public class AllUsers
    {
        public static GetAllPlatformUsersDel GetAllPlatformUsers;

        public static AllUsers Instance
        {
            get { return instance ?? (instance = new AllUsers()); }
        }

        private static AllUsers instance;
        private readonly ThreadSafeList<User> users = new ThreadSafeList<User>();
        private const int LockTimeout = 1000;
        private readonly ThreadSafeTimeStamp lastRequestTime = new ThreadSafeTimeStamp();

        private AllUsers()
        {
            bool timeoutFlag;
            users.ExtractAll(LockTimeout, out timeoutFlag);
            if (GetAllPlatformUsers == null)
                return;
            try
            {
                var allUsers = GetAllPlatformUsers();
                if (allUsers == null)
                    return;
                users.AddRange(allUsers.Select(u => new User(u)), LockTimeout);
                lastRequestTime.Touch();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Ошибка получения списка пользователей\n" + ex);
                Logger.Error("Ошибка получения списка пользователей", ex);
            }
        }

        public List<User> GetAllUsers()
        {
            bool timeoutFlag;
            return users.GetAll(LockTimeout, out timeoutFlag);
        }

        public User GetUser(int id)
        {
            var result = GetAllUsers().FirstOrDefault(u => u.ID == id);
            if (result == null && DateTime.Now.Subtract(lastRequestTime.GetLastHit()).TotalMinutes > 1)
            {
                if (GetAllPlatformUsers == null)
                    return null;
                users.AddRange(GetAllPlatformUsers().Select(u => new User(u)), LockTimeout);
                result = GetAllUsers().FirstOrDefault(u => u.ID == id);
            }
            return result;
        }
    }
}
