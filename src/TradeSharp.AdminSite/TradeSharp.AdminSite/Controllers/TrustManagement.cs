using System.Web.Mvc;
using TradeSharp.SiteAdmin.Contract;

namespace TradeSharp.SiteAdmin.Controllers
{
    /// <summary>
    /// Содержит действия для управления портфелями роботов
    /// </summary>
    [Authorize]
    public partial class TrustManagementController : Controller
    {
        /// <summary>
        /// Содержит текст сообщения, который появляется при прорисовке страници. Это нужно, например, что бы после добавления нового аккаунта 
        /// во всплывающем окне появилось сообщение о результате добавления (удачно / неудачно).
        /// </summary>
        public static string ResultMessage { get; set; }

        private readonly IAccountRepository accountRepository;
        private readonly ITopPortfolioRepository topPortfolioRepository;
        private readonly IUserRepository userRepository;
        private readonly IPammRepository pammRepository;
        private readonly ITradeSignalRepository tradeSignalRepository;

        public TrustManagementController(IAccountRepository accountRepository, ITopPortfolioRepository topPortfolioRepository, IUserRepository userRepository,
            IPammRepository pammRepository, ITradeSignalRepository tradeSignalRepository)
        {
            this.accountRepository = accountRepository;
            this.topPortfolioRepository = topPortfolioRepository;
            this.userRepository = userRepository;
            this.pammRepository = pammRepository;
            this.tradeSignalRepository = tradeSignalRepository;
        }
    }
}