namespace TradeSharp.SiteAdmin.Contract
{
    public interface IWalletRepository
    {
        bool UpdateBalance(int walletId, decimal transferVolume, bool deposit);
        bool WalletTransfer(int walletId, decimal transferVolue, string userLogin, int accountId, bool deposit);
        bool ChangeCurrency(int walletId, string walletCurrency, bool recalculationBalance);
    }
}