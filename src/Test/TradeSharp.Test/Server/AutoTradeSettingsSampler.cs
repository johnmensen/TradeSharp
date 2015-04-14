using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;

namespace TradeSharp.Test.Server
{
    static class AutoTradeSettingsSampler
    {
        public static string TradeSignalSetsAreCorrect(AutoTradeSettings sets, SUBSCRIPTION_SIGNAL setsStored)
        {
            var errors = new List<string>();
            if ((int?)sets.VolumeRound != setsStored.VolumeRound) errors.Add("VolumeRound");
            if (sets.HedgingOrdersEnabled != setsStored.HedgingOrdersEnabled) errors.Add("HedgingOrdersEnabled");
            if (sets.PercentLeverage != setsStored.PercentLeverage) errors.Add("PercentLeverage");
            if (sets.MinVolume != setsStored.MinVolume) errors.Add("MinVolume");
            if (sets.StepVolume != setsStored.StepVolume) errors.Add("StepVolume");
            if (sets.MaxLeverage != setsStored.MaxLeverage) errors.Add("MaxLeverage");
            if (sets.MaxVolume != setsStored.MaxVolume) errors.Add("MaxVolume");
            if (sets.TargetAccount != setsStored.TargetAccount) errors.Add("TargetAccount");
            if (sets.TradeAuto != setsStored.AutoTrade) errors.Add("TradeAuto");
            if (sets.FixedVolume != setsStored.FixedVolume) errors.Add("FixedVolume");
            return string.Join(", ", errors);
        }

        public static AutoTradeSettings MakeSampleTradeSettings()
        {
            var tradeSets = new AutoTradeSettings
            {
                VolumeRound = VolumeRoundType.Вверх,
                HedgingOrdersEnabled = true,
                PercentLeverage = 150,
                MinVolume = 50000,
                StepVolume = 20000,
                MaxLeverage = 11,
                MaxVolume = 500000,
                TradeAuto = true,
                FixedVolume = 10000
            };
            return tradeSets;
        }
    }
}
