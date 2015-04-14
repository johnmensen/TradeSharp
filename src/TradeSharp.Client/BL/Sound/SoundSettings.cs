using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entity;

namespace TradeSharp.Client.BL.Sound
{
    public enum VocalizedEvent
    {
        Started = 0,
        LoggedIn = 1,
        CommonError = 2,
        TradeResponse = 3,
        TradeSignal = 4
    }

    public class VocalizedEventFileName
    {
        [PropertyXMLTag("Sound.Event")]
        public VocalizedEvent EventName { get; set; }

        [PropertyXMLTag("Sound.File")]
        public string FileName { get; set; }
    }

    static class SoundSettings
    {
        private static Dictionary<VocalizedEvent, byte[]> eventSound =
            new Dictionary<VocalizedEvent, byte[]>();

        private static readonly ReaderWriterLock locker = new ReaderWriterLock();
        private const int LockTimeout = 1000;

        public static Dictionary<VocalizedEvent, byte[]> GetEventSound()
        {
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return new Dictionary<VocalizedEvent, byte[]>();
            }
            try
            {
                return eventSound.ToDictionary(e => e.Key, e => e.Value);
            }
            finally
            {
                locker.ReleaseReaderLock();
            }
        }

        public static byte[] GetEventSound(VocalizedEvent evt)
        {
            try
            {
                locker.AcquireReaderLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return null;
            }
            try
            {
                byte[] s;
                eventSound.TryGetValue(evt, out s);
                return s;
            }
            finally
            {
                locker.ReleaseReaderLock();
            }
        }

        public static void UpdateEventSound(Dictionary<VocalizedEvent, byte[]> dic)
        {
            try
            {
                locker.AcquireWriterLock(LockTimeout);
            }
            catch (ApplicationException)
            {
                return;
            }
            try
            {
                eventSound = dic;
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }
    }
}
