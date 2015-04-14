using System;
using System.Net.NetworkInformation;
using TradeSharp.Util;

namespace TradeSharp.Client.Util
{
    public static class TerminalIdGenerator
    {
        public static long MakeTerminalId()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    return (ExecutablePath.ExecPath + nic.GetPhysicalAddress()).GetHashCode();
            }
            return new Random().Next();
        }
    }
}
