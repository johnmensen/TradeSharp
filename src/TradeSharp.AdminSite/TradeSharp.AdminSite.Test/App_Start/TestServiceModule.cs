using Ninject.Modules;
using TradeSharp.SiteAdmin.Contract;
using TradeSharp.SiteAdmin.Repository;

namespace TradeSharp.AdminSite.Test.App_Start
{
    public class TestServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAccountRepository>().To<AccountRepository>().InSingletonScope();
            Bind<IAccountGroupsRepository>().To<AccountGroupsRepository>().InSingletonScope();
            Bind<ISpotRepository>().To<SpotRepository>().InSingletonScope();
            Bind<IPositionRepository>().To<PositionRepository>().InSingletonScope();
            Bind<IDealerRepository>().To<DealerRepository>().InSingletonScope();
            Bind<IWalletRepository>().To<WalletRepository>().InSingletonScope();
            Bind<IPaymentTransferRepository>().To<PaymentTransferRepository>().InSingletonScope();
            Bind<IUserRepository>().To<UserRepository>().InSingletonScope();
            Bind<ITradeSignalRepository>().To<TradeSignalRepository>().InSingletonScope();

            Bind<ITopPortfolioRepository>().To<TopPortfolioRepository>().InSingletonScope();
            Bind<IPammRepository>().To<PammRepository>().InSingletonScope();
        }
    }
}
