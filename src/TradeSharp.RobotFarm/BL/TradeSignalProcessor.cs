using System;
using System.Collections.Generic;
using TradeSharp.Contract.Contract;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.RobotFarm.BL
{
    class TradeSignalProcessor
    {
        /// <summary>
        /// magic ордера, созданного по сигналу, будет равен ID "родительского" ордера
        /// плюс указанное смещение
        /// </summary>
        public const int SignalMagicStart = 0;

        private readonly Func<Account> getAccountInfo;
        private readonly Func<VolumeRoundType> getVolumeRound;
        private readonly Action<string> logMessage;
        private readonly Func<List<MarketOrder>> getMarketOrders;
        private CurrentProtectedContext protectedContext;
        private readonly FarmAccount farmAccount;

        private readonly ITradeSharpServerTrade proxy;

        public TradeSignalProcessor(Func<Account> getAccountInfo, Action<string> logMessage,
            Func<List<MarketOrder>> getMarketOrders, Func<VolumeRoundType> getVolumeRound,
            ITradeSharpServerTrade proxy, CurrentProtectedContext protectedContext,
            FarmAccount farmAccount)
        {
            this.getAccountInfo = getAccountInfo;
            this.proxy = proxy;
            this.logMessage = logMessage;
            this.getMarketOrders = getMarketOrders;
            this.getVolumeRound = getVolumeRound;
            this.protectedContext = protectedContext;
            this.farmAccount = farmAccount;
        }

        public void ProcessTradeSignalAction(TradeSignalAction action)
        {
        }
    }
}
