using System.Windows.Forms;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class CompleteSubscribeOnPortfolioDlg : Form
    {
        public CompleteSubscribeOnPortfolioDlg()
        {
            InitializeComponent();
        }

        public CompleteSubscribeOnPortfolioDlg(TopPortfolio portfolio) : this()
        {
            completeSubscribeOnPortfolioControl1.SetupPortfolio(portfolio);
        }
    }
}
