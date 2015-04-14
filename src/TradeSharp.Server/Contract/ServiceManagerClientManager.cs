using System;
using System.Collections.Generic;
using System.Threading;
using TradeSharp.Contract.Contract;
using TradeSharp.Util;

namespace TradeSharp.Server.Contract
{
    //public static class ServiceManagerClientManager
    //{
    //    /// <summary>
    //    /// Session ID - client callback
    //    /// </summary>
    //    private static Dictionary<string, ITradeSharpServerCallback> clients = new Dictionary<string, ITradeSharpServerCallback>();
    //    private static readonly ReaderWriterLock lockClients = new ReaderWriterLock();
    //    private const int LockTimeout = 500;
    //    public static void AddClient(string sessionId, ITradeSharpServerCallback callback)
    //    {
    //        try
    //        {
    //            Logger.Info("Клиент подключился");
    //            ModuleStatusController.Instance.lastLoginTime.Value = DateTime.Now;
    //            lockClients.AcquireReaderLock(LockTimeout);
    //            if (!clients.ContainsKey(sessionId))
    //            {
    //                try
    //                {
    //                    lockClients.UpgradeToWriterLock(LockTimeout);
    //                    clients.Add(sessionId, callback);
    //                }
    //                finally
    //                {
    //                    lockClients.ReleaseLock();
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            lockClients.ReleaseLock();
    //        }
    //    }
    //    public static void RemoveClient(string sessionId)
    //    {
    //        try
    //        {
    //            lockClients.AcquireReaderLock(LockTimeout);
    //            if (!clients.ContainsKey(sessionId))
    //            {
    //                try
    //                {
    //                    lockClients.UpgradeToWriterLock(LockTimeout);
    //                    clients.Remove(sessionId);
    //                }
    //                finally
    //                {
    //                    lockClients.ReleaseLock();
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            lockClients.ReleaseLock();
    //        }
    //    }
    //    public static void RemoveClient(ITradeSharpServerCallback client)
    //    {
    //        try
    //        {
    //            lockClients.AcquireReaderLock(LockTimeout);
    //            var keys = new List<string>();
    //            foreach (var key in clients.Keys)
    //                if (clients[key] == client) keys.Add(key);
    //            if (keys.Count == 0) return;

    //            try
    //            {
    //                lockClients.UpgradeToWriterLock(LockTimeout);
    //                foreach (var key in keys)
    //                    if (clients.ContainsKey(key)) clients.Remove(key);
    //            }
    //            finally
    //            {
    //                lockClients.ReleaseLock();
    //            }
    //        }
    //        finally
    //        {
    //            lockClients.ReleaseLock();
    //        }
    //    }
    //    public static List<ITradeSharpServerCallback> GetClients()
    //    {
    //        var lstDest = new List<ITradeSharpServerCallback>();
    //        try
    //        {
    //            lockClients.AcquireReaderLock(LockTimeout);
    //            foreach (var client in clients.Values)
    //                lstDest.Add(client);
    //        }
    //        finally
    //        {
    //            lockClients.ReleaseLock();
    //        }
    //        return lstDest;
    //    }
    //}
}
