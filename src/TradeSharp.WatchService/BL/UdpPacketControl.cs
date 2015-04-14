using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;


namespace TradeSharp.WatchService.BL
{
    /// <summary>
    /// слушает порт UDP
    /// получая сообщения вида:
    /// code=MT4;status=OK; или code=MT4;status=HasErrors;description=got no quote for 5 minutes
    /// </summary>
    class UdpPacketControl
    {
        #region Singletone

        private static readonly Lazy<UdpPacketControl> instance =
            new Lazy<UdpPacketControl>(() => new UdpPacketControl());

        public static UdpPacketControl Instance
        {
            get { return instance.Value; }
        }

        #endregion

        /// <summary>
        /// обращения только через thread-safe методы UpdateUnitState
        /// </summary>
        private readonly List<ServiceStateUnit> units = new List<ServiceStateUnit>();
        private readonly ReaderWriterLock unitsLocker = new ReaderWriterLock();
        private const int LockTimeout = 2000;
        private readonly int portToListen;
        /// <summary>
        /// поток прослушки UDP-порта
        /// </summary>
        private Thread threadListen;
        /// <summary>
        /// поток опроса состояния юнитов
        /// </summary>
        private Thread threadPoll;

        private bool serviceIsStopping;

        private UdpPacketControl()
        {
            try
            {
                units.AddRange(ConfigParser.ReadUnits().FindAll(u => u.UdpBinding));
                portToListen = int.Parse(AppConfig.GetStringParam("UDP.Port", "12000"));
            }
            catch (Exception ex)
            {
                Logger.Error("Error in UdpPacketControl ctor", ex);
                throw;
            }
        }

        public void Start()
        {
            threadListen = new Thread(ListenLoop);
            threadListen.Start();
            threadPoll = new Thread(PollUnits);
            threadPoll.Start();
        }

        public void Stop()
        {
            serviceIsStopping = true;
            threadPoll.Join();
            if (threadListen == null) return;
            using (var client = new UdpClient())
            {
                var data = Encoding.ASCII.GetBytes("<quit>");
                client.Send(data, data.Length, "127.0.0.1", portToListen);
            }
            threadListen.Join(3000);
            threadListen = null;
        }

        private void ListenLoop()
        {
            using (var client = new UdpClient(portToListen))
            {
                while (true)
                {
                    try
                    {
                        IPEndPoint ep = null;
                        var data = client.Receive(ref ep);
                        if (data.Length == 0) continue;
                        var dataStr = Encoding.ASCII.GetString(data);
                        if (dataStr == "<quit>") break;
                        // ожидаем сообщение вида code=MT4;status=OK; или code=MT4;status=Error;
                        ProcessUDPMessage(dataStr);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error in ListenLoop(UDP)", ex);
                        throw;
                    }
                }
            }
        }

        private void PollUnits()
        {
            while (!serviceIsStopping)
            {
                var nowTime = DateTime.Now;
                // опросить
                unitsLocker.AcquireWriterLock(LockTimeout);
                try
                {
                    foreach (var unit in units)
                    {
                        // проверить, когда обновлялся крайний раз
                        if ((nowTime - unit.LastUpdated).TotalSeconds > unit.UpdateTimeoutSeconds)
                            unit.LastServiceState.AddError(ServiceProcessState.Offline, "offline", DateTime.Now);
                        if (unit.ShouldReport())
                        {// отправить сообщение на сервер
                            Reporter.SendReports(unit);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in UDP - PollUnits()", ex);
                    throw;
                }
                finally
                {
                    unitsLocker.ReleaseWriterLock();
                }

                // уснуть
                Thread.Sleep(1000);
            }
        }

        private void ProcessUDPMessage(string msg)
        {
            var cmdDic = new Dictionary<string, string>();
            var parts = msg.Split(';');
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                var subParts = part.Split('=');
                if (subParts.Length != 2) continue;
                cmdDic.Add(subParts[0], subParts[1]);
            }

            if (!cmdDic.ContainsKey("code") || !cmdDic.ContainsKey("status")) return;
            var code = cmdDic["code"];
            var status = cmdDic["status"];
            // поиск юнита и установка его состояния
            var description = cmdDic.ContainsKey("description") ? cmdDic["description"] : "";
            UpdateUnitState(code, status, description);
        }

        private void UpdateUnitState(string code, string status, string description)
        {
            unitsLocker.AcquireReaderLock(LockTimeout);
            try
            {
                // найти модуль
                var unit = units.Find(u => u.Code == code);
                if (unit == null) return;
                var state = (ServiceProcessState)Enum.Parse(typeof(ServiceProcessState),
                    status, true);

                // получить доступ на запись
                unitsLocker.UpgradeToWriterLock(LockTimeout);
                unit.LastServiceState.AddError(state, description, DateTime.Now);
                unit.LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                var stack = new StackTrace(ex, true);
                Logger.Error(string.Format("Error in UpdateUnitState(UDP), line {0}, col {1}",
                    stack.GetFrame(0).GetFileLineNumber(), stack.GetFrame(0).GetFileColumnNumber()), ex);
                throw;
            }
            finally
            {
                unitsLocker.ReleaseLock();
            }
        }
    }
}
