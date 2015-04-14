using System.Web.Mvc;
using TradeSharp.SiteAdmin.Contract;

namespace TradeSharp.SiteAdmin.Controllers
{
    [Authorize]
    public partial class ManagementController : Controller
    {
        /// <summary>
        /// Содержит текст сообщения, который появляется при прорисовке страници. Это нужно, например, что бы после добавления нового аккаунта 
        /// во всплывающем окне появилось сообщение о результате добавления (удачно / неудачно).
        /// </summary>
        public static string ResultMessage { get; set; }

        private readonly IAccountRepository accountRepository;
        private readonly ISpotRepository spotRepository;
        private readonly IPositionRepository positionRepository;
        private readonly IAccountGroupsRepository accountGroupsRepository;
        private readonly IDealerRepository dealerRepository;
        private readonly IWalletRepository walletRepository;
        private readonly IPaymentTransferRepository paymentTransferRepository;
        private readonly IUserRepository userRepository;
        private readonly ITradeSignalRepository tradeSignalRepository;

        public ManagementController(IAccountRepository accountRepository, ISpotRepository spotRepository,
            IPositionRepository positionRepository, IAccountGroupsRepository accountGroupsRepository, IDealerRepository dealerRepository,
            IWalletRepository walletRepository, IPaymentTransferRepository paymentTransferRepository, IUserRepository userRepository,
            ITradeSignalRepository tradeSignalRepository)
        {
            this.accountRepository = accountRepository;
            this.accountGroupsRepository = accountGroupsRepository;
            this.spotRepository = spotRepository;
            this.positionRepository = positionRepository;
            this.dealerRepository = dealerRepository;
            this.walletRepository = walletRepository;

            this.paymentTransferRepository = paymentTransferRepository;
            this.userRepository = userRepository;
            this.tradeSignalRepository = tradeSignalRepository;
        }
    }
}