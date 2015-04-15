using TradeSharp.Contract.Entity;

namespace TradeSharp.SiteBridge.Lib.Finance
{
    public interface IEfficiencyCalculator
    {
        bool Calculate(AccountEfficiency ef);
        void CalculateProfitCoeffs(AccountEfficiency ef);
    }
}
